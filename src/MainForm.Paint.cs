using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Keytrace
{
    public partial class MainForm
    {
        private static List<KeyCap> BuildKeyCaps()
        {
            List<KeyCap> a = new List<KeyCap>();
            a.Add(new KeyCap(0x1B, 0, 0, "Esc", 1, 1));
            a.Add(new KeyCap(0x70, 1.25f, 0, "F1", 1, 1));
            a.Add(new KeyCap(0x71, 2.25f, 0, "F2", 1, 1));
            a.Add(new KeyCap(0x72, 3.25f, 0, "F3", 1, 1));
            a.Add(new KeyCap(0x73, 4.25f, 0, "F4", 1, 1));
            a.Add(new KeyCap(0x74, 5.5f, 0, "F5", 1, 1));
            a.Add(new KeyCap(0x75, 6.5f, 0, "F6", 1, 1));
            a.Add(new KeyCap(0x76, 7.5f, 0, "F7", 1, 1));
            a.Add(new KeyCap(0x77, 8.5f, 0, "F8", 1, 1));
            a.Add(new KeyCap(0x78, 9.75f, 0, "F9", 1, 1));
            a.Add(new KeyCap(0x79, 10.75f, 0, "F10", 1, 1));
            a.Add(new KeyCap(0x7A, 11.75f, 0, "F11", 1, 1));
            a.Add(new KeyCap(0x7B, 12.75f, 0, "F12", 1, 1));
            a.Add(new KeyCap(0x2C, 15.5f, 0, "PrtSc", 1, 1));
            a.Add(new KeyCap(0x91, 16.5f, 0, "ScrLk", 1, 1));
            a.Add(new KeyCap(0x13, 17.5f, 0, "Pause", 1, 1));

            a.Add(new KeyCap(0xC0, 0, 1, "`", 1, 1));
            int[] dvk = { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30 };
            string[] dlab = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            for (int i = 0; i < 10; i++) a.Add(new KeyCap(dvk[i], 1 + i, 1, dlab[i], 1, 1));
            a.Add(new KeyCap(0xBD, 11, 1, "-", 1, 1));
            a.Add(new KeyCap(0xBB, 12, 1, "=", 1, 1));
            a.Add(new KeyCap(0x08, 13, 1, "Backspace", 2, 1));
            a.Add(new KeyCap(0x2D, 15.5f, 1, "Ins", 1, 1));
            a.Add(new KeyCap(0x24, 16.5f, 1, "Home", 1, 1));
            a.Add(new KeyCap(0x21, 17.5f, 1, "PgUp", 1, 1));
            a.Add(new KeyCap(0x90, 19, 1, "Num", 1, 1));
            a.Add(new KeyCap(0x6F, 20, 1, "/", 1, 1));
            a.Add(new KeyCap(0x6A, 21, 1, "*", 1, 1));
            a.Add(new KeyCap(0x6D, 22, 1, "-", 1, 1));

            a.Add(new KeyCap(0x09, 0, 2, "Tab", 1.5f, 1));
            int[] qvk = { 0x51, 0x57, 0x45, 0x52, 0x54, 0x59, 0x55, 0x49, 0x4F, 0x50 };
            string[] qlab = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
            for (int i = 0; i < 10; i++) a.Add(new KeyCap(qvk[i], 1.5f + i, 2, qlab[i], 1, 1));
            a.Add(new KeyCap(0xDB, 11.5f, 2, "[", 1, 1));
            a.Add(new KeyCap(0xDD, 12.5f, 2, "]", 1, 1));
            a.Add(new KeyCap(0xDC, 13.5f, 2, "\\", 1.5f, 1));
            a.Add(new KeyCap(0x2E, 15.5f, 2, "Del", 1, 1));
            a.Add(new KeyCap(0x23, 16.5f, 2, "End", 1, 1));
            a.Add(new KeyCap(0x22, 17.5f, 2, "PgDn", 1, 1));
            a.Add(new KeyCap(0x67, 19, 2, "7", 1, 1));
            a.Add(new KeyCap(0x68, 20, 2, "8", 1, 1));
            a.Add(new KeyCap(0x69, 21, 2, "9", 1, 1));
            a.Add(new KeyCap(0x6B, 22, 2, "+", 1, 2));

            a.Add(new KeyCap(0x14, 0, 3, "Caps", 1.75f, 1));
            int[] avk = { 0x41, 0x53, 0x44, 0x46, 0x47, 0x48, 0x4A, 0x4B, 0x4C };
            string[] alab = { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
            for (int i = 0; i < 9; i++) a.Add(new KeyCap(avk[i], 1.75f + i, 3, alab[i], 1, 1));
            a.Add(new KeyCap(0xBA, 10.75f, 3, ";", 1, 1));
            a.Add(new KeyCap(0xDE, 11.75f, 3, "'", 1, 1));
            a.Add(new KeyCap(0x0D, 12.75f, 3, "Enter", 2.25f, 1));
            a.Add(new KeyCap(0x26, 16.5f, 3, "↑", 1, 1));
            a.Add(new KeyCap(0x64, 19, 3, "4", 1, 1));
            a.Add(new KeyCap(0x65, 20, 3, "5", 1, 1));
            a.Add(new KeyCap(0x66, 21, 3, "6", 1, 1));

            a.Add(new KeyCap(0xA0, 0, 4, "Shift", 2.25f, 1));
            int[] zvk = { 0x5A, 0x58, 0x43, 0x56, 0x42, 0x4E, 0x4D };
            string[] zlab = { "Z", "X", "C", "V", "B", "N", "M" };
            for (int i = 0; i < 7; i++) a.Add(new KeyCap(zvk[i], 2.25f + i, 4, zlab[i], 1, 1));
            a.Add(new KeyCap(0xBC, 9.25f, 4, ",", 1, 1));
            a.Add(new KeyCap(0xBE, 10.25f, 4, ".", 1, 1));
            a.Add(new KeyCap(0xBF, 11.25f, 4, "/", 1, 1));
            a.Add(new KeyCap(0xA1, 12.25f, 4, "Shift", 2.75f, 1));
            a.Add(new KeyCap(0x25, 15.5f, 4, "←", 1, 1));
            a.Add(new KeyCap(0x28, 16.5f, 4, "↓", 1, 1));
            a.Add(new KeyCap(0x27, 17.5f, 4, "→", 1, 1));
            a.Add(new KeyCap(0x61, 19, 4, "1", 1, 1));
            a.Add(new KeyCap(0x62, 20, 4, "2", 1, 1));
            a.Add(new KeyCap(0x63, 21, 4, "3", 1, 1));
            a.Add(new KeyCap(0x0D, 22, 4, "Enter", 1, 2));

            a.Add(new KeyCap(0xA2, 0, 5, "Ctrl", 1.25f, 1));
            a.Add(new KeyCap(0x5B, 1.25f, 5, "Win", 1.25f, 1));
            a.Add(new KeyCap(0xA4, 2.5f, 5, "Alt", 1.25f, 1));
            a.Add(new KeyCap(0x20, 3.75f, 5, "Space", 6.25f, 1));
            a.Add(new KeyCap(0xA5, 10, 5, "Alt", 1.25f, 1));
            a.Add(new KeyCap(0x5C, 11.25f, 5, "Win", 1.25f, 1));
            a.Add(new KeyCap(0x5D, 12.5f, 5, "Menu", 1.25f, 1));
            a.Add(new KeyCap(0xA3, 13.75f, 5, "Ctrl", 1.25f, 1));
            a.Add(new KeyCap(0x60, 19, 5, "0", 2, 1));
            a.Add(new KeyCap(0x6E, 21, 5, ".", 1, 1));
            return a;
        }

        // ---------------------------------------------------------- 概览

        private void DrawOverview(Graphics g)
        {
            float left = ContentX, w = ContentW;
            float gap = Gap;
            float cardH = Ui.F(96);
            float cw = (w - gap * 3) / 4;

            long count = snap != null ? snap.TotalCount : 0;
            double hold = snap != null ? snap.TotalHoldMs : 0;
            int keys = snap != null ? snap.KeyCount : 0;
            double avg = count > 0 ? hold / count : 0;
            long prev = snap != null ? snap.PrevCount : 0;

            float hintH = count == 0 ? Ui.F(40) : 0;
            float top = ContentY;
            if (hintH > 0)
            {
                RectangleF hint = new RectangleF(left, top, w, hintH);
                Ui.FillRound(g, hint, Ui.F(10), Ui.AccentSoft);
                Ui.StrokeRound(g, hint, Ui.F(10), Color.FromArgb(216, 228, 253), 1);
                Ui.DrawText(g, "还没有记录到按键。开始打字后，这里会自动出现统计。",
                    Ui.F12, Color.FromArgb(36, 86, 184),
                    new RectangleF(hint.X + Ui.F(14), hint.Y, hint.Width - Ui.F(28), hint.Height));
                top += hintH + gap;
            }

            string[] labels = { "总敲击次数", "按住总时长", "平均按住", "活跃按键" };
            string[] values =
            {
                Ui.FmtInt(count),
                Ui.FmtDur(hold),
                count > 0 ? Math.Round(avg).ToString("0") : "0",
                Ui.FmtInt(keys)
            };
            string[] units = { "", "", "ms", "个" };

            for (int i = 0; i < 4; i++)
            {
                RectangleF r = new RectangleF(left + i * (cw + gap), top, cw, cardH);
                Ui.Card(g, r, true);
                Ui.DrawText(g, labels[i], Ui.F12, Ui.Text2,
                    new RectangleF(r.X + Ui.F(17), r.Y + Ui.F(14), r.Width - Ui.F(34), Ui.F(18)));
                float fs = 26;
                if (i == 1 && values[i].Length > 9) fs = 20;
                RectangleF vr = new RectangleF(r.X + Ui.F(17), r.Y + Ui.F(32), r.Width - Ui.F(34), Ui.F(38));
                Font vf = Ui.FontPx(fs, true);
                Ui.DrawText(g, values[i], vf, Ui.Text, vr);
                if (units[i].Length > 0)
                {
                    SizeF sz = g.MeasureString(values[i], vf);
                    Ui.DrawText(g, units[i], Ui.FontPx(12, false), Ui.Text2,
                        new RectangleF(vr.X + sz.Width - Ui.F(2), vr.Y + Ui.F(8), Ui.F(40), Ui.F(20)));
                }
                if (i == 0)
                {
                    string delta;
                    Color dc;
                    if (prev <= 0) { delta = "上一周期无数据"; dc = Ui.Text3; }
                    else
                    {
                        double pct = (count - prev) * 100.0 / prev;
                        delta = "较上一周期 " + (pct >= 0 ? "+" : "") + pct.ToString("0.0") + "%";
                        dc = Math.Abs(pct) < 0.05 ? Ui.Text3 : (pct > 0 ? Ui.Green : Ui.Red);
                    }
                    Ui.DrawText(g, delta, Ui.FontPx(11.5f, false), dc,
                        new RectangleF(r.X + Ui.F(17), r.Y + Ui.F(73), r.Width - Ui.F(34), Ui.F(16)));
                }
            }
            float y = top + cardH + gap;

            float chartH = Ui.F(300);
            RectangleF cc = new RectangleF(left, y, w, chartH);
            Ui.Card(g, cc, true);
            Ui.DrawText(g, "按小时趋势", Ui.F14B, Ui.Text,
                new RectangleF(cc.X + Ui.F(20), cc.Y + Ui.F(14), Ui.F(260), Ui.F(20)));
            Ui.DrawText(g, "按小时 · " + RangeSub(), Ui.F11, Ui.Text2,
                new RectangleF(cc.X + Ui.F(20), cc.Y + Ui.F(34), cc.Width - Ui.F(200), Ui.F(18)));
            if (snap != null && snap.Hourly.Count > 0)
            {
                HourPoint peak = null;
                double pv = 0;
                foreach (HourPoint p in snap.Hourly)
                {
                    double v = metric == Metric.Count ? p.Count : p.HoldMs;
                    if (v > pv) { pv = v; peak = p; }
                }
                if (peak != null && pv > 0)
                {
                    string chip = "峰值 " + Ui.FmtTime(peak.Hour) + " · " +
                        (metric == Metric.Count ? Ui.FmtInt(peak.Count) : Ui.FmtDurShort(peak.HoldMs));
                    float chipW = Ui.F(150);
                    RectangleF cr = new RectangleF(cc.Right - chipW - Ui.F(20), cc.Y + Ui.F(14), chipW, Ui.F(24));
                    Ui.FillRound(g, cr, Ui.F(12), Ui.AccentSoft);
                    Ui.DrawText(g, chip, Ui.F11, Ui.Accent2, cr, StringAlignment.Center);
                }
                DrawLineChart(g, new RectangleF(cc.X + Ui.F(20), cc.Y + Ui.F(56),
                    cc.Width - Ui.F(40), cc.Height - Ui.F(74)), sinceOf(), untilOf());
            }
            y += chartH + gap;

            float bottomH = Ui.F(318);
            float leftW = (w - gap) * 0.58f;
            RectangleF lc = new RectangleF(left, y, leftW, bottomH);
            Ui.Card(g, lc, true);
            Ui.DrawText(g, "键盘热力图", Ui.F14B, Ui.Text,
                new RectangleF(lc.X + Ui.F(20), lc.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, Ui.FmtInt(count) + " 次 · " + Ui.FmtDurShort(hold), Ui.F11, Ui.Text2,
                new RectangleF(lc.X + Ui.F(20), lc.Y + Ui.F(34), lc.Width - Ui.F(40), Ui.F(18)));
            Ui.DrawText(g, "放大查看", Ui.F11, Ui.Accent,
                new RectangleF(lc.Right - Ui.F(90), lc.Y + Ui.F(16), Ui.F(70), Ui.F(18)),
                StringAlignment.Far);
            AddHit(g, new RectangleF(lc.Right - Ui.F(100), lc.Y + Ui.F(8), Ui.F(90), Ui.F(30)), "nav:1");
            float kh = DrawKeyboard(g, new RectangleF(lc.X + Ui.F(20), lc.Y + Ui.F(58),
                lc.Width - Ui.F(40), Ui.F(200)), true, false, false);
            DrawLegend(g, new RectangleF(lc.X + Ui.F(20), lc.Y + Ui.F(66) + kh, lc.Width - Ui.F(40), Ui.F(16)),
                metric == Metric.Count ? MaxKey(Metric.Count) : MaxKey(Metric.Hold));

            float rx = left + leftW + gap;
            RectangleF rc = new RectangleF(rx, y, w - leftW - gap, bottomH);
            Ui.Card(g, rc, true);
            Ui.DrawText(g, "最常用按键", Ui.F14B, Ui.Text,
                new RectangleF(rc.X + Ui.F(20), rc.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, metric == Metric.Count ? "按次数排序" : "按按住时长排序", Ui.F11, Ui.Text2,
                new RectangleF(rc.X + Ui.F(20), rc.Y + Ui.F(34), Ui.F(200), Ui.F(18)));
            DrawTopList(g, new RectangleF(rc.X + Ui.F(20), rc.Y + Ui.F(60), rc.Width - Ui.F(40), rc.Height - Ui.F(76)));

            contentHeight = y + bottomH + Ui.F(40);
            ClampScroll();
        }

        private long sinceOf()
        {
            if (snap == null) return 0;
            if (snap.Since <= 0 && snap.Hourly.Count > 0) return snap.Hourly[0].Hour;
            return snap.Since;
        }

        private long untilOf()
        {
            return snap != null ? snap.Until : 0;
        }

        private double MaxKey(Metric m)
        {
            double max = 0;
            if (snap == null) return max;
            foreach (KeyStat k in snap.Keys)
            {
                double v = m == Metric.Count ? k.Count : k.HoldMs;
                if (v > max) max = v;
            }
            return max;
        }

        private static double HeatValue(KeyStat k, HeatMetric m)
        {
            if (k == null) return 0;
            if (m == HeatMetric.Count) return k.Count;
            if (m == HeatMetric.Hold) return k.HoldMs;
            return k.Count > 0 ? k.HoldMs / k.Count : 0;
        }

        private double HeatMax(HeatMetric m)
        {
            double max = 0;
            if (snap == null) return max;
            foreach (KeyStat k in snap.Keys)
            {
                double v = HeatValue(k, m);
                if (v > max) max = v;
            }
            return max;
        }

        private static string HeatName(HeatMetric m)
        {
            if (m == HeatMetric.Count) return "次数";
            if (m == HeatMetric.Hold) return "按住时长";
            return "平均按住";
        }

        private static string HeatMaxText(HeatMetric m, double v)
        {
            return m == HeatMetric.Count ? Ui.FmtInt((long)v) : Ui.FmtDurShort(v);
        }

        // 蓝色表示第一个指标高，红色表示第二个指标高，两个都高混成紫色。
        private static Color MixHeat(double tb, double tr)
        {
            double wB = tb * (1 - tr);
            double wR = tr * (1 - tb);
            double wP = Math.Min(tb, tr);
            double sum = wB + wR + wP;
            if (sum <= 0.0001) return HeatC[0];
            Color bg = HeatC[0];
            Color blue = Color.FromArgb(59, 130, 246);
            Color red = Color.FromArgb(225, 62, 62);
            Color purple = Color.FromArgb(139, 92, 246);
            double a = Math.Min(1.0, sum);
            double r = (wB * blue.R + wR * red.R + wP * purple.R) / sum;
            double gg = (wB * blue.G + wR * red.G + wP * purple.G) / sum;
            double b = (wB * blue.B + wR * red.B + wP * purple.B) / sum;
            return Color.FromArgb(
                (int)Math.Round(bg.R * (1 - a) + r * a),
                (int)Math.Round(bg.G * (1 - a) + gg * a),
                (int)Math.Round(bg.B * (1 - a) + b * a));
        }

        private static bool Dark(Color c)
        {
            double l = (c.R * 0.299 + c.G * 0.587 + c.B * 0.114) / 255.0;
            return l < 0.66;
        }

        private void DrawTopList(Graphics g, RectangleF r)
        {
            if (snap == null || snap.Keys.Count == 0)
            {
                Ui.DrawText(g, "暂无数据", Ui.F12, Ui.Text3, new RectangleF(r.X, r.Y, r.Width, Ui.F(30)));
                return;
            }
            List<KeyStat> list = new List<KeyStat>(snap.Keys);
            list.Sort(delegate (KeyStat a, KeyStat b)
            {
                if (metric == Metric.Count) return b.Count.CompareTo(a.Count);
                return b.HoldMs.CompareTo(a.HoldMs);
            });
            double max = 1;
            int n = Math.Min(8, list.Count);
            for (int i = 0; i < n; i++)
            {
                double v = metric == Metric.Count ? list[i].Count : list[i].HoldMs;
                if (v > max) max = v;
            }
            float rowH = Ui.F(30);
            for (int i = 0; i < n; i++)
            {
                KeyStat k = list[i];
                float y = r.Y + i * rowH;
                Ui.DrawText(g, (i + 1).ToString(), Ui.F10, Ui.Text3,
                    new RectangleF(r.X, y, Ui.F(18), rowH));
                Ui.DrawText(g, k.Name, Ui.F12, Ui.Text,
                    new RectangleF(r.X + Ui.F(22), y, Ui.F(90), rowH));
                double v = metric == Metric.Count ? k.Count : k.HoldMs;
                float bx = r.X + Ui.F(118);
                float bw = r.Width - Ui.F(118) - Ui.F(70);
                Ui.FillRound(g, new RectangleF(bx, y + rowH / 2 - Ui.F(3), bw, Ui.F(6)), Ui.F(3), Ui.Track);
                Ui.FillRound(g, new RectangleF(bx, y + rowH / 2 - Ui.F(3),
                    Math.Max(Ui.F(2), bw * (float)(v / max)), Ui.F(6)), Ui.F(3), Ui.Accent);
                Ui.DrawText(g, Ui.FmtCompact(v), Ui.F11, Ui.Text2,
                    new RectangleF(r.Right - Ui.F(66), y, Ui.F(66), rowH), StringAlignment.Far);
            }
        }

        private void DrawLegend(Graphics g, RectangleF r, double max)
        {
            float barW = r.Width - Ui.F(130);
            RectangleF bar = new RectangleF(r.X + Ui.F(24), r.Y + Ui.F(4), barW, Ui.F(7));
            using (LinearGradientBrush b = new LinearGradientBrush(bar,
                Color.FromArgb(238, 241, 245), Ui.Accent2, 0f))
            {
                ColorBlend cb = new ColorBlend();
                cb.Colors = new Color[] { HeatC[0], HeatC[1], HeatC[2], HeatC[3], HeatC[4] };
                cb.Positions = HeatX;
                b.InterpolationColors = cb;
                using (GraphicsPath p = Ui.RoundPath(bar, Ui.F(4)))
                    g.FillPath(b, p);
            }
            Ui.DrawText(g, "0", Ui.FontPx(10.5f, false), Ui.Text3,
                new RectangleF(r.X, r.Y, Ui.F(20), Ui.F(16)));
            string label = "最多 " + (metric == Metric.Count ? Ui.FmtInt((long)max) : Ui.FmtDurShort(max));
            Ui.DrawText(g, label, Ui.FontPx(10.5f, false), Ui.Text3,
                new RectangleF(r.Right - Ui.F(120), r.Y, Ui.F(120), Ui.F(16)), StringAlignment.Far);
        }

        // ---------------------------------------------------------- 热力图

        private void DrawMixChips(Graphics g, RectangleF card, float pad)
        {
            float w = Ui.F(144), h = Ui.F(32), gap = Ui.F(8);
            float x = card.Right - pad - w;
            mixRedRect = new RectangleF(x, card.Y + Ui.F(10), w, h);
            mixBlueRect = new RectangleF(x - gap - w, card.Y + Ui.F(10), w, h);
            DrawMixChip(g, mixBlueRect, Color.FromArgb(59, 130, 246), "蓝", HeatName(mixBlue), mixMenu == 1);
            DrawMixChip(g, mixRedRect, Color.FromArgb(225, 62, 62), "红", HeatName(mixRed), mixMenu == 2);
            AddHit(mixBlueRect, "mixb");
            AddHit(mixRedRect, "mixr");
        }

        private void DrawMixChip(Graphics g, RectangleF r, Color dot, string tag, string val, bool open)
        {
            Ui.FillRound(g, r, Ui.F(8), open ? Color.White : Color.FromArgb(244, 245, 248));
            Ui.StrokeRound(g, r, Ui.F(8), open ? Ui.Accent : Ui.Line, 1);
            using (SolidBrush b = new SolidBrush(dot))
                g.FillEllipse(b, r.X + Ui.F(11), r.Y + r.Height / 2 - Ui.F(4), Ui.F(8), Ui.F(8));
            Ui.DrawText(g, tag + " · " + val, Ui.F11, Ui.Text,
                new RectangleF(r.X + Ui.F(24), r.Y, r.Width - Ui.F(46), r.Height));
            using (Pen p = new Pen(Ui.Text3, Ui.F(1.4f)))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                float cx = r.Right - Ui.F(13), cy = r.Y + r.Height / 2;
                g.DrawLine(p, cx - Ui.F(3), cy - Ui.F(2), cx, cy + Ui.F(1));
                g.DrawLine(p, cx, cy + Ui.F(1), cx + Ui.F(3), cy - Ui.F(2));
            }
        }

        private void DrawMixLegend(Graphics g, RectangleF r)
        {
            float x = r.X, y = r.Y + Ui.F(8);
            x = DrawSwatch(g, x, y, Color.FromArgb(59, 130, 246), "蓝 " + HeatName(mixBlue), Ui.F(104));
            x = DrawSwatch(g, x, y, Color.FromArgb(225, 62, 62), "红 " + HeatName(mixRed), Ui.F(104));
            DrawSwatch(g, x, y, Color.FromArgb(139, 92, 246), "都高", Ui.F(60));
            Ui.DrawText(g, "蓝/红各自按最大值归一", Ui.FontPx(10.5f, false), Ui.Text3,
                new RectangleF(r.Right - Ui.F(150), r.Y, Ui.F(150), Ui.F(16)), StringAlignment.Far);
        }

        private float DrawSwatch(Graphics g, float x, float y, Color c, string label, float slot)
        {
            Ui.FillRound(g, new RectangleF(x, y - Ui.F(4), Ui.F(9), Ui.F(9)), Ui.F(2), c);
            Ui.DrawText(g, label, Ui.FontPx(10.5f, false), Ui.Text2,
                new RectangleF(x + Ui.F(14), y - Ui.F(9), slot - Ui.F(14), Ui.F(16)));
            return x + slot;
        }

        private void DrawHeatmap(Graphics g)
        {
            float left = ContentX, top = ContentY, w = ContentW;
            float pad = Ui.F(20);
            float u = Math.Min(Ui.F(48), (w - pad * 2) / 23.3f);
            float kbH = u * 6;
            float cardH = Ui.F(56) + kbH + Ui.F(46);
            RectangleF card = new RectangleF(left, top, w, cardH);
            Ui.Card(g, card, true);
            Ui.DrawText(g, "键盘热力图", Ui.F14B, Ui.Text,
                new RectangleF(card.X + pad, card.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            if (heatMix) DrawMixChips(g, card, pad);
            string sub = (snap != null ? Ui.FmtInt(snap.TotalCount) + " 次 · " + Ui.FmtDur(snap.TotalHoldMs) : "")
                + " · " + RangeSub();
            float subW = card.Width - pad * 2 - (heatMix ? Ui.F(324) : 0);
            Ui.DrawText(g, sub, Ui.F11, Ui.Text2,
                new RectangleF(card.X + pad, card.Y + Ui.F(34), Math.Max(Ui.F(160), subW), Ui.F(18)));
            float kh = DrawKeyboard(g, new RectangleF(card.X + pad, card.Y + Ui.F(52),
                w - pad * 2, kbH), false, true, heatMix);
            if (heatMix)
                DrawMixLegend(g, new RectangleF(card.X + pad, card.Y + Ui.F(58) + kh,
                    card.Width - pad * 2, Ui.F(16)));
            else
                DrawLegend(g, new RectangleF(card.X + pad, card.Y + Ui.F(58) + kh,
                    card.Width - pad * 2, Ui.F(16)),
                    metric == Metric.Count ? MaxKey(Metric.Count) : MaxKey(Metric.Hold));

            float y = top + cardH + Gap;
            List<KeyStat> keys = SortedKeys();
            float rowH = Ui.F(34);
            float tableH = Ui.F(50) + Ui.F(30) + keys.Count * rowH + Ui.F(24);
            RectangleF tc = new RectangleF(left, y, w, tableH);
            Ui.Card(g, tc, true);
            Ui.DrawText(g, "按键明细", Ui.F14B, Ui.Text,
                new RectangleF(tc.X + pad, tc.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, "点击表头排序 · " + keys.Count + " 个按键", Ui.F11, Ui.Text2,
                new RectangleF(tc.X + pad, tc.Y + Ui.F(34), Ui.F(300), Ui.F(18)));

            float tx = tc.X + pad;
            float ty = tc.Y + Ui.F(58);
            float tw = tc.Width - pad * 2;
            float[] cw = { tw - Ui.F(560), Ui.F(130), Ui.F(170), Ui.F(130), Ui.F(130) };
            string[] heads = { "按键", "次数", "按住总时长", "平均按住", "次数占比" };
            float hx = tx;
            for (int i = 0; i < 5; i++)
            {
                bool active = sortCol == i;
                RectangleF hr = new RectangleF(hx, ty, cw[i], Ui.F(30));
                Color hc = active ? Ui.Accent : Ui.Text3;
                string text = heads[i] + (active ? (sortDesc ? " ↓" : " ↑") : "");
                if (i == 0)
                    Ui.DrawText(g, text, Ui.F11, hc,
                        new RectangleF(hr.X + Ui.F(10), hr.Y, hr.Width - Ui.F(10), hr.Height));
                else
                    Ui.DrawText(g, text, Ui.F11, hc,
                        new RectangleF(hr.X, hr.Y, hr.Width - Ui.F(10), hr.Height),
                        StringAlignment.Far);
                AddSortHit(hr, i);
                hx += cw[i];
            }
            Ui.DrawLine(g, tx, ty + Ui.F(30), tx + tw, ty + Ui.F(30), Ui.Line, 1);

            if (keys.Count == 0)
            {
                Ui.DrawText(g, "暂无数据", Ui.F12, Ui.Text3,
                    new RectangleF(tx + Ui.F(10), ty + Ui.F(40), tw, Ui.F(30)));
            }
            else
            {
                float viewTop = ty + Ui.F(30);
                float first = Math.Max(0, (scroll + Ui.F(TopbarH)) - viewTop);
                int i0 = (int)(first / rowH);
                int i1 = Math.Min(keys.Count, i0 + (int)(ViewportHeight() / rowH) + 2);
                for (int i = i0; i < i1; i++)
                {
                    KeyStat k = keys[i];
                    float ry = viewTop + i * rowH;
                    Ui.DrawLine(g, tx, ry + rowH, tx + tw, ry + rowH, Ui.LineSoft, 1);
                    Ui.DrawText(g, k.Name, Ui.F12, Ui.Text,
                        new RectangleF(tx + Ui.F(10), ry, cw[0] - Ui.F(10), rowH));
                    Ui.DrawText(g, Ui.FmtInt(k.Count), Ui.F12, Ui.Text,
                        new RectangleF(tx + cw[0], ry, cw[1] - Ui.F(10), rowH), StringAlignment.Far);
                    Ui.DrawText(g, Ui.FmtDur(k.HoldMs), Ui.F12, Ui.Text,
                        new RectangleF(tx + cw[0] + cw[1], ry, cw[2] - Ui.F(10), rowH), StringAlignment.Far);
                    Ui.DrawText(g, k.Count > 0 ? Math.Round(k.Avg).ToString("0") + " ms" : "—",
                        Ui.F12, Ui.Text,
                        new RectangleF(tx + cw[0] + cw[1] + cw[2], ry, cw[3] - Ui.F(10), rowH),
                        StringAlignment.Far);
                    double share = snap != null && snap.TotalCount > 0 ? k.Count * 100.0 / snap.TotalCount : 0;
                    Ui.DrawText(g, share.ToString("0.00") + "%", Ui.F12, Ui.Text,
                        new RectangleF(tx + cw[0] + cw[1] + cw[2] + cw[3], ry, cw[4] - Ui.F(10), rowH),
                        StringAlignment.Far);
                }
            }
            contentHeight = y + tableH + Ui.F(40);
            ClampScroll();
        }

        private void AddSortHit(RectangleF r, int col)
        {
            Hit h = new Hit();
            h.Rect = r;
            h.Action = "sort";
            h.Vk = col;
            hits.Add(h);
        }

        private List<KeyStat> SortedKeys()
        {
            List<KeyStat> list = new List<KeyStat>();
            if (snap == null) return list;
            list.AddRange(snap.Keys);
            list.Sort(delegate (KeyStat a, KeyStat b)
            {
                int c;
                if (sortCol == 0) c = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                else if (sortCol == 1) c = a.Count.CompareTo(b.Count);
                else if (sortCol == 2) c = a.HoldMs.CompareTo(b.HoldMs);
                else if (sortCol == 3) c = a.Avg.CompareTo(b.Avg);
                else c = a.Count.CompareTo(b.Count);
                return sortDesc ? -c : c;
            });
            return list;
        }

        // ---------------------------------------------------------- 趋势

        private void DrawTrends(Graphics g)
        {
            float left = ContentX, top = ContentY, w = ContentW;
            RectangleF card = new RectangleF(left, top, w, Ui.F(370));
            Ui.Card(g, card, true);
            Ui.DrawText(g, "按小时趋势", Ui.F14B, Ui.Text,
                new RectangleF(card.X + Ui.F(20), card.Y + Ui.F(14), Ui.F(240), Ui.F(20)));
            Ui.DrawText(g, "按小时 · " + RangeSub(), Ui.F11, Ui.Text2,
                new RectangleF(card.X + Ui.F(20), card.Y + Ui.F(34), card.Width - Ui.F(200), Ui.F(18)));
            if (snap != null && snap.Hourly.Count > 0)
                DrawLineChart(g, new RectangleF(card.X + Ui.F(20), card.Y + Ui.F(56),
                    card.Width - Ui.F(40), card.Height - Ui.F(76)), sinceOf(), untilOf());

            float y = top + Ui.F(370) + Gap;
            float half = (w - Gap) / 2;
            DrawBarCard(g, new RectangleF(left, y, half, Ui.F(250)), 0);
            DrawBarCard(g, new RectangleF(left + half + Gap, y, half, Ui.F(250)), 1);
            contentHeight = y + Ui.F(250) + Ui.F(40);
            ClampScroll();
        }

        private void DrawBarCard(Graphics g, RectangleF r, int mode)
        {
            Ui.Card(g, r, true);
            Ui.DrawText(g, mode == 0 ? "时段分布" : "星期分布", Ui.F14B, Ui.Text,
                new RectangleF(r.X + Ui.F(20), r.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, mode == 0 ? "按一天中的 0–23 时汇总" : "按周一到周日汇总", Ui.F11, Ui.Text2,
                new RectangleF(r.X + Ui.F(20), r.Y + Ui.F(34), r.Width - Ui.F(40), Ui.F(18)));
            if (snap == null) return;
            double[] vals;
            string[] labels;
            if (mode == 0)
            {
                vals = new double[24];
                labels = new string[24];
                for (int i = 0; i < 24; i++) labels[i] = i.ToString();
                foreach (HourPoint p in snap.Hourly)
                {
                    DateTime t = DateTimeOffset.FromUnixTimeSeconds(p.Hour).ToLocalTime().DateTime;
                    vals[t.Hour] += metric == Metric.Count ? p.Count : p.HoldMs;
                }
            }
            else
            {
                vals = new double[7];
                labels = new string[] { "一", "二", "三", "四", "五", "六", "日" };
                foreach (HourPoint p in snap.Hourly)
                {
                    DateTime t = DateTimeOffset.FromUnixTimeSeconds(p.Hour).ToLocalTime().DateTime;
                    int idx = ((int)t.DayOfWeek + 6) % 7;
                    vals[idx] += metric == Metric.Count ? p.Count : p.HoldMs;
                }
            }
            DrawBars(g, new RectangleF(r.X + Ui.F(20), r.Y + Ui.F(58), r.Width - Ui.F(40), r.Height - Ui.F(80)),
                vals, labels);
        }

        // ---------------------------------------------------------- 设置

        private void DrawSettings(Graphics g)
        {
            float left = ContentX, top = ContentY, w = ContentW;
            float pad = Ui.F(20);

            float groupsH = DrawGroupsCard(g);
            top += groupsH + Gap;

            float h1 = Ui.F(226);
            RectangleF c1 = new RectangleF(left, top, w, h1);
            Ui.Card(g, c1, true);
            Ui.DrawText(g, "常规", Ui.F14B, Ui.Text,
                new RectangleF(c1.X + pad, c1.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, "启动与访问", Ui.F11, Ui.Text2,
                new RectangleF(c1.X + pad, c1.Y + Ui.F(34), Ui.F(200), Ui.F(18)));

            float ry = c1.Y + Ui.F(66);
            Ui.DrawText(g, "开机自启动", Ui.F13, Ui.Text,
                new RectangleF(c1.X + pad, ry, Ui.F(240), Ui.F(20)));
            Ui.DrawText(g, "登录 Windows 后自动在后台记录，不弹出窗口", Ui.F11, Ui.Text2,
                new RectangleF(c1.X + pad, ry + Ui.F(19), Ui.F(520), Ui.F(18)));
            DrawToggle(g, new RectangleF(c1.Right - pad - Ui.F(40), ry + Ui.F(4), Ui.F(40), Ui.F(22)),
                GetAutostart());
            AddHit(g, new RectangleF(c1.Right - pad - Ui.F(60), ry - Ui.F(6), Ui.F(80), Ui.F(46)), "autostart");
            Ui.DrawLine(g, c1.X + pad, ry + Ui.F(48), c1.Right - pad, ry + Ui.F(48), Ui.LineSoft, 1);

            ry += Ui.F(62);
            Ui.DrawText(g, "数据目录", Ui.F13, Ui.Text,
                new RectangleF(c1.X + pad, ry, Ui.F(240), Ui.F(20)));
            Ui.DrawText(g, store.Dir, Ui.F11, Ui.Text2,
                new RectangleF(c1.X + pad, ry + Ui.F(19), c1.Width - pad * 2 - Ui.F(100), Ui.F(18)));
            DrawButton(g, new RectangleF(c1.Right - pad - Ui.F(76), ry - Ui.F(2), Ui.F(76), Ui.F(30)),
                "打开", false, "open");

            float h2 = Ui.F(246);
            RectangleF c2 = new RectangleF(left, top + h1 + Gap, w, h2);
            Ui.Card(g, c2, true);
            Ui.DrawText(g, "数据", Ui.F14B, Ui.Text,
                new RectangleF(c2.X + pad, c2.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, "只影响当前分组「" + groups.ActiveName + "」", Ui.F11, Ui.Text2,
                new RectangleF(c2.X + pad, c2.Y + Ui.F(34), Ui.F(400), Ui.F(18)));

            float kw = (c2.Width - pad * 2 - Ui.F(36)) / 4;
            string[] kl = { "文件大小", "聚合行数", "覆盖天数", "最早记录" };
            string[] kv = { Ui.FmtBytes(store.SizeBytes), Ui.FmtInt(store.Rows),
                DaysText(), store.Rows > 0 ? Ui.FmtDate(store.First) : "—" };
            for (int i = 0; i < 4; i++)
            {
                RectangleF kr = new RectangleF(c2.X + pad + i * (kw + Ui.F(12)), c2.Y + Ui.F(62), kw, Ui.F(70));
                Ui.FillRound(g, kr, Ui.F(10), Color.FromArgb(250, 251, 252));
                Ui.StrokeRound(g, kr, Ui.F(10), Ui.LineSoft, 1);
                Ui.DrawText(g, kl[i], Ui.F10, Ui.Text3,
                    new RectangleF(kr.X + Ui.F(12), kr.Y + Ui.F(10), kr.Width - Ui.F(24), Ui.F(16)));
                Ui.DrawText(g, kv[i], Ui.F15B, Ui.Text,
                    new RectangleF(kr.X + Ui.F(12), kr.Y + Ui.F(30), kr.Width - Ui.F(24), Ui.F(26)));
            }
            Ui.DrawText(g, "清空只删除当前分组的记录，文件会立即删除并释放空间；其他分组不受影响。",
                Ui.F10, Ui.Text3, new RectangleF(c2.X + pad, c2.Y + Ui.F(144), c2.Width - pad * 2, Ui.F(18)));
            DrawButton(g, new RectangleF(c2.X + pad, c2.Y + Ui.F(172), Ui.F(118), Ui.F(34)),
                "导出 CSV", false, "export");
            DrawButton(g, new RectangleF(c2.X + pad + Ui.F(130), c2.Y + Ui.F(172), Ui.F(108), Ui.F(34)),
                "清空记录", true, "reset");

            float h3 = Ui.F(178);
            RectangleF c3 = new RectangleF(left, top + h1 + h2 + Gap * 2, w, h3);
            Ui.Card(g, c3, true);
            Ui.DrawText(g, "关于", Ui.F14B, Ui.Text,
                new RectangleF(c3.X + pad, c3.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, "Keytrace 只在本机记录物理按键的键码、次数与按住时长；不记录输入内容、不联网、不上传。",
                Ui.F11, Ui.Text2,
                new RectangleF(c3.X + pad, c3.Y + Ui.F(44), c3.Width - pad * 2, Ui.F(40)),
                StringAlignment.Near, StringAlignment.Near, true);
            Ui.DrawText(g, "版本 1.1.0 · 原生 WinForms · 已运行 " + UptimeText(), Ui.F10, Ui.Text3,
                new RectangleF(c3.X + pad, c3.Y + Ui.F(96), c3.Width - pad * 2, Ui.F(18)));
            DrawButton(g, new RectangleF(c3.X + pad, c3.Y + Ui.F(122), Ui.F(118), Ui.F(34)),
                "退出记录器", false, "quit");

            contentHeight = top + h1 + h2 + h3 + Gap * 2 + Ui.F(40);
            ClampScroll();
        }

        private float DrawGroupsCard(Graphics g)
        {
            float left = ContentX, top = ContentY, w = ContentW;
            float pad = Ui.F(20);
            int n = groups.Count;
            float rowH = Ui.F(46);
            float headH = Ui.F(62);
            float h = headH + n * rowH + Ui.F(50);
            RectangleF c = new RectangleF(left, top, w, h);
            Ui.Card(g, c, true);

            Ui.DrawText(g, "分组", Ui.F14B, Ui.Text,
                new RectangleF(c.X + pad, c.Y + Ui.F(14), Ui.F(200), Ui.F(20)));
            Ui.DrawText(g, "为不同使用者分开记录；切换后，新的按键计入所选分组",
                Ui.F11, Ui.Text2,
                new RectangleF(c.X + pad, c.Y + Ui.F(34), c.Width - pad * 2 - Ui.F(120), Ui.F(18)));
            DrawButton(g, new RectangleF(c.Right - pad - Ui.F(96), c.Y + Ui.F(16), Ui.F(96), Ui.F(32)),
                "新建分组", false, "gnew");

            for (int i = 0; i < n; i++)
            {
                Group grp = groups.All[i];
                bool act = grp.Id == groups.ActiveId;
                float ry = c.Y + headH + i * rowH;
                RectangleF row = new RectangleF(c.X + Ui.F(10), ry + 1, c.Width - Ui.F(20), rowH - 2);

                RectangleF av = new RectangleF(row.X + Ui.F(10), ry + Ui.F(8), Ui.F(30), Ui.F(30));
                Ui.FillRound(g, av, Ui.F(9), act ? Ui.AccentSoft : Color.FromArgb(244, 245, 248));
                string ch = grp.Name.Length > 0 ? grp.Name.Substring(0, 1) : "G";
                Ui.DrawText(g, ch, Ui.FontPx(12.5f, true), act ? Ui.Accent : Ui.Text2, av,
                    StringAlignment.Center, StringAlignment.Center);

                float nx = av.Right + Ui.F(12);
                SizeF ns = g.MeasureString(grp.Name, Ui.F13);
                Ui.DrawText(g, grp.Name, Ui.F13, Ui.Text,
                    new RectangleF(nx, ry + Ui.F(7), Math.Max(Ui.F(40), ns.Width + Ui.F(4)), Ui.F(20)));
                if (act)
                {
                    RectangleF tag = new RectangleF(nx + ns.Width + Ui.F(8), ry + Ui.F(13), Ui.F(38), Ui.F(19));
                    Ui.FillRound(g, tag, Ui.F(6), Ui.AccentSoft);
                    Ui.DrawText(g, "当前", Ui.FontPx(9.5f, false), Ui.Accent, tag,
                        StringAlignment.Center, StringAlignment.Center);
                }

                string meta;
                if (act) meta = Ui.FmtBytes(groups.GroupBytes(grp)) + " · " + Ui.FmtInt(store.Rows) + " 行";
                else meta = Ui.FmtBytes(groups.GroupBytes(grp));
                Ui.DrawText(g, meta, Ui.F10, Ui.Text3,
                    new RectangleF(nx, ry + Ui.F(27), row.Width - Ui.F(90), Ui.F(16)));

                if (!act) AddHit(row, "gswitch:" + grp.Id);

                float bx = row.Right - Ui.F(8);
                if (n > 1)
                {
                    RectangleF del = new RectangleF(bx - Ui.F(54), ry + Ui.F(9), Ui.F(54), Ui.F(28));
                    bx = del.X - Ui.F(8);
                    DrawButton(g, del, "删除", true, "gdelete:" + grp.Id);
                }
                RectangleF ren = new RectangleF(bx - Ui.F(64), ry + Ui.F(9), Ui.F(64), Ui.F(28));
                bx = ren.X - Ui.F(8);
                DrawButton(g, ren, "重命名", false, "grename:" + grp.Id);
                if (!act)
                    DrawButton(g, new RectangleF(bx - Ui.F(56), ry + Ui.F(9), Ui.F(56), Ui.F(28)),
                        "切换", false, "gswitch:" + grp.Id);
                else
                    Ui.DrawText(g, "正在记录", Ui.FontPx(10.5f, false), Ui.Text3,
                        new RectangleF(bx - Ui.F(86), ry + Ui.F(9), Ui.F(82), Ui.F(28)), StringAlignment.Far);

                if (i < n - 1)
                    Ui.DrawLine(g, c.X + pad, ry + rowH - 1, c.Right - pad, ry + rowH - 1, Ui.LineSoft, 1);
            }

            Ui.DrawText(g, "点击分组行或“切换”立即生效；托盘菜单也能直接切换。", Ui.F10, Ui.Text3,
                new RectangleF(c.X + pad, c.Bottom - Ui.F(30), c.Width - pad * 2, Ui.F(18)));
            return h;
        }

        private string DaysText()
        {
            long first = store.First, last = store.Last;
            if (store.Rows == 0 || first == 0) return "—";
            long days = (last - first) / 86400 + 1;
            return days + " 天";
        }

        private string UptimeText()
        {
            TimeSpan t = DateTime.Now - started;
            if (t.TotalHours >= 24) return ((int)t.TotalDays) + " 天 " + t.Hours + " 小时";
            if (t.TotalHours >= 1) return ((int)t.TotalHours) + " 小时 " + t.Minutes + " 分";
            return Math.Max(1, (int)t.TotalMinutes) + " 分钟";
        }

        private void DrawToggle(Graphics g, RectangleF r, bool on)
        {
            Ui.FillRound(g, r, r.Height / 2, on ? Ui.Accent : Color.FromArgb(215, 218, 224));
            float d = r.Height - Ui.F(4);
            float x = on ? r.Right - d - Ui.F(2) : r.X + Ui.F(2);
            using (SolidBrush b = new SolidBrush(Color.White))
                g.FillEllipse(b, x, r.Y + Ui.F(2), d, d);
        }

        private void DrawButton(Graphics g, RectangleF r, string text, bool danger, string action)
        {
            Ui.FillRound(g, r, Ui.F(8), Color.White);
            Ui.StrokeRound(g, r, Ui.F(8), danger ? Color.FromArgb(240, 211, 211) : Ui.Line, 1);
            Ui.DrawText(g, text, Ui.F12, danger ? Ui.Red : Ui.Text, r,
                StringAlignment.Center, StringAlignment.Center);
            AddHit(g, r, action);
        }

        // ---------------------------------------------------------- 图表

        private static double NiceMax(double v)
        {
            if (v <= 0) return 1;
            double exp = Math.Floor(Math.Log10(v));
            double b = Math.Pow(10, exp);
            double n = v / b;
            double[] ladder = { 1, 1.2, 1.5, 2, 2.5, 3, 4, 5, 6, 8, 10 };
            double m = 10;
            for (int i = 0; i < ladder.Length; i++)
                if (n <= ladder[i]) { m = ladder[i]; break; }
            return m * b;
        }

        private string AxisLabel(double v)
        {
            return metric == Metric.Count ? Ui.FmtCompact(v) : Ui.FmtDurShort(v);
        }

        private void DrawLineChart(Graphics g, RectangleF r, long since, long until)
        {
            if (snap == null || snap.Hourly.Count == 0 || r.Width <= 10) return;
            List<HourPoint> pts = snap.Hourly;
            int step = Math.Max(1, (int)Math.Ceiling(pts.Count / 2000.0));
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            for (int i = 0; i < pts.Count; i += step)
            {
                double sum = 0;
                for (int k = 0; k < step && i + k < pts.Count; k++)
                    sum += metric == Metric.Count ? pts[i + k].Count : pts[i + k].HoldMs;
                xs.Add(pts[i].Hour);
                ys.Add(sum);
            }
            double ymax = NiceMax(1);
            for (int i = 0; i < ys.Count; i++) if (ys[i] > ymax) ymax = NiceMax(ys[i]);
            if (ymax < 4) ymax = 4;

            float padL = Ui.F(52), padR = Ui.F(12), padT = Ui.F(10), padB = Ui.F(26);
            float iw = r.Width - padL - padR;
            float ih = r.Height - padT - padB;
            if (iw <= 10 || ih <= 10) return;

            for (int i = 0; i <= 4; i++)
            {
                float y = r.Y + padT + ih * i / 4;
                Ui.DrawLine(g, r.X + padL, y, r.X + r.Width - padR, y,
                    i == 4 ? Color.FromArgb(228, 230, 236) : Color.FromArgb(242, 243, 246), 1);
                Ui.DrawText(g, AxisLabel(ymax * (1 - i / 4.0)), Ui.FontPx(10.5f, false), Ui.Text3,
                    new RectangleF(r.X + Ui.F(6), y - Ui.F(9), Ui.F(40), Ui.F(18)), StringAlignment.Far);
            }

            double span = Math.Max(1, until - since);
            int ticks = 6;
            for (int i = 0; i < ticks; i++)
            {
                long t = since + (long)(span * i / (ticks - 1));
                float x = r.X + padL + iw * i / (ticks - 1);
                string lb;
                if (span <= 36 * 3600) lb = Ui.FmtTime(t).Substring(0, 2) + ":00";
                else
                {
                    DateTime dt = DateTimeOffset.FromUnixTimeSeconds(t).ToLocalTime().DateTime;
                    lb = span <= 8 * 86400 ? dt.ToString("M/d") : dt.ToString("yy/M/d");
                }
                Ui.DrawText(g, lb, Ui.FontPx(10.5f, false), Ui.Text3,
                    new RectangleF(x - Ui.F(30), r.Bottom - Ui.F(22), Ui.F(60), Ui.F(16)),
                    StringAlignment.Center);
            }

            float baseY = r.Y + padT + ih;
            PointF[] line = new PointF[xs.Count];
            for (int i = 0; i < xs.Count; i++)
            {
                float x = r.X + padL + (float)((xs[i] - since) / span) * iw;
                float y = baseY - (float)(ys[i] / ymax) * ih;
                line[i] = new PointF(x, y);
            }
            if (line.Length >= 2)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddLines(line);
                    PointF[] fill = new PointF[line.Length + 2];
                    Array.Copy(line, fill, line.Length);
                    fill[line.Length] = new PointF(line[line.Length - 1].X, baseY);
                    fill[line.Length + 1] = new PointF(line[0].X, baseY);
                    using (GraphicsPath fp = new GraphicsPath())
                    {
                        fp.AddPolygon(fill);
                        using (LinearGradientBrush b = new LinearGradientBrush(
                            new RectangleF(r.X, r.Y, r.Width, r.Height),
                            Color.FromArgb(42, 47, 111, 235), Color.FromArgb(2, 47, 111, 235), 90f))
                            g.FillPath(b, fp);
                    }
                    using (Pen p = new Pen(Ui.Accent, Ui.F(2)))
                    {
                        p.LineJoin = LineJoin.Round;
                        g.DrawPath(p, path);
                    }
                }
            }
        }

        private void DrawBars(Graphics g, RectangleF r, double[] vals, string[] labels)
        {
            double ymax = NiceMax(1);
            for (int i = 0; i < vals.Length; i++) if (vals[i] > ymax) ymax = NiceMax(vals[i]);
            float padL = Ui.F(42), padR = Ui.F(8), padT = Ui.F(8), padB = Ui.F(22);
            float iw = r.Width - padL - padR;
            float ih = r.Height - padT - padB;
            if (iw <= 10 || ih <= 10) return;
            for (int i = 0; i <= 3; i++)
            {
                float y = r.Y + padT + ih * i / 3;
                Ui.DrawLine(g, r.X + padL, y, r.X + r.Width - padR, y,
                    i == 3 ? Color.FromArgb(228, 230, 236) : Color.FromArgb(242, 243, 246), 1);
                Ui.DrawText(g, AxisLabel(ymax * (1 - i / 3.0)), Ui.FontPx(10.5f, false), Ui.Text3,
                    new RectangleF(r.X + Ui.F(4), y - Ui.F(9), Ui.F(34), Ui.F(18)), StringAlignment.Far);
            }
            int n = vals.Length;
            float step = iw / n;
            float bw = Math.Min(Ui.F(26), step * 0.58f);
            for (int i = 0; i < n; i++)
            {
                float x = r.X + padL + step * (i + 0.5f);
                float bh = (float)(vals[i] / ymax) * ih;
                if (vals[i] > 0 && bh < Ui.F(2)) bh = Ui.F(2);
                if (bh > 0.5f)
                    Ui.FillRound(g, new RectangleF(x - bw / 2, r.Y + padT + ih - bh, bw, bh),
                        Ui.F(3), Color.FromArgb(91, 147, 242));
                if (n <= 8 || i % (int)Math.Ceiling(n / 8.0) == 0)
                    Ui.DrawText(g, labels[i], Ui.FontPx(10.5f, false), Ui.Text3,
                        new RectangleF(x - Ui.F(24), r.Bottom - Ui.F(20), Ui.F(48), Ui.F(16)),
                        StringAlignment.Center);
            }
        }

        // ---------------------------------------------------------- 键盘

        private float DrawKeyboard(Graphics g, RectangleF area, bool mini, bool showValues, bool mix)
        {
            float u = Math.Min(mini ? Ui.F(28) : Ui.F(48), area.Width / 23.3f);
            if (u <= 0) return 0;
            Dictionary<int, KeyStat> byVk = ByVk();
            double max = metric == Metric.Count ? MaxKey(Metric.Count) : MaxKey(Metric.Hold);
            double maxBlue = HeatMax(mixBlue);
            double maxRed = HeatMax(mixRed);
            float h = u * 6;
            using (Pen border = new Pen(Color.FromArgb(236, 238, 242), 1))
            {
                for (int i = 0; i < caps.Count; i++)
                {
                    KeyCap k = caps[i];
                    RectangleF r = new RectangleF(area.X + k.X * u + Ui.F(2),
                        area.Y + k.Y * u + Ui.F(2), k.W * u - Ui.F(4), k.H * u - Ui.F(4));
                    KeyStat st;
                    byVk.TryGetValue(k.Vk, out st);
                    double raw = st == null ? 0 : (metric == Metric.Count ? st.Count : st.HoldMs);
                    double t = max > 0 ? Math.Pow(raw / max, 0.55) : 0;
                    double tb = 0, tr = 0;
                    if (mix)
                    {
                        double vb = HeatValue(st, mixBlue), vr = HeatValue(st, mixRed);
                        tb = maxBlue > 0 ? Math.Pow(vb / maxBlue, 0.55) : 0;
                        tr = maxRed > 0 ? Math.Pow(vr / maxRed, 0.55) : 0;
                    }
                    Color bg = mix ? MixHeat(tb, tr) : Heat(t);
                    Color fg;
                    if (mix) fg = Dark(bg) ? Color.White : Color.FromArgb(71, 85, 105);
                    else fg = t > 0.58 ? Color.White : (t > 0.28 ? Color.FromArgb(30, 58, 138) : Color.FromArgb(100, 116, 139));
                    Ui.FillRound(g, r, Ui.F(5), bg);
                    using (GraphicsPath path = Ui.RoundPath(r, Ui.F(5)))
                        g.DrawPath(border, path);
                    string label = (mini || u < Ui.F(32)) ? ShortLabel(k.Label) : k.Label;
                    if (!mix && !mini && showValues && raw > 0)
                    {
                        Ui.DrawText(g, label, Ui.FontPx(9.5f, false), fg,
                            new RectangleF(r.X + Ui.F(2), r.Y + r.Height * 0.18f, r.Width - Ui.F(4), r.Height * 0.42f),
                            StringAlignment.Center, StringAlignment.Center);
                        string v = metric == Metric.Count ? Ui.FmtCompact(raw) : Ui.FmtDurShort(raw);
                        Ui.DrawText(g, v, Ui.FontPx(9f, true), fg,
                            new RectangleF(r.X + Ui.F(2), r.Y + r.Height * 0.5f, r.Width - Ui.F(4), r.Height * 0.42f),
                            StringAlignment.Center, StringAlignment.Center);
                    }
                    else
                    {
                        Ui.DrawText(g, label, Ui.FontPx(mini ? 6.5f : (mix ? 9.5f : 10f), false), fg,
                            new RectangleF(r.X + Ui.F(2), r.Y, r.Width - Ui.F(4), r.Height),
                            StringAlignment.Center, StringAlignment.Center);
                    }
                    if (hoverVk == k.Vk)
                        Ui.StrokeRound(g, r, Ui.F(5), Ui.Accent, Ui.F(2));
                    AddKeyHit(r, k.Vk);
                }
            }
            return h;
        }

        private static string ShortLabel(string s)
        {
            switch (s)
            {
                case "Backspace": return "Bk";
                case "Enter": return "Ent";
                case "PrtSc": return "Prt";
                case "ScrLk": return "Scr";
                case "Pause": return "P";
                case "Caps": return "Cap";
                case "Shift": return "Sh";
                case "Ctrl": return "Ct";
                case "Alt": return "Al";
                case "Menu": return "M";
                case "Space": return "Spc";
                case "Home": return "Hm";
                case "PgUp": return "PU";
                case "PgDn": return "PD";
                default: return s;
            }
        }

        private Dictionary<int, KeyStat> ByVk()
        {
            Dictionary<int, KeyStat> d = new Dictionary<int, KeyStat>();
            if (snap == null) return d;
            foreach (KeyStat k in snap.Keys) d[k.Vk] = k;
            return d;
        }

        private static Color Heat(double t)
        {
            if (t <= 0) return HeatC[0];
            if (t >= 1) return HeatC[4];
            for (int i = 0; i < HeatX.Length - 1; i++)
            {
                if (t <= HeatX[i + 1])
                {
                    double f = (t - HeatX[i]) / (HeatX[i + 1] - HeatX[i]);
                    Color a = HeatC[i], b = HeatC[i + 1];
                    return Color.FromArgb(
                        (int)Math.Round(a.R + (b.R - a.R) * f),
                        (int)Math.Round(a.G + (b.G - a.G) * f),
                        (int)Math.Round(a.B + (b.B - a.B) * f));
                }
            }
            return HeatC[4];
        }
    }
}
