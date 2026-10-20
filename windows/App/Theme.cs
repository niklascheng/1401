using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace A1401
{
    /// <summary>Material Design 3 (M3) pill linear progress bar.</summary>
    class M3ProgressBar : Control
    {
        int min = 0;
        int max = 100;
        int val = 0;

        public int Minimum
        {
            get { return min; }
            set { min = value; if (val < min) val = min; Invalidate(); }
        }

        public int Maximum
        {
            get { return max; }
            set { max = Math.Max(min + 1, value); if (val > max) val = max; Invalidate(); }
        }

        public int Value
        {
            get { return val; }
            set
            {
                int clamped = Math.Max(min, Math.Min(max, value));
                if (val != clamped) { val = clamped; Invalidate(); }
            }
        }

        public M3ProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Color clear = Parent != null ? Parent.BackColor : Theme.Bg;
            g.Clear(clear);

            RectangleF track = new RectangleF(0.5f, 0.5f, Math.Max(2f, Width - 1.5f), Math.Max(2f, Height - 1.5f));
            float radius = track.Height / 2f;
            using (var trackBr = new SolidBrush(Theme.SurfaceContainerHigh))
                Theme.FillRounded(g, trackBr, track, radius);
            using (var trackPen = new Pen(Theme.OutlineVariant, 1f))
                Theme.DrawRounded(g, trackPen, track, radius);

            float range = Math.Max(1, Maximum - Minimum);
            float ratio = Math.Max(0f, Math.Min(1f, (Value - Minimum) / range));
            if (ratio > 0.005f)
            {
                float fillW = Math.Max(track.Height, track.Width * ratio);
                RectangleF barRect = new RectangleF(track.X, track.Y, Math.Min(track.Width, fillW), track.Height);
                using (var fillBr = new SolidBrush(Theme.Btn))
                    Theme.FillRounded(g, fillBr, barRect, radius);
                using (var edgePen = new Pen(Theme.Primary, 1f))
                    Theme.DrawRounded(g, edgePen, barRect, radius);
            }
        }
    }

    /// <summary>Material Design 3 interactive action card row / tonal pill chip built on LinkLabel.
    /// Preserves LinkLabel.Links and OnLinkClicked so verification and keyboard accessibility work identically.</summary>
    class M3LinkLabel : LinkLabel
    {
        public string IconKind { get; set; } = "book";
        public string IconGlyph { get { return IconKind; } set { IconKind = value; } }
        public bool IsChip { get; set; }
        bool hovered;
        bool pressed;
        bool suppressBaseLinkClick;

        public M3LinkLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                Invalidate();
            }
            suppressBaseLinkClick = true;
            try
            {
                base.OnMouseUp(e);
            }
            finally
            {
                suppressBaseLinkClick = false;
            }
        }

        protected override void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
        {
            if (suppressBaseLinkClick) return;
            base.OnLinkClicked(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left && Enabled && ClientRectangle.Contains(e.Location))
            {
                var link = Links.Count > 0 ? Links[0] : new Link(0, (Text ?? "").Length);
                OnLinkClicked(new LinkLabelLinkClickedEventArgs(link, e.Button));
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color clear = (Tag as string) == "on-card"
                ? Theme.SurfaceContainerLow
                : (Parent != null ? Parent.BackColor : Theme.Bg);
            g.Clear(clear);

            RectangleF rect = new RectangleF(1f, 1f, Math.Max(4f, Width - 2.5f), Math.Max(4f, Height - 2.5f));

            if (IsChip)
            {
                float radius = rect.Height / 2f;
                Color fill = !Enabled
                    ? Theme.SurfaceContainerLow
                    : (pressed
                        ? Theme.Blend(Theme.PrimaryContainer, Theme.Primary, 0.25f)
                        : (hovered ? Theme.PrimaryContainer : Theme.Blend(Theme.PrimaryContainer, Theme.SurfaceContainerHigh, 0.35f)));
                Color border = !Enabled ? Theme.OutlineVariant : (hovered ? Theme.Primary : Theme.Blend(Theme.Primary, Theme.OutlineVariant, 0.4f));
                Color fg = !Enabled ? Theme.OnSurfaceVariant : Theme.OnPrimaryContainer;

                using (var br = new SolidBrush(fill))
                    Theme.FillRounded(g, br, rect, radius);
                using (var pen = new Pen(border, hovered ? 1.4f : 1f))
                    Theme.DrawRounded(g, pen, rect, radius);

                float iconSz = Math.Max(12f, Math.Min(16f, rect.Height * 0.54f));
                float iconX = rect.X + 10f;
                float iconY = rect.Y + (rect.Height - iconSz) / 2f;
                Theme.DrawVectorIcon(g, IconKind, new RectangleF(iconX, iconY, iconSz, iconSz), fg, 1.6f);

                int textLeft = (int)Math.Round(iconX + iconSz + 6f);
                int textRight = (int)Math.Round(rect.Right - 18f);
                Rectangle textBox = new Rectangle(textLeft, 0, Math.Max(10, textRight - textLeft), Height);
                TextRenderer.DrawText(g, Text ?? "", Theme.ChipFont, textBox, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                // Trailing chevron
                float cx = rect.Right - 11f;
                float cy = rect.Y + rect.Height / 2f;
                using (var cPen = new Pen(fg, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(cPen, cx - 2f, cy - 3.5f, cx + 1.5f, cy);
                    g.DrawLine(cPen, cx + 1.5f, cy, cx - 2f, cy + 3.5f);
                }
            }
            else
            {
                float radius = Math.Min(10f, rect.Height / 2f);
                Color fill = !Enabled
                    ? Theme.SurfaceContainerLow
                    : (pressed
                        ? Theme.Blend(Theme.SurfaceContainerHigh, Theme.Primary, 0.20f)
                        : (hovered ? Theme.SurfaceContainerHigh : Theme.SurfaceContainerLow));
                Color border = !Enabled
                    ? Theme.OutlineVariant
                    : (hovered ? Theme.Primary : Theme.OutlineVariant);
                Color fg = !Enabled ? Theme.OnSurfaceVariant : (hovered ? Theme.Primary : Theme.OnSurface);

                using (var br = new SolidBrush(fill))
                    Theme.FillRounded(g, br, rect, radius);
                using (var pen = new Pen(border, hovered ? 1.3f : 1f))
                    Theme.DrawRounded(g, pen, rect, radius);

                // Leading tonal circular icon badge
                float badgeSz = Math.Max(18f, Math.Min(24f, rect.Height - 10f));
                float bx = rect.X + 10f;
                float by = rect.Y + (rect.Height - badgeSz) / 2f;
                Color badgeBg = hovered ? Theme.PrimaryContainer : Theme.SurfaceContainerHigh;
                Color badgeFg = hovered ? Theme.OnPrimaryContainer : Theme.Primary;
                using (var bBr = new SolidBrush(badgeBg))
                    g.FillEllipse(bBr, bx, by, badgeSz, badgeSz);

                float iconSz = badgeSz * 0.56f;
                RectangleF iconRect = new RectangleF(bx + (badgeSz - iconSz) / 2f, by + (badgeSz - iconSz) / 2f, iconSz, iconSz);
                Theme.DrawVectorIcon(g, IconKind, iconRect, badgeFg, 1.5f);

                int textLeft = (int)Math.Round(bx + badgeSz + 10f);
                int textRight = (int)Math.Round(rect.Right - 26f);
                Rectangle textBox = new Rectangle(textLeft, 0, Math.Max(10, textRight - textLeft), Height);
                TextRenderer.DrawText(g, Text ?? "", Theme.Link, textBox, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                // Trailing chevron arrow
                float cx = rect.Right - 14f;
                float cy = rect.Y + rect.Height / 2f;
                using (var cPen = new Pen(hovered ? Theme.Primary : Theme.OnSurfaceVariant, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(cPen, cx - 2.5f, cy - 4f, cx + 1.5f, cy);
                    g.DrawLine(cPen, cx + 1.5f, cy, cx - 2.5f, cy + 4f);
                }
            }
        }
    }

    /// <summary>Material Design 3 custom-painted ComboBox without Win32 3D white border or white arrow button frame.</summary>
    class M3ComboBox : ComboBox
    {
        bool hovered;

        public M3ComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            ItemHeight = 22;
            Cursor = Cursors.Hand;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle &= ~0x00000200; // Clear WS_EX_CLIENTEDGE
                cp.ExStyle &= ~0x00020000; // Clear WS_EX_STATICEDGE
                cp.Style &= ~0x00800000;   // Clear WS_BORDER
                return cp;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnDropDown(EventArgs e)
        {
            Invalidate();
            base.OnDropDown(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            Invalidate();
            base.OnDropDownClosed(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            Invalidate();
            base.OnSelectedIndexChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color clear = (Tag as string) == "on-card"
                ? Theme.SurfaceContainerLow
                : (Parent != null ? Parent.BackColor : Theme.Bg);
            g.Clear(clear);

            RectangleF rect = new RectangleF(0.5f, 0.5f, Math.Max(4f, Width - 1.5f), Math.Max(4f, Height - 1.5f));
            float radius = Math.Min(10f, rect.Height / 2f);

            Color fill = !Enabled
                ? Theme.SurfaceContainerLow
                : (DroppedDown || hovered
                    ? Theme.Blend(Theme.SurfaceContainerHigh, Theme.Primary, 0.12f)
                    : Theme.SurfaceContainerHigh);
            Color border = !Enabled
                ? Theme.OutlineVariant
                : (DroppedDown || Focused ? Theme.Primary : (hovered ? Theme.Outline : Theme.OutlineVariant));

            Theme.FillRounded(g, fill, rect, radius);
            Theme.DrawRounded(g, border, rect, radius, (DroppedDown || Focused) ? 1.5f : 1f);

            string text = (SelectedIndex >= 0 && SelectedIndex < Items.Count && Items[SelectedIndex] != null)
                ? Items[SelectedIndex].ToString()
                : (Text ?? "");
            Rectangle textRect = new Rectangle(12, 0, Math.Max(10, Width - 38), Height);
            TextRenderer.DrawText(g, text, Font, textRect, Enabled ? Theme.OnSurface : Theme.OnSurfaceVariant,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

            float isz = 12f;
            Theme.DrawVectorIcon(g, "chevron-down",
                new RectangleF(Width - isz - 12f, (Height - isz) / 2f, isz, isz),
                Enabled ? Theme.Primary : Theme.OnSurfaceVariant, 1.8f);
        }
    }

    /// <summary>Material Design 3 custom-painted hardware table ListView without native SysHeader32 white gap or bevels.</summary>
    class M3ListView : ListView
    {
        public M3ListView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BorderStyle = BorderStyle.None;
            HeaderStyle = ColumnHeaderStyle.None;
            OwnerDraw = true;
            FullRowSelect = true;
            View = View.Details;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle &= ~0x00000200; // Clear WS_EX_CLIENTEDGE
                cp.ExStyle &= ~0x00020000; // Clear WS_EX_STATICEDGE
                cp.Style &= ~0x00800000;   // Clear WS_BORDER
                cp.Style |= 0x00004000;    // LVS_NOCOLUMNHEADER so native SysHeader32 is never created
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Repaint when items or columns are inserted, updated, or cleared
            if (m.Msg == 0x1007 || m.Msg == 0x104D || m.Msg == 0x1008 || m.Msg == 0x1009 || m.Msg == 0x1006 || m.Msg == 0x104C)
                Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color clear = Parent != null ? Parent.BackColor : Theme.CardBg;
            g.Clear(clear);

            RectangleF rect = new RectangleF(0.5f, 0.5f, Math.Max(4f, Width - 1.5f), Math.Max(4f, Height - 1.5f));
            float radius = 10f;
            int hdrH = 28;
            int col0W = Columns.Count > 0 ? Columns[0].Width : 176;

            using (var clipPath = Theme.RoundedRect(rect, radius))
            {
                using (var bgBr = new SolidBrush(Theme.SurfaceContainerLow))
                    g.FillPath(bgBr, clipPath);

                GraphicsState state = g.Save();
                g.SetClip(clipPath);

                using (var hdrBr = new SolidBrush(Theme.SurfaceContainerHigh))
                    g.FillRectangle(hdrBr, 0, 0, Width, hdrH);
                using (var hdrPen = new Pen(Theme.OutlineVariant, 1f))
                    g.DrawLine(hdrPen, 0, hdrH - 1, Width, hdrH - 1);

                string col0Title = Columns.Count > 0 ? (Columns[0].Text ?? "") : "";
                string col1Title = Columns.Count > 1 ? (Columns[1].Text ?? "") : "";
                TextRenderer.DrawText(g, col0Title, Theme.SmallBold,
                    new Rectangle(12, 0, Math.Max(10, col0W - 16), hdrH),
                    Theme.OnSurfaceVariant,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, col1Title, Theme.SmallBold,
                    new Rectangle(col0W + 10, 0, Math.Max(10, Width - col0W - 18), hdrH),
                    Theme.OnSurfaceVariant,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                int count = Items.Count;
                if (count > 0)
                {
                    int availH = Math.Max(20, Height - hdrH - 2);
                    int rowH = Math.Min(28, Math.Max(19, availH / count));
                    for (int i = 0; i < count; i++)
                    {
                        int ry = hdrH + i * rowH;
                        Color rowBg = (i % 2 == 1)
                            ? Theme.Blend(Theme.SurfaceContainerLow, Theme.SurfaceContainer, 0.55f)
                            : Theme.SurfaceContainerLow;
                        using (var rBr = new SolidBrush(rowBg))
                            g.FillRectangle(rBr, 0, ry, Width, rowH);
                        if (i < count - 1)
                        {
                            using (var rPen = new Pen(Theme.Blend(Theme.OutlineVariant, rowBg, 0.55f), 1f))
                                g.DrawLine(rPen, 10, ry + rowH - 1, Width - 10, ry + rowH - 1);
                        }

                        var item = Items[i];
                        string kText = item.SubItems.Count > 0 ? item.SubItems[0].Text : item.Text;
                        string vText = item.SubItems.Count > 1 ? item.SubItems[1].Text : "";

                        TextRenderer.DrawText(g, kText ?? "", Theme.BodyBold,
                            new Rectangle(12, ry, Math.Max(10, col0W - 16), rowH),
                            Theme.Primary,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                        TextRenderer.DrawText(g, vText ?? "", Theme.Fact,
                            new Rectangle(col0W + 10, ry, Math.Max(10, Width - col0W - 18), rowH),
                            Theme.OnSurface,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    }
                }
                else
                {
                    // Subtle M3 placeholder skeleton rows when hardware table has not been populated yet
                    int rowH = Math.Max(22, (Height - hdrH - 2) / 4);
                    for (int i = 0; i < 4; i++)
                    {
                        int ry = hdrH + i * rowH;
                        Color rowBg = (i % 2 == 1)
                            ? Theme.Blend(Theme.SurfaceContainerLow, Theme.SurfaceContainer, 0.45f)
                            : Theme.SurfaceContainerLow;
                        using (var rBr = new SolidBrush(rowBg))
                            g.FillRectangle(rBr, 0, ry, Width, rowH);
                        Color skel = Theme.Blend(rowBg, Theme.OutlineVariant, 0.38f);
                        float pillH = 7f;
                        float py = ry + (rowH - pillH) / 2f;
                        Theme.FillRounded(g, skel, new RectangleF(12f, py, 72f + (i % 2) * 20f, pillH), pillH / 2f);
                        Theme.FillRounded(g, skel, new RectangleF(col0W + 10f, py, 160f + ((i * 37) % 90), pillH), pillH / 2f);
                    }
                }

                g.Restore(state);
            }

            Theme.DrawRounded(g, Theme.OutlineVariant, rect, radius, 1f);
        }
    }

    /// <summary>Material Design 3 (M3) theme and color token system for 1401.
    /// Defaults to Material Blue (seed #0B57D0) and allows user customization in Settings
    /// (M3 color presets, custom seed color picker, and Dark/Light appearance mode).</summary>
    static class Theme
    {
        static Color C(int rgb) { return Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255); }

        public static string CurrentScheme { get; private set; } = "blue";
        public static bool IsDark { get; private set; } = true;
        public static Color CustomSeed { get; private set; } = C(0x0B57D0);

        // Material Design 3 system color tokens (--md-sys-color-*)
        public static Color Surface { get; private set; }
        public static Color SurfaceContainerLow { get; private set; }
        public static Color SurfaceContainer { get; private set; }
        public static Color SurfaceContainerHigh { get; private set; }
        public static Color OutlineVariant { get; private set; }
        public static Color Outline { get; private set; }
        public static Color Primary { get; private set; }
        public static Color OnPrimary { get; private set; }
        public static Color PrimaryContainer { get; private set; }
        public static Color OnPrimaryContainer { get; private set; }
        public static Color Secondary { get; private set; }
        public static Color OnSurface { get; private set; }
        public static Color OnSurfaceVariant { get; private set; }
        public static Color Error { get; private set; }
        public static Color Warning { get; private set; }
        public static Color OnBtn { get; private set; }

        // Mapped properties used across MainForm controls
        public static Color Bg { get; private set; }
        public static Color Panel { get; private set; }
        public static Color Panel2 { get; private set; }
        public static Color Line { get; private set; }
        public static Color Text { get; private set; }
        public static Color Muted { get; private set; }
        public static Color Purple { get; private set; }
        public static Color Purple2 { get; private set; }
        public static Color Btn { get; private set; }
        public static Color BtnEdge { get; private set; }
        public static Color Cyan { get; private set; }
        public static Color Err { get; private set; }
        public static Color Amber { get; private set; }
        public static Color CardBg { get { return SurfaceContainerLow; } }
        public static Color CardBorder { get { return OutlineVariant; } }

        public static event Action Changed;

        public static readonly string[] PresetIds = { "blue", "teal", "green", "purple", "orange", "rose" };

        static readonly HashSet<Control> HoveredControls = new HashSet<Control>();
        static readonly HashSet<Control> PressedControls = new HashSet<Control>();
        static readonly Dictionary<Label, int> NavIndices = new Dictionary<Label, int>();
        static Func<int> NavPageGetter = () => 0;

        static string ConfigPath
        {
            get
            {
                string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(root)) root = Path.GetTempPath();
                return Path.Combine(root, "1401", "theme.json");
            }
        }

        static Theme()
        {
            ApplyTokens("blue", true, C(0x0B57D0));
            LoadSaved();
        }

        public static Color PresetSwatch(string scheme)
        {
            switch ((scheme ?? "blue").ToLowerInvariant())
            {
                case "teal": return C(0x00838F);
                case "green": return C(0x1E8E3E);
                case "purple": return C(0x7C4DFF);
                case "orange": return C(0xE65100);
                case "rose": return C(0xD81B60);
                case "custom": return CustomSeed;
                default: return C(0x1B6EF3); // Material Blue default
            }
        }

        public static string PresetLabelKey(string scheme)
        {
            switch ((scheme ?? "blue").ToLowerInvariant())
            {
                case "teal": return "Teal";
                case "green": return "Green";
                case "purple": return "Purple";
                case "orange": return "Orange";
                case "rose": return "Rose";
                case "custom": return "Custom";
                default: return "Blue (Default)";
            }
        }

        public static void SetPalette(string scheme, bool isDark, Color? customSeed = null, bool save = true)
        {
            string norm = string.IsNullOrEmpty(scheme) ? "blue" : scheme.Trim().ToLowerInvariant();
            Color seed = customSeed ?? CustomSeed;
            ApplyTokens(norm, isDark, seed);
            if (save) SaveCurrent();
            var h = Changed;
            if (h != null) h();
        }

        public static void ResetToDefault(bool save = true)
        {
            SetPalette("blue", true, C(0x0B57D0), save);
        }

        static void ApplyTokens(string scheme, bool isDark, Color customSeed)
        {
            CurrentScheme = scheme;
            IsDark = isDark;
            CustomSeed = customSeed;

            Color seed;
            switch (scheme)
            {
                case "teal": seed = C(0x00838F); break;
                case "green": seed = C(0x1E8E3E); break;
                case "purple": seed = C(0x7C4DFF); break;
                case "orange": seed = C(0xE65100); break;
                case "rose": seed = C(0xD81B60); break;
                case "custom": seed = customSeed; break;
                default:
                    CurrentScheme = "blue";
                    seed = C(0x1B6EF3);
                    break;
            }

            float h, s, l;
            RgbToHsl(seed, out h, out s, out l);
            float chroma = Math.Max(0.45f, Math.Min(0.92f, s));
            float neutralS = Math.Min(0.18f, chroma * 0.22f);

            if (CurrentScheme == "blue")
            {
                if (isDark)
                {
                    Surface = C(0x0F141C);
                    SurfaceContainerLow = C(0x171C26);
                    SurfaceContainer = C(0x1B202B);
                    SurfaceContainerHigh = C(0x252B37);
                    OutlineVariant = C(0x414855);
                    Outline = C(0x8B92A0);

                    Primary = C(0xA8C7FA);
                    OnPrimary = C(0x062E6F);
                    PrimaryContainer = C(0x0842A0);
                    OnPrimaryContainer = C(0xD3E3FD);
                    Secondary = C(0x7FCFFF);

                    OnSurface = C(0xE2E7F0);
                    OnSurfaceVariant = C(0xB6BFD0);
                    Error = C(0xF28B82);
                    Warning = C(0xFDD663);

                    Btn = C(0x1B6EF3);
                    BtnEdge = Primary;
                    OnBtn = Color.White;
                }
                else
                {
                    Surface = C(0xF8F9FF);
                    SurfaceContainerLow = C(0xF0F4FA);
                    SurfaceContainer = C(0xEAEEF6);
                    SurfaceContainerHigh = C(0xE2E7F0);
                    OutlineVariant = C(0xC4C6D0);
                    Outline = C(0x74777F);

                    Primary = C(0x0B57D0);
                    OnPrimary = Color.White;
                    PrimaryContainer = C(0xD3E3FD);
                    OnPrimaryContainer = C(0x041E49);
                    Secondary = C(0x00639B);

                    OnSurface = C(0x191C20);
                    OnSurfaceVariant = C(0x44474E);
                    Error = C(0xB3261E);
                    Warning = C(0xB06000);

                    Btn = Primary;
                    BtnEdge = Primary;
                    OnBtn = Color.White;
                }
            }
            else if (isDark)
            {
                Surface = HslToRgb(h, neutralS, 0.08f);
                SurfaceContainerLow = HslToRgb(h, neutralS, 0.115f);
                SurfaceContainer = HslToRgb(h, neutralS, 0.14f);
                SurfaceContainerHigh = HslToRgb(h, neutralS, 0.185f);
                OutlineVariant = HslToRgb(h, neutralS * 1.1f, 0.28f);
                Outline = HslToRgb(h, neutralS, 0.56f);

                Primary = HslToRgb(h, Math.Max(0.75f, chroma), 0.78f);
                OnPrimary = HslToRgb(h, Math.Max(0.70f, chroma), 0.16f);
                PrimaryContainer = HslToRgb(h, Math.Max(0.68f, chroma), 0.30f);
                OnPrimaryContainer = HslToRgb(h, Math.Max(0.80f, chroma), 0.90f);
                Secondary = HslToRgb((h + 18f) % 360f, Math.Max(0.65f, chroma * 0.85f), 0.74f);

                OnSurface = HslToRgb(h, neutralS * 0.5f, 0.91f);
                OnSurfaceVariant = HslToRgb(h, neutralS * 0.7f, 0.74f);
                Error = C(0xF28B82);
                Warning = C(0xFDD663);

                Btn = HslToRgb(h, Math.Max(0.72f, chroma), 0.48f);
                BtnEdge = Primary;
                OnBtn = Color.White;
            }
            else
            {
                Surface = HslToRgb(h, neutralS * 0.6f, 0.98f);
                SurfaceContainerLow = HslToRgb(h, neutralS * 0.75f, 0.955f);
                SurfaceContainer = HslToRgb(h, neutralS * 0.85f, 0.93f);
                SurfaceContainerHigh = HslToRgb(h, neutralS * 0.9f, 0.895f);
                OutlineVariant = HslToRgb(h, neutralS * 0.8f, 0.78f);
                Outline = HslToRgb(h, neutralS * 0.8f, 0.46f);

                Primary = HslToRgb(h, Math.Max(0.72f, chroma), 0.40f);
                OnPrimary = Color.White;
                PrimaryContainer = HslToRgb(h, Math.Max(0.75f, chroma), 0.90f);
                OnPrimaryContainer = HslToRgb(h, Math.Max(0.75f, chroma), 0.14f);
                Secondary = HslToRgb((h + 18f) % 360f, Math.Max(0.65f, chroma * 0.85f), 0.36f);

                OnSurface = HslToRgb(h, neutralS, 0.12f);
                OnSurfaceVariant = HslToRgb(h, neutralS, 0.30f);
                Error = C(0xB3261E);
                Warning = C(0xB06000);

                Btn = Primary;
                BtnEdge = Primary;
                OnBtn = Color.White;
            }

            Bg = Surface;
            Panel = SurfaceContainerLow;
            Panel2 = SurfaceContainer;
            Line = OutlineVariant;
            Text = OnSurface;
            Muted = OnSurfaceVariant;
            Purple = Primary;
            Purple2 = PrimaryContainer;
            Cyan = Secondary;
            Err = Error;
            Amber = Warning;
        }

        static void LoadSaved()
        {
            try
            {
                string p = ConfigPath;
                if (!File.Exists(p)) return;
                var d = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(p));
                if (d == null) return;
                string scheme = d.ContainsKey("scheme") && d["scheme"] != null ? d["scheme"].ToString() : "blue";
                bool dark = !d.ContainsKey("dark") || !(d["dark"] is bool) || (bool)d["dark"];
                Color custom = CustomSeed;
                if (d.ContainsKey("customHex") && d["customHex"] != null)
                {
                    string hex = d["customHex"].ToString().Trim().TrimStart('#');
                    int rgb;
                    if (hex.Length == 6 && int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out rgb))
                        custom = C(rgb);
                }
                ApplyTokens(scheme, dark, custom);
            }
            catch (Exception) { }
        }

        static void SaveCurrent()
        {
            try
            {
                string p = ConfigPath;
                string dir = Path.GetDirectoryName(p);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                string hex = string.Format("#{0:X2}{1:X2}{2:X2}", CustomSeed.R, CustomSeed.G, CustomSeed.B);
                string json = new JavaScriptSerializer().Serialize(new Dictionary<string, object>
                {
                    { "scheme", CurrentScheme },
                    { "dark", IsDark },
                    { "customHex", hex }
                });
                File.WriteAllText(p, json);
            }
            catch (Exception) { }
        }

        static void RgbToHsl(Color c, out float h, out float s, out float l)
        {
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            l = (max + min) / 2f;
            if (Math.Abs(max - min) < 0.0001f) { h = 215f; s = 0f; return; }
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6f : 0f);
            else if (max == g) h = (b - r) / d + 2f;
            else h = (r - g) / d + 4f;
            h *= 60f;
        }

        static Color HslToRgb(float h, float s, float l)
        {
            h = ((h % 360f) + 360f) % 360f;
            s = Math.Max(0f, Math.Min(1f, s));
            l = Math.Max(0f, Math.Min(1f, l));
            float c = (1f - Math.Abs(2f * l - 1f)) * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = l - c / 2f;
            float r = 0, g = 0, b = 0;
            if (h < 60f) { r = c; g = x; }
            else if (h < 120f) { r = x; g = c; }
            else if (h < 180f) { g = c; b = x; }
            else if (h < 240f) { g = x; b = c; }
            else if (h < 300f) { r = x; b = c; }
            else { r = c; b = x; }
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, (int)Math.Round((r + m) * 255f))),
                Math.Max(0, Math.Min(255, (int)Math.Round((g + m) * 255f))),
                Math.Max(0, Math.Min(255, (int)Math.Round((b + m) * 255f))));
        }

        public static Color Blend(Color baseColor, Color overlay, float alpha)
        {
            alpha = Math.Max(0f, Math.Min(1f, alpha));
            int r = (int)Math.Round(baseColor.R * (1f - alpha) + overlay.R * alpha);
            int g = (int)Math.Round(baseColor.G * (1f - alpha) + overlay.G * alpha);
            int b = (int)Math.Round(baseColor.B * (1f - alpha) + overlay.B * alpha);
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b)));
        }

        public static void EnableDoubleBuffering(Control c)
        {
            if (c == null) return;
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(c, true, null);
            }
            catch (Exception) { }
        }

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float maxR = Math.Min(r.Width, r.Height) / 2f;
            radius = Math.Max(0f, Math.Min(radius, maxR));
            if (radius <= 0.5f)
            {
                path.AddRectangle(r);
                path.CloseFigure();
                return path;
            }
            float d = radius * 2f;
            path.AddArc(r.X, r.Y, d, d, 180f, 90f);
            path.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
            path.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics g, Brush br, RectangleF r, float radius)
        {
            using (var path = RoundedRect(r, radius))
                g.FillPath(br, path);
        }

        public static void FillRounded(Graphics g, Color color, RectangleF r, float radius)
        {
            using (var br = new SolidBrush(color))
                FillRounded(g, br, r, radius);
        }

        public static void DrawRounded(Graphics g, Pen pen, RectangleF r, float radius)
        {
            using (var path = RoundedRect(r, radius))
                g.DrawPath(pen, path);
        }

        public static void DrawRounded(Graphics g, Color color, RectangleF r, float radius, float width = 1f)
        {
            using (var pen = new Pen(color, width))
                DrawRounded(g, pen, r, radius);
        }

        public static void DrawCard(Graphics g, Rectangle bounds, int radius, bool leftAccent = false)
        {
            DrawCard(g, bounds, radius, SurfaceContainerLow, OutlineVariant, leftAccent);
        }

        public static void DrawCard(Graphics g, Rectangle bounds, int radius, Color fill, Color border, bool leftAccent = false)
        {
            if (bounds.Width <= 2 || bounds.Height <= 2) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            RectangleF r = new RectangleF(bounds.X + 0.5f, bounds.Y + 0.5f, bounds.Width - 1f, bounds.Height - 1f);
            using (var br = new SolidBrush(fill))
                FillRounded(g, br, r, radius);
            using (var pen = new Pen(border, 1f))
                DrawRounded(g, pen, r, radius);
            if (leftAccent && bounds.Height > 20)
            {
                RectangleF bar = new RectangleF(bounds.X + 4f, bounds.Y + 10f, 3.5f, bounds.Height - 20f);
                using (var accBr = new SolidBrush(Primary))
                    FillRounded(g, accBr, bar, 1.75f);
            }
        }

        public static void DrawVectorIcon(Graphics g, string kind, RectangleF box, Color color, float strokeWidth = 1.6f)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = box.X + box.Width / 2f;
            float cy = box.Y + box.Height / 2f;
            float w = box.Width;
            float h = box.Height;

            using (var pen = new Pen(color, strokeWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var br = new SolidBrush(color))
            {
                switch (kind ?? "")
                {
                    case "check":
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.X + w * 0.18f, cy + h * 0.02f),
                            new PointF(box.X + w * 0.42f, box.Bottom - h * 0.20f),
                            new PointF(box.Right - w * 0.16f, box.Y + h * 0.22f)
                        });
                        break;

                    case "settings":
                        float rOuter = Math.Min(w, h) * 0.42f;
                        float rInner = Math.Min(w, h) * 0.18f;
                        for (int i = 0; i < 6; i++)
                        {
                            double ang = i * Math.PI / 3.0;
                            float x1 = cx + (float)Math.Cos(ang) * (rInner + 1f);
                            float y1 = cy + (float)Math.Sin(ang) * (rInner + 1f);
                            float x2 = cx + (float)Math.Cos(ang) * rOuter;
                            float y2 = cy + (float)Math.Sin(ang) * rOuter;
                            g.DrawLine(pen, x1, y1, x2, y2);
                        }
                        g.DrawEllipse(pen, cx - rInner * 1.45f, cy - rInner * 1.45f, rInner * 2.9f, rInner * 2.9f);
                        break;

                    case "book":
                        RectangleF bk = new RectangleF(box.X + w * 0.15f, box.Y + h * 0.12f, w * 0.70f, h * 0.76f);
                        FillRounded(g, Color.FromArgb(30, color), bk, 2f);
                        DrawRounded(g, pen, bk, 2f);
                        g.DrawLine(pen, bk.X + bk.Width * 0.28f, bk.Y, bk.X + bk.Width * 0.28f, bk.Bottom);
                        break;

                    case "chevron-down":
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.X + w * 0.18f, cy - h * 0.12f),
                            new PointF(cx, cy + h * 0.18f),
                            new PointF(box.Right - w * 0.18f, cy - h * 0.12f)
                        });
                        break;

                    case "log":
                    case "upload":
                        g.DrawLine(pen, cx, box.Bottom - h * 0.18f, cx, box.Y + h * 0.18f);
                        g.DrawLine(pen, cx - w * 0.24f, box.Y + h * 0.42f, cx, box.Y + h * 0.18f);
                        g.DrawLine(pen, cx + w * 0.24f, box.Y + h * 0.42f, cx, box.Y + h * 0.18f);
                        g.DrawLine(pen, box.X + w * 0.18f, box.Bottom - h * 0.12f, box.Right - w * 0.18f, box.Bottom - h * 0.12f);
                        break;

                    case "gpu":
                    case "firmware":
                    case "chip":
                        RectangleF chip = new RectangleF(box.X + w * 0.22f, box.Y + h * 0.22f, w * 0.56f, h * 0.56f);
                        DrawRounded(g, pen, chip, 2f);
                        g.FillRectangle(br, cx - w * 0.10f, cy - h * 0.10f, w * 0.20f, h * 0.20f);
                        g.DrawLine(pen, cx - w * 0.12f, box.Y + h * 0.08f, cx - w * 0.12f, chip.Y);
                        g.DrawLine(pen, cx + w * 0.12f, box.Y + h * 0.08f, cx + w * 0.12f, chip.Y);
                        g.DrawLine(pen, cx - w * 0.12f, chip.Bottom, cx - w * 0.12f, box.Bottom - h * 0.08f);
                        g.DrawLine(pen, cx + w * 0.12f, chip.Bottom, cx + w * 0.12f, box.Bottom - h * 0.08f);
                        break;

                    case "doctor":
                    case "stethoscope":
                        // Shield + check
                        PointF[] shield =
                        {
                            new PointF(cx, box.Y + h * 0.10f),
                            new PointF(box.Right - w * 0.15f, box.Y + h * 0.24f),
                            new PointF(box.Right - w * 0.20f, box.Y + h * 0.62f),
                            new PointF(cx, box.Bottom - h * 0.10f),
                            new PointF(box.X + w * 0.20f, box.Y + h * 0.62f),
                            new PointF(box.X + w * 0.15f, box.Y + h * 0.24f)
                        };
                        g.DrawPolygon(pen, shield);
                        g.DrawLines(pen, new[]
                        {
                            new PointF(cx - w * 0.14f, cy + h * 0.02f),
                            new PointF(cx - w * 0.02f, cy + h * 0.14f),
                            new PointF(cx + w * 0.16f, cy - h * 0.08f)
                        });
                        break;

                    case "update":
                    case "refresh":
                        RectangleF arc = new RectangleF(box.X + w * 0.16f, box.Y + h * 0.16f, w * 0.68f, h * 0.68f);
                        g.DrawArc(pen, arc, 30f, 285f);
                        g.DrawLine(pen, arc.Right - 1f, cy - h * 0.18f, arc.Right + 2f, cy + 1f);
                        g.DrawLine(pen, arc.Right - 4f, cy + 1f, arc.Right + 2f, cy + 1f);
                        break;

                    case "coffee":
                    case "heart":
                        RectangleF cup = new RectangleF(box.X + w * 0.14f, box.Y + h * 0.30f, w * 0.54f, h * 0.48f);
                        DrawRounded(g, pen, cup, 2.5f);
                        g.DrawArc(pen, cup.Right - 2f, cup.Y + h * 0.06f, w * 0.20f, h * 0.28f, -80f, 160f);
                        g.DrawLine(pen, box.X + w * 0.10f, box.Bottom - h * 0.12f, box.Right - w * 0.16f, box.Bottom - h * 0.12f);
                        break;

                    case "dark":
                        g.DrawArc(pen, box.X + w * 0.18f, box.Y + h * 0.18f, w * 0.64f, h * 0.64f, 45f, 250f);
                        break;

                    case "light":
                        float sr = Math.Min(w, h) * 0.22f;
                        g.DrawEllipse(pen, cx - sr, cy - sr, sr * 2f, sr * 2f);
                        for (int i = 0; i < 8; i++)
                        {
                            double a = i * Math.PI / 4.0;
                            g.DrawLine(pen,
                                cx + (float)Math.Cos(a) * (sr + 2f),
                                cy + (float)Math.Sin(a) * (sr + 2f),
                                cx + (float)Math.Cos(a) * (sr + 4.2f),
                                cy + (float)Math.Sin(a) * (sr + 4.2f));
                        }
                        break;
                }
            }
        }

        // Material Design 3 Typography: Segoe UI & Consolas on Latin Windows, Microsoft YaHei UI for CJK.
        internal static string Face(string latin)
        {
            if (Loc.Language == "zh") return "Microsoft YaHei UI";
            if (latin == "Times New Roman" || latin == "Verdana") return "Segoe UI";
            if (latin == "Courier New") return "Consolas";
            return latin;
        }

        public static readonly Font Body = new Font(Face("Segoe UI"), 9.75f);
        public static readonly Font BodyBold = new Font(Face("Segoe UI"), 9.75f, FontStyle.Bold);
        public static readonly Font Title = new Font(Face("Segoe UI"), 18f, FontStyle.Bold);
        public static readonly Font Brand = new Font(Face("Segoe UI"), 19f, FontStyle.Bold);
        public static readonly Font Mono = new Font(Face("Consolas"), 9.5f, FontStyle.Bold);
        public static readonly Font Link = new Font(Face("Segoe UI"), 9.5f, FontStyle.Regular);
        public static readonly Font ChipFont = new Font(Face("Segoe UI"), 9f, FontStyle.Bold);
        public static readonly Font BadgeFont = ChipFont;
        public static readonly Font SmallBold = new Font(Face("Segoe UI"), 8.25f, FontStyle.Bold);
        public static readonly Font Small = new Font(Face("Segoe UI"), 8.5f, FontStyle.Regular);
        public static readonly Font Log = new Font(Face("Consolas"), 9f);
        public static readonly Font Fact = new Font(Face("Consolas"), 9.5f);
        public static readonly Font Picker = new Font(Face("Segoe UI"), 9.75f);
        static readonly Font BtnBold = new Font(Face("Segoe UI"), 9.5f, FontStyle.Bold);
        static readonly Font BtnRegular = new Font(Face("Segoe UI"), 9.5f, FontStyle.Regular);
        static readonly Font SwatchBold = new Font(Face("Segoe UI"), 9.25f, FontStyle.Bold);
        static readonly Font SwatchRegular = new Font(Face("Segoe UI"), 9.25f, FontStyle.Regular);
        static readonly Font DialogHeading = new Font(Face("Segoe UI"), 14f, FontStyle.Bold);
        static readonly Font ListFont = new Font(Loc.Language == "zh" ? "Microsoft YaHei UI" : "Consolas", 10f);

        public static string CustomSeedHex
        {
            get { return string.Format("#{0:x2}{1:x2}{2:x2}", CustomSeed.R, CustomSeed.G, CustomSeed.B); }
        }

        static void TrackHoverAndPress(Control c)
        {
            EnableDoubleBuffering(c);
            c.MouseEnter -= OnControlMouseEnter;
            c.MouseEnter += OnControlMouseEnter;
            c.MouseLeave -= OnControlMouseLeave;
            c.MouseLeave += OnControlMouseLeave;
            c.MouseDown -= OnControlMouseDown;
            c.MouseDown += OnControlMouseDown;
            c.MouseUp -= OnControlMouseUp;
            c.MouseUp += OnControlMouseUp;
            c.Disposed -= OnControlDisposed;
            c.Disposed += OnControlDisposed;
        }

        static void OnControlMouseEnter(object s, EventArgs e)
        {
            var c = s as Control;
            if (c != null) { HoveredControls.Add(c); c.Invalidate(); }
        }

        static void OnControlMouseLeave(object s, EventArgs e)
        {
            var c = s as Control;
            if (c != null) { HoveredControls.Remove(c); PressedControls.Remove(c); c.Invalidate(); }
        }

        static void OnControlMouseDown(object s, MouseEventArgs e)
        {
            var c = s as Control;
            if (c != null && e.Button == MouseButtons.Left) { PressedControls.Add(c); c.Invalidate(); }
        }

        static void OnControlMouseUp(object s, MouseEventArgs e)
        {
            var c = s as Control;
            if (c != null) { PressedControls.Remove(c); c.Invalidate(); }
        }

        static void OnControlDisposed(object s, EventArgs e)
        {
            var c = s as Control;
            if (c != null)
            {
                HoveredControls.Remove(c);
                PressedControls.Remove(c);
                var lbl = c as Label;
                if (lbl != null) NavIndices.Remove(lbl);
            }
        }

        /// <summary>Material Design 3 Filled (primary) and Tonal/Outlined (secondary) stadium/pill button styling.</summary>
        public static void Style(Button b, bool primary = true)
        {
            b.Tag = primary ? "m3-primary" : "m3-secondary";
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.Font = primary ? BtnBold : BtnRegular;
            b.BackColor = primary ? Btn : SurfaceContainerHigh;
            b.ForeColor = primary ? OnBtn : OnSurface;
            b.FlatAppearance.BorderColor = primary ? BtnEdge : OutlineVariant;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = primary ? Primary : PrimaryContainer;
            b.FlatAppearance.MouseDownBackColor = PrimaryContainer;
            b.Cursor = Cursors.Hand;

            TrackHoverAndPress(b);
            b.Paint -= OnPillButtonPaint;
            b.Paint += OnPillButtonPaint;
            b.Invalidate();
        }

        static void OnPillButtonPaint(object sender, PaintEventArgs pe)
        {
            var b = sender as Button;
            if (b == null || b.Width <= 2 || b.Height <= 2) return;
            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color clear = (b.AccessibleDescription == "on-card")
                ? SurfaceContainerLow
                : (b.Parent != null ? b.Parent.BackColor : Bg);
            g.Clear(clear);

            string tag = (b.Tag as string) ?? "m3-secondary";
            bool isPrimary = tag == "m3-primary";
            bool isTonalActive = tag == "m3-tonal-active";
            bool hovered = HoveredControls.Contains(b) && b.Enabled;
            bool pressed = PressedControls.Contains(b) && b.Enabled;

            Color fill, fg, border;
            float borderW = 1f;
            if (!b.Enabled)
            {
                fill = Blend(SurfaceContainerHigh, Surface, 0.45f);
                fg = Blend(OnSurfaceVariant, Surface, 0.35f);
                border = Blend(OutlineVariant, Surface, 0.4f);
            }
            else if (isPrimary)
            {
                fill = pressed ? Blend(Btn, OnBtn, 0.22f) : (hovered ? Blend(Btn, Primary, 0.25f) : Btn);
                fg = OnBtn;
                border = hovered ? Primary : BtnEdge;
            }
            else if (isTonalActive)
            {
                fill = pressed ? Blend(PrimaryContainer, Primary, 0.24f) : (hovered ? Blend(PrimaryContainer, Primary, 0.12f) : PrimaryContainer);
                fg = OnPrimaryContainer;
                border = Primary;
                borderW = 1.6f;
            }
            else
            {
                fill = pressed ? PrimaryContainer : (hovered ? Blend(SurfaceContainerHigh, Primary, 0.16f) : SurfaceContainerHigh);
                fg = hovered ? OnSurface : OnSurface;
                border = hovered ? Primary : OutlineVariant;
            }

            RectangleF rect = new RectangleF(1f, 1f, b.Width - 2.5f, b.Height - 2.5f);
            float radius = rect.Height / 2f;
            using (var br = new SolidBrush(fill))
                FillRounded(g, br, rect, radius);
            using (var pen = new Pen(border, borderW))
                DrawRounded(g, pen, rect, radius);

            string name = b.Name ?? "";
            if (name.StartsWith("m3-preset-", StringComparison.Ordinal))
            {
                string schemeId = name.Substring("m3-preset-".Length);
                Color swatch = schemeId == "custom" ? CustomSeed : PresetSwatch(schemeId);
                float d = Math.Max(12f, (float)Math.Round(b.Height * 0.40f));
                float sx = Math.Max(10f, (float)Math.Round(b.Height * 0.28f));
                float sy = (b.Height - d) / 2f;
                using (var sBr = new SolidBrush(swatch))
                    g.FillEllipse(sBr, sx, sy, d, d);
                using (var sPen = new Pen(Color.FromArgb(90, Color.White), 1f))
                    g.DrawEllipse(sPen, sx, sy, d, d);

                if (isTonalActive)
                {
                    DrawVectorIcon(g, "check", new RectangleF(sx + d * 0.15f, sy + d * 0.15f, d * 0.70f, d * 0.70f), Color.White, 1.6f);
                }

                int textLeft = (int)Math.Round(sx + d + 8f);
                Rectangle textRect = new Rectangle(textLeft, 0, Math.Max(8, b.Width - textLeft - 8), b.Height);
                TextRenderer.DrawText(g, b.Text ?? "", b.Font, textRect, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                return;
            }

            if (name == "m3-mode-dark" || name == "m3-mode-light")
            {
                string modeIcon = name == "m3-mode-dark" ? "dark" : "light";
                float isz = Math.Max(13f, b.Height * 0.42f);
                float ix = 14f;
                float iy = (b.Height - isz) / 2f;
                DrawVectorIcon(g, modeIcon, new RectangleF(ix, iy, isz, isz), fg, 1.6f);
                if (isTonalActive)
                {
                    float csz = Math.Max(12f, b.Height * 0.36f);
                    DrawVectorIcon(g, "check", new RectangleF(b.Width - csz - 12f, (b.Height - csz) / 2f, csz, csz), fg, 1.7f);
                }
                Rectangle textRect = new Rectangle((int)(ix + isz + 4f), 0, Math.Max(10, b.Width - (int)(ix + isz) - 26), b.Height);
                TextRenderer.DrawText(g, b.Text ?? "", b.Font, textRect, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                return;
            }

            if (name == "settingsBtn" || name == "refreshBtn")
            {
                string iconKind = name == "settingsBtn" ? "settings" : "refresh";
                float isz = Math.Max(12f, Math.Min(15f, b.Height * 0.45f));
                Size textSz = TextRenderer.MeasureText(g, b.Text ?? "", b.Font, b.Size, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                float totalW = isz + 6f + textSz.Width;
                float startX = Math.Max(8f, (b.Width - totalW) / 2f);
                DrawVectorIcon(g, iconKind, new RectangleF(startX, (b.Height - isz) / 2f, isz, isz), fg, 1.5f);
                Rectangle textRect = new Rectangle((int)Math.Round(startX + isz + 5f), 0, Math.Max(10, b.Width - (int)Math.Round(startX + isz + 8f)), b.Height);
                TextRenderer.DrawText(g, b.Text ?? "", b.Font, textRect, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                return;
            }

            TextRenderer.DrawText(g, b.Text ?? "", b.Font, new Rectangle(6, 0, Math.Max(8, b.Width - 12), b.Height), fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>FlClash-style Navigation Drawer stadium pill item with circular step number/checkmark badge.</summary>
        public static void StyleNavStep(Label lbl, int index, Func<int> getCurrentPage)
        {
            if (lbl == null) return;
            NavIndices[lbl] = index;
            if (getCurrentPage != null) NavPageGetter = getCurrentPage;
            TrackHoverAndPress(lbl);
            lbl.Paint -= OnNavStepPaint;
            lbl.Paint += OnNavStepPaint;
            lbl.Invalidate();
        }

        static void OnNavStepPaint(object sender, PaintEventArgs pe)
        {
            var lbl = sender as Label;
            if (lbl == null || lbl.Width <= 4 || lbl.Height <= 4) return;
            int idx;
            if (!NavIndices.TryGetValue(lbl, out idx)) idx = 0;
            int cur = NavPageGetter != null ? NavPageGetter() : 0;

            bool isActive = idx == cur;
            bool isDone = idx < cur;
            bool isHover = HoveredControls.Contains(lbl);

            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color clear = lbl.Parent != null ? lbl.Parent.BackColor : Panel2;
            g.Clear(clear);

            RectangleF pill = new RectangleF(1f, 2f, lbl.Width - 3f, lbl.Height - 4f);
            float radius = pill.Height / 2f;
            if (isActive)
            {
                using (var br = new SolidBrush(PrimaryContainer))
                    FillRounded(g, br, pill, radius);
                using (var pen = new Pen(Blend(Primary, PrimaryContainer, 0.45f), 1f))
                    DrawRounded(g, pen, pill, radius);
            }
            else if (isHover)
            {
                using (var br = new SolidBrush(SurfaceContainerHigh))
                    FillRounded(g, br, pill, radius);
            }

            float badgeSz = Math.Max(18f, Math.Min(22f, pill.Height - 10f));
            float bx = pill.X + 8f;
            float by = pill.Y + (pill.Height - badgeSz) / 2f;
            RectangleF badgeRect = new RectangleF(bx, by, badgeSz, badgeSz);

            if (isActive)
            {
                using (var bBr = new SolidBrush(Btn))
                    g.FillEllipse(bBr, badgeRect);
                TextRenderer.DrawText(g, (idx + 1).ToString(), SmallBold, Rectangle.Round(badgeRect), OnBtn,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            else if (isDone)
            {
                using (var bBr = new SolidBrush(Blend(Secondary, SurfaceContainerHigh, 0.22f)))
                    g.FillEllipse(bBr, badgeRect);
                using (var bPen = new Pen(Secondary, 1f))
                    g.DrawEllipse(bPen, badgeRect);
                DrawVectorIcon(g, "check", new RectangleF(bx + badgeSz * 0.20f, by + badgeSz * 0.20f, badgeSz * 0.60f, badgeSz * 0.60f), Secondary, 1.6f);
            }
            else
            {
                using (var bPen = new Pen(OutlineVariant, 1f))
                    g.DrawEllipse(bPen, badgeRect);
                TextRenderer.DrawText(g, (idx + 1).ToString(), Small, Rectangle.Round(badgeRect), OnSurfaceVariant,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            string raw = lbl.Text ?? "";
            int dotPos = raw.IndexOf(".  ", StringComparison.Ordinal);
            string stepTitle = dotPos >= 0 ? raw.Substring(dotPos + 3) : raw;

            Color fg = isActive ? OnPrimaryContainer : (isDone ? OnSurface : OnSurfaceVariant);
            Font fnt = isActive ? BodyBold : Body;
            int textLeft = (int)Math.Round(bx + badgeSz + 9f);
            Rectangle textBox = new Rectangle(textLeft, 0, Math.Max(10, lbl.Width - textLeft - 8), lbl.Height);
            TextRenderer.DrawText(g, stepTitle, fnt, textBox, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>Material Design 3 Switch styling for CheckBox controls.</summary>
        public static void StyleCheckBox(CheckBox cb)
        {
            if (cb == null) return;
            cb.Cursor = Cursors.Hand;
            TrackHoverAndPress(cb);
            cb.CheckedChanged -= OnSwitchCheckedChanged;
            cb.CheckedChanged += OnSwitchCheckedChanged;
            cb.Paint -= OnSwitchCheckBoxPaint;
            cb.Paint += OnSwitchCheckBoxPaint;
            cb.Invalidate();
        }

        static void OnSwitchCheckedChanged(object s, EventArgs e)
        {
            var cb = s as Control;
            if (cb != null) cb.Invalidate();
        }

        static void OnSwitchCheckBoxPaint(object sender, PaintEventArgs pe)
        {
            var cb = sender as CheckBox;
            if (cb == null || cb.Width <= 4 || cb.Height <= 4) return;
            var g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color clear = (cb.Tag as string) == "on-card" ? SurfaceContainerLow : cb.BackColor;
            g.Clear(clear);

            float swW = Math.Max(32f, Math.Min(38f, cb.Height * 1.45f));
            float swH = Math.Max(16f, Math.Min(20f, cb.Height * 0.76f));
            float sx = 2f;
            float sy = (cb.Height - swH) / 2f;
            RectangleF track = new RectangleF(sx, sy, swW, swH);
            float radius = swH / 2f;

            bool on = cb.Checked;
            bool hover = HoveredControls.Contains(cb) && cb.Enabled;

            Color trackFill = on ? (hover ? Blend(Btn, Primary, 0.25f) : Btn) : (hover ? SurfaceContainerHigh : SurfaceContainer);
            Color trackBorder = on ? Primary : Outline;
            using (var tBr = new SolidBrush(trackFill))
                FillRounded(g, tBr, track, radius);
            using (var tPen = new Pen(trackBorder, on ? 1f : 1.4f))
                DrawRounded(g, tPen, track, radius);

            float thumbSz = on ? (swH - 5f) : (swH - 7f);
            float tx = on ? (track.Right - thumbSz - 3f) : (track.X + 3.5f);
            float ty = track.Y + (swH - thumbSz) / 2f;
            Color thumbColor = on ? OnBtn : OnSurfaceVariant;
            using (var thBr = new SolidBrush(thumbColor))
                g.FillEllipse(thBr, tx, ty, thumbSz, thumbSz);

            int textLeft = (int)Math.Round(track.Right + 10f);
            Rectangle textBox = new Rectangle(textLeft, 0, Math.Max(10, cb.Width - textLeft - 4), cb.Height);
            TextRenderer.DrawText(g, cb.Text ?? "", cb.Font, textBox, cb.Enabled ? OnSurface : OnSurfaceVariant,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>Material Design 3 flat owner-drawn dropdown ComboBox styling.</summary>
        public static void StyleComboBox(ComboBox cb)
        {
            if (cb == null) return;
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = SurfaceContainerHigh;
            cb.ForeColor = OnSurface;
            cb.DrawMode = DrawMode.OwnerDrawFixed;
            cb.DrawItem -= OnComboBoxDrawItem;
            cb.DrawItem += OnComboBoxDrawItem;
            cb.Invalidate();
        }

        static void OnComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            bool selected = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
            Color bg = selected ? PrimaryContainer : SurfaceContainerHigh;
            Color fg = !cb.Enabled ? OnSurfaceVariant : (selected ? OnPrimaryContainer : OnSurface);

            using (var br = new SolidBrush(bg))
                e.Graphics.FillRectangle(br, e.Bounds);

            if (e.Index >= 0 && e.Index < cb.Items.Count)
            {
                string text = cb.Items[e.Index] != null ? cb.Items[e.Index].ToString() : "";
                Rectangle textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, Math.Max(8, e.Bounds.Width - 12), e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, text, cb.Font, textRect, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>Material Design 3 owner-drawn hardware table ListView styling without 3D bevels.</summary>
        public static void StyleListView(ListView lv)
        {
            if (lv == null) return;
            EnableDoubleBuffering(lv);
            lv.BorderStyle = BorderStyle.None;
            lv.HeaderStyle = ColumnHeaderStyle.None;
            lv.OwnerDraw = true;
            lv.BackColor = SurfaceContainerLow;
            lv.ForeColor = OnSurface;
            lv.DrawColumnHeader -= OnListViewDrawHeader;
            lv.DrawColumnHeader += OnListViewDrawHeader;
            lv.DrawItem -= OnListViewDrawItem;
            lv.DrawItem += OnListViewDrawItem;
            lv.DrawSubItem -= OnListViewDrawSubItem;
            lv.DrawSubItem += OnListViewDrawSubItem;
            lv.Invalidate();
        }

        static void OnListViewDrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (var br = new SolidBrush(SurfaceContainerHigh))
                e.Graphics.FillRectangle(br, e.Bounds);
            using (var pen = new Pen(OutlineVariant, 1f))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            Rectangle textRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, Math.Max(8, e.Bounds.Width - 14), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header != null ? e.Header.Text : "", SmallBold, textRect, OnSurfaceVariant,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        static void OnListViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            Color rowBg = (e.ItemIndex % 2 == 1) ? Blend(SurfaceContainerLow, SurfaceContainer, 0.5f) : SurfaceContainerLow;
            using (var br = new SolidBrush(rowBg))
                e.Graphics.FillRectangle(br, e.Bounds);
            using (var pen = new Pen(Blend(OutlineVariant, rowBg, 0.55f), 1f))
                e.Graphics.DrawLine(pen, e.Bounds.Left + 8, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
        }

        static void OnListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            bool isKeyCol = e.ColumnIndex == 0;
            Color fg = isKeyCol ? Primary : OnSurface;
            Font fnt = isKeyCol ? BodyBold : Fact;
            Rectangle textRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, Math.Max(8, e.Bounds.Width - 14), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem != null ? e.SubItem.Text : "", fnt, textRect, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        public static void Style(ListBox l)
        {
            l.BackColor = SurfaceContainerLow;
            l.ForeColor = OnSurface;
            l.BorderStyle = BorderStyle.FixedSingle;
            l.Font = ListFont;
        }

        /// <summary>Builds the Material Design 3 Theme &amp; Appearance Settings dialog.</summary>
        public static Form CreateSettingsDialog(Form owner = null)
        {
            var dlg = new Form();
            EnableDoubleBuffering(dlg);
            dlg.SuspendLayout();
            dlg.AutoScaleDimensions = new SizeF(96F, 96F);
            dlg.AutoScaleMode = AutoScaleMode.Dpi;
            dlg.Text = Loc.T("Settings");
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
            dlg.ClientSize = new Size(496, 392);
            dlg.Font = Body;

            var heading = new Label
            {
                Text = Loc.T("Theme & Appearance"),
                Font = DialogHeading,
                UseMnemonic = false,
                Bounds = new Rectangle(24, 16, 448, 28),
                BackColor = Color.Transparent
            };
            var subHeading = new Label
            {
                Text = Loc.T("Customize the Material Design 3 color palette and surface mode."),
                Font = Small,
                UseMnemonic = false,
                Bounds = new Rectangle(24, 44, 448, 18),
                BackColor = Color.Transparent
            };
            var paletteLbl = new Label
            {
                Text = Loc.T("Color palette"),
                Font = BodyBold,
                UseMnemonic = false,
                Bounds = new Rectangle(36, 76, 424, 20)
            };

            var presetButtons = new Dictionary<string, Button>();
            int colW = 136, rowH = 36, gapX = 8, gapY = 8;
            for (int i = 0; i < PresetIds.Length; i++)
            {
                string id = PresetIds[i];
                int col = i % 3, row = i / 3;
                var btn = new Button
                {
                    Name = "m3-preset-" + id,
                    Text = Loc.T(PresetLabelKey(id)),
                    AccessibleDescription = "on-card",
                    Bounds = new Rectangle(36 + col * (colW + gapX), 102 + row * (rowH + gapY), colW, rowH)
                };
                Style(btn, false);
                string captured = id;
                btn.Click += (s, e) => SetPalette(captured, IsDark);
                presetButtons[id] = btn;
                dlg.Controls.Add(btn);
            }

            var customBtn = new Button
            {
                Name = "m3-preset-custom",
                Text = Loc.T("Custom color..."),
                AccessibleDescription = "on-card",
                Bounds = new Rectangle(36, 190, 424, 34)
            };
            Style(customBtn, false);
            customBtn.Click += (s, e) =>
            {
                using (var cd = new ColorDialog { Color = CustomSeed, FullOpen = true })
                {
                    if (cd.ShowDialog(dlg) == DialogResult.OK)
                        SetPalette("custom", IsDark, cd.Color);
                }
            };
            dlg.Controls.Add(customBtn);

            var modeLbl = new Label
            {
                Text = Loc.T("Appearance mode"),
                Font = BodyBold,
                UseMnemonic = false,
                Bounds = new Rectangle(36, 250, 424, 20)
            };
            var darkBtn = new Button
            {
                Name = "m3-mode-dark",
                Text = Loc.T("Dark mode"),
                AccessibleDescription = "on-card",
                Bounds = new Rectangle(36, 276, 206, 36)
            };
            var lightBtn = new Button
            {
                Name = "m3-mode-light",
                Text = Loc.T("Light mode"),
                AccessibleDescription = "on-card",
                Bounds = new Rectangle(254, 276, 206, 36)
            };
            Style(darkBtn, false);
            Style(lightBtn, false);
            darkBtn.Click += (s, e) => SetPalette(CurrentScheme, true);
            lightBtn.Click += (s, e) => SetPalette(CurrentScheme, false);

            var resetBtn = new Button
            {
                Name = "m3-reset",
                Text = Loc.T("Reset to default"),
                Bounds = new Rectangle(24, 340, 156, 36)
            };
            resetBtn.Click += (s, e) => ResetToDefault();

            var closeBtn = new Button
            {
                Name = "m3-close",
                Text = Loc.T("Close"),
                DialogResult = DialogResult.OK,
                Bounds = new Rectangle(360, 340, 112, 36)
            };

            dlg.Controls.AddRange(new Control[] { heading, subHeading, paletteLbl, modeLbl, darkBtn, lightBtn, resetBtn, closeBtn });
            dlg.AcceptButton = closeBtn;
            dlg.CancelButton = closeBtn;
            dlg.ResumeLayout(false);
            dlg.PerformLayout();

            dlg.Paint += (s, pe) =>
            {
                float k = dlg.CurrentAutoScaleDimensions.Width / 96F;
                if (k <= 0.5f) k = 1f;
                Func<int, int> sc = v => (int)Math.Round(v * k);
                DrawCard(pe.Graphics, new Rectangle(sc(24), sc(68), sc(448), sc(166)), sc(14), SurfaceContainerLow, OutlineVariant, true);
                DrawCard(pe.Graphics, new Rectangle(sc(24), sc(242), sc(448), sc(82)), sc(14), SurfaceContainerLow, OutlineVariant, false);
            };

            Action refreshDialog = () =>
            {
                dlg.BackColor = Surface;
                dlg.ForeColor = OnSurface;
                heading.ForeColor = Primary;
                subHeading.ForeColor = OnSurfaceVariant;
                paletteLbl.BackColor = SurfaceContainerLow;
                paletteLbl.ForeColor = OnSurface;
                modeLbl.BackColor = SurfaceContainerLow;
                modeLbl.ForeColor = OnSurface;

                foreach (var kv in presetButtons)
                {
                    bool active = CurrentScheme == kv.Key;
                    kv.Value.Tag = active ? "m3-tonal-active" : "m3-secondary";
                    kv.Value.Font = active ? SwatchBold : SwatchRegular;
                    kv.Value.Invalidate();
                }

                bool isCustom = CurrentScheme == "custom";
                customBtn.Tag = isCustom ? "m3-tonal-active" : "m3-secondary";
                customBtn.Font = isCustom ? SwatchBold : SwatchRegular;
                customBtn.Invalidate();

                darkBtn.Tag = IsDark ? "m3-tonal-active" : "m3-secondary";
                darkBtn.Font = IsDark ? BtnBold : BtnRegular;
                darkBtn.Invalidate();

                lightBtn.Tag = !IsDark ? "m3-tonal-active" : "m3-secondary";
                lightBtn.Font = !IsDark ? BtnBold : BtnRegular;
                lightBtn.Invalidate();

                Style(resetBtn, false);
                Style(closeBtn, true);
                dlg.Invalidate();
            };

            refreshDialog();
            Changed += refreshDialog;
            dlg.FormClosed += (s, e) => Changed -= refreshDialog;
            dlg.Disposed += (s, e) => Changed -= refreshDialog;
            return dlg;
        }

        /// <summary>Opens the Material Design 3 Theme &amp; Appearance Settings dialog.</summary>
        public static void ShowSettingsDialog(Form owner)
        {
            using (var dlg = CreateSettingsDialog(owner))
            {
                dlg.ShowDialog(owner);
            }
        }

        /// <summary>The moth mark from the site, embedded in the exe. Null if it is somehow missing; the header
        /// then shows the name alone rather than failing to open.</summary>
        public static Image Mark()
        {
            using (var s = typeof(Theme).Assembly.GetManifestResourceStream("moth-mark.jpg"))
                return s == null ? null : Image.FromStream(new MemoryStream(ReadAll(s)));
        }

        static byte[] ReadAll(Stream s)
        {
            using (var m = new MemoryStream()) { s.CopyTo(m); return m.ToArray(); }
        }
    }

    /// <summary>What the tester was asked to change, so "Find my report" can walk them back: Secure Boot (they turn it
    /// off by hand) and BitLocker (paused by this app). Kept in HKLM; the uninstaller removes it.
    /// The reminders do not depend on these marks: the report page always reads the live Secure Boot and BitLocker
    /// state and says what it sees. A mark only sharpens the wording ("it was on before you scanned").</summary>
    static class Restore
    {
        // These marks used to be files under C:\ProgramData\NullMoth\1401 Probe. Any standard user may create folders in ProgramData, and
        // one made before this app ran is theirs (CREATOR OWNER, full control): they could aim it anywhere with a link,
        // and this app writes and deletes there as admin. Standard users cannot create keys under HKLM\SOFTWARE, so a
        // mark here was written by an admin. Registry64 explicitly, so the path matches the uninstaller's SetRegView 64.
        const string KeyPath = @"SOFTWARE\NullMoth\1401 Probe";

        static Microsoft.Win32.RegistryKey Hklm()
        {
            return Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
        }

        public static string Mark(string what)
        {
            try
            {
                using (var hklm = Hklm())
                using (var k = hklm.CreateSubKey(KeyPath))
                    k.SetValue(what, DateTime.UtcNow.ToString("o"));
                return null;
            }
            catch (Exception e) { return e.Message; }  // the reason goes back to the caller
        }

        public static bool Has(string what)
        {
            try
            {
                using (var hklm = Hklm())
                using (var k = hklm.OpenSubKey(KeyPath))
                    return k != null && k.GetValue(what) != null;
            }
            catch (Exception) { return false; }  // no mark readable = plain wording; the live state is still shown
        }

        public static void Clear(string what)
        {
            try
            {
                using (var hklm = Hklm())
                using (var k = hklm.OpenSubKey(KeyPath, true))
                    if (k != null) k.DeleteValue(what, false);
            }
            catch (Exception) { }  // a mark that outlives its reason only makes a reminder more insistent
        }
    }
}
