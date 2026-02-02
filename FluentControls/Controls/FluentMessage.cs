using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Animation;
using FluentControls.Logging;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent消息控件
    /// </summary>
    public class FluentMessage : Control
    {
        private MessageOptions options;
        private Timer autoCloseTimer;
        private bool isClosing = false;
        private bool isPaused = false;
        private int remainingTime;
        private DateTime createdTime;
        private const int MAX_LIFETIME = 60000;

        // UI组件
        private PictureBox iconBox;
        private Label titleLabel;
        private Label contentLabel;
        private Panel closeButtonPanel;
        private Label closeLabel;

        // 边框属性
        private int borderWidth = 1;
        private Color borderColor = Color.DimGray;
        private bool showBorder = false;

        // 动画相关
        private Point targetLocation;
        private float fadeOpacity = 1.0f;
        private Timer fadeTimer;
        private bool isFading = false;

        public event EventHandler MessageClosed;
        public event EventHandler<MessageClickEventArgs> MessageClicked;

        public MessageOptions Options => options;
        public int MessageHeight => this.Height;

        #region 构造函数

        public FluentMessage(MessageOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.Content))
            {
                throw new ArgumentException("消息内容不能为空", nameof(options));
            }

            createdTime = DateTime.Now;

            // 设置控件样式
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;

            InitializeComponents();
            ApplyOptions();
            SetupAutoClose();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(options.Width, 100);
            this.MinimumSize = new Size(200, 60);
            this.MaximumSize = new Size(600, 300);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.White;

            // 图标
            iconBox = new PictureBox
            {
                Size = new Size(24, 24),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Visible = false
            };

            // 标题
            titleLabel = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent,
                UseMnemonic = false,
                Visible = false
            };

            // 内容
            contentLabel = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent,
                UseMnemonic = false
            };

            // 关闭按钮
            closeButtonPanel = new Panel
            {
                Size = new Size(24, 24),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };

            closeLabel = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            closeButtonPanel.Controls.Add(closeLabel);
            closeButtonPanel.Click += (s, e) => Close();
            closeLabel.Click += (s, e) => Close();
            closeButtonPanel.MouseEnter += (s, e) => closeLabel.ForeColor = Color.Red;
            closeButtonPanel.MouseLeave += (s, e) =>
            {
                closeLabel.ForeColor = GetTextColor();
            };

            this.Controls.AddRange(new Control[] { iconBox, titleLabel, contentLabel });

            if (options.Closable)
            {
                this.Controls.Add(closeButtonPanel);
            }

            // 点击事件
            this.Click += OnMessageClick;
            titleLabel.Click += OnMessageClick;
            contentLabel.Click += OnMessageClick;
            iconBox.Click += OnMessageClick;

            // 鼠标悬停暂停
            if (options.PauseOnHover)
            {
                SetupHoverEvents(this);
                SetupHoverEvents(titleLabel);
                SetupHoverEvents(contentLabel);
                SetupHoverEvents(iconBox);
                SetupHoverEvents(closeButtonPanel);
            }
        }

        private void SetupHoverEvents(Control control)
        {
            control.MouseEnter += (s, e) => isPaused = true;
            control.MouseLeave += (s, e) => isPaused = false;
        }

        private void ApplyOptions()
        {
            const int PADDING_TOP = 12;
            const int PADDING_BOTTOM = 12;
            const int PADDING_LEFT = 16;
            const int PADDING_RIGHT = 16;
            const int CLOSE_BUTTON_WIDTH = 40;
            const int ICON_SIZE = 24;
            const int SPACING = 12;

            bool hasIcon = options.ShowIcon;
            bool hasTitle = !string.IsNullOrWhiteSpace(options.Title);

            int currentY = PADDING_TOP;
            int contentLeft = PADDING_LEFT;
            int availableWidth = options.Width - PADDING_LEFT - PADDING_RIGHT;

            if (options.Closable)
            {
                availableWidth -= CLOSE_BUTTON_WIDTH;
            }

            // 配置图标
            if (hasIcon)
            {
                if (options.CustomIcon != null)
                {
                    iconBox.Image = options.CustomIcon;
                }
                else
                {
                    iconBox.Image = GetDefaultIcon(options.Type);
                }
                iconBox.Visible = true;
                contentLeft = PADDING_LEFT + ICON_SIZE + SPACING;
                availableWidth -= (ICON_SIZE + SPACING);
            }
            else
            {
                iconBox.Visible = false;
            }

            // 配置标题
            int titleHeight = 0;
            if (hasTitle)
            {
                titleLabel.Text = options.Title;
                titleLabel.Location = new Point(contentLeft, currentY);
                titleLabel.Width = availableWidth;
                titleLabel.Visible = true;

                using (Graphics g = CreateGraphics())
                {
                    SizeF titleSize = g.MeasureString(options.Title, titleLabel.Font, availableWidth);
                    titleHeight = (int)Math.Ceiling(titleSize.Height);
                    titleLabel.Height = titleHeight;
                    currentY += titleHeight + 4;
                }
            }
            else
            {
                titleLabel.Visible = false;
            }

            // 配置内容
            contentLabel.Location = new Point(contentLeft, currentY);
            contentLabel.Width = availableWidth;
            contentLabel.Text = options.Content;

            int contentHeight;
            using (Graphics g = CreateGraphics())
            {
                SizeF contentSize = g.MeasureString(options.Content, contentLabel.Font, availableWidth);
                contentHeight = (int)Math.Ceiling(contentSize.Height);
            }
            contentLabel.Height = contentHeight;

            // 计算总高度
            int totalHeight = PADDING_TOP + PADDING_BOTTOM;

            if (hasTitle)
            {
                totalHeight += titleHeight + 4;
            }

            totalHeight += contentHeight;

            // 确保图标能完整显示
            if (hasIcon)
            {
                int iconMinHeight = PADDING_TOP + ICON_SIZE + PADDING_BOTTOM;
                totalHeight = Math.Max(totalHeight, iconMinHeight);
            }

            // 设置高度
            this.Height = Math.Max(60, Math.Min(300, totalHeight));

            // 调整图标垂直居中
            if (hasIcon)
            {
                int contentAreaHeight = this.Height - PADDING_TOP - PADDING_BOTTOM;
                int iconY = PADDING_TOP + (contentAreaHeight - ICON_SIZE) / 2;
                iconBox.Location = new Point(PADDING_LEFT, iconY);
            }

            // 如果只有内容没有标题，让内容垂直居中
            if (!hasTitle && hasIcon)
            {
                int contentAreaHeight = this.Height - PADDING_TOP - PADDING_BOTTOM;
                int contentY = PADDING_TOP + (contentAreaHeight - contentHeight) / 2;
                contentLabel.Location = new Point(contentLeft, contentY);
            }

            // 调整关闭按钮位置
            if (options.Closable)
            {
                closeButtonPanel.Location = new Point(this.Width - PADDING_RIGHT - 24, PADDING_TOP);
            }

            // 应用类型样式
            ApplyTypeStyles();

            // 应用圆角区域(关键修复)
            ApplyRoundedRegion();
        }

        private void ApplyRoundedRegion()
        {
            try
            {
                int radius = 8;
                using (GraphicsPath path = GetRoundedRectanglePath(
                    new Rectangle(0, 0, this.Width, this.Height), radius))
                {
                    this.Region = new Region(path);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用圆角区域失败: {ex.Message}");
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRoundedRegion();
        }

        private void ApplyTypeStyles()
        {
            Color accentColor;
            switch (options.Type)
            {
                case MessageType.Success:
                    accentColor = Color.FromArgb(76, 175, 80);
                    break;
                case MessageType.Warning:
                    accentColor = Color.FromArgb(255, 152, 0);
                    break;
                case MessageType.Error:
                    accentColor = Color.FromArgb(244, 67, 54);
                    break;
                case MessageType.Info:
                    accentColor = Color.FromArgb(33, 150, 243);
                    break;
                default:
                    accentColor = Color.FromArgb(33, 150, 243);
                    break;
            }

            // 设置半透明背景色
            Color backgroundColor;
            switch (options.Type)
            {
                case MessageType.Success:
                    backgroundColor = Color.FromArgb(240, 247, 241); // 浅绿
                    break;
                case MessageType.Warning:
                    backgroundColor = Color.FromArgb(255, 248, 225); // 浅黄
                    break;
                case MessageType.Error:
                    backgroundColor = Color.FromArgb(255, 235, 238); // 浅红
                    break;
                case MessageType.Info:
                    backgroundColor = Color.FromArgb(227, 242, 253); // 浅蓝
                    break;
                default:
                    backgroundColor = Color.FromArgb(240, 240, 240);
                    break;
            }

            this.BackColor = backgroundColor;

            // 设置边框颜色
            if (borderColor == Color.Transparent || borderColor == Color.Empty)
            {
                borderColor = accentColor;
            }

            titleLabel.ForeColor = accentColor;
            contentLabel.ForeColor = Color.FromArgb(33, 33, 33);
            closeLabel.ForeColor = Color.FromArgb(100, 100, 100);
        }

        private Color GetTextColor()
        {
            return Color.FromArgb(100, 100, 100);
        }

        private Image GetDefaultIcon(MessageType type)
        {
            Bitmap icon = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(icon))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Color iconColor;
                switch (type)
                {
                    case MessageType.Success:
                        iconColor = Color.FromArgb(76, 175, 80);
                        break;
                    case MessageType.Warning:
                        iconColor = Color.FromArgb(255, 152, 0);
                        break;
                    case MessageType.Error:
                        iconColor = Color.FromArgb(244, 67, 54);
                        break;
                    case MessageType.Info:
                        iconColor = Color.FromArgb(33, 150, 243);
                        break;
                    default:
                        iconColor = Color.FromArgb(33, 150, 243);
                        break;
                }

                using (SolidBrush brush = new SolidBrush(iconColor))
                {
                    g.FillEllipse(brush, 2, 2, 20, 20);
                }

                using (Pen pen = new Pen(Color.White, 2.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    switch (type)
                    {
                        case MessageType.Success:
                            g.DrawLines(pen, new Point[] {
                            new Point(7, 12),
                            new Point(10, 15),
                            new Point(17, 8)
                        });
                            break;

                        case MessageType.Warning:
                            g.DrawLine(pen, 12, 7, 12, 13);
                            using (SolidBrush dotBrush = new SolidBrush(Color.White))
                            {
                                g.FillEllipse(dotBrush, 10.5f, 15, 3, 3);
                            }
                            break;

                        case MessageType.Error:
                            g.DrawLine(pen, 8, 8, 16, 16);
                            g.DrawLine(pen, 16, 8, 8, 16);
                            break;

                        case MessageType.Info:
                            using (SolidBrush dotBrush = new SolidBrush(Color.White))
                            {
                                g.FillEllipse(dotBrush, 10.5f, 7, 3, 3);
                            }
                            g.DrawLine(pen, 12, 11, 12, 17);
                            break;
                    }
                }
            }
            return icon;
        }

        #endregion

        #region 属性

        [Category("Fluent")]
        [DefaultValue(0)]
        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth != value)
                {
                    borderWidth = Math.Max(0, value);
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
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

        [Category("Fluent")]
        [DefaultValue(false)]
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

        #endregion


        #region 自动关闭

        private void SetupAutoClose()
        {
            if (options.Duration > 0)
            {
                remainingTime = options.Duration;

                autoCloseTimer = new Timer();
                autoCloseTimer.Interval = 100;
                autoCloseTimer.Tick += OnAutoCloseTick;
                autoCloseTimer.Start();
            }

            // 超时保护
            Timer lifetimeTimer = new Timer();
            lifetimeTimer.Interval = MAX_LIFETIME;
            lifetimeTimer.Tick += (s, e) =>
            {
                lifetimeTimer.Stop();
                lifetimeTimer.Dispose();

                if (!isClosing && !IsDisposed)
                {
                    System.Diagnostics.Debug.WriteLine($"消息超时强制关闭: {options.Content}");
                    ForceClose();
                }
            };
            lifetimeTimer.Start();
        }

        private void OnAutoCloseTick(object sender, EventArgs e)
        {
            if (isPaused || isClosing || isFading)
            {
                return;
            }

            remainingTime -= autoCloseTimer.Interval;

            if (remainingTime <= 0)
            {
                autoCloseTimer.Stop();
                Close();
            }
        }

        #endregion

        #region 关闭

        public void Close()
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            isClosing = true;
            StopAllTimers();
            PlayCloseAnimation();
        }

        private void ForceClose()
        {
            if (IsDisposed)
            {
                return;
            }

            isClosing = true;
            StopAllTimers();
            OnAnimationComplete();
        }

        private void StopAllTimers()
        {
            try
            {
                if (autoCloseTimer != null)
                {
                    autoCloseTimer.Stop();
                    autoCloseTimer.Tick -= OnAutoCloseTick;
                }

                if (fadeTimer != null)
                {
                    fadeTimer.Stop();
                    fadeTimer.Tick -= FadeOutTick;
                    fadeTimer.Tick -= FadeInTick;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"停止计时器错误: {ex.Message}");
            }
        }

        private void PlayCloseAnimation()
        {
            switch (options.Animation)
            {
                case MessageAnimation.Fade:
                    AnimateFadeOut();
                    break;
                case MessageAnimation.Slide:
                    AnimateSlideOut();
                    break;
                case MessageAnimation.SlideAndFade:
                    AnimateSlideAndFadeOut();
                    break;
                case MessageAnimation.Scale:
                    AnimateScaleOut();
                    break;
                default:
                    OnAnimationComplete();
                    break;
            }
        }

        private void AnimateFadeOut()
        {
            isFading = true;
            fadeOpacity = 1.0f;

            if (fadeTimer == null)
            {
                fadeTimer = new Timer { Interval = 16 };
            }
            else
            {
                fadeTimer.Stop();
                fadeTimer.Tick -= FadeOutTick;
                fadeTimer.Tick -= FadeInTick;
            }

            fadeTimer.Tick += FadeOutTick;
            fadeTimer.Start();
        }

        private void FadeOutTick(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                fadeTimer?.Stop();
                return;
            }

            fadeOpacity -= 0.08f;

            if (fadeOpacity <= 0)
            {
                fadeOpacity = 0;
                fadeTimer.Stop();
                fadeTimer.Tick -= FadeOutTick;
                isFading = false;
                OnAnimationComplete();
            }
            else
            {
                this.Invalidate();
            }
        }

        private void AnimateSlideOut()
        {
            Point targetLoc = this.Location;

            switch (options.Position)
            {
                case MessagePosition.TopLeft:
                case MessagePosition.BottomLeft:
                    targetLoc.X = -this.Width - 50;
                    break;
                case MessagePosition.TopRight:
                case MessagePosition.BottomRight:
                    if (this.Parent != null)
                    {
                        targetLoc.X = this.Parent.Width + 50;
                    }
                    else
                    {
                        targetLoc.X = Screen.PrimaryScreen.WorkingArea.Width + 50;
                    }
                    break;
            }

            AnimationManager.AnimateLocation(this, targetLoc, 300, Easing.CubicIn, OnAnimationComplete);
        }

        private void AnimateSlideAndFadeOut()
        {
            AnimateSlideOut();
            AnimateFadeOut();
        }

        private void AnimateScaleOut()
        {
            Size targetSize = new Size(this.Width, 0);
            AnimationManager.AnimateSize(this, targetSize, 300, Easing.CubicIn, OnAnimationComplete);
        }

        private void OnAnimationComplete()
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(OnAnimationComplete));
                    return;
                }

                StopAllTimers();
                isFading = false;

                MessageClosed?.Invoke(this, EventArgs.Empty);

                if (this.Parent != null)
                {
                    this.Parent.Controls.Remove(this);
                }

                this.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnAnimationComplete 错误: {ex.Message}");
            }
        }

        #endregion

        #region 显示动画

        public void PlayShowAnimation()
        {
            switch (options.Animation)
            {
                case MessageAnimation.Fade:
                    AnimateFadeIn();
                    break;
                case MessageAnimation.Slide:
                    AnimateSlideIn();
                    break;
                case MessageAnimation.SlideAndFade:
                    AnimateSlideAndFadeIn();
                    break;
                case MessageAnimation.Scale:
                    AnimateScaleIn();
                    break;
                case MessageAnimation.Bounce:
                    AnimateBounceIn();
                    break;
                default:
                    this.Visible = true;
                    fadeOpacity = 1.0f;
                    break;
            }
        }

        private void AnimateFadeIn()
        {
            this.Visible = true;
            isFading = true;
            fadeOpacity = 0f;

            if (fadeTimer == null)
            {
                fadeTimer = new Timer { Interval = 16 };
            }
            else
            {
                fadeTimer.Stop();
                fadeTimer.Tick -= FadeOutTick;
                fadeTimer.Tick -= FadeInTick;
            }

            fadeTimer.Tick += FadeInTick;
            fadeTimer.Start();
        }

        private void FadeInTick(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                fadeTimer?.Stop();
                return;
            }

            fadeOpacity += 0.1f;

            if (fadeOpacity >= 1.0f)
            {
                fadeOpacity = 1.0f;
                fadeTimer.Stop();
                fadeTimer.Tick -= FadeInTick;
                isFading = false;
            }

            this.Invalidate();
        }

        private void AnimateSlideIn()
        {
            Point originalLocation = this.Location;
            Point startLocation = originalLocation;

            switch (options.Position)
            {
                case MessagePosition.TopLeft:
                case MessagePosition.BottomLeft:
                    startLocation.X = -this.Width - 50;
                    break;
                case MessagePosition.TopRight:
                case MessagePosition.BottomRight:
                    if (this.Parent != null)
                    {
                        startLocation.X = this.Parent.Width + 50;
                    }
                    else
                    {
                        startLocation.X = Screen.PrimaryScreen.WorkingArea.Width + 50;
                    }
                    break;
            }

            this.Location = startLocation;
            this.Visible = true;
            fadeOpacity = 1.0f;

            AnimationManager.AnimateLocation(this, originalLocation, 300, Easing.CubicOut, null);
        }

        private void AnimateSlideAndFadeIn()
        {
            Point originalLocation = this.Location;
            Point startLocation = originalLocation;

            switch (options.Position)
            {
                case MessagePosition.TopLeft:
                case MessagePosition.BottomLeft:
                    startLocation.X = -this.Width - 50;
                    break;
                case MessagePosition.TopRight:
                case MessagePosition.BottomRight:
                    if (this.Parent != null)
                    {
                        startLocation.X = this.Parent.Width + 50;
                    }
                    else
                    {
                        startLocation.X = Screen.PrimaryScreen.WorkingArea.Width + 50;
                    }
                    break;
            }

            this.Location = startLocation;
            this.Visible = true;

            AnimationManager.AnimateLocation(this, originalLocation, 300, Easing.CubicOut, null);
            AnimateFadeIn();
        }

        private void AnimateScaleIn()
        {
            Size originalSize = this.Size;
            this.Size = new Size(originalSize.Width, 0);
            this.Visible = true;
            fadeOpacity = 1.0f;

            AnimationManager.AnimateSize(this, originalSize, 300, Easing.BackOut, null);
        }

        private void AnimateBounceIn()
        {
            Point originalLocation = this.Location;
            this.Location = new Point(originalLocation.X, originalLocation.Y - 50);
            this.Visible = true;
            fadeOpacity = 1.0f;

            AnimationManager.AnimateLocation(this, originalLocation, 500, Easing.BounceOut, null);
        }

        #endregion

        #region 位置管理

        public void SetTargetLocation(Point location)
        {
            targetLocation = location;
        }

        public void AnimateToTargetLocation()
        {
            if (this.Location != targetLocation && !isClosing)
            {
                AnimationManager.AnimateLocation(this, targetLocation, 300, Easing.CubicOut, null);
            }
        }

        #endregion

        #region 事件处理

        private void OnMessageClick(object sender, EventArgs e)
        {
            MessageClicked?.Invoke(this, new MessageClickEventArgs(options));
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 应用淡入淡出效果
            if (fadeOpacity < 1.0f && isFading)
            {
                using (var bitmap = new Bitmap(this.Width, this.Height))
                {
                    using (var tempG = Graphics.FromImage(bitmap))
                    {
                        tempG.SmoothingMode = SmoothingMode.AntiAlias;
                        DrawContent(tempG);
                    }

                    var colorMatrix = new System.Drawing.Imaging.ColorMatrix
                    {
                        Matrix33 = fadeOpacity
                    };

                    using (var attributes = new System.Drawing.Imaging.ImageAttributes())
                    {
                        attributes.SetColorMatrix(colorMatrix);
                        g.DrawImage(bitmap,
                            new Rectangle(0, 0, this.Width, this.Height),
                            0, 0, this.Width, this.Height,
                            GraphicsUnit.Pixel, attributes);
                    }
                }
            }
            else
            {
                DrawContent(g);
            }
        }

        private void DrawContent(Graphics g)
        {
            // 绘制背景
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }

            // 绘制边框
            if (showBorder && borderWidth > 0)
            {
                int radius = 8;
                int halfWidth = borderWidth / 2;
                Rectangle borderRect = new Rectangle(
                    halfWidth,
                    halfWidth,
                    Width - borderWidth,
                    Height - borderWidth);

                using (GraphicsPath path = GetRoundedRectanglePath(borderRect, radius))
                using (Pen pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        #endregion

        #region 辅助方法

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return path;
            }

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);

            if (radius < 1)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            try
            {
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            }
            catch
            {
                path.Reset();
                path.AddRectangle(rect);
            }

            return path;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopAllTimers();

                autoCloseTimer?.Dispose();
                fadeTimer?.Dispose();
                iconBox?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    #region 消息管理器

    public class FluentMessageManager
    {
        private static FluentMessageManager instance;
        private static readonly object lockObject = new object();

        // 消息列表(按位置分组)
        private Dictionary<MessagePosition, List<FluentMessage>> messages;

        // 定时消息
        private List<ScheduledMessage> scheduledMessages;
        private Timer scheduleTimer;

        // 日志记录器
        private IFluentMessageLogger logger;

        // 配置
        private const int MESSAGE_SPACING = 10;
        private const int MESSAGE_MARGIN = 30;
        private const int MAX_MESSAGES_PER_COLUMN = 5;

        public static FluentMessageManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new FluentMessageManager();
                        }
                    }
                }
                return instance;
            }
        }

        private FluentMessageManager()
        {
            messages = new Dictionary<MessagePosition, List<FluentMessage>>();
            foreach (MessagePosition position in Enum.GetValues(typeof(MessagePosition)))
            {
                messages[position] = new List<FluentMessage>();
            }

            scheduledMessages = new List<ScheduledMessage>();

            // 初始化定时器
            scheduleTimer = new Timer();
            scheduleTimer.Interval = 1000; // 每秒检查一次
            scheduleTimer.Tick += CheckScheduledMessages;
            scheduleTimer.Start();
        }

        #region 公共方法

        /// <summary>
        /// 设置日志记录器
        /// </summary>
        public void SetLogger(IFluentMessageLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 使用委托设置日志记录器
        /// </summary>
        /// <param name="logAction">日志记录委托，接收 ILogEvent 参数</param>
        public void SetLogger(Action<ILogEvent> logAction)
        {
            if (logAction != null)
            {
                this.logger = new DefaultFluentMessageLogger(logAction);
            }
        }

        /// <summary>
        /// 添加日志记录器
        /// </summary>
        public void AddLogger(IFluentMessageLogger logger)
        {
            if (logger == null)
            {
                return;
            }

            if (this.logger == null)
            {
                this.logger = logger;
            }
            else if (this.logger is CompositeFluentMessageLogger composite)
            {
                composite.AddLogger(logger);
            }
            else
            {
                // 将现有logger转换为组合logger
                var existingLogger = this.logger;
                this.logger = new CompositeFluentMessageLogger(existingLogger, logger);
            }
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        public FluentMessage Show(MessageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // 记录日志
            if (options.LogMessage && logger != null)
            {
                try
                {
                    var logEvent = MessageLogAdapter.CreateMessageLogEvent(options);
                    logger.AddLog(logEvent);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"记录消息日志失败: {ex.Message}");
                }
            }

            // 创建消息
            FluentMessage message = new FluentMessage(options);
            message.MessageClosed += OnMessageClosed;

            // 根据显示模式处理
            if (options.DisplayMode == MessageDisplayMode.Application)
            {
                ShowApplicationMessage(message, options);
            }
            else
            {
                ShowSystemMessage(message, options);
            }

            return message;
        }

        /// <summary>
        /// 显示成功消息
        /// </summary>
        public FluentMessage Success(string content, string title = null,MessagePosition position = MessagePosition.BottomRight, int duration = 3000)
        {
            return Show(new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Success,
                Duration = duration,
                Position = position,
                LogMessage = false // 成功消息默认不记录日志
            });
        }

        /// <summary>
        /// 显示警告消息
        /// </summary>
        public FluentMessage Warning(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 4000)
        {
            return Show(new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Warning,
                Duration = duration,
                Position = position,
                LogMessage = true // 警告消息默认记录日志
            });
        }

        /// <summary>
        /// 显示信息消息
        /// </summary>
        public FluentMessage Info(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 3000)
        {
            return Show(new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Info,
                Duration = duration,
                Position = position,
                LogMessage = false // 信息消息默认不记录日志
            });
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        public FluentMessage Error(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 5000, Exception exception = null)
        {
            var options = new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Error,
                Duration = duration,
                Position = position,
                LogMessage = true // 错误默认记录日志
            };

            // 如果提供了异常，记录到日志中
            if (exception != null && logger != null)
            {
                try
                {
                    var logEvent = new DefaultLogEvent(LogLevel.Error, content, "FluentMessage", exception, "FluentMessage");

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        logEvent.Properties["Title"] = title;
                    }
                    logEvent.Properties["MessageType"] = MessageType.Error.ToString();

                    logger.AddLog(logEvent);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"记录错误消息日志失败: {ex.Message}");
                }
            }

            return Show(options);
        }

        /// <summary>
        /// 添加定时消息
        /// </summary>
        public Guid Schedule(MessageOptions options, DateTime scheduledTime)
        {
            var scheduled = new ScheduledMessage(options, scheduledTime);
            lock (scheduledMessages)
            {
                scheduledMessages.Add(scheduled);
            }
            return scheduled.Id;
        }

        /// <summary>
        /// 添加延时消息
        /// </summary>
        public Guid Schedule(MessageOptions options, TimeSpan delay)
        {
            return Schedule(options, DateTime.Now.Add(delay));
        }

        /// <summary>
        /// 取消定时消息
        /// </summary>
        public bool CancelScheduled(Guid id)
        {
            lock (scheduledMessages)
            {
                var scheduled = scheduledMessages.FirstOrDefault(s => s.Id == id);
                if (scheduled != null && !scheduled.IsExecuted)
                {
                    scheduledMessages.Remove(scheduled);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取所有待执行的定时消息
        /// </summary>
        public IEnumerable<ScheduledMessage> GetScheduledMessages()
        {
            lock (scheduledMessages)
            {
                return scheduledMessages.Where(s => !s.IsExecuted).ToList();
            }
        }

        /// <summary>
        /// 关闭所有消息
        /// </summary>
        public void CloseAll()
        {
            foreach (var messageList in messages.Values)
            {
                var messagesToClose = messageList.ToArray();
                foreach (var message in messagesToClose)
                {
                    message.Close();
                }
            }
        }

        /// <summary>
        /// 关闭指定位置的所有消息
        /// </summary>
        public void CloseAll(MessagePosition position)
        {
            if (messages.ContainsKey(position))
            {
                var messagesToClose = messages[position].ToArray();
                foreach (var message in messagesToClose)
                {
                    message.Close();
                }
            }
        }

        #endregion

        #region 私有方法

        private void ShowApplicationMessage(FluentMessage message, MessageOptions options)
        {
            Form ownerForm = options.OwnerForm ?? GetActiveForm();

            if (ownerForm == null)
            {
                throw new InvalidOperationException("无法找到所有者窗口，请在MessageOptions中指定OwnerForm");
            }

            // 添加到消息列表
            var messageList = messages[options.Position];

            // 检查是否超过最大数量
            if (messageList.Count >= MAX_MESSAGES_PER_COLUMN)
            {
                var oldest = messageList.FirstOrDefault();
                oldest?.Close();
                // 给一点时间让消息关闭
                Application.DoEvents();
            }

            // 先添加到列表
            messageList.Add(message);

            // 计算位置(使用当前索引)
            int currentIndex = messageList.IndexOf(message);
            Point location = CalculateMessageLocation(ownerForm, message, options.Position, currentIndex);
            message.Location = location;

            // 添加到窗体
            ownerForm.Controls.Add(message);
            message.BringToFront();

            // 播放显示动画
            message.PlayShowAnimation();
        }

        private void ShowSystemMessage(FluentMessage message, MessageOptions options)
        {
            // 添加到消息列表
            var messageList = messages[options.Position];

            if (messageList.Count >= MAX_MESSAGES_PER_COLUMN)
            {
                var oldest = messageList.FirstOrDefault();
                oldest?.Close();
                Application.DoEvents();
            }

            // 先添加到列表
            messageList.Add(message);

            // 计算系统级位置(使用当前索引)
            int currentIndex = messageList.IndexOf(message);
            Point location = CalculateSystemMessageLocation(message, options.Position, currentIndex);

            // 创建系统消息窗体
            SystemMessageForm messageForm = new SystemMessageForm(message);
            messageForm.Location = location;

            // 消息关闭时关闭窗体
            message.MessageClosed += (s, e) =>
            {
                try
                {
                    if (!messageForm.IsDisposed)
                    {
                        messageForm.Close();
                        messageForm.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"关闭系统消息窗体错误: {ex.Message}");
                }
            };

            // 窗体关闭时确保消息也被清理
            messageForm.FormClosed += (s, e) =>
            {
                if (!message.IsDisposed)
                {
                    message.Dispose();
                }
            };

            // 显示窗体
            messageForm.Show();

            // 播放显示动画
            message.PlayShowAnimation();
        }

        private Point CalculateMessageLocation(Form owner, FluentMessage message, MessagePosition position, int index)
        {
            int x = 0, y = 0;
            int spacing = MESSAGE_SPACING;
            int margin = MESSAGE_MARGIN;
            if (owner is FluentForm fluentForm)
            {
                margin = fluentForm.Padding.Top;
            }

            // 计算之前消息的总高度
            int previousHeight = 0;
            var messageList = messages[position];

            // 正确计算前面消息的高度
            for (int i = 0; i < index; i++)
            {
                if (i < messageList.Count)
                {
                    previousHeight += messageList[i].MessageHeight + spacing;
                }
            }

            switch (position)
            {
                case MessagePosition.TopLeft:
                    x = margin;
                    y = margin + previousHeight;
                    break;

                case MessagePosition.TopRight:
                    x = owner.ClientSize.Width - message.Width - margin;
                    y = margin + previousHeight;
                    break;

                case MessagePosition.BottomRight:
                    x = owner.ClientSize.Width - message.Width - margin;
                    y = owner.ClientSize.Height - message.MessageHeight - margin - previousHeight;
                    break;

                case MessagePosition.BottomLeft:
                    x = margin;
                    y = owner.ClientSize.Height - message.MessageHeight - margin - previousHeight;
                    break;
            }

            return new Point(x, y);
        }

        private Point CalculateSystemMessageLocation(FluentMessage message, MessagePosition position, int index)
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int x = 0, y = 0;
            int spacing = MESSAGE_SPACING;
            int margin = MESSAGE_MARGIN;

            // 计算之前消息的总高度
            int previousHeight = 0;
            var messageList = messages[position];

            // 正确计算前面消息的高度
            for (int i = 0; i < index; i++)
            {
                if (i < messageList.Count)
                {
                    previousHeight += messageList[i].MessageHeight + spacing;
                }
            }

            switch (position)
            {
                case MessagePosition.TopLeft:
                    x = workingArea.Left + margin;
                    y = workingArea.Top + margin + previousHeight;
                    break;

                case MessagePosition.TopRight:
                    x = workingArea.Right - message.Width - margin;
                    y = workingArea.Top + margin + previousHeight;
                    break;

                case MessagePosition.BottomRight:
                    x = workingArea.Right - message.Width - margin;
                    y = workingArea.Bottom - message.MessageHeight - margin - previousHeight;
                    break;

                case MessagePosition.BottomLeft:
                    x = workingArea.Left + margin;
                    y = workingArea.Bottom - message.MessageHeight - margin - previousHeight;
                    break;
            }

            return new Point(x, y);
        }

        private Form GetActiveForm()
        {
            return Form.ActiveForm ?? Application.OpenForms.Cast<Form>().FirstOrDefault();
        }

        private void OnMessageClosed(object sender, EventArgs e)
        {
            if (sender is FluentMessage message)
            {
                // 从列表中移除
                foreach (var messageList in messages.Values)
                {
                    if (messageList.Contains(message))
                    {
                        messageList.Remove(message);

                        // 重新排列剩余消息
                        RearrangeMessages(message.Options.Position);
                        break;
                    }
                }
            }
        }

        private void RearrangeMessages(MessagePosition position)
        {
            var messageList = messages[position];

            for (int i = 0; i < messageList.Count; i++)
            {
                var msg = messageList[i];
                Form owner = msg.Parent as Form;

                if (owner != null)
                {
                    Point newLocation = CalculateMessageLocation(owner, msg, position, i);
                    msg.SetTargetLocation(newLocation);
                    msg.AnimateToTargetLocation();
                }
            }
        }

        private void CheckScheduledMessages(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            List<ScheduledMessage> toExecute;

            lock (scheduledMessages)
            {
                toExecute = scheduledMessages
                    .Where(s => !s.IsExecuted && s.ScheduledTime <= now)
                    .ToList();
            }

            foreach (var scheduled in toExecute)
            {
                scheduled.IsExecuted = true;

                try
                {
                    Show(scheduled.Options);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"显示定时消息失败: {ex.Message}");

                    // 记录错误到日志
                    if (logger != null)
                    {
                        try
                        {
                            var logEvent = new DefaultLogEvent(LogLevel.Error, "显示定时消息失败", "FluentMessageManager", ex, "FluentMessage");
                            logger.AddLog(logEvent);
                        }
                        catch
                        {
                            // 忽略日志错误
                        }
                    }
                }
            }

            // 清理已执行的消息(保留最近5分钟的记录)
            lock (scheduledMessages)
            {
                scheduledMessages.RemoveAll(s => s.IsExecuted && s.ScheduledTime < now.AddMinutes(-5));
            }
        }

        #endregion
    }

    #endregion

    #region 消息配置

    /// <summary>
    /// 消息配置选项
    /// </summary>
    public class MessageOptions
    {
        public MessageOptions()
        {
        }

        public MessageOptions(string content, MessageType type = MessageType.Info)
        {
            Content = content;
            Type = type;

            // 错误类型默认记录日志
            if (type == MessageType.Error)
            {
                LogMessage = true;
            }
        }

        public MessageOptions(string title, string content, MessageType type = MessageType.Info)
            : this(content, type)
        {
            Title = title;
        }

        /// <summary>
        /// 消息标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; set; } = MessageType.Info;

        /// <summary>
        /// 显示位置
        /// </summary>
        public MessagePosition Position { get; set; } = MessagePosition.TopRight;

        /// <summary>
        /// 动画类型
        /// </summary>
        public MessageAnimation Animation { get; set; } = MessageAnimation.Slide;

        /// <summary>
        /// 显示模式
        /// </summary>
        public MessageDisplayMode DisplayMode { get; set; } = MessageDisplayMode.Application;

        /// <summary>
        /// 持续时间(毫秒)，0表示不自动关闭
        /// </summary>
        public int Duration { get; set; } = 3000;

        /// <summary>
        /// 是否显示图标
        /// </summary>
        public bool ShowIcon { get; set; } = true;

        /// <summary>
        /// 是否记录到日志
        /// </summary>
        public bool LogMessage { get; set; }

        /// <summary>
        /// 自定义图标
        /// </summary>
        public Image CustomIcon { get; set; }

        /// <summary>
        /// 应用消息的所有者窗口
        /// </summary>
        public Form OwnerForm { get; set; }

        /// <summary>
        /// 是否可以关闭
        /// </summary>
        public bool Closable { get; set; } = true;

        /// <summary>
        /// 消息宽度
        /// </summary>
        public int Width { get; set; } = 350;

        /// <summary>
        /// 鼠标悬停时暂停自动关闭
        /// </summary>
        public bool PauseOnHover { get; set; } = true;

    }

    /// <summary>
    /// 定时消息
    /// </summary>
    public class ScheduledMessage
    {
        public ScheduledMessage(MessageOptions options, DateTime scheduledTime)
        {
            Id = Guid.NewGuid();
            Options = options ?? throw new ArgumentNullException(nameof(options));
            ScheduledTime = scheduledTime;
            IsExecuted = false;
        }

        public ScheduledMessage(MessageOptions options, TimeSpan delay)
            : this(options, DateTime.Now.Add(delay))
        {
        }

        public Guid Id { get; private set; }
        public MessageOptions Options { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsExecuted { get; internal set; }
    }

    #endregion

    #region 系统消息窗口

    public class SystemMessageForm : Form
    {
        private FluentMessage message;

        public SystemMessageForm(FluentMessage message)
        {
            this.message = message ?? throw new ArgumentNullException(nameof(message));

            // 窗体设置
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = message.Size;
            this.AutoScaleMode = AutoScaleMode.None;

            // 关键：设置透明背景
            this.BackColor = Color.LimeGreen;
            this.TransparencyKey = Color.LimeGreen;

            // 双缓冲
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            this.DoubleBuffered = true;

            // 添加消息
            message.Location = new Point(0, 0);
            message.Anchor = AnchorStyles.None;
            this.Controls.Add(message);

            // 消息大小改变时调整窗体
            message.SizeChanged += (s, e) =>
            {
                if (!this.IsDisposed)
                {
                    this.Size = message.Size;
                }
            };

            // 消息关闭时关闭窗体
            message.MessageClosed += (s, e) =>
            {
                this.Close();
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cp;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                message?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    #endregion

    #region 消息日志

    /// <summary>
    /// 消息日志接口
    /// </summary>
    public interface IFluentMessageLogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        void AddLog(ILogEvent logEvent);
    }

    /// <summary>
    /// 默认的消息日志记录器
    /// </summary>
    public class DefaultFluentMessageLogger : IFluentMessageLogger
    {
        private readonly Action<ILogEvent> logAction;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logAction">日志记录委托</param>
        public DefaultFluentMessageLogger(Action<ILogEvent> logAction)
        {
            this.logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
        }

        public void AddLog(ILogEvent logEvent)
        {
            if (logEvent != null)
            {
                logAction?.Invoke(logEvent);
            }
        }
    }

    /// <summary>
    /// 文件消息日志记录器
    /// </summary>
    public class FileFluentMessageLogger : IFluentMessageLogger
    {
        private readonly string logFilePath;
        private readonly object lockObject = new object();

        public FileFluentMessageLogger(string logFilePath = null)
        {
            this.logFilePath = logFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "message.log");

            // 确保目录存在
            var directory = Path.GetDirectoryName(this.logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void AddLog(ILogEvent logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            lock (lockObject)
            {
                try
                {
                    var logEntry = FormatLogEntry(logEvent);
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"写入消息日志失败: {ex.Message}");
                }
            }
        }

        private string FormatLogEntry(ILogEvent logEvent)
        {
            var sb = new StringBuilder();

            // 时间戳
            sb.Append($"[{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss.fff}]");

            // 日志级别
            sb.Append($" [{logEvent.Level,-5}]");

            // 来源
            if (!string.IsNullOrEmpty(logEvent.Source))
            {
                sb.Append($" [{logEvent.Source}]");
            }

            // 分组
            if (!string.IsNullOrEmpty(logEvent.Group))
            {
                sb.Append($" [{logEvent.Group}]");
            }

            // 标题(如果有)
            if (logEvent.Properties != null && logEvent.Properties.ContainsKey("Title"))
            {
                sb.Append($" {logEvent.Properties["Title"]}:");
            }

            // 消息内容
            sb.Append($" {logEvent.Message}");

            // 异常信息
            if (logEvent.Exception != null)
            {
                sb.AppendLine();
                sb.Append($"  Exception: {logEvent.Exception}");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 控制台消息日志记录器
    /// </summary>
    public class ConsoleFluentMessageLogger : IFluentMessageLogger
    {
        public void AddLog(ILogEvent logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            ConsoleColor color;
            switch (logEvent.Level)
            {
                case LogLevel.Error:
                    color = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    color = ConsoleColor.DarkRed;
                    break;
                case LogLevel.Warn:
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.Info:
                    color = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                    color = ConsoleColor.Gray;
                    break;
                case LogLevel.Trace:
                    color = ConsoleColor.DarkGray;
                    break;
                default:
                    color = ConsoleColor.White;
                    break;
            }

            var originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;

                var message = $"[{logEvent.Timestamp:HH:mm:ss}] [{logEvent.Level}] {logEvent.Message}";

                if (logEvent.Properties != null && logEvent.Properties.ContainsKey("Title"))
                {
                    message = $"[{logEvent.Timestamp:HH:mm:ss}] [{logEvent.Level}] {logEvent.Properties["Title"]}: {logEvent.Message}";
                }

                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
    }

    /// <summary>
    /// 组合日志记录器
    /// (可以同时写入多个日志目标)
    /// </summary>
    public class CompositeFluentMessageLogger : IFluentMessageLogger
    {
        private readonly List<IFluentMessageLogger> loggers;

        public CompositeFluentMessageLogger()
        {
            loggers = new List<IFluentMessageLogger>();
        }

        public CompositeFluentMessageLogger(params IFluentMessageLogger[] loggers)
        {
            this.loggers = new List<IFluentMessageLogger>(loggers);
        }

        public void AddLogger(IFluentMessageLogger logger)
        {
            if (logger != null && !loggers.Contains(logger))
            {
                loggers.Add(logger);
            }
        }

        public void RemoveLogger(IFluentMessageLogger logger)
        {
            loggers.Remove(logger);
        }

        public void AddLog(ILogEvent logEvent)
        {
            foreach (var logger in loggers)
            {
                try
                {
                    logger.AddLog(logEvent);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"日志记录器错误: {ex.Message}");
                }
            }
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        Success,
        Warning,
        Info,
        Error
    }

    /// <summary>
    /// 消息位置
    /// </summary>
    public enum MessagePosition
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

    /// <summary>
    /// 消息动画类型
    /// </summary>
    public enum MessageAnimation
    {
        Fade,           // 淡入淡出
        Slide,          // 滑动
        Bounce,         // 弹跳
        Scale,          // 缩放
        SlideAndFade    // 滑动+淡入
    }

    /// <summary>
    /// 消息显示模式
    /// </summary>
    public enum MessageDisplayMode
    {
        Application,    // 应用内消息
        System          // 系统消息
    }

    public class MessageClickEventArgs : EventArgs
    {
        public MessageOptions Options { get; }

        public MessageClickEventArgs(MessageOptions options)
        {
            Options = options;
        }
    }

    #endregion
}
