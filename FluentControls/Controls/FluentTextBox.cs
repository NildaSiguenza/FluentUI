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
using System.Collections.Specialized;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent文本框控件
    /// </summary>
    public class FluentTextBox : FluentControlBase
    {
        #region 字段

        private TextBox innerTextBox;
        private ListBox autoCompleteListBox;
        private Label placeholderLabel;
        private ToolTip toolTip;
        private StringCollection autoCompleteStringCollection = new StringCollection();

        private string text = "";
        private string placeholder = "";
        private string prefix = "";
        private string suffix = "";
        private Image icon;
        private IconPosition iconPosition = IconPosition.Left;
        private TextAlignment textAlignment = TextAlignment.Left;
        private InputFormat inputFormat = InputFormat.Text;
        private string customPattern = "";
        private bool allowNegative = true;
        private int decimalPlaces = 2;
        private Color innerTextColor = Color.Black;
        private Color innerBackColor = Color.White;
        private Font innerFont;

        // 值范围控制
        private decimal? minimum = null;
        private decimal? maximum = null;
        private bool isValidatingRange = false;

        // 边框
        private int borderSize = 1;
        private Color borderColor = Color.Gray;
        private Color borderFocusedColor = Color.Empty;
        private bool showBorder = true;
        private BorderStyle customBorderStyle = BorderStyle.FixedSingle;

        private Font prefixFont;
        private Font suffixFont;
        private Color prefixColor;
        private Color suffixColor;
        private Padding textPadding = new Padding(8, 8, 8, 8);
        private int prefixSpacing = 4;
        private int suffixSpacing = 4;
        private int prefixAreaWidth = 0;
        private bool readOnly = false;

        // 多行模式下滚动条控制
        private ScrollBars scrollBars = ScrollBars.None;
        private bool autoScrollBars = true; // 自动控制滚动条

        private bool multiline = false;
        private bool wordWrap = true;
        private bool showPrefix = true;
        private bool showSuffix = true;
        private bool isFocused = false;

        private List<string> autoCompleteSource;
        private bool enableAutoComplete = false;
        private bool isAutoCompleteVisible = false;
        private bool backcolorForced = false; // 是否强制使用设置背景色

        // 事件
        public event EventHandler<ValueOutOfRangeEventArgs> ValueOutOfRange; // 值超出范围事件
        public event EventHandler ValueChanged; // 值变更事件

        #endregion

        #region 构造方法

        public FluentTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ContainerControl, true);

            Size = new Size(200, 32);
            Font = new Font("Microsoft YaHei", 9.35f);
            Padding = new Padding(8, 6, 8, 6);

            InitializeInnerTextBox();
            InitializeAutoComplete();
        }

        private void InitializeInnerTextBox()
        {
            innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = InnerFont,
                ForeColor = InnerTextColor,
                BackColor = InnerBackColor,
                ScrollBars = ScrollBars.None
            };

            // 设置 Anchor 让 TextBox 随控件大小变化
            innerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            innerTextBox.TextChanged += (s, e) =>
            {
                UpdatePlaceholderVisibility();
                UpdateScrollBars(); // 文本变化时更新滚动条
                OnTextChanged(e);
                OnValueChanged();
                if (enableAutoComplete)
                {
                    UpdateAutoComplete();
                }
            };

            innerTextBox.KeyPress += (s, e) =>
            {
                if (!ValidateKeyPress(e.KeyChar))
                {
                    e.Handled = true;
                }
            };

            innerTextBox.GotFocus += (s, e) =>
            {
                isFocused = true;
                UpdatePlaceholderVisibility();
                State = ControlState.Focused;
                Invalidate();
            };

            innerTextBox.LostFocus += (s, e) =>
            {
                isFocused = false;
                UpdatePlaceholderVisibility();
                State = ControlState.Normal;
                HideAutoComplete();

                // 失去焦点时验证范围
                ValidateAndClampValue();

                Invalidate();
            };

            innerTextBox.KeyDown += (s, e) =>
            {
                if (isAutoCompleteVisible)
                {
                    HandleAutoCompleteKeyDown(e);
                }
            };

            innerTextBox.SizeChanged += (s, e) =>
            {
                UpdateScrollBars();
            };

            Controls.Add(innerTextBox);

            InitializePlaceholderLabel();
            UpdateLayout();
        }

        private void InitializePlaceholderLabel()
        {
            placeholderLabel = new Label
            {
                Text = placeholder,
                BackColor = Color.Transparent,
                AutoSize = false,
                Cursor = Cursors.IBeam,
                Font = InnerFont,
                ForeColor = Color.Gray,
                Visible = false
            };

            placeholderLabel.Click += (s, e) => innerTextBox.Focus();

            Controls.Add(placeholderLabel);
            placeholderLabel.BringToFront();
        }

        private void InitializeAutoComplete()
        {
            autoCompleteListBox = new ListBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                Visible = false,
                Font = Font
            };

            autoCompleteListBox.SelectedIndexChanged += (s, e) =>
            {
                if (autoCompleteListBox.SelectedItem != null)
                {
                    innerTextBox.Text = autoCompleteListBox.SelectedItem.ToString();
                    innerTextBox.SelectionStart = innerTextBox.Text.Length;
                    HideAutoComplete();
                }
            };

            autoCompleteListBox.MouseClick += (s, e) =>
            {
                if (autoCompleteListBox.SelectedItem != null)
                {
                    innerTextBox.Text = autoCompleteListBox.SelectedItem.ToString();
                    innerTextBox.SelectionStart = innerTextBox.Text.Length;
                    HideAutoComplete();
                    innerTextBox.Focus();
                }
            };
        }

        #endregion

        #region 属性

        [Category("Fluent")]
        [Description("文本内容")]
        public override string Text
        {
            get => innerTextBox?.Text ?? text;
            set
            {
                text = value;
                if (innerTextBox != null)
                {
                    innerTextBox.Text = value;
                }
            }
        }

        [Category("Fluent")]
        [Description("文本颜色")]
        public Color InnerTextColor
        {
            get => innerTextColor;
            set
            {
                innerTextColor = value;
                if (innerTextBox != null)
                {
                    innerTextBox.ForeColor = value;
                }
            }
        }

        [Category("Fluent")]
        [Description("文本框背景色")]
        public Color InnerBackColor
        {
            get => innerBackColor;
            set
            {
                innerBackColor = value;
                if (innerTextBox != null)
                {
                    innerTextBox.BackColor = value;
                }
            }
        }

        [Category("Fluent")]
        [Description("文本字体")]
        public Font InnerFont
        {
            get => innerFont ?? Font;
            set
            {
                innerFont = value;
                if (innerTextBox != null)
                {
                    innerTextBox.Font = value ?? Font;
                }

                if (placeholderLabel != null)
                {
                    placeholderLabel.Font = value ?? Font;
                }
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("最小高度")]
        [DefaultValue(24)]
        public int MinHeight { get; set; } = 24;

        [Category("Border")]
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
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Border")]
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

        [Category("Border")]
        [Description("获得焦点时的边框颜色(留空使用主题色)")]
        public Color BorderFocusedColor
        {
            get => borderFocusedColor;
            set
            {
                if (borderFocusedColor != value)
                {
                    borderFocusedColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
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
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [Description("边框样式")]
        [DefaultValue(BorderStyle.FixedSingle)]
        public BorderStyle CustomBorderStyle
        {
            get => customBorderStyle;
            set
            {
                if (customBorderStyle != value)
                {
                    customBorderStyle = value;
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("占位符文本")]
        public string Placeholder
        {
            get => placeholder;
            set
            {
                if (placeholder != value)
                {
                    placeholder = value;
                    if (placeholderLabel != null)
                    {
                        placeholderLabel.Text = value;
                        UpdatePlaceholderVisibility();
                    }
                }
            }
        }

        [Category("Fluent")]
        [Description("前缀文本")]
        public string Prefix
        {
            get => prefix;
            set
            {
                prefix = value;
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("后缀文本")]
        public string Suffix
        {
            get => suffix;
            set
            {
                suffix = value;
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("图标")]
        public Image Icon
        {
            get => icon;
            set
            {
                icon = value;
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("图标位置")]
        public IconPosition IconPosition
        {
            get => iconPosition;
            set
            {
                iconPosition = value;
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("文本对齐")]
        public TextAlignment TextAlignment
        {
            get => textAlignment;
            set
            {
                textAlignment = value;
                UpdateTextBoxAlignment();
            }
        }

        [Category("Fluent")]
        [Description("输入格式")]
        public InputFormat InputFormat
        {
            get => inputFormat;
            set
            {
                inputFormat = value;
                ValidateInput();
            }
        }

        [Category("Fluent")]
        [Description("自定义正则表达式")]
        public string CustomPattern
        {
            get => customPattern;
            set => customPattern = value;
        }

        [Category("Fluent")]
        [Description("允许负数")]
        public bool AllowNegative
        {
            get => allowNegative;
            set => allowNegative = value;
        }

        [Category("Fluent")]
        [Description("小数位数")]
        public int DecimalPlaces
        {
            get => decimalPlaces;
            set => decimalPlaces = Math.Max(0, value);
        }

        [Category("Fluent")]
        [Description("前缀字体")]
        public Font PrefixFont
        {
            get => prefixFont ?? Font;
            set
            {
                prefixFont = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("后缀字体")]
        public Font SuffixFont
        {
            get => suffixFont ?? Font;
            set
            {
                suffixFont = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("前缀颜色")]
        public Color PrefixColor
        {
            get => prefixColor.IsEmpty ? ForeColor : prefixColor;
            set
            {
                prefixColor = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("后缀颜色")]
        public Color SuffixColor
        {
            get => suffixColor.IsEmpty ? ForeColor : suffixColor;
            set
            {
                suffixColor = value;
                Invalidate();
            }
        }

        // 重写 Padding 属性以触发布局更新
        [Category("Fluent")]
        [Description("文本内边距(控制输入区域与控件边缘的距离)")]
        public new Padding Padding
        {
            get => base.Padding;
            set
            {
                if (base.Padding != value)
                {
                    base.Padding = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("前缀间距")]
        public int PrefixSpacing
        {
            get => prefixSpacing;
            set
            {
                prefixSpacing = Math.Max(0, value);
                UpdateLayout();
            }
        }

        /// <summary>
        /// 前缀整体宽度
        /// (此属性大于0则优先使用此属性确定前缀区域整体宽度,包含前缀文本和前缀间距)
        /// </summary>
        [Category("Fluent")]
        [Description("前缀整体宽度")]
        public int PrefixAreaWidth
        {
            get => prefixAreaWidth;
            set
            {
                prefixAreaWidth = value;
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("后缀间距")]
        public int SuffixSpacing
        {
            get => suffixSpacing;
            set
            {
                suffixSpacing = Math.Max(0, value);
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("多行模式")]
        public bool Multiline
        {
            get => multiline;
            set
            {
                if (multiline != value)
                {
                    multiline = value;
                    if (innerTextBox != null)
                    {
                        innerTextBox.Multiline = value;

                        // 根据模式设置滚动条
                        if (value)
                        {
                            UpdateScrollBars();
                        }
                        else
                        {
                            innerTextBox.ScrollBars = ScrollBars.None;
                        }
                    }
                    UpdateLayout();
                }
            }
        }

        [Category("Fluent")]
        [Description("是否显示滚动条")]
        public bool BackcolorForced
        {
            get => backcolorForced;
            set
            {
                backcolorForced = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("自动换行")]
        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                if (wordWrap != value)
                {
                    wordWrap = value;
                    if (innerTextBox != null)
                    {
                        innerTextBox.WordWrap = value;
                    }

                    // 换行设置改变时更新滚动条
                    UpdateScrollBars();
                }
            }
        }

        /// <summary>
        /// 是否只读
        /// </summary>
        [Category("Fluent")]
        [Description("是否只读")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                if (readOnly != value)
                {
                    readOnly = value;
                    if (innerTextBox != null)
                    {
                        innerTextBox.ReadOnly = value;

                        // 设置光标样式
                        if (value)
                        {
                            innerTextBox.Cursor = Cursors.Default;
                            this.Cursor = Cursors.Default;
                        }
                        else
                        {
                            innerTextBox.Cursor = Cursors.IBeam;
                            this.Cursor = Cursors.IBeam;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 滚动条显示模式
        /// </summary>
        [Category("Fluent")]
        [Description("滚动条显示模式(仅在多行模式下有效)")]
        [DefaultValue(ScrollBars.None)]
        public ScrollBars ScrollBars
        {
            get => scrollBars;
            set
            {
                if (scrollBars != value)
                {
                    scrollBars = value;

                    // 如果手动设置了滚动条, 禁用自动控制
                    if (value != ScrollBars.None)
                    {
                        autoScrollBars = false;
                    }

                    if (innerTextBox != null && multiline)
                    {
                        innerTextBox.ScrollBars = value;
                    }
                }
            }
        }

        /// <summary>
        /// 是否自动控制滚动条显示
        /// </summary>
        [Category("Fluent")]
        [Description("是否根据内容自动显示/隐藏滚动条(仅在多行模式下有效)")]
        [DefaultValue(true)]
        public bool AutoScrollBars
        {
            get => autoScrollBars;
            set
            {
                if (autoScrollBars != value)
                {
                    autoScrollBars = value;
                    if (value)
                    {
                        UpdateScrollBars();
                    }
                }
            }
        }


        [Category("Fluent")]
        [Description("显示前缀")]
        public bool ShowPrefix
        {
            get => showPrefix;
            set
            {
                showPrefix = value;
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("显示后缀")]
        public bool ShowSuffix
        {
            get => showSuffix;
            set
            {
                showSuffix = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 获取当前值(根据 InputFormat 返回适当类型)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Value
        {
            get => GetTypedValue();
            set => SetTypedValue(value);
        }

        /// <summary>
        /// 最小值(InputFormat 为 Integer 或 Decimal 时有效)
        /// </summary>
        [Category("Fluent")]
        [Description("允许输入的最小值(仅对数值类型有效)")]
        [DefaultValue(null)]
        public decimal? Minimum
        {
            get => minimum;
            set
            {
                if (minimum != value)
                {
                    minimum = value;

                    // 验证 Minimum <= Maximum
                    if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
                    {
                        throw new ArgumentException("Minimum 不能大于 Maximum");
                    }

                    // 验证当前值是否在范围内
                    ValidateAndClampValue();
                }
            }
        }

        /// <summary>
        /// 最大值(InputFormat 为 Integer 或 Decimal 时有效)
        /// </summary>
        [Category("Fluent")]
        [Description("允许输入的最大值(仅对数值类型有效)")]
        [DefaultValue(null)]
        public decimal? Maximum
        {
            get => maximum;
            set
            {
                if (maximum != value)
                {
                    maximum = value;

                    // 验证 Minimum <= Maximum
                    if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
                    {
                        throw new ArgumentException("Maximum 不能小于 Minimum");
                    }

                    // 验证当前值是否在范围内
                    ValidateAndClampValue();
                }
            }
        }

        /// <summary>
        /// 整数类型的最小值
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? MinimumInt
        {
            get => minimum.HasValue ? (int?)Convert.ToInt32(minimum.Value) : null;
            set => Minimum = value.HasValue ? (decimal?)value.Value : null;
        }

        /// <summary>
        /// 整数类型的最大值
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? MaximumInt
        {
            get => maximum.HasValue ? (int?)Convert.ToInt32(maximum.Value) : null;
            set => Maximum = value.HasValue ? (decimal?)value.Value : null;
        }

        [Category("Fluent")]
        [Description("启用自动完成")]
        public bool EnableAutoComplete
        {
            get => enableAutoComplete;
            set
            {
                enableAutoComplete = value;
                if (!value && autoCompleteListBox != null)
                {
                    autoCompleteListBox.Visible = false;
                }
            }
        }

        [Category("Fluent")]
        [Description("工具提示文本")]
        public string ToolTipText
        {
            get => toolTip?.GetToolTip(this);
            set
            {
                if (toolTip == null)
                {
                    toolTip = new ToolTip();
                }

                toolTip.SetToolTip(this, value);
                toolTip.SetToolTip(innerTextBox, value);
            }
        }


        [Browsable(false)]
        [Description("自动完成数据源")]
        public List<string> AutoCompleteSource
        {
            get => autoCompleteSource;
            set => autoCompleteSource = value;
        }

        [Category("Fluent")]
        [Description("自动完成数据源")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StringCollection AutoCompleteStringCollection
        {
            get => autoCompleteStringCollection;
            set
            {
                autoCompleteStringCollection = value;
                if (value != null)
                {
                    autoCompleteSource = new List<string>();
                    foreach (string item in value)
                    {
                        autoCompleteSource.Add(item);
                    }
                }
            }
        }

        #endregion

        #region 事件

        protected virtual void OnValueOutOfRange(ValueOutOfRangeEventArgs e)
        {
            ValueOutOfRange?.Invoke(this, e);
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 样式

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
        }

        protected override void InitializeDefaultStyles()
        {
            base.InitializeDefaultStyles();
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            if (borderColor == Color.Gray)
            {
                borderColor = SystemColors.ControlDark;
            }
        }

        protected override void ApplyThemeStyles()
        {
            if (Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;

            if (borderColor == Color.Gray || borderColor == SystemColors.ControlDark)
            {
                borderColor = Theme.Colors.Border;
            }

            if (placeholderLabel != null)
            {
                placeholderLabel.ForeColor = Theme.Colors.TextDisabled;
            }
        }

        #endregion

        #region 方法

        public void SelectAll()
        {
            if (innerTextBox != null)
            {
                innerTextBox.SelectAll();
            }
        }

        /// <summary>
        /// 设置数值范围
        /// </summary>
        public void SetRange(decimal? min, decimal? max)
        {
            if (min.HasValue && max.HasValue && min.Value > max.Value)
            {
                throw new ArgumentException("min 不能大于 max");
            }

            minimum = min;
            maximum = max;
            ValidateAndClampValue();
        }

        /// <summary>
        /// 设置整数范围
        /// </summary>
        public void SetRange(int? min, int? max)
        {
            SetRange(min.HasValue ? (decimal?)min.Value : null,
                     max.HasValue ? (decimal?)max.Value : null);
        }

        /// <summary>
        /// 清除范围限制
        /// </summary>
        public void ClearRange()
        {
            minimum = null;
            maximum = null;
        }

        private void UpdateLayout()
        {
            if (innerTextBox == null)
            {
                return;
            }

            int prefixWidth = 0;
            int suffixWidth = 0;
            int iconWidth = 0;

            using (var g = CreateGraphics())
            {
                if (ShowPrefix && !string.IsNullOrEmpty(Prefix))
                {
                    var prefixSize = g.MeasureString(Prefix, PrefixFont);
                    int prefixTextWidth = (int)Math.Ceiling(prefixSize.Width);

                    if (prefixAreaWidth > prefixTextWidth)
                    {
                        prefixWidth = prefixAreaWidth;
                    }
                    else
                    {
                        prefixWidth = prefixTextWidth + PrefixSpacing;
                    }
                }

                if (ShowSuffix && !string.IsNullOrEmpty(Suffix))
                {
                    var suffixSize = g.MeasureString(Suffix, SuffixFont);
                    suffixWidth = (int)Math.Ceiling(suffixSize.Width) + SuffixSpacing;
                }
            }

            if (Icon != null)
            {
                iconWidth = 20 + 8;
            }

            int borderOffset = showBorder ? borderSize : 0;

            int x = Padding.Left + borderOffset;
            int width = Width - Padding.Horizontal - (borderOffset * 2);

            if (Icon != null && IconPosition == IconPosition.Left)
            {
                x += iconWidth;
                width -= iconWidth;
            }
            else if (Icon != null && IconPosition == IconPosition.Right)
            {
                width -= iconWidth;
            }

            x += prefixWidth;
            width -= prefixWidth + suffixWidth;

            int y = Padding.Top + borderOffset;
            int height = Height - Padding.Vertical - (borderOffset * 2);

            width = Math.Max(20, width);
            height = Math.Max(10, height);

            innerTextBox.Location = new Point(x, y);
            innerTextBox.Size = new Size(width, height);

            if (placeholderLabel != null)
            {
                placeholderLabel.Location = innerTextBox.Location;
                placeholderLabel.Size = innerTextBox.Size;
                placeholderLabel.Font = InnerFont;

                if (UseTheme && Theme != null)
                {
                    placeholderLabel.ForeColor = Theme.Colors.TextDisabled;
                }
                else
                {
                    placeholderLabel.ForeColor = Color.Gray;
                }

                placeholderLabel.BringToFront();
                UpdatePlaceholderVisibility();
            }

            if (Multiline)
            {
                innerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            }
            else
            {
                innerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            }

            UpdateScrollBars();
            Invalidate();
        }

        private void UpdateTextBoxAlignment()
        {
            if (innerTextBox == null)
            {
                return;
            }

            switch (textAlignment)
            {
                case TextAlignment.Left:
                    innerTextBox.TextAlign = HorizontalAlignment.Left;
                    break;
                case TextAlignment.Center:
                    innerTextBox.TextAlign = HorizontalAlignment.Center;
                    break;
                case TextAlignment.Right:
                    innerTextBox.TextAlign = HorizontalAlignment.Right;
                    break;
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            if (placeholderLabel == null)
            {
                return;
            }

            // 当文本为空且未获得焦点时显示 Placeholder
            bool shouldShow = string.IsNullOrEmpty(innerTextBox.Text) &&
                              !isFocused &&
                              !string.IsNullOrEmpty(placeholder);

            if (placeholderLabel.Visible != shouldShow)
            {
                placeholderLabel.Visible = shouldShow;
            }
        }

        /// <summary>
        /// 更新滚动条显示状态
        /// </summary>
        private void UpdateScrollBars()
        {
            if (innerTextBox == null || !multiline || !autoScrollBars)
            {
                return;
            }

            // 在UI线程上执行
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateScrollBars));
                return;
            }

            try
            {
                // 检查文本是否溢出
                bool needScrollBar = IsTextOverflowing();

                // 确定新的滚动条状态
                ScrollBars newScrollBars = needScrollBar ? ScrollBars.Vertical : ScrollBars.None;

                // 仅在状态改变时更新
                if (innerTextBox.ScrollBars != newScrollBars)
                {
                    innerTextBox.ScrollBars = newScrollBars;
                    scrollBars = newScrollBars;
                }
            }
            catch
            {
                // 忽略错误, 避免在某些特殊情况下崩溃
            }
        }

        /// <summary>
        /// 检查文本是否溢出
        /// </summary>
        private bool IsTextOverflowing()
        {
            if (innerTextBox == null || string.IsNullOrEmpty(innerTextBox.Text))
            {
                return false;
            }

            try
            {
                // 使用 TextRenderer 测量文本高度
                Size proposedSize = new Size(innerTextBox.ClientSize.Width, int.MaxValue);

                TextFormatFlags flags = TextFormatFlags.TextBoxControl;
                if (wordWrap)
                {
                    flags |= TextFormatFlags.WordBreak;
                }

                Size textSize = TextRenderer.MeasureText(
                    innerTextBox.Text,
                    innerTextBox.Font,
                    proposedSize,
                    flags);

                // 检查文本高度是否超出控件高度
                // 添加一些容差以避免频繁切换
                int tolerance = 5;
                return textSize.Height > (innerTextBox.ClientSize.Height + tolerance);
            }
            catch
            {
                // 如果测量失败, 使用备用方法
                return UseAlternativeOverflowCheck();
            }
        }

        /// <summary>
        /// 备用的溢出检查方法
        /// </summary>
        private bool UseAlternativeOverflowCheck()
        {
            try
            {
                // 计算文本行数
                int lineCount = innerTextBox.GetLineFromCharIndex(innerTextBox.TextLength) + 1;

                // 估算每行高度
                int lineHeight = innerTextBox.Font.Height;

                // 估算总文本高度
                int totalTextHeight = lineCount * lineHeight;

                // 比较与控件高度
                return totalTextHeight > innerTextBox.ClientSize.Height;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 重置滚动条为自动模式
        /// </summary>
        public void ResetScrollBarsToAuto()
        {
            autoScrollBars = true;
            scrollBars = ScrollBars.None;
            UpdateScrollBars();
        }

        private bool ValidateKeyPress(char keyChar)
        {
            // 始终允许控制字符
            if (char.IsControl(keyChar))
            {
                return true;
            }

            switch (inputFormat)
            {
                case InputFormat.Integer:
                    if (allowNegative && keyChar == '-' && innerTextBox.Text.Length == 0)
                    {
                        return true;
                    }

                    return char.IsDigit(keyChar);

                case InputFormat.Decimal:
                    if (allowNegative && keyChar == '-' && innerTextBox.Text.Length == 0)
                    {
                        return true;
                    }

                    if (keyChar == '.' && !innerTextBox.Text.Contains('.'))
                    {
                        return true;
                    }

                    return char.IsDigit(keyChar);

                case InputFormat.Email:
                    return Regex.IsMatch(keyChar.ToString(), @"[a-zA-Z0-9@._\-]");

                case InputFormat.Custom:
                    if (!string.IsNullOrEmpty(customPattern))
                    {
                        return Regex.IsMatch(keyChar.ToString(), customPattern);
                    }

                    return true;

                default:
                    return true;
            }
        }

        private void ValidateInput()
        {
            if (innerTextBox == null || string.IsNullOrEmpty(innerTextBox.Text))
            {
                return;
            }

            switch (inputFormat)
            {
                case InputFormat.Integer:
                    if (!int.TryParse(innerTextBox.Text, out _))
                    {
                        innerTextBox.Text = "";
                    }

                    break;

                case InputFormat.Decimal:
                    if (!decimal.TryParse(innerTextBox.Text, out _))
                    {
                        innerTextBox.Text = "";
                    }

                    break;

                case InputFormat.Date:
                    if (!DateTime.TryParse(innerTextBox.Text, out _))
                    {
                        innerTextBox.Text = "";
                    }

                    break;

                case InputFormat.Email:
                    if (!Regex.IsMatch(innerTextBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        innerTextBox.Text = "";
                    }

                    break;
            }
        }

        private void UpdateAutoComplete()
        {
            if (!enableAutoComplete || autoCompleteSource == null ||
                string.IsNullOrEmpty(innerTextBox.Text))
            {
                HideAutoComplete();
                return;
            }

            var searchText = innerTextBox.Text.ToLower();
            var matches = autoCompleteSource
                .Where(s => s.ToLower().Contains(searchText) ||
                           GetPinyin(s).ToLower().Contains(searchText))
                .Take(10)
                .ToList();

            if (matches.Any())
            {
                ShowAutoComplete(matches);
            }
            else
            {
                HideAutoComplete();
            }
        }

        private void ShowAutoComplete(List<string> items)
        {
            autoCompleteListBox.Items.Clear();
            autoCompleteListBox.Items.AddRange(items.ToArray());

            var location = PointToScreen(new Point(innerTextBox.Left, innerTextBox.Bottom));
            autoCompleteListBox.Location = PointToClient(location);
            autoCompleteListBox.Width = innerTextBox.Width;
            autoCompleteListBox.Height = Math.Min(150, items.Count * 20 + 4);

            if (!Controls.Contains(autoCompleteListBox))
            {
                Controls.Add(autoCompleteListBox);
            }

            autoCompleteListBox.BringToFront();
            autoCompleteListBox.Visible = true;
            isAutoCompleteVisible = true;
        }

        private void HideAutoComplete()
        {
            if (autoCompleteListBox != null)
            {
                autoCompleteListBox.Visible = false;
                isAutoCompleteVisible = false;
            }
        }

        private void HandleAutoCompleteKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (autoCompleteListBox.SelectedIndex < autoCompleteListBox.Items.Count - 1)
                    {
                        autoCompleteListBox.SelectedIndex++;
                    }

                    e.Handled = true;
                    break;

                case Keys.Up:
                    if (autoCompleteListBox.SelectedIndex > 0)
                    {
                        autoCompleteListBox.SelectedIndex--;
                    }

                    e.Handled = true;
                    break;

                case Keys.Enter:
                    if (autoCompleteListBox.SelectedItem != null)
                    {
                        innerTextBox.Text = autoCompleteListBox.SelectedItem.ToString();
                        innerTextBox.SelectionStart = innerTextBox.Text.Length;
                        HideAutoComplete();
                    }
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    HideAutoComplete();
                    e.Handled = true;
                    break;
            }
        }

        private string GetPinyin(string chinese)
        {
            // Todo: 拼音转换
            return chinese;
        }

        /// <summary>
        /// 支持任意类型的自动完成源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="displaySelector"></param>
        public void SetAutoCompleteSource<T>(IEnumerable<T> source, Func<T, string> displaySelector = null)
        {
            autoCompleteSource = new List<string>();
            foreach (var item in source)
            {
                if (displaySelector != null)
                {
                    autoCompleteSource.Add(displaySelector(item));
                }
                else
                {
                    autoCompleteSource.Add(item.ToString());
                }
            }
        }

        /// <summary>
        /// 根据 InputFormat 获取类型化的值
        /// </summary>
        private object GetTypedValue()
        {
            if (innerTextBox == null || string.IsNullOrEmpty(innerTextBox.Text))
            {
                return null;
            }

            string text = innerTextBox.Text;

            switch (inputFormat)
            {
                case InputFormat.Integer:
                    if (long.TryParse(text, out long longVal))
                    {
                        // 如果在 int 范围内返回 int, 否则返回 long
                        if (longVal >= int.MinValue && longVal <= int.MaxValue)
                        {
                            return (int)longVal;
                        }
                        return longVal;
                    }
                    return null;

                case InputFormat.Decimal:
                    if (decimal.TryParse(text, out decimal decVal))
                    {
                        return decVal;
                    }
                    return null;

                case InputFormat.Date:
                    if (DateTime.TryParse(text, out DateTime dateVal))
                    {
                        return dateVal;
                    }
                    return null;

                case InputFormat.Email:
                case InputFormat.Text:
                case InputFormat.Custom:
                default:
                    return text;
            }
        }

        /// <summary>
        /// 设置类型化的值
        /// </summary>
        private void SetTypedValue(object value)
        {
            if (value == null)
            {
                Text = string.Empty;
                return;
            }

            switch (inputFormat)
            {
                case InputFormat.Integer:
                    if (value is int intVal)
                    {
                        Text = intVal.ToString();
                    }
                    else if (value is long longVal)
                    {
                        Text = longVal.ToString();
                    }
                    else
                    {
                        Text = Convert.ToInt64(value).ToString();
                    }
                    break;

                case InputFormat.Decimal:
                    if (value is decimal decVal)
                    {
                        Text = decVal.ToString($"F{decimalPlaces}");
                    }
                    else if (value is double dblVal)
                    {
                        Text = dblVal.ToString($"F{decimalPlaces}");
                    }
                    else if (value is float fltVal)
                    {
                        Text = fltVal.ToString($"F{decimalPlaces}");
                    }
                    else
                    {
                        Text = Convert.ToDecimal(value).ToString($"F{decimalPlaces}");
                    }
                    break;

                case InputFormat.Date:
                    if (value is DateTime dateVal)
                    {
                        Text = dateVal.ToShortDateString();
                    }
                    else
                    {
                        Text = Convert.ToDateTime(value).ToShortDateString();
                    }
                    break;

                default:
                    Text = value.ToString();
                    break;
            }

            // 设置值后验证范围
            ValidateAndClampValue();
        }

        /// <summary>
        /// 验证并约束当前值到有效范围
        /// </summary>
        private void ValidateAndClampValue()
        {
            // 防止递归调用
            if (isValidatingRange || innerTextBox == null)
            {
                return;
            }

            // 仅对数值类型进行范围验证
            if (inputFormat != InputFormat.Integer && inputFormat != InputFormat.Decimal)
            {
                return;
            }

            // 空文本不需要验证
            if (string.IsNullOrEmpty(innerTextBox.Text))
            {
                return;
            }

            // 如果没有设置范围限制, 不需要验证
            if (!minimum.HasValue && !maximum.HasValue)
            {
                return;
            }

            isValidatingRange = true;

            try
            {
                if (decimal.TryParse(innerTextBox.Text, out decimal currentValue))
                {
                    decimal? clampedValue = null;
                    bool wasOutOfRange = false;
                    decimal originalValue = currentValue;

                    // 检查最小值
                    if (minimum.HasValue && currentValue < minimum.Value)
                    {
                        clampedValue = minimum.Value;
                        wasOutOfRange = true;
                    }

                    // 检查最大值
                    if (maximum.HasValue && currentValue > maximum.Value)
                    {
                        clampedValue = maximum.Value;
                        wasOutOfRange = true;
                    }

                    // 如果值超出范围
                    if (wasOutOfRange && clampedValue.HasValue)
                    {
                        // 触发事件
                        var args = new ValueOutOfRangeEventArgs(originalValue, clampedValue.Value, minimum, maximum);
                        OnValueOutOfRange(args);

                        // 如果事件没有取消, 则修正值
                        if (!args.Cancel)
                        {
                            if (inputFormat == InputFormat.Integer)
                            {
                                innerTextBox.Text = ((long)clampedValue.Value).ToString();
                            }
                            else
                            {
                                innerTextBox.Text = clampedValue.Value.ToString($"F{decimalPlaces}");
                            }
                        }
                    }
                }
            }
            finally
            {
                isValidatingRange = false;
            }
        }

        /// <summary>
        /// 检查值是否在有效范围内
        /// </summary>
        public bool IsValueInRange()
        {
            if (inputFormat != InputFormat.Integer && inputFormat != InputFormat.Decimal)
            {
                return true;
            }

            if (string.IsNullOrEmpty(innerTextBox?.Text))
            {
                return true;
            }

            if (!minimum.HasValue && !maximum.HasValue)
            {
                return true;
            }

            if (decimal.TryParse(innerTextBox.Text, out decimal currentValue))
            {
                if (minimum.HasValue && currentValue < minimum.Value)
                {
                    return false;
                }

                if (maximum.HasValue && currentValue > maximum.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取范围验证错误信息
        /// </summary>
        public string GetRangeValidationMessage()
        {
            if (IsValueInRange())
            {
                return null;
            }

            if (minimum.HasValue && maximum.HasValue)
            {
                return $"值必须在 {minimum.Value} 和 {maximum.Value} 之间";
            }
            else if (minimum.HasValue)
            {
                return $"值不能小于 {minimum.Value}";
            }
            else if (maximum.HasValue)
            {
                return $"值不能大于 {maximum.Value}";
            }

            return null;
        }

        #endregion

        #region 重写方法

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawBackground(g);
            DrawBorder(g);
            DrawPrefix(g);
            DrawSuffix(g);
            DrawIcon(g);
        }

        protected override void DrawBackground(Graphics g)
        {
            var rect = ClientRectangle;

            Color bgColor;
            if (BackcolorForced)
            {
                bgColor = BackColor;
            }
            else if (UseTheme && Theme != null)
            {
                bgColor = isFocused ? Theme.Colors.Surface : Theme.Colors.BackgroundSecondary;
            }
            else
            {
                bgColor = BackColor;
            }

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, rect);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (!showBorder || borderSize <= 0)
            {
                return;
            }

            var rect = ClientRectangle;

            // 根据边框宽度调整矩形
            rect.Width -= 1;
            rect.Height -= 1;

            // 确定边框颜色
            Color currentBorderColor;
            if (isFocused)
            {
                // 焦点状态：使用 BorderFocusedColor, 如果为空则使用主题色或默认色
                if (!borderFocusedColor.IsEmpty)
                {
                    currentBorderColor = borderFocusedColor;
                }
                else if (UseTheme && Theme != null)
                {
                    currentBorderColor = Theme.Colors.Primary;
                }
                else
                {
                    currentBorderColor = Color.DodgerBlue;
                }
            }
            else
            {
                // 普通状态：使用 BorderColor
                currentBorderColor = borderColor;
            }

            // 根据边框宽度调整矩形(居中边框)
            if (borderSize > 1)
            {
                int offset = borderSize / 2;
                rect.Inflate(-offset, -offset);
            }

            using (var pen = new Pen(currentBorderColor, borderSize))
            {
                // 设置边框样式
                switch (customBorderStyle)
                {
                    case BorderStyle.FixedSingle:
                    case BorderStyle.Fixed3D:
                        pen.DashStyle = DashStyle.Solid;
                        break;
                }

                g.DrawRectangle(pen, rect);
            }
        }

        protected override void DrawContent(Graphics g)
        {

        }

        private void DrawPrefix(Graphics g)
        {
            if (!ShowPrefix || string.IsNullOrEmpty(Prefix))
            {
                return;
            }

            using (var brush = new SolidBrush(PrefixColor))
            {
                int borderOffset = showBorder ? borderSize : 0;
                int iconOffset = (Icon != null && IconPosition == IconPosition.Left) ? 28 : 0;

                var x = Padding.Left + borderOffset + iconOffset;
                var y = (Height - (int)PrefixFont.GetHeight()) / 2;

                g.DrawString(Prefix, PrefixFont, brush, x, y);
            }
        }

        private void DrawSuffix(Graphics g)
        {
            if (!ShowSuffix || string.IsNullOrEmpty(Suffix))
            {
                return;
            }

            using (var brush = new SolidBrush(SuffixColor))
            {
                var size = g.MeasureString(Suffix, SuffixFont);
                int borderOffset = showBorder ? borderSize : 0;
                int iconOffset = (Icon != null && IconPosition == IconPosition.Right) ? 28 : 0;

                var x = Width - Padding.Right - borderOffset - size.Width - iconOffset;
                var y = (Height - (int)SuffixFont.GetHeight()) / 2;

                g.DrawString(Suffix, SuffixFont, brush, x, y);
            }
        }

        private void DrawIcon(Graphics g)
        {
            if (Icon == null)
            {
                return;
            }

            int borderOffset = showBorder ? borderSize : 0;
            var iconRect = new Rectangle();

            if (IconPosition == IconPosition.Left)
            {
                iconRect = new Rectangle(
                    Padding.Left + borderOffset,
                    (Height - 20) / 2,
                    20, 20);
            }
            else
            {
                iconRect = new Rectangle(
                    Width - Padding.Right - borderOffset - 20,
                    (Height - 20) / 2,
                    20, 20);
            }

            g.DrawImage(Icon, iconRect);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (innerTextBox != null && innerFont == null)
            {
                innerTextBox.Font = Font;
            }
            UpdateLayout();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            UpdateLayout();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // 应用最小高度限制
            if (height < MinHeight)
            {
                height = MinHeight;
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        #endregion

        #region 资源释放

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip?.Dispose();
                placeholderLabel?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 枚举和辅助类

    public enum IconPosition
    {
        Left,
        Right
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public enum InputFormat
    {
        Text,
        Integer,
        Decimal,
        Date,
        Email,
        Custom
    }

    /// <summary>
    /// 值超出范围事件参数
    /// </summary>
    public class ValueOutOfRangeEventArgs : EventArgs
    {
        /// <summary>
        /// 原始值(超出范围的值)
        /// </summary>
        public decimal OriginalValue { get; }

        /// <summary>
        /// 修正后的值
        /// </summary>
        public decimal ClampedValue { get; }

        /// <summary>
        /// 最小值限制
        /// </summary>
        public decimal? Minimum { get; }

        /// <summary>
        /// 最大值限制
        /// </summary>
        public decimal? Maximum { get; }

        /// <summary>
        /// 是否取消自动修正(设为 true 将保留原始值)
        /// </summary>
        public bool Cancel { get; set; }

        public ValueOutOfRangeEventArgs(decimal originalValue, decimal clampedValue, decimal? minimum, decimal? maximum)
        {
            OriginalValue = originalValue;
            ClampedValue = clampedValue;
            Minimum = minimum;
            Maximum = maximum;
            Cancel = false;
        }
    }

    #endregion
}
