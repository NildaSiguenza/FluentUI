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
using FluentControls.Controls;
using FluentControls.IconFonts;
using ArrowDirection = FluentControls.Controls.ArrowDirection;
using DataGridViewEditMode = FluentControls.Controls.DataGridViewEditMode;
using DataGridViewSelectionMode = FluentControls.Controls.DataGridViewSelectionMode;
using SelectionMode = FluentControls.Controls.SelectionMode;
using TabAlignment = FluentControls.Controls.TabAlignment;

namespace FluentControls.WinformDemo
{
    #region 基础控件配置

    public class ButtonDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本")]
        [PropertyDescription("按钮上显示的文字")]
        public string Text { get; set; } = "Fluent Button";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("图像")]
        [PropertyDescription("按钮上显示的图像")]
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image Image { get; set; }

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 外观样式
        [PropertyCategory("外观样式")]
        [PropertyDisplayName("按钮风格")]
        [PropertyDescription("Primary / Secondary / Text / Danger / Success")]
        public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Primary;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("圆角半径")]
        public int CornerRadius { get; set; } = 6;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        // 内容布局
        [PropertyCategory("内容布局")]
        [PropertyDisplayName("布局方式")]
        [PropertyDescription("图标与文本的排列方向")]
        public ContentLayout ContentLayout { get; set; } = ContentLayout.ImageBeforeText;

        [PropertyCategory("内容布局")]
        [PropertyDisplayName("文本对齐")]
        public ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleCenter;

        [PropertyCategory("内容布局")]
        [PropertyDisplayName("图文间距")]
        public int ImageTextSpacing { get; set; } = 4;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 160;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 42;

        // 自定义颜色
        [PropertyCategory("自定义颜色")]
        [PropertyDisplayName("背景色")]
        public Color BackColor { get; set; } = Color.FromArgb(0, 120, 212);

        [PropertyCategory("自定义颜色")]
        [PropertyDisplayName("前景色")]
        public Color ForeColor { get; set; } = Color.White;

        [PropertyCategory("自定义颜色")]
        [PropertyDisplayName("强制背景色")]
        public Color ForceBackColor { get; set; } = Color.Empty;

        [PropertyCategory("自定义颜色")]
        [PropertyDisplayName("强制前景色")]
        public Color ForceForeColor { get; set; } = Color.Empty;

        // 交互
        [PropertyCategory("交互")]
        [PropertyDisplayName("涟漪效果")]
        [PropertyDescription("点击时是否显示涟漪动画")]
        public bool EnableRippleEffect { get; set; } = true;

        [PropertyCategory("交互")]
        [PropertyDisplayName("快捷键")]
        public Keys ShortcutKeys { get; set; } = Keys.None;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentButton btn)
            {
                btn.Text = Text;
                btn.Image = Image;
                btn.Enabled = Enabled;
                btn.ButtonStyle = ButtonStyle;
                btn.CornerRadius = CornerRadius;
                btn.ContentLayout = ContentLayout;
                btn.TextAlign = TextAlign;
                btn.ImageTextSpacing = ImageTextSpacing;
                btn.Size = new Size(Width, Height);
                btn.BackColor = BackColor;
                btn.ForeColor = ForeColor;
                btn.NormalBackColor = ForceBackColor;
                btn.NormalForeColor = ForceForeColor;
                btn.EnableRippleEffect = EnableRippleEffect;
                btn.ShortcutKeys = ShortcutKeys;
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentButton btn)
            {
                Text = btn.Text;
                Image = btn.Image;
                Enabled = btn.Enabled;
                ButtonStyle = btn.ButtonStyle;
                CornerRadius = btn.CornerRadius;
                ContentLayout = btn.ContentLayout;
                TextAlign = btn.TextAlign;
                ImageTextSpacing = btn.ImageTextSpacing;
                Width = btn.Width;
                Height = btn.Height;
                ForceBackColor = btn.NormalBackColor ?? Color.Empty;
                ForceForeColor = btn.NormalForeColor ?? Color.Empty;
                BackColor = btn.BackColor;
                ForeColor = btn.ForeColor;
                EnableRippleEffect = btn.EnableRippleEffect;
                ShortcutKeys = btn.ShortcutKeys;
            }
        }

        public static ButtonDemoConfig FromControl(FluentButton btn)
        {
            var cfg = new ButtonDemoConfig();
            cfg.ReadFrom(btn);
            return cfg;
        }
    }

    public class LabelDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本")]
        public string Text { get; set; } = "Fluent Label 示例";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本对齐")]
        public ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleCenter;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("自动大小")]
        public bool AutoSize { get; set; } = false;

        // 形状
        [PropertyCategory("形状")]
        [PropertyDisplayName("标签形状")]
        public LabelShape Shape { get; set; } = LabelShape.RoundedRectangle;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = false;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框大小")]
        public int BorderSize { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Gray;

        // 文本装饰
        [PropertyCategory("文本装饰")]
        [PropertyDisplayName("装饰类型")]
        public TextDecoration TextDecoration { get; set; } = TextDecoration.None;

        [PropertyCategory("文本装饰")]
        [PropertyDisplayName("装饰颜色")]
        public Color DecorationColor { get; set; } = Color.Red;

        [PropertyCategory("文本装饰")]
        [PropertyDisplayName("装饰粗细")]
        public int DecorationThickness { get; set; } = 2;

        // 链接
        [PropertyCategory("链接")]
        [PropertyDisplayName("链接模式")]
        public bool IsLinkMode { get; set; } = false;

        [PropertyCategory("链接")]
        [PropertyDisplayName("链接颜色")]
        public Color LinkColor { get; set; } = Color.Blue;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("透明背景")]
        public bool TransparentBackground { get; set; } = false;

        [PropertyCategory("外观")]
        [PropertyDisplayName("背景色")]
        public Color BackColor { get; set; } = Color.White;

        [PropertyCategory("外观")]
        [PropertyDisplayName("前景色")]
        public Color ForeColor { get; set; } = Color.FromArgb(32, 32, 32);

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 220;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 44;

        // 动画
        [PropertyCategory("动画")]
        [PropertyDisplayName("打字机效果")]
        public bool EnableTypewriterEffect { get; set; } = false;

        [PropertyCategory("动画")]
        [PropertyDisplayName("打字速度 (ms)")]
        public int TypewriterSpeed { get; set; } = 50;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentLabel lbl)
            {
                lbl.AutoSize = AutoSize; // 先设 AutoSize, 避免后续 Size 被覆盖
                lbl.Text = Text;
                lbl.TextAlign = TextAlign;
                lbl.Shape = Shape;
                lbl.ShowBorder = ShowBorder;
                lbl.BorderSize = BorderSize;
                lbl.BorderColor = BorderColor;
                lbl.TextDecoration = TextDecoration;
                lbl.DecorationColor = DecorationColor;
                lbl.DecorationThickness = DecorationThickness;
                lbl.IsLinkMode = IsLinkMode;
                lbl.LinkColor = LinkColor;
                lbl.TransparentBackground = TransparentBackground;
                lbl.ForeColor = ForeColor;
                if (!TransparentBackground && !UseTheme)
                {
                    lbl.BackColor = BackColor;
                }

                if (!AutoSize)
                {
                    lbl.Size = new Size(Width, Height);
                }

                lbl.EnableTypewriterEffect = EnableTypewriterEffect;
                lbl.TypewriterSpeed = TypewriterSpeed;
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentLabel lbl)
            {
                Text = lbl.Text;
                TextAlign = lbl.TextAlign;
                AutoSize = lbl.AutoSize;
                Shape = lbl.Shape;
                ShowBorder = lbl.ShowBorder;
                BorderSize = lbl.BorderSize;
                BorderColor = lbl.BorderColor;
                TextDecoration = lbl.TextDecoration;
                DecorationColor = lbl.DecorationColor;
                DecorationThickness = lbl.DecorationThickness;
                IsLinkMode = lbl.IsLinkMode;
                LinkColor = lbl.LinkColor;
                TransparentBackground = lbl.TransparentBackground;
                BackColor = lbl.BackColor;
                ForeColor = lbl.ForeColor;
                Width = lbl.Width;
                Height = lbl.Height;
                EnableTypewriterEffect = lbl.EnableTypewriterEffect;
                TypewriterSpeed = lbl.TypewriterSpeed;
            }
        }

        public static LabelDemoConfig FromControl(FluentLabel lbl)
        {
            var cfg = new LabelDemoConfig();
            cfg.ReadFrom(lbl);
            return cfg;
        }
    }

    public class TextBoxDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本")]
        [PropertyDescription("输入框中的文本内容")]
        public string Text { get; set; } = "";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("占位符")]
        [PropertyDescription("无内容时显示的提示文字")]
        public string Placeholder { get; set; } = "请输入内容...";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("只读")]
        [PropertyDescription("是否禁止编辑")]
        public bool ReadOnly { get; set; } = false;

        // 输入控制
        [PropertyCategory("输入控制")]
        [PropertyDisplayName("输入格式")]
        [PropertyDescription("Text=任意 | Integer=整数 | Decimal=小数 | Email | Phone 等")]
        public InputFormat InputFormat { get; set; } = InputFormat.Text;

        [PropertyCategory("输入控制")]
        [PropertyDisplayName("允许负数")]
        [PropertyDescription("数值模式下是否允许输入负号")]
        public bool AllowNegative { get; set; } = true;

        [PropertyCategory("输入控制")]
        [PropertyDisplayName("小数位数")]
        [PropertyDescription("数值模式下允许的小数位数")]
        public int DecimalPlaces { get; set; } = 2;

        // 前缀后缀
        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("前缀")]
        [PropertyDescription("显示在输入框左侧的固定文本")]
        public string Prefix { get; set; } = "";

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("后缀")]
        [PropertyDescription("显示在输入框右侧的固定文本")]
        public string Suffix { get; set; } = "";

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("显示前缀")]
        public bool ShowPrefix { get; set; } = true;

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("显示后缀")]
        public bool ShowSuffix { get; set; } = true;

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("前缀间距")]
        public int PrefixSpacing { get; set; } = 4;

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("后缀间距")]
        public int SuffixSpacing { get; set; } = 4;

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("前缀颜色")]
        public Color PrefixColor { get; set; } = Color.Gray;

        [PropertyCategory("前缀后缀")]
        [PropertyDisplayName("后缀颜色")]
        public Color SuffixColor { get; set; } = Color.Gray;

        // 多行模式
        [PropertyCategory("多行模式")]
        [PropertyDisplayName("多行")]
        [PropertyDescription("启用多行文本输入")]
        public bool Multiline { get; set; } = false;

        [PropertyCategory("多行模式")]
        [PropertyDisplayName("自动换行")]
        [PropertyDescription("多行模式下文本是否自动换行")]
        public bool WordWrap { get; set; } = true;

        // 文本对齐
        [PropertyCategory("文本")]
        [PropertyDisplayName("文本对齐")]
        [PropertyDescription("输入文本的水平对齐方式")]
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

        [PropertyCategory("文本")]
        [PropertyDisplayName("文字颜色")]
        public Color InnerTextColor { get; set; } = Color.Black;

        [PropertyCategory("文本")]
        [PropertyDisplayName("文本框背景色")]
        public Color InnerBackColor { get; set; } = Color.White;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = true;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框宽度")]
        public int BorderSize { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Gray;

        [PropertyCategory("边框")]
        [PropertyDisplayName("圆角半径")]
        public int CornerRadius { get; set; } = 0;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 240;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        // 自动完成
        [PropertyCategory("自动完成")]
        [PropertyDisplayName("启用自动完成")]
        [PropertyDescription("输入时显示匹配的候选项")]
        public bool EnableAutoComplete { get; set; } = false;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentTextBox tb)
            {
                tb.Text = Text;
                tb.Placeholder = Placeholder;
                tb.Enabled = Enabled;
                tb.ReadOnly = ReadOnly;
                tb.InputFormat = InputFormat;
                tb.AllowNegative = AllowNegative;
                tb.DecimalPlaces = DecimalPlaces;
                tb.Prefix = Prefix;
                tb.Suffix = Suffix;
                tb.ShowPrefix = ShowPrefix;
                tb.ShowSuffix = ShowSuffix;
                tb.PrefixSpacing = PrefixSpacing;
                tb.SuffixSpacing = SuffixSpacing;
                tb.PrefixColor = PrefixColor;
                tb.SuffixColor = SuffixColor;
                tb.Multiline = Multiline;
                tb.WordWrap = WordWrap;
                tb.TextAlignment = TextAlignment;
                tb.InnerTextColor = InnerTextColor;
                tb.InnerBackColor = InnerBackColor;
                tb.ShowBorder = ShowBorder;
                tb.BorderSize = BorderSize;
                tb.BorderColor = BorderColor;
                tb.EnableAutoComplete = EnableAutoComplete;
                tb.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentTextBox tb)
            {
                Text = tb.Text;
                Placeholder = tb.Placeholder;
                Enabled = tb.Enabled;
                ReadOnly = tb.ReadOnly;
                InputFormat = tb.InputFormat;
                AllowNegative = tb.AllowNegative;
                DecimalPlaces = tb.DecimalPlaces;
                Prefix = tb.Prefix;
                Suffix = tb.Suffix;
                ShowPrefix = tb.ShowPrefix;
                ShowSuffix = tb.ShowSuffix;
                PrefixSpacing = tb.PrefixSpacing;
                SuffixSpacing = tb.SuffixSpacing;
                PrefixColor = tb.PrefixColor;
                SuffixColor = tb.SuffixColor;
                Multiline = tb.Multiline;
                WordWrap = tb.WordWrap;
                TextAlignment = tb.TextAlignment;
                InnerTextColor = tb.InnerTextColor;
                InnerBackColor = tb.InnerBackColor;
                ShowBorder = tb.ShowBorder;
                BorderSize = tb.BorderSize;
                BorderColor = tb.BorderColor;
                EnableAutoComplete = tb.EnableAutoComplete;
                Width = tb.Width;
                Height = tb.Height;
            }
        }

        public static TextBoxDemoConfig FromControl(FluentTextBox tb)
        {
            var cfg = new TextBoxDemoConfig();
            cfg.ReadFrom(tb);
            return cfg;
        }
    }

    public class CheckBoxDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本")]
        [PropertyDescription("复选框旁边显示的文本")]
        public string Text { get; set; } = "复选框选项";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("选中状态")]
        [PropertyDescription("复选框是否被选中")]
        public bool Checked { get; set; } = false;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 外观样式
        [PropertyCategory("外观样式")]
        [PropertyDisplayName("显示样式")]
        [PropertyDescription("Standard=标准勾选框 | Switch=开关切换")]
        public CheckBoxStyle CheckBoxStyle { get; set; } = CheckBoxStyle.Standard;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("勾选框大小")]
        [PropertyDescription("标准模式下勾选框的像素大小 (12-40)")]
        public int CheckBoxSize { get; set; } = 18;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("自动大小")]
        [PropertyDescription("根据内容自动调整控件尺寸")]
        public bool AutoSize { get; set; } = false;

        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("勾选框位置")]
        [PropertyDescription("标准模式下勾选框相对于文本的位置")]
        public ContentAlignment CheckAlign { get; set; } = ContentAlignment.MiddleLeft;

        [PropertyCategory("布局")]
        [PropertyDisplayName("间距")]
        [PropertyDescription("勾选框/开关与文本之间的像素间距")]
        public int Spacing { get; set; } = 8;

        // 交互
        [PropertyCategory("交互")]
        [PropertyDisplayName("独立点击模式")]
        [PropertyDescription("勾选区域与文本区域分别响应点击")]
        public bool IndependentClickMode { get; set; } = false;

        [PropertyCategory("交互")]
        [PropertyDisplayName("涟漪效果")]
        public bool EnableRippleEffect { get; set; } = true;

        // 颜色
        [PropertyCategory("颜色")]
        [PropertyDisplayName("前景色")]
        [PropertyDescription("文本颜色")]
        public Color ForeColor { get; set; } = SystemColors.ControlText;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 160;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentCheckBox cb)
            {
                cb.Text = Text;
                cb.Checked = Checked;
                cb.Enabled = Enabled;
                cb.CheckBoxStyle = CheckBoxStyle;
                cb.CheckBoxSize = CheckBoxSize;
                cb.AutoSize = AutoSize;
                cb.CheckAlign = CheckAlign;
                cb.Spacing = Spacing;
                cb.IndependentClickMode = IndependentClickMode;
                cb.EnableRippleEffect = EnableRippleEffect;
                cb.ForeColor = ForeColor;
                if (!AutoSize)
                {
                    cb.Size = new Size(Width, Height);
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentCheckBox cb)
            {
                Text = cb.Text;
                Checked = cb.Checked;
                Enabled = cb.Enabled;
                CheckBoxStyle = cb.CheckBoxStyle;
                CheckBoxSize = cb.CheckBoxSize;
                AutoSize = cb.AutoSize;
                CheckAlign = cb.CheckAlign;
                Spacing = cb.Spacing;
                IndependentClickMode = cb.IndependentClickMode;
                EnableRippleEffect = cb.EnableRippleEffect;
                ForeColor = cb.ForeColor;
                Width = cb.Width;
                Height = cb.Height;
            }
        }

        public static CheckBoxDemoConfig FromControl(FluentCheckBox cb)
        {
            var cfg = new CheckBoxDemoConfig();
            cfg.ReadFrom(cb);
            return cfg;
        }
    }

    public class RadioDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本")]
        [PropertyDescription("单选按钮旁显示的文本")]
        public string Text { get; set; } = "单选按钮";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("选中状态")]
        [PropertyDescription("该单选按钮是否被选中")]
        public bool Checked { get; set; } = true;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 分组
        [PropertyCategory("分组")]
        [PropertyDisplayName("组名称")]
        [PropertyDescription("同组内的单选按钮互斥, 同一时间只能选中一个")]
        public string GroupName { get; set; } = "default";

        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("圆点位置")]
        [PropertyDescription("单选圆点相对于文本的对齐位置")]
        public ContentAlignment RadioAlign { get; set; } = ContentAlignment.MiddleLeft;

        [PropertyCategory("布局")]
        [PropertyDisplayName("间距")]
        [PropertyDescription("圆点与文本之间的像素间距")]
        public int Spacing { get; set; } = 8;

        // 交互
        [PropertyCategory("交互")]
        [PropertyDisplayName("涟漪效果")]
        [PropertyDescription("点击时是否显示涟漪动画")]
        public bool EnableRippleEffect { get; set; } = true;

        [PropertyCategory("交互")]
        [PropertyDisplayName("启用动画")]
        [PropertyDescription("选中/取消时是否播放过渡动画")]
        public bool EnableAnimation { get; set; } = true;

        // 颜色
        [PropertyCategory("颜色")]
        [PropertyDisplayName("前景色")]
        [PropertyDescription("文本颜色")]
        public Color ForeColor { get; set; } = SystemColors.ControlText;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 160;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentRadio radio)
            {
                radio.Text = Text;
                radio.Checked = Checked;
                radio.Enabled = Enabled;
                radio.GroupName = GroupName;
                radio.RadioAlign = RadioAlign;
                radio.Spacing = Spacing;
                radio.EnableRippleEffect = EnableRippleEffect;
                radio.EnableAnimation = EnableAnimation;
                radio.ForeColor = ForeColor;
                radio.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentRadio radio)
            {
                Text = radio.Text;
                Checked = radio.Checked;
                Enabled = radio.Enabled;
                GroupName = radio.GroupName;
                RadioAlign = radio.RadioAlign;
                Spacing = radio.Spacing;
                EnableRippleEffect = radio.EnableRippleEffect;
                EnableAnimation = radio.EnableAnimation;
                ForeColor = radio.ForeColor;
                Width = radio.Width;
                Height = radio.Height;
            }
        }

        public static RadioDemoConfig FromControl(FluentRadio radio)
        {
            var cfg = new RadioDemoConfig();
            cfg.ReadFrom(radio);
            return cfg;
        }
    }

    public class ColorPickerDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("选中颜色")]
        [PropertyDescription("当前选中的颜色值")]
        public Color SelectedColor { get; set; } = Color.CornflowerBlue;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("色块形状")]
        [PropertyDescription("Square=正方形 | Circle=圆形 | RoundedSquare=圆角正方形")]
        public ColorBlockShape BlockShape { get; set; } = ColorBlockShape.Square;

        [PropertyCategory("外观")]
        [PropertyDisplayName("色块大小")]
        [PropertyDescription("颜色预览块的像素大小")]
        public int BlockSize { get; set; } = 24;

        [PropertyCategory("外观")]
        [PropertyDisplayName("颜色文本格式")]
        [PropertyDescription("Hex=#RRGGBB | RGB=RGB(r,g,b) | ARGB | Name")]
        public ColorTextFormat TextFormat { get; set; } = ColorTextFormat.Hex;

        [PropertyCategory("外观")]
        [PropertyDisplayName("显示颜色文本")]
        [PropertyDescription("是否在色块旁显示颜色值文本")]
        public bool ShowColorText { get; set; } = true;

        [PropertyCategory("外观")]
        [PropertyDisplayName("显示Alpha通道")]
        [PropertyDescription("是否支持选择透明度")]
        public bool ShowAlpha { get; set; } = true;

        [PropertyCategory("外观")]
        [PropertyDisplayName("色块间距")]
        [PropertyDescription("色块与文本之间的像素间距")]
        public int Spacing { get; set; } = 8;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = true;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框粗细")]
        public int BorderSize { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Gray;

        [PropertyCategory("边框")]
        [PropertyDisplayName("使用主题边框色")]
        [PropertyDescription("启用后边框颜色跟随主题变化")]
        public bool UseBorderThemeColor { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 200;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentColorPicker cp)
            {
                cp.SelectedColor = SelectedColor;
                cp.Enabled = Enabled;
                cp.BlockShape = BlockShape;
                cp.BlockSize = BlockSize;
                cp.TextFormat = TextFormat;
                cp.ShowColorText = ShowColorText;
                cp.ShowAlpha = ShowAlpha;
                cp.Spacing = Spacing;
                cp.ShowBorder = ShowBorder;
                cp.BorderSize = BorderSize;
                cp.BorderColor = BorderColor;
                cp.UseBorderThemeColor = UseBorderThemeColor;
                cp.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentColorPicker cp)
            {
                SelectedColor = cp.SelectedColor;
                Enabled = cp.Enabled;
                BlockShape = cp.BlockShape;
                BlockSize = cp.BlockSize;
                TextFormat = cp.TextFormat;
                ShowColorText = cp.ShowColorText;
                ShowAlpha = cp.ShowAlpha;
                Spacing = cp.Spacing;
                ShowBorder = cp.ShowBorder;
                BorderSize = cp.BorderSize;
                BorderColor = cp.BorderColor;
                UseBorderThemeColor = cp.UseBorderThemeColor;
                Width = cp.Width;
                Height = cp.Height;
            }
        }

        public static ColorPickerDemoConfig FromControl(FluentColorPicker cp)
        {
            var cfg = new ColorPickerDemoConfig();
            cfg.ReadFrom(cp);
            return cfg;
        }
    }

    public class ColorComboBoxDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("选中颜色")]
        [PropertyDescription("当前选中的颜色")]
        public Color SelectedColor { get; set; } = Color.FromArgb(0, 120, 212);

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 外观样式
        [PropertyCategory("外观样式")]
        [PropertyDisplayName("颜色块形状")]
        [PropertyDescription("颜色预览块的形状 (Square / Circle)")]
        public ColorBlockShape BlockShape { get; set; } = ColorBlockShape.Square;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("颜色块大小")]
        [PropertyDescription("颜色预览块的像素大小")]
        public int BlockSize { get; set; } = 18;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("显示颜色文本")]
        [PropertyDescription("是否在颜色块旁边显示颜色文本")]
        public bool ShowColorText { get; set; } = true;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("颜色块与文本间距")]
        public int Spacing { get; set; } = 8;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = true;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框粗细")]
        public int BorderSize { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Gray;

        [PropertyCategory("边框")]
        [PropertyDisplayName("使用主题边框色")]
        [PropertyDescription("是否使用主题提供的边框颜色")]
        public bool UseBorderThemeColor { get; set; } = true;

        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("自动调整宽度")]
        [PropertyDescription("内容改变时是否自动调整控件宽度")]
        public bool AutoResizeWidth { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 200;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 28;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentColorComboBox ccb)
            {
                ccb.Enabled = Enabled;
                ccb.BlockShape = BlockShape;
                ccb.BlockSize = BlockSize;
                ccb.ShowColorText = ShowColorText;
                ccb.Spacing = Spacing;
                ccb.ShowBorder = ShowBorder;
                ccb.BorderSize = BorderSize;
                ccb.UseBorderThemeColor = UseBorderThemeColor;
                if (!UseBorderThemeColor)
                {
                    ccb.BorderColor = BorderColor;
                }
                ccb.AutoResizeWidth = AutoResizeWidth;
                ccb.SelectedColor = SelectedColor;
                if (!AutoResizeWidth)
                {
                    ccb.Size = new Size(Width, Height);
                }
                else
                {
                    ccb.Height = Height;
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentColorComboBox ccb)
            {
                Enabled = ccb.Enabled;
                SelectedColor = ccb.SelectedColor;
                BlockShape = ccb.BlockShape;
                BlockSize = ccb.BlockSize;
                ShowColorText = ccb.ShowColorText;
                Spacing = ccb.Spacing;
                ShowBorder = ccb.ShowBorder;
                BorderSize = ccb.BorderSize;
                BorderColor = ccb.BorderColor;
                UseBorderThemeColor = ccb.UseBorderThemeColor;
                AutoResizeWidth = ccb.AutoResizeWidth;
                Width = ccb.Width;
                Height = ccb.Height;
            }
        }

        public static ColorComboBoxDemoConfig FromControl(FluentColorComboBox ccb)
        {
            var cfg = new ColorComboBoxDemoConfig();
            cfg.ReadFrom(ccb);
            return cfg;
        }
    }

    public class ComboBoxDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 选择行为
        [PropertyCategory("选择行为")]
        [PropertyDisplayName("选择模式")]
        [PropertyDescription("Single=单选 | Multiple=多选")]
        public ComboBoxSelectionStyle SelectionStyle { get; set; } = ComboBoxSelectionStyle.Single;

        [PropertyCategory("选择行为")]
        [PropertyDisplayName("仅允许选择")]
        [PropertyDescription("为true时用户不能输入, 只能从下拉列表选择")]
        public bool OnlySelection { get; set; } = false;

        [PropertyCategory("选择行为")]
        [PropertyDisplayName("支持搜索")]
        [PropertyDescription("是否允许输入关键字筛选下拉项")]
        public bool Searchable { get; set; } = false;

        [PropertyCategory("选择行为")]
        [PropertyDisplayName("多选分隔符")]
        [PropertyDescription("多选模式下各选中项之间的分隔字符")]
        public string Separator { get; set; } = ", ";

        // 下拉面板
        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("下拉项数量")]
        [PropertyDescription("下拉列表一次可见的最大项数")]
        public int DropDownItemCount { get; set; } = 8;

        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("项高度")]
        [PropertyDescription("下拉列表中每一项的像素高度")]
        public int ItemHeight { get; set; } = 32;

        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("下拉列表宽度")]
        [PropertyDescription("0 表示与控件同宽")]
        public int DropDownWidth { get; set; } = 0;

        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("下拉动画时长")]
        [PropertyDescription("下拉列表展开/收起的动画时长(毫秒)")]
        public int DropDownAnimationDuration { get; set; } = 100;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 200;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentComboBox cb)
            {
                cb.Enabled = Enabled;
                cb.SelectionStyle = SelectionStyle;
                cb.OnlySelection = OnlySelection;
                cb.Searchable = Searchable;
                cb.Separator = Separator;
                cb.DropDownItemCount = DropDownItemCount;
                cb.ItemHeight = ItemHeight;
                cb.DropDownWidth = DropDownWidth;
                cb.DropDownAnimationDuration = DropDownAnimationDuration;
                cb.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentComboBox cb)
            {
                Enabled = cb.Enabled;
                SelectionStyle = cb.SelectionStyle;
                OnlySelection = cb.OnlySelection;
                Searchable = cb.Searchable;
                Separator = cb.Separator;
                DropDownItemCount = cb.DropDownItemCount;
                ItemHeight = cb.ItemHeight;
                DropDownWidth = cb.DropDownWidth;
                DropDownAnimationDuration = cb.DropDownAnimationDuration;
                Width = cb.Width;
                Height = cb.Height;
            }
        }

        public static ComboBoxDemoConfig FromControl(FluentComboBox cb)
        {
            var cfg = new ComboBoxDemoConfig();
            cfg.ReadFrom(cb);
            return cfg;
        }
    }

    public class DateTimePickerDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("当前值")]
        [PropertyDescription("当前选中的日期时间")]
        public DateTime Value { get; set; } = DateTime.Now;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否只读")]
        public bool ReadOnly { get; set; } = false;

        // 模式
        [PropertyCategory("模式")]
        [PropertyDisplayName("选择器模式")]
        [PropertyDescription("Date=仅日期 | Time=仅时间 | DateTime=日期+时间")]
        public DateTimePickerMode Mode { get; set; } = DateTimePickerMode.DateTime;

        [PropertyCategory("模式")]
        [PropertyDisplayName("输入模式")]
        [PropertyDescription("Both=手动+下拉 | TextOnly=仅手动 | DropDownOnly=仅下拉")]
        public DateTimeInputMode InputMode { get; set; } = DateTimeInputMode.Both;

        [PropertyCategory("模式")]
        [PropertyDisplayName("包含毫秒")]
        [PropertyDescription("时间模式下是否显示和编辑毫秒")]
        public bool IncludeMilliseconds { get; set; } = false;

        [PropertyCategory("模式")]
        [PropertyDisplayName("自定义格式")]
        [PropertyDescription("自定义日期时间格式字符串, 如 yyyy-MM-dd HH:mm")]
        public string CustomFormat { get; set; } = "";

        // 倒计时
        [PropertyCategory("倒计时")]
        [PropertyDisplayName("倒计时模式")]
        [PropertyDescription("启用后控件变为倒计时器")]
        public bool CountdownMode { get; set; } = false;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("显示下拉按钮")]
        public bool ShowDropDownButton { get; set; } = true;

        [PropertyCategory("外观")]
        [PropertyDisplayName("下拉按钮颜色")]
        public Color DropDownButtonColor { get; set; } = Color.Black;

        [PropertyCategory("外观")]
        [PropertyDisplayName("下拉按钮悬停色")]
        public Color DropDownButtonHoverColor { get; set; } = Color.Empty;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 220;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentDateTimePicker dtp)
            {
                dtp.ReadOnly = ReadOnly;
                dtp.Mode = Mode;
                dtp.InputMode = InputMode;
                dtp.IncludeMilliseconds = IncludeMilliseconds;
                dtp.CustomFormat = CustomFormat;
                dtp.CountdownMode = CountdownMode;
                dtp.ShowDropDownButton = ShowDropDownButton;
                dtp.Size = new Size(Width, Height);

                if (!CountdownMode)
                {
                    try { dtp.Value = Value; } catch { }
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentDateTimePicker dtp)
            {
                Value = dtp.Value;
                ReadOnly = dtp.ReadOnly;
                Mode = dtp.Mode;
                InputMode = dtp.InputMode;
                IncludeMilliseconds = dtp.IncludeMilliseconds;
                CustomFormat = dtp.CustomFormat;
                CountdownMode = dtp.CountdownMode;
                ShowDropDownButton = dtp.ShowDropDownButton;
                Width = dtp.Width;
                Height = dtp.Height;
            }
        }

        public static DateTimePickerDemoConfig FromControl(FluentDateTimePicker dtp)
        {
            var cfg = new DateTimePickerDemoConfig();
            cfg.ReadFrom(dtp);
            return cfg;
        }
    }

    public class DropdownDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("显示文本")]
        [PropertyDescription("按钮上显示的文字")]
        public string DisplayText { get; set; } = "请选择";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 选择行为
        [PropertyCategory("选择行为")]
        [PropertyDisplayName("选择模式")]
        [PropertyDescription("Single=单选 | Multiple=多选")]
        public SelectionMode SelectionMode { get; set; } = SelectionMode.Single;

        [PropertyCategory("选择行为")]
        [PropertyDisplayName("多选文本格式")]
        [PropertyDescription("多选模式下按钮上显示的文本格式, {0}为数量")]
        public string MultiSelectTextFormat { get; set; } = "(已选{0}项)";

        // 外观样式
        [PropertyCategory("外观样式")]
        [PropertyDisplayName("圆角半径")]
        public int CornerRadius { get; set; } = 4;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("显示图标")]
        public bool ShowIcon { get; set; } = true;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("显示文本")]
        public bool ShowText { get; set; } = true;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("图标位置")]
        [PropertyDescription("图标相对于文本的位置")]
        public DropdownIconPosition IconPosition { get; set; } = DropdownIconPosition.Left;

        // 边框颜色
        [PropertyCategory("边框颜色")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.FromArgb(200, 200, 200);

        [PropertyCategory("边框颜色")]
        [PropertyDisplayName("悬停边框色")]
        public Color BorderHoverColor { get; set; } = Color.FromArgb(100, 100, 100);

        [PropertyCategory("边框颜色")]
        [PropertyDisplayName("焦点边框色")]
        public Color BorderFocusColor { get; set; } = Color.FromArgb(0, 120, 212);

        // 下拉面板
        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("项高度")]
        public int ItemHeight { get; set; } = 36;

        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("项间距")]
        public int ItemSpacing { get; set; } = 2;

        [PropertyCategory("下拉面板")]
        [PropertyDisplayName("最大高度")]
        [PropertyDescription("下拉列表的最大像素高度")]
        public int DropdownMaxHeight { get; set; } = 300;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 200;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 36;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentDropdown dd)
            {
                dd.Enabled = Enabled;
                dd.DisplayText = DisplayText;
                dd.SelectionMode = SelectionMode;
                dd.MultiSelectTextFormat = MultiSelectTextFormat;
                dd.CornerRadius = CornerRadius;
                dd.ShowIcon = ShowIcon;
                dd.ShowText = ShowText;
                dd.IconPosition = IconPosition;
                dd.BorderColor = BorderColor;
                dd.BorderHoverColor = BorderHoverColor;
                dd.BorderFocusColor = BorderFocusColor;
                dd.ItemHeight = ItemHeight;
                dd.ItemSpacing = ItemSpacing;
                dd.DropdownMaxHeight = DropdownMaxHeight;
                dd.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentDropdown dd)
            {
                Enabled = dd.Enabled;
                DisplayText = dd.DisplayText;
                SelectionMode = dd.SelectionMode;
                MultiSelectTextFormat = dd.MultiSelectTextFormat;
                CornerRadius = dd.CornerRadius;
                ShowIcon = dd.ShowIcon;
                ShowText = dd.ShowText;
                IconPosition = dd.IconPosition;
                BorderColor = dd.BorderColor;
                BorderHoverColor = dd.BorderHoverColor;
                BorderFocusColor = dd.BorderFocusColor;
                ItemHeight = dd.ItemHeight;
                ItemSpacing = dd.ItemSpacing;
                DropdownMaxHeight = dd.DropdownMaxHeight;
                Width = dd.Width;
                Height = dd.Height;
            }
        }

        public static DropdownDemoConfig FromControl(FluentDropdown dd)
        {
            var cfg = new DropdownDemoConfig();
            cfg.ReadFrom(dd);
            return cfg;
        }
    }

    public class SplitButtonDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("文本")]
        [PropertyDescription("按钮上显示的文字")]
        public string Text { get; set; } = "分割按钮";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("图标")]
        [PropertyDescription("按钮上显示的图像")]
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image Image { get; set; }

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 布局方向
        [PropertyCategory("布局方向")]
        [PropertyDisplayName("排列方向")]
        [PropertyDescription("按钮主体内容的排列方向")]
        public FluentSplitButtonOrientation Orientation { get; set; } = FluentSplitButtonOrientation.Horizontal;

        // 显示元素
        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示图标")]
        public bool ShowIcon { get; set; } = true;

        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示文本")]
        public bool ShowText { get; set; } = true;

        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示下拉边框")]
        [PropertyDescription("下拉面板是否显示边框")]
        public bool ShowDropDownBorder { get; set; } = true;

        // 外观样式
        [PropertyCategory("外观样式")]
        [PropertyDisplayName("图标大小")]
        public Size IconSize { get; set; } = new Size(16, 16);

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("图文间距")]
        [PropertyDescription("图标和文本之间的像素间距")]
        public int ItemSpacing { get; set; } = 4;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("下拉区域大小")]
        [PropertyDescription("下拉箭头区域的宽度(横向)或高度(纵向)")]
        public int DropDownSize { get; set; } = 20;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("内容位置")]
        [PropertyDescription("图标和文本在内容区域中的对齐方式")]
        public FluentSplitButtonItemPosition ItemPosition { get; set; } = FluentSplitButtonItemPosition.Center;

        [PropertyCategory("外观样式")]
        [PropertyDisplayName("内边距")]
        [PropertyDescription("内容区域的内边距")]
        public int ContentPadding { get; set; } = 4;

        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("启用按压状态")]
        [PropertyDescription("点击后是否保持按压状态")]
        public bool EnablePressedState { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("自动调整大小")]
        [PropertyDescription("是否根据内容自动调整控件大小")]
        public bool AutoSizeToContent { get; set; } = false;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 150;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 36;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentSplitButton sb)
            {
                sb.Enabled = Enabled;
                sb.Text = Text;
                sb.Image = Image;
                sb.Orientation = Orientation;
                sb.ShowIcon = ShowIcon;
                sb.ShowText = ShowText;
                sb.ShowDropDownBorder = ShowDropDownBorder;
                sb.IconSize = IconSize;
                sb.ItemSpacing = ItemSpacing;
                sb.DropDownSize = DropDownSize;
                sb.ItemPosition = ItemPosition;
                sb.ContentPadding = ContentPadding;
                sb.EnablePressedState = EnablePressedState;
                sb.AutoSizeToContent = AutoSizeToContent;
                if (!AutoSizeToContent)
                {
                    sb.Size = new Size(Width, Height);
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentSplitButton sb)
            {
                Enabled = sb.Enabled;
                Text = sb.Text;
                Image = sb.Image;
                Orientation = sb.Orientation;
                ShowIcon = sb.ShowIcon;
                ShowText = sb.ShowText;
                ShowDropDownBorder = sb.ShowDropDownBorder;
                IconSize = sb.IconSize;
                ItemSpacing = sb.ItemSpacing;
                DropDownSize = sb.DropDownSize;
                ItemPosition = sb.ItemPosition;
                ContentPadding = sb.ContentPadding;
                EnablePressedState = sb.EnablePressedState;
                AutoSizeToContent = sb.AutoSizeToContent;
                Width = sb.Width;
                Height = sb.Height;
            }
        }

        public static SplitButtonDemoConfig FromControl(FluentSplitButton sb)
        {
            var cfg = new SplitButtonDemoConfig();
            cfg.ReadFrom(sb);
            return cfg;
        }
    }

    public class SliderDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("当前值")]
        [PropertyDescription("滑块的当前值")]
        public double Value { get; set; } = 50;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("最小值")]
        public double Minimum { get; set; } = 0;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("最大值")]
        public double Maximum { get; set; } = 100;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("步长")]
        [PropertyDescription("离散模式下每次调节的步进值")]
        public double SmallChange { get; set; } = 10;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 模式
        [PropertyCategory("模式")]
        [PropertyDisplayName("方向")]
        [PropertyDescription("Horizontal=水平 | Vertical=垂直")]
        public SliderOrientation Orientation { get; set; } = SliderOrientation.Horizontal;

        [PropertyCategory("模式")]
        [PropertyDisplayName("值类型")]
        [PropertyDescription("Continuous=连续 | Discrete=离散(步进)")]
        public SliderValueType ValueType { get; set; } = SliderValueType.Continuous;

        [PropertyCategory("模式")]
        [PropertyDisplayName("范围选择模式")]
        [PropertyDescription("启用后可选择一个范围区间")]
        public bool IsRangeMode { get; set; } = false;

        [PropertyCategory("模式")]
        [PropertyDisplayName("范围起始值")]
        public double RangeStart { get; set; } = 0;

        [PropertyCategory("模式")]
        [PropertyDisplayName("范围结束值")]
        public double RangeEnd { get; set; } = 50;

        // 滑块外观
        [PropertyCategory("滑块外观")]
        [PropertyDisplayName("滑块形状")]
        [PropertyDescription("Circle=圆形 | Square=方形 | Diamond=菱形 | Triangle=三角")]
        public SliderShape ThumbShape { get; set; } = SliderShape.Circle;

        [PropertyCategory("滑块外观")]
        [PropertyDisplayName("滑块大小")]
        [PropertyDescription("滑块的像素大小")]
        public int ThumbSize { get; set; } = 20;

        [PropertyCategory("滑块外观")]
        [PropertyDisplayName("显示滑块边框")]
        public bool ShowThumbBorder { get; set; } = false;

        [PropertyCategory("滑块外观")]
        [PropertyDisplayName("轨道高度")]
        [PropertyDescription("轨道条的粗细")]
        public int TrackHeight { get; set; } = 6;

        [PropertyCategory("滑块外观")]
        [PropertyDisplayName("颜色饱和度增强")]
        [PropertyDescription("0~1之间, 增强滑块颜色饱和度")]
        public float ColorSaturationBoost { get; set; } = 0.2f;

        // 刻度与提示
        [PropertyCategory("刻度与提示")]
        [PropertyDisplayName("显示刻度标记")]
        public bool ShowTickMarks { get; set; } = false;

        [PropertyCategory("刻度与提示")]
        [PropertyDisplayName("显示刻度文本")]
        public bool ShowTickText { get; set; } = false;

        [PropertyCategory("刻度与提示")]
        [PropertyDisplayName("显示值提示")]
        [PropertyDescription("鼠标悬停时是否显示当前值的Tooltip")]
        public bool ShowValueTooltip { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 260;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 40;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentSlider sl)
            {
                sl.Enabled = Enabled;
                sl.Minimum = Minimum;
                sl.Maximum = Maximum;
                sl.SmallChange = SmallChange;
                sl.Orientation = Orientation;
                sl.ValueType = ValueType;
                sl.IsRangeMode = IsRangeMode;
                sl.ThumbShape = ThumbShape;
                sl.ThumbSize = ThumbSize;
                sl.TrackHeight = TrackHeight;
                sl.ShowTickMarks = ShowTickMarks;
                sl.ShowTickText = ShowTickText;
                sl.ShowValueTooltip = ShowValueTooltip;
                sl.Size = new Size(Width, Height);
                sl.Value = Value;
                if (IsRangeMode)
                {
                    sl.RangeStart = RangeStart;
                    sl.RangeEnd = RangeEnd;
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentSlider sl)
            {
                Enabled = sl.Enabled;
                Value = sl.Value;
                Minimum = sl.Minimum;
                Maximum = sl.Maximum;
                SmallChange = sl.SmallChange;
                Orientation = sl.Orientation;
                ValueType = sl.ValueType;
                IsRangeMode = sl.IsRangeMode;
                RangeStart = sl.RangeStart;
                RangeEnd = sl.RangeEnd;
                ThumbShape = sl.ThumbShape;
                ThumbSize = sl.ThumbSize;
                TrackHeight = sl.TrackHeight;
                ShowTickMarks = sl.ShowTickMarks;
                ShowTickText = sl.ShowTickText;
                ShowValueTooltip = sl.ShowValueTooltip;
                Width = sl.Width;
                Height = sl.Height;
            }
        }

        public static SliderDemoConfig FromControl(FluentSlider sl)
        {
            var cfg = new SliderDemoConfig();
            cfg.ReadFrom(sl);
            return cfg;
        }
    }

    public class ProgressDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("当前进度")]
        [PropertyDescription("当前进度值")]
        public double Progress { get; set; } = 65;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("最小值")]
        public double Minimum { get; set; } = 0;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("最大值")]
        public double Maximum { get; set; } = 100;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 模式与样式
        [PropertyCategory("模式与样式")]
        [PropertyDisplayName("进度模式")]
        [PropertyDescription("Determinate=确定进度 | Indeterminate=不确定(循环)")]
        public ProgressMode Mode { get; set; } = ProgressMode.Determinate;

        [PropertyCategory("模式与样式")]
        [PropertyDisplayName("进度条样式")]
        [PropertyDescription("Linear=线性 | Segmented=分段 | Circular=圆形")]
        public ProgressStyle Style { get; set; } = ProgressStyle.Linear;

        [PropertyCategory("模式与样式")]
        [PropertyDisplayName("使用渐变")]
        [PropertyDescription("进度条是否使用渐变色效果")]
        public bool UseGradient { get; set; } = true;

        // 文本显示
        [PropertyCategory("文本显示")]
        [PropertyDisplayName("显示进度文本")]
        public bool ShowProgressText { get; set; } = true;

        [PropertyCategory("文本显示")]
        [PropertyDisplayName("显示百分比")]
        public bool ShowPercentage { get; set; } = true;

        [PropertyCategory("文本显示")]
        [PropertyDisplayName("自定义文本")]
        [PropertyDescription("自定义显示文本, 为空时显示默认百分比")]
        public string CustomText { get; set; } = "";

        [PropertyCategory("文本显示")]
        [PropertyDisplayName("前缀文本")]
        public string PrefixText { get; set; } = "";

        [PropertyCategory("文本显示")]
        [PropertyDisplayName("后缀文本")]
        public string SuffixText { get; set; } = "";

        [PropertyCategory("文本显示")]
        [PropertyDisplayName("文本位置")]
        [PropertyDescription("进度文本在进度条中的显示位置")]
        public ProgressTextPosition TextPosition { get; set; } = ProgressTextPosition.Center;

        [PropertyCategory("文本显示")]
        [PropertyDisplayName("显示任务信息")]
        public bool ShowTaskInfo { get; set; } = false;

        // 颜色
        [PropertyCategory("颜色")]
        [PropertyDisplayName("进度条颜色")]
        [PropertyDescription("空值表示使用主题色")]
        public Color ProgressBarColor { get; set; } = Color.Empty;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("进度条背景色")]
        [PropertyDescription("空值表示使用主题色")]
        public Color ProgressBackColor { get; set; } = Color.Empty;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("文本颜色")]
        [PropertyDescription("空值表示自动")]
        public Color ProgressTextColor { get; set; } = Color.Empty;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = false;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Empty;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        // 分段样式
        [PropertyCategory("分段样式")]
        [PropertyDisplayName("分段数量")]
        [PropertyDescription("Segmented样式下的分段数")]
        public int SegmentCount { get; set; } = 10;

        [PropertyCategory("分段样式")]
        [PropertyDisplayName("分段间距")]
        public int SegmentSpacing { get; set; } = 2;

        // 圆形样式
        [PropertyCategory("圆形样式")]
        [PropertyDisplayName("圆环厚度")]
        [PropertyDescription("Circular样式下圆环线条的粗细")]
        public int CircularThickness { get; set; } = 8;

        [PropertyCategory("圆形样式")]
        [PropertyDisplayName("起始角度")]
        [PropertyDescription("圆形进度条的起始角度(-90为顶部)")]
        public int CircularStartAngle { get; set; } = -90;

        // 动画
        [PropertyCategory("动画")]
        [PropertyDisplayName("循环动画速度")]
        [PropertyDescription("Indeterminate模式下动画移动速度(像素/秒)")]
        public int IndeterminateSpeed { get; set; } = 20;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 260;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 24;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentProgress pg)
            {
                pg.Enabled = Enabled;
                pg.Minimum = Minimum;
                pg.Maximum = Maximum;
                pg.Mode = Mode;
                pg.Style = Style;
                pg.UseGradient = UseGradient;
                pg.ShowProgressText = ShowProgressText;
                pg.ShowPercentage = ShowPercentage;
                pg.CustomText = CustomText;
                pg.PrefixText = PrefixText;
                pg.SuffixText = SuffixText;
                pg.TextPosition = TextPosition;
                pg.ShowTaskInfo = ShowTaskInfo;
                if (ProgressBarColor != Color.Empty)
                {
                    pg.ProgressBarColor = ProgressBarColor;
                }

                if (ProgressBackColor != Color.Empty)
                {
                    pg.ProgressBackColor = ProgressBackColor;
                }

                if (ProgressTextColor != Color.Empty)
                {
                    pg.ProgressTextColor = ProgressTextColor;
                }

                pg.ShowBorder = ShowBorder;
                if (BorderColor != Color.Empty)
                {
                    pg.BorderColor = BorderColor;
                }

                pg.BorderWidth = BorderWidth;
                pg.SegmentCount = SegmentCount;
                pg.SegmentSpacing = SegmentSpacing;
                pg.CircularThickness = CircularThickness;
                pg.CircularStartAngle = CircularStartAngle;
                pg.IndeterminateSpeed = IndeterminateSpeed;
                pg.Size = new Size(Width, Height);
                pg.Progress = Progress;
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentProgress pg)
            {
                Enabled = pg.Enabled;
                Progress = pg.Progress;
                Minimum = pg.Minimum;
                Maximum = pg.Maximum;
                Mode = pg.Mode;
                Style = pg.Style;
                UseGradient = pg.UseGradient;
                ShowProgressText = pg.ShowProgressText;
                ShowPercentage = pg.ShowPercentage;
                CustomText = pg.CustomText;
                PrefixText = pg.PrefixText;
                SuffixText = pg.SuffixText;
                TextPosition = pg.TextPosition;
                ShowTaskInfo = pg.ShowTaskInfo;
                ProgressBarColor = pg.ProgressBarColor;
                ProgressBackColor = pg.ProgressBackColor;
                ProgressTextColor = pg.ProgressTextColor;
                ShowBorder = pg.ShowBorder;
                BorderColor = pg.BorderColor;
                BorderWidth = pg.BorderWidth;
                SegmentCount = pg.SegmentCount;
                SegmentSpacing = pg.SegmentSpacing;
                CircularThickness = pg.CircularThickness;
                CircularStartAngle = pg.CircularStartAngle;
                IndeterminateSpeed = pg.IndeterminateSpeed;
                Width = pg.Width;
                Height = pg.Height;
            }
        }

        public static ProgressDemoConfig FromControl(FluentProgress pg)
        {
            var cfg = new ProgressDemoConfig();
            cfg.ReadFrom(pg);
            return cfg;
        }
    }

    public class ShapeDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("形状类型")]
        [PropertyDescription("Rectangle=矩形 | RoundedRectangle=圆角矩形 | Ellipse=椭圆 | Circle=圆 | Triangle=三角 | Star=星形 | Polygon=多边形 | Arrow=箭头 | Line=线段 | Cylinder=圆柱 | Sphere=球体 | Cuboid=立方体")]
        public ShapeType ShapeType { get; set; } = ShapeType.Rectangle;

        // 填充
        [PropertyCategory("填充")]
        [PropertyDisplayName("填充颜色")]
        public Color FillColor { get; set; } = Color.FromArgb(0, 120, 212);

        [PropertyCategory("填充")]
        [PropertyDisplayName("不透明度")]
        [PropertyDescription("0~1之间, 控制形状的透明度")]
        public float Opacity { get; set; } = 1.0f;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.FromArgb(0, 90, 158);

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框样式")]
        [PropertyDescription("Solid=实线 | Dash=虚线 | Dot=点线 | DashDot=点划线")]
        public DashStyle BorderStyle { get; set; } = DashStyle.Solid;

        // 渐变
        [PropertyCategory("渐变")]
        [PropertyDisplayName("使用渐变")]
        public bool UseGradient { get; set; } = false;

        [PropertyCategory("渐变")]
        [PropertyDisplayName("渐变起始色")]
        public Color GradientStartColor { get; set; } = Color.White;

        [PropertyCategory("渐变")]
        [PropertyDisplayName("渐变结束色")]
        public Color GradientEndColor { get; set; } = Color.FromArgb(0, 120, 212);

        [PropertyCategory("渐变")]
        [PropertyDisplayName("渐变方向")]
        public LinearGradientMode GradientMode { get; set; } = LinearGradientMode.Vertical;

        // 圆角矩形
        [PropertyCategory("圆角矩形")]
        [PropertyDisplayName("圆角半径")]
        public int CornerRadius { get; set; } = 8;

        // 多边形
        [PropertyCategory("多边形")]
        [PropertyDisplayName("边数")]
        public int PolygonSides { get; set; } = 6;

        [PropertyCategory("多边形")]
        [PropertyDisplayName("起始角度")]
        public float PolygonStartAngle { get; set; } = -90f;

        // 星形
        [PropertyCategory("星形")]
        [PropertyDisplayName("角数")]
        public int StarPoints { get; set; } = 5;

        [PropertyCategory("星形")]
        [PropertyDisplayName("内半径比例")]
        [PropertyDescription("内半径与外半径的比值(0~1)")]
        public float StarInnerRadiusRatio { get; set; } = 0.4f;

        [PropertyCategory("星形")]
        [PropertyDisplayName("起始角度")]
        public float StarStartAngle { get; set; } = -18f;

        // 线段
        [PropertyCategory("线段")]
        [PropertyDisplayName("线段模式")]
        [PropertyDescription("Horizontal=水平 | Vertical=垂直 | DiagonalDown=左上到右下 | DiagonalUp=右上到左下")]
        public LineMode LineMode { get; set; } = LineMode.DiagonalDown;

        [PropertyCategory("线段")]
        [PropertyDisplayName("线段边距")]
        public int LineMargin { get; set; } = 5;

        // 箭头
        [PropertyCategory("箭头")]
        [PropertyDisplayName("箭头方向")]
        public ArrowDirection ArrowDirection { get; set; } = ArrowDirection.Right;

        [PropertyCategory("箭头")]
        [PropertyDisplayName("头部宽度比例")]
        public float ArrowHeadWidthRatio { get; set; } = 0.4f;

        [PropertyCategory("箭头")]
        [PropertyDisplayName("尾部宽度比例")]
        public float ArrowTailWidthRatio { get; set; } = 0.6f;

        // 梯形
        [PropertyCategory("梯形")]
        [PropertyDisplayName("顶边宽度比例")]
        public float TrapezoidTopWidthRatio { get; set; } = 0.6f;

        // 阴影
        [PropertyCategory("阴影")]
        [PropertyDisplayName("显示阴影")]
        public bool ShowShapeShadow { get; set; } = false;

        [PropertyCategory("阴影")]
        [PropertyDisplayName("阴影颜色")]
        public Color ShadowColor { get; set; } = Color.FromArgb(80, 0, 0, 0);

        [PropertyCategory("阴影")]
        [PropertyDisplayName("阴影X偏移")]
        public int ShadowOffsetX { get; set; } = 3;

        [PropertyCategory("阴影")]
        [PropertyDisplayName("阴影Y偏移")]
        public int ShadowOffsetY { get; set; } = 3;

        // 3D
        [PropertyCategory("3D")]
        [PropertyDisplayName("水平视角")]
        public float HorizontalViewAngle { get; set; } = 30f;

        [PropertyCategory("3D")]
        [PropertyDisplayName("垂直视角")]
        public float VerticalViewAngle { get; set; } = 30f;

        [PropertyCategory("3D")]
        [PropertyDisplayName("深度比例")]
        public float DepthRatio { get; set; } = 0.3f;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 120;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 120;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentShape sh)
            {
                sh.ShapeType = ShapeType;
                sh.FillColor = FillColor;
                sh.Opacity = Opacity;
                sh.BorderColor = BorderColor;
                sh.BorderWidth = BorderWidth;
                sh.BorderStyle = BorderStyle;
                sh.UseGradient = UseGradient;
                sh.GradientStartColor = GradientStartColor;
                sh.GradientEndColor = GradientEndColor;
                sh.GradientMode = GradientMode;
                sh.CornerRadius = CornerRadius;
                sh.PolygonSides = PolygonSides;
                sh.PolygonStartAngle = PolygonStartAngle;
                sh.StarPoints = StarPoints;
                sh.StarInnerRadiusRatio = StarInnerRadiusRatio;
                sh.StarStartAngle = StarStartAngle;
                sh.LineMode = LineMode;
                sh.LineMargin = LineMargin;
                sh.ArrowDirection = ArrowDirection;
                sh.ArrowHeadWidthRatio = ArrowHeadWidthRatio;
                sh.ArrowTailWidthRatio = ArrowTailWidthRatio;
                sh.TrapezoidTopWidthRatio = TrapezoidTopWidthRatio;
                sh.ShowShapeShadow = ShowShapeShadow;
                sh.ShadowColor = ShadowColor;
                sh.ShadowOffsetX = ShadowOffsetX;
                sh.ShadowOffsetY = ShadowOffsetY;
                sh.HorizontalViewAngle = HorizontalViewAngle;
                sh.VerticalViewAngle = VerticalViewAngle;
                sh.DepthRatio = DepthRatio;
                sh.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentShape sh)
            {
                ShapeType = sh.ShapeType;
                FillColor = sh.FillColor;
                Opacity = sh.Opacity;
                BorderColor = sh.BorderColor;
                BorderWidth = sh.BorderWidth;
                BorderStyle = sh.BorderStyle;
                UseGradient = sh.UseGradient;
                GradientStartColor = sh.GradientStartColor;
                GradientEndColor = sh.GradientEndColor;
                GradientMode = sh.GradientMode;
                CornerRadius = sh.CornerRadius;
                PolygonSides = sh.PolygonSides;
                PolygonStartAngle = sh.PolygonStartAngle;
                StarPoints = sh.StarPoints;
                StarInnerRadiusRatio = sh.StarInnerRadiusRatio;
                StarStartAngle = sh.StarStartAngle;
                LineMode = sh.LineMode;
                LineMargin = sh.LineMargin;
                ArrowDirection = sh.ArrowDirection;
                ArrowHeadWidthRatio = sh.ArrowHeadWidthRatio;
                ArrowTailWidthRatio = sh.ArrowTailWidthRatio;
                TrapezoidTopWidthRatio = sh.TrapezoidTopWidthRatio;
                ShowShapeShadow = sh.ShowShapeShadow;
                ShadowColor = sh.ShadowColor;
                ShadowOffsetX = sh.ShadowOffsetX;
                ShadowOffsetY = sh.ShadowOffsetY;
                HorizontalViewAngle = sh.HorizontalViewAngle;
                VerticalViewAngle = sh.VerticalViewAngle;
                DepthRatio = sh.DepthRatio;
                Width = sh.Width;
                Height = sh.Height;
            }
        }

        public static ShapeDemoConfig FromControl(FluentShape sh)
        {
            var cfg = new ShapeDemoConfig();
            cfg.ReadFrom(sh);
            return cfg;
        }
    }

    public class MessageDemoConfig : ControlDemoConfigBase
    {
        // 内容
        [PropertyCategory("内容")]
        [PropertyDisplayName("标题")]
        [PropertyDescription("消息顶部的加粗标题文本")]
        public string Title { get; set; } = "操作成功";

        [PropertyCategory("内容")]
        [PropertyDisplayName("内容")]
        [PropertyDescription("消息主体文本")]
        public string Content { get; set; } = "数据已保存到数据库。";

        // 类型与样式
        [PropertyCategory("类型与样式")]
        [PropertyDisplayName("消息类型")]
        [PropertyDescription("Info=信息 | Success=成功 | Warning=警告 | Error=错误")]
        public MessageType MessageType { get; set; } = MessageType.Success;

        [PropertyCategory("类型与样式")]
        [PropertyDisplayName("显示图标")]
        [PropertyDescription("是否在消息左侧显示类型图标")]
        public bool ShowIcon { get; set; } = true;

        [PropertyCategory("类型与样式")]
        [PropertyDisplayName("可关闭")]
        [PropertyDescription("是否显示右上角关闭按钮")]
        public bool Closable { get; set; } = true;

        [PropertyCategory("类型与样式")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = false;

        [PropertyCategory("类型与样式")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        [PropertyCategory("类型与样式")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.DimGray;

        // 动画
        [PropertyCategory("动画")]
        [PropertyDisplayName("动画方式")]
        [PropertyDescription("None | Fade | Slide | SlideAndFade | Scale | Bounce")]
        public MessageAnimation Animation { get; set; } = MessageAnimation.Fade;

        // 自动关闭
        [PropertyCategory("自动关闭")]
        [PropertyDisplayName("持续时间 (ms)")]
        [PropertyDescription("自动关闭的毫秒数 (0=不自动关闭)")]
        public int Duration { get; set; } = 0;

        [PropertyCategory("自动关闭")]
        [PropertyDisplayName("悬停暂停")]
        [PropertyDescription("鼠标悬停时暂停自动关闭计时")]
        public bool PauseOnHover { get; set; } = true;

        // 位置
        [PropertyCategory("位置")]
        [PropertyDisplayName("显示位置")]
        [PropertyDescription("消息在容器中的位置")]
        public MessagePosition Position { get; set; } = MessagePosition.TopRight;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 320;

        /// <summary>构建 MessageOptions</summary>
        public MessageOptions ToMessageOptions()
        {
            return new MessageOptions
            {
                Title = Title,
                Content = string.IsNullOrWhiteSpace(Content) ? " " : Content,
                Type = MessageType,
                ShowIcon = ShowIcon,
                Closable = Closable,
                Animation = Animation,
                Duration = Duration,
                PauseOnHover = PauseOnHover,
                Position = Position,
                Width = Width
            };
        }

        public FluentMessage CreateMessage()
        {
            var opts = ToMessageOptions();
            //var msg = new FluentMessage(opts);
            var msg = FluentMessageManager.Instance.Show(opts);

            msg.ShowBorder = ShowBorder;
            msg.BorderWidth = BorderWidth;
            msg.BorderColor = BorderColor;
            return msg;
        }

        public override void ApplyTo(Control ctrl)
        {
            if (ctrl is FluentMessage msg)
            {
                msg.ShowBorder = ShowBorder;
                msg.BorderWidth = BorderWidth;
                msg.BorderColor = BorderColor;
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentMessage msg)
            {
                var opts = msg.Options;
                Title = opts.Title;
                Content = opts.Content;
                MessageType = opts.Type;
                ShowIcon = opts.ShowIcon;
                Closable = opts.Closable;
                Animation = opts.Animation;
                Duration = opts.Duration;
                PauseOnHover = opts.PauseOnHover;
                Position = opts.Position;
                Width = opts.Width;
                ShowBorder = msg.ShowBorder;
                BorderWidth = msg.BorderWidth;
                BorderColor = msg.BorderColor;
            }
        }

        public static MessageDemoConfig FromControl(FluentMessage msg)
        {
            var cfg = new MessageDemoConfig();
            cfg.ReadFrom(msg);
            return cfg;
        }
    }

    #endregion

    #region 列表控件配置

    public class CheckBoxListDemoConfig : ControlDemoConfigBase
    {
        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("排列方向")]
        [PropertyDescription("Vertical=垂直排列 | Horizontal=水平排列")]
        public ListOrientation Orientation { get; set; } = ListOrientation.Vertical;

        [PropertyCategory("布局")]
        [PropertyDisplayName("项目间距")]
        [PropertyDescription("各复选框之间的像素间距")]
        public int ItemSpacing { get; set; } = 8;

        [PropertyCategory("布局")]
        [PropertyDisplayName("自动项宽")]
        [PropertyDescription("根据文本内容自动计算每项宽度")]
        public bool AutoSizeItems { get; set; } = true;

        [PropertyCategory("布局")]
        [PropertyDisplayName("固定项尺寸")]
        [PropertyDescription("AutoSizeItems=false 时使用的固定尺寸")]
        public Size FixedItemSize { get; set; } = new Size(120, 32);

        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("最大可见项数")]
        [PropertyDescription("0=不限制, 超出时显示滚动条")]
        public int MaxVisibleItems { get; set; } = 0;

        [PropertyCategory("行为")]
        [PropertyDisplayName("自动滚动")]
        [PropertyDescription("内容超出时是否自动显示滚动条")]
        public bool AutoScroll { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("项目动画")]
        [PropertyDescription("项目出现时是否播放动画效果")]
        public bool ShowItemAnimation { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 240;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 240;

        // 数据
        [PropertyCategory("数据")]
        [PropertyDisplayName("示例项数量")]
        [PropertyDescription("预览时生成的示例选项数量")]
        public int SampleItemCount { get; set; } = 5;

        [PropertyIgnoreEdit]
        public bool Enabled { get; set; } = true;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentCheckBoxList list)
            {
                list.Orientation = Orientation;
                list.ItemSpacing = ItemSpacing;
                list.AutoSizeItems = AutoSizeItems;
                list.FixedItemSize = FixedItemSize;
                list.MaxVisibleItems = MaxVisibleItems;
                list.AutoScroll = AutoScroll;
                list.ShowItemAnimation = ShowItemAnimation;
                list.Enabled = Enabled;
                list.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentCheckBoxList list)
            {
                Orientation = list.Orientation;
                ItemSpacing = list.ItemSpacing;
                AutoSizeItems = list.AutoSizeItems;
                FixedItemSize = list.FixedItemSize;
                MaxVisibleItems = list.MaxVisibleItems;
                AutoScroll = list.AutoScroll;
                ShowItemAnimation = list.ShowItemAnimation;
                Enabled = list.Enabled;
                Width = list.Width;
                Height = list.Height;
                SampleItemCount = list.Items?.Count ?? 5;
            }
        }

        public static CheckBoxListDemoConfig FromControl(FluentCheckBoxList list)
        {
            var cfg = new CheckBoxListDemoConfig();
            cfg.ReadFrom(list);
            return cfg;
        }
    }

    public class RadioListDemoConfig : ControlDemoConfigBase
    {
        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("排列方向")]
        [PropertyDescription("Vertical=垂直排列 | Horizontal=水平排列")]
        public ListOrientation Orientation { get; set; } = ListOrientation.Vertical;

        [PropertyCategory("布局")]
        [PropertyDisplayName("项目间距")]
        public int ItemSpacing { get; set; } = 8;

        [PropertyCategory("布局")]
        [PropertyDisplayName("自动项宽")]
        [PropertyDescription("根据文本内容自动计算每项宽度")]
        public bool AutoSizeItems { get; set; } = true;

        [PropertyCategory("布局")]
        [PropertyDisplayName("固定项尺寸")]
        [PropertyDescription("AutoSizeItems=false 时使用的固定尺寸")]
        public Size FixedItemSize { get; set; } = new Size(120, 32);

        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("组名称")]
        [PropertyDescription("同组内的单选按钮互斥")]
        public string GroupName { get; set; } = "DemoGroup";

        [PropertyCategory("行为")]
        [PropertyDisplayName("最大可见项数")]
        [PropertyDescription("0=不限制")]
        public int MaxVisibleItems { get; set; } = 0;

        [PropertyCategory("行为")]
        [PropertyDisplayName("自动滚动")]
        public bool AutoScroll { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("项目动画")]
        public bool ShowItemAnimation { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("默认选中项")]
        [PropertyDescription("初始选中项的索引 (-1=无选中)")]
        public int SelectedIndex { get; set; } = 0;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 240;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 240;

        [PropertyIgnoreEdit]
        public bool Enabled { get; set; } = true;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentRadioList list)
            {
                list.Orientation = Orientation;
                list.ItemSpacing = ItemSpacing;
                list.AutoSizeItems = AutoSizeItems;
                list.FixedItemSize = FixedItemSize;
                list.GroupName = GroupName;
                list.MaxVisibleItems = MaxVisibleItems;
                list.AutoScroll = AutoScroll;
                list.ShowItemAnimation = ShowItemAnimation;
                list.Enabled = Enabled;
                list.Size = new Size(Width, Height);

                if (SelectedIndex >= 0 && SelectedIndex < list.Items.Count)
                {
                    list.SelectedIndex = SelectedIndex;
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentRadioList list)
            {
                Orientation = list.Orientation;
                ItemSpacing = list.ItemSpacing;
                AutoSizeItems = list.AutoSizeItems;
                FixedItemSize = list.FixedItemSize;
                GroupName = list.GroupName;
                MaxVisibleItems = list.MaxVisibleItems;
                AutoScroll = list.AutoScroll;
                ShowItemAnimation = list.ShowItemAnimation;
                SelectedIndex = list.SelectedIndex;
                Enabled = list.Enabled;
                Width = list.Width;
                Height = list.Height;
            }
        }

        public static RadioListDemoConfig FromControl(FluentRadioList list)
        {
            var cfg = new RadioListDemoConfig();
            cfg.ReadFrom(list);
            return cfg;
        }
    }

    public class ListBoxDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("多选模式")]
        [PropertyDescription("是否允许选中多个列表项")]
        public bool MultiSelect { get; set; } = false;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("显示复选框")]
        [PropertyDescription("每个列表项前是否显示勾选框")]
        public bool ShowCheckBoxes { get; set; } = false;

        // 项目外观
        [PropertyCategory("项目外观")]
        [PropertyDisplayName("项高度")]
        [PropertyDescription("每个列表项的像素高度")]
        public int ItemHeight { get; set; } = 30;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("项间距")]
        public int ItemSpacing { get; set; } = 2;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("项内边距")]
        public Padding ItemPadding { get; set; } = new Padding(8, 4, 8, 4);

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("图标位置")]
        [PropertyDescription("图标相对于文本的位置")]
        public IconPosition IconPosition { get; set; } = IconPosition.Left;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("图标文本间距")]
        public int IconTextSpacing { get; set; } = 8;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("复选框边距")]
        [PropertyDescription("复选框与内容之间的间距")]
        public int CheckBoxMargin { get; set; } = 4;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("边框样式")]
        [PropertyDescription("None=无 | Always=始终显示 | FocusOnly=仅聚焦时")]
        public FluentBorderStyle BorderStyle { get; set; } = FluentBorderStyle.FocusOnly;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("聚焦边框")]
        [PropertyDescription("聚焦时是否显示高亮边框")]
        public bool ShowFocusBorder { get; set; } = true;

        // 滚动条
        [PropertyCategory("滚动条")]
        [PropertyDisplayName("滚动条模式")]
        [PropertyDescription("Auto=自动 | Always=始终 | Never=隐藏")]
        public ScrollBarMode ScrollBarMode { get; set; } = ScrollBarMode.Auto;

        // 颜色
        [PropertyCategory("颜色")]
        [PropertyDisplayName("容器背景色")]
        public Color ContainerBackColor { get; set; } = SystemColors.Window;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("项目背景色")]
        public Color ItemBackColor { get; set; } = Color.White;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("项目前景色")]
        public Color ItemForeColor { get; set; } = Color.Black;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("选中项背景色")]
        public Color SelectedBackColor { get; set; } = SystemColors.Highlight;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("选中项前景色")]
        public Color SelectedForeColor { get; set; } = Color.White;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("悬停项背景色")]
        public Color HoverBackColor { get; set; } = Color.LightGray;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 220;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 200;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentListBox lb)
            {
                lb.Enabled = Enabled;
                lb.MultiSelect = MultiSelect;
                lb.ShowCheckBoxes = ShowCheckBoxes;
                lb.ItemHeight = ItemHeight;
                lb.ItemSpacing = ItemSpacing;
                lb.ItemPadding = ItemPadding;
                lb.IconPosition = IconPosition;
                lb.IconTextSpacing = IconTextSpacing;
                lb.CheckBoxMargin = CheckBoxMargin;
                lb.BorderStyle = BorderStyle;
                lb.BorderWidth = BorderWidth;
                lb.ShowFocusBorder = ShowFocusBorder;
                lb.ScrollBarMode = ScrollBarMode;
                lb.ContainerBackColor = ContainerBackColor;
                lb.ItemBackColor = ItemBackColor;
                lb.ItemForeColor = ItemForeColor;
                lb.SelectedBackColor = SelectedBackColor;
                lb.SelectedForeColor = SelectedForeColor;
                lb.HoverBackColor = HoverBackColor;
                lb.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentListBox lb)
            {
                Enabled = lb.Enabled;
                MultiSelect = lb.MultiSelect;
                ShowCheckBoxes = lb.ShowCheckBoxes;
                ItemHeight = lb.ItemHeight;
                ItemSpacing = lb.ItemSpacing;
                ItemPadding = lb.ItemPadding;
                IconPosition = lb.IconPosition;
                IconTextSpacing = lb.IconTextSpacing;
                CheckBoxMargin = lb.CheckBoxMargin;
                BorderStyle = lb.BorderStyle;
                BorderWidth = lb.BorderWidth;
                ShowFocusBorder = lb.ShowFocusBorder;
                ScrollBarMode = lb.ScrollBarMode;
                ContainerBackColor = lb.ContainerBackColor;
                ItemBackColor = lb.ItemBackColor;
                ItemForeColor = lb.ItemForeColor;
                SelectedBackColor = lb.SelectedBackColor;
                SelectedForeColor = lb.SelectedForeColor;
                HoverBackColor = lb.HoverBackColor;
                Width = lb.Width;
                Height = lb.Height;
            }
        }

        public static ListBoxDemoConfig FromControl(FluentListBox lb)
        {
            var cfg = new ListBoxDemoConfig();
            cfg.ReadFrom(lb);
            return cfg;
        }
    }

    public class TreeViewDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 节点外观
        [PropertyCategory("节点外观")]
        [PropertyDisplayName("节点高度")]
        [PropertyDescription("每个节点的像素高度")]
        public int NodeHeight { get; set; } = 24;

        [PropertyCategory("节点外观")]
        [PropertyDisplayName("节点缩进")]
        [PropertyDescription("子节点相对父节点的缩进像素数")]
        public int NodeIndent { get; set; } = 20;

        [PropertyCategory("节点外观")]
        [PropertyDisplayName("整行选择")]
        [PropertyDescription("选中时是否高亮整行")]
        public bool FullRowSelect { get; set; } = true;

        // 显示元素
        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示连接线")]
        [PropertyDescription("是否在父子节点之间绘制连接线")]
        public bool ShowLines { get; set; } = true;

        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示展开按钮")]
        [PropertyDescription("是否在可展开节点旁显示 +/- 按钮")]
        public bool ShowPlusMinus { get; set; } = true;

        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示复选框")]
        [PropertyDescription("是否在每个节点前显示勾选框")]
        public bool CheckBoxes { get; set; } = false;

        [PropertyCategory("显示元素")]
        [PropertyDisplayName("显示搜索框")]
        [PropertyDescription("是否在树顶部显示搜索输入框")]
        public bool ShowSearchBox { get; set; } = false;

        // 交互
        [PropertyCategory("交互")]
        [PropertyDisplayName("允许拖放")]
        [PropertyDescription("是否允许拖拽节点进行重新排序")]
        public bool AllowDrop { get; set; } = false;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 240;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 240;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentTreeView tv)
            {
                tv.Enabled = Enabled;
                tv.NodeHeight = NodeHeight;
                tv.NodeIndent = NodeIndent;
                tv.FullRowSelect = FullRowSelect;
                tv.ShowLines = ShowLines;
                tv.ShowPlusMinus = ShowPlusMinus;
                tv.CheckBoxes = CheckBoxes;
                tv.ShowSearchBox = ShowSearchBox;
                tv.AllowDrop = AllowDrop;
                tv.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentTreeView tv)
            {
                Enabled = tv.Enabled;
                NodeHeight = tv.NodeHeight;
                NodeIndent = tv.NodeIndent;
                FullRowSelect = tv.FullRowSelect;
                ShowLines = tv.ShowLines;
                ShowPlusMinus = tv.ShowPlusMinus;
                CheckBoxes = tv.CheckBoxes;
                ShowSearchBox = tv.ShowSearchBox;
                AllowDrop = tv.AllowDrop;
                Width = tv.Width;
                Height = tv.Height;
            }
        }

        public static TreeViewDemoConfig FromControl(FluentTreeView tv)
        {
            var cfg = new TreeViewDemoConfig();
            cfg.ReadFrom(tv);
            return cfg;
        }
    }

    #endregion

    #region 容器控件配置

    public class PanelDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 标题栏
        [PropertyCategory("标题栏")]
        [PropertyDisplayName("显示标题栏")]
        public bool ShowTitleBar { get; set; } = true;

        [PropertyCategory("标题栏")]
        [PropertyDisplayName("显示标题文本")]
        public bool ShowTitleText { get; set; } = true;

        [PropertyCategory("标题栏")]
        [PropertyDisplayName("标题文本")]
        public string TitleText { get; set; } = "Panel Title";

        [PropertyCategory("标题栏")]
        [PropertyDisplayName("标题对齐")]
        [PropertyDescription("Left=左对齐 | Center=居中 | Right=右对齐")]
        public TitleAlignment TitleAlignment { get; set; } = TitleAlignment.Left;

        [PropertyCategory("标题栏")]
        [PropertyDisplayName("标题栏高度")]
        public int TitleHeight { get; set; } = 32;

        [PropertyCategory("标题栏")]
        [PropertyDisplayName("标题背景色")]
        public Color TitleBackColor { get; set; } = Color.FromArgb(0, 120, 212);

        [PropertyCategory("标题栏")]
        [PropertyDisplayName("标题文本色")]
        public Color TitleForeColor { get; set; } = Color.White;

        // 折叠
        [PropertyCategory("折叠")]
        [PropertyDisplayName("可折叠")]
        [PropertyDescription("是否允许通过点击标题栏折叠面板")]
        public bool Collapsible { get; set; } = true;

        [PropertyCategory("折叠")]
        [PropertyDisplayName("已折叠")]
        public bool IsCollapsed { get; set; } = false;

        [PropertyCategory("折叠")]
        [PropertyDisplayName("折叠动画")]
        [PropertyDescription("折叠/展开时是否播放过渡动画")]
        public bool ShowAnimation { get; set; } = false;

        [PropertyCategory("折叠")]
        [PropertyDisplayName("动画时长")]
        [PropertyDescription("折叠动画持续时间(毫秒)")]
        public int AnimationDuration { get; set; } = 200;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = true;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.LightGray;

        // 滚动条
        [PropertyCategory("滚动条")]
        [PropertyDisplayName("滚动条显示")]
        [PropertyDescription("Auto=自动 | Always=始终 | Never=隐藏")]
        public ScrollBarVisibility ScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

        // 状态文本
        [PropertyCategory("状态文本")]
        [PropertyDisplayName("状态文本")]
        [PropertyDescription("面板底部显示的状态信息")]
        public string StatusText { get; set; } = "";

        [PropertyCategory("状态文本")]
        [PropertyDisplayName("状态文本对齐")]
        public ContentAlignment StatusAlignment { get; set; } = ContentAlignment.BottomLeft;

        [PropertyCategory("状态文本")]
        [PropertyDisplayName("状态文本前景色")]
        public Color StatusForeColor { get; set; } = Color.FromArgb(100, 100, 100);

        [PropertyCategory("状态文本")]
        [PropertyDisplayName("状态文本背景色")]
        public Color StatusBackColor { get; set; } = Color.FromArgb(240, 240, 240);

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 300;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 200;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentPanel pnl)
            {
                pnl.Enabled = Enabled;
                pnl.ShowTitleBar = ShowTitleBar;
                pnl.ShowTitleText = ShowTitleText;
                pnl.TitleText = TitleText;
                pnl.TitleAlignment = TitleAlignment;
                pnl.TitleHeight = TitleHeight;
                try { pnl.TitleBackColor = TitleBackColor; } catch { }
                try { pnl.TitleForeColor = TitleForeColor; } catch { }
                pnl.Collapsible = Collapsible;
                pnl.ShowAnimation = ShowAnimation;
                pnl.AnimationDuration = AnimationDuration;
                pnl.ShowBorder = ShowBorder;
                pnl.BorderWidth = BorderWidth;
                try { pnl.BorderColor = BorderColor; } catch { }
                pnl.ScrollBarVisibility = ScrollBarVisibility;
                pnl.StatusText = StatusText;
                pnl.StatusAlignment = StatusAlignment;
                pnl.StatusForeColor = StatusForeColor;
                pnl.StatusBackColor = StatusBackColor;
                pnl.Size = new Size(Width, Height);

                // IsCollapsed 放在最后以确保尺寸先设定好
                if (pnl.IsCollapsed != IsCollapsed)
                {
                    pnl.IsCollapsed = IsCollapsed;
                }
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentPanel pnl)
            {
                Enabled = pnl.Enabled;
                ShowTitleBar = pnl.ShowTitleBar;
                ShowTitleText = pnl.ShowTitleText;
                TitleText = pnl.TitleText;
                TitleAlignment = pnl.TitleAlignment;
                TitleHeight = pnl.TitleHeight;
                TitleBackColor = pnl.TitleBackColor;
                TitleForeColor = pnl.TitleForeColor;
                Collapsible = pnl.Collapsible;
                IsCollapsed = pnl.IsCollapsed;
                ShowAnimation = pnl.ShowAnimation;
                AnimationDuration = pnl.AnimationDuration;
                ShowBorder = pnl.ShowBorder;
                BorderWidth = pnl.BorderWidth;
                BorderColor = pnl.BorderColor;
                ScrollBarVisibility = pnl.ScrollBarVisibility;
                StatusText = pnl.StatusText;
                StatusAlignment = pnl.StatusAlignment;
                StatusForeColor = pnl.StatusForeColor;
                StatusBackColor = pnl.StatusBackColor;
                Width = pnl.Width;
                Height = pnl.Height;
            }
        }

        public static PanelDemoConfig FromControl(FluentPanel pnl)
        {
            var cfg = new PanelDemoConfig();
            cfg.ReadFrom(pnl);
            return cfg;
        }
    }

    public class TabControlDemoConfig : ControlDemoConfigBase
    {
        // 标签外观
        [PropertyCategory("标签外观")]
        [PropertyDisplayName("标签位置")]
        [PropertyDescription("Top=顶部 | Bottom=底部 | Left=左侧 | Right=右侧")]
        public TabAlignment TabAlignment { get; set; } = TabAlignment.Top;

        [PropertyCategory("标签外观")]
        [PropertyDisplayName("标签高度")]
        [PropertyDescription("每个标签页头的像素高度")]
        public int TabHeight { get; set; } = 32;

        [PropertyCategory("标签外观")]
        [PropertyDisplayName("标签最小宽度")]
        public int TabMinWidth { get; set; } = 100;

        [PropertyCategory("标签外观")]
        [PropertyDisplayName("标签最大宽度")]
        public int TabMaxWidth { get; set; } = 200;

        [PropertyCategory("标签外观")]
        [PropertyDisplayName("标签内边距")]
        public Padding TabPadding { get; set; } = new Padding(10, 5, 10, 5);

        [PropertyCategory("标签外观")]
        [PropertyDisplayName("边框样式")]
        [PropertyDescription("None=无 | FixedSingle=单线")]
        public BorderStyle BorderStyle { get; set; } = BorderStyle.FixedSingle;

        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("关闭按钮")]
        [PropertyDescription("是否在标签上显示关闭(×)按钮")]
        public bool ShowCloseButton { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("切换动画")]
        [PropertyDescription("切换标签页时是否播放过渡动画")]
        public bool AnimateTabSwitch { get; set; } = true;

        // 颜色
        [PropertyCategory("颜色")]
        [PropertyDisplayName("标签栏背景色")]
        public Color TabStripBackColor { get; set; } = SystemColors.Control;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("默认标签背景色")]
        public Color DefaultTabBackColor { get; set; } = SystemColors.Control;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("默认标签前景色")]
        public Color DefaultTabForeColor { get; set; } = SystemColors.ControlText;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("选中标签背景色")]
        public Color SelectedTabBackColor { get; set; } = SystemColors.Highlight;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("选中标签前景色")]
        public Color SelectedTabForeColor { get; set; } = SystemColors.HighlightText;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("悬停标签背景色")]
        public Color HoverTabBackColor { get; set; } = SystemColors.ControlLight;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 400;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 260;

        // 数据
        [PropertyCategory("数据")]
        [PropertyDisplayName("标签页数量")]
        [PropertyDescription("预览时生成的示例标签页数")]
        [PropertyReadOnly]
        public int TabCount { get; set; } = 3;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentTabControl tab)
            {
                tab.TabAlignment = TabAlignment;
                tab.TabHeight = TabHeight;
                tab.TabMinWidth = TabMinWidth;
                tab.TabMaxWidth = TabMaxWidth;
                tab.TabPadding = TabPadding;
                tab.BorderStyle = BorderStyle;
                tab.ShowCloseButton = ShowCloseButton;
                tab.AnimateTabSwitch = AnimateTabSwitch;
                tab.TabStripBackColor = TabStripBackColor;
                tab.DefaultTabBackColor = DefaultTabBackColor;
                tab.DefaultTabForeColor = DefaultTabForeColor;
                tab.SelectedTabBackColor = SelectedTabBackColor;
                tab.SelectedTabForeColor = SelectedTabForeColor;
                tab.HoverTabBackColor = HoverTabBackColor;
                tab.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentTabControl tab)
            {
                TabAlignment = tab.TabAlignment;
                TabHeight = tab.TabHeight;
                TabMinWidth = tab.TabMinWidth;
                TabMaxWidth = tab.TabMaxWidth;
                TabPadding = tab.TabPadding;
                BorderStyle = tab.BorderStyle;
                ShowCloseButton = tab.ShowCloseButton;
                AnimateTabSwitch = tab.AnimateTabSwitch;
                TabStripBackColor = tab.TabStripBackColor;
                DefaultTabBackColor = tab.DefaultTabBackColor;
                DefaultTabForeColor = tab.DefaultTabForeColor;
                SelectedTabBackColor = tab.SelectedTabBackColor;
                SelectedTabForeColor = tab.SelectedTabForeColor;
                HoverTabBackColor = tab.HoverTabBackColor;
                Width = tab.Width;
                Height = tab.Height;
                TabCount = tab.TabCount;
            }
        }

        public static TabControlDemoConfig FromControl(FluentTabControl tab)
        {
            var cfg = new TabControlDemoConfig();
            cfg.ReadFrom(tab);
            return cfg;
        }
    }

    public class RepeaterDemoConfig : ControlDemoConfigBase
    {
        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("布局模式")]
        [PropertyDescription("Auto=自适应 | Horizontal=横向 | Vertical=纵向 | Grid=网格")]
        public RepeaterLayoutMode LayoutMode { get; set; } = RepeaterLayoutMode.Auto;

        [PropertyCategory("布局")]
        [PropertyDisplayName("项默认尺寸")]
        [PropertyDescription("每个重复项的默认宽高")]
        public Size ItemDefaultSize { get; set; } = new Size(200, 100);

        [PropertyCategory("布局")]
        [PropertyDisplayName("项间距")]
        [PropertyDescription("各项之间的上下左右间距")]
        public Padding ItemPadding { get; set; } = new Padding(6);

        [PropertyCategory("布局")]
        [PropertyDisplayName("自适应项大小")]
        [PropertyDescription("根据内容自动调整每项尺寸")]
        public bool AutoSizeItems { get; set; } = false;

        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("最大项数")]
        [PropertyDescription("容器可容纳的最大项数量")]
        public int MaxItemCount { get; set; } = 100;

        [PropertyCategory("行为")]
        [PropertyDisplayName("自动滚动")]
        [PropertyDescription("内容超出时是否启用滚动")]
        public bool AutoScroll { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("显示添加按钮")]
        [PropertyDescription("悬停空白区域时是否显示 + 按钮")]
        public bool ShowAddButton { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("删除图标位置")]
        [PropertyDescription("每项上删除按钮的位置")]
        public ItemDeleteIconPosition DeleteIconPosition { get; set; } = ItemDeleteIconPosition.TopRight;

        [PropertyCategory("行为")]
        [PropertyDisplayName("删除图标大小")]
        public int DeleteIconSize { get; set; } = 24;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.FromArgb(200, 200, 200);

        [PropertyCategory("外观")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        [PropertyCategory("外观")]
        [PropertyDisplayName("圆角半径")]
        public int CornerRadius { get; set; } = 4;

        [PropertyCategory("外观")]
        [PropertyDisplayName("背景色")]
        public Color BackColor { get; set; } = Color.FromArgb(250, 250, 250);

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 400;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 300;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentRepeater rp)
            {
                rp.LayoutMode = LayoutMode;
                rp.ItemDefaultSize = ItemDefaultSize;
                rp.ItemPadding = ItemPadding;
                rp.AutoSizeItems = AutoSizeItems;
                rp.MaxItemCount = MaxItemCount;
                rp.AutoScroll = AutoScroll;
                rp.ShowAddButton = ShowAddButton;
                rp.DeleteIconPosition = DeleteIconPosition;
                rp.DeleteIconSize = DeleteIconSize;
                rp.BorderColor = BorderColor;
                rp.BorderWidth = BorderWidth;
                rp.CornerRadius = CornerRadius;
                rp.BackColor = BackColor;
                rp.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentRepeater rp)
            {
                LayoutMode = rp.LayoutMode;
                ItemDefaultSize = rp.ItemDefaultSize;
                ItemPadding = rp.ItemPadding;
                AutoSizeItems = rp.AutoSizeItems;
                MaxItemCount = rp.MaxItemCount;
                AutoScroll = rp.AutoScroll;
                ShowAddButton = rp.ShowAddButton;
                DeleteIconPosition = rp.DeleteIconPosition;
                DeleteIconSize = rp.DeleteIconSize;
                BorderColor = rp.BorderColor;
                BorderWidth = rp.BorderWidth;
                CornerRadius = rp.CornerRadius;
                BackColor = rp.BackColor;
                Width = rp.Width;
                Height = rp.Height;
            }
        }

        public static RepeaterDemoConfig FromControl(FluentRepeater rp)
        {
            var cfg = new RepeaterDemoConfig();
            cfg.ReadFrom(rp);
            return cfg;
        }
    }

    #endregion

    #region 数据控件配置

    public class DataGridViewDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否只读")]
        public bool ReadOnly { get; set; } = false;

        // 行头与列头
        [PropertyCategory("行头与列头")]
        [PropertyDisplayName("显示行头")]
        public bool ShowRowHeader { get; set; } = true;

        [PropertyCategory("行头与列头")]
        [PropertyDisplayName("行头宽度")]
        public int RowHeaderWidth { get; set; } = 42;

        [PropertyCategory("行头与列头")]
        [PropertyDisplayName("列头高度")]
        public int ColumnHeaderHeight { get; set; } = 36;

        [PropertyCategory("行头与列头")]
        [PropertyDisplayName("显示行号")]
        public bool ShowRowNumbers { get; set; } = true;

        [PropertyCategory("行头与列头")]
        [PropertyDisplayName("起始行号")]
        public int StartRowNumber { get; set; } = 1;

        // 行
        [PropertyCategory("行")]
        [PropertyDisplayName("默认行高")]
        public int DefaultRowHeight { get; set; } = 32;

        [PropertyCategory("行")]
        [PropertyDisplayName("交替行颜色")]
        [PropertyDescription("是否启用奇偶行交替背景色")]
        public bool AlternatingRowColors { get; set; } = true;

        [PropertyCategory("行")]
        [PropertyDisplayName("交替行色")]
        public Color AlternatingRowColor { get; set; } = Color.FromArgb(245, 245, 245);

        // 网格线
        [PropertyCategory("网格线")]
        [PropertyDisplayName("显示网格线")]
        public bool ShowGridLines { get; set; } = true;

        [PropertyCategory("网格线")]
        [PropertyDisplayName("网格线颜色")]
        public Color GridColor { get; set; } = Color.FromArgb(229, 229, 229);

        // 选择与编辑
        [PropertyCategory("选择与编辑")]
        [PropertyDisplayName("选择模式")]
        [PropertyDescription("FullRowSelect=整行选择 | CellSelect=单元格选择")]
        public DataGridViewSelectionMode SelectionMode { get; set; } = DataGridViewSelectionMode.FullRowSelect;

        [PropertyCategory("选择与编辑")]
        [PropertyDisplayName("编辑模式")]
        [PropertyDescription("EditOnDoubleClick=双击编辑 | EditOnEnter=进入编辑 | EditOnKeystroke=按键编辑")]
        public DataGridViewEditMode EditMode { get; set; } = DataGridViewEditMode.EditOnDoubleClick;

        // 用户操作
        [PropertyCategory("用户操作")]
        [PropertyDisplayName("允许添加行")]
        public bool AllowUserToAddRows { get; set; } = false;

        [PropertyCategory("用户操作")]
        [PropertyDisplayName("允许删除行")]
        public bool AllowUserToDeleteRows { get; set; } = false;

        [PropertyCategory("用户操作")]
        [PropertyDisplayName("允许调整列宽")]
        public bool AllowUserToResizeColumns { get; set; } = true;

        [PropertyCategory("用户操作")]
        [PropertyDisplayName("启用筛选")]
        public bool EnableFiltering { get; set; } = true;

        [PropertyCategory("用户操作")]
        [PropertyDisplayName("启用默认右键菜单")]
        public bool EnableDefaultContextMenu { get; set; } = true;

        // 分页
        [PropertyCategory("分页")]
        [PropertyDisplayName("启用分页")]
        public bool EnablePagination { get; set; } = false;

        [PropertyCategory("分页")]
        [PropertyDisplayName("每页行数")]
        [PropertyDescription("0表示不分页")]
        public int PageSize { get; set; } = 0;

        [PropertyCategory("分页")]
        [PropertyDisplayName("分页位置")]
        [PropertyDescription("Bottom=底部 | Top=顶部")]
        public PaginationPosition PaginationPosition { get; set; } = PaginationPosition.Bottom;

        [PropertyCategory("分页")]
        [PropertyDisplayName("分页控件高度")]
        public int PaginationHeight { get; set; } = 45;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 400;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 220;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentDataGridView dgv)
            {
                dgv.Enabled = Enabled;
                dgv.ReadOnly = ReadOnly;
                dgv.ShowRowHeader = ShowRowHeader;
                dgv.RowHeaderWidth = RowHeaderWidth;
                dgv.ColumnHeaderHeight = ColumnHeaderHeight;
                dgv.ShowRowNumbers = ShowRowNumbers;
                dgv.StartRowNumber = StartRowNumber;
                dgv.DefaultRowHeight = DefaultRowHeight;
                dgv.AlternatingRowColors = AlternatingRowColors;
                dgv.AlternatingRowColor = AlternatingRowColor;
                dgv.ShowGridLines = ShowGridLines;
                dgv.GridColor = GridColor;
                dgv.SelectionMode = SelectionMode;
                dgv.EditMode = EditMode;
                dgv.AllowUserToAddRows = AllowUserToAddRows;
                dgv.AllowUserToDeleteRows = AllowUserToDeleteRows;
                dgv.AllowUserToResizeColumns = AllowUserToResizeColumns;
                dgv.EnableFiltering = EnableFiltering;
                dgv.EnableDefaultContextMenu = EnableDefaultContextMenu;
                dgv.EnablePagination = EnablePagination;
                if (EnablePagination)
                {
                    dgv.PageSize = PageSize;
                    dgv.PaginationPosition = PaginationPosition;
                    dgv.PaginationHeight = PaginationHeight;
                }
                dgv.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentDataGridView dgv)
            {
                Enabled = dgv.Enabled;
                ReadOnly = dgv.ReadOnly;
                ShowRowHeader = dgv.ShowRowHeader;
                RowHeaderWidth = dgv.RowHeaderWidth;
                ColumnHeaderHeight = dgv.ColumnHeaderHeight;
                ShowRowNumbers = dgv.ShowRowNumbers;
                StartRowNumber = dgv.StartRowNumber;
                DefaultRowHeight = dgv.DefaultRowHeight;
                AlternatingRowColors = dgv.AlternatingRowColors;
                AlternatingRowColor = dgv.AlternatingRowColor;
                ShowGridLines = dgv.ShowGridLines;
                GridColor = dgv.GridColor;
                SelectionMode = dgv.SelectionMode;
                EditMode = dgv.EditMode;
                AllowUserToAddRows = dgv.AllowUserToAddRows;
                AllowUserToDeleteRows = dgv.AllowUserToDeleteRows;
                AllowUserToResizeColumns = dgv.AllowUserToResizeColumns;
                EnableFiltering = dgv.EnableFiltering;
                EnableDefaultContextMenu = dgv.EnableDefaultContextMenu;
                EnablePagination = dgv.EnablePagination;
                PageSize = dgv.PageSize;
                PaginationPosition = dgv.PaginationPosition;
                PaginationHeight = dgv.PaginationHeight;
                Width = dgv.Width;
                Height = dgv.Height;
            }
        }

        public static DataGridViewDemoConfig FromControl(FluentDataGridView dgv)
        {
            var cfg = new DataGridViewDemoConfig();
            cfg.ReadFrom(dgv);
            return cfg;
        }
    }

    public class ListViewDemoConfig : ControlDemoConfigBase
    {
        // 视图模式
        [PropertyCategory("视图模式")]
        [PropertyDisplayName("视图")]
        [PropertyDescription("Details=详细 | LargeIcon=大图标 | SmallIcon=小图标 | List=列表 | Tile=平铺")]
        public FluentListViewMode View { get; set; } = FluentListViewMode.Details;

        // 列标题
        [PropertyCategory("列标题")]
        [PropertyDisplayName("显示列标题")]
        [PropertyDescription("Details 模式下是否显示表头")]
        public bool ShowColumnHeaders { get; set; } = true;

        [PropertyCategory("列标题")]
        [PropertyDisplayName("标题栏高度")]
        public int HeaderHeight { get; set; } = 28;

        [PropertyCategory("列标题")]
        [PropertyDisplayName("允许列调宽")]
        [PropertyDescription("是否允许拖拽调整列宽")]
        public bool AllowColumnResize { get; set; } = true;

        [PropertyCategory("列标题")]
        [PropertyDisplayName("允许列重排")]
        [PropertyDescription("是否允许拖拽调整列顺序")]
        public bool AllowColumnReorder { get; set; } = false;

        // 项目外观
        [PropertyCategory("项目外观")]
        [PropertyDisplayName("项高度")]
        [PropertyDescription("Details/List 模式下每行高度")]
        public int ItemHeight { get; set; } = 24;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("项间距")]
        public int ItemSpacing { get; set; } = 4;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("大图标尺寸")]
        [PropertyDescription("LargeIcon 模式下图标像素大小")]
        public int LargeIconSize { get; set; } = 48;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("小图标尺寸")]
        [PropertyDescription("SmallIcon/Details 模式下图标像素大小")]
        public int SmallIconSize { get; set; } = 16;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("大图标项宽")]
        [PropertyDescription("LargeIcon 模式下每项的宽度")]
        public int LargeIconItemWidth { get; set; } = 120;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("小图标项宽")]
        [PropertyDescription("SmallIcon 模式下每项的宽度")]
        public int SmallIconItemWidth { get; set; } = 100;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("Tile项高度")]
        [PropertyDescription("Tile 模式下每项的高度")]
        public int TileItemHeight { get; set; } = 64;

        [PropertyCategory("项目外观")]
        [PropertyDisplayName("内容边距")]
        public Padding ContentPadding { get; set; } = new Padding(4);

        // 交互
        [PropertyCategory("交互")]
        [PropertyDisplayName("选中整行")]
        [PropertyDescription("点击任意列时是否选中整行")]
        public bool FullRowSelect { get; set; } = true;

        [PropertyCategory("交互")]
        [PropertyDisplayName("网格线")]
        [PropertyDescription("是否显示行列网格线")]
        public bool GridLines { get; set; } = false;

        [PropertyCategory("交互")]
        [PropertyDisplayName("复选框")]
        [PropertyDescription("是否在每行前显示勾选框")]
        public bool CheckBoxes { get; set; } = false;

        [PropertyCategory("交互")]
        [PropertyDisplayName("复选框样式")]
        [PropertyDescription("Check=勾选框 | Switch=开关")]
        public ListViewCheckBoxStyle CheckBoxStyle { get; set; } = ListViewCheckBoxStyle.Check;

        [PropertyCategory("交互")]
        [PropertyDisplayName("多选")]
        [PropertyDescription("是否允许 Ctrl/Shift 多选")]
        public bool MultiSelect { get; set; } = true;

        [PropertyCategory("交互")]
        [PropertyDisplayName("悬停选中")]
        [PropertyDescription("鼠标悬停时自动选中该项")]
        public bool HoverSelection { get; set; } = false;

        // 排序
        [PropertyCategory("排序")]
        [PropertyDisplayName("排序列")]
        [PropertyDescription("按哪一列排序 (-1=不排序)")]
        public int SortColumnIndex { get; set; } = -1;

        [PropertyCategory("排序")]
        [PropertyDisplayName("排序方式")]
        [PropertyDescription("None=无 | Ascending=升序 | Descending=降序")]
        public ColumnSortOrder Sorting { get; set; } = ColumnSortOrder.None;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 480;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 300;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentListView lv)
            {
                lv.View = View;
                lv.ShowColumnHeaders = ShowColumnHeaders;
                lv.HeaderHeight = HeaderHeight;
                lv.AllowColumnResize = AllowColumnResize;
                lv.AllowColumnReorder = AllowColumnReorder;
                lv.ItemHeight = ItemHeight;
                lv.ItemSpacing = ItemSpacing;
                lv.LargeIconSize = LargeIconSize;
                lv.SmallIconSize = SmallIconSize;
                lv.LargeIconItemWidth = LargeIconItemWidth;
                lv.SmallIconItemWidth = SmallIconItemWidth;
                lv.TileItemHeight = TileItemHeight;
                lv.Padding = ContentPadding;
                lv.FullRowSelect = FullRowSelect;
                lv.GridLines = GridLines;
                lv.CheckBoxes = CheckBoxes;
                lv.CheckBoxStyle = CheckBoxStyle;
                lv.MultiSelect = MultiSelect;
                lv.HoverSelection = HoverSelection;
                lv.SortColumnIndex = SortColumnIndex;
                lv.Sorting = Sorting;
                lv.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentListView lv)
            {
                View = lv.View;
                ShowColumnHeaders = lv.ShowColumnHeaders;
                HeaderHeight = lv.HeaderHeight;
                AllowColumnResize = lv.AllowColumnResize;
                AllowColumnReorder = lv.AllowColumnReorder;
                ItemHeight = lv.ItemHeight;
                ItemSpacing = lv.ItemSpacing;
                LargeIconSize = lv.LargeIconSize;
                SmallIconSize = lv.SmallIconSize;
                LargeIconItemWidth = lv.LargeIconItemWidth;
                SmallIconItemWidth = lv.SmallIconItemWidth;
                TileItemHeight = lv.TileItemHeight;
                ContentPadding = lv.Padding;
                FullRowSelect = lv.FullRowSelect;
                GridLines = lv.GridLines;
                CheckBoxes = lv.CheckBoxes;
                CheckBoxStyle = lv.CheckBoxStyle;
                MultiSelect = lv.MultiSelect;
                HoverSelection = lv.HoverSelection;
                SortColumnIndex = lv.SortColumnIndex;
                Sorting = lv.Sorting;
                Width = lv.Width;
                Height = lv.Height;
            }
        }

        public static ListViewDemoConfig FromControl(FluentListView lv)
        {
            var cfg = new ListViewDemoConfig();
            cfg.ReadFrom(lv);
            return cfg;
        }
    }

    #endregion

    #region 组件配置

    public class TooltipDemoConfig : ControlDemoConfigBase
    {
        // 行为
        [PropertyCategory("行为")]
        [PropertyDisplayName("是否激活")]
        [PropertyDescription("是否启用工具提示功能")]
        public bool Active { get; set; } = true;

        [PropertyCategory("行为")]
        [PropertyDisplayName("显示延迟")]
        [PropertyDescription("鼠标悬停后显示提示的延迟时间(ms)")]
        public int ShowDelay { get; set; } = 500;

        [PropertyCategory("行为")]
        [PropertyDisplayName("自动隐藏延迟")]
        [PropertyDescription("提示显示后自动隐藏的延迟时间(ms), 0不自动隐藏")]
        public int AutoPopDelay { get; set; } = 5000;

        // 动画
        [PropertyCategory("动画")]
        [PropertyDisplayName("淡入时长")]
        [PropertyDescription("淡入动画持续时间(ms)")]
        public int FadeInDuration { get; set; } = 150;

        [PropertyCategory("动画")]
        [PropertyDisplayName("淡出时长")]
        [PropertyDescription("淡出动画持续时间(ms)")]
        public int FadeOutDuration { get; set; } = 100;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("背景颜色")]
        public Color BackgroundColor { get; set; } = Color.WhiteSmoke;

        [PropertyCategory("外观")]
        [PropertyDisplayName("内容文字色")]
        public Color ForeColor { get; set; } = Color.DimGray;

        [PropertyCategory("外观")]
        [PropertyDisplayName("标题文字色")]
        public Color TitleColor { get; set; } = Color.Black;

        [PropertyCategory("外观")]
        [PropertyDisplayName("标题背景色")]
        public Color TitleBackgroundColor { get; set; } = Color.FromArgb(210, 210, 210);

        [PropertyCategory("外观")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Gray;

        [PropertyCategory("外观")]
        [PropertyDisplayName("圆角半径")]
        public int BorderRadius { get; set; } = 6;

        [PropertyCategory("外观")]
        [PropertyDisplayName("显示阴影")]
        public bool ShowShadow { get; set; } = true;

        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("最大宽度")]
        public int MaxWidth { get; set; } = 350;

        [PropertyCategory("布局")]
        [PropertyDisplayName("内容内边距")]
        public Padding ContentPadding { get; set; } = new Padding(5, 1, 5, 1);

        [PropertyCategory("布局")]
        [PropertyDisplayName("标题内边距")]
        public Padding TitlePadding { get; set; } = new Padding(0, 2, 0, 2);

        [PropertyCategory("布局")]
        [PropertyDisplayName("显示位置")]
        [PropertyDescription("Bottom=下方 | Top=上方 | Left=左侧 | Right=右侧")]
        public TooltipPosition Position { get; set; } = TooltipPosition.Bottom;

        [PropertyCategory("布局")]
        [PropertyDisplayName("偏移量")]
        [PropertyDescription("提示框距离目标控件的像素偏移")]
        public int OffsetFromControl { get; set; } = 8;

        [PropertyCategory("布局")]
        [PropertyDisplayName("标题图标大小")]
        [PropertyDescription("0表示根据字体自动计算")]
        public int TitleIconSize { get; set; } = 0;

        [PropertyCategory("布局")]
        [PropertyDisplayName("标题图标缩放")]
        [PropertyDescription("标题图标相对于字体行高的缩放(0.5~3)")]
        public float TitleIconScale { get; set; } = 1.3f;

        // 示例内容
        [PropertyCategory("示例内容")]
        [PropertyDisplayName("示例提示文本")]
        [PropertyDescription("悬停在预览按钮上时显示的提示内容")]
        public string DemoTooltipText { get; set; } = "这是一段工具提示内容, 用于演示FluentTooltip的效果。";

        [PropertyCategory("示例内容")]
        [PropertyDisplayName("示例提示标题")]
        public string DemoTooltipTitle { get; set; } = "提示标题";

        /// <summary>
        /// FluentTooltip 是 Component, 此处仅将样式属性保存到配置
        /// 实际应用时需要在预览控件上找到关联的 FluentTooltip 实例
        /// </summary>
        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            // FluentTooltip 不是 Control, 需要在预览面板中查找关联的 tooltip
            // 通过 Tag 获取
            if (ctrl.Tag is FluentTooltip tt)
            {
                tt.Active = Active;
                tt.ShowDelay = ShowDelay;
                tt.AutoPopDelay = AutoPopDelay;
                tt.FadeInDuration = FadeInDuration;
                tt.FadeOutDuration = FadeOutDuration;
                tt.BackgroundColor = BackgroundColor;
                tt.ForeColor = ForeColor;
                tt.TitleColor = TitleColor;
                tt.TitleBackgroundColor = TitleBackgroundColor;
                tt.BorderColor = BorderColor;
                tt.BorderRadius = BorderRadius;
                tt.ShowShadow = ShowShadow;
                tt.MaxWidth = MaxWidth;
                tt.ContentPadding = ContentPadding;
                tt.TitlePadding = TitlePadding;
                tt.Position = Position;
                tt.OffsetFromControl = OffsetFromControl;

                // 更新示例内容
                tt.SetTooltipText(ctrl, DemoTooltipText);
                tt.SetTooltipTitle(ctrl, DemoTooltipTitle);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl.Tag is FluentTooltip tt)
            {
                Active = tt.Active;
                ShowDelay = tt.ShowDelay;
                AutoPopDelay = tt.AutoPopDelay;
                FadeInDuration = tt.FadeInDuration;
                FadeOutDuration = tt.FadeOutDuration;
                BackgroundColor = tt.BackgroundColor;
                ForeColor = tt.ForeColor;
                TitleColor = tt.TitleColor;
                TitleBackgroundColor = tt.TitleBackgroundColor;
                BorderColor = tt.BorderColor;
                BorderRadius = tt.BorderRadius;
                ShowShadow = tt.ShowShadow;
                MaxWidth = tt.MaxWidth;
                ContentPadding = tt.ContentPadding;
                TitlePadding = tt.TitlePadding;
                Position = tt.Position;
                OffsetFromControl = tt.OffsetFromControl;
                DemoTooltipText = tt.GetTooltipText(ctrl);
                DemoTooltipTitle = tt.GetTooltipTitle(ctrl);
            }
        }

        public static TooltipDemoConfig FromControl(Control ctrl)
        {
            var cfg = new TooltipDemoConfig();
            cfg.ReadFrom(ctrl);
            return cfg;
        }
    }

    #endregion

    #region 工具栏控件配置

    public class MenuStripDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("菜单背景色")]
        [PropertyDescription("留空使用主题色")]
        public Color MenuBackColor { get; set; } = Color.Empty;

        [PropertyCategory("外观")]
        [PropertyDisplayName("菜单前景色")]
        [PropertyDescription("留空使用主题色")]
        public Color MenuForeColor { get; set; } = Color.Empty;

        [PropertyCategory("外观")]
        [PropertyDisplayName("菜单项高度")]
        public int ItemHeight { get; set; } = 28;

        // 下拉菜单
        [PropertyCategory("下拉菜单")]
        [PropertyDisplayName("显示下拉边框")]
        public bool ShowDropDownBorder { get; set; } = true;

        [PropertyCategory("下拉菜单")]
        [PropertyDisplayName("下拉边框颜色")]
        [PropertyDescription("留空使用主题色")]
        public Color DropDownBorderColor { get; set; } = Color.Empty;

        [PropertyCategory("下拉菜单")]
        [PropertyDisplayName("下拉边框宽度")]
        public int DropDownBorderWidth { get; set; } = 1;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 360;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 28;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentMenuStrip ms)
            {
                ms.Enabled = Enabled;
                ms.MenuBackColor = MenuBackColor;
                ms.MenuForeColor = MenuForeColor;
                ms.ItemHeight = ItemHeight;
                ms.ShowDropDownBorder = ShowDropDownBorder;
                ms.DropDownBorderColor = DropDownBorderColor;
                ms.DropDownBorderWidth = DropDownBorderWidth;
                ms.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentMenuStrip ms)
            {
                Enabled = ms.Enabled;
                MenuBackColor = ms.MenuBackColor;
                MenuForeColor = ms.MenuForeColor;
                ItemHeight = ms.ItemHeight;
                ShowDropDownBorder = ms.ShowDropDownBorder;
                DropDownBorderColor = ms.DropDownBorderColor;
                DropDownBorderWidth = ms.DropDownBorderWidth;
                Width = ms.Width;
                Height = ms.Height;
            }
        }

        public static MenuStripDemoConfig FromControl(FluentMenuStrip ms)
        {
            var cfg = new MenuStripDemoConfig();
            cfg.ReadFrom(ms);
            return cfg;
        }
    }

    public class ToolStripDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("方向")]
        [PropertyDescription("Horizontal=水平 | Vertical=垂直")]
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        [PropertyCategory("布局")]
        [PropertyDisplayName("项目间距")]
        public int ItemSpacing { get; set; } = 2;

        [PropertyCategory("布局")]
        [PropertyDisplayName("项目内边距")]
        public Padding ItemPadding { get; set; } = new Padding(2, 0, 2, 0);

        [PropertyCategory("布局")]
        [PropertyDisplayName("允许溢出")]
        [PropertyDescription("项目超出宽度时是否显示溢出按钮")]
        public bool CanOverflow { get; set; } = true;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("工具栏背景色")]
        [PropertyDescription("留空使用主题色")]
        public Color ToolStripBackColor { get; set; } = Color.Empty;

        [PropertyCategory("外观")]
        [PropertyDisplayName("渲染模式")]
        [PropertyDescription("Professional=专业 | Flat=扁平")]
        public FluentToolStripRenderMode RenderMode { get; set; } = FluentToolStripRenderMode.Professional;

        [PropertyCategory("外观")]
        [PropertyDisplayName("显示拖动手柄")]
        public bool ShowGripHandle { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 360;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 32;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentToolStrip ts)
            {
                ts.Enabled = Enabled;
                ts.Orientation = Orientation;
                ts.ItemSpacing = ItemSpacing;
                ts.ItemPadding = ItemPadding;
                ts.CanOverflow = CanOverflow;
                ts.ToolStripBackColor = ToolStripBackColor;
                ts.RenderMode = RenderMode;
                ts.ShowGripHandle = ShowGripHandle;
                ts.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentToolStrip ts)
            {
                Enabled = ts.Enabled;
                Orientation = ts.Orientation;
                ItemSpacing = ts.ItemSpacing;
                ItemPadding = ts.ItemPadding;
                CanOverflow = ts.CanOverflow;
                ToolStripBackColor = ts.ToolStripBackColor;
                RenderMode = ts.RenderMode;
                ShowGripHandle = ts.ShowGripHandle;
                Width = ts.Width;
                Height = ts.Height;
            }
        }

        public static ToolStripDemoConfig FromControl(FluentToolStrip ts)
        {
            var cfg = new ToolStripDemoConfig();
            cfg.ReadFrom(ts);
            return cfg;
        }
    }

    public class StatusStripDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 外观
        [PropertyCategory("外观")]
        [PropertyDisplayName("握柄样式")]
        [PropertyDescription("Visible=显示 | Hidden=隐藏")]
        public StatusStripGripStyle GripStyle { get; set; } = StatusStripGripStyle.Visible;

        [PropertyCategory("外观")]
        [PropertyDisplayName("握柄边距")]
        public int GripMargin { get; set; } = 4;

        [PropertyCategory("外观")]
        [PropertyDisplayName("显示项工具提示")]
        public bool ShowItemToolTips { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 400;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 26;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentStatusStrip ss)
            {
                ss.Enabled = Enabled;
                ss.GripStyle = GripStyle;
                ss.ShowItemToolTips = ShowItemToolTips;
                ss.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentStatusStrip ss)
            {
                Enabled = ss.Enabled;
                GripStyle = ss.GripStyle;
                ShowItemToolTips = ss.ShowItemToolTips;
                Width = ss.Width;
                Height = ss.Height;
            }
        }

        public static StatusStripDemoConfig FromControl(FluentStatusStrip ss)
        {
            var cfg = new StatusStripDemoConfig();
            cfg.ReadFrom(ss);
            return cfg;
        }
    }

    #endregion

    #region 复合控件配置

    public class CardDemoConfig : ControlDemoConfigBase
    {
        // 内容
        [PropertyCategory("内容")]
        [PropertyDisplayName("标题")]
        public string Title { get; set; } = "Card Title";

        [PropertyCategory("内容")]
        [PropertyDisplayName("副标题")]
        public string Subtitle { get; set; } = "";

        [PropertyCategory("内容")]
        [PropertyDisplayName("卡片图片")]
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image CardImage { get; set; }

        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("布局模式")]
        [PropertyDescription("LeftImageRightContent=左图右文 | TopImageBottomContent=上图下文 | ContentOnly=纯内容 | ImageOnly=纯图片")]
        public CardLayout Layout { get; set; } = CardLayout.LeftImageRightContent;

        [PropertyCategory("布局")]
        [PropertyDisplayName("图片大小模式")]
        [PropertyDescription("Auto=自适应 | Fixed=固定大小 | Fill=填充")]
        public CardImageSizeMode ImageSizeMode { get; set; } = CardImageSizeMode.Auto;

        [PropertyCategory("布局")]
        [PropertyDisplayName("图片大小")]
        [PropertyDescription("固定模式下的图片尺寸")]
        public Size ImageSize { get; set; } = new Size(120, 120);

        [PropertyCategory("布局")]
        [PropertyDisplayName("元素间距")]
        [PropertyDescription("卡片内各元素之间的间距")]
        public int ElementSpacing { get; set; } = 12;

        [PropertyCategory("布局")]
        [PropertyDisplayName("内边距")]
        public Padding ContentPadding { get; set; } = new Padding(16);

        // 颜色
        [PropertyCategory("颜色")]
        [PropertyDisplayName("标题颜色")]
        public Color TitleColor { get; set; } = Color.Empty;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("副标题颜色")]
        public Color SubtitleColor { get; set; } = Color.Empty;

        // 边框
        [PropertyCategory("边框")]
        [PropertyDisplayName("显示边框")]
        public bool ShowBorder { get; set; } = true;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框宽度")]
        public int BorderWidth { get; set; } = 1;

        [PropertyCategory("边框")]
        [PropertyDisplayName("边框颜色")]
        public Color BorderColor { get; set; } = Color.Empty;

        [PropertyCategory("边框")]
        [PropertyDisplayName("圆角半径")]
        public int CornerRadius { get; set; } = 4;

        // 阴影
        [PropertyCategory("阴影")]
        [PropertyDisplayName("显示阴影")]
        public bool ShowShadow { get; set; } = true;

        [PropertyCategory("阴影")]
        [PropertyDisplayName("阴影级别")]
        [PropertyDescription("0~24, 数值越大阴影越明显")]
        public int ShadowLevel { get; set; } = 2;

        // 操作按钮
        [PropertyCategory("操作按钮")]
        [PropertyDisplayName("按钮高度")]
        public int ActionButtonHeight { get; set; } = 32;

        [PropertyCategory("操作按钮")]
        [PropertyDisplayName("按钮宽度")]
        public int ActionButtonWidth { get; set; } = 80;

        [PropertyCategory("操作按钮")]
        [PropertyDisplayName("按钮最小宽度")]
        public int ActionButtonMinWidth { get; set; } = 60;

        [PropertyCategory("操作按钮")]
        [PropertyDisplayName("按钮间距")]
        public int ActionSpacing { get; set; } = 8;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 280;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 160;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentCard card)
            {
                card.Title = Title;
                card.Subtitle = Subtitle;
                card.CardImage = CardImage;
                card.Layout = Layout;
                card.ImageSizeMode = ImageSizeMode;
                card.ImageSize = ImageSize;
                card.ElementSpacing = ElementSpacing;
                card.Padding = ContentPadding;
                if (TitleColor != Color.Empty)
                {
                    card.TitleColor = TitleColor;
                }

                if (SubtitleColor != Color.Empty)
                {
                    card.SubtitleColor = SubtitleColor;
                }

                card.ShowBorder = ShowBorder;
                card.BorderWidth = BorderWidth;
                if (BorderColor != Color.Empty)
                {
                    card.BorderColor = BorderColor;
                }

                card.CornerRadius = CornerRadius;
                card.ShowShadow = ShowShadow;
                card.ShadowLevel = ShadowLevel;
                card.ActionButtonHeight = ActionButtonHeight;
                card.ActionButtonMinWidth = ActionButtonMinWidth;
                card.ActionSpacing = ActionSpacing;
                card.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentCard card)
            {
                Title = card.Title;
                Subtitle = card.Subtitle;
                CardImage = card.CardImage;
                Layout = card.Layout;
                ImageSizeMode = card.ImageSizeMode;
                ImageSize = card.ImageSize;
                ElementSpacing = card.ElementSpacing;
                ContentPadding = card.Padding;
                TitleColor = card.TitleColor;
                SubtitleColor = card.SubtitleColor;
                ShowBorder = card.ShowBorder;
                BorderWidth = card.BorderWidth;
                BorderColor = card.BorderColor;
                CornerRadius = card.CornerRadius;
                ShowShadow = card.ShowShadow;
                ShadowLevel = card.ShadowLevel;
                ActionButtonHeight = card.ActionButtonHeight;
                ActionButtonMinWidth = card.ActionButtonMinWidth;
                ActionSpacing = card.ActionSpacing;
                Width = card.Width;
                Height = card.Height;
            }
        }

        public static CardDemoConfig FromControl(FluentCard card)
        {
            var cfg = new CardDemoConfig();
            cfg.ReadFrom(card);
            return cfg;
        }
    }

    public class FileSelectDemoConfig : ControlDemoConfigBase
    {
        // 基本属性
        [PropertyCategory("基本属性")]
        [PropertyDisplayName("标签文本")]
        [PropertyDescription("左侧标签显示的文字")]
        public string LabelText { get; set; } = "选择文件: ";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("显示标签")]
        public bool ShowLabel { get; set; } = true;

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("选择按钮文本")]
        public string SelectButtonText { get; set; } = "选择文件";

        [PropertyCategory("基本属性")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        // 选择行为
        [PropertyCategory("选择行为")]
        [PropertyDisplayName("文件过滤器")]
        [PropertyDescription("文件选择对话框的过滤器, 如：图片|*.png;*.jpg|所有文件|*.*")]
        public string Filter { get; set; } = "所有文件|*.*";

        [PropertyCategory("选择行为")]
        [PropertyDisplayName("允许多选")]
        public bool MultiSelect { get; set; } = true;

        [PropertyCategory("选择行为")]
        [PropertyDisplayName("最大文件数")]
        [PropertyDescription("允许选择的最大文件数量")]
        public int MaxFileCount { get; set; } = int.MaxValue;

        // 布局
        [PropertyCategory("布局")]
        [PropertyDisplayName("布局模式")]
        [PropertyDescription("SingleLine=单行 | MultiLine=多行")]
        public FileSelectLayout SelectLayout { get; set; } = FileSelectLayout.SingleLine;

        [PropertyCategory("布局")]
        [PropertyDisplayName("文件项大小")]
        [PropertyDescription("文件项的默认大小(自适应模式下仅高度生效)")]
        public Size FileItemSize { get; set; } = new Size(200, 28);

        [PropertyCategory("布局")]
        [PropertyDisplayName("自适应文件项")]
        [PropertyDescription("是否根据文件名长度自动调整文件项宽度")]
        public bool AutoSizeFileItems { get; set; } = true;

        [PropertyCategory("布局")]
        [PropertyDisplayName("选择按钮大小")]
        public Size SelectButtonSize { get; set; } = new Size(100, 30);

        // 显示
        [PropertyCategory("显示")]
        [PropertyDisplayName("显示文件大小")]
        [PropertyDescription("文件项中是否显示文件大小信息")]
        public bool ShowFileSize { get; set; } = true;

        // 尺寸
        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 320;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 36;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            if (ctrl is FluentFileSelect fs)
            {
                fs.Enabled = Enabled;
                fs.LabelText = LabelText;
                fs.ShowLabel = ShowLabel;
                fs.SelectButtonText = SelectButtonText;
                fs.Filter = Filter;
                fs.MultiSelect = MultiSelect;
                fs.MaxFileCount = MaxFileCount;
                fs.SelectLayout = SelectLayout;
                fs.FileItemSize = FileItemSize;
                fs.AutoSizeFileItems = AutoSizeFileItems;
                fs.SelectButtonSize = SelectButtonSize;
                fs.ShowFileSize = ShowFileSize;
                fs.Size = new Size(Width, Height);
            }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            if (ctrl is FluentFileSelect fs)
            {
                Enabled = fs.Enabled;
                LabelText = fs.LabelText;
                ShowLabel = fs.ShowLabel;
                SelectButtonText = fs.SelectButtonText;
                Filter = fs.Filter;
                MultiSelect = fs.MultiSelect;
                MaxFileCount = fs.MaxFileCount;
                SelectLayout = fs.SelectLayout;
                FileItemSize = fs.FileItemSize;
                AutoSizeFileItems = fs.AutoSizeFileItems;
                SelectButtonSize = fs.SelectButtonSize;
                ShowFileSize = fs.ShowFileSize;
                Width = fs.Width;
                Height = fs.Height;
            }
        }

        public static FileSelectDemoConfig FromControl(FluentFileSelect fs)
        {
            var cfg = new FileSelectDemoConfig();
            cfg.ReadFrom(fs);
            return cfg;
        }
    }

    #endregion
}
