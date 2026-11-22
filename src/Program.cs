using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Keytrace
{
    internal static class Program
    {
        private const int WM_SHOW = 0x8000 + 100;
        private const int WM_RESET = 0x8000 + 101;
        private const int WM_SWITCHGROUP = 0x8000 + 102;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int value);
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        private static void Main(string[] args)
        {
            bool background = false;
            bool reset = false;
            int viewIndex = 0;
            int groupId = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--background" || args[i] == "--no-window") background = true;
                if (args[i] == "--reset") reset = true;
                if ((args[i] == "--view" || args[i] == "-v") && i + 1 < args.Length)
                    int.TryParse(args[i + 1], out viewIndex);
                if ((args[i] == "--group" || args[i] == "-g") && i + 1 < args.Length)
                    int.TryParse(args[i + 1], out groupId);
            }

            bool created;
            Mutex mutex = new Mutex(true, "Local\\KeytraceNative", out created);
            if (!created)
            {
                IntPtr h = FindWindow(null, "Keytrace");
                if (h != IntPtr.Zero)
                {
                    if (groupId > 0) PostMessage(h, WM_SWITCHGROUP, (IntPtr)groupId, IntPtr.Zero);
                    if (reset) PostMessage(h, WM_RESET, IntPtr.Zero, IntPtr.Zero);
                    if (viewIndex > 0)
                        PostMessage(h, WM_SHOW, (IntPtr)viewIndex, IntPtr.Zero);
                }
                return;
            }

            try { SetProcessDpiAwareness(1); }
            catch { try { SetProcessDPIAware(); } catch { } }

            Ui.Init();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            GroupManager groups = null;
            Recorder recorder = null;
            try
            {
                groups = new GroupManager(dataDir, groupId);
                recorder = new Recorder(groups.Store);
                recorder.Start();

                MainForm form = new MainForm(groups, recorder, background);
                if (reset) form.ResetDataSilent();
                if (viewIndex > 0) form.SetView(viewIndex - 1);
                using (TrayContext ctx = new TrayContext(form, background))
                {
                    Application.Run(ctx);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Keytrace 启动失败：\n" + ex.Message, "Keytrace",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (recorder != null) recorder.Stop();
                if (groups != null) groups.Dispose();
                try { mutex.ReleaseMutex(); } catch { }
                mutex.Close();
            }
        }
    }

    internal class TrayContext : ApplicationContext
    {
        private readonly MainForm form;
        private readonly NotifyIcon icon;
        private readonly ToolStripMenuItem groupRoot;
        private bool exiting;

        public TrayContext(MainForm form, bool background)
        {
            this.form = form;
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            Icon ico = SystemIcons.Application;
            try { if (File.Exists(iconPath)) ico = new Icon(iconPath); } catch { }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = false;
            ToolStripMenuItem open = new ToolStripMenuItem("打开面板");
            open.Click += delegate { ShowForm(); };
            groupRoot = new ToolStripMenuItem("分组");
            ToolStripMenuItem quit = new ToolStripMenuItem("退出 Keytrace");
            quit.Click += delegate { ExitApp(); };
            menu.Items.Add(open);
            menu.Items.Add(groupRoot);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(quit);
            menu.Opening += delegate { FillGroups(); };

            icon = new NotifyIcon();
            icon.Icon = ico;
            icon.Text = "Keytrace · " + form.ActiveGroupName;
            icon.ContextMenuStrip = menu;
            icon.Visible = true;
            icon.MouseClick += delegate (object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left) ShowForm();
            };

            form.GroupsChanged += delegate { UpdateText(); };
            form.FormClosed += delegate
            {
                if (!exiting) ExitApp();
            };

            if (!background) form.Show();
        }

        private void FillGroups()
        {
            groupRoot.DropDownItems.Clear();
            for (int i = 0; i < form.GroupList.Count; i++)
            {
                Group g = form.GroupList[i];
                int id = g.Id;
                ToolStripMenuItem it = new ToolStripMenuItem(g.Name);
                it.Checked = id == form.ActiveGroupId;
                it.Click += delegate { form.SwitchGroupFromTray(id); };
                groupRoot.DropDownItems.Add(it);
            }
            groupRoot.DropDownItems.Add(new ToolStripSeparator());
            ToolStripMenuItem manage = new ToolStripMenuItem("管理分组…");
            manage.Click += delegate { form.ShowGroupsSettings(); };
            groupRoot.DropDownItems.Add(manage);
            UpdateText();
        }

        private void UpdateText()
        {
            string name = form.ActiveGroupName;
            if (name.Length > 32) name = name.Substring(0, 32) + "…";
            icon.Text = "Keytrace · " + name;
        }

        private void ShowForm()
        {
            form.ShowFromTray();
        }

        private void ExitApp()
        {
            if (exiting) return;
            exiting = true;
            icon.Visible = false;
            icon.Dispose();
            form.ExitApp();
            ExitThread();
        }
    }
}
