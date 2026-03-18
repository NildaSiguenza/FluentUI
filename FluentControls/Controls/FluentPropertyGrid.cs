using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.IconFonts;
using FluentControls.Themes;
using Infrastructure;
using static FluentControls.Controls.FluentPanel;

namespace FluentControls.Controls
{
    public class FluentPropertyGrid : FluentControlBase
    {
        private object selectedObject;
        private IList selectedObjects;
        private int currentObjectIndex = 0;
        private int lastContentWidth = 0;

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

        private bool isRefreshingEditors = false; // 是否正在从控件刷新值到编辑器
        private bool hasUnsavedChanges = false;
        private bool isRendering = false;

        // 事件
        public event EventHandler PropertyValueEdited; // 用户通过编辑器修改了属性值时触发

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

            // 按正确的顺序添加(RightToLeft, 所以顺序相反)
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
                propertyItem.ValueChanged += OnPropertyItemValueChanged; // 订阅 ValueChanged事件, 当用户通过编辑器修改值时标记为脏状态
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

        private void OnPropertyItemValueChanged(object sender, EventArgs e)
        {
            if (isRefreshingEditors)
            {
                return;
            }

            if (sender is PropertyItem item)
            {
                item.IsDirtyFromEditor = true;
            }
            // 通知用户编辑属性状态
            PropertyValueEdited?.Invoke(this, EventArgs.Empty);
        }

        private void RenderProperties()
        {
            if (isRendering)
            {
                return;
            }

            if (propertyItems == null || categorizedItems == null)
            {
                return;
            }

            isRendering = true;

            try
            {
                contentPanel.SuspendLayout();

                // 重置滚动状态
                contentPanel.AutoScrollPosition = new Point(0, 0);
                contentPanel.AutoScrollMinSize = Size.Empty;
                contentPanel.Controls.Clear();

                // 计算布局参数
                int currentPadding = 10;
                int currentLabelWidth = 140;
                int currentRowHeight = 32;
                int currentCategorySpacing = 12;
                int currentGap = 5;

                // 动态计算可用宽度
                int availableWidth = contentPanel.ClientSize.Width;
                if (availableWidth <= 0)
                {
                    availableWidth = contentPanel.Width;
                }

                // 编辑器宽度 = 总宽度 - 左边距 - 标签宽 - 间距 - 右边距
                int editorLeft = currentPadding + currentLabelWidth + currentGap;
                int currentEditorWidth = Math.Max(80, availableWidth - editorLeft - currentPadding);

                // 分类标题宽度 = 总宽度 - 两侧边距
                int categoryWidth = Math.Max(80, availableWidth - currentPadding * 2);

                int y = currentPadding / 2;

                if (GroupByCategory)
                {
                    foreach (var category in categorizedItems.Keys.OrderBy(k => k))
                    {
                        // 分类标题 
                        var categoryPanel = new Panel
                        {
                            Location = new Point(currentPadding, y),
                            Size = new Size(categoryWidth, currentRowHeight - 4),
                            BackColor = Color.FromArgb(245, 247, 250),
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                        };

                        var categoryLabel = new Label
                        {
                            Text = "  " + category,
                            Dock = DockStyle.Fill,
                            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                            ForeColor = Color.FromArgb(70, 70, 80),
                            TextAlign = ContentAlignment.MiddleLeft,
                            Padding = new Padding(8, 0, 0, 0)
                        };

                        categoryPanel.Controls.Add(categoryLabel);
                        contentPanel.Controls.Add(categoryPanel);
                        y += currentRowHeight;

                        foreach (var item in categorizedItems[category])
                        {
                            RenderPropertyItem(item, y, currentPadding, currentLabelWidth, currentGap,
                                currentEditorWidth, currentRowHeight);
                            y += currentRowHeight + 2;
                        }

                        y += currentCategorySpacing;
                    }
                }
                else
                {
                    foreach (var item in propertyItems)
                    {
                        RenderPropertyItem(item, y, currentPadding, currentLabelWidth, currentGap,
                            currentEditorWidth, currentRowHeight);
                        y += currentRowHeight + 2;
                    }
                }

                int visibleHeight = contentPanel.ClientSize.Height;
                if (visibleHeight <= 0)
                {
                    visibleHeight = contentPanel.Height;
                }

                if (y > visibleHeight)
                {
                    contentPanel.AutoScrollMinSize = new Size(0, y + currentPadding);
                }
                else
                {
                    contentPanel.AutoScrollMinSize = Size.Empty;
                }

                // 先恢复布局, 让滚动条状态确定下来
                contentPanel.ResumeLayout(false);
                contentPanel.PerformLayout();

                // 布局完成后记录最终宽度
                lastContentWidth = contentPanel.ClientSize.Width;
            }
            finally
            {
                isRendering = false;
            }
        }

        private void RenderPropertyItem(PropertyItem item, int y, int padding, int labelWidth, int gap, int editorWidth, int rowHeight)
        {
            // 属性名标签
            var nameLabel = new Label
            {
                Text = item.DisplayName,
                Location = new Point(padding, y + (rowHeight - 20) / 2),
                Size = new Size(labelWidth, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 60, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            if (!string.IsNullOrEmpty(item.Description))
            {
                var toolTip = new ToolTip
                {
                    InitialDelay = 500,
                    ReshowDelay = 200
                };
                toolTip.SetToolTip(nameLabel, item.Description);
            }

            contentPanel.Controls.Add(nameLabel);

            // 编辑器 
            var editor = PropertyEditorFactory.CreateEditor(item);

            int editorLeft = padding + labelWidth + gap;
            editor.Location = new Point(editorLeft, y + (rowHeight - editor.Height) / 2);
            editor.Width = editorWidth;
            editor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            item.EditorControl = editor;
            contentPanel.Controls.Add(editor);
        }

        /// <summary>
        /// 主动同步编辑器值到属性值
        /// </summary>
        private void SyncEditorValues()
        {
            if (propertyItems == null)
            {
                return;
            }

            foreach (var item in propertyItems)
            {
                if (item.EditorControl == null || item.IsReadOnly)
                {
                    continue;
                }

                try
                {
                    Type targetType = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType;

                    // FluentNumericInput
                    if (item.EditorControl is FluentNumericInput numInput)
                    {
                        if (PropertyEditorFactory.IsNumericType(targetType))
                        {
                            item.Value = Convert.ChangeType(numInput.Value, targetType);
                        }
                    }
                    // FluentTextBox
                    else if (item.EditorControl is FluentTextBox textBox)
                    {
                        if (targetType == typeof(string))
                        {
                            item.Value = textBox.Text;
                        }
                        else
                        {
                            try
                            {
                                item.Value = Convert.ChangeType(textBox.Text, targetType);
                            }
                            catch { }
                        }
                    }
                    // FluentCheckBox
                    else if (item.EditorControl is FluentCheckBox checkBox)
                    {
                        if (targetType == typeof(bool))
                        {
                            item.Value = checkBox.Checked;
                        }
                    }
                    // FluentComboBox
                    else if (item.EditorControl is FluentComboBox comboBox)
                    {
                        if (comboBox.SelectedItem != null)
                        {
                            item.Value = comboBox.SelectedItem;
                        }
                    }
                    // FluentColorPicker
                    else if (item.EditorControl is FluentColorPicker colorPicker)
                    {
                        if (targetType == typeof(Color))
                        {
                            item.Value = colorPicker.SelectedColor;
                        }
                    }
                    // FluentDateTimePicker
                    else if (item.EditorControl is FluentDateTimePicker dtPicker)
                    {
                        if (targetType == typeof(DateTime))
                        {
                            item.Value = dtPicker.Value;
                        }
                    }
                    // 兼容常规控件
                    else if (item.EditorControl is NumericUpDown numericUpDown)
                    {
                        item.Value = Convert.ChangeType(numericUpDown.Value, targetType);
                    }
                    else if (item.EditorControl is TextBox tb)
                    {
                        if (targetType == typeof(string))
                        {
                            item.Value = tb.Text;
                        }
                        else
                        {
                            item.Value = Convert.ChangeType(tb.Text, targetType);
                        }
                    }
                    else if (item.EditorControl is CheckBox cb)
                    {
                        item.Value = cb.Checked;
                    }
                    else if (item.EditorControl is ComboBox cmb)
                    {
                        if (cmb.SelectedItem != null)
                        {
                            item.Value = cmb.SelectedItem;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
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

        /// <summary>
        /// 仅保存被用户编辑过的属性到目标对象
        /// </summary>
        public void SaveDirtyChanges()
        {
            var targetObject = GetCurrentObject();
            if (targetObject == null || propertyItems == null)
            {
                return;
            }

            // 先同步所有编辑器值到 PropertyItem.Value
            SyncEditorValues();

            // 仅将脏属性写入目标对象
            foreach (var item in propertyItems)
            {
                if (item.IsDirtyFromEditor && !item.IsReadOnly)
                {
                    item.SaveValue(targetObject);
                }
            }
        }

        /// <summary>
        /// 清除所有脏标记
        /// </summary>
        public void ClearDirtyFlags()
        {
            if (propertyItems == null)
            {
                return;
            }

            foreach (var item in propertyItems)
            {
                item.IsDirtyFromEditor = false;
            }
        }

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
            // 主动同步
            SyncEditorValues();

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

            isRefreshingEditors = true; // 刷新保护
            try
            {
                foreach (var item in propertyItems)
                {
                    item.UpdateValue(targetObject);
                    // 更新编辑器显示
                    UpdateEditorValue(item);
                }

                hasUnsavedChanges = false;
            }
            finally
            {
                isRefreshingEditors = false;
            }
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
                else if (item.EditorControl is Panel imgPanel && typeof(Image).IsAssignableFrom(item.PropertyType))
                {
                    // 查找 PictureBox 和 Label
                    var pictureBox = imgPanel.Controls.OfType<PictureBox>().FirstOrDefault();
                    var label = imgPanel.Controls.OfType<Label>().FirstOrDefault();
                    var clearBtn = imgPanel.Controls.OfType<FluentButton>()
                        .FirstOrDefault(b => b.Text == "×");

                    if (pictureBox != null)
                    {
                        var oldImage = pictureBox.Image;
                        if (item.Value is Image img)
                        {
                            pictureBox.Image = PropertyEditorFactory.CreateImageCopy(img);
                        }
                        else
                        {
                            pictureBox.Image = null;
                        }
                        oldImage?.Dispose();
                    }

                    if (label != null)
                    {
                        label.Text = item.Value is Image img2
                            ? $"{img2.Width}×{img2.Height}"
                            : "(无图像)";
                    }

                    if (clearBtn != null)
                    {
                        clearBtn.Visible = item.Value != null;
                    }

                    imgPanel.Refresh();
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

            return value is IList list ? $"(Collection) {list.Count} 项" : value.ToString();
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

                return value is Padding padding ? $"{padding.Left}, {padding.Top}, {padding.Right}, {padding.Bottom}" : value.ToString();
            }

            return $"({value.GetType().Name})";
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = FluentDialog.Show("有未保存的更改, 是否继续刷新?", "确认", DialogType.Warning);

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

        /// <summary>
        /// 标记属性是否被用户通过编辑器修改过
        /// </summary>
        public bool IsDirtyFromEditor { get; set; } = false;

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

            // 对于其他类型, 尝试序列化克隆(简化版本)
            return value;
        }

        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
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

            int editorHeight = 28;
            int cornerRadius = 4;

            switch (editorType)
            {
                case PropertyEditorType.Boolean:
                    editor = CreateBooleanEditor(propertyItem, editorHeight);
                    break;
                case PropertyEditorType.Number:
                    editor = CreateNumberEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.Enum:
                    editor = CreateEnumEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.Color:
                    editor = CreateColorEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.DateTime:
                    editor = CreateDateTimeEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.List:
                    editor = CreateListEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.Image:
                    editor = CreateImageEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.Object:
                    editor = CreateObjectEditor(propertyItem, editorHeight, cornerRadius);
                    break;
                case PropertyEditorType.Text:
                default:
                    editor = CreateTextEditor(propertyItem, editorHeight, cornerRadius);
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

            if (typeof(Image).IsAssignableFrom(underlyingType) || underlyingType == typeof(Bitmap))
            {
                return PropertyEditorType.Image;
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

            return underlyingType.IsClass && underlyingType != typeof(string) ? PropertyEditorType.Object : PropertyEditorType.Text;
        }

        public static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(ushort) || type == typeof(sbyte) || type == typeof(decimal) ||
                   type == typeof(double) || type == typeof(float);
        }
        private static bool IsFloatingPointType(Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal);
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

        private static Control CreateTextEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            var textBox = new FluentTextBox
            {
                Text = propertyItem.Value?.ToString() ?? "",
                Width = 200,
                Height = height,
                Padding = new Padding(4, 0, 4, 0),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ShowBorder = true
            };

            textBox.TextChanged += (s, e) =>
            {
                propertyItem.Value = textBox.Text;
            };

            return textBox;
        }

        private static Control CreateNumberEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            Type underlyingType = Nullable.GetUnderlyingType(propertyItem.PropertyType) ?? propertyItem.PropertyType;
            bool isInt = underlyingType.IsInteger();

            var textBox = new FluentNumericInput
            {
                Width = 200,
                Height = height,
                Value = Convert.ToDecimal(propertyItem.Value?.ToString() ?? "0"),
                DecimalPlaces = isInt ? 0 : 3,
                Padding = new Padding(4, 0, 4, 0),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ShowBorder = true
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

            Action syncValueToProperty = () =>
            {
                try
                {
                    if (IsFloatingPointType(underlyingType))
                    {
                        var newValue = Convert.ChangeType(textBox.Value, underlyingType);
                        if (!Equals(propertyItem.Value, newValue))
                        {
                            propertyItem.Value = newValue;
                        }
                    }
                    else
                    {
                        var newValue = Convert.ChangeType((long)textBox.Value, underlyingType);
                        if (!Equals(propertyItem.Value, newValue))
                        {
                            propertyItem.Value = newValue;
                        }
                    }
                }
                catch { }
            };

            // 同时监听 TextChanged 和 ValueChanged
            textBox.TextChanged += (s, e) => syncValueToProperty();
            textBox.ValueChanged += (s, e) => syncValueToProperty();

            return textBox;
        }

        private static Control CreateBooleanEditor(PropertyItem propertyItem, int height = 22)
        {
            var checkBox = new FluentCheckBox
            {
                Checked = propertyItem.Value is bool b && b,
                Width = 200,
                Height = height,
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

        private static Control CreateEnumEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            Type underlyingType = Nullable.GetUnderlyingType(propertyItem.PropertyType) ?? propertyItem.PropertyType;

            var comboBox = new FluentComboBox
            {
                Width = 200,
                Height = height,
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

        private static Control CreateColorEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            var colorPicker = new FluentColorPicker
            {
                Width = 200,
                Height = height,
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

        private static Control CreateDateTimeEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            var dateTimePicker = new FluentDateTimePicker
            {
                Width = 200,
                Height = height,
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

        private static Control CreateListEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            // 美化后的列表编辑器面板
            var panel = new DoubleBufferedPanel
            {
                Width = 200,
                Height = height,
                BackColor = Color.White
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 绘制圆角边框背景
                using (var path = RoundedCornerRenderer.CreateRoundedRectPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), cornerRadius))
                {
                    using (var brush = new SolidBrush(Color.White))
                    {
                        g.FillPath(brush, path);
                    }
                    using (var pen = new Pen(Color.FromArgb(210, 210, 215), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            };

            var label = new Label
            {
                Text = GetListDisplayText(propertyItem.Value),
                Width = panel.Width - 36,
                Height = height - 4,
                Location = new Point(10, 2),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            var button = new FluentButton
            {
                Text = "...",
                Width = 28,
                Height = height - 4,
                Location = new Point(panel.Width - 30, 2),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            button.Click += (s, e) =>
            {
                Size dlgSize =new Size(560, 460);
                using (var editor = new ListEditorDialog(propertyItem, dlgSize))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        label.Text = GetListDisplayText(propertyItem.Value);
                        propertyItem.OriginalValue = propertyItem.Value;
                    }
                }
            };

            panel.Controls.Add(label);
            panel.Controls.Add(button);

            return panel;
        }

        private static Control CreateImageEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            int panelHeight = Math.Max(height, 28);

            var panel = new DoubleBufferedPanel
            {
                Width = 200,
                Height = panelHeight,
                BackColor = Color.White
            };

            // 绘制边框
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedCornerRenderer.CreateRoundedRectPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), cornerRadius))
                {
                    using (var brush = new SolidBrush(Color.White))
                    {
                        g.FillPath(brush, path);
                    }

                    using (var pen = new Pen(Color.FromArgb(210, 210, 215), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            };

            // 图片预览区域
            int previewSize = panelHeight - 6;
            var previewBox = new PictureBox
            {
                Location = new Point(3, 3),
                Size = new Size(previewSize, previewSize),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 245, 248),
                BorderStyle = BorderStyle.None
            };
            // 绘制棋盘格背景
            previewBox.Paint += (s, e) =>
            {
                if (previewBox.Image == null)
                {
                    DrawCheckerboard(e.Graphics, previewBox.ClientRectangle, 4);
                    // 绘制占位符
                    using (var font = new Font("Segoe UI", 7f))
                    using (var brush = new SolidBrush(Color.FromArgb(160, 160, 160)))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        e.Graphics.DrawString("无", font, brush, new RectangleF(0, 0, previewBox.Width, previewBox.Height), sf);
                    }
                }
                else
                {
                    DrawCheckerboard(e.Graphics, previewBox.ClientRectangle, 4);
                }
            };

            // 设置当前图片预览
            UpdateImagePreview(previewBox, propertyItem.Value as Image);

            // 信息标签
            int labelLeft = previewSize + 10;
            var infoLabel = new Label
            {
                Location = new Point(labelLeft, 2),
                Size = new Size(panel.Width - labelLeft - 68, panelHeight - 4),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(80, 80, 80),
                Text = GetImageDisplayText(propertyItem.Value as Image)
            };

            // 清除按钮
            var btnClear = new FluentButton
            {
                Text = "×",
                Size = new Size(24, panelHeight - 4),
                Location = new Point(panel.Width - 56, 2),
                Anchor = AnchorStyles.Right,
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Text,
                CornerRadius = 0,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Visible = propertyItem.Value != null
            };
            btnClear.Click += (s, e) =>
            {
                propertyItem.Value = null;
                UpdateImagePreview(previewBox, null);
                infoLabel.Text = GetImageDisplayText(null);
                btnClear.Visible = false;
                panel.Refresh();
            };

            // 编辑按钮
            var btnEdit = new FluentButton
            {
                Text = "...",
                Size = new Size(28, panelHeight - 4),
                Location = new Point(panel.Width - 30, 2),
                Anchor = AnchorStyles.Right,
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            btnEdit.Click += (s, e) =>
            {
                OpenImageEditor(propertyItem, previewBox, infoLabel, btnClear, panel);
            };

            // 双击预览图打开编辑器
            previewBox.DoubleClick += (s, e) =>
            {
                OpenImageEditor(propertyItem, previewBox, infoLabel, btnClear, panel);
            };

            // 监听属性值变化
            propertyItem.ValueChanged += (s, e) =>
            {
                UpdateImagePreview(previewBox, propertyItem.Value as Image);
                infoLabel.Text = GetImageDisplayText(propertyItem.Value as Image);
                btnClear.Visible = propertyItem.Value != null;
                panel.Refresh();
            };

            panel.Controls.Add(previewBox);
            panel.Controls.Add(infoLabel);
            panel.Controls.Add(btnClear);
            panel.Controls.Add(btnEdit);

            return panel;
        }

        private static Control CreateObjectEditor(PropertyItem propertyItem, int height = 28, int cornerRadius = 4)
        {
            var panel = new DoubleBufferedPanel
            {
                Width = 200,
                Height = height,
                BackColor = Color.White
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = RoundedCornerRenderer.CreateRoundedRectPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), cornerRadius))
                {
                    using (var brush = new SolidBrush(Color.White))
                    {
                        g.FillPath(brush, path);
                    }
                    using (var pen = new Pen(Color.FromArgb(210, 210, 215), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            };

            var label = new Label
            {
                Text = GetObjectDisplayText(propertyItem.Value, propertyItem.PropertyType),
                Width = panel.Width - 36,
                Height = height - 4,
                Location = new Point(10, 2),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            var button = new FluentButton
            {
                Text = "...",
                Width = 28,
                Height = height - 4,
                Location = new Point(panel.Width - 30, 2),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = ButtonStyle.Secondary,
                CornerRadius = 0,
                EnableRippleEffect = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            propertyItem.ValueChanged += (s, e) =>
            {
                label.Text = GetObjectDisplayText(propertyItem.Value, propertyItem.PropertyType);
                panel.Refresh();
            };

            button.Click += (s, e) =>
            {
                if (IsStructureType(propertyItem.PropertyType))
                {
                    Size dlgSize = new Size(340, 300);
                    using (var editor = new StructureEditorDialog(propertyItem, dlgSize))
                    {
                        editor.ShowDialog();
                    }
                }
                else
                {
                    Size dlgSize = new Size(520, 520);
                    using (var editor = new ObjectEditorDialog(propertyItem, dlgSize))
                    {
                        editor.ShowDialog();
                    }
                }
            };

            panel.Controls.Add(label);
            panel.Controls.Add(button);

            return panel;
        }

        /// <summary>
        /// 打开图片编辑器对话框
        /// </summary>
        private static void OpenImageEditor(PropertyItem propertyItem, PictureBox previewBox, Label infoLabel, FluentButton btnClear, Panel panel)
        {
            Image currentImage = propertyItem.Value as Image;
            bool useIconFontEditor = HasIconFontEditorAttribute(propertyItem);

            if (useIconFontEditor)
            {
                using (var dialog = new IconFontImageSelectorDialog(null, currentImage))
                {
                    if (dialog.ShowDialog(panel.FindForm()) == DialogResult.OK)
                    {
                        var selectedImage = dialog.SelectedImage;

                        if (selectedImage != null)
                        {
                            // 创建独立副本
                            Image newImage = CreateImageCopy(selectedImage);
                            propertyItem.Value = newImage;
                        }
                        else
                        {
                            propertyItem.Value = null;
                        }

                        UpdateImagePreview(previewBox, propertyItem.Value as Image);
                        infoLabel.Text = GetImageDisplayText(propertyItem.Value as Image);
                        btnClear.Visible = propertyItem.Value != null;
                        panel.Refresh();
                    }
                }
            }
            else
            {
                // 使用标准文件选择对话框
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.ico|所有文件|*.*";
                    dialog.Title = "选择图像";

                    if (dialog.ShowDialog(panel.FindForm()) == DialogResult.OK)
                    {
                        try
                        {
                            using (var loadedImage = Image.FromFile(dialog.FileName))
                            {
                                Image newImage = CreateImageCopy(loadedImage);
                                propertyItem.Value = newImage;
                            }

                            UpdateImagePreview(previewBox, propertyItem.Value as Image);
                            infoLabel.Text = GetImageDisplayText(propertyItem.Value as Image);
                            btnClear.Visible = true;
                            panel.Refresh();
                        }
                        catch (Exception ex)
                        {
                            FluentMessageManager.Instance.Error(ex.Message, "错误");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查属性是否标记了 IconFontImageEditor 特性
        /// </summary>
        private static bool HasIconFontEditorAttribute(PropertyItem propertyItem)
        {
            if (propertyItem.PropertyInfo == null)
            {
                return false;
            }

            var editorAttr = propertyItem.PropertyInfo
                .GetCustomAttributes(typeof(EditorAttribute), true)
                .OfType<EditorAttribute>()
                .FirstOrDefault();

            if (editorAttr != null)
            {
                return editorAttr.EditorTypeName.Contains("IconFontImageEditor");
            }

            return true;
        }

        private static void UpdateImagePreview(PictureBox previewBox, Image image)
        {
            var oldImage = previewBox.Image;
            previewBox.Image = image != null ? CreateImageCopy(image) : null;
            oldImage?.Dispose();
        }

        private static string GetImageDisplayText(Image image)
        {
            if (image == null)
            {
                return "(无图像)";
            }

            try
            {
                return $"{image.Width}×{image.Height}";
            }
            catch
            {
                return "(无效图像)";
            }
        }

        /// <summary>
        /// 创建图片的独立副本
        /// </summary>
        public static Image CreateImageCopy(Image source)
        {
            if (source == null)
            {
                return null;
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    source.Save(ms, ImageFormat.Png);
                    var bytes = ms.ToArray();
                    var newStream = new MemoryStream(bytes);
                    return Image.FromStream(newStream);
                }
            }
            catch
            {
                try
                {
                    var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(source, 0, 0, source.Width, source.Height);
                    }
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 绘制棋盘格背景
        /// </summary>
        private static void DrawCheckerboard(Graphics g, Rectangle rect, int checkSize)
        {
            using (var lightBrush = new SolidBrush(Color.White))
            using (var darkBrush = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                g.FillRectangle(lightBrush, rect);
                for (int y = rect.Top; y < rect.Bottom; y += checkSize)
                {
                    for (int x = rect.Left; x < rect.Right; x += checkSize)
                    {
                        if (((x - rect.Left) / checkSize + (y - rect.Top) / checkSize) % 2 == 1)
                        {
                            int w = Math.Min(checkSize, rect.Right - x);
                            int h = Math.Min(checkSize, rect.Bottom - y);
                            g.FillRectangle(darkBrush, x, y, w, h);
                        }
                    }
                }
            }
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

                return value is Padding padding ? $"{padding.Left}, {padding.Top}, {padding.Right}, {padding.Bottom}" : value.ToString();
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
        private Dictionary<string, FluentNumericInput> editors;
        private int cornerRadius = 6;
        private int fieldSpacing = 48;
        private int fieldHeight = 36;
        private Size dialogSize = new Size(340, 300);

        public StructureEditorDialog(PropertyItem propertyItem, Size? size = null) : base(DialogType.Custom, size: size)
        {
            this.propertyItem = propertyItem;
            editors = new Dictionary<string, FluentNumericInput>();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.TitleBar.Title = $"编辑 {propertyItem.DisplayName}";
            this.DialogButtons = DialogButtons.OKCancel;
            this.StartPosition = FormStartPosition.CenterParent;

            Type type = propertyItem.PropertyType;
            object value = propertyItem.Value ?? Activator.CreateInstance(type);

            int y = 20;
            int labelWidth = 80;
            int editorWidth = 180;

            if (type == typeof(Rectangle) || type == typeof(RectangleF))
            {
                InputFormat format = (type == typeof(Rectangle)) ? InputFormat.Integer : InputFormat.Decimal;

                CreateField("X", y, labelWidth, editorWidth, format);
                y += fieldSpacing;
                CreateField("Y", y, labelWidth, editorWidth, format);
                y += fieldSpacing;
                CreateField("Width", y, labelWidth, editorWidth, format);
                y += fieldSpacing;
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
                    editors["X"].Text = rectF.X.ToString("F2");
                    editors["Y"].Text = rectF.Y.ToString("F2");
                    editors["Width"].Text = rectF.Width.ToString("F2");
                    editors["Height"].Text = rectF.Height.ToString("F2");
                }

                dialogSize = new Size(340, 300);
            }
            else if (type == typeof(Point) || type == typeof(PointF))
            {
                InputFormat format = (type == typeof(Point)) ? InputFormat.Integer : InputFormat.Decimal;

                CreateField("X", y, labelWidth, editorWidth, format);
                y += fieldSpacing;
                CreateField("Y", y, labelWidth, editorWidth, format);

                if (value is Point point)
                {
                    editors["X"].Text = point.X.ToString();
                    editors["Y"].Text = point.Y.ToString();
                }
                else if (value is PointF pointF)
                {
                    editors["X"].Text = pointF.X.ToString("F2");
                    editors["Y"].Text = pointF.Y.ToString("F2");
                }

                dialogSize = new Size(340, 210);
            }
            else if (type == typeof(Size) || type == typeof(SizeF))
            {
                InputFormat format = (type == typeof(Size)) ? InputFormat.Integer : InputFormat.Decimal;

                CreateField("Width", y, labelWidth, editorWidth, format);
                y += fieldSpacing;
                CreateField("Height", y, labelWidth, editorWidth, format);

                if (value is Size size)
                {
                    editors["Width"].Text = size.Width.ToString();
                    editors["Height"].Text = size.Height.ToString();
                }
                else if (value is SizeF sizeF)
                {
                    editors["Width"].Text = sizeF.Width.ToString("F2");
                    editors["Height"].Text = sizeF.Height.ToString("F2");
                }

                dialogSize = new Size(340, 210);
            }
            else if (type == typeof(Padding))
            {
                CreateField("Left", y, labelWidth, editorWidth, InputFormat.Integer);
                y += fieldSpacing;
                CreateField("Top", y, labelWidth, editorWidth, InputFormat.Integer);
                y += fieldSpacing;
                CreateField("Right", y, labelWidth, editorWidth, InputFormat.Integer);
                y += fieldSpacing;
                CreateField("Bottom", y, labelWidth, editorWidth, InputFormat.Integer);

                if (value is Padding padding)
                {
                    editors["Left"].Text = padding.Left.ToString();
                    editors["Top"].Text = padding.Top.ToString();
                    editors["Right"].Text = padding.Right.ToString();
                    editors["Bottom"].Text = padding.Bottom.ToString();
                }

                dialogSize = new Size(340, 300);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            BeginInvoke(new Action(() =>
            {
                this.Size = dialogSize;
            }));
        }

        private void CreateField(string fieldName, int y, int labelWidth, int editorWidth, InputFormat inputFormat)
        {
            // 创建标签
            var label = new Label
            {
                Text = fieldName,
                Location = new Point(24, y + 8),
                Size = new Size(labelWidth, 20),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(70, 70, 70),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.AddCustomControl(label);

            // 创建输入框
            bool isInt = inputFormat == InputFormat.Integer;
            var textBox = new FluentNumericInput
            {
                Location = new Point(24 + labelWidth + 12, y),
                Width = editorWidth,
                Height = fieldHeight,
                DecimalPlaces = isInt ? 0 : 3,
                Padding = new Padding(12, 8, 12, 8),
                ButtonSpacing = 5,
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ShowBorder = true
            };

            editors[fieldName] = textBox;
            this.AddCustomControl(textBox);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.SuspendLayout();
            SetupDialogButtons();
            this.ResumeLayout(true);
        }

        private void SetupDialogButtons()
        {
            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel != null)
            {
                panel.Height = 60;
                panel.Padding = new Padding(16, 12, 16, 12);
                panel.Controls.Clear();

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    BackColor = Color.Transparent
                };

                var btnCancel = new FluentButton
                {
                    Text = "取消",
                    Size = new Size(90, 36),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Secondary,
                    CornerRadius = 0,
                    Margin = new Padding(0, 0, 0, 0),
                    Font = new Font("Segoe UI", 9.5f)
                };
                btnCancel.Click += (s, be) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
                flowPanel.Controls.Add(btnCancel);

                var btnOK = new FluentButton
                {
                    Text = "确定",
                    Size = new Size(90, 36),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Primary,
                    CornerRadius = 0,
                    Margin = new Padding(0, 0, 12, 0),
                    Font = new Font("Segoe UI", 9.5f)
                };
                btnOK.Click += (s, be) =>
                {
                    SaveValue();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                flowPanel.Controls.Add(btnOK);

                panel.Controls.Add(flowPanel);
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
        private int cornerRadius = 6;

        public ListEditorDialog(PropertyItem propertyItem, Size? size = null) : base(DialogType.Custom, size: size)
        {
            this.propertyItem = propertyItem;

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
            this.DialogButtons = DialogButtons.OKCancel;
            this.StartPosition = FormStartPosition.CenterParent;

            this.SuspendLayout();

            // 美化后的 ListBox
            listBox = new ListBox
            {
                Location = new Point(24, 20),
                Size = new Size(400, 340),
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ItemHeight = 28
            };
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBox.DoubleClick += (s, e) => EditSelectedItem();
            this.AddCustomControl(listBox);

            // 按钮区域
            int btnX = 440;
            int btnY = 20;
            int btnWidth = 90;
            int btnHeight = 36;
            int spacing = 8;

            btnAdd = CreateButton("添加", btnX, btnY, btnWidth, btnHeight, ButtonStyle.Primary);
            btnAdd.Click += BtnAdd_Click;
            this.AddCustomControl(btnAdd);
            btnY += btnHeight + spacing;

            btnEdit = CreateButton("编辑", btnX, btnY, btnWidth, btnHeight, ButtonStyle.Secondary);
            btnEdit.Click += (s, e) => EditSelectedItem();
            btnEdit.Enabled = false;
            this.AddCustomControl(btnEdit);
            btnY += btnHeight + spacing;

            btnRemove = CreateButton("删除", btnX, btnY, btnWidth, btnHeight, ButtonStyle.Danger);
            btnRemove.Click += BtnRemove_Click;
            btnRemove.Enabled = false;
            this.AddCustomControl(btnRemove);
            btnY += btnHeight + spacing + 16; // 额外间距分隔

            btnMoveUp = CreateButton("上移", btnX, btnY, btnWidth, btnHeight, ButtonStyle.Secondary);
            btnMoveUp.Click += BtnMoveUp_Click;
            btnMoveUp.Enabled = false;
            this.AddCustomControl(btnMoveUp);
            btnY += btnHeight + spacing;

            btnMoveDown = CreateButton("下移", btnX, btnY, btnWidth, btnHeight, ButtonStyle.Secondary);
            btnMoveDown.Click += BtnMoveDown_Click;
            btnMoveDown.Enabled = false;
            this.AddCustomControl(btnMoveDown);

            this.ResumeLayout();
        }

        private FluentButton CreateButton(string text, int x, int y, int width, int height, ButtonStyle style)
        {
            return new FluentButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name,
                ButtonStyle = style,
                CornerRadius = 0,
                EnableRippleEffect = true,
                Font = new Font("Segoe UI", 9)
            };
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

            // 对象类型, 显示类型名和ToString
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
                    // 对于值类型和字符串, 提示输入
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
                    try
                    {
                        newItem = Activator.CreateInstance(itemType);

                        Size dlgSize = new Size(520, 520);
                        using (var editor = new ObjectEditorDialog(newItem, itemType, "新建项", dlgSize))
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
                // 值类型, 使用输入框
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
                Size dlgSize = new Size(520, 520);
                using (var editor = new ObjectEditorDialog(item, itemType, $"编辑 {itemType.Name}", dlgSize))
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

            return type.IsValueType ? Activator.CreateInstance(type).ToString() : "";
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
            this.SuspendLayout();

            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel != null)
            {
                panel.Height = 60;
                panel.Padding = new Padding(16, 12, 16, 12);
                panel.Controls.Clear();

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    BackColor = Color.Transparent
                };

                var btnCancel = new FluentButton
                {
                    Text = "取消",
                    Size = new Size(90, 36),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Secondary,
                    CornerRadius = 0,
                    Margin = new Padding(0),
                    Font = new Font("Segoe UI", 9.5f)
                };
                btnCancel.Click += (s, be) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
                flowPanel.Controls.Add(btnCancel);

                var btnOK = new FluentButton
                {
                    Text = "确定",
                    Size = new Size(90, 36),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Primary,
                    CornerRadius = 0,
                    Margin = new Padding(0, 0, 12, 0),
                    Font = new Font("Segoe UI", 9.5f)
                };
                btnOK.Click += (s, be) =>
                {
                    SaveList();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                flowPanel.Controls.Add(btnOK);

                panel.Controls.Add(flowPanel);
                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;

                this.ResumeLayout(true);
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
        private int cornerRadius = 6;

        public object EditedObject => editedObject;

        public ObjectEditorDialog(PropertyItem propertyItem, Size? size = null) : base(DialogType.Custom, size: size)
        {
            this.propertyItem = propertyItem;
            this.editedObject = propertyItem.Value;
            InitializeComponents();
        }

        public ObjectEditorDialog(object obj, Type objectType, string title, Size? size = null) : base(DialogType.Custom, size: size)
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
            this.DialogButtons = DialogButtons.OKCancel;
            this.StartPosition = FormStartPosition.CenterParent;

            propertyGrid = new FluentPropertyGrid
            {
                Location = new Point(20, 16),
                Size = new Size(480, 410),
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
            this.SuspendLayout();

            var panel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (panel != null)
            {
                panel.Height = 60;
                panel.Padding = new Padding(16, 12, 16, 12);
                panel.Controls.Clear();

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    BackColor = Color.Transparent
                };

                var btnCancel = new FluentButton
                {
                    Text = "取消",
                    Size = new Size(90, 36),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Secondary,
                    CornerRadius = 0,
                    Margin = new Padding(0),
                    Font = new Font("Segoe UI", 9.5f)
                };
                btnCancel.Click += (s, be) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
                flowPanel.Controls.Add(btnCancel);

                var btnOK = new FluentButton
                {
                    Text = "确定",
                    Size = new Size(90, 36),
                    UseTheme = true,
                    ThemeName = ThemeManager.CurrentTheme?.Name,
                    ButtonStyle = ButtonStyle.Primary,
                    CornerRadius = 0,
                    Margin = new Padding(0, 0, 12, 0),
                    Font = new Font("Segoe UI", 9.5f)
                };
                btnOK.Click += (s, be) =>
                {
                    propertyGrid.SaveChanges();
                    editedObject = propertyGrid.SelectedObject;
                    if (propertyItem != null)
                    {
                        propertyItem.Value = editedObject;
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                flowPanel.Controls.Add(btnOK);

                panel.Controls.Add(flowPanel);
                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;

                this.ResumeLayout(true);
            }
        }
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
        Image,          // 图片选择器
        Object,         // 对象编辑器
        Custom          // 自定义
    }

    #endregion
}
