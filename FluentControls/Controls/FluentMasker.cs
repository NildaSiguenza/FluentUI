using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent遮罩控件
    /// </summary>
    public class FluentMasker : Form
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
        private string closeConfirmMessage = "操作正在进行中, 确定要关闭吗？";

        private bool autoClose = false;
        private int autoCloseDelay = 3000;
        private bool countdownMode = false;

        private Padding maskPadding = new Padding(0);

        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Timer autoCloseTimer;
        private System.Windows.Forms.Timer countdownTimer;
        private System.Windows.Forms.Timer delayShowTimer;

        private float animationProgress = 0f;
        private int remainingSeconds = 0;

        private Button closeButton;
        private Rectangle animationRect;
        private Rectangle textRect;

        private bool isShowing = false;
        private bool isClosing = false;

        // 事件
        public event EventHandler<CancelEventArgs> Showing;
        public event EventHandler Shown;
        public event EventHandler<CancelEventArgs> Closing;
        public event EventHandler Closed;
        public event EventHandler<CancelEventArgs> Canceled;

        // 导入API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        private static readonly IntPtr HWND_TOP = IntPtr.Zero;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        #endregion

        #region 构造函数

        public FluentMasker()
        {
            InitializeForm();
            InitializeTimers();
            InitializeCloseButton();
        }

        public FluentMasker(Control target) : this()
        {
            targetControl = target;
        }

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = false;
            this.KeyPreview = true;

            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        private void InitializeTimers()
        {
            // 动画定时器
            animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            animationTimer.Tick += AnimationTimer_Tick;

            // 自动关闭定时器
            autoCloseTimer = new System.Windows.Forms.Timer();
            autoCloseTimer.Tick += AutoCloseTimer_Tick;

            // 倒计时定时器
            countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += CountdownTimer_Tick;

            // 延迟显示定时器
            delayShowTimer = new System.Windows.Forms.Timer();
            delayShowTimer.Tick += DelayShowTimer_Tick;
        }

        private void InitializeCloseButton()
        {
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

        #endregion

        #region 属性

        [Category("Mask")]
        [Description("要遮罩的目标控件")]
        public Control TargetControl
        {
            get => targetControl;
            set => targetControl = value;
        }

        [Category("Mask")]
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

        [Category("Mask")]
        [Description("遮罩的背景颜色")]
        public Color MaskBackColor
        {
            get => maskBackColor;
            set
            {
                maskBackColor = value;
                Invalidate();
            }
        }

        [Category("Mask")]
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

        [Category("Mask")]
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

        [Category("Mask")]
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

        [Category("Mask")]
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

        [Category("Mask")]
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

        [Category("Mask")]
        [Description("是否显示右上角的关闭按钮")]
        [DefaultValue(false)]
        public bool ShowCloseButton
        {
            get => showCloseButton;
            set
            {
                showCloseButton = value;
                UpdateCloseButtonVisibility();
            }
        }

        [Category("Mask")]
        [Description("点击关闭按钮时是否需要确认")]
        [DefaultValue(true)]
        public bool ConfirmBeforeClose
        {
            get => confirmBeforeClose;
            set => confirmBeforeClose = value;
        }

        [Category("Mask")]
        [Description("关闭确认对话框显示的消息")]
        [DefaultValue("操作正在进行中, 确定要关闭吗？")]
        public string CloseConfirmMessage
        {
            get => closeConfirmMessage;
            set => closeConfirmMessage = value;
        }

        [Category("Mask")]
        [Description("是否在指定时间后自动关闭")]
        [DefaultValue(false)]
        public bool AutoClose
        {
            get => autoClose;
            set => autoClose = value;
        }

        [Category("Mask")]
        [Description("自动关闭的延迟时间(毫秒)")]
        [DefaultValue(3000)]
        public int AutoCloseDelay
        {
            get => autoCloseDelay;
            set => autoCloseDelay = Math.Max(0, value);
        }

        [Category("Mask")]
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

        [Category("Mask")]
        [Description("遮罩区域相对于目标控件的内边距")]
        public Padding MaskPadding
        {
            get => maskPadding;
            set
            {
                maskPadding = value;
                UpdatePositionAndSize();
            }
        }

        [Category("Mask")]
        [Description("动画区域的大小")]
        [DefaultValue(typeof(Size), "60, 60")]
        public Size AnimationSize { get; set; } = new Size(60, 60);

        [Category("Mask")]
        [Description("提示文本的字体")]
        public Font TextFont { get; set; } = new Font("Microsoft YaHei UI", 10F);

        [Category("Mask")]
        [Description("提示文本的颜色")]
        public Color TextColor { get; set; } = Color.White;

        [Category("Mask")]
        [Description("加载动画的颜色")]
        public Color AnimationColor { get; set; } = Color.White;

        [Category("Mask")]
        [Description("动画和文本之间的间距")]
        [DefaultValue(16)]
        public int ContentSpacing { get; set; } = 16;

        /// <summary>
        /// 是否正在显示
        /// </summary>
        [Browsable(false)]
        public bool IsShowing => isShowing;

        #endregion

        #region 重写

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // ESC 键关闭
            if (e.KeyCode == Keys.Escape && showCloseButton)
            {
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.Clear(maskBackColor);

            // 绘制半透明背景
            //using (var brush = new SolidBrush(Color.FromArgb((int)(255 * backgroundOpacity), maskBackColor)))
            //{
            //    g.FillRectangle(brush, ClientRectangle);
            //}

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAllTimers();
            base.OnFormClosing(e);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示遮罩
        /// </summary>
        public new void Show()
        {
            Show(0);
        }

        /// <summary>
        /// 延迟显示遮罩
        /// </summary>
        /// <param name="delayMilliseconds">延迟毫秒数</param>
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

        /// <summary>
        /// 关闭遮罩
        /// </summary>
        public new void Close()
        {
            CloseInternal(false);
        }

        /// <summary>
        /// 强制关闭遮罩
        /// </summary>
        public void ForceClose()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => CloseInternal(true)));
            }
            else
            {
                CloseInternal(true);
            }
        }

        /// <summary>
        /// 刷新配置
        /// </summary>
        public void RefreshConfiguration()
        {
            UpdatePositionAndSize();
            UpdateCloseButtonVisibility();
            Invalidate();
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
            isClosing = false;

            UpdatePositionAndSize();
            AttachTargetEvents();

            // 获取目标控件所在的窗体
            var parentForm = targetControl.FindForm();
            if (parentForm != null)
            {
                AttachParentFormEvents(parentForm);
                this.Owner = parentForm;
            }

            this.Opacity = backgroundOpacity;
            this.BackColor = maskBackColor;

            base.Show();

            UpdateCloseButtonVisibility();
            UpdateCloseButtonPosition();

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

        private void CloseInternal(bool force, bool byCloseButton = false)
        {
            if (!isShowing || isClosing)
            {
                return;
            }

            // 保存父窗口引用
            var parentForm = this.Owner;

            if (!force && confirmBeforeClose && showCloseButton)
            {
                var result = MessageBox.Show(
                    this,
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

            // 如果是由关闭按钮触发的关闭,触发 Canceled 事件
            if(byCloseButton)
            {
                Canceled?.Invoke(this, closingArgs);
            }

            isClosing = true;
            StopAllTimers();
            DetachTargetEvents();
            DetachParentFormEvents();

            isShowing = false;

            // 先激活父窗口，再隐藏遮罩
            if (parentForm != null && !parentForm.IsDisposed && parentForm.IsHandleCreated)
            {
                // 使用 Win32 API 确保父窗口在最前面
                SetWindowPos(parentForm.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                SetForegroundWindow(parentForm.Handle);
                BringWindowToTop(parentForm.Handle);
            }

            this.Hide();

            // 确保父窗口激活
            if (parentForm != null && !parentForm.IsDisposed)
            {
                parentForm.Activate();
                parentForm.Focus();
            }

            OnClosed();
        }

        private void StopAllTimers()
        {
            animationTimer?.Stop();
            autoCloseTimer?.Stop();
            countdownTimer?.Stop();
            delayShowTimer?.Stop();
        }

        private void AttachTargetEvents()
        {
            if (targetControl == null)
            {
                return;
            }

            targetControl.LocationChanged += TargetControl_Changed;
            targetControl.SizeChanged += TargetControl_Changed;
            targetControl.ParentChanged += TargetControl_ParentChanged;
            targetControl.VisibleChanged += TargetControl_VisibleChanged;
        }

        private void DetachTargetEvents()
        {
            if (targetControl == null)
            {
                return;
            }

            targetControl.LocationChanged -= TargetControl_Changed;
            targetControl.SizeChanged -= TargetControl_Changed;
            targetControl.ParentChanged -= TargetControl_ParentChanged;
            targetControl.VisibleChanged -= TargetControl_VisibleChanged;
        }

        private void AttachParentFormEvents(Form parentForm)
        {
            if (parentForm == null)
            {
                return;
            }

            parentForm.LocationChanged += ParentForm_LocationChanged;
            parentForm.SizeChanged += ParentForm_SizeChanged;
            parentForm.Activated += ParentForm_Activated;
            parentForm.Deactivate += ParentForm_Deactivate;
        }

        private void DetachParentFormEvents()
        {
            var parentForm = this.Owner;
            if (parentForm == null)
            {
                return;
            }

            parentForm.LocationChanged -= ParentForm_LocationChanged;
            parentForm.SizeChanged -= ParentForm_SizeChanged;
            parentForm.Activated -= ParentForm_Activated;
            parentForm.Deactivate -= ParentForm_Deactivate;
        }

        /// <summary>
        /// 更新位置和大小
        /// </summary>
        public void UpdatePositionAndSize()
        {
            if (targetControl == null || !targetControl.IsHandleCreated)
            {
                return;
            }

            try
            {
                Point screenLocation = targetControl.PointToScreen(Point.Empty);

                this.Location = new Point(
                    screenLocation.X + maskPadding.Left,
                    screenLocation.Y + maskPadding.Top
                );
                this.Size = new Size(
                    targetControl.Width - maskPadding.Left - maskPadding.Right,
                    targetControl.Height - maskPadding.Top - maskPadding.Bottom
                );

                UpdateCloseButtonPosition();
            }
            catch
            {
                // 忽略可能的异常
            }
        }

        private void UpdateCloseButtonVisibility()
        {
            if (closeButton != null)
            {
                closeButton.Visible = showCloseButton && isShowing;
            }
        }

        private void UpdateCloseButtonPosition()
        {
            if (closeButton == null)
            {
                return;
            }

            closeButton.Location = new Point(
                this.Width - closeButton.Width - 16,
                16
            );
            closeButton.BringToFront();
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
            else // Horizontal
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

        #region 绘制

        protected virtual void DrawAnimation(Graphics g, Rectangle rect)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int radius = Math.Min(rect.Width, rect.Height) / 2 - 4;

            switch (animationStyle)
            {
                case MaskAnimationStyle.Spinner:
                    DrawSpinnerAnimation(g, centerX, centerY, radius);
                    break;
                case MaskAnimationStyle.Dots:
                    DrawDotsAnimation(g, rect);
                    break;
                case MaskAnimationStyle.Circle:
                    DrawCircleAnimation(g, centerX, centerY, radius);
                    break;
                case MaskAnimationStyle.Ring:
                    DrawRingAnimation(g, centerX, centerY, radius);
                    break;
            }
        }

        private void DrawSpinnerAnimation(Graphics g, int centerX, int centerY, int radius)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f + animationProgress * 360f;
                float alpha = 1f - (i / 12f) * 0.8f;

                double radians = angle * Math.PI / 180;
                int x1 = centerX + (int)(radius * 0.6 * Math.Cos(radians));
                int y1 = centerY + (int)(radius * 0.6 * Math.Sin(radians));
                int x2 = centerX + (int)(radius * Math.Cos(radians));
                int y2 = centerY + (int)(radius * Math.Sin(radians));

                using (var pen = new Pen(Color.FromArgb((int)(255 * alpha), AnimationColor), 3))
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

        private void DrawCircleAnimation(Graphics g, int centerX, int centerY, int radius)
        {
            float startAngle = animationProgress * 360f;
            float sweepAngle = 270f;

            using (var pen = new Pen(AnimationColor, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2,
                    startAngle, sweepAngle);
            }
        }

        private void DrawRingAnimation(Graphics g, int centerX, int centerY, int radius)
        {
            // 背景圆环
            using (var pen = new Pen(Color.FromArgb(50, AnimationColor), 4))
            {
                g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // 进度圆环
            float sweepAngle = animationProgress * 360f;
            using (var pen = new Pen(AnimationColor, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2,
                    -90, sweepAngle);
            }
        }

        protected virtual void DrawText(Graphics g, Rectangle rect)
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
            CloseInternal(true);
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
            CloseInternal(false, true);
        }

        private void TargetControl_Changed(object sender, EventArgs e)
        {
            if (!isClosing && isShowing)
            {
                UpdatePositionAndSize();
            }
        }

        private void TargetControl_ParentChanged(object sender, EventArgs e)
        {
            if (!isClosing && isShowing)
            {
                UpdatePositionAndSize();
            }
        }

        private void TargetControl_VisibleChanged(object sender, EventArgs e)
        {
            if (targetControl != null && !targetControl.Visible && isShowing)
            {
                this.Hide();
            }
            else if (targetControl != null && targetControl.Visible && isShowing)
            {
                this.Show();
            }
        }

        private void ParentForm_LocationChanged(object sender, EventArgs e)
        {
            if (!isClosing && isShowing)
            {
                UpdatePositionAndSize();
            }
        }

        private void ParentForm_SizeChanged(object sender, EventArgs e)
        {
            if (!isClosing && isShowing)
            {
                UpdatePositionAndSize();
            }
        }

        private void ParentForm_Activated(object sender, EventArgs e)
        {
            if (isShowing && !isClosing)
            {
                this.BringToFront();
            }
        }

        private void ParentForm_Deactivate(object sender, EventArgs e)
        {
            // 可选：当父窗体失去焦点时的处理
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

        #region 释放资源

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopAllTimers();
                DetachTargetEvents();
                DetachParentFormEvents();

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
