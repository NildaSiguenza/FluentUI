using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Themes;
using FluentControls.Controls;

namespace FluentControls
{
    /// <summary>
    /// Fluent控件基类
    /// </summary>
    public abstract class FluentControlBase : Control
    {
        private IFluentTheme theme;
        private ControlState state = ControlState.Normal;
        private bool useTheme = false;

        // 主题继承
        private bool isInheritingTheme = false;
        private bool enableThemeInheritance = true;

        // 动画相关
        private bool isAnimating = false;
        private Timer animationTimer;
        private float animationProgress = 0;

        // 波纹效果
        private Point rippleCenter;
        private float rippleRadius = 0;
        private Timer rippleTimer;
        private float rippleOpacity = 0.3f;

        // 样式缓存
        private bool stylesInitialized = false;


        protected FluentControlBase()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;

            InitializeAnimationTimer();
            InitializeDefaultStyles();
        }

        #region 属性


        /// <summary>
        /// 控件的主题
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]  // 不在设计器中显示
        public IFluentTheme Theme
        {
            get
            {
                // 优先使用显式设置的主题
                if (theme != null)
                {
                    return theme;
                }

                // 其次使用全局主题(如果已设置)
                if (ThemeManager.IsThemeSet)
                {
                    return ThemeManager.CurrentTheme;
                }

                // 不返回默认主题
                return ThemeManager.DefaultTheme;
            }
            set
            {
                UseTheme = (value != null);

                if (theme != value)
                {
                    theme = value;
                    OnThemeChanged();
                }
            }
        }

        /// <summary>
        /// 是否使用主题
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(false)]
        [Description("是否使用主题样式")]
        public bool UseTheme
        {
            get => useTheme;
            set
            {
                if (useTheme != value)
                {
                    useTheme = value;
                    if (value)
                    {
                        OnThemeChanged();
                    }
                }
            }
        }

        /// <summary>
        /// 主题名称(用于设计器)
        /// </summary>
        [Category("Fluent")]
        [DefaultValue("")]
        [Description("要使用的主题名称")]
        [TypeConverter(typeof(ThemeNameConverter))]
        public string ThemeName
        {
            get => theme?.Name ?? "";
            set
            {
                if (isInheritingTheme)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var mTheme= ThemeManager.GetTheme(value);
                        if (mTheme != null)
                        {
                            theme = mTheme;
                        }
                    }
                    else
                    {
                        theme = null;
                    }
                    OnThemeChanged();
                    return;
                }

                var oldValue = ThemeName;

                if (!string.IsNullOrEmpty(value))
                {
                    var theme = ThemeManager.GetTheme(value);
                    if (theme != null)
                    {
                        Theme = theme;
                        UseTheme = true;
                    }
                }
                else
                {
                    Theme = null;
                    UseTheme = false;
                }

                // 如果当前控件是容器, 则将主题传递至子控件
                if (this is IThemeContainer container && oldValue != value)
                {
                    container.ApplyThemeToChildren(true);
                }
            }
        }

        /// <summary>
        /// 是否启用主题继承
        /// (从父容器继承主题)
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("是否从父容器继承主题设置")]
        public bool EnableThemeInheritance
        {
            get => enableThemeInheritance;
            set
            {
                if (enableThemeInheritance != value)
                {
                    enableThemeInheritance = value;
                    if (value && DesignMode)
                    {
                        InheritThemeFromParent();
                    }
                }
            }
        }

        /// <summary>
        /// 是否正在继承主题
        /// (使用protected以避免循环)
        /// </summary>
        [Browsable(false)]
        protected bool IsInheritingTheme
        {
            get => isInheritingTheme;
            set => isInheritingTheme = value;
        }

        [Category("Fluent")]
        [DefaultValue(0)]
        [Description("阴影级别")]
        public int ShadowLevel { get; set; } = 0;

        [Browsable(false)]
        public ControlState State
        {
            get => state;
            protected set
            {
                if (state != value)
                {
                    var oldState = state;
                    state = value;
                    OnStateChanged(oldState, value);
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("启用动画效果")]
        public bool EnableAnimation { get; set; } = true;

        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("启用波纹效果")]
        public bool EnableRippleEffect { get; set; } = true;

        [Category("Fluent")]
        [DefaultValue(300)]
        [Description("动画持续时间(毫秒)")]
        public int AnimationDuration { get; set; } = 300;

        #endregion

        #region 主题相关方法

        /// <summary>
        /// 初始化默认样式
        /// </summary>
        protected virtual void InitializeDefaultStyles()
        {
            // 子类可以重写以设置默认样式
            if (!stylesInitialized)
            {
                // 设置默认值(不依赖主题)
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;
                Font = SystemFonts.DefaultFont;
                stylesInitialized = true;
            }
        }

        /// <summary>
        /// 主题变更时调用
        /// </summary>
        protected virtual void OnThemeChanged()
        {
            if (UseTheme && Theme != null)
            {
                ApplyThemeStyles();
            }
            Invalidate();
        }

        /// <summary>
        /// 应用主题样式
        /// </summary>
        protected virtual void ApplyThemeStyles()
        {
            // 子类重写以应用具体的主题样式
        }

        /// <summary>
        /// 获取主题颜色(带默认值)
        /// </summary>
        protected Color GetThemeColor(Func<IColorPalette, Color> colorSelector, Color defaultColor)
        {
            if (UseTheme && Theme?.Colors != null)
            {
                try
                {
                    return colorSelector(Theme.Colors);
                }
                catch
                {
                    // 如果获取失败, 返回默认值
                }
            }
            return defaultColor;
        }

        /// <summary>
        /// 获取主题字体(带默认值)
        /// </summary>
        protected Font GetThemeFont(Func<ITypography, Font> fontSelector, Font defaultFont)
        {
            if (UseTheme && Theme?.Typography != null)
            {
                try
                {
                    return fontSelector(Theme.Typography);
                }
                catch
                {
                    // 如果获取失败, 返回默认值
                }
            }
            return defaultFont ?? Font;
        }


        /// <summary>
        /// 父控件改变时, 尝试继承主题
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // 当控件被添加到新的父容器时, 尝试继承主题
            if (EnableThemeInheritance && DesignMode)
            {
                InheritThemeFromParent();
            }
        }

        /// <summary>
        /// 从父容器继承主题
        /// </summary>
        protected virtual void InheritThemeFromParent()
        {
            if (!EnableThemeInheritance)
            {
                return;
            }

            var parent = GetThemeParent();
            if (parent != null && parent.UseTheme && !string.IsNullOrEmpty(parent.ThemeName))
            {
                // 只有当父容器明确启用了主题且有有效的主题名称时才继承
                InheritThemeFrom(parent);
            }
        }

        /// <summary>
        /// 获取主题父容器
        /// </summary>
        protected virtual FluentControlBase GetThemeParent()
        {
            Control current = this.Parent;

            while (current != null)
            {
                if (current is FluentControlBase fluentParent && fluentParent.UseTheme && !string.IsNullOrEmpty(fluentParent.ThemeName))
                {
                    return fluentParent;
                }

                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// 从指定控件继承主题
        /// </summary>
        internal virtual void InheritThemeFrom(FluentControlBase source)
        {
            if (source == null || !source.UseTheme || string.IsNullOrEmpty(source.ThemeName))
            {
                return;
            }

            try
            {
                isInheritingTheme = true;

                // 继承主题设置
                UseTheme = true; // 现在设置 UseTheme

                // 设置主题名称
                if (!string.IsNullOrEmpty(source.ThemeName))
                {
                    var mTheme = ThemeManager.GetTheme(source.ThemeName);
                    if (mTheme != null)
                    {
                        theme = mTheme;
                        OnThemeChanged();
                    }
                }
            }
            finally
            {
                isInheritingTheme = false;
            }
        }

        #endregion

        #region 状态管理

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);

            // 触发自定义的布局更新
            OnBoundsChanged();
        }

        protected virtual void OnBoundsChanged()
        {
            // 子类逻辑
        }

        protected virtual void OnStateChanged(ControlState oldState, ControlState newState)
        {
            if (EnableAnimation)
            {
                StartAnimation();
            }
            else
            {
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled && State == ControlState.Normal)
            {
                State = ControlState.Hover;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (State == ControlState.Hover)
            {
                State = ControlState.Normal;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Enabled && e.Button == MouseButtons.Left)
            {
                State = ControlState.Pressed;
                if (EnableRippleEffect)
                {
                    StartRipple(e.Location);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (State == ControlState.Pressed)
            {
                State = ClientRectangle.Contains(e.Location)
                    ? ControlState.Hover
                    : ControlState.Normal;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            State = Enabled ? ControlState.Normal : ControlState.Disabled;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (Enabled && State == ControlState.Normal)
            {
                State = ControlState.Focused;
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (State == ControlState.Focused)
            {
                State = ControlState.Normal;
            }
        }

        #endregion

        #region 动画

        private void InitializeAnimationTimer()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 16; // ~60 FPS
            animationTimer.Tick += OnAnimationTick;
        }

        protected virtual void StartAnimation()
        {
            if (!EnableAnimation || isAnimating)
            {
                return;
            }

            isAnimating = true;
            animationProgress = 0;
            animationTimer.Start();
        }

        protected void StopAnimation()
        {
            if (isAnimating)
            {
                animationTimer.Stop();
                isAnimating = false;
                animationProgress = 0;
            }
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            float step = 16f / AnimationDuration; // 根据持续时间计算步长
            animationProgress += step;

            if (animationProgress >= 1.0f)
            {
                animationProgress = 1.0f;
                StopAnimation();
            }

            OnAnimationProgress(animationProgress);
            Invalidate();
        }

        protected virtual void OnAnimationProgress(float progress)
        {
            // 子类重写以实现具体动画效果
        }

        protected float GetAnimationProgress() => animationProgress;
        protected bool IsAnimating() => isAnimating;

        #endregion

        #region 波纹效果

        protected virtual void StartRipple(Point center)
        {
            if (!EnableRippleEffect)
            {
                return;
            }

            rippleCenter = center;
            rippleRadius = 0;
            rippleOpacity = 0.3f;

            if (rippleTimer == null)
            {
                rippleTimer = new Timer();
                rippleTimer.Interval = 16;
                rippleTimer.Tick += OnRippleTick;
            }

            rippleTimer.Start();
        }

        private void OnRippleTick(object sender, EventArgs e)
        {
            rippleRadius += 10;
            rippleOpacity -= 0.02f;

            if (rippleOpacity <= 0 || rippleRadius > Math.Max(Width, Height))
            {
                rippleTimer.Stop();
                rippleRadius = 0;
                rippleOpacity = 0.3f;
            }

            Invalidate();
        }

        protected virtual void DrawRipple(Graphics g)
        {
            if (rippleRadius > 0 && rippleOpacity > 0)
            {
                var rippleColor = GetThemeColor(
                    c => c.Primary,
                    SystemColors.Highlight);

                using (var brush = new SolidBrush(
                    Color.FromArgb((int)(255 * rippleOpacity), rippleColor)))
                {
                    g.FillEllipse(brush,
                        rippleCenter.X - rippleRadius,
                        rippleCenter.Y - rippleRadius,
                        rippleRadius * 2,
                        rippleRadius * 2);
                }
            }
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 绘制阴影
            if (ShadowLevel > 0)
            {
                DrawShadow(g);
            }

            // 绘制背景
            DrawBackground(g);

            // 绘制内容
            DrawContent(g);

            // 绘制波纹效果
            if (EnableRippleEffect)
            {
                DrawRipple(g);
            }

            // 绘制边框
            DrawBorder(g);
        }

        protected virtual void DrawShadow(Graphics g)
        {
            if (UseTheme && Theme?.Elevation != null)
            {
                var shadow = Theme.Elevation.GetShadow(ShadowLevel);
                if (shadow != null)
                {
                    DrawShadowEffect(g, shadow);
                    return;
                }
            }

            // 默认阴影
            DrawDefaultShadow(g, ShadowLevel);
        }

        private void DrawShadowEffect(Graphics g, Shadow shadow)
        {
            var shadowColor = Color.FromArgb((int)(255 * shadow.Opacity), shadow.Color);
            using (var shadowBrush = new SolidBrush(shadowColor))
            {
                var shadowRect = new Rectangle(
                    shadow.OffsetX,
                    shadow.OffsetY,
                    Width - Math.Abs(shadow.OffsetX),
                    Height - Math.Abs(shadow.OffsetY));

                g.FillRectangle(shadowBrush, shadowRect);
            }
        }

        private void DrawDefaultShadow(Graphics g, int level)
        {
            if (level <= 0)
            {
                return;
            }

            int offset = level;
            int opacity = Math.Min(80, level * 10);

            using (var shadowBrush = new SolidBrush(Color.FromArgb(opacity, Color.Black)))
            {
                var shadowRect = new Rectangle(offset, offset, Width - offset, Height - offset);
                g.FillRectangle(shadowBrush, shadowRect);
            }
        }

        protected abstract void DrawBackground(Graphics g);
        protected abstract void DrawContent(Graphics g);
        protected abstract void DrawBorder(Graphics g);

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取圆角矩形路径
        /// </summary>
        protected GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            // 验证矩形有效性
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return path;
            }

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // 确保半径不超过矩形的一半
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);

            // 如果半径太小, 直接绘制矩形
            if (radius < 1)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            try
            {
                // 左上
                path.AddArc(arc, 180, 90);
                // 右上
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                // 右下
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                // 左下
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
            }
            catch
            {
                // 如果出错, 返回普通矩形
                path.Reset();
                path.AddRectangle(rect);
            }

            return path;
        }

        /// <summary>
        /// 混合两种颜色
        /// </summary>
        protected Color BlendColors(Color color1, Color color2, float ratio)
        {
            ratio = Math.Max(0, Math.Min(1, ratio));
            int r = (int)(color1.R * (1 - ratio) + color2.R * ratio);
            int g = (int)(color1.G * (1 - ratio) + color2.G * ratio);
            int b = (int)(color1.B * (1 - ratio) + color2.B * ratio);
            int a = (int)(color1.A * (1 - ratio) + color2.A * ratio);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// 调整颜色亮度
        /// </summary>
        protected Color AdjustBrightness(Color color, float factor)
        {
            factor = Math.Max(-1, Math.Min(1, factor));

            int r = color.R;
            int g = color.G;
            int b = color.B;

            if (factor > 0)
            {
                // 变亮
                r = r + (int)((255 - r) * factor);
                g = g + (int)((255 - g) * factor);
                b = b + (int)((255 - b) * factor);
            }
            else
            {
                // 变暗
                r = r + (int)(r * factor);
                g = g + (int)(g * factor);
                b = b + (int)(b * factor);
            }

            return Color.FromArgb(color.A, r, g, b);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
                rippleTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }


    /// <summary>
    /// 控件状态枚举
    /// </summary>
    public enum ControlState
    {
        Normal,
        Hover,
        Pressed,
        Focused,
        Disabled
    }

}
