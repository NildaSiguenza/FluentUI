using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public class FluentMasker : Control
    {
        #region 字段

        private Control targetControl;
        private float backgroundOpacity = 0.5f;
        private Color maskBackColor = Color.Black;

        private bool showAnimation = true;
        private MaskAnimationStyle animationStyle = MaskAnimationStyle.Spinner;
        private bool showText = true;
        private string maskText = "加载中...";
        private MaskContentLayout contentLayout = MaskContentLayout.Vertical;

        private bool showCloseButton = false;
        private bool confirmBeforeClose = true;
        private string closeConfirmMessage = "操作正在进行中，确定要关闭吗？";

        private bool autoClose = false;
        private int autoCloseDelay = 3000;
        private bool countdownMode = false;

        private Padding maskPadding = new Padding(0);

        private Timer animationTimer;
        private Timer autoCloseTimer;
        private Timer countdownTimer;
        private float animationProgress = 0f;
        private int remainingSeconds = 0;

        private Button closeButton;
        private Rectangle animationRect;
        private Rectangle textRect;

        private bool isShowing = false;
        private Timer delayShowTimer;

        #endregion

        #region 构造函数

        public FluentMasker()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 关键：设置正确的样式
            this.SetStyle(
                ControlStyles.Opaque |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);

            this.BackColor = Color.Black;
            this.Visible = false;
            this.Dock = DockStyle.None;

            // 创建动画定时器
            animationTimer = new Timer { Interval = 50 };
            animationTimer.Tick += AnimationTimer_Tick;

            // 创建自动关闭定时器
            autoCloseTimer = new Timer();
            autoCloseTimer.Tick += AutoCloseTimer_Tick;

            // 创建倒计时定时器
            countdownTimer = new Timer { Interval = 1000 };
            countdownTimer.Tick += CountdownTimer_Tick;

            // 创建延迟显示定时器
            delayShowTimer = new Timer();
            delayShowTimer.Tick += DelayShowTimer_Tick;

            // 创建关闭按钮
            closeButton = new Button
            {
                Size = new Size(32, 32),
                Text = "✕",
                Font = new Font("Arial", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(220, 53, 69),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false,
                TabStop = false
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 35, 51);
            closeButton.Click += CloseButton_Click;
            this.Controls.Add(closeButton);
        }

        // 关键：重写 CreateParams 添加透明扩展样式
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        #endregion

        #region 属性

        [Category("Fluent Masker")]
        [Description("要遮罩的目标控件")]
        public Control TargetControl
        {
            get => targetControl;
            set
            {
                if (targetControl != value)
                {
                    DetachFromTarget();
                    targetControl = value;
                    AttachToTarget();
                }
            }
        }

        [Category("Fluent Masker")]
        [Description("遮罩背景的透明度 (0.0 - 1.0)")]
        [DefaultValue(0.5f)]
        public float BackgroundOpacity
        {
            get => backgroundOpacity;
            set
            {
                backgroundOpacity = Math.Max(0f, Math.Min(1f, value));
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("遮罩的背景颜色")]
        public Color MaskBackColor
        {
            get => maskBackColor;
            set
            {
                maskBackColor = value;
                BackColor = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("是否显示加载动画")]
        [DefaultValue(true)]
        public bool ShowAnimation
        {
            get => showAnimation;
            set
            {
                showAnimation = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("加载动画的样式")]
        [DefaultValue(MaskAnimationStyle.Spinner)]
        public MaskAnimationStyle AnimationStyle
        {
            get => animationStyle;
            set
            {
                animationStyle = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("是否显示提示文本")]
        [DefaultValue(true)]
        public bool ShowText
        {
            get => showText;
            set
            {
                showText = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("显示的提示文本")]
        [DefaultValue("加载中...")]
        public string MaskText
        {
            get => maskText;
            set
            {
                maskText = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("动画和文本的布局方式")]
        [DefaultValue(MaskContentLayout.Vertical)]
        public MaskContentLayout ContentLayout
        {
            get => contentLayout;
            set
            {
                contentLayout = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("是否显示右上角的关闭按钮")]
        [DefaultValue(false)]
        public bool ShowCloseButton
        {
            get => showCloseButton;
            set
            {
                showCloseButton = value;
                if (closeButton != null)
                {
                    closeButton.Visible = value && isShowing;
                }
            }
        }

        [Category("Fluent Masker")]
        [Description("点击关闭按钮时是否需要确认")]
        [DefaultValue(true)]
        public bool ConfirmBeforeClose
        {
            get => confirmBeforeClose;
            set => confirmBeforeClose = value;
        }

        [Category("Fluent Masker")]
        [Description("关闭确认对话框显示的消息")]
        [DefaultValue("操作正在进行中，确定要关闭吗？")]
        public string CloseConfirmMessage
        {
            get => closeConfirmMessage;
            set => closeConfirmMessage = value;
        }

        [Category("Fluent Masker")]
        [Description("是否在指定时间后自动关闭")]
        [DefaultValue(false)]
        public bool AutoClose
        {
            get => autoClose;
            set => autoClose = value;
        }

        [Category("Fluent Masker")]
        [Description("自动关闭的延迟时间（毫秒）")]
        [DefaultValue(3000)]
        public int AutoCloseDelay
        {
            get => autoCloseDelay;
            set => autoCloseDelay = Math.Max(0, value);
        }

        [Category("Fluent Masker")]
        [Description("是否启用倒计时关闭模式")]
        [DefaultValue(false)]
        public bool CountdownMode
        {
            get => countdownMode;
            set
            {
                countdownMode = value;
                Invalidate();
            }
        }

        [Category("Fluent Masker")]
        [Description("遮罩区域相对于目标控件的内边距")]
        public Padding MaskPadding
        {
            get => maskPadding;
            set
            {
                maskPadding = value;
                UpdateBounds();
            }
        }

        [Category("Fluent Masker")]
        [Description("动画区域的大小")]
        [DefaultValue(typeof(Size), "60, 60")]
        public Size AnimationSize { get; set; } = new Size(60, 60);

        [Category("Fluent Masker")]
        [Description("提示文本的字体")]
        public Font TextFont { get; set; } = new Font("Microsoft YaHei UI", 10F);

        [Category("Fluent Masker")]
        [Description("提示文本的颜色")]
        public Color TextColor { get; set; } = Color.White;

        [Category("Fluent Masker")]
        [Description("加载动画的颜色")]
        public Color AnimationColor { get; set; } = Color.White;

        [Category("Fluent Masker")]
        [Description("动画和文本之间的间距")]
        [DefaultValue(16)]
        public int ContentSpacing { get; set; } = 16;

        #endregion

        #region 事件

        [Category("Fluent Masker")]
        public event EventHandler<CancelEventArgs> Showing;

        [Category("Fluent Masker")]
        public event EventHandler Shown;

        [Category("Fluent Masker")]
        public event EventHandler<CancelEventArgs> Closing;

        [Category("Fluent Masker")]
        public event EventHandler Closed;

        #endregion

        #region 公共方法

        public new void Show()
        {
            Show(0);
        }

        public void Show(int delayMilliseconds)
        {
            if (isShowing)
            {
                return;
            }

            if (targetControl == null)
            {
                throw new InvalidOperationException("必须先设置 TargetControl 属性");
            }

            if (delayMilliseconds > 0)
            {
                delayShowTimer.Interval = delayMilliseconds;
                delayShowTimer.Start();
            }
            else
            {
                ShowInternal();
            }
        }

        public new void Close()
        {
            Close(false);
        }

        public void ForceClose()
        {
            Close(true);
        }

        #endregion

        #region 私有方法

        private void ShowInternal()
        {
            var showingArgs = new CancelEventArgs();
            OnShowing(showingArgs);
            if (showingArgs.Cancel)
            {
                return;
            }

            isShowing = true;
            UpdateBounds();

            this.Visible = true;
            this.BringToFront();

            closeButton.Visible = showCloseButton;
            if (showCloseButton)
            {
                closeButton.Location = new Point(
                    this.Width - closeButton.Width - 16,
                    16
                );
                closeButton.BringToFront();
            }

            if (showAnimation)
            {
                animationProgress = 0f;
                animationTimer.Start();
            }

            if (autoClose)
            {
                if (countdownMode)
                {
                    remainingSeconds = (int)Math.Ceiling(autoCloseDelay / 1000.0);
                    countdownTimer.Start();
                }

                autoCloseTimer.Interval = autoCloseDelay;
                autoCloseTimer.Start();
            }

            OnShown();
        }

        private void Close(bool force)
        {
            if (!isShowing)
            {
                return;
            }

            if (!force && confirmBeforeClose && showCloseButton)
            {
                var result = MessageBox.Show(
                    closeConfirmMessage,
                    "确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            var closingArgs = new CancelEventArgs();
            OnClosing(closingArgs);
            if (closingArgs.Cancel && !force)
            {
                return;
            }

            isShowing = false;

            animationTimer.Stop();
            autoCloseTimer.Stop();
            countdownTimer.Stop();
            delayShowTimer.Stop();

            this.Visible = false;
            closeButton.Visible = false;
            //backgroundDrawn = false;

            OnClosed();
        }

        private void AttachToTarget()
        {
            if (targetControl == null)
            {
                return;
            }

            if (!targetControl.Controls.Contains(this))
            {
                targetControl.Controls.Add(this);
            }

            targetControl.SizeChanged += TargetControl_SizeChanged;
            targetControl.LocationChanged += TargetControl_LocationChanged;
            targetControl.ControlAdded += TargetControl_ControlAdded;

            UpdateBounds();
        }

        private void DetachFromTarget()
        {
            if (targetControl == null)
            {
                return;
            }

            targetControl.SizeChanged -= TargetControl_SizeChanged;
            targetControl.LocationChanged -= TargetControl_LocationChanged;
            targetControl.ControlAdded -= TargetControl_ControlAdded;

            if (targetControl.Controls.Contains(this))
            {
                targetControl.Controls.Remove(this);
            }
        }

        private void UpdateBounds()
        {
            if (targetControl == null)
            {
                return;
            }

            this.Bounds = new Rectangle(
                maskPadding.Left,
                maskPadding.Top,
                targetControl.Width - maskPadding.Left - maskPadding.Right,
                targetControl.Height - maskPadding.Top - maskPadding.Bottom
            );
        }

        private void CalculateLayout()
        {
            int centerX = this.Width / 2;
            int centerY = this.Height / 2;

            Size textSize = Size.Empty;
            if (showText)
            {
                using (var g = this.CreateGraphics())
                {
                    string displayText = GetDisplayText();
                    textSize = TextRenderer.MeasureText(g, displayText, TextFont);
                }
            }

            if (contentLayout == MaskContentLayout.Vertical)
            {
                int totalHeight = 0;
                if (showAnimation)
                {
                    totalHeight += AnimationSize.Height;
                }

                if (showAnimation && showText)
                {
                    totalHeight += ContentSpacing;
                }

                if (showText)
                {
                    totalHeight += textSize.Height;
                }

                int startY = centerY - totalHeight / 2;

                if (showAnimation)
                {
                    animationRect = new Rectangle(
                        centerX - AnimationSize.Width / 2,
                        startY,
                        AnimationSize.Width,
                        AnimationSize.Height
                    );
                    startY += AnimationSize.Height + ContentSpacing;
                }

                if (showText)
                {
                    textRect = new Rectangle(
                        centerX - textSize.Width / 2,
                        startY,
                        textSize.Width,
                        textSize.Height
                    );
                }
            }
            else
            {
                int totalWidth = 0;
                if (showAnimation)
                {
                    totalWidth += AnimationSize.Width;
                }

                if (showAnimation && showText)
                {
                    totalWidth += ContentSpacing;
                }

                if (showText)
                {
                    totalWidth += textSize.Width;
                }

                int startX = centerX - totalWidth / 2;

                if (showAnimation)
                {
                    animationRect = new Rectangle(
                        startX,
                        centerY - AnimationSize.Height / 2,
                        AnimationSize.Width,
                        AnimationSize.Height
                    );
                    startX += AnimationSize.Width + ContentSpacing;
                }

                if (showText)
                {
                    textRect = new Rectangle(
                        startX,
                        centerY - textSize.Height / 2,
                        textSize.Width,
                        textSize.Height
                    );
                }
            }
        }

        private string GetDisplayText()
        {
            if (countdownMode && remainingSeconds > 0)
            {
                return $"{maskText} ({remainingSeconds}秒后关闭)";
            }
            return maskText;
        }

        #endregion

        #region 重写方法

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 不调用基类，完全自己控制
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                this.BringToFront();
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (this.Parent != null)
            {
                // 设置在父控件中的Z-Order, 使其总是在最上层
                this.Parent.Controls.SetChildIndex(this, 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 绘制半透明背景
            var bgColor = Color.FromArgb((int)(255 * backgroundOpacity), maskBackColor);

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }

            if (!isShowing)
            {
                return;
            }

            CalculateLayout();

            if (showAnimation)
            {
                DrawAnimation(g, animationRect);
            }

            if (showText)
            {
                DrawText(g, textRect);
            }
        }

        #endregion

        #region 绘制

        private void DrawAnimation(Graphics g, Rectangle rect)
        {
            switch (animationStyle)
            {
                case MaskAnimationStyle.Spinner:
                    DrawSpinnerAnimation(g, rect);
                    break;
                case MaskAnimationStyle.Dots:
                    DrawDotsAnimation(g, rect);
                    break;
                case MaskAnimationStyle.Circle:
                    DrawCircleAnimation(g, rect);
                    break;
                case MaskAnimationStyle.Ring:
                    DrawRingAnimation(g, rect);
                    break;
            }
        }

        private void DrawSpinnerAnimation(Graphics g, Rectangle rect)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int radius = Math.Min(rect.Width, rect.Height) / 2 - 4;

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f + animationProgress * 360f;
                float alpha = 1f - (i / 12f) * 0.8f;

                double radians = angle * Math.PI / 180;
                int x1 = centerX + (int)(radius * 0.6 * Math.Cos(radians));
                int y1 = centerY + (int)(radius * 0.6 * Math.Sin(radians));
                int x2 = centerX + (int)(radius * Math.Cos(radians));
                int y2 = centerY + (int)(radius * Math.Sin(radians));

                using (var pen = new Pen(Color.FromArgb(
                    (int)(255 * alpha), AnimationColor), 3))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        private void DrawDotsAnimation(Graphics g, Rectangle rect)
        {
            int dotSize = 12;
            int spacing = 8;
            int totalWidth = dotSize * 3 + spacing * 2;
            int startX = rect.X + (rect.Width - totalWidth) / 2;
            int centerY = rect.Y + rect.Height / 2;

            for (int i = 0; i < 3; i++)
            {
                float offset = (float)Math.Sin((animationProgress * Math.PI * 2) + i * Math.PI / 3) * 8;
                int x = startX + i * (dotSize + spacing);
                int y = centerY + (int)offset;

                using (var brush = new SolidBrush(AnimationColor))
                {
                    g.FillEllipse(brush, x, y - dotSize / 2, dotSize, dotSize);
                }
            }
        }

        private void DrawCircleAnimation(Graphics g, Rectangle rect)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int radius = Math.Min(rect.Width, rect.Height) / 2 - 4;

            float startAngle = animationProgress * 360f;
            float sweepAngle = 270f;

            using (var pen = new Pen(AnimationColor, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                g.DrawArc(pen,
                    centerX - radius,
                    centerY - radius,
                    radius * 2,
                    radius * 2,
                    startAngle,
                    sweepAngle);
            }
        }

        private void DrawRingAnimation(Graphics g, Rectangle rect)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int radius = Math.Min(rect.Width, rect.Height) / 2 - 4;

            using (var pen = new Pen(Color.FromArgb(50, AnimationColor), 4))
            {
                g.DrawEllipse(pen,
                    centerX - radius,
                    centerY - radius,
                    radius * 2,
                    radius * 2);
            }

            float sweepAngle = animationProgress * 360f;
            using (var pen = new Pen(AnimationColor, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                g.DrawArc(pen,
                    centerX - radius,
                    centerY - radius,
                    radius * 2,
                    radius * 2,
                    -90,
                    sweepAngle);
            }
        }

        private void DrawText(Graphics g, Rectangle rect)
        {
            string displayText = GetDisplayText();

            using (var brush = new SolidBrush(TextColor))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(displayText, TextFont, brush, rect, sf);
            }
        }

        #endregion

        #region 事件处理

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationProgress += 0.05f;
            if (animationProgress > 1f)
            {
                animationProgress = 0f;
            }
            Invalidate();
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            autoCloseTimer.Stop();
            Close(true);
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            if (remainingSeconds <= 0)
            {
                countdownTimer.Stop();
            }
            Invalidate();
        }

        private void DelayShowTimer_Tick(object sender, EventArgs e)
        {
            delayShowTimer.Stop();
            ShowInternal();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close(false);
        }

        private void TargetControl_SizeChanged(object sender, EventArgs e)
        {
            UpdateBounds();
        }

        private void TargetControl_LocationChanged(object sender, EventArgs e)
        {
            UpdateBounds();
        }

        private void TargetControl_ControlAdded(object sender, ControlEventArgs e)
        {
            if (isShowing && e.Control != this)
            {
                this.BringToFront();
            }
        }

        protected virtual void OnShowing(CancelEventArgs e)
        {
            Showing?.Invoke(this, e);
        }

        protected virtual void OnShown()
        {
            Shown?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            Closing?.Invoke(this, e);
        }

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachFromTarget();

                animationTimer?.Dispose();
                autoCloseTimer?.Dispose();
                countdownTimer?.Dispose();
                delayShowTimer?.Dispose();
                closeButton?.Dispose();
                TextFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 枚举和辅助类

    /// <summary>
    /// 遮罩动画样式
    /// </summary>
    public enum MaskAnimationStyle
    {
        Spinner,        // 旋转线条
        Dots,           // 跳动圆点
        Circle,         // 圆形进度
        Ring            // 圆环进度
    }

    /// <summary>
    /// 内容布局方式
    /// </summary>
    public enum MaskContentLayout
    {
        Vertical,       // 垂直布局
        Horizontal      // 水平布局
    }

    #endregion
}
