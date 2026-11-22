using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;

namespace Keytrace
{
    internal static class Ui
    {
        public static float S = 1f;

        public static readonly Color Bg = FromHex("#FCFCFD");
        public static readonly Color Side = FromHex("#FBFBFC");
        public static readonly Color Panel = Color.White;
        public static readonly Color Line = FromHex("#E9E9EE");
        public static readonly Color LineSoft = FromHex("#F1F1F5");
        public static readonly Color Text = FromHex("#1B1F24");
        public static readonly Color Text2 = FromHex("#69707D");
        public static readonly Color Text3 = FromHex("#9AA1AD");
        public static readonly Color Accent = FromHex("#2F6FEB");
        public static readonly Color Accent2 = FromHex("#1D4ED8");
        public static readonly Color AccentSoft = FromHex("#EEF4FF");
        public static readonly Color Green = FromHex("#16A34A");
        public static readonly Color Red = FromHex("#DC2626");
        public static readonly Color Track = FromHex("#F1F2F5");

        public static Font F10, F11, F12, F13, F14B, F15B, F18B, F26B, F28B;

        public static void Init()
        {
            try
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    S = Math.Max(1f, g.DpiX / 96f);
            }
            catch { S = 1f; }
            F10 = FontPx(10.5f, false);
            F11 = FontPx(11.5f, false);
            F12 = FontPx(12.5f, false);
            F13 = FontPx(13.5f, false);
            F14B = FontPx(14f, true);
            F15B = FontPx(15.5f, true);
            F18B = FontPx(18f, true);
            F26B = FontPx(26f, true);
            F28B = FontPx(28f, true);
        }

        public static Font FontPx(float px, bool bold)
        {
            string key = px.ToString("0.##") + (bold ? "b" : "r");
            Font f;
            if (fontCache.TryGetValue(key, out f)) return f;
            f = new Font("Microsoft YaHei UI", px * S,
                bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
            fontCache[key] = f;
            return f;
        }

        private static readonly Dictionary<string, Font> fontCache = new Dictionary<string, Font>();

        public static Color FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            return Color.FromArgb(
                int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber));
        }

        public static float F(float v) { return v * S; }
        public static int Px(float v) { return (int)Math.Round(v * S); }

        public static GraphicsPath RoundPath(RectangleF r, float radius)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0.5f)
            {
                p.AddRectangle(r);
                return p;
            }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void FillRound(Graphics g, RectangleF r, float rad, Color c)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (GraphicsPath p = RoundPath(r, rad))
            using (SolidBrush b = new SolidBrush(c))
                g.FillPath(b, p);
        }

        public static void StrokeRound(Graphics g, RectangleF r, float rad, Color c, float width)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (GraphicsPath p = RoundPath(r, rad))
            using (Pen pen = new Pen(c, width))
                g.DrawPath(pen, p);
        }

        public static void Card(Graphics g, RectangleF r, bool shadow)
        {
            FillRound(g, r, F(14), Panel);
            StrokeRound(g, r, F(14), Line, 1f);
        }

        public static void DrawText(Graphics g, string s, Font f, Color c, RectangleF r,
            StringAlignment h = StringAlignment.Near, StringAlignment v = StringAlignment.Center,
            bool wrap = false)
        {
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = h;
                sf.LineAlignment = v;
                if (wrap) sf.FormatFlags = StringFormatFlags.LineLimit;
                else
                {
                    sf.FormatFlags = StringFormatFlags.NoWrap;
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                }
                using (SolidBrush b = new SolidBrush(c))
                    g.DrawString(s, f, b, r, sf);
            }
        }

        public static void DrawLine(Graphics g, float x1, float y1, float x2, float y2, Color c, float w)
        {
            using (Pen p = new Pen(c, w))
                g.DrawLine(p, x1, y1, x2, y2);
        }

        public static string FmtInt(long n)
        {
            return n.ToString("N0", CultureInfo.CurrentCulture);
        }

        public static string FmtCompact(double n)
        {
            if (n < 1000) return Math.Round(n).ToString("0");
            if (n < 10000) return (n / 1000.0).ToString("0.0") + "k";
            if (n < 1000000) return Math.Round(n / 1000.0).ToString("0") + "k";
            return (n / 1000000.0).ToString("0.0") + "M";
        }

        public static string FmtDur(double ms)
        {
            if (ms < 1000) return Math.Round(ms).ToString("0") + " ms";
            double s = ms / 1000.0;
            if (s < 60) return s.ToString(s < 10 ? "0.00" : "0.0") + " s";
            long m = (long)Math.Floor(s / 60);
            long rs = (long)Math.Round(s - m * 60);
            if (rs == 60) { rs = 0; m++; }
            if (m < 60) return m + " 分 " + rs + " 秒";
            long h = m / 60;
            return h + " 小时 " + (m % 60) + " 分";
        }

        public static string FmtDurShort(double ms)
        {
            if (ms < 1000) return Math.Round(ms).ToString("0") + "ms";
            double s = ms / 1000.0;
            if (s < 60) return s.ToString(s < 10 ? "0.0" : "0") + "s";
            double m = s / 60.0;
            if (m < 60) return m.ToString(m < 10 ? "0.0" : "0") + "m";
            return (m / 60.0).ToString("0.0") + "h";
        }

        public static string FmtBytes(long n)
        {
            if (n < 1024) return n + " B";
            if (n < 1024 * 1024) return (n / 1024.0).ToString("0.0") + " KB";
            return (n / 1024.0 / 1024.0).ToString("0.00") + " MB";
        }

        public static string FmtDate(long hourEpoch)
        {
            DateTime t = DateTimeOffset.FromUnixTimeSeconds(hourEpoch).ToLocalTime().DateTime;
            return t.ToString("yyyy-MM-dd");
        }

        public static string FmtDateTime(long hourEpoch)
        {
            DateTime t = DateTimeOffset.FromUnixTimeSeconds(hourEpoch).ToLocalTime().DateTime;
            return t.ToString("M/d HH:mm");
        }

        public static string FmtTime(long hourEpoch)
        {
            DateTime t = DateTimeOffset.FromUnixTimeSeconds(hourEpoch).ToLocalTime().DateTime;
            return t.ToString("HH:mm");
        }

        // ---------------------------------------------------------- 键名

        private static readonly string[] KeyNames = BuildKeyNames();

        private static string[] BuildKeyNames()
        {
            string[] a = new string[256];
            for (int i = 0; i < a.Length; i++) a[i] = null;
            a[0x08] = "Backspace"; a[0x09] = "Tab"; a[0x0C] = "Clear"; a[0x0D] = "Enter";
            a[0x13] = "Pause"; a[0x14] = "CapsLock"; a[0x1B] = "Esc"; a[0x20] = "Space";
            a[0x21] = "PageUp"; a[0x22] = "PageDown"; a[0x23] = "End"; a[0x24] = "Home";
            a[0x25] = "Left"; a[0x26] = "Up"; a[0x27] = "Right"; a[0x28] = "Down";
            a[0x2C] = "PrintScreen"; a[0x2D] = "Insert"; a[0x2E] = "Delete";
            a[0x5B] = "LWin"; a[0x5C] = "RWin"; a[0x5D] = "Menu";
            a[0x6A] = "Numpad *"; a[0x6B] = "Numpad +"; a[0x6D] = "Numpad -";
            a[0x6E] = "Numpad ."; a[0x6F] = "Numpad /";
            a[0x90] = "NumLock"; a[0x91] = "ScrollLock";
            a[0xA0] = "LShift"; a[0xA1] = "RShift"; a[0xA2] = "LCtrl"; a[0xA3] = "RCtrl";
            a[0xA4] = "LAlt"; a[0xA5] = "RAlt";
            a[0xAD] = "VolumeMute"; a[0xAE] = "VolumeDown"; a[0xAF] = "VolumeUp";
            a[0xB0] = "MediaNext"; a[0xB1] = "MediaPrev"; a[0xB2] = "MediaStop";
            a[0xB3] = "MediaPlay"; a[0xBA] = ";"; a[0xBB] = "="; a[0xBC] = ",";
            a[0xBD] = "-"; a[0xBE] = "."; a[0xBF] = "/"; a[0xC0] = "`";
            a[0xDB] = "["; a[0xDC] = "\\"; a[0xDD] = "]"; a[0xDE] = "'";
            for (int i = 0; i < 10; i++) a[0x30 + i] = i.ToString();
            for (int i = 0; i < 26; i++) a[0x41 + i] = ((char)('A' + i)).ToString();
            for (int i = 0; i < 10; i++) a[0x60 + i] = "Numpad" + i;
            for (int i = 0; i < 24; i++) a[0x70 + i] = "F" + (i + 1);
            return a;
        }

        public static string KeyName(int vk)
        {
            if (vk >= 0 && vk < KeyNames.Length && KeyNames[vk] != null) return KeyNames[vk];
            return "VK_" + vk.ToString("X2");
        }
    }
}
