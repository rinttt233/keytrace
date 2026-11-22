using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Keytrace
{
    public class Recorder
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_QUIT = 0x0012;
        private const int WM_APP = 0x8000;
        private const int WM_REHOOK = WM_APP + 10;
        private const int WM_FLUSH = WM_APP + 11;
        private const int THREAD_PRIORITY_TIME_CRITICAL = 15;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetThreadPriority(IntPtr hThread, int nPriority);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        private class Press
        {
            public DateTime Wall;
            public double Down;
            public double Last;
        }

        private Store store;
        private readonly object storeGate = new object();
        private readonly object gate = new object();
        private readonly Dictionary<int, Press> pressed = new Dictionary<int, Press>();
        private readonly Dictionary<Key, Stat> pending = new Dictionary<Key, Stat>();
        private readonly ManualResetEvent flushEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent ready = new ManualResetEvent(false);
        private readonly HookProc proc;

        private Thread hookThread;
        private Thread flushThread;
        private IntPtr hook;
        private uint hookTid;
        private volatile bool stop;
        public volatile bool Ok;
        public volatile string Error;
        public long TotalPresses;
        public double TotalHoldMs;

        public Recorder(Store store)
        {
            this.store = store;
            proc = new HookProc(OnHook);
        }

        public void Start()
        {
            flushThread = new Thread(FlushLoop);
            flushThread.IsBackground = true;
            flushThread.Name = "keytrace-flush";
            flushThread.Start();

            hookThread = new Thread(HookLoop);
            hookThread.IsBackground = true;
            hookThread.Name = "keytrace-hook";
            hookThread.Start();

            ready.WaitOne(3000);
        }

        private void HookLoop()
        {
            try
            {
                hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0);
                if (hook == IntPtr.Zero)
                {
                    Error = "SetWindowsHookEx failed: " + Marshal.GetLastWin32Error();
                    ready.Set();
                    return;
                }
                hookTid = GetCurrentThreadId();
                SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
                Ok = true;
                ready.Set();

                MSG msg;
                while (true)
                {
                    int ret = GetMessage(out msg, IntPtr.Zero, 0, 0);
                    if (ret == 0) break;
                    if (ret == -1) break;
                    if (msg.message == WM_REHOOK) { Reinstall(); continue; }
                    if (msg.message == WM_FLUSH) { flushEvent.Set(); continue; }
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                ready.Set();
            }
            finally
            {
                if (hook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(hook);
                    hook = IntPtr.Zero;
                }
            }
        }

        private void Reinstall()
        {
            try
            {
                if (hook != IntPtr.Zero) UnhookWindowsHookEx(hook);
                hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0);
                Ok = hook != IntPtr.Zero;
                if (!Ok) Error = "rehook failed";
            }
            catch (Exception ex) { Error = ex.Message; }
        }

        private IntPtr OnHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vk = (int)kb.vkCode;
                double now = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
                int msg = (int)wParam;

                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    Press p;
                    if (pressed.TryGetValue(vk, out p))
                    {
                        if (now - p.Last > 2.0) pressed[vk] = NewPress(now);
                        else p.Last = now;
                    }
                    else pressed[vk] = NewPress(now);
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    Press p;
                    if (pressed.TryGetValue(vk, out p))
                    {
                        pressed.Remove(vk);
                        double hold = (now - p.Down) * 1000.0;
                        if (hold < 0) hold = 0;
                        if (hold > 60000) hold = 60000;
                        AddPress(p.Wall, vk, hold);
                    }
                }
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private Press NewPress(double now)
        {
            Press p = new Press();
            p.Wall = DateTime.Now;
            p.Down = now;
            p.Last = now;
            return p;
        }

        private void AddPress(DateTime wall, int vk, double holdMs)
        {
            long hour = EpochHour(wall);
            Key k = new Key(hour, vk);
            lock (gate)
            {
                Stat st;
                if (!pending.TryGetValue(k, out st)) { st = new Stat(); pending[k] = st; }
                st.Count++;
                st.HoldMs += holdMs;
                TotalPresses++;
                TotalHoldMs += holdMs;
            }
        }

        private static long EpochHour(DateTime local)
        {
            DateTime utc = local.ToUniversalTime();
            long sec = (long)(utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return sec / 3600 * 3600;
        }

        // ---------------------------------------------------------- 落库

        private void FlushLoop()
        {
            while (true)
            {
                flushEvent.WaitOne(10000);
                if (stop) break;
                FlushPending();
                flushEvent.Reset();
            }
            FlushPending();
        }

        public void FlushPending()
        {
            lock (storeGate)
            {
                FlushLocked();
            }
        }

        private void FlushLocked()
        {
            List<KeyValuePair<Key, Stat>> rows = null;
            lock (gate)
            {
                if (pending.Count > 0)
                {
                    rows = new List<KeyValuePair<Key, Stat>>(pending.Count);
                    foreach (KeyValuePair<Key, Stat> kv in pending)
                    {
                        Stat copy = new Stat();
                        copy.Count = kv.Value.Count;
                        copy.HoldMs = kv.Value.HoldMs;
                        rows.Add(new KeyValuePair<Key, Stat>(kv.Key, copy));
                    }
                    pending.Clear();
                }
            }
            if (rows != null) store.Append(rows);
        }

        /// <summary>切换记录目标。先把待写数据落到旧存储，再切换到新存储。</summary>
        public void SwitchStore(Store next)
        {
            if (next == null) return;
            lock (storeGate)
            {
                FlushLocked();
                store = next;
            }
        }

        public void RequestFlush() { flushEvent.Set(); }

        public void RequestRehook()
        {
            if (hookTid != 0) PostThreadMessage(hookTid, WM_REHOOK, IntPtr.Zero, IntPtr.Zero);
        }

        public void Stop()
        {
            stop = true;
            flushEvent.Set();
            if (hookTid != 0) PostThreadMessage(hookTid, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            try { if (hookThread != null) hookThread.Join(2000); } catch { }
            try { if (flushThread != null) flushThread.Join(3000); } catch { }
            FlushPending();
            Ok = false;
        }
    }
}
