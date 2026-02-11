using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using FluentControls.IconFonts;
using System.Diagnostics;
using Infrastructure;
using System.ComponentModel.Design;

namespace FluentControls.Controls
{
    /// <summary>
    /// 字体图标控件
    /// </summary>
    public class FluentFontIcon : Control
    {
        private string fontFamily = "";
        private string iconCharName = "";
        private Color iconColor = Color.Black;
        private float iconSize = 24f;
        private float rotation = 0f;
        private bool flipHorizontal = false;
        private bool flipVertical = false;

        private Image iconImage = null;

        private bool isDesignMode;

        public FluentFontIcon()
        {
            Size = new Size(32, 32);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;

            isDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
        }

        #region 属性

        /// <summary>
        /// 字体族名称
        /// </summary>
        [Category("Icon")]
        [Description("字体族名称")]
        [Editor(typeof(IconFontFamilyEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FontFamily
        {
            get => fontFamily;
            set
            {
                if (fontFamily != value)
                {
                    fontFamily = value;
                    iconCharName = "";  // 重置图标选择

                    if (isDesignMode)
                    {
                        // 设计时清空图像，等待选择新的 IconChar
                        iconImage?.Dispose();
                        iconImage = null;
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 图标字符名称
        /// </summary>
        [Category("Icon")]
        [Description("图标字符名称")]
        [Editor(typeof(IconCharEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconChar
        {
            get => iconCharName;
            set
            {
                if (iconCharName != value)
                {
                    iconCharName = value;

                    if (isDesignMode)
                    {
                        GenerateIconImage();
                    }
                }
            }
        }

        /// <summary>
        /// 图标颜色
        /// </summary>
        [Category("Icon")]
        [Description("图标颜色")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color IconColor
        {
            get => iconColor;
            set
            {
                if (iconColor != value)
                {
                    iconColor = value;

                    if (isDesignMode)
                    {
                        GenerateIconImage();
                    }
                }
            }
        }

        /// <summary>
        /// 图标大小
        /// </summary>
        [Category("Icon")]
        [Description("图标大小")]
        [DefaultValue(24f)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float IconSize
        {
            get => iconSize;
            set
            {
                if (iconSize != value && value > 0)
                {
                    iconSize = value;

                    if (isDesignMode)
                    {
                        GenerateIconImage();
                    }
                }
            }
        }

        /// <summary>
        /// 旋转角度
        /// </summary>
        [Category("Icon")]
        [Description("旋转角度")]
        [DefaultValue(0f)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;

                    if (isDesignMode)
                    {
                        GenerateIconImage();
                    }
                }
            }
        }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [Category("Icon")]
        [Description("水平翻转")]
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FlipHorizontal
        {
            get => flipHorizontal;
            set
            {
                if (flipHorizontal != value)
                {
                    flipHorizontal = value;

                    if (isDesignMode)
                    {
                        GenerateIconImage();
                    }
                }
            }
        }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [Category("Icon")]
        [Description("垂直翻转")]
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FlipVertical
        {
            get => flipVertical;
            set
            {
                if (flipVertical != value)
                {
                    flipVertical = value;

                    if (isDesignMode)
                    {
                        GenerateIconImage();
                    }
                }
            }
        }

        /// <summary>
        /// 图标图像
        /// </summary>
        [Category("Icon")]
        [Description("图标图像")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image IconImage
        {
            get => iconImage;
            set
            {
                if (iconImage != value)
                {
                    iconImage?.Dispose();
                    iconImage = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 生成图标图像
        /// </summary>
        private void GenerateIconImage()
        {
            // 清理旧图像
            if (iconImage != null)
            {
                iconImage.Dispose();
                iconImage = null;
            }

            if (string.IsNullOrEmpty(fontFamily) || string.IsNullOrEmpty(iconCharName))
            {
                Invalidate();
                return;
            }

            try
            {
                var provider = IconFontManager.Instance.GetProvider(fontFamily);
                if (provider == null)
                {
                    Debug.WriteLine($"找不到字体提供者: {fontFamily}");
                    Invalidate();
                    return;
                }

                // 获取枚举类型
                var enumType = provider.GetIconEnumType();
                Image generatedImage = null;

                if (enumType != null)
                {
                    // 尝试解析为枚举值
                    if (enumType.TryParseEnum(iconCharName, true, out var enumValue))
                    {
                        generatedImage = provider.GetIcon((Enum)enumValue, iconSize, iconColor, rotation);
                    }
                    else
                    {
                        // 如果解析失败，尝试通过名称获取
                        generatedImage = provider.GetIcon(iconCharName, iconSize, iconColor, rotation);
                    }
                }
                else
                {
                    // 没有枚举类型，直接通过名称获取
                    generatedImage = provider.GetIcon(iconCharName, iconSize, iconColor, rotation);
                }

                if (generatedImage != null)
                {
                    // 应用翻转
                    if (flipHorizontal || flipVertical)
                    {
                        var flipType = RotateFlipType.RotateNoneFlipNone;

                        if (flipHorizontal && flipVertical)
                        {
                            flipType = RotateFlipType.RotateNoneFlipXY;
                        }
                        else if (flipHorizontal)
                        {
                            flipType = RotateFlipType.RotateNoneFlipX;
                        }
                        else if (flipVertical)
                        {
                            flipType = RotateFlipType.RotateNoneFlipY;
                        }

                        generatedImage.RotateFlip(flipType);
                    }

                    // 保存生成的图像
                    iconImage = generatedImage;

                    Debug.WriteLine($"图标生成成功: {fontFamily} - {iconCharName}");

                    // 通知设计器属性已更改，触发序列化
                    if (isDesignMode)
                    {
                        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host != null)
                        {
                            var componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            if (componentChangeService != null)
                            {
                                try
                                {
                                    var propertyDescriptor = TypeDescriptor.GetProperties(this)["IconImage"];
                                    componentChangeService.OnComponentChanging(this, propertyDescriptor);
                                    componentChangeService.OnComponentChanged(this, propertyDescriptor, null, iconImage);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"通知设计器失败: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生成图标失败: {ex.Message}");
                Debug.WriteLine($"FontFamily: {fontFamily}, IconChar: {iconCharName}");
            }

            Invalidate();
        }

        /// <summary>
        /// 设置图标
        /// </summary>
        public void SetIcon(string fontFamily, Enum iconEnum, float size, Color color, float rotation = 0)
        {
            this.fontFamily = fontFamily;
            this.iconCharName = iconEnum.ToString();
            this.iconSize = size;
            this.iconColor = color;
            this.rotation = rotation;

            GenerateIconImage();
        }

        /// <summary>
        /// 设置图标
        /// </summary>
        public void SetIcon(string fontFamily, string iconName, float size, Color color, float rotation = 0)
        {
            this.fontFamily = fontFamily;
            this.iconCharName = iconName;
            this.iconSize = size;
            this.iconColor = color;
            this.rotation = rotation;

            GenerateIconImage();
        }

        /// <summary>
        /// 直接设置图标图像
        /// </summary>
        public void SetIconImage(Image image)
        {
            iconImage?.Dispose();
            iconImage = image != null ? new Bitmap(image) : null;
            Invalidate();
        }

        /// <summary>
        /// 获取当前图标信息
        /// </summary>
        public string GetIconInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"FontFamily: {fontFamily}");
            sb.AppendLine($"IconChar: {iconCharName}");
            sb.AppendLine($"IconSize: {iconSize}");
            sb.AppendLine($"IconColor: {iconColor}");
            sb.AppendLine($"HasIconImage: {iconImage != null}");
            if (iconImage != null)
            {
                sb.AppendLine($"ImageSize: {iconImage.Width}x{iconImage.Height}");
            }
            return sb.ToString();
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // 绘制背景
            if (BackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }

            // 绘制图标
            if (iconImage != null)
            {
                var x = (Width - iconImage.Width) / 2;
                var y = (Height - iconImage.Height) / 2;
                g.DrawImage(iconImage, x, y, iconImage.Width, iconImage.Height);
            }
            else if (isDesignMode)
            {
                // 设计时占位符
                DrawPlaceholder(g);
            }
        }

        private void DrawPlaceholder(Graphics g)
        {
            using (var pen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            using (var font = new Font("Arial", 8f))
            using (var brush = new SolidBrush(Color.Gray))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

                var text = "Icon";
                var textSize = g.MeasureString(text, font);
                var x = (Width - textSize.Width) / 2;
                var y = (Height - textSize.Height) / 2;

                g.DrawString(text, font, brush, x, y);
            }
        }

        #endregion

        #region 序列化支持

        /// <summary>
        /// 判断是否应该序列化 IconImage 属性
        /// </summary>
        private bool ShouldSerializeIconImage()
        {
            return iconImage != null;
        }

        /// <summary>
        /// 重置 IconImage 属性
        /// </summary>
        private void ResetIconImage()
        {
            iconImage?.Dispose();
            iconImage = null;
            Invalidate();
        }

        #endregion

        #region Designer 服务

        /// <summary>
        /// 获取设计器服务
        /// </summary>
        private object GetService(Type serviceType)
        {
            if (Site != null)
            {
                return Site.GetService(serviceType);
            }
            return null;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                iconImage?.Dispose();
                iconImage = null;
            }
            base.Dispose(disposing);
        }
    }

    #region 设计时支持

    /// <summary>
    /// 字体族选择编辑器
    /// </summary>
    public class IconFontFamilyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (editorService == null)
            {
                return value;
            }

            var listBox = new ListBox();
            listBox.Click += (s, e) => editorService.CloseDropDown();

            var providers = IconFontManager.Instance.GetAllProviders();
            foreach (var p in providers)
            {
                listBox.Items.Add(p.FontFamilyName);
            }

            if (value != null)
            {
                listBox.SelectedItem = value;
            }

            editorService.DropDownControl(listBox);

            return listBox.SelectedItem ?? value;
        }
    }

    /// <summary>
    /// 图标字符选择编辑器
    /// </summary>
    public class IconCharEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (editorService == null)
            {
                return value;
            }

            var control = context.Instance as FluentFontIcon;
            if (control == null || string.IsNullOrEmpty(control.FontFamily))
            {
                MessageBox.Show("请先选择字体族(FontFamily)", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return value;
            }

            var enumType = IconFontManager.Instance.GetIconEnumType(control.FontFamily);
            if (enumType == null)
            {
                MessageBox.Show("该字体族没有对应的图标枚举类型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return value;
            }

            var iconProvider = IconFontManager.Instance.GetProvider(control.FontFamily);

            // 创建选择面板
            var selectorPanel = new IconCharSelectorPanel(iconProvider, enumType, value as string);
            selectorPanel.ItemSelected += (s, selectedName) =>
            {
                value = selectedName;  // 返回枚举名称字符串
                editorService.CloseDropDown();
            };

            editorService.DropDownControl(selectorPanel);

            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Context?.Instance is FluentFontIcon control &&
                !string.IsNullOrEmpty(control.FontFamily) &&
                !string.IsNullOrEmpty(control.IconChar))
            {
                var provider = IconFontManager.Instance.GetProvider(control.FontFamily);
                if (provider != null)
                {
                    try
                    {
                        var enumType = provider.GetIconEnumType();
                        Image icon = null;

                        if (enumType != null && enumType.TryParseEnum(control.IconChar, true, out var enumValue))
                        {
                            icon = provider.GetIcon((Enum)enumValue, 16f, Color.Black);
                        }
                        else
                        {
                            icon = provider.GetIcon(control.IconChar, 16f, Color.Black);
                        }

                        if (icon != null)
                        {
                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            e.Graphics.DrawImage(icon, e.Bounds);
                            icon.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"PaintValue 失败: {ex.Message}");
                    }
                }
            }
        }
    }


    /// <summary>
    /// 图标字符选择面板
    /// </summary>
    [ToolboxItem(false)]
    public class IconCharSelectorPanel : UserControl
    {
        private ListBox listBox;
        private TextBox searchBox;
        private PictureBox previewBox;
        private Label lblDescription;
        private IIconFontProvider provider;
        private Type enumType;
        private Array enumValues;

        public event EventHandler<string> ItemSelected;  // 返回枚举名称字符串

        public IconCharSelectorPanel(IIconFontProvider provider, Type enumType, string currentValue)
        {
            this.provider = provider;
            this.enumType = enumType;
            this.enumValues = Enum.GetValues(enumType);

            InitializeComponent();
            LoadIcons(currentValue);
        }

        private void InitializeComponent()
        {
            Size = new Size(400, 450);
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = SystemColors.Window;
            Padding = new Padding(5);

            // 搜索框
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30
            };

            searchBox = new TextBox
            {
                Dock = DockStyle.Fill
            };
            searchBox.TextChanged += SearchBox_TextChanged;

            searchPanel.Controls.Add(searchBox);

            // 列表框
            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28
            };
            listBox.DrawItem += ListBox_DrawItem;
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBox.Click += (s, e) =>
            {
                if (listBox.SelectedItem is IconEnumItem item)
                {
                    ItemSelected?.Invoke(this, item.Name);  // 返回枚举名称字符串
                }
            };

            // 预览区
            var previewPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            var previewContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            previewContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            previewContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            previewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblDescription = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 8.5f),
                Padding = new Padding(5, 0, 0, 0)
            };

            previewContainer.Controls.Add(previewBox, 0, 0);
            previewContainer.Controls.Add(lblDescription, 1, 0);
            previewPanel.Controls.Add(previewContainer);

            Controls.Add(listBox);
            Controls.Add(searchPanel);
            Controls.Add(previewPanel);
        }

        private void LoadIcons(string currentValue)
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();

            foreach (Enum enumValue in enumValues)
            {
                var item = new IconEnumItem
                {
                    EnumValue = enumValue,
                    Name = enumValue.ToString(),
                    Description = GetEnumDescription(enumValue),
                    Unicode = $"U+{(int)(object)enumValue:X4}"
                };

                listBox.Items.Add(item);

                if (!string.IsNullOrEmpty(currentValue) && item.Name.Equals(currentValue, StringComparison.OrdinalIgnoreCase))
                {
                    listBox.SelectedItem = item;
                }
            }

            listBox.EndUpdate();
        }

        private string GetEnumDescription(Enum enumValue)
        {
            var field = enumType.GetField(enumValue.ToString());

            // 首先尝试获取 Summary 特性(从生成的代码注释)
            var summaryAttr = field?.GetCustomAttributes(false)
                .FirstOrDefault(a => a.GetType().Name == "SummaryAttribute");

            if (summaryAttr != null)
            {
                var prop = summaryAttr.GetType().GetProperty("Text");
                if (prop != null)
                {
                    var text = prop.GetValue(summaryAttr) as string;
                    if (!string.IsNullOrEmpty(text))
                    {
                        // 提取第一行(通常是描述)
                        var lines = text.Split('\n');
                        if (lines.Length > 0)
                        {
                            return lines[0].Trim().TrimStart('/', ' ');
                        }
                    }
                }
            }

            // 尝试 Description 特性
            var attributes = field?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attributes != null && attributes.Length > 0)
            {
                var lines = attributes[0].Description.Split('\n');
                if (lines.Length > 0)
                {
                    return lines[0].Trim();
                }
            }

            return enumValue.ToString();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var filter = searchBox.Text.Trim();

            listBox.BeginUpdate();
            listBox.Items.Clear();

            foreach (Enum enumValue in enumValues)
            {
                var name = enumValue.ToString();
                var description = GetEnumDescription(enumValue);

                if (string.IsNullOrEmpty(filter) ||
                    name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    listBox.Items.Add(new IconEnumItem
                    {
                        EnumValue = enumValue,
                        Name = name,
                        Description = description,
                        Unicode = $"U+{(int)(object)enumValue:X4}"
                    });
                }
            }

            listBox.EndUpdate();
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();

            var item = listBox.Items[e.Index] as IconEnumItem;
            if (item != null)
            {
                // 绘制小图标
                try
                {
                    var icon = provider.GetIcon(item.EnumValue, 20f, Color.Black);
                    if (icon != null)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        // 绘制时留出边距避免裁切
                        var iconRect = new Rectangle(e.Bounds.Left + 6, e.Bounds.Top + 4, 20, 20);
                        e.Graphics.DrawImage(icon, iconRect);
                        icon.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"绘制图标失败: {ex.Message}");
                }

                // 绘制文本
                using (var brush = new SolidBrush(e.ForeColor))
                {
                    var textRect = new Rectangle(e.Bounds.Left + 32, e.Bounds.Top + 2, e.Bounds.Width - 32, e.Bounds.Height);
                    e.Graphics.DrawString(item.Name, e.Font, brush, textRect);

                    if (!string.IsNullOrEmpty(item.Description) && item.Description != item.Name)
                    {
                        using (var grayBrush = new SolidBrush(Color.Gray))
                        using (var smallFont = new Font(e.Font.FontFamily, 7.5f))
                        {
                            var descRect = new Rectangle(e.Bounds.Left + 32, e.Bounds.Top + 14, e.Bounds.Width - 32, e.Bounds.Height);
                            e.Graphics.DrawString(item.Description, smallFont, grayBrush, descRect);
                        }
                    }
                }
            }

            e.DrawFocusRectangle();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedItem is IconEnumItem item)
            {
                try
                {
                    var icon = provider.GetIcon(item.EnumValue, 72f, Color.Black);

                    previewBox.Image?.Dispose();
                    previewBox.Image = icon;

                    lblDescription.Text = $"名称: {item.Name}\n" +
                                        $"Unicode: {item.Unicode}\n" +
                                        $"描述: {item.Description}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"生成预览图标失败: {ex.Message}");
                }
            }
        }

        private class IconEnumItem
        {
            public Enum EnumValue { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Unicode { get; set; }

            public override string ToString() => Name;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                previewBox?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    #endregion

    #region 控件扩展

    public static class IconFontControlExtension
    {
        /// <summary>
        /// 运行时通过 IconFontManager 设置图标
        /// </summary>
        public static void SetIconRuntime(this FluentFontIcon control, string fontFamily, string iconChar, float size, Color color, float rotation = 0)
        {
            try
            {
                var provider = IconFontManager.Instance.GetProvider(fontFamily);
                if (provider == null)
                {
                    Debug.WriteLine($"找不到字体提供者: {fontFamily}");
                    return;
                }

                var enumType = provider.GetIconEnumType();
                Image icon = null;

                if (enumType != null && enumType.TryParseEnum(iconChar, true, out var enumValue))
                {
                    icon = provider.GetIcon((Enum)enumValue, size, color, rotation);
                }
                else
                {
                    icon = provider.GetIcon(iconChar, size, color, rotation);
                }

                if (icon != null)
                {
                    control.SetIconImage(icon);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"运行时设置图标失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 运行时通过枚举设置图标
        /// </summary>
        public static void SetIconRuntime(this FluentFontIcon control, string fontFamily, Enum iconEnum, float size, Color color, float rotation = 0)
        {
            try
            {
                var provider = IconFontManager.Instance.GetProvider(fontFamily);
                if (provider == null)
                {
                    Debug.WriteLine($"找不到字体提供者: {fontFamily}");
                    return;
                }

                var icon = provider.GetIcon(iconEnum, size, color, rotation);
                if (icon != null)
                {
                    control.SetIconImage(icon);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"运行时设置图标失败: {ex.Message}");
            }
        }
    }

    #endregion
}
