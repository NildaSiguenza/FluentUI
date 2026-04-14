using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using FluentControls.IconFonts;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentGroupBoxDesigner))]
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class FluentGroupBox : FluentContainerBase
    {
        #region 字段

        // Header相关
        private string headerText = "GroupBox";
        private Image headerIcon = null;
        private Font headerFont = null;
        private Color headerForeColor = Color.Empty;
        private Color headerBackColor = Color.Empty;
        private HeaderPosition headerPosition = HeaderPosition.Top;
        private HeaderTextAlignment headerTextAlign = HeaderTextAlignment.Left;
        private int headerHeight = 26;
        private int headerIconSize = 16;
        private int headerIconTextSpacing = 4;
        private Padding headerPadding = new Padding(10, 4, 10, 4);
        private bool showHeader = true;

        // Border相关
        private Color borderColor = Color.FromArgb(200, 200, 200);
        private int borderWidth = 1;
        private DashStyle borderDashStyle = DashStyle.Solid;
        private int cornerRadius = 4;
        private bool showTopBorder = true;
        private bool showBottomBorder = true;
        private bool showLeftBorder = true;
        private bool showRightBorder = true;

        // 内容区域
        private FluentGroupBoxContentPanel contentPanel;
        private Padding contentPadding = new Padding(6, 4, 6, 4);

        // 滚动相关
        private bool enableScrolling = false;
        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;
        private int scrollBarSize = 17;
        private bool autoShowScrollBars = true;

        // 内部状态
        private Rectangle headerRect;
        private Rectangle contentRect;
        private Rectangle borderRect;

        #endregion

        #region 构造函数

        public FluentGroupBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.ContainerControl,
                true);

            this.Size = new Size(300, 200);
            this.Padding = new Padding(1);
            this.BackColor = Color.Transparent;
            this.EnableRippleEffect = false;

            InitializeComponents();
            CalculateRects();
        }

        private void InitializeComponents()
        {
            // 创建内容面板 - 使用自定义面板类
            contentPanel = new FluentGroupBoxContentPanel(this)
            {
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            contentPanel.ControlAdded += ContentPanel_ControlAdded;
            contentPanel.ControlRemoved += ContentPanel_ControlRemoved;

            // 使用 base.Controls 添加到内部控件集合
            base.Controls.Add(contentPanel);

            // 创建垂直滚动条
            vScrollBar = new VScrollBar
            {
                Visible = false,
                SmallChange = 20,
                LargeChange = 100
            };
            vScrollBar.Scroll += VScrollBar_Scroll;
            base.Controls.Add(vScrollBar);

            // 创建水平滚动条
            hScrollBar = new HScrollBar
            {
                Visible = false,
                SmallChange = 20,
                LargeChange = 100
            };
            hScrollBar.Scroll += HScrollBar_Scroll;
            base.Controls.Add(hScrollBar);
        }

        #endregion

        #region 属性

        /// <summary>
        /// 内容面板(用于添加子控件)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentGroupBoxContentPanel ContentPanel => contentPanel;

        /// <summary>
        /// 内容面板的控件集合
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control.ControlCollection ContentControls => contentPanel?.Controls;

        [Category("Header")]
        [Description("GroupBox的标题文本")]
        [DefaultValue("GroupBox")]
        [Localizable(true)]
        public override string Text
        {
            get => headerText;
            set
            {
                if (headerText != value)
                {
                    headerText = value;
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("显示在标题文本前的图标")]
        [DefaultValue(null)]
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image HeaderIcon
        {
            get => headerIcon;
            set
            {
                if (headerIcon != value)
                {
                    headerIcon = value;
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("标题文本的字体")]
        [AmbientValue(null)]
        public Font HeaderFont
        {
            get => headerFont ?? Font;
            set
            {
                headerFont = value;
                CalculateRects();
                Invalidate();
            }
        }

        [Category("Header")]
        [Description("标题文本的颜色")]
        public Color HeaderForeColor
        {
            get => headerForeColor.IsEmpty ? ForeColor : headerForeColor;
            set
            {
                headerForeColor = value;
                Invalidate();
            }
        }

        [Category("Header")]
        [Description("标题区域的背景色")]
        public Color HeaderBackColor
        {
            get => headerBackColor;
            set
            {
                headerBackColor = value;
                Invalidate();
            }
        }

        [Category("Header")]
        [Description("标题栏的位置")]
        [DefaultValue(HeaderPosition.Top)]
        [TypeConverter(typeof(HeaderPositionConverter))]
        public HeaderPosition HeaderPosition
        {
            get => headerPosition;
            set
            {
                if (headerPosition != value)
                {
                    headerPosition = value;
                    CalculateRects();
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("标题文本的对齐方式")]
        [DefaultValue(HeaderTextAlignment.Left)]
        [TypeConverter(typeof(HeaderTextAlignmentConverter))]
        public HeaderTextAlignment HeaderTextAlign
        {
            get => headerTextAlign;
            set
            {
                if (headerTextAlign != value)
                {
                    headerTextAlign = value;
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("标题栏的高度(用于Top/Bottom位置)或宽度(用于Left/Right位置)")]
        [DefaultValue(26)]
        public int HeaderHeight
        {
            get => headerHeight;
            set
            {
                if (headerHeight != value && value > 0)
                {
                    headerHeight = value;
                    CalculateRects();
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("标题图标的大小")]
        [DefaultValue(16)]
        public int HeaderIconSize
        {
            get => headerIconSize;
            set
            {
                if (headerIconSize != value && value > 0)
                {
                    headerIconSize = value;
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("标题图标与文本之间的间距")]
        [DefaultValue(4)]
        public int HeaderIconTextSpacing
        {
            get => headerIconTextSpacing;
            set
            {
                if (headerIconTextSpacing != value && value >= 0)
                {
                    headerIconTextSpacing = value;
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("标题区域的内边距")]
        public Padding HeaderPadding
        {
            get => headerPadding;
            set
            {
                if (headerPadding != value)
                {
                    headerPadding = value;
                    Invalidate();
                }
            }
        }

        [Category("Header")]
        [Description("是否显示标题区域")]
        [DefaultValue(true)]
        public bool ShowHeader
        {
            get => showHeader;
            set
            {
                if (showHeader != value)
                {
                    showHeader = value;
                    CalculateRects();
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("边框的颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("边框的宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth != value && value >= 0)
                {
                    borderWidth = value;
                    CalculateRects();
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("边框的线型样式")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle BorderDashStyle
        {
            get => borderDashStyle;
            set
            {
                if (borderDashStyle != value)
                {
                    borderDashStyle = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("边框的圆角半径")]
        [DefaultValue(4)]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                if (cornerRadius != value && value >= 0)
                {
                    cornerRadius = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("是否显示顶部边框")]
        [DefaultValue(true)]
        public bool ShowTopBorder
        {
            get => showTopBorder;
            set
            {
                if (showTopBorder != value)
                {
                    showTopBorder = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("是否显示底部边框")]
        [DefaultValue(true)]
        public bool ShowBottomBorder
        {
            get => showBottomBorder;
            set
            {
                if (showBottomBorder != value)
                {
                    showBottomBorder = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("是否显示左侧边框")]
        [DefaultValue(true)]
        public bool ShowLeftBorder
        {
            get => showLeftBorder;
            set
            {
                if (showLeftBorder != value)
                {
                    showLeftBorder = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("是否显示右侧边框")]
        [DefaultValue(true)]
        public bool ShowRightBorder
        {
            get => showRightBorder;
            set
            {
                if (showRightBorder != value)
                {
                    showRightBorder = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAllBorders
        {
            get => showTopBorder && showBottomBorder && showLeftBorder && showRightBorder;
            set
            {
                showTopBorder = value;
                showBottomBorder = value;
                showLeftBorder = value;
                showRightBorder = value;
                Invalidate();
            }
        }

        [Category("Content")]
        [Description("内容区域的内边距")]
        public Padding ContentPadding
        {
            get => contentPadding;
            set
            {
                if (contentPadding != value)
                {
                    contentPadding = value;
                    UpdateLayout();
                }
            }
        }

        [Category("Scrolling")]
        [Description("是否启用滚动模式")]
        [DefaultValue(false)]
        public bool EnableScrolling
        {
            get => enableScrolling;
            set
            {
                if (enableScrolling != value)
                {
                    enableScrolling = value;
                    UpdateScrollBars();
                    Invalidate();
                }
            }
        }

        [Category("Scrolling")]
        [Description("内容超出时是否自动显示滚动条")]
        [DefaultValue(true)]
        public bool AutoShowScrollBars
        {
            get => autoShowScrollBars;
            set
            {
                if (autoShowScrollBars != value)
                {
                    autoShowScrollBars = value;
                    UpdateScrollBars();
                }
            }
        }

        [Category("Scrolling")]
        [Description("滚动条的宽度/高度")]
        [DefaultValue(17)]
        public int ScrollBarSize
        {
            get => scrollBarSize;
            set
            {
                if (scrollBarSize != value && value > 0)
                {
                    scrollBarSize = value;
                    UpdateScrollBars();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new ControlCollection Controls => contentPanel?.Controls ?? base.Controls;

        public override Rectangle DisplayRectangle
        {
            get
            {
                if (contentPanel != null)
                {
                    return new Rectangle(
                        contentPanel.Left + contentPadding.Left,
                        contentPanel.Top + contentPadding.Top,
                        contentPanel.Width - contentPadding.Horizontal,
                        contentPanel.Height - contentPadding.Vertical);
                }
                return contentRect;
            }
        }

        /// <summary>
        /// 隐藏基类的 Padding 属性
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        #endregion

        #region 序列化控制

        private bool ShouldSerializeHeaderFont()
        {
            return headerFont != null;
        }

        private void ResetHeaderFont()
        {
            headerFont = null;
            Invalidate();
        }

        private bool ShouldSerializeHeaderForeColor()
        {
            return !headerForeColor.IsEmpty;
        }

        private void ResetHeaderForeColor()
        {
            headerForeColor = Color.Empty;
            Invalidate();
        }

        private bool ShouldSerializeHeaderBackColor()
        {
            return !headerBackColor.IsEmpty;
        }

        private void ResetHeaderBackColor()
        {
            headerBackColor = Color.Empty;
            Invalidate();
        }

        private bool ShouldSerializeBorderColor()
        {
            return borderColor != Color.FromArgb(200, 200, 200);
        }

        private void ResetBorderColor()
        {
            borderColor = Color.FromArgb(200, 200, 200);
            Invalidate();
        }

        private bool ShouldSerializeContentPadding()
        {
            return contentPadding != new Padding(6, 4, 6, 4);
        }

        private void ResetContentPadding()
        {
            contentPadding = new Padding(6, 4, 6, 4);
            UpdateLayout();
        }

        private bool ShouldSerializeHeaderPadding()
        {
            return headerPadding != new Padding(10, 4, 10, 4);
        }

        private void ResetHeaderPadding()
        {
            headerPadding = new Padding(10, 4, 10, 4);
            Invalidate();
        }

        #endregion

        #region 布局计算

        private void CalculateRects()
        {
            int bw = borderWidth;
            int hh = showHeader ? headerHeight : 0;

            // 计算边框区域(考虑边框宽度)
            borderRect = new Rectangle(
                bw / 2,
                bw / 2,
                Width - bw,
                Height - bw);

            // 根据Header位置计算各区域
            switch (headerPosition)
            {
                case HeaderPosition.Top:
                    headerRect = new Rectangle(bw, bw, Width - bw * 2, hh);
                    contentRect = new Rectangle(
                        bw + contentPadding.Left,
                        bw + hh + contentPadding.Top,
                        Width - bw * 2 - contentPadding.Horizontal,
                        Height - bw * 2 - hh - contentPadding.Vertical);
                    break;

                case HeaderPosition.Bottom:
                    contentRect = new Rectangle(
                        bw + contentPadding.Left,
                        bw + contentPadding.Top,
                        Width - bw * 2 - contentPadding.Horizontal,
                        Height - bw * 2 - hh - contentPadding.Vertical);
                    headerRect = new Rectangle(bw, Height - bw - hh, Width - bw * 2, hh);
                    break;

                case HeaderPosition.Left:
                    headerRect = new Rectangle(bw, bw, hh, Height - bw * 2);
                    contentRect = new Rectangle(
                        bw + hh + contentPadding.Left,
                        bw + contentPadding.Top,
                        Width - bw * 2 - hh - contentPadding.Horizontal,
                        Height - bw * 2 - contentPadding.Vertical);
                    break;

                case HeaderPosition.Right:
                    contentRect = new Rectangle(
                        bw + contentPadding.Left,
                        bw + contentPadding.Top,
                        Width - bw * 2 - hh - contentPadding.Horizontal,
                        Height - bw * 2 - contentPadding.Vertical);
                    headerRect = new Rectangle(Width - bw - hh, bw, hh, Height - bw * 2);
                    break;
            }

            // 确保尺寸有效
            contentRect.Width = Math.Max(0, contentRect.Width);
            contentRect.Height = Math.Max(0, contentRect.Height);
        }

        private void UpdateLayout()
        {
            if (contentPanel == null)
            {
                return;
            }

            CalculateRects();

            // 计算内容面板的实际位置和大小
            Rectangle panelRect = contentRect;

            // 如果启用滚动，需要考虑滚动条空间
            if (enableScrolling)
            {
                UpdateScrollBars();

                if (vScrollBar.Visible)
                {
                    panelRect.Width -= scrollBarSize;
                }
                if (hScrollBar.Visible)
                {
                    panelRect.Height -= scrollBarSize;
                }
            }

            contentPanel.Bounds = panelRect;

            // 不启用滚动时，裁剪内容
            if (!enableScrolling)
            {
                contentPanel.AutoScroll = false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CalculateRects();
            UpdateLayout();
            Invalidate();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            CalculateRects();
            UpdateLayout();
        }

        #endregion

        #region 滚动条处理

        private void UpdateScrollBars()
        {
            if (!enableScrolling || contentPanel == null)
            {
                vScrollBar.Visible = false;
                hScrollBar.Visible = false;
                return;
            }

            // 计算内容的实际大小
            Size contentSize = GetContentSize();
            Size viewportSize = new Size(contentRect.Width, contentRect.Height);

            // 判断是否需要滚动条
            bool needVScroll = autoShowScrollBars && contentSize.Height > viewportSize.Height;
            bool needHScroll = autoShowScrollBars && contentSize.Width > viewportSize.Width;

            // 如果需要一个滚动条，重新计算是否需要另一个
            if (needVScroll && !needHScroll)
            {
                needHScroll = contentSize.Width > (viewportSize.Width - scrollBarSize);
            }
            if (needHScroll && !needVScroll)
            {
                needVScroll = contentSize.Height > (viewportSize.Height - scrollBarSize);
            }

            // 更新垂直滚动条
            if (needVScroll)
            {
                int scrollHeight = contentRect.Height - (needHScroll ? scrollBarSize : 0);
                vScrollBar.Bounds = new Rectangle(
                    contentRect.Right - scrollBarSize,
                    contentRect.Top,
                    scrollBarSize,
                    scrollHeight);
                vScrollBar.Maximum = contentSize.Height;
                vScrollBar.LargeChange = Math.Max(1, scrollHeight);
                vScrollBar.Visible = true;
                vScrollBar.BringToFront();
            }
            else
            {
                vScrollBar.Visible = false;
                vScrollBar.Value = 0;
            }

            // 更新水平滚动条
            if (needHScroll)
            {
                int scrollWidth = contentRect.Width - (needVScroll ? scrollBarSize : 0);
                hScrollBar.Bounds = new Rectangle(
                    contentRect.Left,
                    contentRect.Bottom - scrollBarSize,
                    scrollWidth,
                    scrollBarSize);
                hScrollBar.Maximum = contentSize.Width;
                hScrollBar.LargeChange = Math.Max(1, scrollWidth);
                hScrollBar.Visible = true;
                hScrollBar.BringToFront();
            }
            else
            {
                hScrollBar.Visible = false;
                hScrollBar.Value = 0;
            }
        }

        private Size GetContentSize()
        {
            int maxRight = 0;
            int maxBottom = 0;

            foreach (Control control in contentPanel.Controls)
            {
                maxRight = Math.Max(maxRight, control.Right + control.Margin.Right);
                maxBottom = Math.Max(maxBottom, control.Bottom + control.Margin.Bottom);
            }

            return new Size(
                maxRight + contentPadding.Right,
                maxBottom + contentPadding.Bottom);
        }

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollContent();
        }

        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollContent();
        }

        private void ScrollContent()
        {
            if (contentPanel == null)
            {
                return;
            }

            // 计算滚动偏移
            int offsetX = hScrollBar.Visible ? -hScrollBar.Value : 0;
            int offsetY = vScrollBar.Visible ? -vScrollBar.Value : 0;

            // 移动内容面板的位置来实现滚动
            contentPanel.Location = new Point(contentRect.Left + offsetX, contentRect.Top + offsetY);
        }

        private void ContentPanel_ControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.LocationChanged += ChildControl_LayoutChanged;
            e.Control.SizeChanged += ChildControl_LayoutChanged;

            if (enableScrolling)
            {
                UpdateScrollBars();
            }
        }

        private void ContentPanel_ControlRemoved(object sender, ControlEventArgs e)
        {
            e.Control.LocationChanged -= ChildControl_LayoutChanged;
            e.Control.SizeChanged -= ChildControl_LayoutChanged;

            if (enableScrolling)
            {
                UpdateScrollBars();
            }
        }

        private void ChildControl_LayoutChanged(object sender, EventArgs e)
        {
            if (enableScrolling && autoShowScrollBars)
            {
                UpdateScrollBars();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (enableScrolling && vScrollBar.Visible)
            {
                int newValue = vScrollBar.Value - (e.Delta / 3);
                newValue = Math.Max(vScrollBar.Minimum, Math.Min(newValue, vScrollBar.Maximum - vScrollBar.LargeChange + 1));

                if (vScrollBar.Value != newValue)
                {
                    vScrollBar.Value = newValue;
                    ScrollContent();
                }
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 获取实际使用的背景色
            Color bgColor = BackColor;

            // 处理透明背景
            if (bgColor == Color.Transparent || bgColor.A == 0)
            {
                if (Parent != null)
                {
                    bgColor = Parent.BackColor;
                }

                // 如果还是透明，使用系统默认色
                if (bgColor == Color.Transparent || bgColor.A == 0)
                {
                    bgColor = SystemColors.Control;
                }
            }

            if (cornerRadius > 0 && HasVisibleBorder())
            {
                using (var path = CreateRoundedRectPath(ClientRectangle, cornerRadius))
                using (var brush = new SolidBrush(bgColor))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillPath(brush, path);
                }
            }
            else
            {
                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制Header
            if (showHeader)
            {
                DrawHeader(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (borderWidth <= 0 || !HasVisibleBorder())
            {
                return;
            }

            using (var pen = new Pen(borderColor, borderWidth))
            {
                pen.DashStyle = borderDashStyle;
                pen.Alignment = PenAlignment.Inset;

                if (cornerRadius > 0 && ShowAllBorders)
                {
                    // 完整圆角边框
                    using (var path = CreateRoundedRectPath(borderRect, cornerRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    // 分别绘制各边
                    DrawIndividualBorders(g, pen);
                }
            }
        }

        private void DrawHeader(Graphics g)
        {
            // 绘制Header背景
            if (!headerBackColor.IsEmpty)
            {
                using (var brush = new SolidBrush(headerBackColor))
                {
                    if (cornerRadius > 0)
                    {
                        using (var path = CreateHeaderPath())
                        {
                            g.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        g.FillRectangle(brush, headerRect);
                    }
                }
            }

            // 计算内容区域(考虑内边距)
            Rectangle contentArea = new Rectangle(
                headerRect.X + headerPadding.Left,
                headerRect.Y + headerPadding.Top,
                headerRect.Width - headerPadding.Horizontal,
                headerRect.Height - headerPadding.Vertical);

            // 处理左/右位置时的旋转
            if (headerPosition == HeaderPosition.Left || headerPosition == HeaderPosition.Right)
            {
                DrawVerticalHeader(g, contentArea);
            }
            else
            {
                DrawHorizontalHeader(g, contentArea);
            }
        }

        private void DrawHorizontalHeader(Graphics g, Rectangle contentArea)
        {
            Font font = HeaderFont;
            Color foreColor = HeaderForeColor;

            // 使用 TextRenderer 测量，确保一致性
            TextFormatFlags measureFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            Size textSize = TextRenderer.MeasureText(g, headerText, font,
                new Size(int.MaxValue, contentArea.Height), measureFlags);

            // 计算图标和文本的总宽度
            int totalWidth = 0;
            int iconWidth = 0;

            if (headerIcon != null)
            {
                iconWidth = headerIconSize;
                totalWidth += iconWidth + headerIconTextSpacing;
            }

            totalWidth += textSize.Width;

            // 确保总宽度不超过可用区域
            int availableWidth = contentArea.Width;
            bool needEllipsis = totalWidth > availableWidth;

            // 计算起始X坐标
            int startX;
            switch (headerTextAlign)
            {
                case HeaderTextAlignment.Center:
                    if (needEllipsis)
                    {
                        startX = contentArea.Left;
                    }
                    else
                    {
                        startX = contentArea.Left + (availableWidth - totalWidth) / 2;
                    }
                    break;

                case HeaderTextAlignment.Right:
                    if (needEllipsis)
                    {
                        startX = contentArea.Left;
                    }
                    else
                    {
                        startX = contentArea.Right - totalWidth;
                    }
                    break;

                default:
                    startX = contentArea.Left;
                    break;
            }

            // 绘制图标
            if (headerIcon != null)
            {
                Rectangle iconRect = new Rectangle(
                    startX,
                    contentArea.Top + (contentArea.Height - headerIconSize) / 2,
                    headerIconSize,
                    headerIconSize);

                g.DrawImage(headerIcon, iconRect);
                startX += headerIconSize + headerIconTextSpacing;
            }

            // 计算文本可用宽度
            int textAvailableWidth = contentArea.Right - startX;
            if (textAvailableWidth <= 0)
            {
                return;
            }

            // 使用 TextRenderer 绘制文本，确保与测量一致
            TextFormatFlags drawFlags = TextFormatFlags.SingleLine |
                                         TextFormatFlags.NoPrefix |
                                         TextFormatFlags.VerticalCenter |
                                         TextFormatFlags.EndEllipsis;

            Rectangle textRect = new Rectangle(
                startX,
                contentArea.Top,
                textAvailableWidth,
                contentArea.Height);

            TextRenderer.DrawText(g, headerText, font, textRect, foreColor, drawFlags);
        }

        private void DrawVerticalHeader(Graphics g, Rectangle contentArea)
        {
            Font font = HeaderFont;
            Color foreColor = HeaderForeColor;

            // 保存图形状态
            var state = g.Save();

            try
            {
                // 计算旋转中心
                float centerX = headerRect.Left + headerRect.Width / 2f;
                float centerY = headerRect.Top + headerRect.Height / 2f;

                // 根据位置决定旋转方向
                if (headerPosition == HeaderPosition.Left)
                {
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(-90);
                    g.TranslateTransform(-centerY, -centerX);
                }
                else // Right
                {
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(90);
                    g.TranslateTransform(-centerY, -centerX);
                }

                // 计算旋转后的内容区域
                // 注意：旋转后宽度和高度互换
                Rectangle rotatedArea = new Rectangle(
                    headerRect.Top + headerPadding.Top,
                    headerRect.Left + headerPadding.Left,
                    headerRect.Height - headerPadding.Vertical,
                    headerRect.Width - headerPadding.Horizontal);

                // 使用 TextRenderer 测量
                TextFormatFlags measureFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
                Size textSize = TextRenderer.MeasureText(g, headerText, font,
                    new Size(int.MaxValue, rotatedArea.Height), measureFlags);

                // 计算内容宽度
                int totalWidth = 0;
                int iconWidth = 0;

                if (headerIcon != null)
                {
                    iconWidth = headerIconSize;
                    totalWidth += iconWidth + headerIconTextSpacing;
                }

                totalWidth += textSize.Width;

                // 确保总宽度不超过可用区域
                int availableWidth = rotatedArea.Width;
                bool needEllipsis = totalWidth > availableWidth;

                // 计算起始位置
                int startX;
                switch (headerTextAlign)
                {
                    case HeaderTextAlignment.Center:
                        if (needEllipsis)
                        {
                            startX = rotatedArea.Left;
                        }
                        else
                        {
                            startX = rotatedArea.Left + (availableWidth - totalWidth) / 2;
                        }
                        break;

                    case HeaderTextAlignment.Right:
                        if (needEllipsis)
                        {
                            startX = rotatedArea.Left;
                        }
                        else
                        {
                            startX = rotatedArea.Right - totalWidth;
                        }
                        break;

                    default: // Left
                        startX = rotatedArea.Left;
                        break;
                }

                // 绘制图标
                if (headerIcon != null)
                {
                    Rectangle iconRect = new Rectangle(
                        startX,
                        rotatedArea.Top + (rotatedArea.Height - headerIconSize) / 2,
                        headerIconSize,
                        headerIconSize);

                    g.DrawImage(headerIcon, iconRect);
                    startX += headerIconSize + headerIconTextSpacing;
                }

                // 计算文本可用宽度
                int textAvailableWidth = rotatedArea.Right - startX;
                if (textAvailableWidth <= 0)
                {
                    return;
                }

                // 使用 TextRenderer 绘制文本
                TextFormatFlags drawFlags = TextFormatFlags.SingleLine |
                                             TextFormatFlags.NoPrefix |
                                             TextFormatFlags.VerticalCenter |
                                             TextFormatFlags.EndEllipsis;

                Rectangle textRect = new Rectangle(
                    startX,
                    rotatedArea.Top,
                    textAvailableWidth,
                    rotatedArea.Height);

                TextRenderer.DrawText(g, headerText, font, textRect, foreColor, drawFlags);
            }
            finally
            {
                // 恢复图形状态
                g.Restore(state);
            }
        }

        private void DrawIndividualBorders(Graphics g, Pen pen)
        {
            int left = borderRect.Left;
            int top = borderRect.Top;
            int right = borderRect.Right;
            int bottom = borderRect.Bottom;
            int radius = cornerRadius;

            // 顶部边框
            if (showTopBorder)
            {
                int topStartX = showLeftBorder && radius > 0 ? left + radius : left;
                int topEndX = showRightBorder && radius > 0 ? right - radius : right;

                if (topEndX > topStartX)
                {
                    g.DrawLine(pen, topStartX, top, topEndX, top);
                }

                if (radius > 0)
                {
                    if (showLeftBorder)
                    {
                        g.DrawArc(pen, left, top, radius * 2, radius * 2, 180, 90);
                    }
                    if (showRightBorder)
                    {
                        g.DrawArc(pen, right - radius * 2, top, radius * 2, radius * 2, 270, 90);
                    }
                }
            }

            // 右侧边框
            if (showRightBorder)
            {
                int rightStartY = showTopBorder && radius > 0 ? top + radius : top;
                int rightEndY = showBottomBorder && radius > 0 ? bottom - radius : bottom;

                if (rightEndY > rightStartY)
                {
                    g.DrawLine(pen, right, rightStartY, right, rightEndY);
                }

                if (radius > 0)
                {
                    if (showBottomBorder)
                    {
                        g.DrawArc(pen, right - radius * 2, bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                    }
                }
            }

            // 底部边框
            if (showBottomBorder)
            {
                int bottomStartX = showRightBorder && radius > 0 ? right - radius : right;
                int bottomEndX = showLeftBorder && radius > 0 ? left + radius : left;

                if (bottomStartX > bottomEndX)
                {
                    g.DrawLine(pen, bottomStartX, bottom, bottomEndX, bottom);
                }

                if (radius > 0)
                {
                    if (showLeftBorder)
                    {
                        g.DrawArc(pen, left, bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                    }
                }
            }

            // 左侧边框
            if (showLeftBorder)
            {
                int leftStartY = showBottomBorder && radius > 0 ? bottom - radius : bottom;
                int leftEndY = showTopBorder && radius > 0 ? top + radius : top;

                if (leftStartY > leftEndY)
                {
                    g.DrawLine(pen, left, leftStartY, left, leftEndY);
                }
            }
        }

        private bool HasVisibleBorder()
        {
            return showTopBorder || showBottomBorder || showLeftBorder || showRightBorder;
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上角
            path.AddArc(arc, 180, 90);

            // 右上角
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // 右下角
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // 左下角
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateHeaderPath()
        {
            var path = new GraphicsPath();
            int radius = cornerRadius;

            if (radius <= 0)
            {
                path.AddRectangle(headerRect);
                return path;
            }

            int diameter = radius * 2;

            switch (headerPosition)
            {
                case HeaderPosition.Top:
                    // 只有上面两个角是圆角
                    path.AddArc(headerRect.Left, headerRect.Top, diameter, diameter, 180, 90);
                    path.AddArc(headerRect.Right - diameter, headerRect.Top, diameter, diameter, 270, 90);
                    path.AddLine(headerRect.Right, headerRect.Bottom, headerRect.Left, headerRect.Bottom);
                    break;

                case HeaderPosition.Bottom:
                    path.AddLine(headerRect.Left, headerRect.Top, headerRect.Right, headerRect.Top);
                    path.AddArc(headerRect.Right - diameter, headerRect.Bottom - diameter, diameter, diameter, 0, 90);
                    path.AddArc(headerRect.Left, headerRect.Bottom - diameter, diameter, diameter, 90, 90);
                    break;

                case HeaderPosition.Left:
                    path.AddArc(headerRect.Left, headerRect.Top, diameter, diameter, 180, 90);
                    path.AddLine(headerRect.Right, headerRect.Top, headerRect.Right, headerRect.Bottom);
                    path.AddArc(headerRect.Left, headerRect.Bottom - diameter, diameter, diameter, 90, 90);
                    break;

                case HeaderPosition.Right:
                    path.AddLine(headerRect.Left, headerRect.Top, headerRect.Left, headerRect.Bottom);
                    path.AddArc(headerRect.Right - diameter, headerRect.Bottom - diameter, diameter, diameter, 0, 90);
                    path.AddArc(headerRect.Right - diameter, headerRect.Top, diameter, diameter, 270, 90);
                    break;
            }

            path.CloseFigure();
            return path;
        }

        #endregion

        #region 背景色变化处理

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            // 通知内容面板刷新
            if (contentPanel != null)
            {
                contentPanel.Invalidate();
            }
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);

            // 如果当前是透明背景，需要刷新
            if (BackColor == Color.Transparent)
            {
                Invalidate();
                contentPanel?.Invalidate();
            }
        }

        #endregion

        #region 主题支持

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                borderColor = Theme.Colors.Border;
                Invalidate();
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置边框显示状态
        /// </summary>
        public void SetBorderVisibility(bool top, bool right, bool bottom, bool left)
        {
            showTopBorder = top;
            showRightBorder = right;
            showBottomBorder = bottom;
            showLeftBorder = left;
            Invalidate();
        }

        /// <summary>
        /// 隐藏所有边框
        /// </summary>
        public void HideAllBorders()
        {
            ShowAllBorders = false;
        }

        /// <summary>
        /// 显示所有边框
        /// </summary>
        public void ShowBorders()
        {
            ShowAllBorders = true;
        }

        /// <summary>
        /// 滚动到指定控件
        /// </summary>
        public void ScrollToControl(Control control)
        {
            if (!enableScrolling || control == null || !contentPanel.Controls.Contains(control))
            {
                return;
            }

            // 计算需要滚动的位置
            if (vScrollBar.Visible)
            {
                int targetY = control.Top - contentPadding.Top;
                targetY = Math.Max(0, Math.Min(targetY, vScrollBar.Maximum - vScrollBar.LargeChange + 1));
                vScrollBar.Value = targetY;
            }

            if (hScrollBar.Visible)
            {
                int targetX = control.Left - contentPadding.Left;
                targetX = Math.Max(0, Math.Min(targetX, hScrollBar.Maximum - hScrollBar.LargeChange + 1));
                hScrollBar.Value = targetX;
            }

            ScrollContent();
        }

        /// <summary>
        /// 滚动到顶部
        /// </summary>
        public void ScrollToTop()
        {
            if (vScrollBar.Visible)
            {
                vScrollBar.Value = 0;
                ScrollContent();
            }
        }

        /// <summary>
        /// 滚动到底部
        /// </summary>
        public void ScrollToBottom()
        {
            if (vScrollBar.Visible)
            {
                vScrollBar.Value = vScrollBar.Maximum - vScrollBar.LargeChange + 1;
                ScrollContent();
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                headerFont?.Dispose();
                headerIcon?.Dispose();
                contentPanel?.Dispose();
                vScrollBar?.Dispose();
                hScrollBar?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 内容面板类

    /// <summary>
    /// FluentGroupBox 的内容面板
    /// </summary>
    [ToolboxItem(false)]
    [DesignerCategory("")]
    public class FluentGroupBoxContentPanel : Panel
    {
        private FluentGroupBox owner;

        public FluentGroupBoxContentPanel(FluentGroupBox owner)
        {
            this.owner = owner;
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.ResizeRedraw,
                true);

            // 默认透明
            this.BackColor = Color.Transparent;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        [Browsable(false)]
        public FluentGroupBox Owner => owner;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor == Color.Transparent)
            {
                // 获取父控件的背景色
                Color parentBgColor = Color.White;

                if (owner != null)
                {
                    parentBgColor = owner.BackColor;

                    // 如果父控件也是透明的，继续向上查找
                    if (parentBgColor == Color.Transparent && owner.Parent != null)
                    {
                        parentBgColor = owner.Parent.BackColor;
                    }
                }
                else if (Parent != null)
                {
                    parentBgColor = Parent.BackColor;
                }

                // 如果最终还是透明，使用系统控件颜色
                if (parentBgColor == Color.Transparent || parentBgColor.A == 0)
                {
                    parentBgColor = SystemColors.Control;
                }

                using (var brush = new SolidBrush(parentBgColor))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            if (BackColor == Color.Transparent)
            {
                Invalidate();
            }
        }
    }

    #endregion

    #region 枚举

    /// <summary>
    /// Header位置
    /// </summary>
    public enum HeaderPosition
    {
        Top,        // 顶部
        Bottom,     // 底部
        Left,       // 左侧
        Right       // 右侧
    }

    /// <summary>
    /// Header文本对齐方式
    /// </summary>
    public enum HeaderTextAlignment
    {
        Left,       // 左对齐
        Center,     // 居中
        Right       // 右对齐
    }

    #endregion

    #region 设计时支持

    public class FluentGroupBoxDesigner : ParentControlDesigner
    {
        private FluentGroupBox groupBox;
        private DesignerActionListCollection actionLists;

        #region 初始化

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            groupBox = component as FluentGroupBox;

            if (groupBox != null && groupBox.ContentPanel != null)
            {
                // 启用内容面板的设计时支持
                EnableDesignMode(groupBox.ContentPanel, "ContentPanel");
            }
        }

        #endregion

        #region 设计器属性

        /// <summary>
        /// 获取设计器操作列表
        /// </summary>
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentGroupBoxActionList(Component));
                }
                return actionLists;
            }
        }

        /// <summary>
        /// 选择规则
        /// </summary>
        public override SelectionRules SelectionRules
        {
            get
            {
                return SelectionRules.Moveable |
                       SelectionRules.Visible |
                       SelectionRules.AllSizeable;
            }
        }

        /// <summary>
        /// 指定设计器关联的控件数量
        /// </summary>
        public override int NumberOfInternalControlDesigners()
        {
            return 1;
        }

        /// <summary>
        /// 获取内部控件设计器
        /// </summary>
        public override ControlDesigner InternalControlDesigner(int internalControlIndex)
        {
            if (internalControlIndex == 0 && groupBox?.ContentPanel != null)
            {
                var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    return host.GetDesigner(groupBox.ContentPanel) as ControlDesigner;
                }
            }
            return null;
        }

        #endregion

        #region 子控件处理

        /// <summary>
        /// 获取内部命中测试
        /// </summary>
        protected override bool GetHitTest(Point point)
        {
            if (groupBox == null)
            {
                return false;
            }

            Point clientPoint = groupBox.PointToClient(point);

            // 检查是否在滚动条区域
            if (groupBox.EnableScrolling)
            {
                // 可以添加滚动条的命中测试
            }

            return false;
        }

        /// <summary>
        /// 处理控件的添加
        /// </summary>
        protected override void OnDragDrop(DragEventArgs de)
        {
            // 默认行为会将控件添加到正确的位置
            base.OnDragDrop(de);
        }

        #endregion

        #region 绘制装饰

        /// <summary>
        /// 绘制设计时装饰
        /// </summary>
        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);

            if (groupBox == null || groupBox.ContentPanel == null)
            {
                return;
            }

            // 绘制内容区域虚线边框（仅在设计时）
            Rectangle contentBounds = groupBox.ContentPanel.Bounds;

            using (var pen = new Pen(Color.FromArgb(100, 0, 120, 212), 1))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                pe.Graphics.DrawRectangle(pen,
                    contentBounds.X,
                    contentBounds.Y,
                    contentBounds.Width - 1,
                    contentBounds.Height - 1);
            }
        }

        #endregion

        #region 属性过滤

        /// <summary>
        /// 预过滤属性
        /// </summary>
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 隐藏一些不需要的属性
            string[] propertiesToRemove = { "AutoScroll", "AutoScrollMargin", "AutoScrollMinSize" };
            foreach (string prop in propertiesToRemove)
            {
                if (properties.Contains(prop))
                {
                    properties.Remove(prop);
                }
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                groupBox = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    /// FluentGroupBox 智能标签操作列表
    /// </summary>
    public class FluentGroupBoxActionList : DesignerActionList
    {
        private FluentGroupBox groupBox;
        private DesignerActionUIService designerActionUISvc;

        public FluentGroupBoxActionList(IComponent component) : base(component)
        {
            groupBox = component as FluentGroupBox;
            designerActionUISvc = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        #region 属性

        /// <summary>
        /// 标题文本
        /// </summary>
        public string Text
        {
            get => groupBox.Text;
            set
            {
                SetProperty("Text", value);
            }
        }

        /// <summary>
        /// 是否显示标题
        /// </summary>
        public bool ShowHeader
        {
            get => groupBox.ShowHeader;
            set
            {
                SetProperty("ShowHeader", value);
            }
        }

        /// <summary>
        /// 标题位置
        /// </summary>
        public HeaderPosition HeaderPosition
        {
            get => groupBox.HeaderPosition;
            set
            {
                SetProperty("HeaderPosition", value);
            }
        }

        /// <summary>
        /// 标题对齐
        /// </summary>
        public HeaderTextAlignment HeaderTextAlign
        {
            get => groupBox.HeaderTextAlign;
            set
            {
                SetProperty("HeaderTextAlign", value);
            }
        }

        /// <summary>
        /// 标题高度
        /// </summary>
        public int HeaderHeight
        {
            get => groupBox.HeaderHeight;
            set
            {
                SetProperty("HeaderHeight", value);
            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        public Color BorderColor
        {
            get => groupBox.BorderColor;
            set
            {
                SetProperty("BorderColor", value);
            }
        }

        /// <summary>
        /// 边框宽度
        /// </summary>
        public int BorderWidth
        {
            get => groupBox.BorderWidth;
            set
            {
                SetProperty("BorderWidth", value);
            }
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        public int CornerRadius
        {
            get => groupBox.CornerRadius;
            set
            {
                SetProperty("CornerRadius", value);
            }
        }

        /// <summary>
        /// 启用滚动
        /// </summary>
        public bool EnableScrolling
        {
            get => groupBox.EnableScrolling;
            set
            {
                SetProperty("EnableScrolling", value);
            }
        }

        /// <summary>
        /// 使用主题
        /// </summary>
        public bool UseTheme
        {
            get => groupBox.UseTheme;
            set
            {
                SetProperty("UseTheme", value);
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 显示所有边框
        /// </summary>
        public void ShowAllBorders()
        {
            SetProperty("ShowTopBorder", true);
            SetProperty("ShowBottomBorder", true);
            SetProperty("ShowLeftBorder", true);
            SetProperty("ShowRightBorder", true);
            RefreshUI();
        }

        /// <summary>
        /// 隐藏所有边框
        /// </summary>
        public void HideAllBorders()
        {
            SetProperty("ShowTopBorder", false);
            SetProperty("ShowBottomBorder", false);
            SetProperty("ShowLeftBorder", false);
            SetProperty("ShowRightBorder", false);
            RefreshUI();
        }

        /// <summary>
        /// 只显示顶部和底部边框
        /// </summary>
        public void ShowHorizontalBordersOnly()
        {
            SetProperty("ShowTopBorder", true);
            SetProperty("ShowBottomBorder", true);
            SetProperty("ShowLeftBorder", false);
            SetProperty("ShowRightBorder", false);
            RefreshUI();
        }

        /// <summary>
        /// 只显示左侧和右侧边框
        /// </summary>
        public void ShowVerticalBordersOnly()
        {
            SetProperty("ShowTopBorder", false);
            SetProperty("ShowBottomBorder", false);
            SetProperty("ShowLeftBorder", true);
            SetProperty("ShowRightBorder", true);
            RefreshUI();
        }

        /// <summary>
        /// 重置为默认样式
        /// </summary>
        public void ResetToDefault()
        {
            SetProperty("ShowHeader", true);
            SetProperty("HeaderPosition", HeaderPosition.Top);
            SetProperty("HeaderTextAlign", HeaderTextAlignment.Left);
            SetProperty("HeaderHeight", 26);
            SetProperty("BorderWidth", 1);
            SetProperty("CornerRadius", 4);
            SetProperty("EnableScrolling", false);
            ShowAllBorders();
            RefreshUI();
        }

        #endregion

        #region 操作项

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 标题设置
            items.Add(new DesignerActionHeaderItem("标题设置"));
            items.Add(new DesignerActionPropertyItem("Text", "标题文本", "标题设置", "设置GroupBox的标题文本"));
            items.Add(new DesignerActionPropertyItem("ShowHeader", "显示标题", "标题设置", "是否显示标题区域"));
            items.Add(new DesignerActionPropertyItem("HeaderPosition", "标题位置", "标题设置", "标题栏的位置"));
            items.Add(new DesignerActionPropertyItem("HeaderTextAlign", "文本对齐", "标题设置", "标题文本的对齐方式"));
            items.Add(new DesignerActionPropertyItem("HeaderHeight", "标题高度", "标题设置", "标题栏的高度"));

            // 边框设置
            items.Add(new DesignerActionHeaderItem("边框设置"));
            items.Add(new DesignerActionPropertyItem("BorderColor", "边框颜色", "边框设置", "边框的颜色"));
            items.Add(new DesignerActionPropertyItem("BorderWidth", "边框宽度", "边框设置", "边框的宽度"));
            items.Add(new DesignerActionPropertyItem("CornerRadius", "圆角半径", "边框设置", "边框的圆角半径"));

            // 边框快捷操作
            items.Add(new DesignerActionMethodItem(this, "ShowAllBorders", "显示所有边框", "边框设置", "显示四个方向的边框", true));
            items.Add(new DesignerActionMethodItem(this, "HideAllBorders", "隐藏所有边框", "边框设置", "隐藏所有边框", true));
            items.Add(new DesignerActionMethodItem(this, "ShowHorizontalBordersOnly", "仅水平边框", "边框设置", "只显示顶部和底部边框", true));
            items.Add(new DesignerActionMethodItem(this, "ShowVerticalBordersOnly", "仅垂直边框", "边框设置", "只显示左侧和右侧边框", true));

            // 滚动设置
            items.Add(new DesignerActionHeaderItem("滚动设置"));
            items.Add(new DesignerActionPropertyItem("EnableScrolling", "启用滚动", "滚动设置", "是否启用滚动模式"));

            // 其他操作
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "ResetToDefault", "重置为默认", "操作", "将所有设置重置为默认值", true));

            return items;
        }

        #endregion

        #region 辅助方法

        private void SetProperty(string propertyName, object value)
        {
            var prop = TypeDescriptor.GetProperties(groupBox)[propertyName];
            if (prop != null)
            {
                prop.SetValue(groupBox, value);
            }
        }

        private void RefreshUI()
        {
            designerActionUISvc?.Refresh(Component);
        }

        #endregion
    }

    /// <summary>
    /// HeaderPosition 类型转换器
    /// </summary>
    public class HeaderPositionConverter : EnumConverter
    {
        public HeaderPositionConverter() : base(typeof(HeaderPosition))
        {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    /// <summary>
    /// HeaderTextAlignment 类型转换器
    /// </summary>
    public class HeaderTextAlignmentConverter : EnumConverter
    {
        public HeaderTextAlignmentConverter() : base(typeof(HeaderTextAlignment))
        {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    #endregion
}
