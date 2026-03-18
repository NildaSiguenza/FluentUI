using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.IconFonts;

namespace FluentControls.Controls
{
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    public class FluentNumericInput : FluentControlBase
    {
        #region 成员变量

        private TextBox innerTextBox;

        // 数值相关
        private decimal value = 0;
        private decimal minimum = decimal.MinValue;
        private decimal maximum = decimal.MaxValue;
        private decimal increment = 1;
        private int decimalPlaces = 0;
        private bool thousandsSeparator = false;
        private bool hexadecimal = false;

        // 前缀后缀
        private string prefix = "";
        private string suffix = "";
        private bool showPrefix = true;
        private bool showSuffix = true;
        private Font prefixFont;
        private Font suffixFont;
        private Color prefixColor = Color.Empty;
        private Color suffixColor = Color.Empty;
        private int prefixSpacing = 4;
        private int suffixSpacing = 4;

        // 图标
        private Image icon;
        private IconPosition iconPosition = IconPosition.Left;
        private Size iconSize = new Size(16, 16);
        private int iconSpacing = 4;

        // 按钮相关
        private int buttonWidth = 24;
        private bool showButtons = true;
        private SpinButtonStyle buttonStyle = SpinButtonStyle.Stacked;
        private int buttonSpacing = 4; // 输入框和按钮之间的间距
        private Rectangle upButtonRect;
        private Rectangle downButtonRect;
        private SpinButtonState upButtonState = SpinButtonState.Normal;
        private SpinButtonState downButtonState = SpinButtonState.Normal;

        // 边框
        private int borderSize = 1;
        private Color borderColor = Color.Gray;
        private Color borderFocusedColor = Color.Empty;
        private bool showBorder = true;
        private int cornerRadius = 4;
        private Padding innerPadding = new Padding(6, 2, 6, 2);

        // 状态
        private bool isFocused = false;
        private bool isReadOnly = false;
        private bool allowKeyboardInput = true;

        // 重复触发
        private Timer repeatTimer;
        private bool isRepeating = false;
        private SpinDirection repeatDirection = SpinDirection.None;
        private int repeatDelay = 400;
        private int repeatInterval = 75;

        // 鼠标滚轮
        private bool interceptArrowKeys = true;
        private bool allowMouseWheel = true;
        private decimal mouseWheelIncrement = 1;

        // 基准值
        private int baseBorderSize;
        private int baseCornerRadius;
        private int baseButtonWidth;
        private int baseButtonSpacing;
        private int basePrefixSpacing;
        private int baseSuffixSpacing;
        private int baseIconSpacing;
        private Size baseIconSize;
        private int baseMinHeight;
        private Padding baseInnerPadding;
        private Font baseInnerFont;
        private Font basePrefixFont;
        private Font baseSuffixFont;

        // 缓存
        private bool layoutCacheValid = false;
        private Rectangle cachedInputAreaRect;
        private Rectangle cachedButtonAreaRect;
        private Rectangle cachedBorderRect;
        private Rectangle cachedIconRect;

        // 颜色缓存
        private Color cachedBackColor;
        private Color cachedBorderColor;
        private Color cachedTextColor;
        private Color cachedButtonBackColor;
        private Color cachedButtonHoverColor;
        private Color cachedButtonPressedColor;
        private Color cachedButtonIconColor;
        private Color cachedButtonSeparatorColor;

        // 事件
        public event EventHandler ValueChanged; // 值变更事件
        public event EventHandler<NumericValueOutOfRangeEventArgs> ValueOutOfRange; // 值超出范围事件

        #endregion

        #region 构造函数

        public FluentNumericInput()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.Selectable, true);

            Size = new Size(150, 32);
            MinimumSize = new Size(80, 24);
            Padding = new Padding(0);
            Cursor = Cursors.IBeam;

            InitializeInnerTextBox();
            InitializeRepeatTimer();
            UpdateCachedColors();
        }

        private void InitializeInnerTextBox()
        {
            innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Right,
                Text = FormatValue(value)
            };

            innerTextBox.TextChanged += OnInnerTextChanged;
            innerTextBox.KeyPress += OnInnerKeyPress;
            innerTextBox.KeyDown += OnInnerKeyDown;
            innerTextBox.GotFocus += OnInnerGotFocus;
            innerTextBox.LostFocus += OnInnerLostFocus;
            innerTextBox.MouseWheel += OnInnerMouseWheel;

            Controls.Add(innerTextBox);
        }

        private void InitializeRepeatTimer()
        {
            repeatTimer = new Timer();
            repeatTimer.Tick += OnRepeatTimerTick;
        }

        #endregion

        #region 属性

        [Category("数值")]
        [Description("当前数值")]
        [DefaultValue(typeof(decimal), "0")]
        public decimal Value
        {
            get => value;
            set
            {
                decimal newValue = ConstrainValue(value);
                if (this.value != newValue)
                {
                    this.value = newValue;
                    UpdateTextFromValue();
                    OnValueChanged();
                }
            }
        }

        [Category("数值")]
        [Description("最小值")]
        [DefaultValue(typeof(decimal), "-79228162514264337593543950335")]
        public decimal Minimum
        {
            get => minimum;
            set
            {
                if (minimum != value)
                {
                    minimum = value;
                    if (this.value < minimum)
                    {
                        Value = minimum;
                    }
                }
            }
        }

        [Category("数值")]
        [Description("最大值")]
        [DefaultValue(typeof(decimal), "79228162514264337593543950335")]
        public decimal Maximum
        {
            get => maximum;
            set
            {
                if (maximum != value)
                {
                    maximum = value;
                    if (this.value > maximum)
                    {
                        Value = maximum;
                    }
                }
            }
        }

        [Category("数值")]
        [Description("每次增减的步进值")]
        [DefaultValue(typeof(decimal), "1")]
        public decimal Increment
        {
            get => increment;
            set => increment = value;
        }

        [Category("数值")]
        [Description("显示的小数位数")]
        [DefaultValue(0)]
        public int DecimalPlaces
        {
            get => decimalPlaces;
            set
            {
                if (decimalPlaces != value && value >= 0)
                {
                    decimalPlaces = value;
                    UpdateTextFromValue();
                }
            }
        }

        [Category("数值")]
        [Description("是否显示千位分隔符")]
        [DefaultValue(false)]
        public bool ThousandsSeparator
        {
            get => thousandsSeparator;
            set
            {
                if (thousandsSeparator != value)
                {
                    thousandsSeparator = value;
                    UpdateTextFromValue();
                }
            }
        }

        [Category("数值")]
        [Description("是否以十六进制显示")]
        [DefaultValue(false)]
        public bool Hexadecimal
        {
            get => hexadecimal;
            set
            {
                if (hexadecimal != value)
                {
                    hexadecimal = value;
                    UpdateTextFromValue();
                }
            }
        }

        [Category("前后缀")]
        [Description("前缀文本")]
        [DefaultValue("")]
        public string Prefix
        {
            get => prefix;
            set
            {
                if (prefix != value)
                {
                    prefix = value ?? "";
                    InvalidateLayout();
                }
            }
        }

        [Category("前后缀")]
        [Description("后缀文本")]
        [DefaultValue("")]
        public string Suffix
        {
            get => suffix;
            set
            {
                if (suffix != value)
                {
                    suffix = value ?? "";
                    InvalidateLayout();
                }
            }
        }

        [Category("前后缀")]
        [Description("是否显示前缀")]
        [DefaultValue(true)]
        public bool ShowPrefix
        {
            get => showPrefix;
            set
            {
                if (showPrefix != value)
                {
                    showPrefix = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("前后缀")]
        [Description("是否显示后缀")]
        [DefaultValue(true)]
        public bool ShowSuffix
        {
            get => showSuffix;
            set
            {
                if (showSuffix != value)
                {
                    showSuffix = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("前后缀")]
        [Description("前缀字体")]
        [DefaultValue(null)]
        public Font PrefixFont
        {
            get => prefixFont ?? Font;
            set
            {
                prefixFont = value;
                InvalidateLayout();
            }
        }

        [Category("前后缀")]
        [Description("后缀字体")]
        [DefaultValue(null)]
        public Font SuffixFont
        {
            get => suffixFont ?? Font;
            set
            {
                suffixFont = value;
                InvalidateLayout();
            }
        }

        [Category("前后缀")]
        [Description("前缀颜色")]
        public Color PrefixColor
        {
            get => prefixColor.IsEmpty ? cachedTextColor : prefixColor;
            set
            {
                prefixColor = value;
                Invalidate();
            }
        }

        [Category("前后缀")]
        [Description("后缀颜色")]
        public Color SuffixColor
        {
            get => suffixColor.IsEmpty ? cachedTextColor : suffixColor;
            set
            {
                suffixColor = value;
                Invalidate();
            }
        }

        [Category("前后缀")]
        [Description("前缀与边框的间距")]
        [DefaultValue(4)]
        public int PrefixSpacing
        {
            get => prefixSpacing;
            set
            {
                if (prefixSpacing != value && value >= 0)
                {
                    prefixSpacing = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("前后缀")]
        [Description("后缀与边框的间距")]
        [DefaultValue(4)]
        public int SuffixSpacing
        {
            get => suffixSpacing;
            set
            {
                if (suffixSpacing != value && value >= 0)
                {
                    suffixSpacing = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("图标")]
        [Description("输入框内显示的图标")]
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
                    InvalidateLayout();
                }
            }
        }

        [Category("图标")]
        [Description("图标在输入框内的位置")]
        [DefaultValue(IconPosition.Left)]
        public IconPosition IconPosition
        {
            get => iconPosition;
            set
            {
                if (iconPosition != value)
                {
                    iconPosition = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("图标")]
        [Description("图标显示大小")]
        [DefaultValue(typeof(Size), "16, 16")]
        public Size IconSize
        {
            get => iconSize;
            set
            {
                if (iconSize != value)
                {
                    iconSize = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("图标")]
        [Description("图标与输入框的间距")]
        [DefaultValue(4)]
        public int IconSpacing
        {
            get => iconSpacing;
            set
            {
                if (iconSpacing != value && value >= 0)
                {
                    iconSpacing = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("按钮")]
        [Description("是否显示上下调节按钮")]
        [DefaultValue(true)]
        public bool ShowButtons
        {
            get => showButtons;
            set
            {
                if (showButtons != value)
                {
                    showButtons = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("按钮")]
        [Description("调节按钮的宽度")]
        [DefaultValue(24)]
        public int ButtonWidth
        {
            get => buttonWidth;
            set
            {
                if (buttonWidth != value && value > 0)
                {
                    buttonWidth = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("按钮")]
        [Description("调节按钮与输入框的间距")]
        [DefaultValue(4)]
        public int ButtonSpacing
        {
            get => buttonSpacing;
            set
            {
                if (buttonSpacing != value && value >= 0)
                {
                    buttonSpacing = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("按钮")]
        [Description("调节按钮的样式")]
        [DefaultValue(SpinButtonStyle.Stacked)]
        public SpinButtonStyle ButtonStyle
        {
            get => buttonStyle;
            set
            {
                if (buttonStyle != value)
                {
                    buttonStyle = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("外观")]
        [Description("边框宽度")]
        [DefaultValue(1)]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                if (borderSize != value && value >= 0)
                {
                    borderSize = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("外观")]
        [Description("边框颜色")]
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

        [Category("外观")]
        [Description("获得焦点时的边框颜色")]
        public Color BorderFocusedColor
        {
            get => borderFocusedColor;
            set
            {
                borderFocusedColor = value;
                Invalidate();
            }
        }

        [Category("外观")]
        [Description("是否显示边框")]
        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                if (showBorder != value)
                {
                    showBorder = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("外观")]
        [Description("圆角半径")]
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

        [Category("外观")]
        [Description("输入框相对于边框的内边距")]
        public Padding InnerPadding
        {
            get => innerPadding;
            set
            {
                if (innerPadding != value)
                {
                    innerPadding = value;
                    InvalidateLayout();
                }
            }
        }

        [Category("外观")]
        [Description("控件最小高度")]
        [DefaultValue(24)]
        public int MinHeight { get; set; } = 24;

        [Category("行为")]
        [Description("是否只读")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => isReadOnly;
            set
            {
                if (isReadOnly != value)
                {
                    isReadOnly = value;
                    if (innerTextBox != null)
                    {
                        innerTextBox.ReadOnly = value;
                    }
                }
            }
        }

        [Category("行为")]
        [Description("是否允许键盘输入数值")]
        [DefaultValue(true)]
        public bool AllowKeyboardInput
        {
            get => allowKeyboardInput;
            set => allowKeyboardInput = value;
        }

        [Category("行为")]
        [Description("是否允许鼠标滚轮调节数值")]
        [DefaultValue(true)]
        public bool AllowMouseWheel
        {
            get => allowMouseWheel;
            set => allowMouseWheel = value;
        }

        [Category("行为")]
        [Description("鼠标滚轮调节的增量")]
        [DefaultValue(typeof(decimal), "1")]
        public decimal MouseWheelIncrement
        {
            get => mouseWheelIncrement;
            set => mouseWheelIncrement = value;
        }

        [Category("行为")]
        [Description("是否拦截上下方向键调节数值")]
        [DefaultValue(true)]
        public bool InterceptArrowKeys
        {
            get => interceptArrowKeys;
            set => interceptArrowKeys = value;
        }

        [Category("行为")]
        [Description("按住按钮时开始重复触发的延迟时间(毫秒)")]
        [DefaultValue(400)]
        public int RepeatDelay
        {
            get => repeatDelay;
            set => repeatDelay = Math.Max(100, value);
        }

        [Category("行为")]
        [Description("重复触发的间隔时间(毫秒)")]
        [DefaultValue(75)]
        public int RepeatInterval
        {
            get => repeatInterval;
            set => repeatInterval = Math.Max(25, value);
        }

        /// <summary>
        /// 整数值
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IntValue
        {
            get => (int)Math.Round(value);
            set => Value = value;
        }

        /// <summary>
        /// 双精度值
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double DoubleValue
        {
            get => (double)value;
            set => Value = (decimal)value;
        }

        /// <summary>
        /// 文本值
        /// </summary>
        [Browsable(false)]
        public override string Text
        {
            get => innerTextBox?.Text ?? FormatValue(value);
            set
            {
                if (decimal.TryParse(value, out decimal result))
                {
                    Value = result;
                }
            }
        }

        #endregion

        #region 布局

        private void InvalidateLayout()
        {
            layoutCacheValid = false;
            UpdateLayout();
            Invalidate();
        }

        private void UpdateLayout()
        {
            if (innerTextBox == null || Width <= 0 || Height <= 0)
            {
                return;
            }

            layoutCacheValid = false;

            // 1. 计算前缀后缀宽度
            int prefixWidth = CalculatePrefixWidth();
            int suffixWidth = CalculateSuffixWidth();

            // 2. 计算边框区域
            int borderX = prefixWidth;
            int borderWidth = Width - prefixWidth - suffixWidth;
            int borderOffset = showBorder ? borderSize : 0;

            cachedBorderRect = new Rectangle(borderX, 0, borderWidth, Height);

            // 3. 计算边框内部的可用区域
            int innerLeft = borderX + borderOffset + innerPadding.Left;
            int innerTop = borderOffset + innerPadding.Top;
            int innerRight = borderX + borderWidth - borderOffset - innerPadding.Right;
            int innerBottom = Height - borderOffset - innerPadding.Bottom;
            int innerHeight = innerBottom - innerTop;

            // 4. 计算按钮区域
            int textBoxRightBoundary = innerRight; // 文本框的右边界

            if (showButtons)
            {
                // 按钮紧贴边框内右侧
                int buttonX = innerRight - buttonWidth;
                int buttonY = innerTop;
                int buttonAreaHeight = innerHeight;

                if (buttonStyle == SpinButtonStyle.Stacked)
                {
                    int halfHeight = buttonAreaHeight / 2;
                    upButtonRect = new Rectangle(buttonX, buttonY, buttonWidth, halfHeight);
                    downButtonRect = new Rectangle(buttonX, buttonY + halfHeight, buttonWidth, buttonAreaHeight - halfHeight);
                }
                else
                {
                    int halfWidth = buttonWidth / 2;
                    downButtonRect = new Rectangle(buttonX, buttonY, halfWidth, buttonAreaHeight);
                    upButtonRect = new Rectangle(buttonX + halfWidth, buttonY, buttonWidth - halfWidth, buttonAreaHeight);
                }

                cachedButtonAreaRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonAreaHeight);

                // 文本框的右边界
                textBoxRightBoundary = buttonX - buttonSpacing;
            }
            else
            {
                upButtonRect = Rectangle.Empty;
                downButtonRect = Rectangle.Empty;
                cachedButtonAreaRect = Rectangle.Empty;
            }

            // 5. 计算图标区域
            int textBoxLeftBoundary = innerLeft;

            if (icon != null)
            {
                int iconY = innerTop + (innerHeight - iconSize.Height) / 2;

                if (iconPosition == IconPosition.Left)
                {
                    cachedIconRect = new Rectangle(innerLeft, iconY, iconSize.Width, iconSize.Height);
                    // 文本框左边界
                    textBoxLeftBoundary = innerLeft + iconSize.Width + iconSpacing;
                }
                else
                {
                    int iconX = textBoxRightBoundary - iconSize.Width;
                    cachedIconRect = new Rectangle(iconX, iconY, iconSize.Width, iconSize.Height);
                    // 文本框右边界
                    textBoxRightBoundary = iconX - iconSpacing;
                }
            }
            else
            {
                cachedIconRect = Rectangle.Empty;
            }

            // 6. 计算文本框区域
            int textBoxWidth = Math.Max(20, textBoxRightBoundary - textBoxLeftBoundary);

            // 单行文本框垂直居中
            int preferredHeight = innerTextBox.PreferredHeight;
            int textBoxHeight = Math.Min(preferredHeight, innerHeight);
            int textBoxY = innerTop + (innerHeight - textBoxHeight) / 2;

            innerTextBox.SetBounds(textBoxLeftBoundary, textBoxY, textBoxWidth, textBoxHeight);
            cachedInputAreaRect = new Rectangle(textBoxLeftBoundary, textBoxY, textBoxWidth, textBoxHeight);
            layoutCacheValid = true;
        }

        private int CalculatePrefixWidth()
        {
            if (!showPrefix || string.IsNullOrEmpty(prefix))
            {
                return 0;
            }

            var size = TextRenderer.MeasureText(prefix, GetSafeFont(PrefixFont));
            return size.Width + prefixSpacing;
        }

        private int CalculateSuffixWidth()
        {
            if (!showSuffix || string.IsNullOrEmpty(suffix))
            {
                return 0;
            }

            var size = TextRenderer.MeasureText(suffix, GetSafeFont(SuffixFont));
            return size.Width + suffixSpacing;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            height = Math.Max(height, MinHeight);
            base.SetBoundsCore(x, y, width, height, specified);

            if (IsHandleCreated)
            {
                UpdateLayout();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InvalidateLayout();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            InvalidateLayout();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 绘制控件整体背景
            using (var brush = new SolidBrush(cachedBackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 绘制边框内部背景
            if (!cachedBorderRect.IsEmpty)
            {
                int borderOffset = showBorder ? borderSize : 0;
                Rectangle bgRect = new Rectangle(
                    cachedBorderRect.X + borderOffset,
                    cachedBorderRect.Y + borderOffset,
                    cachedBorderRect.Width - borderOffset * 2,
                    cachedBorderRect.Height - borderOffset * 2);

                if (cornerRadius > 0)
                {
                    int innerRadius = Math.Max(0, cornerRadius - borderOffset);
                    using (var path = GetRoundedRectangle(bgRect, innerRadius))
                    using (var brush = new SolidBrush(cachedBackColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(cachedBackColor))
                    {
                        g.FillRectangle(brush, bgRect);
                    }
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            DrawPrefix(g);
            DrawSuffix(g);
            DrawIcon(g);
            DrawButtons(g);
        }

        protected override void DrawBorder(Graphics g)
        {
            if (!showBorder || borderSize <= 0 || cachedBorderRect.IsEmpty)
            {
                return;
            }

            Color currentBorderColor = GetCurrentBorderColor();

            Rectangle borderRect = new Rectangle(
                cachedBorderRect.X + borderSize / 2,
                cachedBorderRect.Y + borderSize / 2,
                cachedBorderRect.Width - borderSize,
                cachedBorderRect.Height - borderSize);

            using (var pen = new Pen(currentBorderColor, borderSize))
            {
                if (cornerRadius > 0)
                {
                    using (var path = GetRoundedRectangle(borderRect, cornerRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    g.DrawRectangle(pen, borderRect);
                }
            }
        }

        private void DrawPrefix(Graphics g)
        {
            if (!showPrefix || string.IsNullOrEmpty(prefix))
            {
                return;
            }

            var font = GetSafeFont(PrefixFont);
            var size = TextRenderer.MeasureText(prefix, font);

            int x = 0;
            int y = (Height - size.Height) / 2;

            using (var brush = new SolidBrush(PrefixColor))
            {
                g.DrawString(prefix, font, brush, x, y);
            }
        }

        private void DrawSuffix(Graphics g)
        {
            if (!showSuffix || string.IsNullOrEmpty(suffix))
            {
                return;
            }

            var font = GetSafeFont(SuffixFont);
            var size = TextRenderer.MeasureText(suffix, font);

            int x = Width - size.Width;
            int y = (Height - size.Height) / 2;

            using (var brush = new SolidBrush(SuffixColor))
            {
                g.DrawString(suffix, font, brush, x, y);
            }
        }

        private void DrawIcon(Graphics g)
        {
            if (icon == null || cachedIconRect.IsEmpty)
            {
                return;
            }

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(icon, cachedIconRect);
        }

        private void DrawButtons(Graphics g)
        {
            if (!showButtons || cachedButtonAreaRect.IsEmpty)
            {
                return;
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制按钮之间的分隔线
            if (buttonStyle == SpinButtonStyle.Stacked)
            {
                using (var pen = new Pen(cachedButtonSeparatorColor, 1))
                {
                    int lineY = upButtonRect.Bottom;
                    g.DrawLine(pen, cachedButtonAreaRect.Left + 4, lineY, cachedButtonAreaRect.Right - 4, lineY);
                }
            }

            // 绘制上按钮
            DrawSpinButton(g, upButtonRect, true, upButtonState);
            // 绘制下按钮
            DrawSpinButton(g, downButtonRect, false, downButtonState);
        }

        private void DrawSpinButton(Graphics g, Rectangle rect, bool isUp, SpinButtonState state)
        {
            if (rect.IsEmpty)
            {
                return;
            }

            // 绘制按钮背景
            Color bgColor;
            switch (state)
            {
                case SpinButtonState.Hover:
                    bgColor = cachedButtonHoverColor;
                    break;
                case SpinButtonState.Pressed:
                    bgColor = cachedButtonPressedColor;
                    break;
                default:
                    bgColor = Color.Transparent;
                    break;
            }

            if (bgColor != Color.Transparent)
            {
                // 圆角背景
                int btnRadius = Math.Max(0, cornerRadius - borderSize - 2);
                if (btnRadius > 0)
                {
                    using (var path = GetRoundedRectangle(rect, btnRadius))
                    using (var brush = new SolidBrush(bgColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(bgColor))
                    {
                        g.FillRectangle(brush, rect);
                    }
                }
            }

            // 绘制箭头
            DrawArrow(g, rect, isUp, state);
        }

        private void DrawArrow(Graphics g, Rectangle rect, bool isUp, SpinButtonState state)
        {
            Color arrowColor = state == SpinButtonState.Disabled
                ? Color.FromArgb(128, cachedButtonIconColor)
                : (state == SpinButtonState.Hover || state == SpinButtonState.Pressed)
                    ? GetThemeColor(c => c.Primary, cachedButtonIconColor)
                    : cachedButtonIconColor;

            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float size = Math.Min(rect.Width, rect.Height) * 0.28f;

            using (var pen = new Pen(arrowColor, 1.5f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                PointF[] points;
                if (isUp)
                {
                    points = new PointF[]
                    {
                        new PointF(cx - size, cy + size * 0.4f),
                        new PointF(cx, cy - size * 0.4f),
                        new PointF(cx + size, cy + size * 0.4f)
                    };
                }
                else
                {
                    points = new PointF[]
                    {
                        new PointF(cx - size, cy - size * 0.4f),
                        new PointF(cx, cy + size * 0.4f),
                        new PointF(cx + size, cy - size * 0.4f)
                    };
                }

                g.DrawLines(pen, points);
            }
        }

        #endregion

        #region 颜色

        private void UpdateCachedColors()
        {
            cachedBackColor = GetThemeColor(c => c.Surface, SystemColors.Window);
            cachedBorderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            cachedTextColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
            cachedButtonBackColor = GetThemeColor(c => c.Surface, SystemColors.Control);
            cachedButtonHoverColor = GetThemeColor(c => c.SurfaceHover, Color.FromArgb(240, 240, 240));
            cachedButtonPressedColor = GetThemeColor(c => c.SurfacePressed, Color.FromArgb(220, 220, 220));
            cachedButtonIconColor = GetThemeColor(c => c.TextSecondary, SystemColors.ControlDarkDark);
            cachedButtonSeparatorColor = GetThemeColor(c => c.BorderLight, Color.FromArgb(220, 220, 220));

            if (innerTextBox != null)
            {
                innerTextBox.BackColor = cachedBackColor;
                innerTextBox.ForeColor = cachedTextColor;
            }
        }

        private Color GetCurrentBorderColor()
        {
            if (isFocused)
            {
                if (!borderFocusedColor.IsEmpty)
                {
                    return borderFocusedColor;
                }

                return GetThemeColor(c => c.Primary, Color.DodgerBlue);
            }

            return borderColor.IsEmpty ? cachedBorderColor : borderColor;
        }

        protected override void OnThemeChanged()
        {
            UpdateCachedColors();
            base.OnThemeChanged();
        }

        #endregion

        #region 交互

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!showButtons)
            {
                return;
            }

            var oldUpState = upButtonState;
            var oldDownState = downButtonState;

            // 更新悬停状态
            if (upButtonRect.Contains(e.Location))
            {
                upButtonState = upButtonState == SpinButtonState.Pressed ? SpinButtonState.Pressed : SpinButtonState.Hover;
                downButtonState = SpinButtonState.Normal;
                Cursor = Cursors.Hand;
            }
            else if (downButtonRect.Contains(e.Location))
            {
                downButtonState = downButtonState == SpinButtonState.Pressed ? SpinButtonState.Pressed : SpinButtonState.Hover;
                upButtonState = SpinButtonState.Normal;
                Cursor = Cursors.Hand;
            }
            else
            {
                upButtonState = SpinButtonState.Normal;
                downButtonState = SpinButtonState.Normal;
                Cursor = Cursors.IBeam;
            }

            if (oldUpState != upButtonState || oldDownState != downButtonState)
            {
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (showButtons && upButtonRect.Contains(e.Location))
            {
                upButtonState = SpinButtonState.Pressed;
                PerformIncrement();
                StartRepeat(SpinDirection.Up);
                Invalidate();
            }
            else if (showButtons && downButtonRect.Contains(e.Location))
            {
                downButtonState = SpinButtonState.Pressed;
                PerformDecrement();
                StartRepeat(SpinDirection.Down);
                Invalidate();
            }
            else if (cachedBorderRect.Contains(e.Location))
            {
                // 点击边框内区域时聚焦文本框
                innerTextBox?.Focus();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            StopRepeat();

            if (upButtonState == SpinButtonState.Pressed)
            {
                upButtonState = upButtonRect.Contains(e.Location) ? SpinButtonState.Hover : SpinButtonState.Normal;
            }

            if (downButtonState == SpinButtonState.Pressed)
            {
                downButtonState = downButtonRect.Contains(e.Location) ? SpinButtonState.Hover : SpinButtonState.Normal;
            }

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (upButtonState != SpinButtonState.Pressed)
            {
                upButtonState = SpinButtonState.Normal;
            }

            if (downButtonState != SpinButtonState.Pressed)
            {
                downButtonState = SpinButtonState.Normal;
            }

            Cursor = Cursors.Default;
            Invalidate();
        }

        private void OnInnerTextChanged(object sender, EventArgs e)
        {
            // 不在这里更新值, 等失去焦点时再验证
        }

        private void OnInnerKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!allowKeyboardInput && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // 验证输入字符
            if (!ValidateKeyPress(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void OnInnerKeyDown(object sender, KeyEventArgs e)
        {
            if (interceptArrowKeys)
            {
                if (e.KeyCode == Keys.Up)
                {
                    PerformIncrement();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    PerformDecrement();
                    e.Handled = true;
                }
            }

            if (e.KeyCode == Keys.Enter)
            {
                ParseAndSetValue();
                e.Handled = true;
            }
        }

        private void OnInnerGotFocus(object sender, EventArgs e)
        {
            isFocused = true;
            State = ControlState.Focused;
            Invalidate();
        }

        private void OnInnerLostFocus(object sender, EventArgs e)
        {
            isFocused = false;
            State = ControlState.Normal;
            ParseAndSetValue();
            Invalidate();
        }

        private void OnInnerMouseWheel(object sender, MouseEventArgs e)
        {
            if (!allowMouseWheel)
            {
                return;
            }

            if (e.Delta > 0)
            {
                Value += mouseWheelIncrement;
            }
            else if (e.Delta < 0)
            {
                Value -= mouseWheelIncrement;
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }

        private bool ValidateKeyPress(char keyChar)
        {
            if (char.IsControl(keyChar))
            {
                return true;
            }

            if (hexadecimal)
            {
                return char.IsDigit(keyChar) ||
                       (keyChar >= 'a' && keyChar <= 'f') ||
                       (keyChar >= 'A' && keyChar <= 'F');
            }

            // 允许数字
            if (char.IsDigit(keyChar))
            {
                return true;
            }

            // 允许负号
            if (keyChar == '-' && minimum < 0)
            {
                string text = innerTextBox.Text;
                int selStart = innerTextBox.SelectionStart;
                return selStart == 0 && !text.Contains("-");
            }

            // 允许小数点
            if (keyChar == '.' && decimalPlaces > 0)
            {
                return !innerTextBox.Text.Contains(".");
            }

            return false;
        }

        #endregion

        #region 数值操作

        private void PerformIncrement()
        {
            if (isReadOnly)
            {
                return;
            }

            Value += increment;
        }

        private void PerformDecrement()
        {
            if (isReadOnly)
            {
                return;
            }

            Value -= increment;
        }

        private decimal ConstrainValue(decimal val)
        {
            if (val < minimum)
            {
                OnValueOutOfRange(val, minimum);
                return minimum;
            }

            if (val > maximum)
            {
                OnValueOutOfRange(val, maximum);
                return maximum;
            }

            return val;
        }

        private void ParseAndSetValue()
        {
            if (innerTextBox == null)
            {
                return;
            }

            string text = innerTextBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                Value = minimum > 0 ? minimum : 0;
                return;
            }

            // 移除千位分隔符
            if (thousandsSeparator)
            {
                text = text.Replace(",", "").Replace(" ", "");
            }

            if (hexadecimal)
            {
                if (long.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out long hexVal))
                {
                    Value = hexVal;
                }
                else
                {
                    UpdateTextFromValue();
                }
            }
            else
            {
                if (decimal.TryParse(text, out decimal result))
                {
                    Value = result;
                }
                else
                {
                    UpdateTextFromValue();
                }
            }
        }

        private void UpdateTextFromValue()
        {
            if (innerTextBox == null)
            {
                return;
            }

            innerTextBox.Text = FormatValue(value);
        }

        private string FormatValue(decimal val)
        {
            if (hexadecimal)
            {
                return ((long)val).ToString("X");
            }

            string format = "F" + decimalPlaces;

            if (thousandsSeparator)
            {
                format = "N" + decimalPlaces;
            }

            return val.ToString(format);
        }

        public void UpButton()
        {
            PerformIncrement();
        }

        public void DownButton()
        {
            PerformDecrement();
        }

        public void SelectAll()
        {
            innerTextBox?.SelectAll();
        }

        #endregion

        #region 重复触发

        private void StartRepeat(SpinDirection direction)
        {
            repeatDirection = direction;
            isRepeating = false;
            repeatTimer.Interval = repeatDelay;
            repeatTimer.Start();
        }

        private void StopRepeat()
        {
            repeatTimer.Stop();
            repeatDirection = SpinDirection.None;
            isRepeating = false;
        }

        private void OnRepeatTimerTick(object sender, EventArgs e)
        {
            if (!isRepeating)
            {
                isRepeating = true;
                repeatTimer.Interval = repeatInterval;
            }

            if (repeatDirection == SpinDirection.Up)
            {
                PerformIncrement();
            }
            else if (repeatDirection == SpinDirection.Down)
            {
                PerformDecrement();
            }
        }

        #endregion

        #region 事件触发




        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnValueOutOfRange(decimal attemptedValue, decimal constrainedValue)
        {
            ValueOutOfRange?.Invoke(this, new NumericValueOutOfRangeEventArgs(attemptedValue, constrainedValue, minimum, maximum));
        }

        #endregion

        #region 辅助方法

        private Font GetSafeFont(Font font)
        {
            if (font != null && font.Size > 0)
            {
                return font;
            }

            if (base.Font != null && base.Font.Size > 0)
            {
                return base.Font;
            }

            return SystemFonts.DefaultFont;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            BeginInvoke(new Action(() =>
            {
                UpdateLayout();
                UpdateTextFromValue();
            }));
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repeatTimer?.Stop();
                repeatTimer?.Dispose();

                if (baseInnerFont != null && !baseInnerFont.IsSystemFont)
                {
                    baseInnerFont.Dispose();
                }

                if (basePrefixFont != null && !basePrefixFont.IsSystemFont)
                {
                    basePrefixFont.Dispose();
                }

                if (baseSuffixFont != null && !baseSuffixFont.IsSystemFont)
                {
                    baseSuffixFont.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    #region 枚举和辅助类

    /// <summary>
    /// 调节按钮样式
    /// </summary>
    public enum SpinButtonStyle
    {
        Stacked,        // 上下堆叠
        LeftRight       // 左右并排
    }

    /// <summary>
    /// 调节按钮状态
    /// </summary>
    public enum SpinButtonState
    {
        Normal,
        Hover,
        Pressed,
        Disabled
    }

    /// <summary>
    /// 调节方向
    /// </summary>
    public enum SpinDirection
    {
        None,
        Up,
        Down
    }

    /// <summary>
    /// 数值超出范围事件参数
    /// </summary>
    public class NumericValueOutOfRangeEventArgs : EventArgs
    {
        public NumericValueOutOfRangeEventArgs(decimal attemptedValue, decimal constrainedValue, decimal minimum, decimal maximum)
        {
            AttemptedValue = attemptedValue;
            ConstrainedValue = constrainedValue;
            Minimum = minimum;
            Maximum = maximum;
        }

        public decimal AttemptedValue { get; }
        public decimal ConstrainedValue { get; }
        public decimal Minimum { get; }
        public decimal Maximum { get; }
    }

    #endregion
}
