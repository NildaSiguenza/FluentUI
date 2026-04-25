using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Animation;
using System.Windows.Forms;
using FluentControls.IconFonts;
using System.Drawing.Design;

namespace FluentControls.Controls
{
    [DefaultEvent("Click")]
    public class FluentIcon : FluentControlBase
    {
        #region 字段

        // 图标相关
        private Image icon;
        private Size iconSize = new Size(24, 24);
        private Color iconColor = Color.Empty;
        private float rotationAngle = 0f;
        private float skewX = 0f;
        private float skewY = 0f;
        private IconFlipMode flipMode = IconFlipMode.None;

        // 悬停背景相关
        private Color hoverBackColor = Color.FromArgb(255, 237, 237, 237);
        private Color pressedBackColor = Color.FromArgb(200, 237, 237, 237);
        private IconBackgroundShape backgroundShape = IconBackgroundShape.Circle;
        private int backgroundCornerRadius = 4;
        private Padding iconPadding = new Padding(8);
        private bool showHoverBackground = true;

        // 悬停颜色变化
        private Color hoverIconColor = Color.Empty;
        private bool changeColorOnHover = false;

        // Tooltip
        private string tooltipText = "";
        private ToolTip toolTip;

        // 动画相关
        private float hoverProgress = 0f;
        private Timer hoverAnimationTimer;
        private bool isHovering = false;
        private const int HOVER_ANIMATION_INTERVAL = 16;
        private const float HOVER_ANIMATION_STEP = 0.1f;

        // 旋转动画相关字段
        private Timer rotationAnimationTimer;
        private float rotationStartAngle;
        private float rotationTargetAngle;
        private int rotationCurrentStep;
        private int rotationTotalSteps;
        private Action rotationOnComplete;

        // 图标大小动画相关
        private Timer sizeAnimationTimer;
        private Size sizeStart;
        private Size sizeTarget;
        private int sizeCurrentStep;
        private int sizeTotalSteps;
        private AnimationState.EasingFunction sizeEasing;
        private Action sizeOnComplete;

        // 脉冲动画相关
        private float pulseScale = 1.0f;
        private Timer pulseAnimationTimer;
        private float pulseStartScale;
        private float pulseTargetScale;
        private int pulseCurrentStep;
        private int pulseTotalSteps;
        private int pulseCount;
        private int pulseCurrentCount;
        private int pulseDuration;
        private Action pulseOnComplete;

        // 缓存
        private Image coloredIconCache;
        private Color cachedIconColor = Color.Empty;
        private Image cachedSourceIcon;

        // 光标
        private bool useHandCursor = true;

        // 同步标志
        private bool isSynchronizingSize = false;

        #endregion

        #region 构造函数

        public FluentIcon()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            // 初始化 ToolTip
            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true
            };

            // 初始化悬停动画计时器
            hoverAnimationTimer = new Timer
            {
                Interval = HOVER_ANIMATION_INTERVAL
            };
            hoverAnimationTimer.Tick += OnHoverAnimationTick;

            // 设置默认大小
            Size = new Size(40, 40);

            // 不使用水波纹
            EnableRippleEffect = false;
            // 默认启用动画
            EnableAnimation = true;

            // 默认 Size 应为 40x40
            isSynchronizingSize = true;
            try
            {
                Size = new Size(
                    iconSize.Width + iconPadding.Horizontal,
                    iconSize.Height + iconPadding.Vertical);
            }
            finally
            {
                isSynchronizingSize = false;
            }
        }

        #endregion

        #region 属性

        [Category("FluentIcon")]
        [Description("要显示的图标图像")]
        [DefaultValue(null)]
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image Icon
        {
            get => icon;
            set
            {
                if (icon != value)
                {
                    icon = value;
                    InvalidateIconCache();
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("图标的显示大小")]
        [DefaultValue(typeof(Size), "24, 24")]
        public Size IconSize
        {
            get => iconSize;
            set
            {
                value = new Size(Math.Max(1, value.Width), Math.Max(1, value.Height));

                if (iconSize != value)
                {
                    iconSize = value;
                    InvalidateIconCache();

                    // 同步更新控件Size
                    SyncSizeFromIconSize();

                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("图标的着色颜色，设置为 Empty 使用原始颜色")]
        [DefaultValue(typeof(Color), "")]
        public Color IconColor
        {
            get => iconColor;
            set
            {
                if (iconColor != value)
                {
                    iconColor = value;
                    InvalidateIconCache();
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("鼠标悬停时图标的颜色")]
        [DefaultValue(typeof(Color), "")]
        public Color HoverIconColor
        {
            get => hoverIconColor;
            set
            {
                if (hoverIconColor != value)
                {
                    hoverIconColor = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("是否在鼠标悬停时改变图标颜色")]
        [DefaultValue(false)]
        public bool ChangeColorOnHover
        {
            get => changeColorOnHover;
            set
            {
                if (changeColorOnHover != value)
                {
                    changeColorOnHover = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("图标的旋转角度(0-360度)")]
        [DefaultValue(0f)]
        public float RotationAngle
        {
            get => rotationAngle;
            set
            {
                // 规范化到 0-360
                value = value % 360;
                if (value < 0)
                {
                    value += 360;
                }

                if (Math.Abs(rotationAngle - value) > 0.001f)
                {
                    rotationAngle = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("X 轴倾斜度(-1 到 1)")]
        [DefaultValue(0f)]
        public float SkewX
        {
            get => skewX;
            set
            {
                value = Math.Max(-1f, Math.Min(1f, value));
                if (Math.Abs(skewX - value) > 0.001f)
                {
                    skewX = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("Y 轴倾斜度(-1 到 1)")]
        [DefaultValue(0f)]
        public float SkewY
        {
            get => skewY;
            set
            {
                value = Math.Max(-1f, Math.Min(1f, value));
                if (Math.Abs(skewY - value) > 0.001f)
                {
                    skewY = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("图标的翻转模式")]
        [DefaultValue(IconFlipMode.None)]
        public IconFlipMode FlipMode
        {
            get => flipMode;
            set
            {
                if (flipMode != value)
                {
                    flipMode = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("是否在悬停时显示背景")]
        [DefaultValue(true)]
        public bool ShowHoverBackground
        {
            get => showHoverBackground;
            set
            {
                if (showHoverBackground != value)
                {
                    showHoverBackground = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("鼠标悬停时的背景颜色")]
        [DefaultValue(typeof(Color), "255,237,237,237")]
        public Color HoverBackColor
        {
            get => hoverBackColor;
            set
            {
                if (hoverBackColor != value)
                {
                    hoverBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("鼠标按下时的背景颜色")]
        [DefaultValue(typeof(Color), "200,237,237,237")]
        public Color PressedBackColor
        {
            get => pressedBackColor;
            set
            {
                if (pressedBackColor != value)
                {
                    pressedBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("悬停背景的形状")]
        [DefaultValue(IconBackgroundShape.Circle)]
        public IconBackgroundShape BackgroundShape
        {
            get => backgroundShape;
            set
            {
                if (backgroundShape != value)
                {
                    backgroundShape = value;
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("圆角矩形背景的圆角半径")]
        [DefaultValue(4)]
        public int BackgroundCornerRadius
        {
            get => backgroundCornerRadius;
            set
            {
                if (backgroundCornerRadius != value)
                {
                    backgroundCornerRadius = Math.Max(0, value);
                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("图标周围的内边距")]
        [DefaultValue(typeof(Padding), "8, 8, 8, 8")]
        public Padding IconPadding
        {
            get => iconPadding;
            set
            {
                if (iconPadding != value)
                {
                    iconPadding = value;
                    SyncSizeFromIconSize();

                    Invalidate();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("鼠标悬停时显示的提示文本")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TooltipText
        {
            get => tooltipText;
            set
            {
                if (tooltipText != value)
                {
                    tooltipText = value ?? "";
                    UpdateTooltip();
                }
            }
        }

        [Category("FluentIcon")]
        [Description("Tooltip 显示前的延迟时间(毫秒)")]
        [DefaultValue(500)]
        public int TooltipInitialDelay
        {
            get => toolTip.InitialDelay;
            set => toolTip.InitialDelay = value;
        }

        [Category("FluentIcon")]
        [Description("鼠标悬停时是否显示手型光标")]
        [DefaultValue(true)]
        public bool UseHandCursor
        {
            get => useHandCursor;
            set
            {
                if (useHandCursor != value)
                {
                    useHandCursor = value;
                    UpdateCursor();
                }
            }
        }

        [Browsable(false)]
        public float HoverProgress => hoverProgress;

        #endregion

        #region 主题支持

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                // 使用主题颜色
                if (iconColor.IsEmpty)
                {
                    iconColor = GetThemeColor(c => c.TextPrimary, Color.Black);
                    InvalidateIconCache();
                }

                if (hoverIconColor.IsEmpty && changeColorOnHover)
                {
                    hoverIconColor = GetThemeColor(c => c.Primary, Color.Blue);
                }
            }
        }

        #endregion

        #region 绘制方法

        protected override void DrawBackground(Graphics g)
        {
            // 默认透明，不绘制背景
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制悬停背景
            DrawHoverBackground(g);

            // 绘制图标
            DrawIcon(g);
        }

        protected override void DrawBorder(Graphics g)
        {
            // 图标控件默认不绘制边框
        }

        /// <summary>
        /// 绘制悬停背景
        /// </summary>
        private void DrawHoverBackground(Graphics g)
        {
            if (!showHoverBackground || backgroundShape == IconBackgroundShape.None)
            {
                return;
            }

            if (hoverProgress <= 0 && State != ControlState.Pressed)
            {
                return;
            }

            // 计算背景颜色和透明度
            Color bgColor;
            float alpha;

            if (State == ControlState.Pressed)
            {
                bgColor = pressedBackColor;
                alpha = 1f;
            }
            else
            {
                bgColor = hoverBackColor;
                alpha = hoverProgress;
            }

            if (alpha <= 0)
            {
                return;
            }

            // 应用透明度
            Color finalColor = Color.FromArgb(
                (int)(bgColor.A * alpha),
                bgColor.R,
                bgColor.G,
                bgColor.B);

            Rectangle bgRect = GetBackgroundRect();

            using (var brush = new SolidBrush(finalColor))
            {
                switch (backgroundShape)
                {
                    case IconBackgroundShape.Circle:
                        g.FillEllipse(brush, bgRect);
                        break;

                    case IconBackgroundShape.Square:
                        g.FillRectangle(brush, bgRect);
                        break;

                    case IconBackgroundShape.RoundedRectangle:
                        using (var path = GetRoundedRectangle(bgRect, backgroundCornerRadius))
                        {
                            g.FillPath(brush, path);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 获取背景矩形
        /// </summary>
        private Rectangle GetBackgroundRect()
        {
            int size = Math.Min(Width, Height);
            return new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);
        }

        /// <summary>
        /// 绘制图标
        /// </summary>
        private void DrawIcon(Graphics g)
        {
            if (icon == null)
            {
                return;
            }

            // 计算图标绘制区域
            Rectangle iconRect = GetIconRect();

            if (iconRect.Width <= 0 || iconRect.Height <= 0)
            {
                return;
            }

            // 保存图形状态
            var state = g.Save();

            try
            {
                // 计算变换中心
                float centerX = iconRect.X + iconRect.Width / 2f;
                float centerY = iconRect.Y + iconRect.Height / 2f;

                // 创建变换矩阵
                using (var matrix = new Matrix())
                {
                    // 移动到中心
                    matrix.Translate(centerX, centerY);

                    // 应用旋转
                    if (Math.Abs(rotationAngle) > 0.001f)
                    {
                        matrix.Rotate(rotationAngle);
                    }

                    // 应用倾斜
                    if (Math.Abs(skewX) > 0.001f || Math.Abs(skewY) > 0.001f)
                    {
                        matrix.Shear(skewX, skewY);
                    }

                    // 应用翻转
                    float scaleX = 1f;
                    float scaleY = 1f;

                    if ((flipMode & IconFlipMode.Horizontal) != 0)
                    {
                        scaleX = -1f;
                    }
                    if ((flipMode & IconFlipMode.Vertical) != 0)
                    {
                        scaleY = -1f;
                    }

                    if (scaleX != 1f || scaleY != 1f)
                    {
                        matrix.Scale(scaleX, scaleY);
                    }

                    // 移回原位
                    matrix.Translate(-centerX, -centerY);

                    g.Transform = matrix;
                }

                // 获取要绘制的图标
                Image imageToDraw = GetIconToDraw();

                if (imageToDraw != null)
                {
                    // 设置高质量插值
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // 如果禁用，绘制为半透明
                    if (!Enabled)
                    {
                        using (var attributes = new ImageAttributes())
                        {
                            var colorMatrix = new ColorMatrix
                            {
                                Matrix33 = 0.5f // 50% 透明度
                            };
                            attributes.SetColorMatrix(colorMatrix);

                            g.DrawImage(imageToDraw, iconRect,
                                0, 0, imageToDraw.Width, imageToDraw.Height,
                                GraphicsUnit.Pixel, attributes);
                        }
                    }
                    else
                    {
                        g.DrawImage(imageToDraw, iconRect);
                    }
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        /// <summary>
        /// 获取图标绘制矩形
        /// </summary>
        private Rectangle GetIconRect()
        {
            // 应用脉冲缩放
            int scaledWidth = (int)(iconSize.Width * pulseScale);
            int scaledHeight = (int)(iconSize.Height * pulseScale);

            // 居中绘制
            int x = (Width - scaledWidth) / 2;
            int y = (Height - scaledHeight) / 2;

            return new Rectangle(x, y, scaledWidth, scaledHeight);
        }

        /// <summary>
        /// 获取要绘制的图标(处理着色)
        /// </summary>
        private Image GetIconToDraw()
        {
            if (icon == null)
            {
                return null;
            }

            // 确定当前应该使用的颜色
            Color colorToUse = iconColor;

            if (changeColorOnHover && !hoverIconColor.IsEmpty && hoverProgress > 0)
            {
                if (iconColor.IsEmpty)
                {
                    // 如果原始颜色为空，直接使用悬停颜色
                    colorToUse = Color.FromArgb(
                        (int)(hoverIconColor.A * hoverProgress),
                        hoverIconColor.R,
                        hoverIconColor.G,
                        hoverIconColor.B);
                }
                else
                {
                    // 混合两种颜色
                    colorToUse = BlendColors(iconColor, hoverIconColor, hoverProgress);
                }
            }

            // 如果不需要着色，返回原始图标
            if (colorToUse.IsEmpty)
            {
                return icon;
            }

            // 检查缓存
            if (coloredIconCache != null &&
                cachedIconColor == colorToUse &&
                cachedSourceIcon == icon)
            {
                return coloredIconCache;
            }

            // 创建着色图标
            coloredIconCache?.Dispose();
            coloredIconCache = ColorizeImage(icon, colorToUse);
            cachedIconColor = colorToUse;
            cachedSourceIcon = icon;

            return coloredIconCache;
        }

        /// <summary>
        /// 将图像着色为指定颜色
        /// </summary>
        private Image ColorizeImage(Image source, Color color)
        {
            if (source == null)
            {
                return null;
            }

            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                // 创建颜色矩阵，将图像转换为指定颜色
                // 保持原始透明度
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { color.R / 255f, color.G / 255f, color.B / 255f, 0, 1 }
                });

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);

                    g.DrawImage(source,
                        new Rectangle(0, 0, result.Width, result.Height),
                        0, 0, source.Width, source.Height,
                        GraphicsUnit.Pixel,
                        attributes);
                }
            }

            return result;
        }

        /// <summary>
        /// 使缓存失效
        /// </summary>
        private void InvalidateIconCache()
        {
            coloredIconCache?.Dispose();
            coloredIconCache = null;
            cachedSourceIcon = null;
        }

        #endregion

        #region 状态处理

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            isHovering = true;
            UpdateCursor();

            if (EnableAnimation)
            {
                hoverAnimationTimer.Start();
            }
            else
            {
                hoverProgress = 1f;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            isHovering = false;
            Cursor = Cursors.Default;

            if (EnableAnimation)
            {
                hoverAnimationTimer.Start();
            }
            else
            {
                hoverProgress = 0f;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            UpdateCursor();
            Invalidate();
        }

        /// <summary>
        /// 悬停动画更新
        /// </summary>
        private void OnHoverAnimationTick(object sender, EventArgs e)
        {
            if (isHovering)
            {
                hoverProgress += HOVER_ANIMATION_STEP;
                if (hoverProgress >= 1f)
                {
                    hoverProgress = 1f;
                    hoverAnimationTimer.Stop();
                }
            }
            else
            {
                hoverProgress -= HOVER_ANIMATION_STEP;
                if (hoverProgress <= 0f)
                {
                    hoverProgress = 0f;
                    hoverAnimationTimer.Stop();
                }
            }

            // 如果颜色会改变，需要清除缓存
            if (changeColorOnHover && !hoverIconColor.IsEmpty)
            {
                InvalidateIconCache();
            }

            Invalidate();
        }

        /// <summary>
        /// 更新光标
        /// </summary>
        private void UpdateCursor()
        {
            if (Enabled && useHandCursor && isHovering)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 更新 Tooltip
        /// </summary>
        private void UpdateTooltip()
        {
            if (string.IsNullOrEmpty(tooltipText))
            {
                toolTip.SetToolTip(this, null);
            }
            else
            {
                toolTip.SetToolTip(this, tooltipText);
            }
        }

        #endregion

        #region 尺寸处理

        /// <summary>
        /// 根据 IconSize 和 Padding 同步控件 Size
        /// </summary>
        private void SyncSizeFromIconSize()
        {
            if (isSynchronizingSize)
            {
                return;
            }

            isSynchronizingSize = true;
            try
            {
                Size newSize = new Size(
                    iconSize.Width + iconPadding.Horizontal,
                    iconSize.Height + iconPadding.Vertical);

                if (Size != newSize)
                {
                    Size = newSize;
                }
            }
            finally
            {
                isSynchronizingSize = false;
            }
        }

        /// <summary>
        /// 根据控件 Size 和 Padding 同步 IconSize
        /// </summary>
        private void SyncIconSizeFromSize()
        {
            if (isSynchronizingSize)
            {
                return;
            }

            isSynchronizingSize = true;
            try
            {
                Size newIconSize = new Size(
                    Math.Max(1, Width - iconPadding.Horizontal),
                    Math.Max(1, Height - iconPadding.Vertical));

                if (iconSize != newIconSize)
                {
                    iconSize = newIconSize;
                    InvalidateIconCache();
                }
            }
            finally
            {
                isSynchronizingSize = false;
            }
        }

        /// <summary>
        /// 重写 SetBoundsCore 以捕获 Size 变化
        /// </summary>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // 如果正在同步中，直接调用基类
            if (isSynchronizingSize)
            {
                base.SetBoundsCore(x, y, width, height, specified);
                return;
            }

            // 检查是否指定了宽度或高度
            bool sizeChanging = (specified & BoundsSpecified.Size) != 0 ||
                                (specified & BoundsSpecified.Width) != 0 ||
                                (specified & BoundsSpecified.Height) != 0;

            // 先调用基类设置边界
            base.SetBoundsCore(x, y, width, height, specified);

            // 如果尺寸发生变化，同步 IconSize
            if (sizeChanging)
            {
                SyncIconSizeFromSize();
            }
        }

        /// <summary>
        /// 重写 OnResize 确保同步
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 确保 IconSize 同步（双重保障）
            SyncIconSizeFromSize();
        }

        /// <summary>
        /// 获取首选尺寸
        /// </summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(
                iconSize.Width + iconPadding.Horizontal,
                iconSize.Height + iconPadding.Vertical);
        }

        /// <summary>
        /// 自动调整大小以适应图标
        /// </summary>
        public void SizeToFit()
        {
            Size = GetPreferredSize(Size.Empty);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置图标并自动调整大小
        /// </summary>
        public void SetIcon(Image image, Size? size = null, bool autoResize = true)
        {
            icon = image;

            if (size.HasValue)
            {
                // 使用指定的大小
                IconSize = size.Value;  // 自动同步 Size
            }
            else if (image != null && autoResize)
            {
                // 使用图像原始大小
                IconSize = image.Size;  // 自动同步 Size
            }

            InvalidateIconCache();
            Invalidate();
        }

        /// <summary>
        /// 旋转图标（动画）
        /// </summary>
        /// <param name="targetAngle">目标角度</param>
        /// <param name="duration">持续时间（毫秒）</param>
        /// <param name="onComplete">完成回调</param>
        public void AnimateRotation(float targetAngle, int duration = 300, Action onComplete = null)
        {
            // 停止之前的旋转动画
            StopRotationAnimation();

            if (!EnableAnimation || duration <= 0)
            {
                RotationAngle = targetAngle;
                onComplete?.Invoke();
                return;
            }

            rotationStartAngle = rotationAngle;
            float delta = targetAngle - rotationStartAngle;

            // 规范化旋转方向
            if (delta > 180)
            {
                delta -= 360;
            }

            if (delta < -180)
            {
                delta += 360;
            }

            rotationTargetAngle = rotationStartAngle + delta;
            rotationOnComplete = onComplete;

            const int frameInterval = 16;
            rotationTotalSteps = Math.Max(1, duration / frameInterval);
            rotationCurrentStep = 0;

            rotationAnimationTimer = new Timer { Interval = frameInterval };
            rotationAnimationTimer.Tick += OnRotationAnimationTick;
            rotationAnimationTimer.Start();
        }

        private void OnRotationAnimationTick(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                StopRotationAnimation();
                return;
            }

            rotationCurrentStep++;
            double progress = Math.Min(1.0, (double)rotationCurrentStep / rotationTotalSteps);
            double easedProgress = Easing.CubicOut(progress);

            rotationAngle = rotationStartAngle + (float)((rotationTargetAngle - rotationStartAngle) * easedProgress);
            Invalidate();

            if (rotationCurrentStep >= rotationTotalSteps)
            {
                // 规范化最终角度
                rotationAngle = rotationTargetAngle % 360;
                if (rotationAngle < 0)
                {
                    rotationAngle += 360;
                }

                var callback = rotationOnComplete;
                StopRotationAnimation();
                Invalidate();
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 停止旋转动画
        /// </summary>
        public void StopRotationAnimation()
        {
            if (rotationAnimationTimer != null)
            {
                rotationAnimationTimer.Stop();
                rotationAnimationTimer.Dispose();
                rotationAnimationTimer = null;
            }
            rotationOnComplete = null;
        }

        /// <summary>
        /// 连续旋转动画
        /// </summary>
        /// <param name="degreesPerSecond">每秒旋转度数</param>
        /// <param name="clockwise">是否顺时针</param>
        public void StartContinuousRotation(float degreesPerSecond = 360, bool clockwise = true)
        {
            StopRotationAnimation();

            const int frameInterval = 16;
            float degreesPerFrame = degreesPerSecond * frameInterval / 1000f;
            if (!clockwise)
            {
                degreesPerFrame = -degreesPerFrame;
            }

            rotationAnimationTimer = new Timer { Interval = frameInterval };
            rotationAnimationTimer.Tick += (s, e) =>
            {
                if (IsDisposed)
                {
                    StopRotationAnimation();
                    return;
                }

                rotationAngle = (rotationAngle + degreesPerFrame) % 360;
                if (rotationAngle < 0)
                {
                    rotationAngle += 360;
                }

                Invalidate();
            };
            rotationAnimationTimer.Start();
        }

        /// <summary>
        /// 停止连续旋转
        /// </summary>
        public void StopContinuousRotation()
        {
            StopRotationAnimation();
        }

        /// <summary>
        /// 执行脉冲效果
        /// </summary>
        /// <param name="scaleFactor">最大缩放因子</param>
        /// <param name="duration">单次脉冲持续时间（毫秒）</param>
        /// <param name="count">脉冲次数</param>
        /// <param name="onComplete">完成回调</param>
        public void Pulse(float scaleFactor = 1.2f, int duration = 300, int count = 1, Action onComplete = null)
        {
            StopPulseAnimation();

            if (!EnableAnimation || count <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            pulseCount = count;
            pulseCurrentCount = 0;
            pulseDuration = duration;
            pulseOnComplete = onComplete;

            StartPulseExpand(scaleFactor);
        }

        private void StartPulseExpand(float targetScale)
        {
            pulseStartScale = pulseScale;
            pulseTargetScale = targetScale;

            const int frameInterval = 16;
            pulseTotalSteps = Math.Max(1, pulseDuration / 2 / frameInterval);
            pulseCurrentStep = 0;

            if (pulseAnimationTimer == null)
            {
                pulseAnimationTimer = new Timer { Interval = frameInterval };
                pulseAnimationTimer.Tick += OnPulseExpandTick;
            }
            else
            {
                pulseAnimationTimer.Tick -= OnPulseExpandTick;
                pulseAnimationTimer.Tick -= OnPulseContractTick;
                pulseAnimationTimer.Tick += OnPulseExpandTick;
            }

            pulseAnimationTimer.Start();
        }

        private void OnPulseExpandTick(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                StopPulseAnimation();
                return;
            }

            pulseCurrentStep++;
            double progress = Math.Min(1.0, (double)pulseCurrentStep / pulseTotalSteps);
            double easedProgress = Easing.QuadOut(progress);

            pulseScale = pulseStartScale + (float)((pulseTargetScale - pulseStartScale) * easedProgress);
            Invalidate();

            if (pulseCurrentStep >= pulseTotalSteps)
            {
                pulseAnimationTimer.Stop();
                StartPulseContract();
            }
        }

        private void StartPulseContract()
        {
            pulseStartScale = pulseScale;
            pulseTargetScale = 1.0f;

            const int frameInterval = 16;
            pulseTotalSteps = Math.Max(1, pulseDuration / 2 / frameInterval);
            pulseCurrentStep = 0;

            pulseAnimationTimer.Tick -= OnPulseExpandTick;
            pulseAnimationTimer.Tick += OnPulseContractTick;
            pulseAnimationTimer.Start();
        }

        private void OnPulseContractTick(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                StopPulseAnimation();
                return;
            }

            pulseCurrentStep++;
            double progress = Math.Min(1.0, (double)pulseCurrentStep / pulseTotalSteps);
            double easedProgress = Easing.QuadIn(progress);

            pulseScale = pulseStartScale + (float)((pulseTargetScale - pulseStartScale) * easedProgress);
            Invalidate();

            if (pulseCurrentStep >= pulseTotalSteps)
            {
                pulseAnimationTimer.Stop();
                pulseScale = 1.0f;
                pulseCurrentCount++;

                if (pulseCurrentCount < pulseCount)
                {
                    // 继续下一次脉冲
                    StartPulseExpand(pulseTargetScale + (pulseStartScale - 1.0f)); // 使用相同的缩放幅度
                }
                else
                {
                    // 完成所有脉冲
                    var callback = pulseOnComplete;
                    StopPulseAnimation();
                    Invalidate();
                    callback?.Invoke();
                }
            }
        }

        private void StopPulseAnimation()
        {
            if (pulseAnimationTimer != null)
            {
                pulseAnimationTimer.Stop();
                pulseAnimationTimer.Tick -= OnPulseExpandTick;
                pulseAnimationTimer.Tick -= OnPulseContractTick;
            }
            pulseScale = 1.0f;
            pulseOnComplete = null;
        }

        /// <summary>
        /// 摇摆动画
        /// </summary>
        /// <param name="angle">摇摆角度</param>
        /// <param name="duration">单次摇摆时间（毫秒）</param>
        /// <param name="count">摇摆次数</param>
        public void Shake(float angle = 15f, int duration = 100, int count = 3)
        {
            if (!EnableAnimation || count <= 0)
            {
                return;
            }

            float originalAngle = rotationAngle;
            int currentCount = 0;

            Action doShake = null;
            doShake = () =>
            {
                currentCount++;
                bool isEven = currentCount % 2 == 0;
                float target = isEven ? originalAngle - angle : originalAngle + angle;

                AnimateRotation(target, duration, () =>
                {
                    if (currentCount < count * 2)
                    {
                        doShake();
                    }
                    else
                    {
                        // 回到原始角度
                        AnimateRotation(originalAngle, duration, null);
                    }
                });
            };

            doShake();
        }

        /// <summary>
        /// 图标大小动画
        /// </summary>
        private void AnimateIconSize(Size targetSize, int duration, AnimationState.EasingFunction easing = null, Action onComplete = null)
        {
            StopSizeAnimation();

            if (!EnableAnimation || duration <= 0)
            {
                iconSize = targetSize;
                InvalidateIconCache();
                Invalidate();
                onComplete?.Invoke();
                return;
            }

            sizeStart = iconSize;
            sizeTarget = targetSize;
            sizeEasing = easing ?? Easing.CubicOut;
            sizeOnComplete = onComplete;

            const int frameInterval = 16;
            sizeTotalSteps = Math.Max(1, duration / frameInterval);
            sizeCurrentStep = 0;

            sizeAnimationTimer = new Timer { Interval = frameInterval };
            sizeAnimationTimer.Tick += OnSizeAnimationTick;
            sizeAnimationTimer.Start();
        }

        private void OnSizeAnimationTick(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                StopSizeAnimation();
                return;
            }

            sizeCurrentStep++;
            double progress = Math.Min(1.0, (double)sizeCurrentStep / sizeTotalSteps);
            double easedProgress = sizeEasing(progress);

            int width = sizeStart.Width + (int)((sizeTarget.Width - sizeStart.Width) * easedProgress);
            int height = sizeStart.Height + (int)((sizeTarget.Height - sizeStart.Height) * easedProgress);
            iconSize = new Size(width, height);

            InvalidateIconCache();
            Invalidate();

            if (sizeCurrentStep >= sizeTotalSteps)
            {
                iconSize = sizeTarget;
                var callback = sizeOnComplete;
                StopSizeAnimation();
                InvalidateIconCache();
                Invalidate();
                callback?.Invoke();
            }
        }

        private void StopSizeAnimation()
        {
            if (sizeAnimationTimer != null)
            {
                sizeAnimationTimer.Stop();
                sizeAnimationTimer.Dispose();
                sizeAnimationTimer = null;
            }
            sizeOnComplete = null;
        }


        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hoverAnimationTimer?.Stop();
                hoverAnimationTimer?.Dispose();

                StopRotationAnimation();
                StopPulseAnimation();

                pulseAnimationTimer?.Dispose();
                toolTip?.Dispose();

                InvalidateIconCache();
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    #region 枚举和辅助类

    /// <summary>
    /// 图标背景形状
    /// </summary>
    public enum IconBackgroundShape
    {
        None,               // 无背景
        Circle,             // 圆形
        Square,             // 方形
        RoundedRectangle    // 圆角矩形
    }

    /// <summary>
    /// 图标翻转模式
    /// </summary>
    [Flags]
    public enum IconFlipMode
    {
        None = 0,                       // 不翻转
        Horizontal = 1,                 // 水平翻转
        Vertical = 2,                   // 垂直翻转
        Both = Horizontal | Vertical    // 水平和垂直翻转
    }

    #endregion
}
