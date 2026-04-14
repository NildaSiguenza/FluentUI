using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Animation;
using FluentControls.Themes;
using Infrastructure;

namespace FluentControls.Controls
{
    public class FluentSettingsPanel<T> : FluentDialog where T : class, new()
    {
        private FluentToolList toolList;
        private Panel contentPanel;
        private Panel scrollContainer;
        private SettingControlGenerator controlGenerator;

        private Dictionary<string, SettingItemInfo> settingItems;
        private Dictionary<string, SettingGroupInfo> settingGroups;
        private Dictionary<string, Panel> groupPanels;
        private Dictionary<string, IHierarchicalItem<T>> toolItemMapping;

        private T currentData;
        private List<T> listData;
        private IHierarchicalItem<T> hierarchicalData;
        private IHierarchicalItem<T> selectedHierarchicalItem;

        private SettingsDataMode dataMode;
        private int currentListIndex;
        private bool isInitialized;
        private bool suppressSave;

        private const int TOOL_LIST_WIDTH = 200;
        private const int CONTENT_PADDING = 16;

        // 动画控制
        private bool enableScrollAnimation = true;

        // 滚动控制
        private bool isScrolling = false;
        private bool allowScrollAnimation = false;
        private Timer scrollDebounceTimer;

        // 事件
        public event EventHandler<SettingsInitializeEventArgs<T>> InitializeData; // 初始化数据事件
        public event EventHandler<SettingsSaveEventArgs<T>> SaveData; // 保存数据事件
        public event EventHandler<SettingValueChangedEventArgs> SettingValueChanged; // 设置值变更事件
        public event EventHandler<SettingsItemSelectedEventArgs<T>> ItemSelected; // 设置项选中变更事件

        #region 构造函数

        public FluentSettingsPanel() : this("设置", new Size(900, 600))
        {
        }

        public FluentSettingsPanel(string title) : this(title, new Size(900, 600))
        {
        }

        public FluentSettingsPanel(string title, Size size)
            : base(DialogType.Custom, DialogButtons.OKCancel, size)
        {
            TitleBar.Title = title;
            InitializeSettingsPanel();
        }

        private void InitializeSettingsPanel()
        {
            EnableOpenAnimation = false;
            settingItems = new Dictionary<string, SettingItemInfo>();
            settingGroups = new Dictionary<string, SettingGroupInfo>();
            groupPanels = new Dictionary<string, Panel>();
            toolItemMapping = new Dictionary<string, IHierarchicalItem<T>>();
            controlGenerator = new SettingControlGenerator(Theme);

            dataMode = SettingsDataMode.Single;
            currentListIndex = -1;
            isInitialized = false;
            suppressSave = false;
            allowScrollAnimation = false;  // 初始化时不允许滚动动画

            // 初始化滚动防抖定时器
            scrollDebounceTimer = new Timer { Interval = 100 };
            scrollDebounceTimer.Tick += ScrollDebounceTimer_Tick;

            CreateLayout();
            WireEvents();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 当前数据对象
        /// </summary>
        [Browsable(false)]
        public T CurrentData => currentData;

        /// <summary>
        /// 列表数据
        /// </summary>
        [Browsable(false)]
        public List<T> ListData => listData;

        /// <summary>
        /// 层次化数据
        /// </summary>
        [Browsable(false)]
        public IHierarchicalItem<T> HierarchicalData => hierarchicalData;

        /// <summary>
        /// 当前选中的层次项
        /// </summary>
        [Browsable(false)]
        public IHierarchicalItem<T> SelectedHierarchicalItem => selectedHierarchicalItem;

        /// <summary>
        /// 数据模式
        /// </summary>
        [Browsable(false)]
        public SettingsDataMode DataMode => dataMode;

        /// <summary>
        /// 左侧工具列表
        /// </summary>
        [Browsable(false)]
        public FluentToolList ToolList => toolList;

        /// <summary>
        /// 内容面板
        /// </summary>
        [Browsable(false)]
        public Panel ContentPanel => contentPanel;

        /// <summary>
        /// 工具列表宽度
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(200)]
        public int ToolListWidth
        {
            get => toolList?.Width ?? TOOL_LIST_WIDTH;
            set
            {
                if (toolList != null && value > 100)
                {
                    toolList.Width = value;
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("是否启用滚动时的动画效果")]
        public bool EnableScrollAnimation
        {
            get => enableScrollAnimation;
            set => enableScrollAnimation = value;
        }

        #endregion

        #region 初始化

        private void ScrollDebounceTimer_Tick(object sender, EventArgs e)
        {
            scrollDebounceTimer.Stop();
            isScrolling = false;
        }

        private void CreateLayout()
        {
            // 创建主容器
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill
            };

            // 创建左侧工具列表
            toolList = new FluentToolList
            {
                Dock = DockStyle.Left,
                Width = TOOL_LIST_WIDTH,
                UseTheme = true
            };

            if (Theme != null)
            {
                toolList.ThemeName = Theme.Name;
            }

            // 创建分隔线
            var splitter = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = Theme?.Colors.Border ?? SystemColors.ControlDark
            };

            // 创建右侧滚动容器
            scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(CONTENT_PADDING)
            };

            // 创建内容面板
            contentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0)
            };

            scrollContainer.Controls.Add(contentPanel);

            // 添加到主容器(注意顺序)
            mainContainer.Controls.Add(scrollContainer);
            mainContainer.Controls.Add(splitter);
            mainContainer.Controls.Add(toolList);

            // 添加到对话框
            AddCustomControl(mainContainer);
        }

        private void WireEvents()
        {
            // 工具列表选中项变更
            toolList.SelectedItemChanged += ToolList_SelectedItemChanged;
            toolList.ItemClick += ToolList_ItemClick;

            // 窗体显示时触发初始化
            this.Shown += FluentSettingsPanel_Shown;
        }

        #endregion

        #region 事件处理

        private void FluentSettingsPanel_Shown(object sender, EventArgs e)
        {
            if (isInitialized)
            {
                return;
            }

            // 触发初始化事件
            var args = new SettingsInitializeEventArgs<T>();
            OnInitializeData(args);

            // 根据返回的数据模式加载
            switch (args.DataMode)
            {
                case SettingsDataMode.Hierarchical:
                    if (args.HierarchicalData != null)
                    {
                        LoadHierarchicalData(args.HierarchicalData);
                    }
                    else
                    {
                        LoadData(new T());
                    }
                    break;

                case SettingsDataMode.List:
                    if (args.ListData != null && args.ListData.Count > 0)
                    {
                        LoadListData(args.ListData);
                    }
                    else
                    {
                        LoadData(new T());
                    }
                    break;

                case SettingsDataMode.Single:
                default:
                    LoadData(args.Data ?? new T());
                    break;
            }

            isInitialized = true;

            // 延迟启用滚动动画, 避免初始化时的滚动问题
            var enableScrollTimer = new Timer { Interval = 300 };
            enableScrollTimer.Tick += (s, ev) =>
            {
                enableScrollTimer.Stop();
                enableScrollTimer.Dispose();
                allowScrollAnimation = true;
            };
            enableScrollTimer.Start();
        }

        private void ToolList_SelectedItemChanged(object sender, FluentToolListItemEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            HandleToolItemSelection(e.Item);
        }

        private void ToolList_ItemClick(object sender, FluentToolListItemEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            // 滚动到对应的分组
            string groupKey = e.Item.Name;
            if (groupPanels.TryGetValue(groupKey, out Panel groupPanel))
            {
                ScrollToControl(groupPanel);
            }
        }

        private void HandleToolItemSelection(FluentToolListItem item)
        {
            // 防止重复滚动
            if (isScrolling)
            {
                return;
            }

            // 保存当前数据
            if (!suppressSave)
            {
                SaveCurrentDataToSource();
            }

            // 层次化模式
            if (dataMode == SettingsDataMode.Hierarchical)
            {
                if (toolItemMapping.TryGetValue(item.Id, out IHierarchicalItem<T> hierarchicalItem))
                {
                    selectedHierarchicalItem = hierarchicalItem;

                    if (hierarchicalItem.CurrentItem != null)
                    {
                        currentData = hierarchicalItem.CurrentItem;
                        LoadDataToControls(currentData);

                        OnItemSelected(new SettingsItemSelectedEventArgs<T>(hierarchicalItem));
                    }
                }
            }
            // 列表模式
            else if (dataMode == SettingsDataMode.List)
            {
                if (item.Tag is int index && index >= 0 && index < listData.Count)
                {
                    currentListIndex = index;
                    currentData = listData[index];
                    LoadDataToControls(currentData);

                    OnItemSelected(new SettingsItemSelectedEventArgs<T>(index));
                }
            }
            // 单一模式 - 滚动到分组
            else
            {
                if (groupPanels.TryGetValue(item.Name, out Panel groupPanel))
                {
                    // 只有在允许滚动动画时才执行
                    if (allowScrollAnimation)
                    {
                        ScrollToControl(groupPanel);
                    }
                    else
                    {
                        // 直接定位, 不使用动画
                        scrollContainer.AutoScrollPosition = new Point(0, groupPanel.Top);
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (DialogResult == DialogResult.OK)
            {
                // 保存当前编辑的数据
                SaveCurrentDataToSource();

                // 触发保存事件
                var saveArgs = new SettingsSaveEventArgs<T>
                {
                    DataMode = dataMode
                };

                switch (dataMode)
                {
                    case SettingsDataMode.Hierarchical:
                        saveArgs.HierarchicalData = hierarchicalData;
                        break;
                    case SettingsDataMode.List:
                        saveArgs.ListData = listData;
                        break;
                    default:
                        saveArgs.Data = currentData;
                        break;
                }

                // 验证
                var errors = ValidateSettings();
                if (errors.Count > 0)
                {
                    saveArgs.ValidationErrors = errors;
                }

                OnSaveData(saveArgs);

                if (saveArgs.Cancel)
                {
                    e.Cancel = true;
                    if (!string.IsNullOrEmpty(saveArgs.CancelReason))
                    {
                        FluentDialog.Show(this, saveArgs.CancelReason, "保存失败", DialogType.Error);
                    }
                    else if (saveArgs.ValidationErrors.Count > 0)
                    {
                        FluentDialog.Show(this, string.Join("\n", saveArgs.ValidationErrors), "验证失败", DialogType.Warning);
                    }
                }
            }
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 加载单个数据对象
        /// </summary>
        public void LoadData(T data)
        {
            if (data == null)
            {
                data = new T();
            }

            currentData = data;
            dataMode = SettingsDataMode.Single;
            listData = null;
            hierarchicalData = null;
            currentListIndex = -1;

            GenerateSettingsUI();
            LoadDataToControls(data);
        }

        /// <summary>
        /// 加载列表数据
        /// </summary>
        public void LoadListData(List<T> data)
        {
            if (data == null || data.Count == 0)
            {
                data = new List<T> { new T() };
            }

            listData = data;
            dataMode = SettingsDataMode.List;
            hierarchicalData = null;
            currentListIndex = 0;
            currentData = data[0];

            // 生成工具列表
            BuildToolListForListData(data);

            // 生成设置UI
            GenerateSettingsUI();

            // 加载第一项数据
            LoadDataToControls(currentData);

            // 选中第一项
            if (toolList.Categories.Count > 0 && toolList.Categories[0].Items.Count > 0)
            {
                suppressSave = true;
                toolList.SelectedItem = toolList.Categories[0].Items[0];
                suppressSave = false;
            }
        }

        /// <summary>
        /// 加载层次化数据
        /// </summary>
        public void LoadHierarchicalData(IHierarchicalItem<T> data)
        {
            if (data == null)
            {
                LoadData(new T());
                return;
            }

            hierarchicalData = data;
            dataMode = SettingsDataMode.Hierarchical;
            listData = null;
            currentListIndex = -1;

            // 清除映射
            toolItemMapping.Clear();

            // 生成工具列表
            BuildToolListFromHierarchy(data);

            // 生成设置UI
            GenerateSettingsUI();

            // 加载根节点数据(如果有)
            if (data.CurrentItem != null)
            {
                currentData = data.CurrentItem;
                selectedHierarchicalItem = data;
                LoadDataToControls(currentData);
            }
            // 否则尝试加载第一个有数据的子项
            else
            {
                var firstWithData = data.GetAllDescendants().FirstOrDefault(x => x.CurrentItem != null);
                if (firstWithData != null)
                {
                    currentData = firstWithData.CurrentItem;
                    selectedHierarchicalItem = firstWithData;
                    LoadDataToControls(currentData);
                }
            }

            // 选中第一项
            if (toolList.Categories.Count > 0 && toolList.Categories[0].Items.Count > 0)
            {
                suppressSave = true;
                toolList.SelectedItem = toolList.Categories[0].Items[0];
                suppressSave = false;
            }
        }

        private void BuildToolListForListData(List<T> data)
        {
            toolList.Categories.Clear();

            var category = new FluentToolListCategory("数据项");

            for (int i = 0; i < data.Count; i++)
            {
                var displayText = GetItemDisplayText(data[i], i);
                var item = new FluentToolListItem(displayText)
                {
                    Tag = i
                };
                category.Add(item);
            }

            toolList.Categories.Add(category);
        }

        private void BuildToolListFromHierarchy(IHierarchicalItem<T> root)
        {
            toolList.Categories.Clear();

            // 如果根节点有子项, 将每个一级子项作为分组
            if (root.HasChildren())
            {
                foreach (var child in root.Childs)
                {
                    var category = new FluentToolListCategory(child.DisplayText)
                    {
                        Tag = child,
                        IsExpanded = child.IsExpanded
                    };

                    // 建立映射
                    toolItemMapping[category.Id] = child;

                    // 如果子项还有子项, 添加为分组的项目
                    if (child.HasChildren())
                    {
                        foreach (var grandChild in child.Childs)
                        {
                            var toolItem = new FluentToolListItem(grandChild.DisplayText)
                            {
                                Tag = grandChild,
                                Image = grandChild.Icon,
                                Enabled = grandChild.IsEnabled
                            };

                            toolItemMapping[toolItem.Id] = grandChild;
                            category.Add(toolItem);
                        }
                    }
                    else if (child.CurrentItem != null)
                    {
                        // 如果没有孙子项但有数据, 添加自身作为项目
                        var toolItem = new FluentToolListItem(child.DisplayText)
                        {
                            Tag = child,
                            Image = child.Icon,
                            Enabled = child.IsEnabled
                        };
                        toolItemMapping[toolItem.Id] = child;
                        category.Add(toolItem);
                    }

                    toolList.Categories.Add(category);
                }
            }
            else if (root.CurrentItem != null)
            {
                // 只有根节点有数据
                var category = new FluentToolListCategory(root.DisplayText);
                var toolItem = new FluentToolListItem(root.DisplayText)
                {
                    Tag = root,
                    Image = root.Icon
                };
                toolItemMapping[toolItem.Id] = root;
                category.Add(toolItem);
                toolList.Categories.Add(category);
            }
        }

        private string GetItemDisplayText(T item, int index)
        {
            if (item == null)
            {
                return $"项目 {index + 1}";
            }

            // 尝试获取 Name 或 DisplayName 属性
            var nameProperty = typeof(T).GetProperty("Name") ?? typeof(T).GetProperty("DisplayName");
            if (nameProperty != null)
            {
                var value = nameProperty.GetValue(item);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    return value.ToString();
                }
            }

            return $"项目 {index + 1}";
        }

        #endregion

        #region UI生成

        private void GenerateSettingsUI()
        {
            contentPanel.Controls.Clear();
            settingItems.Clear();
            settingGroups.Clear();
            groupPanels.Clear();

            // 生成设置项信息
            var items = controlGenerator.GenerateSettingItems<T>();

            // 按分组组织
            var groups = items.GroupBy(i => i.GroupName ?? "常规")
                             .OrderBy(g => g.Min(i => i.Order))
                             .ToList();

            // 单一模式下生成工具列表导航
            if (dataMode == SettingsDataMode.Single)
            {
                BuildToolListForGroups(groups);
            }

            // 生成分组面板
            foreach (var group in groups)
            {
                var groupInfo = new SettingGroupInfo(group.Key);
                var groupItems = group.ToList();
                groupInfo.Items = groupItems;

                // 创建分组面板
                var groupPanel = CreateGroupPanel(groupInfo, groupItems);
                groupPanels[group.Key] = groupPanel;
                settingGroups[group.Key] = groupInfo;

                contentPanel.Controls.Add(groupPanel);

                // 保存设置项引用
                foreach (var item in groupItems)
                {
                    settingItems[item.PropertyName] = item;
                }
            }

            // 选中第一个工具项
            SelectFirstToolItem();
        }

        private void BuildToolListForGroups(IEnumerable<IGrouping<string, SettingItemInfo>> groups)
        {
            toolList.Categories.Clear();

            var category = new FluentToolListCategory("设置");

            foreach (var group in groups)
            {
                var toolItem = new FluentToolListItem(group.Key);
                category.Add(toolItem);
            }

            toolList.Categories.Add(category);
        }

        private void SelectFirstToolItem()
        {
            if (toolList.Categories.Count > 0 && toolList.Categories[0].Items.Count > 0)
            {
                suppressSave = true;
                toolList.SelectedItem = toolList.Categories[0].Items[0];
                suppressSave = false;
            }
        }

        private Panel CreateGroupPanel(SettingGroupInfo groupInfo, List<SettingItemInfo> items)
        {
            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24),
                Margin = new Padding(0)
            };

            int top = 0;
            int contentWidth = scrollContainer.Width - CONTENT_PADDING * 2 - SystemInformation.VerticalScrollBarWidth;

            // 分组标题
            var titleLabel = new Label
            {
                Text = groupInfo.Name,
                Location = new Point(0, top),
                AutoSize = true,
                Font = Theme?.Typography.Title ?? new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold),
                ForeColor = Theme?.Colors.TextPrimary ?? SystemColors.ControlText
            };
            panel.Controls.Add(titleLabel);
            top += titleLabel.PreferredHeight + 8;

            // 分隔线
            var separator = new Panel
            {
                Location = new Point(0, top),
                Size = new Size(contentWidth > 0 ? contentWidth : 500, 1),
                BackColor = Theme?.Colors.BorderLight ?? SystemColors.ControlLight
            };
            panel.Controls.Add(separator);
            top += 16;

            // 添加设置项
            foreach (var item in items)
            {
                var itemPanel = CreateSettingItemPanel(item);
                itemPanel.Location = new Point(0, top);
                panel.Controls.Add(itemPanel);
                top += itemPanel.Height + 12;
            }

            groupInfo.ContentPanel = panel;
            return panel;
        }

        private Panel CreateSettingItemPanel(SettingItemInfo itemInfo)
        {
            int labelWidth = controlGenerator.LabelWidth;
            int controlWidth = controlGenerator.ControlWidth;
            int controlHeight = controlGenerator.ControlHeight;
            int itemSpacing = 8;

            bool hasDescription = !string.IsNullOrEmpty(itemInfo.Description);
            int panelHeight = controlHeight + (hasDescription ? 20 : 0);

            var panel = new Panel
            {
                Size = new Size(labelWidth + controlWidth + itemSpacing, panelHeight),
                Margin = new Padding(0)
            };

            // 标签
            var label = new Label
            {
                Text = itemInfo.DisplayName,
                Location = new Point(0, (controlHeight - 20) / 2),
                Size = new Size(labelWidth, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Theme?.Colors.TextPrimary ?? SystemColors.ControlText,
                Font = Theme?.Typography.Body ?? SystemFonts.DefaultFont
            };
            itemInfo.LabelControl = label;
            panel.Controls.Add(label);

            // 编辑器控件
            var editor = controlGenerator.CreateEditorControl(itemInfo);
            if (editor != null)
            {
                editor.Location = new Point(labelWidth + itemSpacing, 0);
                editor.Size = new Size(controlWidth, controlHeight);

                // 绑定值变更事件
                WireEditorEvents(editor, itemInfo);

                panel.Controls.Add(editor);
            }

            // 描述标签
            if (hasDescription)
            {
                var descLabel = new Label
                {
                    Text = itemInfo.Description,
                    Location = new Point(labelWidth + itemSpacing, controlHeight + 2),
                    Size = new Size(controlWidth, 16),
                    ForeColor = Theme?.Colors.TextSecondary ?? SystemColors.GrayText,
                    Font = Theme?.Typography.Caption ?? new Font(SystemFonts.DefaultFont.FontFamily, 8f)
                };
                itemInfo.DescriptionLabel = descLabel;
                panel.Controls.Add(descLabel);
            }

            itemInfo.ContainerPanel = panel;
            return panel;
        }

        private void WireEditorEvents(Control editor, SettingItemInfo itemInfo)
        {
            if (editor is FluentTextBox textBox)
            {
                textBox.ValueChanged += (s, e) =>
                {
                    OnSettingValueChanged(new SettingValueChangedEventArgs(
                        itemInfo.PropertyName, null, textBox.Value)
                    { ItemInfo = itemInfo });
                };
            }
            else if (editor is FluentCheckBox checkBox)
            {
                checkBox.CheckedChanged += (s, e) =>
                {
                    OnSettingValueChanged(new SettingValueChangedEventArgs(
                        itemInfo.PropertyName, null, checkBox.Checked)
                    { ItemInfo = itemInfo });
                };
            }
            else if (editor is ComboBox comboBox)
            {
                comboBox.SelectedIndexChanged += (s, e) =>
                {
                    OnSettingValueChanged(new SettingValueChangedEventArgs(
                        itemInfo.PropertyName, null, comboBox.SelectedItem)
                    { ItemInfo = itemInfo });
                };
            }
            else if (editor is DateTimePicker datePicker)
            {
                datePicker.ValueChanged += (s, e) =>
                {
                    OnSettingValueChanged(new SettingValueChangedEventArgs(
                        itemInfo.PropertyName, null, datePicker.Value)
                    { ItemInfo = itemInfo });
                };
            }
        }

        #endregion

        #region 数据绑定

        private void LoadDataToControls(T data)
        {
            if (data == null)
            {
                return;
            }

            suppressSave = true;

            foreach (var kvp in settingItems)
            {
                var itemInfo = kvp.Value;
                if (itemInfo.PropertyInfo != null && itemInfo.EditorControl != null)
                {
                    try
                    {
                        var value = itemInfo.PropertyInfo.GetValue(data);
                        SetControlValue(itemInfo.EditorControl, value, itemInfo);
                    }
                    catch
                    {
                        // 忽略加载错误
                    }
                }
            }

            suppressSave = false;
        }

        private void SetControlValue(Control control, object value, SettingItemInfo itemInfo)
        {
            if (control == null)
            {
                return;
            }

            try
            {
                if (control is FluentTextBox textBox)
                {
                    textBox.Text = ConvertToString(value, itemInfo);
                }
                else if (control is FluentCheckBox checkBox)
                {
                    checkBox.Checked = value != null && Convert.ToBoolean(value);
                }
                else if (control is FluentComboBox comboBox)
                {
                    if (itemInfo.PropertyType.IsEnum ||
                        (Nullable.GetUnderlyingType(itemInfo.PropertyType)?.IsEnum ?? false))
                    {
                        comboBox.SelectedItem = value;
                    }
                    else
                    {
                        comboBox.SelectedItem = value;
                    }
                }              
                else if (control is FluentDateTimePicker datePicker)
                {
                    if (value is DateTime dt)
                    {
                        datePicker.Value = dt;
                    }
                    else if (value == null)
                    {
                        datePicker.Value = DateTime.Now;
                    }
                }
                else if (control is FluentColorPicker colorPicker)
                {
                    if (value is Color color)
                    {
                        colorPicker.SelectedColor = color;
                    }
                }
            }
            catch
            {
                // 忽略设置错误
            }
        }

        private object GetControlValue(Control control, SettingItemInfo itemInfo)
        {
            if (control == null)
            {
                return null;
            }

            try
            {
                if (control is FluentTextBox textBox)
                {
                    return ConvertFromString(textBox.Text, itemInfo.PropertyType, itemInfo);
                }
                else if (control is FluentCheckBox checkBox)
                {
                    return checkBox.Checked;
                }
                else if (control is ComboBox comboBox)
                {
                    return comboBox.SelectedItem;
                }
                else if (control is DateTimePicker datePicker)
                {
                    return datePicker.Value;
                }
                else if (control is Panel colorPanel && colorPanel.Tag is Panel colorPreview)
                {
                    return colorPreview.BackColor;
                }
            }
            catch
            {
                // 返回默认值
            }

            return itemInfo.DefaultValue;
        }

        private string ConvertToString(object value, SettingItemInfo itemInfo)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is float f)
            {
                return f.ToString($"F{itemInfo.DecimalPlaces}");
            }

            if (value is double d)
            {
                return d.ToString($"F{itemInfo.DecimalPlaces}");
            }

            if (value is decimal m)
            {
                return m.ToString($"F{itemInfo.DecimalPlaces}");
            }

            return value.ToString();
        }

        private object ConvertFromString(string text, Type targetType, SettingItemInfo itemInfo)
        {
            if (string.IsNullOrEmpty(text))
            {
                return GetDefaultValue(targetType);
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlyingType == typeof(string))
                {
                    return text;
                }

                if (underlyingType == typeof(int))
                {
                    return int.TryParse(text, out int i) ? i : 0;
                }

                if (underlyingType == typeof(long))
                {
                    return long.TryParse(text, out long l) ? l : 0L;
                }

                if (underlyingType == typeof(short))
                {
                    return short.TryParse(text, out short s) ? s : (short)0;
                }

                if (underlyingType == typeof(byte))
                {
                    return byte.TryParse(text, out byte b) ? b : (byte)0;
                }

                if (underlyingType == typeof(float))
                {
                    return float.TryParse(text, out float f) ? f : 0f;
                }

                if (underlyingType == typeof(double))
                {
                    return double.TryParse(text, out double d) ? d : 0.0;
                }

                if (underlyingType == typeof(decimal))
                {
                    return decimal.TryParse(text, out decimal m) ? m : 0m;
                }
            }
            catch
            {
                return GetDefaultValue(targetType);
            }

            return text;
        }

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private void SaveCurrentDataToSource()
        {
            if (currentData == null)
            {
                currentData = new T();
            }

            foreach (var kvp in settingItems)
            {
                var itemInfo = kvp.Value;
                if (itemInfo.PropertyInfo != null && itemInfo.EditorControl != null && !itemInfo.ReadOnly)
                {
                    try
                    {
                        var value = GetControlValue(itemInfo.EditorControl, itemInfo);
                        if (value != null)
                        {
                            var targetType = Nullable.GetUnderlyingType(itemInfo.PropertyType) ?? itemInfo.PropertyType;
                            var convertedValue = Convert.ChangeType(value, targetType);
                            itemInfo.PropertyInfo.SetValue(currentData, convertedValue);
                        }
                        else if (Nullable.GetUnderlyingType(itemInfo.PropertyType) != null)
                        {
                            // 可空类型可以设置为 null
                            itemInfo.PropertyInfo.SetValue(currentData, null);
                        }
                    }
                    catch
                    {
                        // 忽略保存错误
                    }
                }
            }

            // 更新数据源
            if (dataMode == SettingsDataMode.List && currentListIndex >= 0 && currentListIndex < listData.Count)
            {
                listData[currentListIndex] = currentData;
            }
            // 层次化模式数据已通过引用更新
        }

        #endregion

        #region 滚动定位

        private new void ScrollToControl(Control control)
        {
            if (control == null || scrollContainer == null)
            {
                return;
            }

            if (isScrolling)
            {
                return;  // 防止重复滚动
            }

            isScrolling = true;
            scrollDebounceTimer.Stop();
            scrollDebounceTimer.Start();

            // 计算目标位置
            int targetY = control.Top;
            int currentY = -scrollContainer.AutoScrollPosition.Y;

            // 如果距离很小, 直接定位
            if (Math.Abs(targetY - currentY) < 10)
            {
                scrollContainer.AutoScrollPosition = new Point(0, targetY);
                isScrolling = false;
                return;
            }

            // 平滑滚动
            if (EnableScrollAnimation && allowScrollAnimation)
            {
                AnimateScrollTo(targetY);
            }
            else
            {
                scrollContainer.AutoScrollPosition = new Point(0, targetY);
                isScrolling = false;
            }
        }

        private void AnimateScrollTo(int targetY)
        {
            int startY = -scrollContainer.AutoScrollPosition.Y;
            int distance = targetY - startY;

            if (Math.Abs(distance) < 5)
            {
                isScrolling = false;
                return;
            }

            int duration = Theme?.Animation.FastDuration ?? 150;  // 使用较短的动画时长
            int steps = Math.Max(1, duration / 16);
            int currentStep = 0;

            var timer = new Timer { Interval = 16 };
            timer.Tick += (s, e) =>
            {
                currentStep++;
                float progress = Math.Min(1f, (float)currentStep / steps);
                float easedProgress = (float)Easing.CubicOut(progress);

                int newY = startY + (int)(distance * easedProgress);

                try
                {
                    scrollContainer.AutoScrollPosition = new Point(0, newY);
                }
                catch
                {
                    // 忽略可能的异常
                }

                if (currentStep >= steps)
                {
                    timer.Stop();
                    timer.Dispose();

                    try
                    {
                        scrollContainer.AutoScrollPosition = new Point(0, targetY);
                    }
                    catch { }

                    isScrolling = false;
                }
            };
            timer.Start();
        }

        #endregion

        #region 事件触发

        protected virtual void OnInitializeData(SettingsInitializeEventArgs<T> e)
        {
            InitializeData?.Invoke(this, e);
        }

        protected virtual void OnSaveData(SettingsSaveEventArgs<T> e)
        {
            SaveData?.Invoke(this, e);
        }

        protected virtual void OnSettingValueChanged(SettingValueChangedEventArgs e)
        {
            if (!suppressSave)
            {
                SettingValueChanged?.Invoke(this, e);
            }
        }

        protected virtual void OnItemSelected(SettingsItemSelectedEventArgs<T> e)
        {
            ItemSelected?.Invoke(this, e);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 刷新设置项值
        /// </summary>
        public void RefreshValues()
        {
            if (currentData != null)
            {
                LoadDataToControls(currentData);
            }
        }

        /// <summary>
        /// 获取指定属性的当前值
        /// </summary>
        public object GetSettingValue(string propertyName)
        {
            if (settingItems.TryGetValue(propertyName, out var itemInfo))
            {
                return GetControlValue(itemInfo.EditorControl, itemInfo);
            }
            return null;
        }

        /// <summary>
        /// 设置指定属性的值
        /// </summary>
        public void SetSettingValue(string propertyName, object value)
        {
            if (settingItems.TryGetValue(propertyName, out var itemInfo))
            {
                SetControlValue(itemInfo.EditorControl, value, itemInfo);
            }
        }

        /// <summary>
        /// 验证所有设置项
        /// </summary>
        public List<string> ValidateSettings()
        {
            var errors = new List<string>();

            foreach (var kvp in settingItems)
            {
                var itemInfo = kvp.Value;

                // 检查范围
                if (itemInfo.EditorControl is FluentTextBox textBox)
                {
                    if (!textBox.IsValueInRange())
                    {
                        var msg = textBox.GetRangeValidationMessage();
                        errors.Add($"{itemInfo.DisplayName}: {msg ?? "值超出范围"}");
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// 重置所有设置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            currentData = new T();
            LoadDataToControls(currentData);
        }

        /// <summary>
        /// 获取设置项信息
        /// </summary>
        public SettingItemInfo GetSettingItemInfo(string propertyName)
        {
            settingItems.TryGetValue(propertyName, out var itemInfo);
            return itemInfo;
        }

        /// <summary>
        /// 显示/隐藏设置项
        /// </summary>
        public void SetSettingVisibility(string propertyName, bool visible)
        {
            if (settingItems.TryGetValue(propertyName, out var itemInfo))
            {
                if (itemInfo.ContainerPanel != null)
                {
                    itemInfo.ContainerPanel.Visible = visible;
                }
            }
        }

        /// <summary>
        /// 设置设置项是否启用
        /// </summary>
        public void SetSettingEnabled(string propertyName, bool enabled)
        {
            if (settingItems.TryGetValue(propertyName, out var itemInfo))
            {
                if (itemInfo.EditorControl != null)
                {
                    itemInfo.EditorControl.Enabled = enabled;
                }
            }
        }

        #endregion

        #region 资源释放

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                scrollDebounceTimer?.Stop();
                scrollDebounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 设置项

    // <summary>
    /// 设置项信息
    /// </summary>
    public class SettingItemInfo
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 编辑器控件
        /// </summary>
        public Control EditorControl { get; set; }

        /// <summary>
        /// 标签控件
        /// </summary>
        public Label LabelControl { get; set; }

        /// <summary>
        /// 描述标签控件
        /// </summary>
        public Label DescriptionLabel { get; set; }

        /// <summary>
        /// 容器面板
        /// </summary>
        public Panel ContainerPanel { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 最小值(数值类型)
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// 最大值(数值类型)
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// 小数位数
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// 占位符
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// 关联的数据源对象
        /// </summary>
        public object DataSource { get; set; }

        /// <summary>
        /// 获取当前编辑器中的值
        /// </summary>
        public object GetValue()
        {
            if (EditorControl == null)
            {
                return null;
            }

            try
            {
                if (EditorControl is FluentTextBox textBox)
                {
                    return ConvertFromString(textBox.Text, PropertyType);
                }
                else if (EditorControl is FluentCheckBox checkBox)
                {
                    return checkBox.Checked;
                }
                else if (EditorControl is ComboBox comboBox)
                {
                    return comboBox.SelectedItem;
                }
                else if (EditorControl is DateTimePicker datePicker)
                {
                    return datePicker.Value;
                }
                else if (EditorControl is Panel panel && panel.Tag is Panel colorPreview)
                {
                    return colorPreview.BackColor;
                }
            }
            catch
            {
                return DefaultValue;
            }

            return null;
        }

        /// <summary>
        /// 设置编辑器中的值
        /// </summary>
        public void SetValue(object value)
        {
            if (EditorControl == null)
            {
                return;
            }

            try
            {
                if (EditorControl is FluentTextBox textBox)
                {
                    textBox.Text = ConvertToString(value);
                }
                else if (EditorControl is FluentCheckBox checkBox)
                {
                    checkBox.Checked = value != null && Convert.ToBoolean(value);
                }
                else if (EditorControl is ComboBox comboBox)
                {
                    comboBox.SelectedItem = value;
                }
                else if (EditorControl is DateTimePicker datePicker)
                {
                    if (value is DateTime dt)
                    {
                        datePicker.Value = dt;
                    }
                }
                else if (EditorControl is Panel panel && panel.Tag is Panel colorPreview)
                {
                    if (value is Color color)
                    {
                        colorPreview.BackColor = color;
                    }
                }
            }
            catch
            {
                // 忽略转换错误
            }
        }

        private object ConvertFromString(string text, Type targetType)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
            {
                return text;
            }

            if (underlyingType == typeof(int))
            {
                return int.TryParse(text, out int i) ? i : 0;
            }

            if (underlyingType == typeof(long))
            {
                return long.TryParse(text, out long l) ? l : 0L;
            }

            if (underlyingType == typeof(short))
            {
                return short.TryParse(text, out short s) ? s : (short)0;
            }

            if (underlyingType == typeof(byte))
            {
                return byte.TryParse(text, out byte b) ? b : (byte)0;
            }

            if (underlyingType == typeof(float))
            {
                return float.TryParse(text, out float f) ? f : 0f;
            }

            if (underlyingType == typeof(double))
            {
                return double.TryParse(text, out double d) ? d : 0.0;
            }

            if (underlyingType == typeof(decimal))
            {
                return decimal.TryParse(text, out decimal m) ? m : 0m;
            }

            return text;
        }

        private string ConvertToString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is float f)
            {
                return f.ToString($"F{DecimalPlaces}");
            }

            if (value is double d)
            {
                return d.ToString($"F{DecimalPlaces}");
            }

            if (value is decimal m)
            {
                return m.ToString($"F{DecimalPlaces}");
            }

            return value.ToString();
        }
    }

    /// <summary>
    /// 设置分组信息
    /// </summary>
    public class SettingGroupInfo
    {
        public SettingGroupInfo()
        {
            Items = new List<SettingItemInfo>();
        }

        public SettingGroupInfo(string name) : this()
        {
            Name = name;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public Image Icon { get; set; }
        public Panel ContentPanel { get; set; }
        public List<SettingItemInfo> Items { get; set; }
    }

    #endregion

    #region 控件生成器

    public class SettingControlGenerator
    {
        private readonly IFluentTheme theme;

        // 布局常量
        private int controlHeight = 32;
        private int labelWidth = 160;
        private int controlWidth = 280;
        private int itemSpacing = 8;
        private int groupSpacing = 24;
        private int descriptionHeight = 18;

        public SettingControlGenerator(IFluentTheme theme = null)
        {
            this.theme = theme ?? ThemeManager.CurrentTheme ?? ThemeManager.DefaultTheme;
        }

        #region 属性

        public int ControlHeight
        {
            get => controlHeight;
            set => controlHeight = Math.Max(24, value);
        }

        public int LabelWidth
        {
            get => labelWidth;
            set => labelWidth = Math.Max(80, value);
        }

        public int ControlWidth
        {
            get => controlWidth;
            set => controlWidth = Math.Max(100, value);
        }

        public int ItemSpacing
        {
            get => itemSpacing;
            set => itemSpacing = Math.Max(0, value);
        }

        public int GroupSpacing
        {
            get => groupSpacing;
            set => groupSpacing = Math.Max(0, value);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从类型生成设置项信息列表
        /// </summary>
        public List<SettingItemInfo> GenerateSettingItems<T>() where T : class
        {
            return GenerateSettingItems(typeof(T));
        }

        /// <summary>
        /// 从类型生成设置项信息列表
        /// </summary>
        public List<SettingItemInfo> GenerateSettingItems(Type type)
        {
            var items = new List<SettingItemInfo>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 获取类级别的默认分组
            var classGroupAttr = type.GetCustomAttribute<SettingGroupAttribute>();
            string defaultGroup = classGroupAttr?.GroupName ?? "常规";

            foreach (var property in properties)
            {
                // 检查是否应该忽略
                if (ShouldIgnoreProperty(property))
                {
                    continue;
                }

                // 检查是否支持的类型
                if (!IsSupportedType(property.PropertyType))
                {
                    continue;
                }

                var itemInfo = CreateSettingItemInfo(property, defaultGroup);
                if (itemInfo != null)
                {
                    items.Add(itemInfo);
                }
            }

            // 按分组和顺序排序
            return items
                .OrderBy(i => i.GroupName ?? "")
                .ThenBy(i => i.Order)
                .ThenBy(i => i.DisplayName)
                .ToList();
        }

        /// <summary>
        /// 为设置项创建编辑器控件
        /// </summary>
        public Control CreateEditorControl(SettingItemInfo itemInfo)
        {
            if (itemInfo == null)
            {
                return null;
            }

            Control control = null;
            Type propertyType = Nullable.GetUnderlyingType(itemInfo.PropertyType) ?? itemInfo.PropertyType;

            // 字符串类型
            if (propertyType == typeof(string))
            {
                control = CreateTextBox(itemInfo, InputFormat.Text);
            }
            // 整数类型
            else if (IsIntegerType(propertyType))
            {
                control = CreateTextBox(itemInfo, InputFormat.Integer);
            }
            // 小数类型
            else if (IsDecimalType(propertyType))
            {
                control = CreateTextBox(itemInfo, InputFormat.Decimal);
            }
            // 布尔类型
            else if (propertyType == typeof(bool))
            {
                control = CreateCheckBox(itemInfo);
            }
            // 日期时间类型
            else if (propertyType == typeof(DateTime))
            {
                control = CreateDateTimePicker(itemInfo);
            }
            // 枚举类型
            else if (propertyType.IsEnum)
            {
                control = CreateEnumComboBox(itemInfo);
            }
            // 颜色类型
            else if (propertyType == typeof(Color))
            {
                control = CreateColorPicker(itemInfo);
            }
            // 结构体列表类型
            else if (IsStructList(propertyType))
            {
                control = CreateStructListComboBox(itemInfo);
            }

            if (control != null)
            {
                itemInfo.EditorControl = control;
                control.Tag = itemInfo;
            }

            return control;
        }

        /// <summary>
        /// 创建完整的设置项面板(包含标签和控件)
        /// </summary>
        public Panel CreateSettingItemPanel(SettingItemInfo itemInfo)
        {
            bool hasDescription = !string.IsNullOrEmpty(itemInfo.Description);
            int panelHeight = controlHeight + (hasDescription ? descriptionHeight + 4 : 0);

            var panel = new Panel
            {
                Width = labelWidth + controlWidth + itemSpacing,
                Height = panelHeight,
                Margin = new Padding(0, 0, 0, itemSpacing)
            };

            // 创建标签
            var label = new Label
            {
                Text = itemInfo.DisplayName,
                Location = new Point(0, (controlHeight - 20) / 2),
                Size = new Size(labelWidth, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = theme?.Colors.TextPrimary ?? SystemColors.ControlText,
                Font = theme?.Typography.Body ?? SystemFonts.DefaultFont
            };
            itemInfo.LabelControl = label;
            panel.Controls.Add(label);

            // 创建编辑器控件
            var editor = CreateEditorControl(itemInfo);
            if (editor != null)
            {
                editor.Location = new Point(labelWidth + itemSpacing, 0);
                editor.Size = new Size(controlWidth, controlHeight);
                panel.Controls.Add(editor);
            }

            // 创建描述标签
            if (hasDescription)
            {
                var descLabel = new Label
                {
                    Text = itemInfo.Description,
                    Location = new Point(labelWidth + itemSpacing, controlHeight + 2),
                    Size = new Size(controlWidth, descriptionHeight),
                    ForeColor = theme?.Colors.TextSecondary ?? SystemColors.GrayText,
                    Font = theme?.Typography.Caption ?? new Font(SystemFonts.DefaultFont.FontFamily, 8f)
                };
                itemInfo.DescriptionLabel = descLabel;
                panel.Controls.Add(descLabel);
            }

            itemInfo.ContainerPanel = panel;
            return panel;
        }

        /// <summary>
        /// 创建分组面板
        /// </summary>
        public Panel CreateGroupPanel(SettingGroupInfo groupInfo, List<SettingItemInfo> items)
        {
            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, groupSpacing),
                Margin = new Padding(0)
            };

            int top = 0;

            // 分组标题
            var titleLabel = new Label
            {
                Text = groupInfo.Name,
                Location = new Point(0, top),
                AutoSize = true,
                Font = theme?.Typography.Title ?? new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold),
                ForeColor = theme?.Colors.TextPrimary ?? SystemColors.ControlText
            };
            panel.Controls.Add(titleLabel);
            top += titleLabel.PreferredHeight + itemSpacing;

            // 分组描述
            if (!string.IsNullOrEmpty(groupInfo.Description))
            {
                var descLabel = new Label
                {
                    Text = groupInfo.Description,
                    Location = new Point(0, top),
                    AutoSize = true,
                    ForeColor = theme?.Colors.TextSecondary ?? SystemColors.GrayText,
                    Font = theme?.Typography.Caption ?? SystemFonts.DefaultFont
                };
                panel.Controls.Add(descLabel);
                top += descLabel.PreferredHeight + itemSpacing;
            }

            // 分隔线
            var separator = new Panel
            {
                Location = new Point(0, top),
                Size = new Size(labelWidth + controlWidth + itemSpacing, 1),
                BackColor = theme?.Colors.BorderLight ?? SystemColors.ControlLight
            };
            panel.Controls.Add(separator);
            top += itemSpacing * 2;

            // 添加设置项
            foreach (var item in items)
            {
                var itemPanel = CreateSettingItemPanel(item);
                itemPanel.Location = new Point(0, top);
                panel.Controls.Add(itemPanel);
                top += itemPanel.Height + itemSpacing;
            }

            groupInfo.ContentPanel = panel;
            return panel;
        }

        #endregion

        #region 类型检查

        private bool ShouldIgnoreProperty(PropertyInfo property)
        {
            // 检查 SettingIgnore 特性
            if (property.GetCustomAttribute<SettingIgnoreAttribute>() != null)
            {
                return true;
            }

            // 检查是否可读写
            if (!property.CanRead || !property.CanWrite)
            {
                return true;
            }

            // 检查索引器
            if (property.GetIndexParameters().Length > 0)
            {
                return true;
            }

            return false;
        }

        private bool IsSupportedType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // 基本类型
            if (underlyingType == typeof(string))
            {
                return true;
            }

            if (underlyingType == typeof(bool))
            {
                return true;
            }

            if (underlyingType == typeof(DateTime))
            {
                return true;
            }

            if (underlyingType == typeof(Color))
            {
                return true;
            }

            // 数值类型
            if (IsIntegerType(underlyingType))
            {
                return true;
            }

            if (IsDecimalType(underlyingType))
            {
                return true;
            }

            // 枚举类型
            if (underlyingType.IsEnum)
            {
                return true;
            }

            // 结构体列表
            if (IsStructList(type))
            {
                return true;
            }

            return false;
        }

        private bool IsIntegerType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType == typeof(int) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(short) ||
                   underlyingType == typeof(byte) ||
                   underlyingType == typeof(uint) ||
                   underlyingType == typeof(ulong) ||
                   underlyingType == typeof(ushort) ||
                   underlyingType == typeof(sbyte);
        }

        private bool IsDecimalType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType == typeof(float) ||
                   underlyingType == typeof(double) ||
                   underlyingType == typeof(decimal);
        }

        private bool IsStructList(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef != typeof(List<>))
            {
                return false;
            }

            var elementType = type.GetGenericArguments()[0];
            return elementType.IsValueType && !elementType.IsPrimitive && elementType != typeof(DateTime);
        }

        #endregion

        #region 创建设置项信息

        private SettingItemInfo CreateSettingItemInfo(PropertyInfo property, string defaultGroup)
        {
            var displayAttr = property.GetCustomAttribute<SettingDisplayAttribute>();
            var rangeAttr = property.GetCustomAttribute<SettingRangeAttribute>();

            var itemInfo = new SettingItemInfo
            {
                PropertyName = property.Name,
                DisplayName = displayAttr?.DisplayName ?? FormatPropertyName(property.Name),
                Description = displayAttr?.Description,
                GroupName = displayAttr?.GroupName ?? defaultGroup,
                PropertyType = property.PropertyType,
                PropertyInfo = property,
                Order = displayAttr?.Order ?? 0,
                ReadOnly = displayAttr?.ReadOnly ?? false,
                Placeholder = displayAttr?.Placeholder
            };

            // 处理范围特性
            if (rangeAttr != null)
            {
                itemInfo.Minimum = rangeAttr.Minimum;
                itemInfo.Maximum = rangeAttr.Maximum;
                itemInfo.DecimalPlaces = rangeAttr.DecimalPlaces;
            }

            return itemInfo;
        }

        private string FormatPropertyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var result = new System.Text.StringBuilder();
            result.Append(char.ToUpper(name[0]));

            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                {
                    result.Append(' ');
                }
                result.Append(name[i]);
            }

            return result.ToString();
        }

        #endregion

        #region 创建控件

        private FluentTextBox CreateTextBox(SettingItemInfo itemInfo, InputFormat format)
        {
            var textBox = new FluentTextBox
            {
                UseTheme = true,
                InputFormat = format,
                Placeholder = itemInfo.Placeholder ?? "",
                ReadOnly = itemInfo.ReadOnly,
                BorderSize = 1
            };

            if (itemInfo.Minimum.HasValue)
            {
                textBox.Minimum = (decimal)itemInfo.Minimum.Value;
            }
            if (itemInfo.Maximum.HasValue)
            {
                textBox.Maximum = (decimal)itemInfo.Maximum.Value;
            }
            if (format == InputFormat.Decimal)
            {
                textBox.DecimalPlaces = itemInfo.DecimalPlaces;
            }

            return textBox;
        }

        private FluentCheckBox CreateCheckBox(SettingItemInfo itemInfo)
        {
            var checkBox = new FluentCheckBox
            {
                Text = "",
                CheckBoxStyle = CheckBoxStyle.Switch,
                Enabled = !itemInfo.ReadOnly,
                SwitchPosition = SwitchPosition.Left,
                BackColor = Color.Transparent,
                UseTheme = true
            };

            return checkBox;
        }

        private Control CreateDateTimePicker(SettingItemInfo itemInfo)
        {
            var picker = new FluentDateTimePicker
            {
                Mode = DateTimePickerMode.DateTime,
                ShowDropDownButton = true,
                IncludeMilliseconds = false,
                Enabled = !itemInfo.ReadOnly,
                UseTheme = true
            };

            return picker;
        }

        private Control CreateEnumComboBox(SettingItemInfo itemInfo)
        {
            var comboBox = new FluentComboBox
            {
                SelectionStyle = ComboBoxSelectionStyle.Single,
                OnlySelection = true,
                Enabled = !itemInfo.ReadOnly,
                UseTheme = true
            };

            // 填充枚举值
            var enumType = Nullable.GetUnderlyingType(itemInfo.PropertyType) ?? itemInfo.PropertyType;
            var enumValues = Enum.GetValues(enumType);
            foreach (var value in enumValues)
            {
                comboBox.Items.Add(value);
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            return comboBox;
        }

        private Control CreateColorPicker(SettingItemInfo itemInfo)
        {
            var colorPicker = new FluentColorPicker
            {
                TextFormat = ColorTextFormat.Hex,
                SelectedColor = Color.Transparent,
                Enabled = !itemInfo.ReadOnly,
                UseTheme = true
            };

            return colorPicker;
        }

        private Control CreateStructListComboBox(SettingItemInfo itemInfo)
        {
            var comboBox = new FluentComboBox
            {
                Enabled = !itemInfo.ReadOnly,
                SelectionStyle = ComboBoxSelectionStyle.Single,
                OnlySelection = true,
                UseTheme = true
            };

            return comboBox;
        }

        #endregion
    }

    #endregion

    #region 枚举和辅助类


    /// <summary>
    /// 设置数据模式
    /// </summary>
    public enum SettingsDataMode
    {
        Single,         // 单个对象模式
        List,           // 列表模式
        Hierarchical    // 层次化模式
    }

    /// <summary>
    /// 设置面板初始化事件参数
    /// </summary>
    public class SettingsInitializeEventArgs<T> : EventArgs where T : class
    {
        public SettingsInitializeEventArgs()
        {
            DataMode = SettingsDataMode.Single;
        }

        public SettingsInitializeEventArgs(T data) : this()
        {
            Data = data;
            DataMode = SettingsDataMode.Single;
        }

        public SettingsInitializeEventArgs(List<T> listData) : this()
        {
            ListData = listData;
            DataMode = SettingsDataMode.List;
        }

        public SettingsInitializeEventArgs(IHierarchicalItem<T> hierarchicalData) : this()
        {
            HierarchicalData = hierarchicalData;
            DataMode = SettingsDataMode.Hierarchical;
        }

        /// <summary>
        /// 单个数据对象
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 层次化数据
        /// </summary>
        public IHierarchicalItem<T> HierarchicalData { get; set; }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<T> ListData { get; set; }

        /// <summary>
        /// 数据模式
        /// </summary>
        public SettingsDataMode DataMode { get; set; }

    }

    /// <summary>
    /// 设置面板保存事件参数
    /// </summary>
    public class SettingsSaveEventArgs<T> : EventArgs where T : class
    {

        public SettingsSaveEventArgs()
        {
            Cancel = false;
            ValidationErrors = new List<string>();
        }

        /// <summary>
        /// 单个数据对象
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 层次化数据
        /// </summary>
        public IHierarchicalItem<T> HierarchicalData { get; set; }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<T> ListData { get; set; }

        /// <summary>
        /// 数据模式
        /// </summary>
        public SettingsDataMode DataMode { get; set; }

        /// <summary>
        /// 是否取消保存
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// 取消原因
        /// </summary>
        public string CancelReason { get; set; }

        /// <summary>
        /// 验证错误信息
        /// </summary>
        public List<string> ValidationErrors { get; set; }
    }

    /// <summary>
    /// 设置值变更事件参数
    /// </summary>
    public class SettingValueChangedEventArgs : EventArgs
    {

        public SettingValueChangedEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// 关联的设置项信息
        /// </summary>
        public SettingItemInfo ItemInfo { get; set; }
    }

    /// <summary>
    /// 设置项选中变更事件参数
    /// </summary>
    public class SettingsItemSelectedEventArgs<T> : EventArgs where T : class
    {
        public SettingsItemSelectedEventArgs(IHierarchicalItem<T> selectedItem)
        {
            SelectedItem = selectedItem;
        }

        public SettingsItemSelectedEventArgs(int listIndex)
        {
            ListIndex = listIndex;
        }

        /// <summary>
        /// 选中的层次项
        /// </summary>
        public IHierarchicalItem<T> SelectedItem { get; set; }

        /// <summary>
        /// 选中项的数据
        /// </summary>
        public T Data => SelectedItem?.CurrentItem;

        /// <summary>
        /// 列表索引(列表模式)
        /// </summary>
        public int ListIndex { get; set; } = -1;

    }

    #endregion
}
