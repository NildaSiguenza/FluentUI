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

namespace FluentControls.Controls
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Panel))]
    [Designer(typeof(FluentPanelDesigner))]
    public class FluentPanel : FluentContainerBase
    {
        // 组件
        private Panel titlePanel;
        private Panel contentPanel;
        private Label titleLabel;
        private Button collapseButton;

        // 标题栏相关
        private bool showTitleBar = true;
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
        private bool showAnimation = true;
        private int animationDuration = 200;
        private bool isAnimating = false;

        private bool isInitialized = false;

        #region 事件

        [Category("Fluent")]
        [Description("折叠状态改变时触发")]
        public event EventHandler CollapsedChanged;

        #endregion

        #region 构造函数

        public FluentPanel()
        {
            Size = new Size(300, 200);
            InitializeComponents();
            isInitialized = true;

            UpdateLayout();
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            // 标题面板
            titlePanel = new Panel
            {
                Name = "titlePanel",
                Dock = DockStyle.Top,
                Height = titleHeight,
                BackColor = Color.Transparent
            };
            titlePanel.Paint += OnTitlePanelPaint;
            titlePanel.Resize += (s, e) => UpdateLayout();

            // 标题标签
            titleLabel = new Label
            {
                Name = "titleLabel",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Text = titleText
            };
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
                BackColor = Color.Transparent
            };
            collapseButton.FlatAppearance.BorderSize = 0;
            collapseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 0, 0, 0);
            collapseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 0, 0, 0);
            collapseButton.Click += OnCollapseButtonClick;

            titlePanel.Controls.Add(collapseButton);
            collapseButton.BringToFront();

            // 内容面板
            contentPanel = new Panel
            {
                Name = "contentPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            contentPanel.Paint += OnContentPanelPaint;
            contentPanel.ControlAdded += OnContentPanelControlAdded;
            contentPanel.ControlRemoved += OnContentPanelControlRemoved;
            //contentPanel.Resize += OnContentPanelResize;

            // 状态文本定时器
            statusTimer = new Timer();
            statusTimer.Tick += OnStatusTimerTick;

            // 添加控件
            base.Controls.Add(contentPanel);
            base.Controls.Add(titlePanel);

            ResumeLayout(false);

            UpdateScrollBarVisibility();
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (isInitialized)
            {
                UpdateLayout();
            }
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

            //Todo: 应用主题后的额外处理
            //if (control is FluentControlBase fluentControl && UseTheme)
            //{

            //}
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 确保句柄创建后更新布局
            if (isInitialized)
            {
                BeginInvoke(new Action(() => UpdateLayout()));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                statusTimer?.Dispose();
                AnimationManager.StopAllAnimations(this);
                //animationTimer?.Dispose();
                //resizeTimer?.Dispose();
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

            // 确保标题面板宽度正确
            titlePanel.Width = this.Width;

            // 更新标题标签
            var buttonWidth = (collapsible && showTitleBar) ? 30 : 0;
            titleLabel.Location = new Point(titlePadding.Left, titlePadding.Top);
            titleLabel.Size = new Size(
                Math.Max(0, titlePanel.Width - titlePadding.Horizontal - buttonWidth),
                Math.Max(0, titleHeight - titlePadding.Vertical));

            switch (titleAlignment)
            {
                case TitleAlignment.Left:
                    titleLabel.TextAlign = ContentAlignment.MiddleLeft;
                    break;
                case TitleAlignment.Center:
                    titleLabel.TextAlign = ContentAlignment.MiddleCenter;
                    break;
                case TitleAlignment.Right:
                    titleLabel.TextAlign = ContentAlignment.MiddleRight;
                    break;
            }

            // 更新折叠按钮位置
            int buttonX = titlePanel.Width - 28;
            int buttonY = (titleHeight - 24) / 2;
            collapseButton.Location = new Point(Math.Max(0, buttonX), Math.Max(0, buttonY));
            collapseButton.Visible = showTitleBar && collapsible;

            // 更新内容面板
            if (contentPanel != null)
            {
                contentPanel.Visible = !isCollapsed;
            }
        }

        private void UpdateScrollBarVisibility()
        {
            if (contentPanel == null || !isInitialized || isUpdatingScrollBars)
            {
                return;
            }

            try
            {
                isUpdatingScrollBars = true;
                contentPanel.AutoScroll = (scrollBarVisibility == ScrollBarVisibility.Auto);
            }
            finally
            {
                isUpdatingScrollBars = false;
            }
        }

        private void PerformCollapse()
        {
            if (!isInitialized || isAnimating)
            {
                return;
            }

            // 记录受影响的控件
            RecordAffectedControls();

            if (showAnimation && !DesignMode)
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

            if (isCollapsed)
            {
                // 折叠动画
                originalHeight = Height;
                int targetHeight = showTitleBar ? titleHeight : 0;

                // 使用AnimationManager进行动画
                AnimationManager.AnimateSize(
                    this,
                    new Size(Width, targetHeight),
                    animationDuration,
                    Easing.CubicOut,
                    () =>
                    {
                        // 动画完成后的处理
                        contentPanel.Visible = false;
                        isAnimating = false;
                        UpdateAffectedControls(originalHeight, targetHeight);
                        CollapsedChanged?.Invoke(this, EventArgs.Empty);
                    });

                // 同时动画化受影响的控件
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
                contentPanel.Visible = true;

                // 使用AnimationManager进行动画
                AnimationManager.AnimateSize(
                    this,
                    new Size(Width, originalHeight),
                    animationDuration,
                    Easing.CubicOut,
                    () =>
                    {
                        // 动画完成后的处理
                        isAnimating = false;
                        UpdateAffectedControls(startHeight, originalHeight);
                        CollapsedChanged?.Invoke(this, EventArgs.Empty);
                    });

                // 同时动画化受影响的控件
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
                int newHeight = showTitleBar ? titleHeight : 0;

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
                Height = originalHeight;
                if (contentPanel != null)
                {
                    contentPanel.Visible = true;
                }

                if (collapseButton != null)
                {
                    collapseButton.Text = "▼";
                }

                UpdateAffectedControls(showTitleBar ? titleHeight : 0, originalHeight);
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

            using (var brush = new SolidBrush(TitleBackColor))
            {
                g.FillRectangle(brush, titlePanel.ClientRectangle);
            }

            using (var pen = new Pen(Color.FromArgb(50, TitleForeColor)))
            {
                g.DrawLine(pen, 0, titlePanel.Height - 1, titlePanel.Width, titlePanel.Height - 1);
            }
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
    }

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
}
