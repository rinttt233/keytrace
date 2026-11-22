using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Keytrace
{
    public partial class MainForm : Form
    {
        private const int WM_QUERYENDSESSION = 0x0011;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int WM_SHOW = 0x8000 + 100;
        private const int WM_RESET = 0x8000 + 101;
        private const int WM_SWITCHGROUP = 0x8000 + 102;
        private const int PBT_APMSUSPEND = 0x0004;
        private const int PBT_APMRESUMESUSPEND = 0x0007;
        private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

        private const int SidebarW = 248;
        private const int TopbarH = 70;
        private const int PadX = 30;
        private const int PadY = 26;

        private enum View { Overview, Heatmap, Trends, Settings }
        private enum RangeMode { Today, H24, D7, D30, All, Custom }
        private enum Metric { Count, Hold }

        private class Hit
        {
            public RectangleF Rect;
            public string Action;
            public int Vk;
        }

        private class KeyCap
        {
            public int Vk;
            public float X, Y, W = 1, H = 1;
            public string Label;
            public KeyCap(int vk, float x, float y, string label, float w, float h)
            {
                Vk = vk; X = x; Y = y; Label = label; W = w; H = h;
            }
        }

        private readonly GroupManager groups;
        private readonly Recorder recorder;
        private readonly bool background;
        private readonly DateTime started = DateTime.Now;
        private readonly List<Hit> hits = new List<Hit>();
        private readonly List<Hit> menuHits = new List<Hit>();
        private readonly List<KeyCap> caps = BuildKeyCaps();
        private readonly Timer refreshTimer;
        private readonly Timer statusTimer;
        private readonly string iconPath;
        private Store store { get { return groups.Store; } }

        private View view = View.Overview;
        private RangeMode range = RangeMode.H24;
        private Metric metric = Metric.Count;
        private DateTime customFrom = DateTime.Today.AddDays(-6);
        private DateTime customTo = DateTime.Today;
        private Snapshot snap;
        private float scroll;
        private float contentHeight;
        private string statusTitle = "记录中";
        private string statusMeta = "";
        private bool exiting;
        private int hoverIndex = -1;
        private int hoverVk;
        private Point mousePos;
        private int sortCol = 1;
        private bool sortDesc = true;
        private bool groupMenu;
        private bool hoverChip;
        private int groupMenuHover = -1;
        private RectangleF groupChipRect;

        private static readonly float[] HeatX = { 0f, 0.28f, 0.55f, 0.78f, 1f };
        private static readonly Color[] HeatC =
        {
            Color.FromArgb(240,242,246), Color.FromArgb(219,231,253),
            Color.FromArgb(156,194,251), Color.FromArgb(75,139,245),
            Color.FromArgb(29,78,216)
        };

        public MainForm(GroupManager groups, Recorder recorder, bool background)
        {
            this.groups = groups;
            this.recorder = recorder;
            this.background = background;
            iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.Sizable;
            Text = "Keytrace";
            BackColor = Ui.Bg;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = true;
            try { if (File.Exists(iconPath)) Icon = new Icon(iconPath); } catch { }
            try { Font = Ui.F13; } catch { }

            ApplyGeometry();
            Resize += delegate { ClampScroll(); Refresh(); };
            Paint += OnPaint;
            MouseMove += OnMouseMove;
            MouseClick += OnMouseClick;
            MouseWheel += OnMouseWheel;
            MouseLeave += delegate { hoverIndex = -1; hoverVk = 0; Refresh(); };
            KeyPreview = true;
            KeyDown += delegate (object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (groupMenu) { groupMenu = false; Refresh(); }
                    else { view = View.Overview; Refresh(); }
                }
                if (e.Control && e.KeyCode == Keys.G)
                {
                    groupMenu = !groupMenu;
                    groupMenuHover = -1;
                    hoverIndex = -1;
                    hoverVk = 0;
                    Refresh();
                    e.Handled = true;
                }
                if (e.Control && e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D4)
                {
                    SetView(e.KeyCode - Keys.D1);
                    e.Handled = true;
                }
            };

            refreshTimer = new Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += delegate { RefreshData(); };

            statusTimer = new Timer();
            statusTimer.Interval = 5000;
            statusTimer.Tick += delegate { RefreshStatus(); };

            VisibleChanged += delegate
            {
                refreshTimer.Enabled = Visible;
                statusTimer.Enabled = Visible;
                if (Visible)
                {
                    RefreshData();
                    RefreshStatus();
                }
            };

            Deactivate += delegate
            {
                if (!groupMenu) return;
                groupMenu = false;
                groupMenuHover = -1;
                Refresh();
            };

            RefreshStatus();
            RefreshData();
            IntPtr force = Handle;
        }

        public void ShowFromTray()
        {
            Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
            Refresh();
        }

        public void SetView(int index)
        {
            if (index < 0 || index > 3) return;
            view = (View)index;
            scroll = 0;
            ClampScroll();
            Refresh();
        }

        public IList<Group> GroupList { get { return groups.All; } }
        public int ActiveGroupId { get { return groups.ActiveId; } }
        public string ActiveGroupName { get { return groups.ActiveName; } }
        public event EventHandler GroupsChanged;

        public void SwitchGroupFromTray(int id) { SwitchGroup(id); }

        public void ShowGroupsSettings()
        {
            view = View.Settings;
            scroll = 0;
            ClampScroll();
            ShowFromTray();
        }

        private void RaiseGroupsChanged()
        {
            EventHandler h = GroupsChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        public void SwitchGroup(int id)
        {
            if (groups.Find(id) == null || id == groups.ActiveId) return;
            try
            {
                Store next = groups.Open(id);
                if (next == null) return;
                recorder.SwitchStore(next);
                groups.Activate(id, next);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "切换分组失败：" + ex.Message, "Keytrace",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            groupMenu = false;
            hoverVk = 0;
            hoverIndex = -1;
            snap = null;
            RefreshData();
            RefreshStatus();
            RaiseGroupsChanged();
        }

        private void NewGroup()
        {
            string initial = "分组 " + (groups.Count + 1);
            using (GroupNameDialog dlg = new GroupNameDialog("新建分组", initial))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                Group g;
                try { g = groups.Create(dlg.Value); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Keytrace");
                    return;
                }
                SwitchGroup(g.Id);
            }
        }

        private void RenameGroup(int id)
        {
            Group g = groups.Find(id);
            if (g == null) return;
            using (GroupNameDialog dlg = new GroupNameDialog("重命名分组", g.Name))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try { groups.Rename(id, dlg.Value); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Keytrace");
                    return;
                }
                RaiseGroupsChanged();
                Refresh();
            }
        }

        private void DeleteGroup(int id)
        {
            Group g = groups.Find(id);
            if (g == null) return;
            if (groups.Count <= 1)
            {
                MessageBox.Show(this, "至少需要保留一个分组。", "Keytrace");
                return;
            }
            if (id == groups.ActiveId)
            {
                int other = 0;
                for (int i = 0; i < groups.Count; i++)
                    if (groups.All[i].Id != id) { other = groups.All[i].Id; break; }
                string msg = "「" + g.Name + "」正在使用。删除后会切换到「" +
                    groups.Find(other).Name + "」，并删除该分组的全部记录。\n\n此操作不可恢复。";
                if (MessageBox.Show(this, msg, "Keytrace", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes) return;
                SwitchGroup(other);
                g = groups.Find(id);
                if (g == null) return;
            }
            else
            {
                if (MessageBox.Show(this, "删除分组「" + g.Name + "」及其全部记录？\n\n此操作不可恢复。",
                    "Keytrace", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            }
            string err;
            groups.Delete(id, out err);
            if (!string.IsNullOrEmpty(err))
                MessageBox.Show(this, err, "Keytrace");
            RaiseGroupsChanged();
            RefreshData();
            RefreshStatus();
        }

        public void ExitApp()
        {
            exiting = true;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!exiting && e.CloseReason != CloseReason.WindowsShutDown &&
                e.CloseReason != CloseReason.TaskManagerClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SHOW)
            {
                int v = m.WParam.ToInt32();
                if (v > 0) SetView(v - 1);
                ShowFromTray();
                return;
            }
            if (m.Msg == WM_RESET)
            {
                ResetDataSilent();
                return;
            }
            if (m.Msg == WM_SWITCHGROUP)
            {
                SwitchGroup(m.WParam.ToInt32());
                return;
            }
            if (m.Msg == WM_QUERYENDSESSION)
            {
                recorder.RequestFlush();
                m.Result = (IntPtr)1;
                base.WndProc(ref m);
                return;
            }
            if (m.Msg == WM_POWERBROADCAST)
            {
                int wp = m.WParam.ToInt32();
                if (wp == PBT_APMRESUMEAUTOMATIC || wp == PBT_APMRESUMESUSPEND) recorder.RequestRehook();
                else if (wp == PBT_APMSUSPEND) recorder.RequestFlush();
                m.Result = (IntPtr)1;
                return;
            }
            base.WndProc(ref m);
        }

        private void ApplyGeometry()
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            int w = Ui.Px(1360);
            int h = Ui.Px(900);
            if (w > wa.Width - Ui.Px(40)) w = wa.Width - Ui.Px(40);
            if (h > wa.Height - Ui.Px(40)) h = wa.Height - Ui.Px(40);
            if (w < Ui.Px(960)) w = Ui.Px(960);
            if (h < Ui.Px(640)) h = Ui.Px(640);
            ClientSize = new Size(w, h);
            Location = new Point(wa.Left + (wa.Width - w) / 2, wa.Top + (wa.Height - h) / 2);
            MinimumSize = new Size(Ui.Px(980), Ui.Px(660));
        }

        // ---------------------------------------------------------- 状态

        private void RefreshStatus()
        {
            string title = "未记录";
            string meta = "";
            try
            {
                title = recorder.Ok ? "记录中" : "未记录";
                meta = Ui.FmtBytes(store.SizeBytes) + " · " + Ui.FmtInt(store.Rows) + " 行";
            }
            catch { }
            bool changed = title != statusTitle || meta != statusMeta;
            statusTitle = title;
            statusMeta = meta;
            if (changed && Visible) Refresh();
        }

        private void RefreshData()
        {
            long since, until;
            GetBounds(out since, out until);
            try { snap = store.Build(since, until); }
            catch { snap = new Snapshot(); }
            ClampScroll();
            Refresh();
        }

        private void GetBounds(out long since, out long until)
        {
            DateTime now = DateTime.Now;
            until = ToEpoch(now) + 1;
            switch (range)
            {
                case RangeMode.Today:
                    since = ToEpoch(now.Date); break;
                case RangeMode.H24:
                    since = until - 86400; break;
                case RangeMode.D7:
                    since = until - 7 * 86400; break;
                case RangeMode.D30:
                    since = until - 30 * 86400; break;
                case RangeMode.All:
                    since = 0; break;
                case RangeMode.Custom:
                    since = ToEpoch(customFrom.Date);
                    until = ToEpoch(customTo.Date.AddDays(1));
                    break;
                default:
                    since = until - 86400; break;
            }
        }

        private static long ToEpoch(DateTime local)
        {
            return new DateTimeOffset(local).ToUnixTimeSeconds();
        }

        private string RangeLabel()
        {
            switch (range)
            {
                case RangeMode.Today: return "今天";
                case RangeMode.H24: return "近 24 小时";
                case RangeMode.D7: return "近 7 天";
                case RangeMode.D30: return "近 30 天";
                case RangeMode.All: return "全部";
                default: return "自定义";
            }
        }

        private string RangeSub()
        {
            if (snap == null) return "加载中…";
            string label = RangeLabel();
            long start = snap.FirstHour;
            if (range != RangeMode.All && range != RangeMode.Custom && snap.Until - snap.Since <= 86400)
                return label + " · " + Ui.FmtDateTime(snap.Since) + " → " + Ui.FmtDateTime(snap.Until - 1);
            if (range == RangeMode.All && snap.TotalCount == 0)
                return label + " · 暂无数据";
            return label + " · " + Ui.FmtDate(start) + " → " + Ui.FmtDate(snap.Until - 1) + " · 按小时统计";
        }

        // ---------------------------------------------------------- 交互

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
            PointF sp = new PointF(e.X, e.Y);

            if (groupMenu)
            {
                int mi = MenuIndex(sp);
                if (mi != groupMenuHover) { groupMenuHover = mi; Refresh(); }
                Cursor = mi >= 0 ? Cursors.Hand : Cursors.Default;
                return;
            }

            bool chip = groupChipRect.Contains(sp);
            bool changed = chip != hoverChip;
            hoverChip = chip;
            if (chip)
            {
                if (hoverIndex != -1 || hoverVk != 0) { hoverIndex = -1; hoverVk = 0; changed = true; }
                Cursor = Cursors.Hand;
            }
            else
            {
                PointF cp = new PointF(e.X, e.Y + scroll);
                int idx = HitIndex(cp);
                bool ch = idx != hoverIndex;
                int vk = 0;
                if (idx >= 0 && hits[idx].Action == "key") vk = hits[idx].Vk;
                if (vk != hoverVk) ch = true;
                hoverIndex = idx;
                hoverVk = vk;
                Cursor = idx >= 0 && hits[idx].Action != "key" ? Cursors.Hand : Cursors.Default;
                if (ch) changed = true;
            }
            if (changed) Refresh();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            PointF sp = new PointF(e.X, e.Y);

            if (groupMenu)
            {
                int mi = MenuIndex(sp);
                groupMenu = false;
                groupMenuHover = -1;
                if (mi >= 0)
                {
                    string ma = menuHits[mi].Action;
                    if (ma.StartsWith("gswitch:")) SwitchGroup(int.Parse(ma.Substring(8)));
                    else if (ma == "gnew") { Refresh(); NewGroup(); return; }
                    else if (ma == "gmanage") { view = View.Settings; scroll = 0; ClampScroll(); }
                }
                Refresh();
                return;
            }

            if (groupChipRect.Contains(sp))
            {
                groupMenu = true;
                groupMenuHover = -1;
                hoverIndex = -1;
                hoverVk = 0;
                Refresh();
                return;
            }

            PointF cp = new PointF(e.X, e.Y + scroll);
            int idx = HitIndex(cp);
            if (idx < 0) return;
            Hit h = hits[idx];
            string a = h.Action;
            if (a.StartsWith("nav:")) { view = (View)int.Parse(a.Substring(4)); scroll = 0; ClampScroll(); Refresh(); }
            else if (a.StartsWith("range:"))
            {
                int r = int.Parse(a.Substring(6));
                if (r == 5)
                {
                    using (CustomRangeDialog dlg = new CustomRangeDialog(customFrom, customTo))
                    {
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            customFrom = dlg.FromDate;
                            customTo = dlg.ToDate;
                            range = RangeMode.Custom;
                            scroll = 0;
                            RefreshData();
                        }
                    }
                }
                else { range = (RangeMode)r; scroll = 0; RefreshData(); }
            }
            else if (a.StartsWith("metric:")) { metric = (Metric)int.Parse(a.Substring(7)); scroll = 0; RefreshData(); }
            else if (a == "refresh") { RefreshData(); RefreshStatus(); }
            else if (a == "sort")
            {
                if (sortCol == h.Vk) sortDesc = !sortDesc;
                else { sortCol = h.Vk; sortDesc = true; }
                Refresh();
            }
            else if (a == "open") { try { Process.Start("explorer.exe", store.Dir); } catch { } }
            else if (a == "export") ExportCsv();
            else if (a == "reset") ResetData();
            else if (a == "autostart") ToggleAutostart();
            else if (a == "quit") ExitApp();
            else if (a == "gnew") NewGroup();
            else if (a.StartsWith("grename:")) RenameGroup(int.Parse(a.Substring(8)));
            else if (a.StartsWith("gdelete:")) DeleteGroup(int.Parse(a.Substring(8)));
            else if (a.StartsWith("gswitch:")) SwitchGroup(int.Parse(a.Substring(8)));
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (contentHeight <= ClientSize.Height) return;
            scroll -= (e.Delta / 120f) * Ui.F(80);
            ClampScroll();
            Refresh();
        }

        private int HitIndex(PointF p)
        {
            for (int i = hits.Count - 1; i >= 0; i--)
                if (hits[i].Rect.Contains(p)) return i;
            return -1;
        }

        private void ClampScroll()
        {
            float max = Math.Max(0, contentHeight - ClientSize.Height);
            if (scroll < 0) scroll = 0;
            if (scroll > max) scroll = max;
        }

        private float ViewportHeight()
        {
            return ClientSize.Height - Ui.F(TopbarH);
        }

        private void AddHit(RectangleF r, string action)
        {
            Hit h = new Hit();
            h.Rect = r;
            h.Action = action;
            hits.Add(h);
        }

        private void AddHit(Graphics g, RectangleF r, string action)
        {
            AddHit(r, action);
        }

        private void AddKeyHit(RectangleF r, int vk)
        {
            Hit h = new Hit();
            h.Rect = r;
            h.Action = "key";
            h.Vk = vk;
            hits.Add(h);
        }

        private void AddFixedHit(RectangleF r, string action)
        {
            AddHit(new RectangleF(r.X, r.Y - scroll, r.Width, r.Height), action);
        }

        private void AddMenuHit(RectangleF r, string action)
        {
            Hit h = new Hit();
            h.Rect = r;
            h.Action = action;
            menuHits.Add(h);
        }

        private int MenuIndex(PointF p)
        {
            for (int i = menuHits.Count - 1; i >= 0; i--)
                if (menuHits[i].Rect.Contains(p)) return i;
            return -1;
        }

        // ---------------------------------------------------------- 导出 / 清空 / 自启动

        private void ExportCsv()
        {
            long since, until;
            GetBounds(out since, out until);
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV 文件 (*.csv)|*.csv";
                string gn = groups.ActiveName;
                foreach (char bad in Path.GetInvalidFileNameChars()) gn = gn.Replace(bad, '_');
                dlg.FileName = "keytrace_" + gn + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    Cursor = Cursors.WaitCursor;
                    store.ExportCsv(dlg.FileName, since, until);
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, "已导出到：\n" + dlg.FileName, "Keytrace",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, "导出失败：" + ex.Message, "Keytrace",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ResetData()
        {
            if (MessageBox.Show(this, "确定清空当前分组「" + groups.ActiveName + "」的所有按键记录？\n\n" +
                "其他分组不受影响，此操作不可恢复。", "Keytrace",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            ResetDataSilent();
        }

        public void ResetDataSilent()
        {
            try
            {
                recorder.FlushPending();
                store.Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "清空失败：" + ex.Message, "Keytrace");
            }
            RefreshData();
            RefreshStatus();
        }

        private bool GetAutostart()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (k == null) return false;
                    object v = k.GetValue("KeytraceNative");
                    return v != null && v.ToString().Length > 0;
                }
            }
            catch { return false; }
        }

        private void ToggleAutostart()
        {
            try
            {
                bool on = !GetAutostart();
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (on)
                        k.SetValue("KeytraceNative",
                            "\"" + Application.ExecutablePath + "\" --background");
                    else
                        k.DeleteValue("KeytraceNative", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "写入自启动失败：" + ex.Message, "Keytrace");
            }
            Refresh();
        }

        // ---------------------------------------------------------- 布局辅助

        private RectangleF ContentRect()
        {
            float s = Ui.S;
            return new RectangleF(SidebarW * s, TopbarH * s,
                ClientSize.Width - SidebarW * s, ClientSize.Height - TopbarH * s);
        }

        private float ContentX { get { return Ui.F(SidebarW + PadX); } }
        private float ContentY { get { return Ui.F(TopbarH + PadY); } }
        private float ContentW { get { return ClientSize.Width - Ui.F(SidebarW + PadX * 2); } }

        private static float Gap { get { return Ui.F(16); } }

        // ---------------------------------------------------------- 绘制入口

        private void OnPaint(object sender, PaintEventArgs e)
        {
            hits.Clear();
            menuHits.Clear();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Ui.Bg);

            DrawSidebar(g);
            DrawTopbar(g);

            RectangleF content = ContentRect();
            g.SetClip(content);
            g.TranslateTransform(0, -scroll);
            switch (view)
            {
                case View.Overview: DrawOverview(g); break;
                case View.Heatmap: DrawHeatmap(g); break;
                case View.Trends: DrawTrends(g); break;
                default: DrawSettings(g); break;
            }
            g.ResetTransform();
            g.ResetClip();

            DrawScrollbar(g);
            DrawTooltip(g);
            DrawGroupMenu(g);
        }

        private void DrawSidebar(Graphics g)
        {
            float s = Ui.S;
            float w = SidebarW * s;
            using (SolidBrush b = new SolidBrush(Ui.Side))
                g.FillRectangle(b, 0, 0, w, ClientSize.Height);
            Ui.DrawLine(g, w - 0.5f, 0, w - 0.5f, ClientSize.Height, Ui.Line, 1);

            float lx = Ui.F(20), ly = Ui.F(20);
            RectangleF logo = new RectangleF(lx, ly, Ui.F(32), Ui.F(32));
            using (LinearGradientBrush b = new LinearGradientBrush(logo,
                Color.FromArgb(75, 139, 255), Ui.Accent2, 45f))
                g.FillPath(b, Ui.RoundPath(logo, Ui.F(9)));
            Ui.DrawText(g, "K", Ui.FontPx(17, true), Color.White, logo,
                StringAlignment.Center, StringAlignment.Center);

            Ui.DrawText(g, "Keytrace", Ui.FontPx(15, true), Ui.Text,
                new RectangleF(lx + Ui.F(42), ly, w - lx - Ui.F(50), Ui.F(18)));
            Ui.DrawText(g, "键迹", Ui.FontPx(10.5f, false), Ui.Text3,
                new RectangleF(lx + Ui.F(42), ly + Ui.F(17), Ui.F(80), Ui.F(14)));

            // 当前分组选择器
            float chipY = Ui.F(62);
            RectangleF chip = new RectangleF(Ui.F(14), chipY, w - Ui.F(28), Ui.F(46));
            groupChipRect = chip;
            bool chipOn = hoverChip || groupMenu;
            Ui.FillRound(g, chip, Ui.F(10), chipOn ? Color.FromArgb(246, 247, 250) : Color.White);
            Ui.StrokeRound(g, chip, Ui.F(10), chipOn ? Color.FromArgb(222, 225, 232) : Ui.Line, 1);

            RectangleF av = new RectangleF(chip.X + Ui.F(9), chip.Y + Ui.F(8), Ui.F(30), Ui.F(30));
            using (LinearGradientBrush b = new LinearGradientBrush(av,
                Color.FromArgb(238, 244, 255), Color.FromArgb(224, 234, 254), 90f))
                g.FillPath(b, Ui.RoundPath(av, Ui.F(9)));
            string initial = groups.ActiveName.Length > 0 ? groups.ActiveName.Substring(0, 1) : "G";
            Ui.DrawText(g, initial, Ui.FontPx(13, true), Ui.Accent2, av,
                StringAlignment.Center, StringAlignment.Center);

            float textX = av.Right + Ui.F(10);
            Ui.DrawText(g, groups.ActiveName, Ui.FontPx(12.5f, true), Ui.Text,
                new RectangleF(textX, chip.Y + Ui.F(7), chip.Right - textX - Ui.F(26), Ui.F(18)));
            Ui.DrawText(g, "当前分组", Ui.FontPx(10, false), Ui.Text3,
                new RectangleF(textX, chip.Y + Ui.F(25), chip.Right - textX - Ui.F(26), Ui.F(14)));

            using (Pen p = new Pen(Ui.Text3, Ui.F(1.5f)))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                float cx = chip.Right - Ui.F(16);
                float cy = chip.Y + chip.Height / 2;
                if (groupMenu)
                {
                    g.DrawLine(p, cx - Ui.F(4), cy + Ui.F(2), cx, cy - Ui.F(2));
                    g.DrawLine(p, cx, cy - Ui.F(2), cx + Ui.F(4), cy + Ui.F(2));
                }
                else
                {
                    g.DrawLine(p, cx - Ui.F(4), cy - Ui.F(2), cx, cy + Ui.F(2));
                    g.DrawLine(p, cx, cy + Ui.F(2), cx + Ui.F(4), cy - Ui.F(2));
                }
            }

            string[] nav = { "概览", "热力图", "趋势", "设置" };
            float ny = Ui.F(122);
            for (int i = 0; i < 4; i++)
            {
                RectangleF r = new RectangleF(Ui.F(14), ny + i * Ui.F(42), w - Ui.F(28), Ui.F(36));
                if ((int)view == i)
                    Ui.FillRound(g, r, Ui.F(9), Color.FromArgb(236, 239, 244));
                Color c = (int)view == i ? Ui.Text : Ui.Text2;
                Ui.DrawText(g, nav[i], Ui.F13, c,
                    new RectangleF(r.X + Ui.F(27), r.Y, r.Width - Ui.F(35), r.Height));
                DrawNavIcon(g, i, new RectangleF(r.X + Ui.F(9), r.Y + Ui.F(9), Ui.F(18), Ui.F(18)), c);
                AddFixedHit(r, "nav:" + i);
            }

            float cardH = Ui.F(78);
            RectangleF card = new RectangleF(Ui.F(14), ClientSize.Height - cardH - Ui.F(44),
                w - Ui.F(28), cardH);
            Ui.FillRound(g, card, Ui.F(11), Color.White);
            Ui.StrokeRound(g, card, Ui.F(11), Ui.Line, 1);
            using (SolidBrush b = new SolidBrush(recorder.Ok ? Ui.Green : Ui.Red))
                g.FillEllipse(b, card.X + Ui.F(12), card.Y + Ui.F(22), Ui.F(7), Ui.F(7));
            Ui.DrawText(g, statusTitle, Ui.FontPx(12.5f, true), Ui.Text,
                new RectangleF(card.X + Ui.F(26), card.Y + Ui.F(12), card.Width - Ui.F(34), Ui.F(20)));
            Ui.DrawText(g, statusMeta, Ui.FontPx(11, false), Ui.Text3,
                new RectangleF(card.X + Ui.F(12), card.Y + Ui.F(38), card.Width - Ui.F(20), Ui.F(18)));

            Ui.DrawText(g, "本机记录 · 不联网", Ui.FontPx(10.5f, false), Ui.Text3,
                new RectangleF(Ui.F(18), ClientSize.Height - Ui.F(34), w - Ui.F(36), Ui.F(18)));
        }

        private void DrawNavIcon(Graphics g, int i, RectangleF r, Color c)
        {
            using (Pen p = new Pen(c, Ui.F(1.6f)))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                if (i == 0)
                {
                    float q = r.Width / 2 - Ui.F(1);
                    g.DrawRectangle(p, r.X + Ui.F(2), r.Y + Ui.F(2), q - Ui.F(1), q - Ui.F(1));
                    g.DrawRectangle(p, r.X + q + Ui.F(3), r.Y + Ui.F(2), q - Ui.F(1), q - Ui.F(1));
                    g.DrawRectangle(p, r.X + Ui.F(2), r.Y + q + Ui.F(3), q - Ui.F(1), q - Ui.F(1));
                    g.DrawRectangle(p, r.X + q + Ui.F(3), r.Y + q + Ui.F(3), q - Ui.F(1), q - Ui.F(1));
                }
                else if (i == 1)
                {
                    g.DrawRectangle(p, r.X + Ui.F(2), r.Y + Ui.F(5), r.Width - Ui.F(4), r.Height - Ui.F(10));
                    g.DrawLine(p, r.X + Ui.F(6), r.Y + Ui.F(10), r.X + Ui.F(7), r.Y + Ui.F(10));
                    g.DrawLine(p, r.X + Ui.F(10), r.Y + Ui.F(10), r.X + Ui.F(11), r.Y + Ui.F(10));
                    g.DrawLine(p, r.X + Ui.F(14), r.Y + Ui.F(10), r.X + Ui.F(15), r.Y + Ui.F(10));
                    g.DrawLine(p, r.X + Ui.F(5), r.Y + Ui.F(14), r.X + Ui.F(15), r.Y + Ui.F(14));
                }
                else if (i == 2)
                {
                    g.DrawLine(p, r.X + Ui.F(3), r.Y + Ui.F(3), r.X + Ui.F(3), r.Bottom - Ui.F(3));
                    g.DrawLine(p, r.X + Ui.F(3), r.Bottom - Ui.F(3), r.Right - Ui.F(2), r.Bottom - Ui.F(3));
                    g.DrawLines(p, new PointF[]
                    {
                        new PointF(r.X + Ui.F(6), r.Bottom - Ui.F(7)),
                        new PointF(r.X + Ui.F(10), r.Bottom - Ui.F(11)),
                        new PointF(r.X + Ui.F(13), r.Bottom - Ui.F(9)),
                        new PointF(r.Right - Ui.F(3), r.Y + Ui.F(5))
                    });
                }
                else
                {
                    g.DrawLine(p, r.X + Ui.F(2), r.Y + Ui.F(6), r.Right - Ui.F(2), r.Y + Ui.F(6));
                    g.DrawLine(p, r.X + Ui.F(2), r.Bottom - Ui.F(6), r.Right - Ui.F(2), r.Bottom - Ui.F(6));
                    g.DrawEllipse(p, r.X + Ui.F(9), r.Y + Ui.F(4), Ui.F(4), Ui.F(4));
                    g.DrawEllipse(p, r.X + Ui.F(9), r.Bottom - Ui.F(8), Ui.F(4), Ui.F(4));
                }
            }
        }

        private void DrawTopbar(Graphics g)
        {
            float s = Ui.S;
            float x = SidebarW * s;
            using (SolidBrush b = new SolidBrush(Color.FromArgb(252, 252, 253)))
                g.FillRectangle(b, x, 0, ClientSize.Width - x, TopbarH * s);
            Ui.DrawLine(g, x, TopbarH * s - 0.5f, ClientSize.Width, TopbarH * s - 0.5f, Ui.Line, 1);

            string[] titles = { "概览", "热力图", "趋势", "设置" };
            string[] subs = { "键盘使用情况一览", "颜色越深，使用越多", "按小时统计的变化", "启动、数据与隐私" };
            Ui.DrawText(g, titles[(int)view], Ui.F15B, Ui.Text,
                new RectangleF(x + Ui.F(30), Ui.F(16), Ui.F(260), Ui.F(22)));
            Ui.DrawText(g, view == View.Settings ? subs[3] : RangeSub(), Ui.F11, Ui.Text3,
                new RectangleF(x + Ui.F(30), Ui.F(40), Ui.F(560), Ui.F(18)));

            if (view != View.Settings)
            {
                float rx = ClientSize.Width - Ui.F(30);
                RectangleF rb = new RectangleF(rx - Ui.F(32), Ui.F(19), Ui.F(32), Ui.F(32));
                Ui.FillRound(g, rb, Ui.F(8), Color.White);
                Ui.StrokeRound(g, rb, Ui.F(8), Ui.Line, 1);
                using (Pen p = new Pen(Ui.Text2, Ui.F(1.6f)))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawArc(p, rb.X + Ui.F(9), rb.Y + Ui.F(9), Ui.F(14), Ui.F(14), 20, 300);
                    g.DrawLine(p, rb.X + Ui.F(23), rb.Y + Ui.F(7), rb.X + Ui.F(23), rb.Y + Ui.F(12));
                }
                AddHit(rb, "refresh");
                rx = rb.X - Ui.F(10);

                string[] mt = { "次数", "按住时长" };
                float mw = Ui.F(118);
                RectangleF mr = new RectangleF(rx - mw, Ui.F(19), mw, Ui.F(32));
                Ui.FillRound(g, mr, Ui.F(9), Color.FromArgb(241, 242, 245));
                for (int i = 0; i < 2; i++)
                {
                    RectangleF br = new RectangleF(mr.X + Ui.F(3) + i * (mw - Ui.F(6)) / 2,
                        mr.Y + Ui.F(3), (mw - Ui.F(6)) / 2, mr.Height - Ui.F(6));
                    if ((int)metric == i) Ui.FillRound(g, br, Ui.F(7), Color.White);
                    Ui.DrawText(g, mt[i], Ui.F11, (int)metric == i ? Ui.Text : Ui.Text2, br,
                        StringAlignment.Center, StringAlignment.Center);
                    AddHit(br, "metric:" + i);
                }
                rx = mr.X - Ui.F(10);

                string[] rt = { "今天", "24 小时", "7 天", "30 天", "全部", "自定义" };
                float[] rw = { 52, 66, 48, 54, 48, 62 };
                float total = 0;
                for (int i = 0; i < rw.Length; i++) total += Ui.F(rw[i]);
                total += Ui.F(rw.Length * 2);
                RectangleF rr = new RectangleF(rx - total, Ui.F(19), total, Ui.F(32));
                Ui.FillRound(g, rr, Ui.F(9), Color.FromArgb(241, 242, 245));
                float bx = rr.X + Ui.F(3);
                for (int i = 0; i < rw.Length; i++)
                {
                    float bw = Ui.F(rw[i]);
                    RectangleF br = new RectangleF(bx, rr.Y + Ui.F(3), bw, rr.Height - Ui.F(6));
                    if ((int)range == i) Ui.FillRound(g, br, Ui.F(7), Color.White);
                    Ui.DrawText(g, rt[i], Ui.F11, (int)range == i ? Ui.Text : Ui.Text2, br,
                        StringAlignment.Center, StringAlignment.Center);
                    AddHit(br, "range:" + i);
                    bx += bw + Ui.F(2);
                }
            }
        }

        private void DrawGroupMenu(Graphics g)
        {
            if (!groupMenu) return;
            float w = Ui.F(234);
            float rowH = Ui.F(34);
            float headH = Ui.F(32);
            int n = groups.Count;
            float footH = Ui.F(78);
            float h = headH + n * rowH + footH;
            float x = groupChipRect.X;
            float y = groupChipRect.Bottom + Ui.F(6);
            if (y + h > ClientSize.Height - Ui.F(10))
                y = Math.Max(Ui.F(10), ClientSize.Height - Ui.F(10) - h);

            using (SolidBrush b = new SolidBrush(Color.FromArgb(10, 15, 23, 42)))
                g.FillPath(b, Ui.RoundPath(new RectangleF(x - Ui.F(2), y + Ui.F(4), w + Ui.F(4), h), Ui.F(13)));
            RectangleF box = new RectangleF(x, y, w, h);
            Ui.FillRound(g, box, Ui.F(12), Color.White);
            Ui.StrokeRound(g, box, Ui.F(12), Color.FromArgb(225, 228, 234), 1);

            Ui.DrawText(g, "切换分组", Ui.FontPx(10.5f, false), Ui.Text3,
                new RectangleF(box.X + Ui.F(16), box.Y + Ui.F(7), w - Ui.F(32), Ui.F(18)));

            float iy = y + headH;
            for (int i = 0; i < n; i++)
            {
                Group grp = groups.All[i];
                RectangleF rr = new RectangleF(x + Ui.F(6), iy, w - Ui.F(12), rowH);
                if (groupMenuHover == i) Ui.FillRound(g, rr, Ui.F(7), Color.FromArgb(243, 245, 249));
                bool act = grp.Id == groups.ActiveId;
                float dy = rr.Y + rowH / 2;
                using (Pen p = new Pen(act ? Ui.Accent : Color.FromArgb(203, 208, 216), Ui.F(1.4f)))
                    g.DrawEllipse(p, rr.X + Ui.F(12), dy - Ui.F(5), Ui.F(10), Ui.F(10));
                if (act)
                    using (SolidBrush b = new SolidBrush(Ui.Accent))
                        g.FillEllipse(b, rr.X + Ui.F(15), dy - Ui.F(2), Ui.F(4), Ui.F(4));
                Ui.DrawText(g, grp.Name, Ui.FontPx(12, act), Ui.Text,
                    new RectangleF(rr.X + Ui.F(30), rr.Y, rr.Width - Ui.F(78), rowH));
                if (act)
                {
                    RectangleF tag = new RectangleF(rr.Right - Ui.F(50), rr.Y + Ui.F(7), Ui.F(40), Ui.F(20));
                    Ui.FillRound(g, tag, Ui.F(6), Ui.AccentSoft);
                    Ui.DrawText(g, "当前", Ui.FontPx(9.5f, false), Ui.Accent,
                        tag, StringAlignment.Center, StringAlignment.Center);
                }
                AddMenuHit(rr, "gswitch:" + grp.Id);
                iy += rowH;
            }

            Ui.DrawLine(g, x + Ui.F(12), iy + 0.5f, x + w - Ui.F(12), iy + 0.5f, Ui.LineSoft, 1);
            iy += Ui.F(8);

            RectangleF nr = new RectangleF(x + Ui.F(6), iy, w - Ui.F(12), Ui.F(32));
            if (groupMenuHover == n) Ui.FillRound(g, nr, Ui.F(7), Color.FromArgb(243, 245, 249));
            Ui.DrawText(g, "＋ 新建分组…", Ui.FontPx(12, false), Ui.Text,
                new RectangleF(nr.X + Ui.F(16), nr.Y, nr.Width - Ui.F(28), nr.Height));
            AddMenuHit(nr, "gnew");
            iy += Ui.F(34);

            RectangleF mr = new RectangleF(x + Ui.F(6), iy, w - Ui.F(12), Ui.F(32));
            if (groupMenuHover == n + 1) Ui.FillRound(g, mr, Ui.F(7), Color.FromArgb(243, 245, 249));
            Ui.DrawText(g, "管理分组…", Ui.FontPx(12, false), Ui.Text,
                new RectangleF(mr.X + Ui.F(16), mr.Y, mr.Width - Ui.F(28), mr.Height));
            AddMenuHit(mr, "gmanage");
        }

        private void DrawScrollbar(Graphics g)
        {
            float vh = ClientSize.Height - Ui.F(TopbarH);
            if (contentHeight <= ClientSize.Height + 1) return;
            float trackX = ClientSize.Width - Ui.F(13);
            float trackY = Ui.F(TopbarH) + Ui.F(6);
            float trackH = vh - Ui.F(12);
            Ui.FillRound(g, new RectangleF(trackX, trackY, Ui.F(7), trackH), Ui.F(4),
                Color.FromArgb(240, 241, 245));
            float ratio = vh / contentHeight;
            float thumbH = Math.Max(Ui.F(30), trackH * ratio);
            float max = contentHeight - ClientSize.Height;
            float t = max <= 0 ? 0 : Math.Min(1, scroll / max);
            float thumbY = trackY + (trackH - thumbH) * t;
            Ui.FillRound(g, new RectangleF(trackX, thumbY, Ui.F(7), thumbH), Ui.F(4),
                Color.FromArgb(210, 213, 220));
        }

        private void DrawTooltip(Graphics g)
        {
            if (hoverVk == 0 || groupMenu) return;
            if (view != View.Heatmap && view != View.Overview) return;
            KeyStat ks = FindKey(hoverVk);
            if (ks == null) return;
            string[] lines =
            {
                ks.Name,
                "次数  " + Ui.FmtInt(ks.Count),
                "按住  " + Ui.FmtDur(ks.HoldMs),
                "平均  " + (ks.Count > 0 ? Ui.FmtDur(ks.Avg) : "—"),
                "占比  " + (snap.TotalCount > 0 ? (ks.Count * 100.0 / snap.TotalCount).ToString("0.00") + "%" : "—")
            };
            float w = Ui.F(200), h = Ui.F(118);
            float x = mousePos.X + Ui.F(16);
            float y = mousePos.Y + Ui.F(16);
            if (x + w > ClientSize.Width) x = mousePos.X - w - Ui.F(12);
            if (y + h > ClientSize.Height) y = mousePos.Y - h - Ui.F(12);
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            RectangleF r = new RectangleF(x, y, w, h);
            Ui.FillRound(g, r, Ui.F(9), Color.FromArgb(17, 24, 39));
            Ui.DrawText(g, lines[0], Ui.FontPx(12.5f, true), Color.White,
                new RectangleF(r.X + Ui.F(12), r.Y + Ui.F(8), w - Ui.F(24), Ui.F(20)));
            for (int i = 1; i < lines.Length; i++)
                Ui.DrawText(g, lines[i], Ui.FontPx(11.5f, false), Color.FromArgb(209, 213, 219),
                    new RectangleF(r.X + Ui.F(12), r.Y + Ui.F(10) + i * Ui.F(20), w - Ui.F(24), Ui.F(18)));
        }

        private KeyStat FindKey(int vk)
        {
            if (snap == null) return null;
            for (int i = 0; i < snap.Keys.Count; i++)
                if (snap.Keys[i].Vk == vk) return snap.Keys[i];
            return null;
        }
    }
}
