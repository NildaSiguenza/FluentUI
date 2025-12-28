using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public class FluentLabel : FluentControlBase
    {
        // 形状相关
        private LabelShape shape = LabelShape.Rectangle;
        private int cornerRadius = 4;

        // 边框相关
        private bool showBorder = false;
        private int borderSize = 1;
        private Color borderColor = Color.Gray;

        // 文本相关
        private string text = string.Empty;
        private ContentAlignment textAlign = ContentAlignment.MiddleLeft;
        private bool autoSize = true;  // 默认启用自动大小
        private Size autoSizeOffset = new Size(8, 4);
        private Padding textPadding = new Padding(4);

        // 链接相关
        private bool isLinkMode = false;
        private List<LinkArea> linkAreas = new List<LinkArea>();
        private LinkArea hoveredLink = null;
        private Color linkColor = Color.Blue;
        private Color activeLinkColor = Color.Red;
        private Color visitedLinkColor = Color.Purple;
        private HashSet<string> visitedLinks = new HashSet<string>();
        private Cursor originalCursor;
        private TextDecoration originalDecoration = TextDecoration.None;

        // 装饰线相关
        private TextDecoration textDecoration = TextDecoration.None;
        private Color decorationColor = Color.Empty;
        private int decorationThickness = 1;
        private int decorationOffset = 2;  // 装饰线与文本的间距

        // 动画显示相关
        private bool enableTypewriterEffect = false;
        private int typewriterSpeed = 50; // 毫秒/字符
        private Timer typewriterTimer;
        private int currentCharIndex = 0;
        private string displayText = string.Empty;

        // 自动消失相关
        private int autoHideDelay = 0; // 0表示不自动消失
        private Timer autoHideTimer;
        private bool isFadingOut = false;

        // 透明背景
        private bool transparentBackground = false;
        private Color baseBackColor;

        public FluentLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            originalCursor = Cursor;
            baseBackColor = BackColor;

            InitializeTimers();
            Size = new Size(100, 23);

            // 默认启用自动大小
            autoSize = true;
        }

        #region 属性

        [Category("外观")]
        [DefaultValue(LabelShape.Rectangle)]
        [Description("标签的形状")]
        public LabelShape Shape
        {
            get => shape;
            set
            {
                if (shape != value)
                {
                    shape = value;
                    RecreateRegion();
                    Invalidate();
                }
            }
        }

        [Category("外观")]
        [DefaultValue(4)]
        [Description("圆角半径(仅在圆角矩形时有效)")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                if (cornerRadius != value)
                {
                    cornerRadius = Math.Max(0, value);
                    if (shape == LabelShape.RoundedRectangle)
                    {
                        RecreateRegion();
                        Invalidate();
                    }
                }
            }
        }

        [Category("边框")]
        [DefaultValue(false)]
        [Description("是否显示边框")]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                if (showBorder != value)
                {
                    showBorder = value;
                    Invalidate();
                }
            }
        }

        [Category("边框")]
        [DefaultValue(1)]
        [Description("边框大小")]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                if (borderSize != value)
                {
                    borderSize = Math.Max(1, value);
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("边框")]
        [DefaultValue(typeof(Color), "Gray")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("外观")]
        [DefaultValue(false)]
        [Description("是否使用透明背景")]
        public bool TransparentBackground
        {
            get => transparentBackground;
            set
            {
                if (transparentBackground != value)
                {
                    transparentBackground = value;
                    if (value)
                    {
                        baseBackColor = BackColor;
                        BackColor = Color.Transparent;
                    }
                    else
                    {
                        BackColor = baseBackColor;
                    }
                    Invalidate();
                }
            }
        }

        [Category("外观")]
        [Description("文本内容")]
        public override string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value ?? string.Empty;

                    if (enableTypewriterEffect && !DesignMode)
                    {
                        StartTypewriterEffect();
                    }
                    else
                    {
                        displayText = text;
                    }

                    // 如果是链接模式, 重新解析链接
                    if (isLinkMode)
                    {
                        ParseLinks();
                    }

                    if (autoSize)
                    {
                        AdjustSize();
                    }

                    Invalidate();
                }
            }
        }

        [Category("外观")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        [Description("文本对齐方式")]
        public ContentAlignment TextAlign
        {
            get => textAlign;
            set
            {
                if (textAlign != value)
                {
                    textAlign = value;
                    Invalidate();
                }
            }
        }

        // 重写AutoSize属性, 确保它在设计器中可见
        [Category("布局")]
        [DefaultValue(true)]
        [Description("是否自动调整大小")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get => autoSize;
            set
            {
                if (autoSize != value)
                {
                    autoSize = value;
                    if (value)
                    {
                        AdjustSize();
                    }
                    // 触发AutoSizeChanged事件
                    OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Category("外观")]
        [Description("文本内边距")]
        public Padding TextPadding
        {
            get => textPadding;
            set
            {
                if (textPadding != value)
                {
                    textPadding = value;
                    if (autoSize)
                    {
                        AdjustSize();
                    }
                    Invalidate();
                }
            }
        }

        #endregion

        #region 链接模式属性

        [Category("链接")]
        [DefaultValue(false)]
        [Description("是否启用链接模式")]
        public bool IsLinkMode
        {
            get => isLinkMode;
            set
            {
                if (isLinkMode != value)
                {
                    isLinkMode = value;

                    if (value)
                    {
                        // 启用链接模式
                        ParseLinks();
                        originalDecoration = textDecoration;
                        textDecoration = TextDecoration.Underline;
                        Cursor = Cursors.Hand;
                    }
                    else
                    {
                        // 关闭链接模式
                        linkAreas.Clear();
                        hoveredLink = null;
                        textDecoration = originalDecoration;
                        Cursor = originalCursor;
                    }
                    Invalidate();
                }
            }
        }

        [Category("链接")]
        [DefaultValue(typeof(Color), "Blue")]
        [Description("链接颜色")]
        public Color LinkColor
        {
            get => linkColor;
            set
            {
                if (linkColor != value)
                {
                    linkColor = value;
                    if (isLinkMode)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("链接")]
        [DefaultValue(typeof(Color), "Red")]
        [Description("活动链接颜色")]
        public Color ActiveLinkColor
        {
            get => activeLinkColor;
            set
            {
                if (activeLinkColor != value)
                {
                    activeLinkColor = value;
                    if (isLinkMode)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("链接")]
        [DefaultValue(typeof(Color), "Purple")]
        [Description("已访问链接颜色")]
        public Color VisitedLinkColor
        {
            get => visitedLinkColor;
            set
            {
                if (visitedLinkColor != value)
                {
                    visitedLinkColor = value;
                    if (isLinkMode)
                    {
                        Invalidate();
                    }
                }
            }
        }

        #endregion

        #region 文本装饰属性

        [Category("文本装饰")]
        [DefaultValue(TextDecoration.None)]
        [Description("文本装饰线类型")]
        public TextDecoration TextDecoration
        {
            get => textDecoration;
            set
            {
                if (textDecoration != value)
                {
                    textDecoration = value;
                    if (!isLinkMode)
                    {
                        originalDecoration = value;
                    }
                    Invalidate();
                }
            }
        }

        [Category("文本装饰")]
        [Description("装饰线颜色(空则使用文本颜色)")]
        public Color DecorationColor
        {
            get => decorationColor;
            set
            {
                if (decorationColor != value)
                {
                    decorationColor = value;
                    Invalidate();
                }
            }
        }

        [Category("文本装饰")]
        [DefaultValue(1)]
        [Description("装饰线粗细")]
        public int DecorationThickness
        {
            get => decorationThickness;
            set
            {
                if (decorationThickness != value)
                {
                    decorationThickness = Math.Max(1, value);
                    Invalidate();
                }
            }
        }

        [Category("文本装饰")]
        [DefaultValue(2)]
        [Description("装饰线与文本的间距")]
        public int DecorationOffset
        {
            get => decorationOffset;
            set
            {
                if (decorationOffset != value)
                {
                    decorationOffset = Math.Max(0, value);
                    Invalidate();
                }
            }
        }

        #endregion

        #region 动画效果属性

        [Category("动画")]
        [DefaultValue(false)]
        [Description("是否启用打字机效果")]
        public bool EnableTypewriterEffect
        {
            get => enableTypewriterEffect;
            set
            {
                if (enableTypewriterEffect != value)
                {
                    enableTypewriterEffect = value;
                    if (value && !string.IsNullOrEmpty(text) && !DesignMode)
                    {
                        StartTypewriterEffect();
                    }
                }
            }
        }

        [Category("动画")]
        [DefaultValue(50)]
        [Description("打字机效果速度(毫秒/字符)")]
        public int TypewriterSpeed
        {
            get => typewriterSpeed;
            set
            {
                typewriterSpeed = Math.Max(10, value);
                if (typewriterTimer != null)
                {
                    typewriterTimer.Interval = typewriterSpeed;
                }
            }
        }

        [Category("动画")]
        [DefaultValue(0)]
        [Description("自动隐藏延迟(毫秒, 0表示不自动隐藏)")]
        public int AutoHideDelay
        {
            get => autoHideDelay;
            set
            {
                autoHideDelay = Math.Max(0, value);
                if (autoHideDelay > 0 && !DesignMode)
                {
                    StartAutoHideTimer();
                }
            }
        }

        #endregion

        #region 事件

        public event EventHandler<LinkClickedEventArgs> LinkClicked;
        public event EventHandler TypewriterCompleted;
        public event EventHandler AutoHidden;

        #endregion

        #region 初始化

        private void InitializeTimers()
        {
            // 打字机效果定时器
            typewriterTimer = new Timer();
            typewriterTimer.Interval = typewriterSpeed;
            typewriterTimer.Tick += OnTypewriterTick;

            // 自动隐藏定时器
            autoHideTimer = new Timer();
            autoHideTimer.Tick += OnAutoHideTick;
        }

        #endregion

        #region 主题应用

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
        }

        protected override void InitializeDefaultStyles()
        {
            base.InitializeDefaultStyles();
            BackColor = Color.Transparent;
            ForeColor = SystemColors.ControlText;
        }

        protected override void ApplyThemeStyles()
        {
            if (Theme == null)
            {
                return;
            }

            ForeColor = GetThemeColor(c => c.TextPrimary, ForeColor);

            if (!transparentBackground)
            {
                BackColor = GetThemeColor(c => c.Surface, BackColor);
            }

            if (UseTheme && Theme.Elevation != null)
            {
                cornerRadius = Theme.Elevation.CornerRadius;
            }

            Font = GetThemeFont(t => t.Body, Font);
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            if (transparentBackground)
            {
                return;
            }

            var rect = ClientRectangle;
            Color bgColor = BackColor;

            if (UseTheme && Theme != null)
            {
                switch (State)
                {
                    case ControlState.Hover:
                        bgColor = GetThemeColor(c => c.SurfaceHover, bgColor);
                        break;
                    case ControlState.Pressed:
                        bgColor = GetThemeColor(c => c.SurfacePressed, bgColor);
                        break;
                }
            }

            using (var brush = new SolidBrush(bgColor))
            {
                switch (shape)
                {
                    case LabelShape.Rectangle:
                        g.FillRectangle(brush, rect);
                        break;

                    case LabelShape.RoundedRectangle:
                        using (var path = GetRoundedRectangle(rect, cornerRadius))
                        {
                            g.FillPath(brush, path);
                        }
                        break;

                    case LabelShape.Ellipse:
                        g.FillEllipse(brush, rect);
                        break;

                    case LabelShape.Circle:
                        var size = Math.Min(rect.Width, rect.Height);
                        var circleRect = new Rectangle(
                            rect.X + (rect.Width - size) / 2,
                            rect.Y + (rect.Height - size) / 2,
                            size, size);
                        g.FillEllipse(brush, circleRect);
                        break;
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (string.IsNullOrEmpty(displayText))
            {
                return;
            }

            var textRect = GetTextRectangle();

            if (isLinkMode && linkAreas.Count > 0)
            {
                DrawLinkedText(g, textRect);
            }
            else if (isLinkMode && linkAreas.Count == 0)
            {
                // 如果是链接模式但没有解析到链接, 作为整体链接处理
                DrawWholeAsLink(g, textRect);
            }
            else
            {
                DrawNormalText(g, textRect);
            }
        }

        private void DrawNormalText(Graphics g, Rectangle textRect)
        {
            Color textColor = ForeColor;

            if (UseTheme && Theme != null)
            {
                textColor = State == ControlState.Disabled
                    ? GetThemeColor(c => c.TextDisabled, textColor)
                    : GetThemeColor(c => c.TextPrimary, textColor);
            }

            using (var brush = new SolidBrush(textColor))
            {
                var format = GetStringFormat();
                g.DrawString(displayText, Font, brush, textRect, format);

                // 绘制装饰线
                if (textDecoration != TextDecoration.None)
                {
                    DrawTextDecoration(g, displayText, textRect, textColor);
                }
            }
        }

        private void DrawWholeAsLink(Graphics g, Rectangle textRect)
        {
            Color textColor = linkColor;

            // 如果整个文本是链接
            if (hoveredLink != null)
            {
                textColor = activeLinkColor;
            }
            else if (visitedLinks.Contains(text))
            {
                textColor = visitedLinkColor;
            }

            using (var brush = new SolidBrush(textColor))
            {
                var format = GetStringFormat();
                g.DrawString(displayText, Font, brush, textRect, format);

                // 绘制装饰线
                if (textDecoration != TextDecoration.None)
                {
                    DrawTextDecoration(g, displayText, textRect, textColor);
                }
            }
        }

        private void DrawLinkedText(Graphics g, Rectangle textRect)
        {
            var format = GetStringFormat();
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

            int startIndex = 0;

            foreach (var link in linkAreas.OrderBy(l => l.Start))
            {
                // 绘制链接前的普通文本
                if (startIndex < link.Start)
                {
                    string normalText = displayText.Substring(startIndex, link.Start - startIndex);
                    DrawTextPart(g, normalText, textRect, ForeColor, startIndex, format);
                }

                // 绘制链接文本
                string linkText = displayText.Substring(link.Start, Math.Min(link.Length, displayText.Length - link.Start));
                Color linkTextColor = GetLinkColor(link);
                DrawTextPart(g, linkText, textRect, linkTextColor, link.Start, format);

                // 为链接添加下划线
                if (textDecoration == TextDecoration.Underline)
                {
                    DrawLinkUnderline(g, linkText, textRect, linkTextColor, link.Start);
                }

                startIndex = link.Start + link.Length;
            }

            // 绘制最后的普通文本
            if (startIndex < displayText.Length)
            {
                string remainingText = displayText.Substring(startIndex);
                DrawTextPart(g, remainingText, textRect, ForeColor, startIndex, format);
            }
        }

        private void DrawTextPart(Graphics g, string text, Rectangle textRect, Color color, int startIndex, StringFormat format)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            using (var brush = new SolidBrush(color))
            {
                // 计算文本起始位置
                string beforeText = startIndex > 0 ? displayText.Substring(0, startIndex) : "";
                var beforeSize = g.MeasureString(beforeText, Font, textRect.Width, format);

                // 根据对齐方式调整位置
                float x = textRect.X;
                float y = textRect.Y;

                if (format.Alignment == StringAlignment.Near)
                {
                    x += beforeSize.Width;
                }

                var partRect = new RectangleF(x, y, textRect.Width - beforeSize.Width, textRect.Height);
                g.DrawString(text, Font, brush, partRect, format);
            }
        }

        private void DrawTextDecoration(Graphics g, string text, Rectangle textRect, Color color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var format = GetStringFormat();
            var textSize = g.MeasureString(text, Font, textRect.Size, format);
            var decorColor = decorationColor.IsEmpty ? color : decorationColor;

            // 计算文本实际位置
            PointF textLocation = GetTextLocation(textRect, textSize, format);

            using (var pen = new Pen(decorColor, decorationThickness))
            {
                float x = textLocation.X;
                float y = textLocation.Y;
                float width = textSize.Width;
                float textHeight = g.MeasureString("Ag", Font).Height; // 使用Ag来获取准确的文本高度

                switch (textDecoration)
                {
                    case TextDecoration.Underline:
                        // 下划线：在文本基线下方
                        y = y + textHeight - decorationOffset;
                        g.DrawLine(pen, x, y, x + width, y);
                        break;

                    case TextDecoration.Overline:
                        // 上划线：在文本顶部上方
                        y = y + decorationOffset;
                        g.DrawLine(pen, x, y, x + width, y);
                        break;

                    case TextDecoration.Strikethrough:
                        // 删除线：在文本中间
                        y = y + textHeight / 2;
                        g.DrawLine(pen, x, y, x + width, y);
                        break;
                }
            }
        }

        private void DrawLinkUnderline(Graphics g, string text, Rectangle textRect, Color color, int startIndex)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var format = GetStringFormat();
            string beforeText = startIndex > 0 ? displayText.Substring(0, startIndex) : "";
            var beforeSize = g.MeasureString(beforeText, Font, textRect.Width, format);
            var linkSize = g.MeasureString(text, Font, textRect.Width, format);

            // 获取文本实际位置
            var fullTextSize = g.MeasureString(displayText, Font, textRect.Size, format);
            PointF textLocation = GetTextLocation(textRect, fullTextSize, format);

            using (var pen = new Pen(color, decorationThickness))
            {
                float x = textLocation.X + beforeSize.Width;
                float textHeight = g.MeasureString("Ag", Font).Height;
                float y = textLocation.Y + textHeight - decorationOffset;

                g.DrawLine(pen, x, y, x + linkSize.Width, y);
            }
        }

        private PointF GetTextLocation(Rectangle textRect, SizeF textSize, StringFormat format)
        {
            float x = textRect.X;
            float y = textRect.Y;

            // 水平位置
            switch (format.Alignment)
            {
                case StringAlignment.Center:
                    x = textRect.X + (textRect.Width - textSize.Width) / 2;
                    break;
                case StringAlignment.Far:
                    x = textRect.Right - textSize.Width;
                    break;
                default: // Near
                    x = textRect.X;
                    break;
            }

            // 垂直位置
            switch (format.LineAlignment)
            {
                case StringAlignment.Center:
                    y = textRect.Y + (textRect.Height - textSize.Height) / 2;
                    break;
                case StringAlignment.Far:
                    y = textRect.Bottom - textSize.Height;
                    break;
                default: // Near
                    y = textRect.Y;
                    break;
            }

            return new PointF(x, y);
        }

        protected override void DrawBorder(Graphics g)
        {
            if (!showBorder)
            {
                return;
            }

            Color borderCol = borderColor;

            if (UseTheme && Theme != null && borderColor == Color.Gray)
            {
                borderCol = GetThemeColor(c => c.Border, borderColor);
            }

            using (var pen = new Pen(borderCol, borderSize))
            {
                var rect = new Rectangle(
                    borderSize / 2,
                    borderSize / 2,
                    Width - borderSize,
                    Height - borderSize);

                switch (shape)
                {
                    case LabelShape.Rectangle:
                        g.DrawRectangle(pen, rect);
                        break;

                    case LabelShape.RoundedRectangle:
                        using (var path = GetRoundedRectangle(rect, cornerRadius))
                        {
                            g.DrawPath(pen, path);
                        }
                        break;

                    case LabelShape.Ellipse:
                        g.DrawEllipse(pen, rect);
                        break;

                    case LabelShape.Circle:
                        var size = Math.Min(rect.Width, rect.Height);
                        var circleRect = new Rectangle(
                            rect.X + (rect.Width - size) / 2,
                            rect.Y + (rect.Height - size) / 2,
                            size, size);
                        g.DrawEllipse(pen, circleRect);
                        break;
                }
            }
        }

        #endregion

        #region 形状处理

        private void RecreateRegion()
        {
            if (Region != null)
            {
                Region.Dispose();
                Region = null;
            }

            var rect = ClientRectangle;

            switch (shape)
            {
                case LabelShape.RoundedRectangle:
                    using (var path = GetRoundedRectangle(rect, cornerRadius))
                    {
                        Region = new Region(path);
                    }
                    break;

                case LabelShape.Ellipse:
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(rect);
                        Region = new Region(path);
                    }
                    break;

                case LabelShape.Circle:
                    var size = Math.Min(rect.Width, rect.Height);
                    var circleRect = new Rectangle(
                        (rect.Width - size) / 2,
                        (rect.Height - size) / 2,
                        size, size);
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(circleRect);
                        Region = new Region(path);
                    }
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecreateRegion();
        }

        #endregion

        #region 链接处理

        private void ParseLinks()
        {
            linkAreas.Clear();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // 简单的URL检测模式
            var urlPattern = @"https?://[^\s]+|www\.[^\s]+";
            var matches = Regex.Matches(text, urlPattern);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    linkAreas.Add(new LinkArea
                    {
                        Start = match.Index,
                        Length = match.Length,
                        Url = match.Value
                    });
                }
            }
            else
            {
                // 如果没有找到URL, 将整个文本作为链接
                linkAreas.Add(new LinkArea
                {
                    Start = 0,
                    Length = text.Length,
                    Url = text
                });
            }
        }

        public void AddLink(int start, int length, string url = null)
        {
            linkAreas.Add(new LinkArea
            {
                Start = start,
                Length = length,
                Url = url ?? text.Substring(start, Math.Min(length, text.Length - start))
            });
            Invalidate();
        }

        public void ClearLinks()
        {
            linkAreas.Clear();
            hoveredLink = null;
            Invalidate();
        }

        private LinkArea GetLinkAtPoint(Point point)
        {
            if (!isLinkMode)
            {
                return null;
            }

            // 如果没有特定链接区域, 整个标签都是链接
            if (linkAreas.Count == 0 || (linkAreas.Count == 1 && linkAreas[0].Start == 0 && linkAreas[0].Length == text.Length))
            {
                var textRect = GetTextRectangle();
                if (textRect.Contains(point))
                {
                    if (linkAreas.Count == 0)
                    {
                        return new LinkArea { Start = 0, Length = text.Length, Url = text };
                    }
                    return linkAreas[0];
                }
                return null;
            }

            using (var g = CreateGraphics())
            {
                var textRect = GetTextRectangle();
                var format = GetStringFormat();
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                foreach (var link in linkAreas)
                {
                    string beforeText = link.Start > 0 ? displayText.Substring(0, link.Start) : "";
                    var beforeSize = g.MeasureString(beforeText, Font, textRect.Width, format);

                    string linkText = displayText.Substring(link.Start, Math.Min(link.Length, displayText.Length - link.Start));
                    var linkSize = g.MeasureString(linkText, Font, textRect.Width, format);

                    var linkRect = new RectangleF(
                        textRect.X + beforeSize.Width,
                        textRect.Y,
                        linkSize.Width,
                        textRect.Height);

                    if (linkRect.Contains(point))
                    {
                        return link;
                    }
                }
            }

            return null;
        }

        private Color GetLinkColor(LinkArea link)
        {
            if (link == hoveredLink)
            {
                return activeLinkColor;
            }

            if (visitedLinks.Contains(link.Url))
            {
                return visitedLinkColor;
            }

            return linkColor;
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!isLinkMode)
            {
                return;
            }

            var link = GetLinkAtPoint(e.Location);

            if (link != hoveredLink)
            {
                hoveredLink = link;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredLink != null)
            {
                hoveredLink = null;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!isLinkMode || e.Button != MouseButtons.Left)
            {
                return;
            }

            var link = GetLinkAtPoint(e.Location);
            if (link != null)
            {
                visitedLinks.Add(link.Url);
                LinkClicked?.Invoke(this, new LinkClickedEventArgs(link.Url));
                Invalidate();
            }
        }

        #endregion

        #region 打字机效果

        private void StartTypewriterEffect()
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            currentCharIndex = 0;
            displayText = string.Empty;
            typewriterTimer.Start();
        }

        private void OnTypewriterTick(object sender, EventArgs e)
        {
            if (currentCharIndex < text.Length)
            {
                displayText += text[currentCharIndex];
                currentCharIndex++;

                if (autoSize)
                {
                    AdjustSize();
                }

                Invalidate();
            }
            else
            {
                typewriterTimer.Stop();
                TypewriterCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RestartTypewriter()
        {
            if (enableTypewriterEffect)
            {
                StartTypewriterEffect();
            }
        }

        #endregion

        #region 自动隐藏

        private void StartAutoHideTimer()
        {
            if (autoHideDelay <= 0)
            {
                return;
            }

            autoHideTimer.Stop();
            autoHideTimer.Interval = autoHideDelay;
            autoHideTimer.Start();
        }

        private void OnAutoHideTick(object sender, EventArgs e)
        {
            autoHideTimer.Stop();

            if (EnableAnimation)
            {
                // 使用淡出动画
                isFadingOut = true;
                var fadeSteps = 20;
                var fadeInterval = 50;
                var currentStep = 0;

                var fadeTimer = new Timer { Interval = fadeInterval };
                fadeTimer.Tick += (s, args) =>
                {
                    currentStep++;
                    var progress = (float)currentStep / fadeSteps;

                    if (this.Parent != null)
                    {
                        // 通过重绘实现淡出效果
                        this.Invalidate();
                    }

                    if (currentStep >= fadeSteps)
                    {
                        fadeTimer.Stop();
                        fadeTimer.Dispose();
                        this.Visible = false;
                        isFadingOut = false;
                        AutoHidden?.Invoke(this, EventArgs.Empty);
                    }
                };
                fadeTimer.Start();
            }
            else
            {
                this.Visible = false;
                AutoHidden?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ResetAutoHide()
        {
            if (autoHideDelay > 0)
            {
                StartAutoHideTimer();
            }
        }

        #endregion

        #region 辅助方法

        private Rectangle GetTextRectangle()
        {
            var rect = ClientRectangle;

            if (shape == LabelShape.Circle)
            {
                var size = Math.Min(rect.Width, rect.Height);
                return new Rectangle(
                    rect.X + (rect.Width - size) / 2 + textPadding.Left,
                    rect.Y + (rect.Height - size) / 2 + textPadding.Top,
                    size - textPadding.Horizontal,
                    size - textPadding.Vertical);
            }

            return new Rectangle(
                rect.X + textPadding.Left,
                rect.Y + textPadding.Top,
                rect.Width - textPadding.Horizontal,
                rect.Height - textPadding.Vertical);
        }

        private StringFormat GetStringFormat()
        {
            var format = new StringFormat();

            // 水平对齐
            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    break;
            }

            // 垂直对齐
            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    format.LineAlignment = StringAlignment.Far;
                    break;
            }

            format.Trimming = StringTrimming.EllipsisCharacter;

            return format;
        }

        private void AdjustSize()
        {
            if (!autoSize || string.IsNullOrEmpty(displayText))
            {
                return;
            }

            using (var g = CreateGraphics())
            {
                var textSize = g.MeasureString(displayText, Font);

                int newWidth = (int)Math.Ceiling(textSize.Width) + textPadding.Horizontal + autoSizeOffset.Width;
                int newHeight = (int)Math.Ceiling(textSize.Height) + textPadding.Vertical + autoSizeOffset.Height;

                // 如果有边框, 增加边框大小
                if (showBorder)
                {
                    newWidth += borderSize * 2;
                    newHeight += borderSize * 2;
                }

                Size = new Size(newWidth, newHeight);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (autoSize)
            {
                AdjustSize();
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                typewriterTimer?.Dispose();
                autoHideTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

    }


    #region 枚举和辅助类

    public class LinkArea
    {
        public int Start { get; set; }

        public int Length { get; set; }

        public string Url { get; set; }
    }

    public class LinkClickedEventArgs : EventArgs
    {
        public LinkClickedEventArgs(string url)
        {
            Url = url;
        }

        public string Url { get; }

    }

    /// <summary>
    /// 标签形状
    /// </summary>
    public enum LabelShape
    {
        Rectangle,
        RoundedRectangle,
        Ellipse,
        Circle
    }

    /// <summary>
    /// 文本装饰
    /// </summary>
    public enum TextDecoration
    {
        None,
        Underline,
        Overline,
        Strikethrough
    }

    #endregion
}
