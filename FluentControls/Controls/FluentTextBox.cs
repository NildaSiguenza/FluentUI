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
        // private bool autoHeight = true;

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

        private bool multiline = false;
        private bool wordWrap = true;
        private bool showPrefix = true;
        private bool showSuffix = true;
        private bool isFocused = false;

        private List<string> autoCompleteSource;
        private bool enableAutoComplete = false;
        private bool isAutoCompleteVisible = false;

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
                BackColor = InnerBackColor
            };

            // 设置 Anchor 让 TextBox 随控件大小变化
            innerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            innerTextBox.TextChanged += (s, e) =>
            {
                OnTextChanged(e);
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
                State = ControlState.Focused;
                Invalidate();
            };

            innerTextBox.LostFocus += (s, e) =>
            {
                isFocused = false;
                State = ControlState.Normal;
                HideAutoComplete();
                Invalidate();
            };

            innerTextBox.KeyDown += (s, e) =>
            {
                if (isAutoCompleteVisible)
                {
                    HandleAutoCompleteKeyDown(e);
                }
            };

            Controls.Add(innerTextBox);
            UpdateLayout();
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
                    UpdateLayout();
                }
            }
        }

        [Category("Fluent")]
        [Description("最小高度")]
        [DefaultValue(24)]
        public int MinHeight { get; set; } = 24;

        #region 新增：边框属性

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
        [Description("获得焦点时的边框颜色（留空使用主题色）")]
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

        #endregion

        [Category("Fluent")]
        [Description("占位符文本")]
        public string Placeholder
        {
            get => placeholder;
            set
            {
                placeholder = value;
                Invalidate();
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
        [Description("文本内边距（控制输入区域与控件边缘的距离）")]
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
                multiline = value;
                if (innerTextBox != null)
                {
                    innerTextBox.Multiline = value;
                    innerTextBox.ScrollBars = value ? ScrollBars.Vertical : ScrollBars.None;
                }
                UpdateLayout();
            }
        }

        [Category("Fluent")]
        [Description("自动换行")]
        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                wordWrap = value;
                if (innerTextBox != null)
                {
                    innerTextBox.WordWrap = value;
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

            // 使用主题色作为默认边框色
            if (borderColor == Color.Gray || borderColor == SystemColors.ControlDark)
            {
                borderColor = Theme.Colors.Border;
            }
        }

        #endregion

        #region 方法

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
                    prefixWidth = (int)Math.Ceiling(prefixSize.Width) + PrefixSpacing;
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

            // 计算边框占用的空间
            int borderOffset = showBorder ? borderSize : 0;

            // 计算水平位置和宽度
            int x = Padding.Left + borderOffset;
            int width = Width - Padding.Horizontal - (borderOffset * 2);

            // 根据图标位置调整
            if (Icon != null && IconPosition == IconPosition.Left)
            {
                x += iconWidth;
                width -= iconWidth;
            }
            else if (Icon != null && IconPosition == IconPosition.Right)
            {
                width -= iconWidth;
            }

            // 调整前后缀
            x += prefixWidth;
            width -= prefixWidth + suffixWidth;

            // 计算垂直位置和高度
            int y = Padding.Top + borderOffset;
            int height = Height - Padding.Vertical - (borderOffset * 2);

            // 确保最小尺寸
            width = Math.Max(20, width);
            height = Math.Max(10, height);

            // 设置位置和大小
            innerTextBox.Location = new Point(x, y);
            innerTextBox.Size = new Size(width, height);

            // 设置 Anchor 让 TextBox 随控件大小变化
            if (Multiline)
            {
                innerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            }
            else
            {
                innerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            }

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
            // 简化的拼音转换, 实际使用时可以引入专门的拼音库
            // 这里仅作示例
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
            DrawPlaceholder(g);
        }

        protected override void DrawBackground(Graphics g)
        {
            var rect = ClientRectangle;

            Color bgColor;
            if (UseTheme && Theme != null)
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
                // 焦点状态：使用 BorderFocusedColor，如果为空则使用主题色或默认色
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

            // 根据边框宽度调整矩形（居中边框）
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

        private void DrawPlaceholder(Graphics g)
        {
            if (!string.IsNullOrEmpty(innerTextBox.Text) ||
                string.IsNullOrEmpty(Placeholder) ||
                isFocused)
            {
                return;
            }

            Color placeholderColor;
            if (UseTheme && Theme != null)
            {
                placeholderColor = Theme.Colors.TextDisabled;
            }
            else
            {
                placeholderColor = Color.Gray;
            }

            using (var brush = new SolidBrush(placeholderColor))
            {
                var rect = innerTextBox.Bounds;
                var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

                TextRenderer.DrawText(g, Placeholder, innerTextBox.Font, rect, placeholderColor, flags);
            }
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
    }

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

}
