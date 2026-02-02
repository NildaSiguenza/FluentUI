using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Themes;
using Infrastructure;

namespace FluentControls.Controls
{
    public class FluentPropertyGrid : FluentControlBase
    {
        private object selectedObject;
        private IList selectedObjects;
        private int currentObjectIndex = 0;

        private List<PropertyItem> propertyItems;
        private Dictionary<string, List<PropertyItem>> categorizedItems;

        // UI组件
        private Panel headerPanel;
        private Label titleLabel;
        private FluentButton btnPrevious;
        private FluentButton btnNext;
        private Label indexLabel;

        private Panel contentPanel;
        private Panel footerPanel;
        private FluentButton btnSave;
        private FluentButton btnRefresh;

        private bool hasUnsavedChanges = false;

        #region 构造函数

        public FluentPropertyGrid()
        {
            this.Size = new Size(400, 500);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 头部面板
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            titleLabel = new Label
            {
                Text = "属性",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            headerPanel.Controls.Add(titleLabel);

            // 导航按钮容器 - 使用FlowLayoutPanel确保按钮正确布局
            var navigationPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Right,
                Width = 200,
                Height = 40,
                Padding = new Padding(5),
                WrapContents = false
            };

            btnNext = new FluentButton
            {
                Text = "▶",
                Size = new Size(30, 30),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = false,
                Margin = new Padding(2, 0, 2, 0)
            };
            btnNext.Click += BtnNext_Click;

            btnPrevious = new FluentButton
            {
                Text = "◀",
                Size = new Size(30, 30),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = false,
                Margin = new Padding(2, 0, 2, 0)
            };
            btnPrevious.Click += BtnPrevious_Click;

            indexLabel = new Label
            {
                Text = "",
                Size = new Size(100, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9),
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(2, 0, 5, 0)
            };

            // 按正确的顺序添加(RightToLeft，所以顺序相反)
            navigationPanel.Controls.Add(btnNext);
            navigationPanel.Controls.Add(btnPrevious);
            navigationPanel.Controls.Add(indexLabel);

            headerPanel.Controls.Add(navigationPanel);

            // 内容面板
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);

            // 底部面板
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var buttonContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 9, 10, 9),
                WrapContents = false
            };

            btnSave = new FluentButton
            {
                Text = "保存",
                Size = new Size(80, 32),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Primary,
                CornerRadius = 0,
                Margin = new Padding(2, 0, 2, 0)
            };
            btnSave.Click += BtnSave_Click;

            btnRefresh = new FluentButton
            {
                Text = "刷新",
                Size = new Size(80, 32),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                Margin = new Padding(2, 0, 2, 0)
            };
            btnRefresh.Click += BtnRefresh_Click;

            buttonContainer.Controls.Add(btnSave);
            buttonContainer.Controls.Add(btnRefresh);

            footerPanel.Controls.Add(buttonContainer);
            this.Controls.Add(footerPanel);
        }

        #endregion

        #region 属性

        [Browsable(false)]
        public object SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                selectedObjects = null;
                currentObjectIndex = 0;
                LoadProperties();
                UpdateNavigationButtons();
            }
        }

        [Browsable(false)]
        public IList SelectedObjects
        {
            get => selectedObjects;
            set
            {
                selectedObjects = value;
                selectedObject = null;
                currentObjectIndex = 0;
                LoadProperties();
                UpdateNavigationButtons();
            }
        }

        [Category("Fluent")]
        [DefaultValue(0)]
        public int SelectedObjectIndex
        {
            get => currentObjectIndex;
            set
            {
                if (selectedObjects != null && value >= 0 && value < selectedObjects.Count)
                {
                    SaveCurrentObject();
                    currentObjectIndex = value;
                    LoadProperties();
                    UpdateNavigationButtons();
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(true)]
        public bool ShowNavigationButtons { get; set; } = true;

        [Category("Fluent")]
        [DefaultValue(true)]
        public bool ShowActionButtons { get; set; } = true;

        [Category("Fluent")]
        [DefaultValue(true)]
        public bool GroupByCategory { get; set; } = true;

        #endregion

        #region 属性加载

        private void LoadProperties()
        {
            contentPanel.Controls.Clear();
            propertyItems = new List<PropertyItem>();
            categorizedItems = new Dictionary<string, List<PropertyItem>>();

            var targetObject = GetCurrentObject();
            if (targetObject == null)
            {
                return;
            }

            // 获取所有属性
            var properties = targetObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // 检查是否忽略
                if (prop.GetCustomAttribute<PropertyIgnoreEditAttribute>() != null)
                {
                    continue;
                }

                var propertyItem = new PropertyItem(prop, targetObject);
                propertyItems.Add(propertyItem);

                // 分类
                if (!categorizedItems.ContainsKey(propertyItem.Category))
                {
                    categorizedItems[propertyItem.Category] = new List<PropertyItem>();
                }
                categorizedItems[propertyItem.Category].Add(propertyItem);
            }

            // 渲染属性
            RenderProperties();
        }

        private void RenderProperties()
        {
            contentPanel.SuspendLayout();

            int y = 5; // 减少顶部边距
            const int ROW_HEIGHT = 28; // 减少行高
            const int LABEL_WIDTH = 140; // 稍微减少标签宽度
            const int EDITOR_WIDTH = 210; // 增加编辑器宽度
            const int PADDING = 8; // 减少边距
            const int CATEGORY_SPACING = 8; // 减少分类间距

            if (GroupByCategory)
            {
                foreach (var category in categorizedItems.Keys.OrderBy(k => k))
                {
                    // 分类标题 - 使用更紧凑的样式
                    var categoryLabel = new Label
                    {
                        Text = category,
                        Location = new Point(PADDING, y),
                        Size = new Size(LABEL_WIDTH + EDITOR_WIDTH + 10, 22), // 减少高度
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), // 减小字体
                        BackColor = Color.FromArgb(235, 235, 235),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(5, 0, 0, 0)
                    };
                    contentPanel.Controls.Add(categoryLabel);
                    y += 24; // 减少间距

                    // 分类下的属性
                    foreach (var item in categorizedItems[category])
                    {
                        RenderPropertyItem(item, y, LABEL_WIDTH, EDITOR_WIDTH, PADDING);
                        y += ROW_HEIGHT + 2;
                    }

                    y += CATEGORY_SPACING; // 分类间距
                }
            }
            else
            {
                foreach (var item in propertyItems)
                {
                    RenderPropertyItem(item, y, LABEL_WIDTH, EDITOR_WIDTH, PADDING);
                    y += ROW_HEIGHT + 2;
                }
            }

            // 设置自动滚动最小尺寸
            if (y > contentPanel.Height)
            {
                contentPanel.AutoScrollMinSize = new Size(0, y + 10);
            }

            contentPanel.ResumeLayout();
        }

        private void RenderPropertyItem(PropertyItem item, int y, int labelWidth, int editorWidth, int padding)
        {
            // 属性名标签 - 使用更小的字体
            var nameLabel = new Label
            {
                Text = item.DisplayName,
                Location = new Point(padding + 5, y + 3), // 调整位置
                Size = new Size(labelWidth - 5, 22), // 调整大小
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5f) // 减小字体
            };

            if (!string.IsNullOrEmpty(item.Description))
            {
                var toolTip = new ToolTip();
                toolTip.SetToolTip(nameLabel, item.Description);
            }

            contentPanel.Controls.Add(nameLabel);

            // 编辑器
            var editor = PropertyEditorFactory.CreateEditor(item);
            editor.Location = new Point(padding + labelWidth + 5, y);
            editor.Width = editorWidth;
            editor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // 添加锚定
            item.EditorControl = editor;

            contentPanel.Controls.Add(editor);
        }

        #endregion

        #region 对象获取

        private object GetCurrentObject()
        {
            if (selectedObject != null)
            {
                return selectedObject;
            }

            if (selectedObjects != null && currentObjectIndex >= 0 && currentObjectIndex < selectedObjects.Count)
            {
                return selectedObjects[currentObjectIndex];
            }

            return null;
        }

        #endregion

        #region 导航

        private void UpdateNavigationButtons()
        {
            bool hasMultiple = selectedObjects != null && selectedObjects.Count > 1;

            btnPrevious.Visible = ShowNavigationButtons && hasMultiple;
            btnNext.Visible = ShowNavigationButtons && hasMultiple;
            indexLabel.Visible = ShowNavigationButtons && hasMultiple;

            if (hasMultiple)
            {
                indexLabel.Text = $"{currentObjectIndex + 1} / {selectedObjects.Count}";
                btnPrevious.Enabled = currentObjectIndex > 0;
                btnNext.Enabled = currentObjectIndex < selectedObjects.Count - 1;
            }

            footerPanel.Visible = ShowActionButtons;
        }

        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (currentObjectIndex > 0)
            {
                SaveCurrentObject();
                currentObjectIndex--;
                LoadProperties();
                UpdateNavigationButtons();
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (selectedObjects != null && currentObjectIndex < selectedObjects.Count - 1)
            {
                SaveCurrentObject();
                currentObjectIndex++;
                LoadProperties();
                UpdateNavigationButtons();
            }
        }

        #endregion

        #region 保存和刷新

        public void SaveChanges()
        {
            SaveCurrentObject();
            hasUnsavedChanges = false;
        }

        private void SaveCurrentObject()
        {
            var targetObject = GetCurrentObject();
            if (targetObject == null)
            {
                return;
            }

            foreach (var item in propertyItems)
            {
                item.SaveValue(targetObject);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveChanges();
            FluentMessageManager.Instance.Success("保存成功");
        }

        public void RefreshValues()
        {
            var targetObject = GetCurrentObject();
            if (targetObject == null)
            {
                return;
            }

            foreach (var item in propertyItems)
            {
                item.UpdateValue(targetObject);

                // 更新编辑器显示
                UpdateEditorValue(item);
            }

            hasUnsavedChanges = false;
        }

        private void UpdateEditorValue(PropertyItem item)
        {
            if (item.EditorControl == null)
            {
                return;
            }

            try
            {
                // FluentTextBox
                if (item.EditorControl is FluentTextBox fluentTextBox)
                {
                    fluentTextBox.Text = item.Value?.ToString() ?? "";
                }
                // FluentCheckBox
                else if (item.EditorControl is FluentCheckBox fluentCheckBox)
                {
                    fluentCheckBox.Checked = item.Value is bool b && b;
                }
                // FluentComboBox
                else if (item.EditorControl is FluentComboBox fluentComboBox)
                {
                    fluentComboBox.SelectedItem = item.Value;
                }
                // FluentDateTimePicker
                else if (item.EditorControl is FluentDateTimePicker fluentDateTimePicker)
                {
                    if (item.Value is DateTime dt)
                    {
                        fluentDateTimePicker.Value = dt;
                    }
                }
                // FluentColorPicker
                else if (item.EditorControl is FluentColorPicker fluentColorPicker)
                {
                    if (item.Value is Color color)
                    {
                        fluentColorPicker.SelectedColor = color;
                    }
                }
                // Panel (复杂类型)
                else if (item.EditorControl is Panel panel)
                {
                    var label = panel.Controls.OfType<Label>().FirstOrDefault();
                    if (label != null)
                    {
                        if (item.PropertyType.IsGenericType &&
                            item.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            label.Text = PropertyEditorFactory.GetListDisplayText(item.Value);
                        }
                        else if (PropertyEditorFactory.IsStructureType(item.PropertyType))
                        {
                            label.Text = PropertyEditorFactory.GetObjectDisplayText(item.Value, item.PropertyType);
                        }
                        else
                        {
                            label.Text = PropertyEditorFactory.GetObjectDisplayText(item.Value, item.PropertyType);
                        }

                        // 强制刷新
                        panel.Refresh();
                    }
                }
                // 旧版控件兼容
                else if (item.EditorControl is TextBox textBox)
                {
                    textBox.Text = item.Value?.ToString() ?? "";
                }
                else if (item.EditorControl is NumericUpDown numericUpDown)
                {
                    numericUpDown.Value = Convert.ToDecimal(item.Value);
                }
                else if (item.EditorControl is CheckBox checkBox)
                {
                    checkBox.Checked = item.Value is bool b && b;
                }
                else if (item.EditorControl is ComboBox comboBox)
                {
                    comboBox.SelectedItem = item.Value;
                }
                else if (item.EditorControl is DateTimePicker dateTimePicker)
                {
                    if (item.Value is DateTime dt)
                    {
                        dateTimePicker.Value = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新编辑器值失败: {ex.Message}");
            }
        }

        private static string GetListDisplayText(object value)
        {
            if (value == null)
            {
                return "(null)";
            }

            if (value is IList list)
            {
                return $"(Collection) {list.Count} 项";
            }

            return value.ToString();
        }

        private static string GetObjectDisplayText(object value, Type type)
        {
            if (value == null)
            {
                return "(null)";
            }

            // 结构体类型显示其值
            if (PropertyEditorFactory.IsStructureType(type))
            {
                if (value is Rectangle rect)
                {
                    return $"{rect.X}, {rect.Y}, {rect.Width}, {rect.Height}";
                }

                if (value is Point point)
                {
                    return $"{point.X}, {point.Y}";
                }

                if (value is Size size)
                {
                    return $"{size.Width}, {size.Height}";
                }

                if (value is Padding padding)
                {
                    return $"{padding.Left}, {padding.Top}, {padding.Right}, {padding.Bottom}";
                }

                return value.ToString();
            }

            return $"({value.GetType().Name})";
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show("有未保存的更改，是否继续刷新?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            RefreshValues();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 由子控件绘制
        }

        protected override void DrawBorder(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        #endregion
    }

    #region 属性项

    public class PropertyItem
    {
        private object val;

        public event EventHandler ValueChanged;

        public PropertyItem()
        {
        }

        public PropertyItem(PropertyInfo propertyInfo, object targetObject)
        {
            PropertyInfo = propertyInfo;
            Name = propertyInfo.Name;
            PropertyType = propertyInfo.PropertyType;

            // 获取特性
            var displayNameAttr = propertyInfo.GetCustomAttribute<PropertyDisplayNameAttribute>();
            DisplayName = displayNameAttr?.DisplayName ?? Name;

            var categoryAttr = propertyInfo.GetCustomAttribute<PropertyCategoryAttribute>();
            Category = categoryAttr?.Category ?? "其他";

            var descriptionAttr = propertyInfo.GetCustomAttribute<PropertyDescriptionAttribute>();
            Description = descriptionAttr?.Description ?? "";

            var readOnlyAttr = propertyInfo.GetCustomAttribute<PropertyReadOnlyAttribute>();
            IsReadOnly = readOnlyAttr?.IsReadOnly ?? !propertyInfo.CanWrite;

            // 获取值
            try
            {
                Value = propertyInfo.GetValue(targetObject);
                OriginalValue = CloneValue(Value);
            }
            catch
            {
                Value = GetDefaultValue(PropertyType);
                OriginalValue = Value;
            }
        }

        public PropertyInfo PropertyInfo { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public Type PropertyType { get; set; }
        public bool IsReadOnly { get; set; }
        public object OriginalValue { get; set; }

        public object Value
        {
            get => val;
            set
            {
                if (!Equals(val, value))
                {
                    val = value;
                    OnValueChanged();
                }
            }
        }

        // UI相关
        public Control EditorControl { get; set; }
        public int Order { get; set; }

        public bool IsModified => !Equals(Value, OriginalValue);

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }


        public void UpdateValue(object targetObject)
        {
            try
            {
                Value = PropertyInfo.GetValue(targetObject);
                OriginalValue = CloneValue(Value);
            }
            catch
            {
                // 忽略错误
            }
        }

        public void SaveValue(object targetObject)
        {
            if (!IsReadOnly && PropertyInfo.CanWrite)
            {
                try
                {
                    PropertyInfo.SetValue(targetObject, Value);
                    OriginalValue = CloneValue(Value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"保存属性 {Name} 失败: {ex.Message}");
                }
            }
        }

        public void ResetValue()
        {
            Value = CloneValue(OriginalValue);
        }

        private object CloneValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is ICloneable cloneable)
            {
                return cloneable.Clone();
            }

            if (value.GetType().IsValueType || value is string)
            {
                return value;
            }

            // 对于其他类型，尝试序列化克隆(简化版本)
            return value;
        }

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }

    #endregion

    #region 属性编辑工厂

    public static class PropertyEditorFactory
    {
        public static Control CreateEditor(PropertyItem propertyItem)
        {
            var editorType = GetEditorType(propertyItem.PropertyType);
            Control editor = null;

            switch (editorType)
            {
                case PropertyEditorType.Boolean:
                    editor = CreateBooleanEditor(propertyItem);
                    break;

                case PropertyEditorType.Number:
                    editor = CreateNumberEditor(propertyItem);
                    break;

                case PropertyEditorType.Enum:
                    editor = CreateEnumEditor(propertyItem);
                    break;

                case PropertyEditorType.Color:
                    editor = CreateColorEditor(propertyItem);
                    break;

                case PropertyEditorType.DateTime:
                    editor = CreateDateTimeEditor(propertyItem);
                    break;

                case PropertyEditorType.List:
                    editor = CreateListEditor(propertyItem);
                    break;

                case PropertyEditorType.Object:
                    editor = CreateObjectEditor(propertyItem);
                    break;

                case PropertyEditorType.Text:
                default:
                    editor = CreateTextEditor(propertyItem);
                    break;
            }

            editor.Enabled = !propertyItem.IsReadOnly;
            return editor;
        }

        private static PropertyEditorType GetEditorType(Type propertyType)
        {
            // 处理Nullable类型
            Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlyingType == typeof(bool))
            {
                return PropertyEditorType.Boolean;
            }

            if (underlyingType.IsEnum)
            {
                return PropertyEditorType.Enum;
            }

            if (underlyingType == typeof(Color))
            {
                return PropertyEditorType.Color;
            }

            if (underlyingType == typeof(DateTime))
            {
                return PropertyEditorType.DateTime;
            }

            if (IsNumericType(underlyingType))
            {
                return PropertyEditorType.Number;
            }

            // 检查是否是结构体类型(Rectangle, Point, Size, Padding等)
            if (IsStructureType(underlyingType))
            {
                return PropertyEditorType.Object;
            }

            if (typeof(IList).IsAssignableFrom(propertyType) ||
                (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                return PropertyEditorType.List;
            }

            if (underlyingType.IsClass && underlyingType != typeof(string))
            {
                return PropertyEditorType.Object;
            }

            return PropertyEditorType.Text;
        }

        public static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(ushort) || type == typeof(sbyte) || type == typeof(decimal) ||
                   type == typeof(double) || type == typeof(float);
        }

        public static bool IsStructureType(Type type)
        {
            return type == typeof(Rectangle) ||
                   type == typeof(RectangleF) ||
                   type == typeof(Point) ||
                   type == typeof(PointF) ||
                   type == typeof(Size) ||
                   type == typeof(SizeF) ||
                   type == typeof(Padding) ||
                   type == typeof(Margins);
        }

        #region 编辑器创建方法

        private static Control CreateTextEditor(PropertyItem propertyItem)
        {
            var textBox = new FluentTextBox
            {
                Text = propertyItem.Value?.ToString() ?? "",
                Width = 200,
                Height = 28,
                Padding = new Padding(8, 4, 8, 4),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            textBox.TextChanged += (s, e) =>
            {
                propertyItem.Value = textBox.Text;
            };

            return textBox;
        }

        private static Control CreateNumberEditor(PropertyItem propertyItem)
        {
            Type underlyingType = Nullable.GetUnderlyingType(propertyItem.PropertyType) ?? propertyItem.PropertyType;
            bool isInt = underlyingType.IsInteger();

            var textBox = new FluentTextBox
            {
                Text = propertyItem.Value?.ToString() ?? "0",
                Width = 200,
                Height = 26,
                InputFormat = isInt ? InputFormat.Integer : InputFormat.Decimal,
                AllowNegative = true,
                DecimalPlaces = 3,
                Padding = new Padding(8, 4, 8, 4),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            if (propertyItem.Value != null)
            {
                try
                {
                    textBox.Text = propertyItem.Value.ToString();
                }
                catch
                {
                    textBox.Text = "0";
                }
            }

            textBox.TextChanged += (s, e) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        if (IsFloatingPointType(underlyingType))
                        {
                            propertyItem.Value = Convert.ChangeType(
                                decimal.Parse(textBox.Text), underlyingType);
                        }
                        else
                        {
                            // 去掉小数部分
                            var value = decimal.Parse(textBox.Text);
                            propertyItem.Value = Convert.ChangeType(
                                (long)value, underlyingType);
                        }
                    }
                }
                catch
                {
                    // 忽略转换错误
                }
            };

            return textBox;
        }

        private static bool IsFloatingPointType(Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal);
        }

        private static Control CreateBooleanEditor(PropertyItem propertyItem)
        {
            var checkBox = new FluentCheckBox
            {
                Checked = propertyItem.Value is bool b && b,
                Width = 200,
                Height = 22,
                Text = "",
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            checkBox.CheckedChanged += (s, e) =>
            {
                propertyItem.Value = checkBox.Checked;
            };

            return checkBox;
        }

        private static Control CreateEnumEditor(PropertyItem propertyItem)
        {
            Type underlyingType = Nullable.GetUnderlyingType(propertyItem.PropertyType) ?? propertyItem.PropertyType;

            var comboBox = new FluentComboBox
            {
                Width = 200,
                Height = 26,
                UseTheme = true,
                OnlySelection = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            var enumValues = Enum.GetValues(underlyingType);
            foreach (var enumValue in enumValues)
            {
                comboBox.Items.Add(enumValue);
            }

            if (propertyItem.Value != null)
            {
                comboBox.SelectedItem = propertyItem.Value;
            }

            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox.SelectedItem != null)
                {
                    propertyItem.Value = comboBox.SelectedItem;
                }
            };

            return comboBox;
        }

        private static Control CreateColorEditor(PropertyItem propertyItem)
        {
            var colorPicker = new FluentColorPicker
            {
                Width = 200,
                Height = 26,
                SelectedColor = propertyItem.Value is Color c ? c : Color.White,
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            colorPicker.ColorChanged += (s, e) =>
            {
                propertyItem.Value = colorPicker.SelectedColor;
            };

            return colorPicker;
        }

        private static Control CreateDateTimeEditor(PropertyItem propertyItem)
        {
            var dateTimePicker = new FluentDateTimePicker
            {
                Width = 200,
                Height = 26,
                Mode = DateTimePickerMode.DateTime,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            if (propertyItem.Value is DateTime dt)
            {
                dateTimePicker.Value = dt;
            }

            dateTimePicker.ValueChanged += (s, e) =>
            {
                propertyItem.Value = dateTimePicker.Value;
            };

            return dateTimePicker;
        }

        private static Control CreateListEditor(PropertyItem propertyItem)
        {
            var panel = new Panel
            {
                Width = 200,
                Height = 26,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var label = new Label
            {
                Text = GetListDisplayText(propertyItem.Value),
                Width = 152,
                Height = 24,
                Location = new Point(3, 1),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f)
            };

            var button = new FluentButton
            {
                Text = "...",
                Width = 30,
                Height = 22,
                Location = new Point(160, 1),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = false
            };

            button.Click += (s, e) =>
            {
                using (var editor = new ListEditorDialog(propertyItem))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        // 更新显示文本
                        label.Text = GetListDisplayText(propertyItem.Value);

                        // 标记为已修改
                        propertyItem.OriginalValue = propertyItem.Value;
                    }
                }
            };

            panel.Controls.Add(label);
            panel.Controls.Add(button);

            return panel;
        }

        private static Control CreateObjectEditor(PropertyItem propertyItem)
        {
            var panel = new Panel
            {
                Width = 200,
                Height = 26,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var label = new Label
            {
                Text = GetObjectDisplayText(propertyItem.Value, propertyItem.PropertyType),
                Width = 152,
                Height = 24,
                Location = new Point(3, 1),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f)
            };

            var button = new FluentButton
            {
                Text = "...",
                Width = 30,
                Height = 22,
                Location = new Point(160, 1),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = false
            };

            // 订阅值更改事件
            propertyItem.ValueChanged += (s, e) =>
            {
                label.Text = GetObjectDisplayText(propertyItem.Value, propertyItem.PropertyType);
                panel.Refresh();
            };

            button.Click += (s, e) =>
            {
                // 检查是否是结构体类型
                if (IsStructureType(propertyItem.PropertyType))
                {
                    using (var editor = new StructureEditorDialog(propertyItem))
                    {
                        editor.ShowDialog();
                    }
                }
                else
                {
                    using (var editor = new ObjectEditorDialog(propertyItem))
                    {
                        editor.ShowDialog();
                    }
                }
            };

            panel.Controls.Add(label);
            panel.Controls.Add(button);

            return panel;
        }

        internal static string GetListDisplayText(object value)
        {
            if (value == null)
            {
                return "(Null)";
            }

            string collectionText = string.Empty;
            if (value is IList list)
            {
                if (list.Count == 0)
                {
                    collectionText = "(空)";
                }
                else
                {
                    var strs = list.Cast<object>().Take(2).Select(t => t.ToString());
                    collectionText = $"{string.Join(", ", strs)}";

                    if (collectionText.Length > 13)
                    {
                        collectionText = collectionText.Substring(0, 12) + "..";
                    }

                    collectionText = $"[{collectionText}] 等 {list.Count} 项";
                }

                return collectionText;
            }

            return value.ToString();
        }

        internal static string GetObjectDisplayText(object value, Type type)
        {
            if (value == null)
            {
                return "(null)";
            }

            // 结构体类型显示其值
            if (IsStructureType(type))
            {
                if (value is Rectangle rect)
                {
                    return $"{rect.X}, {rect.Y}, {rect.Width}, {rect.Height}";
                }

                if (value is Point point)
                {
                    return $"{point.X}, {point.Y}";
                }

                if (value is Size size)
                {
                    return $"{size.Width}, {size.Height}";
                }

                if (value is Padding padding)
                {
                    return $"{padding.Left}, {padding.Top}, {padding.Right}, {padding.Bottom}";
                }

                return value.ToString();
            }

            return $"({value.GetType().Name})";
        }

        #endregion
    }

    #endregion

    #region 对话框

    /// <summary>
    /// 结构体编辑对话框
    /// (支持 Rectangle, Point, Size, Padding 等常用结构体)
    /// </summary>
    public class StructureEditorDialog : FluentDialog
    {
        private PropertyItem propertyItem;
        private Dictionary<string, FluentTextBox> editors;

        public StructureEditorDialog(PropertyItem propertyItem) : base(DialogType.Custom)
        {
            this.propertyItem = propertyItem;
            editors = new Dictionary<string, FluentTextBox>();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.TitleBar.Title = $"编辑 {propertyItem.DisplayName}";
            this.DialogButtons = DialogButtons.OKCancel;

            Type type = propertyItem.PropertyType;
            object value = propertyItem.Value ?? Activator.CreateInstance(type);

            int y = 16;
            int labelWidth = 100;
            int editorWidth = 150;
            int spacing = 42;

            if (type == typeof(Rectangle) || type == typeof(RectangleF))
            {
                InputFormat format = (type == typeof(Rectangle)) ? InputFormat.Integer : InputFormat.Decimal;

                CreateField("X", y, labelWidth, editorWidth, format);
                y += spacing;
                CreateField("Y", y, labelWidth, editorWidth, format);
                y += spacing;
                CreateField("Width", y, labelWidth, editorWidth, format);
                y += spacing;
                CreateField("Height", y, labelWidth, editorWidth, format);

                if (value is Rectangle rect)
                {
                    editors["X"].Text = rect.X.ToString();
                    editors["Y"].Text = rect.Y.ToString();
                    editors["Width"].Text = rect.Width.ToString();
                    editors["Height"].Text = rect.Height.ToString();
                }
                else if (value is RectangleF rectF)
                {
                    editors["X"].Text = rectF.X.ToString();
                    editors["Y"].Text = rectF.Y.ToString();
                    editors["Width"].Text = rectF.Width.ToString();
                    editors["Height"].Text = rectF.Height.ToString();
                    editors["X"].DecimalPlaces = 2;
                    editors["Y"].DecimalPlaces = 2;
                    editors["Width"].DecimalPlaces = 2;
                    editors["Height"].DecimalPlaces = 2;
                }

                this.Size = new Size(350, 260);
            }
            else if (type == typeof(Point) || type == typeof(PointF))
            {
                InputFormat format = (type == typeof(Point)) ? InputFormat.Integer : InputFormat.Decimal;

                CreateField("X", y, labelWidth, editorWidth, format);
                y += spacing;
                CreateField("Y", y, labelWidth, editorWidth, format);

                if (value is Point point)
                {
                    editors["X"].Text = point.X.ToString();
                    editors["Y"].Text = point.Y.ToString();
                }
                else if (value is PointF pointF)
                {
                    editors["X"].Text = pointF.X.ToString();
                    editors["Y"].Text = pointF.Y.ToString();
                    editors["X"].DecimalPlaces = 2;
                    editors["Y"].DecimalPlaces = 2;
                }

                this.Size = new Size(300, 190);
            }
            else if (type == typeof(Size) || type == typeof(SizeF))
            {
                InputFormat format = (type == typeof(Size)) ? InputFormat.Integer : InputFormat.Decimal;

                CreateField("Width", y, labelWidth, editorWidth, format);
                y += spacing;
                CreateField("Height", y, labelWidth, editorWidth, format);

                if (value is Size size)
                {
                    editors["Width"].Text = size.Width.ToString();
                    editors["Height"].Text = size.Height.ToString();
                }
                else if (value is SizeF sizeF)
                {
                    editors["Width"].Text = sizeF.Width.ToString();
                    editors["Height"].Text = sizeF.Height.ToString();
                    editors["Width"].DecimalPlaces = 2;
                    editors["Height"].DecimalPlaces = 2;
                }
                this.Size = new Size(300, 190);
            }
            else if (type == typeof(Padding))
            {
                CreateField("Left", y, labelWidth, editorWidth, InputFormat.Integer);
                y += spacing;
                CreateField("Top", y, labelWidth, editorWidth, InputFormat.Integer);
                y += spacing;
                CreateField("Right", y, labelWidth, editorWidth, InputFormat.Integer);
                y += spacing;
                CreateField("Bottom", y, labelWidth, editorWidth, InputFormat.Integer);

                if (value is Padding padding)
                {
                    editors["Left"].Text = padding.Left.ToString();
                    editors["Top"].Text = padding.Top.ToString();
                    editors["Right"].Text = padding.Right.ToString();
                    editors["Bottom"].Text = padding.Bottom.ToString();
                }

                this.Size = new Size(350, 260);
            }
        }

        private void CreateField(string fieldName, int y, int labelWidth, int editorWidth, InputFormat inputFormat = InputFormat.Decimal)
        {
            string text = $"{fieldName}:";
            int prefixWidth = TextRenderer.MeasureText(text, Font).Width;
            int prefixAreaWidth = 75;

            var numericUpDown = new FluentTextBox
            {
                Location = new Point(40, y),
                Size = new Size(prefixAreaWidth + editorWidth, 32),
                ShowBorder = false,
                InputFormat = inputFormat,
                Padding = new Padding(8, 6, 8, 6),
                ShowPrefix = true,
                Prefix = text,
                PrefixAreaWidth = 75
            };
            editors[fieldName] = numericUpDown;
            this.AddCustomControl(numericUpDown);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel != null)
            {
                panel.Controls.Clear();

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(10, 5, 10, 5)
                };

                var btnCancel = new FluentButton
                {
                    Text = "取消",
                    Size = new Size(100, 30),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Secondary,
                    Margin = new Padding(5, 0, 5, 0)
                };
                // 添加关闭逻辑
                btnCancel.Click += (s, be) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
                flowPanel.Controls.Add(btnCancel);

                var btnOK = new FluentButton
                {
                    Text = "确定",
                    Size = new Size(100, 30),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Primary,
                    Margin = new Padding(5, 0, 5, 0)
                };
                btnOK.Click += (s, be) =>
                {
                    SaveValue();
                    // 添加关闭逻辑
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                flowPanel.Controls.Add(btnOK);

                panel.Controls.Add(flowPanel);

                // 设置默认按钮和取消按钮
                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;
            }
        }

        private void SaveValue()
        {
            Type type = propertyItem.PropertyType;

            try
            {
                if (type == typeof(Rectangle))
                {
                    propertyItem.Value = new Rectangle(
                        Convert.ToInt32(editors["X"].Text),
                        Convert.ToInt32(editors["Y"].Text),
                        Convert.ToInt32(editors["Width"].Text),
                        Convert.ToInt32(editors["Height"].Text));
                }
                else if (type == typeof(RectangleF))
                {
                    propertyItem.Value = new RectangleF(
                        Convert.ToSingle(editors["X"].Text),
                        Convert.ToSingle(editors["Y"].Text),
                        Convert.ToSingle(editors["Width"].Text),
                        Convert.ToSingle(editors["Height"].Text));
                }
                else if (type == typeof(Point))
                {
                    propertyItem.Value = new Point(
                        Convert.ToInt32(editors["X"].Text),
                        Convert.ToInt32(editors["Y"].Text));
                }
                else if (type == typeof(PointF))
                {
                    propertyItem.Value = new PointF(
                        Convert.ToSingle(editors["X"].Text),
                        Convert.ToSingle(editors["Y"].Text));
                }
                else if (type == typeof(Size))
                {
                    propertyItem.Value = new Size(
                        Convert.ToInt32(editors["Width"].Text),
                        Convert.ToInt32(editors["Height"].Text));
                }
                else if (type == typeof(SizeF))
                {
                    propertyItem.Value = new SizeF(
                        Convert.ToSingle(editors["Width"].Text),
                        Convert.ToSingle(editors["Height"].Text));
                }
                else if (type == typeof(Padding))
                {
                    propertyItem.Value = new Padding(
                        Convert.ToInt32(editors["Left"].Text),
                        Convert.ToInt32(editors["Top"].Text),
                        Convert.ToInt32(editors["Right"].Text),
                        Convert.ToInt32(editors["Bottom"].Text));
                }
            }
            catch (Exception ex)
            {
                FluentDialog.Show(this, $"保存失败: {ex.Message}", "错误", DialogType.Error);
            }
        }
    }

    /// <summary>
    /// 列表编辑对话框
    /// </summary>
    public class ListEditorDialog : FluentDialog
    {
        private PropertyItem propertyItem;
        private ListBox listBox;
        private FluentButton btnAdd;
        private FluentButton btnRemove;
        private FluentButton btnMoveUp;
        private FluentButton btnMoveDown;
        private FluentButton btnEdit;
        private IList workingList;
        private Type itemType;

        public ListEditorDialog(PropertyItem propertyItem) : base(DialogType.Custom)
        {
            this.propertyItem = propertyItem;

            // 获取列表的元素类型
            if (propertyItem.PropertyType.IsGenericType)
            {
                itemType = propertyItem.PropertyType.GetGenericArguments()[0];
            }
            else if (propertyItem.PropertyType.IsArray)
            {
                itemType = propertyItem.PropertyType.GetElementType();
            }
            else
            {
                itemType = typeof(object);
            }

            InitializeComponents();
            LoadList();
        }

        private void InitializeComponents()
        {
            this.TitleBar.Title = $"编辑 {propertyItem.DisplayName}";
            this.Size = new Size(510, 420);
            this.DialogButtons = DialogButtons.OKCancel;

            // ListBox
            listBox = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(370, 300),
                HorizontalScrollbar = true,
                Font = new Font("Consolas", 9)
            };
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBox.DoubleClick += (s, e) => EditSelectedItem();
            this.AddCustomControl(listBox);

            // 按钮
            int btnX = 410;
            int btnY = 60;
            int btnWidth = 80;
            int btnHeight = 35;
            int spacing = 10;

            btnAdd = new FluentButton
            {
                Text = "添加",
                Location = new Point(btnX, btnY),
                Size = new Size(btnWidth, btnHeight),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Primary
            };
            btnAdd.Click += BtnAdd_Click;
            this.AddCustomControl(btnAdd);
            btnY += btnHeight + spacing;

            btnEdit = new FluentButton
            {
                Text = "编辑",
                Location = new Point(btnX, btnY),
                Size = new Size(btnWidth, btnHeight),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                Enabled = false
            };
            btnEdit.Click += (s, e) => EditSelectedItem();
            this.AddCustomControl(btnEdit);
            btnY += btnHeight + spacing;

            btnRemove = new FluentButton
            {
                Text = "删除",
                Location = new Point(btnX, btnY),
                Size = new Size(btnWidth, btnHeight),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Danger,
                Enabled = false
            };
            btnRemove.Click += BtnRemove_Click;
            this.AddCustomControl(btnRemove);
            btnY += btnHeight + spacing;

            btnMoveUp = new FluentButton
            {
                Text = "上移",
                Location = new Point(btnX, btnY),
                Size = new Size(btnWidth, btnHeight),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                Enabled = false
            };
            btnMoveUp.Click += BtnMoveUp_Click;
            this.AddCustomControl(btnMoveUp);
            btnY += btnHeight + spacing;

            btnMoveDown = new FluentButton
            {
                Text = "下移",
                Location = new Point(btnX, btnY),
                Size = new Size(btnWidth, btnHeight),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                Enabled = false
            };
            btnMoveDown.Click += BtnMoveDown_Click;
            this.AddCustomControl(btnMoveDown);
        }

        private void LoadList()
        {
            if (propertyItem.Value == null)
            {
                // 创建新列表
                if (propertyItem.PropertyType.IsGenericType)
                {
                    var listType = typeof(List<>).MakeGenericType(itemType);
                    workingList = (IList)Activator.CreateInstance(listType);
                }
                else if (propertyItem.PropertyType.IsArray)
                {
                    workingList = new List<object>();
                }
                else
                {
                    workingList = new List<object>();
                }
            }
            else
            {
                // 克隆现有列表
                if (propertyItem.Value is IList sourceList)
                {
                    if (propertyItem.PropertyType.IsGenericType)
                    {
                        var listType = typeof(List<>).MakeGenericType(itemType);
                        workingList = (IList)Activator.CreateInstance(listType);
                    }
                    else
                    {
                        workingList = new List<object>();
                    }

                    foreach (var item in sourceList)
                    {
                        workingList.Add(item);
                    }
                }
            }

            RefreshListBox();
        }

        private void RefreshListBox()
        {
            int selectedIndex = listBox.SelectedIndex;
            listBox.Items.Clear();

            if (workingList != null)
            {
                foreach (var item in workingList)
                {
                    listBox.Items.Add(FormatListItem(item));
                }
            }

            // 恢复选择
            if (selectedIndex >= 0 && selectedIndex < listBox.Items.Count)
            {
                listBox.SelectedIndex = selectedIndex;
            }
        }

        private string FormatListItem(object item)
        {
            if (item == null)
            {
                return "(null)";
            }

            if (item is string str)
            {
                return $"\"{str}\"";
            }

            if (item.GetType().IsValueType)
            {
                return item.ToString();
            }

            // 对象类型，显示类型名和ToString
            return $"[{item.GetType().Name}] {item}";
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = listBox.SelectedIndex >= 0;
            btnEdit.Enabled = hasSelection && !itemType.IsValueType;
            btnRemove.Enabled = hasSelection;
            btnMoveUp.Enabled = hasSelection && listBox.SelectedIndex > 0;
            btnMoveDown.Enabled = hasSelection && listBox.SelectedIndex < listBox.Items.Count - 1;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                object newItem = null;

                if (itemType.IsValueType || itemType == typeof(string))
                {
                    // 对于值类型和字符串，提示输入
                    string input = FluentDialog.InputBox(
                        $"输入新的 {itemType.Name}:",
                        "添加项",
                        GetDefaultValueString(itemType));

                    if (input == null)
                    {
                        return;
                    }

                    try
                    {
                        if (itemType == typeof(string))
                        {
                            newItem = input;
                        }
                        else
                        {
                            newItem = Convert.ChangeType(input, itemType);
                        }
                    }
                    catch
                    {
                        FluentDialog.Show(this, $"无法转换为 {itemType.Name}", "错误", DialogType.Error);
                        return;
                    }
                }
                else
                {
                    // 对于引用类型，尝试创建实例
                    try
                    {
                        newItem = Activator.CreateInstance(itemType);

                        // 使用新的方式创建 PropertyItem
                        using (var editor = new ObjectEditorDialog(newItem, itemType, "新建项"))
                        {
                            if (editor.ShowDialog(this) == DialogResult.OK)
                            {
                                newItem = editor.EditedObject;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FluentDialog.Show(this, $"无法创建 {itemType.Name} 的实例:\n{ex.Message}", "错误", DialogType.Error);
                        return;
                    }
                }

                if (newItem != null)
                {
                    workingList.Add(newItem);
                    RefreshListBox();
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                FluentDialog.Show(this, $"添加项失败: {ex.Message}", "错误", DialogType.Error);
            }
        }

        private void EditSelectedItem()
        {
            if (listBox.SelectedIndex < 0)
            {
                return;
            }

            int index = listBox.SelectedIndex;
            var item = workingList[index];

            if (item == null)
            {
                return;
            }

            if (itemType.IsValueType || itemType == typeof(string))
            {
                // 值类型，使用输入框
                string currentValue = item?.ToString() ?? "";
                string input = FluentDialog.InputBox($"编辑 {itemType.Name}:", "编辑项", currentValue);

                if (input == null)
                {
                    return;
                }

                try
                {
                    if (itemType == typeof(string))
                    {
                        workingList[index] = input;
                    }
                    else
                    {
                        workingList[index] = Convert.ChangeType(input, itemType);
                    }
                    RefreshListBox();
                }
                catch
                {
                    FluentDialog.Show(this, $"无法转换为 {itemType.Name}", "错误", DialogType.Error);
                }
            }
            else
            {
                // 对象类型，使用对象编辑器
                using (var editor = new ObjectEditorDialog(item, itemType, $"编辑 {itemType.Name}"))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        workingList[index] = editor.EditedObject;
                        RefreshListBox();
                    }
                }
            }
        }

        private string GetDefaultValueString(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type).ToString();
            }

            return "";
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                workingList.RemoveAt(index);
                RefreshListBox();

                if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = Math.Min(index, listBox.Items.Count - 1);
                }
            }
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                var item = workingList[index];
                workingList.RemoveAt(index);
                workingList.Insert(index - 1, item);
                RefreshListBox();
                listBox.SelectedIndex = index - 1;
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex >= 0 && listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                var item = workingList[index];
                workingList.RemoveAt(index);
                workingList.Insert(index + 1, item);
                RefreshListBox();
                listBox.SelectedIndex = index + 1;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // 在底部面板添加自定义按钮
            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel != null)
            {
                panel.Controls.Clear();

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(10, 5, 10, 5)
                };

                var btnCancel = new FluentButton
                {
                    Text = "取消",
                    Size = new Size(100, 30),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Secondary,
                    Margin = new Padding(5, 0, 5, 0)
                };
                // 添加关闭逻辑
                btnCancel.Click += (s, be) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
                flowPanel.Controls.Add(btnCancel);

                var btnOK = new FluentButton
                {
                    Text = "确定",
                    Size = new Size(100, 30),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Primary,
                    Margin = new Padding(5, 0, 5, 0)
                };
                // 添加关闭逻辑
                btnOK.Click += (s, be) =>
                {
                    SaveList();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                flowPanel.Controls.Add(btnOK);

                panel.Controls.Add(flowPanel);

                // 设置默认按钮和取消按钮
                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;
            }
        }
        private void SaveList()
        {
            // 保存到属性
            if (propertyItem.PropertyType.IsArray)
            {
                // 转换为数组
                Array array = Array.CreateInstance(itemType, workingList.Count);
                for (int i = 0; i < workingList.Count; i++)
                {
                    array.SetValue(workingList[i], i);
                }
                propertyItem.Value = array;
            }
            else
            {
                propertyItem.Value = workingList;
            }
        }
    }

    /// <summary>
    /// 对象编辑对话框
    /// </summary>
    public class ObjectEditorDialog : FluentDialog
    {
        private PropertyItem propertyItem;
        private FluentPropertyGrid propertyGrid;
        private object editedObject;

        public object EditedObject => editedObject;

        // 原有构造函数
        public ObjectEditorDialog(PropertyItem propertyItem) : base(DialogType.Custom)
        {
            this.propertyItem = propertyItem;
            this.editedObject = propertyItem.Value;
            InitializeComponents();
        }

        // 新增构造函数：直接传入对象
        public ObjectEditorDialog(object obj, Type objectType, string title) : base(DialogType.Custom)
        {
            this.editedObject = obj;

            this.propertyItem = new PropertyItem
            {
                Name = "Value",
                DisplayName = title,
                PropertyType = objectType,
                Value = obj,
                IsReadOnly = false
            };

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.TitleBar.Title = $"编辑 {propertyItem.DisplayName}";
            this.Size = new Size(500, 500);
            this.DialogButtons = DialogButtons.OKCancel;

            propertyGrid = new FluentPropertyGrid
            {
                Location = new Point(20, 10),
                Size = new Size(460, 400),
                ShowNavigationButtons = false,
                ShowActionButtons = false,
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name
            };

            if (editedObject != null)
            {
                propertyGrid.SelectedObject = editedObject;
            }
            else if (propertyItem.PropertyType.IsClass && propertyItem.PropertyType != typeof(string))
            {
                // 尝试创建实例
                try
                {
                    editedObject = Activator.CreateInstance(propertyItem.PropertyType);
                    propertyGrid.SelectedObject = editedObject;
                }
                catch
                {
                    FluentDialog.Show(this, $"无法创建 {propertyItem.PropertyType.Name} 的实例", "警告", DialogType.Warning);
                }
            }

            this.AddCustomControl(propertyGrid);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel != null)
            {
                panel.Controls.Clear();

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(10, 5, 10, 5)
                };

                var btnCancel = new FluentButton
                {
                    Text = "取消",
                    Size = new Size(100, 30),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Secondary,
                    Margin = new Padding(5, 0, 5, 0)
                };
                // 添加关闭逻辑
                btnCancel.Click += (s, be) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
                flowPanel.Controls.Add(btnCancel);

                var btnOK = new FluentButton
                {
                    Text = "确定",
                    Size = new Size(100, 30),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Primary,
                    Margin = new Padding(5, 0, 5, 0)
                };
                btnOK.Click += (s, be) =>
                {
                    propertyGrid.SaveChanges();
                    editedObject = propertyGrid.SelectedObject;
                    if (propertyItem != null)
                    {
                        propertyItem.Value = editedObject;
                    }
                    // 添加关闭逻辑
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                flowPanel.Controls.Add(btnOK);

                panel.Controls.Add(flowPanel);

                // 设置默认按钮和取消按钮
                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;
            }
        }
    }

    #endregion

    #region 特性

    /// <summary>
    /// 忽略编辑特性
    /// 标记此特性的属性不会在PropertyGrid中显示
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyIgnoreEditAttribute : Attribute
    {
    }

    /// <summary>
    /// 属性显示名称特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyDisplayNameAttribute : Attribute
    {
        public PropertyDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }

    /// <summary>
    /// 属性描述特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyDescriptionAttribute : Attribute
    {
        public PropertyDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }

    /// <summary>
    /// 属性分类特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyCategoryAttribute : Attribute
    {
        public PropertyCategoryAttribute(string category)
        {
            Category = category;
        }

        public string Category { get; }
    }

    /// <summary>
    /// 只读属性特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyReadOnlyAttribute : Attribute
    {
        public PropertyReadOnlyAttribute(bool isReadOnly = true)
        {
            IsReadOnly = isReadOnly;
        }

        public bool IsReadOnly { get; }
    }


    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 属性编辑器类型
    /// </summary>
    public enum PropertyEditorType
    {
        Text,           // 文本框
        Number,         // 数字框
        Boolean,        // 复选框
        Enum,           // 下拉框(枚举)
        Color,          // 颜色选择器
        DateTime,       // 日期时间选择器
        List,           // 列表编辑器
        Object,         // 对象编辑器
        Custom          // 自定义
    }

    #endregion
}
