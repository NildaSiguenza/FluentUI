using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using FluentControls.Animation;
using System.ComponentModel.Design;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace FluentControls.Controls
{
    [ToolboxBitmap(typeof(Panel))]
    [Designer(typeof(FluentPanelDesigner))]
    public class FluentPanel : FluentContainerBase
    {
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        // 组件
        private DoubleBufferedPanel titlePanel;
        private DoubleBufferedPanel contentPanel;
        private Label titleLabel;
        private Button collapseButton;

        // 标题栏相关
        private bool showTitleBar = true;
        private bool showTitleText = true;
        private string titleText = "Panel Title";
        private TitleAlignment titleAlignment = TitleAlignment.Left;
        private Color? titleBackColor;
        private Color? titleForeColor;
        private Font titleFont;
        private int titleHeight = 32;
        private Padding titlePadding = new Padding(8, 0, 8, 0);

        // 折叠相关
        private bool collapsible = true;
        private bool isCollapsed = false;
        private int originalHeight;
        private Dictionary<Control, int> originalPositions = new Dictionary<Control, int>();

        // 状态文本相关
        private string statusText = "";
        private int statusDuration = 0;
        private Timer statusTimer;
        private bool isStatusVisible = false;
        private Font statusFont;
        private Color statusForeColor = Color.FromArgb(100, 100, 100);
        private Color statusBackColor = Color.FromArgb(240, 240, 240);
        private Point statusOffset = new Point(10, 10);
        private ContentAlignment statusAlignment = ContentAlignment.BottomLeft;

        // 边框相关
        private bool showBorder = true;
        private int borderWidth = 1;
        private Color? borderColor;

        // 滚动条相关
        private ScrollBarVisibility scrollBarVisibility = ScrollBarVisibility.Auto;
        private bool isUpdatingScrollBars = false;

        // 折叠动画
        private bool showAnimation = false;
        private int animationDuration = 200;
        private bool isAnimating = false;

        // 定时器
        private System.Windows.Forms.Timer resizeDebounceTimer;
        private Size pendingSize;

        private bool isInitialized = false;

        // 事件
        public event EventHandler BeforeCollapsedChange;
        public event EventHandler CollapsedChanged;
        public event EventHandler TitleBarClicked;

        #region 构造函数

        public FluentPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor,
                    true);

            DoubleBuffered = true;
            UpdateStyles();

            Size = new Size(300, 200);
            this.Padding = new Padding(1);

            InitializeComponents();
            InitializeDebounceTimer();

            isInitialized = true;

            UpdateLayout();
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            titlePanel = new DoubleBufferedPanel
            {
                Name = "titlePanel",
                Dock = DockStyle.None,
                Height = titleHeight,
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            titlePanel.Paint += OnTitlePanelPaint;
            titlePanel.MouseDown += OnTitlePanelMouseDown;

            // 标题标签
            titleLabel = new Label
            {
                Name = "titleLabel",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Text = titleText,
                UseMnemonic = false // 防止&字符被解析为快捷键
            };
            titleLabel.MouseDown += OnTitlePanelMouseDown;

            // 启用标题标签的双缓冲
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(titleLabel, true, null);

            titlePanel.Controls.Add(titleLabel);

            // 折叠按钮
            collapseButton = new Button
            {
                Name = "collapseButton",
                Size = new Size(24, 24),
                FlatStyle = FlatStyle.Flat,
                Text = "▼",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = collapsible && showTitleBar,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                UseVisualStyleBackColor = false,
                TabStop = false // 避免获得焦点时重绘
            };

            // 启用按钮的双缓冲
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(collapseButton, true, null);

            collapseButton.FlatAppearance.BorderSize = 0;
            collapseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 0, 0, 0);
            collapseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 0, 0, 0);
            collapseButton.Click += OnCollapseButtonClick;

            titlePanel.Controls.Add(collapseButton);
            collapseButton.BringToFront();

            contentPanel = new DoubleBufferedPanel
            {
                Name = "contentPanel",
                Dock = DockStyle.None,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                AutoScroll = false
            };
            contentPanel.Paint += OnContentPanelPaint;
            contentPanel.ControlAdded += OnContentPanelControlAdded;
            contentPanel.ControlRemoved += OnContentPanelControlRemoved;

            // 状态文本定时器
            statusTimer = new Timer();
            statusTimer.Tick += OnStatusTimerTick;

            // 添加控件
            base.Controls.Add(contentPanel);
            base.Controls.Add(titlePanel);

            ResumeLayout(false);

            UpdateScrollBarVisibility();
        }

        private void InitializeDebounceTimer()
        {
            resizeDebounceTimer = new System.Windows.Forms.Timer
            {
                Interval = 10 // 10ms防抖
            };
            resizeDebounceTimer.Tick += (s, e) =>
            {
                resizeDebounceTimer.Stop();
                UpdateLayout();
                Invalidate(false); // 不刷新子控件
            };
        }
        #endregion

        #region 属性

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        public Panel ContentPanel => contentPanel;

        public new ControlCollection Controls => contentPanel?.Controls;


        [Category("TitleBar")]
        [Description("是否显示标题栏")]
        [DefaultValue(true)]
        public bool ShowTitleBar
        {
            get => showTitleBar;
            set
            {
                showTitleBar = value;
                if (titlePanel != null)
                {
                    titlePanel.Visible = value;
                }
                if (collapseButton != null)
                {
                    collapseButton.Visible = value && collapsible;
                }
                UpdateLayout();
            }
        }

        [Category("TitleBar")]
        [Description("是否显示标题栏文本")]
        [DefaultValue(true)]
        public bool ShowTitleText
        {
            get => showTitleText;
            set
            {
                showTitleText = value;
                titleLabel.Visible = value;
                titlePanel.Invalidate();
            }
        }

        [Category("TitleBar")]
        [Description("标题文本")]
        [DefaultValue("Panel Title")]
        public string TitleText
        {
            get => titleText;
            set
            {
                titleText = value;
                if (titleLabel != null)
                {
                    titleLabel.Text = value;
                }
                UpdateLayout();
            }
        }

        [Category("TitleBar")]
        [Description("标题对齐方式")]
        [DefaultValue(TitleAlignment.Left)]
        public TitleAlignment TitleAlignment
        {
            get => titleAlignment;
            set
            {
                titleAlignment = value;
                UpdateLayout();
            }
        }

        [Category("TitleBar")]
        [Description("标题栏背景色")]
        public Color TitleBackColor
        {
            get => GetThemeColor(c => c.Primary, titleBackColor ?? Color.FromArgb(0, 120, 212));
            set
            {
                titleBackColor = value;
                if (titlePanel != null)
                {
                    titlePanel.Invalidate();
                }
            }
        }

        [Category("TitleBar")]
        [Description("标题文本颜色")]
        public Color TitleForeColor
        {
            get => GetThemeColor(c => c.TextOnPrimary, titleForeColor ?? Color.White);
            set
            {
                titleForeColor = value;
                if (titleLabel != null)
                {
                    titleLabel.ForeColor = value;
                }
                if (collapseButton != null)
                {
                    collapseButton.ForeColor = value;
                }
            }
        }

        [Category("TitleBar")]
        [Description("标题字体")]
        public Font TitleFont
        {
            get => titleFont ?? Font;
            set
            {
                titleFont = value;
                if (titleLabel != null)
                {
                    titleLabel.Font = value ?? Font;
                }
            }
        }

        [Category("TitleBar")]
        [Description("标题栏高度")]
        [DefaultValue(32)]
        public int TitleHeight
        {
            get => titleHeight;
            set
            {
                titleHeight = Math.Max(20, value);
                if (titlePanel != null)
                {
                    titlePanel.Height = titleHeight;
                }
                UpdateLayout();
            }
        }

        public new Padding Padding
        {
            get => base.Padding;
            set
            {
                if (base.Padding != value)
                {
                    base.Padding = value;
                    UpdateLayout();
                }
            }
        }

        [Category("TitleBar")]
        [Description("标题内边距")]
        public Padding TitlePadding
        {
            get => titlePadding;
            set
            {
                titlePadding = value;
                UpdateLayout();
            }
        }

        [Category("Collapse")]
        [Description("是否可折叠")]
        [DefaultValue(true)]
        public bool Collapsible
        {
            get => collapsible;
            set
            {
                collapsible = value;
                if (collapseButton != null)
                {
                    collapseButton.Visible = value && showTitleBar;
                }
                UpdateLayout();
            }
        }

        [Category("Collapse")]
        [Description("是否已折叠")]
        [DefaultValue(false)]
        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                if (isCollapsed != value && isInitialized)
                {
                    isCollapsed = value;
                    PerformCollapse();
                }
            }
        }

        [Category("Collapse")]
        [Description("是否显示折叠/展开动画")]
        [DefaultValue(true)]
        public bool ShowAnimation
        {
            get => showAnimation;
            set => showAnimation = value;
        }

        [Category("Collapse")]
        [Description("动画持续时间(毫秒)")]
        [DefaultValue(200)]
        public new int AnimationDuration
        {
            get => animationDuration;
            set => animationDuration = Math.Max(50, Math.Min(1000, value));
        }

        [Category("Border")]
        [Description("是否显示边框")]
        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                showBorder = value;
                Invalidate();
            }
        }

        [Category("Border")]
        [Description("边框宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = Math.Max(0, Math.Min(10, value));

                // 自动调整 Padding 以适应边框
                if (borderWidth > 0)
                {
                    int paddingValue = Math.Max(borderWidth, 1);
                    if (this.Padding.All != paddingValue)
                    {
                        this.Padding = new Padding(paddingValue);
                    }
                }

                Invalidate();
            }
        }

        [Category("Border")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => GetThemeColor(c => c.Border, borderColor ?? Color.LightGray);
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        [Category("Status")]
        [Description("状态文本")]
        [DefaultValue("")]
        public string StatusText
        {
            get => statusText;
            set => statusText = value ?? "";
        }

        [Category("Status")]
        [Description("状态文本显示时长(毫秒), 0为永久显示")]
        [DefaultValue(0)]
        public int StatusDuration
        {
            get => statusDuration;
            set => statusDuration = Math.Max(0, value);
        }

        [Category("Status")]
        [Description("状态文本字体")]
        public Font StatusFont
        {
            get => statusFont ?? new Font(Font.FontFamily, Font.Size * 0.85f);
            set
            {
                statusFont = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本前景色")]
        public Color StatusForeColor
        {
            get => statusForeColor;
            set
            {
                statusForeColor = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本背景色")]
        public Color StatusBackColor
        {
            get => statusBackColor;
            set
            {
                statusBackColor = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本偏移")]
        public Point StatusOffset
        {
            get => statusOffset;
            set
            {
                statusOffset = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本对齐方式")]
        [DefaultValue(ContentAlignment.BottomLeft)]
        public ContentAlignment StatusAlignment
        {
            get => statusAlignment;
            set
            {
                statusAlignment = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public bool IsStatusVisible => isStatusVisible;


        [Category("ScrollBar")]
        [Description("滚动条显示模式")]
        [DefaultValue(ScrollBarVisibility.Auto)]
        public ScrollBarVisibility ScrollBarVisibility
        {
            get => scrollBarVisibility;
            set
            {
                if (scrollBarVisibility != value)
                {
                    scrollBarVisibility = value;
                    UpdateScrollBarVisibility();
                }
            }
        }

        #endregion

        #region 公共方法

        public void ShowStatus()
        {
            ShowStatus(statusText, statusDuration);
        }

        public void ShowStatus(string text, int duration = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            statusText = text;
            statusDuration = duration;
            isStatusVisible = true;

            if (statusTimer != null)
            {
                statusTimer.Stop();
            }

            if (duration > 0)
            {
                statusTimer.Interval = duration;
                statusTimer.Start();
            }

            Invalidate();
        }

        public void HideStatus()
        {
            isStatusVisible = false;

            if (statusTimer != null)
            {
                statusTimer.Stop();
            }

            Invalidate();
        }

        public void Expand()
        {
            if (isCollapsed)
            {
                IsCollapsed = false;
            }
        }

        public void Collapse()
        {
            if (!isCollapsed && collapsible)
            {
                IsCollapsed = true;
            }
        }

        public void ToggleCollapse()
        {
            if (collapsible)
            {
                IsCollapsed = !IsCollapsed;
            }
        }

        #endregion

        #region 重写方法

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            ApplyThemeStyles();
        }

        protected override void InitializeDefaultStyles()
        {
            base.InitializeDefaultStyles();
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
        }

        protected override void ApplyThemeStyles()
        {

            if (!isInitialized || Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;

            titleLabel.ForeColor = TitleForeColor;
            titleLabel.BackColor = TitleBackColor;
            titleLabel.Invalidate();

            // 更新折叠按钮颜色
            if (collapseButton != null)
            {
                collapseButton.ForeColor = TitleForeColor;
                collapseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
                collapseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 255, 255, 255);
            }

            // 更新边框颜色
            if (!borderColor.HasValue)
            {
                Invalidate();
            }

            // 更新内容面板背景色
            if (contentPanel != null)
            {
                contentPanel.BackColor = Color.Transparent;
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);

            if (isInitialized)
            {
                UpdateLayout();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 在动画期间跳过布局更新
            if (isInitialized && !isAnimating)
            {
                // 使用防抖定时器, 避免频繁更新
                if (resizeDebounceTimer != null)
                {
                    resizeDebounceTimer.Stop();
                    resizeDebounceTimer.Start();
                }
                else
                {
                    UpdateLayout();
                }
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制状态文本
            if (isStatusVisible && !string.IsNullOrEmpty(statusText) && contentPanel != null && contentPanel.Visible)
            {
                DrawStatusText(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (showBorder && borderWidth > 0)
            {
                using (var pen = new Pen(BorderColor, borderWidth))
                {
                    var rect = new Rectangle(
                        borderWidth / 2,
                        borderWidth / 2,
                        Width - borderWidth,
                        Height - borderWidth);
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        private void DrawStatusText(Graphics g)
        {
            var font = StatusFont;
            var textSize = TextRenderer.MeasureText(statusText, font);

            Rectangle statusRect = CalculateStatusRectangle(textSize);

            using (GraphicsPath path = GetRoundedRectangle(statusRect, 4))
            {
                using (var bgBrush = new SolidBrush(Color.FromArgb(230, statusBackColor)))
                {
                    g.FillPath(bgBrush, path);
                }

                // 状态文本边框
                //using (var pen = new Pen(Color.FromArgb(100, statusForeColor), 1))
                //{
                //    g.DrawPath(pen, path);
                //}
            }

            TextRenderer.DrawText(g, statusText, font, statusRect, statusForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private Rectangle CalculateStatusRectangle(Size textSize)
        {
            int padding = 8;
            int width = textSize.Width + padding * 2;
            int height = textSize.Height + padding;

            Rectangle contentBounds = contentPanel.Bounds;
            int x = 0, y = 0;

            switch (statusAlignment)
            {
                case ContentAlignment.TopLeft:
                    x = contentBounds.X + statusOffset.X;
                    y = contentBounds.Y + statusOffset.Y;
                    break;
                case ContentAlignment.TopCenter:
                    x = contentBounds.X + (contentBounds.Width - width) / 2;
                    y = contentBounds.Y + statusOffset.Y;
                    break;
                case ContentAlignment.TopRight:
                    x = contentBounds.Right - width - statusOffset.X;
                    y = contentBounds.Y + statusOffset.Y;
                    break;
                case ContentAlignment.MiddleLeft:
                    x = contentBounds.X + statusOffset.X;
                    y = contentBounds.Y + (contentBounds.Height - height) / 2;
                    break;
                case ContentAlignment.MiddleCenter:
                    x = contentBounds.X + (contentBounds.Width - width) / 2;
                    y = contentBounds.Y + (contentBounds.Height - height) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    x = contentBounds.Right - width - statusOffset.X;
                    y = contentBounds.Y + (contentBounds.Height - height) / 2;
                    break;
                case ContentAlignment.BottomLeft:
                    x = contentBounds.X + statusOffset.X;
                    y = contentBounds.Bottom - height - statusOffset.Y;
                    break;
                case ContentAlignment.BottomCenter:
                    x = contentBounds.X + (contentBounds.Width - width) / 2;
                    y = contentBounds.Bottom - height - statusOffset.Y;
                    break;
                case ContentAlignment.BottomRight:
                    x = contentBounds.Right - width - statusOffset.X;
                    y = contentBounds.Bottom - height - statusOffset.Y;
                    break;
            }

            return new Rectangle(x, y, width, height);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (isAnimating)
            {
                BeginInvoke(new Action(() => OnControlAdded(e)));
                return;
            }

            base.OnControlAdded(e);

            if (isInitialized && e?.Control != null &&
                e.Control != titlePanel &&
                e.Control != contentPanel &&
                contentPanel != null)
            {
                BeginInvoke(new Action(() =>
                {
                    if (base.Controls.Contains(e.Control))
                    {
                        base.Controls.Remove(e.Control);
                        contentPanel.Controls.Add(e.Control);
                    }
                }));
            }
        }

        protected override void ApplyThemeToControl(Control control, bool recursive)
        {
            base.ApplyThemeToControl(control, recursive);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 确保句柄创建后更新布局
            if (isInitialized)
            {
                BeginInvoke(new Action(() => UpdateLayout()));
                UpdateScrollBarVisibility();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                statusTimer?.Dispose();
                resizeDebounceTimer?.Dispose();
                AnimationManager.StopAllAnimations(this);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 私有方法

        private void UpdateLayout()
        {
            if (!isInitialized || titlePanel == null || titleLabel == null || collapseButton == null)
            {
                return;
            }

            // 如果正在动画, 只做最小限度的更新
            if (isAnimating)
            {
                UpdateLayoutDuringAnimation();
                return;
            }

            SuspendLayout();
            titlePanel.SuspendLayout();

            try
            {
                // 计算可用区域(减去 Padding 和 Border)
                int leftOffset = this.Padding.Left;
                int topOffset = this.Padding.Top;
                int rightOffset = this.Padding.Right;
                int bottomOffset = this.Padding.Bottom;

                int availableWidth = Math.Max(0, this.Width - leftOffset - rightOffset);
                int availableHeight = Math.Max(0, this.Height - topOffset - bottomOffset);

                // 更新标题面板位置和大小
                if (showTitleBar)
                {
                    int actualTitleHeight = Math.Min(titleHeight, availableHeight);

                    titlePanel.Bounds = new Rectangle(
                        leftOffset,
                        topOffset,
                        availableWidth,
                        actualTitleHeight
                    );
                    titlePanel.Visible = true;

                    // 更新标题标签
                    var buttonWidth = collapsible ? 30 : 0;
                    titleLabel.Bounds = new Rectangle(
                        titlePadding.Left,
                        titlePadding.Top,
                        Math.Max(0, titlePanel.Width - titlePadding.Horizontal - buttonWidth),
                        Math.Max(0, actualTitleHeight - titlePadding.Vertical)
                    );

                    // 更新对齐方式
                    ContentAlignment newAlignment;
                    switch (titleAlignment)
                    {
                        case TitleAlignment.Left:
                            newAlignment = ContentAlignment.MiddleLeft;
                            break;
                        case TitleAlignment.Center:
                            newAlignment = ContentAlignment.MiddleCenter;
                            break;
                        case TitleAlignment.Right:
                            newAlignment = ContentAlignment.MiddleRight;
                            break;
                        default:
                            newAlignment = ContentAlignment.MiddleLeft;
                            break;
                    }

                    if (titleLabel.TextAlign != newAlignment)
                    {
                        titleLabel.TextAlign = newAlignment;
                    }

                    // 更新折叠按钮位置
                    if (collapsible)
                    {
                        collapseButton.Location = new Point(
                            Math.Max(0, titlePanel.Width - 28),
                            Math.Max(0, (actualTitleHeight - 24) / 2)
                        );
                        collapseButton.Visible = true;
                    }
                    else
                    {
                        collapseButton.Visible = false;
                    }
                }
                else
                {
                    titlePanel.Visible = false;
                    collapseButton.Visible = false;
                }

                // 更新内容面板位置和大小
                if (contentPanel != null && !isCollapsed)
                {
                    int titlePanelHeight = showTitleBar ? Math.Min(titleHeight, availableHeight) : 0;
                    int contentTop = topOffset + titlePanelHeight;
                    int contentHeight = Math.Max(0, availableHeight - titlePanelHeight);

                    if (contentHeight > 0)
                    {
                        contentPanel.Bounds = new Rectangle(
                            leftOffset,
                            contentTop,
                            availableWidth,
                            contentHeight
                        );
                        contentPanel.Visible = true;
                    }
                    else
                    {
                        contentPanel.Visible = false;
                    }
                }
                else if (contentPanel != null)
                {
                    contentPanel.Visible = false;
                }
            }
            finally
            {
                titlePanel.ResumeLayout(false);
                ResumeLayout(false);
            }
        }

        /// <summary>
        /// 动画期间的轻量级布局更新
        /// </summary>
        private void UpdateLayoutDuringAnimation()
        {
            if (titlePanel == null || contentPanel == null)
            {
                return;
            }

            // 不使用 SuspendLayout, 直接更新关键布局
            int leftOffset = this.Padding.Left;
            int topOffset = this.Padding.Top;
            int rightOffset = this.Padding.Right;
            int bottomOffset = this.Padding.Bottom;

            int availableWidth = Math.Max(0, this.Width - leftOffset - rightOffset);
            int availableHeight = Math.Max(0, this.Height - topOffset - bottomOffset);

            // 只更新宽度, 让高度由动画控制
            if (showTitleBar)
            {
                titlePanel.Width = availableWidth;

                // 更新标题标签宽度
                var buttonWidth = collapsible ? 30 : 0;
                titleLabel.Width = Math.Max(0, titlePanel.Width - titlePadding.Horizontal - buttonWidth);

                // 更新折叠按钮位置
                if (collapsible)
                {
                    collapseButton.Left = Math.Max(0, titlePanel.Width - 28);
                }
            }

            // 更新内容面板
            if (!isCollapsed && contentPanel.Visible)
            {
                contentPanel.Left = leftOffset;
                contentPanel.Width = availableWidth;

                int titlePanelHeight = showTitleBar ? titleHeight : 0;
                int contentTop = topOffset + titlePanelHeight;
                int contentHeight = Math.Max(0, availableHeight - titlePanelHeight);

                if (contentHeight > 0)
                {
                    contentPanel.Top = contentTop;
                    contentPanel.Height = contentHeight;
                }
            }
        }

        private void UpdateScrollBarVisibility()
        {
            if (contentPanel == null || !isInitialized || IsCollapsed)
            {
                return;
            }

            switch (scrollBarVisibility)
            {
                case ScrollBarVisibility.Auto:
                    contentPanel.AutoScroll = true;
                    contentPanel.HorizontalScroll.Enabled = true;
                    contentPanel.VerticalScroll.Enabled = true;
                    break;

                case ScrollBarVisibility.None:
                    contentPanel.AutoScroll = false;
                    contentPanel.HorizontalScroll.Visible = false;
                    contentPanel.VerticalScroll.Visible = false;
                    break;

                case ScrollBarVisibility.Horizontal:
                    contentPanel.AutoScroll = true;
                    contentPanel.HorizontalScroll.Enabled = true;
                    contentPanel.VerticalScroll.Enabled = false;
                    // 强制只显示水平滚动条
                    contentPanel.VerticalScroll.Visible = false;
                    break;

                case ScrollBarVisibility.Vertical:
                    contentPanel.AutoScroll = true;
                    contentPanel.HorizontalScroll.Enabled = false;
                    contentPanel.VerticalScroll.Enabled = true;
                    // 强制只显示垂直滚动条
                    contentPanel.HorizontalScroll.Visible = false;
                    break;

                case ScrollBarVisibility.Both:
                    contentPanel.AutoScroll = true;
                    contentPanel.HorizontalScroll.Enabled = true;
                    contentPanel.VerticalScroll.Enabled = true;
                    contentPanel.HorizontalScroll.Visible = true;
                    contentPanel.VerticalScroll.Visible = true;
                    break;
            }

            contentPanel.PerformLayout();
        }

        private void PerformCollapse()
        {
            if (!isInitialized || isAnimating)
            {
                return;
            }

            // 记录受影响的控件
            RecordAffectedControls();

            BeforeCollapsedChange?.Invoke(this, EventArgs.Empty);

            if (ShowAnimation && !DesignMode)
            {
                PerformCollapseWithAnimation();
            }
            else
            {
                PerformCollapseWithoutAnimation();
            }
        }

        private void PerformCollapseWithAnimation()
        {
            isAnimating = true;

            // 停止所有现有动画
            AnimationManager.StopAllAnimations(this);

            if (isCollapsed)
            {
                // 折叠动画
                originalHeight = Height;

                int targetHeight = showTitleBar
                    ? (titleHeight + this.Padding.Top + this.Padding.Bottom)
                    : (this.Padding.Top + this.Padding.Bottom);

                // 立即隐藏内容面板
                if (contentPanel != null)
                {
                    contentPanel.Visible = false;
                }

                // 启动流畅的尺寸动画
                AnimationManager.AnimateSize(
                    this,
                    new Size(Width, targetHeight),
                    animationDuration,
                    Easing.Linear,
                    () =>
                    {
                        // 动画完成
                        isAnimating = false;
                        UpdateAffectedControls(originalHeight, targetHeight);
                        UpdateLayout(); // 完成后完整更新布局
                        CollapsedChanged?.Invoke(this, EventArgs.Empty);
                    }
                );

                // 同时动画受影响的控件
                AnimateAffectedControls(originalHeight, targetHeight);

                if (collapseButton != null)
                {
                    collapseButton.Text = "▶";
                }
            }
            else
            {
                // 展开动画
                int startHeight = Height;

                // 先显示内容面板
                if (contentPanel != null)
                {
                    contentPanel.Visible = true;
                }

                // 启动流畅的尺寸动画
                AnimationManager.AnimateSize(
                    this,
                    new Size(Width, originalHeight),
                    animationDuration,
                    Easing.Linear,
                    () =>
                    {
                        // 动画完成
                        isAnimating = false;
                        UpdateAffectedControls(startHeight, originalHeight);
                        UpdateLayout(); // 完成后完整更新布局
                        UpdateScrollBarVisibility();
                        CollapsedChanged?.Invoke(this, EventArgs.Empty);
                    }
                );

                // 同时动画受影响的控件
                AnimateAffectedControls(startHeight, originalHeight);

                if (collapseButton != null)
                {
                    collapseButton.Text = "▼";
                }
            }
        }

        private void PerformCollapseWithoutAnimation()
        {
            if (isCollapsed)
            {
                originalHeight = Height;

                // 计算折叠后的高度
                int newHeight = showTitleBar
                    ? (titleHeight + this.Padding.Top + this.Padding.Bottom)
                    : (this.Padding.Top + this.Padding.Bottom);

                Height = newHeight;
                if (contentPanel != null)
                {
                    contentPanel.Visible = false;
                }

                if (collapseButton != null)
                {
                    collapseButton.Text = "▶";
                }

                UpdateAffectedControls(originalHeight, newHeight);
            }
            else
            {
                int oldHeight = Height;
                Height = originalHeight;
                if (contentPanel != null)
                {
                    contentPanel.Visible = true;
                }

                if (collapseButton != null)
                {
                    collapseButton.Text = "▼";
                }

                UpdateAffectedControls(oldHeight, originalHeight);
            }

            UpdateLayout();
            CollapsedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RecordAffectedControls()
        {
            originalPositions.Clear();

            if (Parent == null)
            {
                return;
            }

            foreach (Control control in Parent.Controls)
            {
                if (control != this && control.Top >= Bottom)
                {
                    originalPositions[control] = control.Top;
                }
            }
        }

        private void AnimateAffectedControls(int fromHeight, int toHeight)
        {
            if (Parent == null || originalPositions.Count == 0)
            {
                return;
            }

            int offset = fromHeight - toHeight;

            foreach (var kvp in originalPositions)
            {
                if (kvp.Key.Parent == Parent)
                {
                    AnimationManager.AnimateLocation(
                        kvp.Key,
                        new Point(kvp.Key.Left, kvp.Value - offset),
                        animationDuration,
                        Easing.CubicOut);
                }
            }
        }

        private void UpdateAffectedControls(int fromHeight, int toHeight)
        {
            if (Parent == null || originalPositions.Count == 0)
            {
                return;
            }

            int offset = fromHeight - toHeight;

            foreach (var kvp in originalPositions)
            {
                if (kvp.Key.Parent == Parent)
                {
                    kvp.Key.Top = kvp.Value - offset;
                }
            }
        }

        #endregion

        #region 事件处理

        private void OnTitlePanelPaint(object sender, PaintEventArgs e)
        {
            if (e == null || titlePanel == null)
            {
                return;
            }

            var g = e.Graphics;

            // 使用高质量渲染
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // 只绘制需要更新的区域
            using (var brush = new SolidBrush(TitleBackColor))
            {
                g.FillRectangle(brush, e.ClipRectangle);
            }

            // 绘制底部分隔线
            using (var pen = new Pen(Color.FromArgb(50, TitleForeColor)))
            {
                int y = titlePanel.Height - 1;
                g.DrawLine(pen, e.ClipRectangle.Left, y, e.ClipRectangle.Right, y);
            }
        }

        private void OnTitlePanelMouseDown(object sender, MouseEventArgs e)
        {
            TitleBarClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnContentPanelControlAdded(object sender, ControlEventArgs e)
        {
            // 当控件添加到内容面板时, 应用主题
            if (EnableChildThemeInheritance && UseTheme && e.Control is Control c)
            {
                // 进一步判断添加的控件是否启用了主题
                if (c is FluentControlBase fc && !fc.UseTheme)
                {
                    return;
                }
                ApplyThemeToControl(e.Control, true);
            }
        }

        private void OnContentPanelControlRemoved(object sender, ControlEventArgs e)
        {
            // 控件移除时的处理
        }

        private void OnContentPanelPaint(object sender, PaintEventArgs e)
        {
            // 内容面板的自定义绘制
        }

        private void OnContentPanelResize(object sender, EventArgs e)
        {
            // 只在必要时更新滚动条
        }

        private void OnCollapseButtonClick(object sender, EventArgs e)
        {
            ToggleCollapse();
        }

        private void OnStatusTimerTick(object sender, EventArgs e)
        {
            HideStatus();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 暂停重绘
        /// </summary>
        public void SuspendDrawing()
        {
            if (IsHandleCreated)
            {
                SendMessage(Handle, WM_SETREDRAW, false, 0);
            }
        }

        /// <summary>
        /// 恢复重绘
        /// </summary>
        public void ResumeDrawing()
        {
            if (IsHandleCreated)
            {
                SendMessage(Handle, WM_SETREDRAW, true, 0);
                Refresh();
            }
        }

        #endregion

        #region 内部类

        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor,
                    true);

                DoubleBuffered = true;
                UpdateStyles();
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                    return cp;
                }
            }
        }

        #endregion
    }

    #region 枚举和辅助类

    public enum TitleAlignment
    {
        Left,
        Center,
        Right
    }

    public enum ScrollBarVisibility
    {
        Auto,
        None,
        Horizontal,
        Vertical,
        Both
    }

    #endregion

    #region 设计时支持

    public class FluentPanelDesigner : ParentControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (Control is FluentPanel fluentPanel && fluentPanel.ContentPanel != null)
            {
                EnableDesignMode(fluentPanel.ContentPanel, "ContentPanel");
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentPanelActionList(Component));
                }
                return actionLists;
            }
        }
    }

    public class FluentPanelActionList : DesignerActionList
    {
        private FluentPanel panel;
        private DesignerActionUIService designerService;

        public FluentPanelActionList(IComponent component) : base(component)
        {
            panel = component as FluentPanel;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        #region 属性

        public bool UseTheme
        {
            get => panel.UseTheme;
            set => SetProperty("UseTheme", value);
        }

        public string ThemeName
        {
            get => panel.ThemeName;
            set => SetProperty("ThemeName", value);
        }

        public bool ShowTitleBar
        {
            get => panel.ShowTitleBar;
            set => SetProperty("ShowTitleBar", value);
        }

        public string TitleText
        {
            get => panel.TitleText;
            set => SetProperty("TitleText", value);
        }

        public bool Collapsible
        {
            get => panel.Collapsible;
            set => SetProperty("Collapsible", value);
        }

        public bool ShowBorder
        {
            get => panel.ShowBorder;
            set => SetProperty("ShowBorder", value);
        }

        public bool EnableChildThemeInheritance
        {
            get => panel.EnableChildThemeInheritance;
            set => SetProperty("EnableChildThemeInheritance", value);
        }

        #endregion

        #region 方法

        [DisplayName("在父容器中停靠")]
        [Description("将控件停靠填充父容器")]
        public void DockFill()
        {
            if (panel.Parent != null)
            {
                PropertyDescriptor prop = GetPropertyByName("Dock");
                prop.SetValue(panel, DockStyle.Fill);
            }
        }

        [DisplayName("取消停靠")]
        [Description("取消控件的停靠")]
        public void UndockControl()
        {
            PropertyDescriptor prop = GetPropertyByName("Dock");
            prop.SetValue(panel, DockStyle.None);
        }

        public void ApplyLightTheme()
        {
            panel.ThemeName = "FluentLight";
            panel.UseTheme = true;
            RefreshDesigner();
        }

        public void ApplyDarkTheme()
        {
            panel.ThemeName = "FluentDark";
            panel.UseTheme = true;
            RefreshDesigner();
        }

        public void ApplyClassicTheme()
        {
            panel.ThemeName = "Classic";
            panel.UseTheme = true;
            RefreshDesigner();
        }

        public void ClearTheme()
        {
            panel.UseTheme = false;
            panel.ThemeName = "";
            RefreshDesigner();
        }

        public void ApplyThemeToChildren()
        {
            panel.ApplyThemeToChildren(true);
            RefreshDesigner();
        }

        #endregion

        #region 辅助方法

        private PropertyDescriptor GetPropertyByName(string propName)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(panel)[propName];
            if (prop == null)
            {
                throw new ArgumentException("未找到属性", propName);
            }

            return prop;
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(panel)[propertyName];
            property.SetValue(panel, value);
        }

        private void RefreshDesigner()
        {
            designerService?.Refresh(Component);
        }

        #endregion

        // 获取项目列表
        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            items.Add(new DesignerActionHeaderItem("操作"));

            if (panel.Dock == DockStyle.Fill)
            {
                items.Add(new DesignerActionMethodItem(this, "UndockControl", "取消停靠", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "DockFill", "在父容器中停靠", "操作", true));
            }

            // 主题设置组
            items.Add(new DesignerActionHeaderItem("主题设置"));
            items.Add(new DesignerActionPropertyItem("UseTheme", "启用主题", "主题设置", "是否使用主题样式"));
            items.Add(new DesignerActionPropertyItem("ThemeName", "主题名称", "主题设置", "选择要使用的主题"));
            items.Add(new DesignerActionPropertyItem("EnableChildThemeInheritance", "子控件继承主题", "主题设置", "是否将主题应用到子控件"));

            // 快速主题切换
            items.Add(new DesignerActionMethodItem(this, "ApplyLightTheme", "应用浅色主题", "主题设置", "快速应用浅色主题", true));
            items.Add(new DesignerActionMethodItem(this, "ApplyDarkTheme", "应用深色主题", "主题设置", "快速应用深色主题", true));
            items.Add(new DesignerActionMethodItem(this, "ApplyClassicTheme", "应用经典主题", "主题设置", "快速应用经典主题", true));
            items.Add(new DesignerActionMethodItem(this, "ClearTheme", "清除主题", "主题设置", "清除主题设置", true));
            items.Add(new DesignerActionMethodItem(this, "ApplyThemeToChildren", "应用到子控件", "主题设置", "将当前主题应用到所有子控件", true));

            // 标题栏设置组
            items.Add(new DesignerActionHeaderItem("标题栏"));
            items.Add(new DesignerActionPropertyItem("ShowTitleBar", "显示标题栏", "标题栏", "是否显示标题栏"));
            items.Add(new DesignerActionPropertyItem("TitleText", "标题文本", "标题栏", "标题栏显示的文本"));
            items.Add(new DesignerActionPropertyItem("Collapsible", "可折叠", "标题栏", "是否允许折叠面板"));

            // 外观设置组
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowBorder", "显示边框", "外观", "是否显示边框"));

            return items;
        }
    }

    #endregion
}
