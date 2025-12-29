using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Animation;
using FluentControls.Themes;
using System.Windows.Forms;
using System.Drawing.Design;

namespace FluentControls.Controls
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(ComboBox))]
    [DefaultEvent("SelectedIndexChanged")]
    [DefaultProperty("Items")]
    public class FluentComboBox : FluentControlBase
    {
        private TextBox textBox;
        private Button dropButton;
        private FluentDropDownList dropDownList;

        private ItemCollection items;
        private IList externalDataSource;
        private string displayMember = "";
        private string valueMember = "";

        private ComboBoxSelectionStyle selectionStyle = ComboBoxSelectionStyle.Single;
        private bool onlySelection = false;
        private bool searchable = false;
        private bool isDroppedDown = false;
        private int dropDownItemCount = 8;
        private int itemHeight = 32;
        private Padding dropDownPadding = new Padding(0);
        private int dropDownWidth = 0;
        private int dropDownAnimationDuration = 100;

        private List<object> selectedItems = new List<object>();
        private string separator = ", ";
        private bool isSearching = false; // 是否正在搜索
        private string lastSearchText = ""; // 上次搜索文本

        #region 事件

        [Category("Behavior")]
        [Description("选中项改变时触发")]
        public event EventHandler SelectedIndexChanged;

        [Category("Behavior")]
        [Description("选中值改变时触发")]
        public event EventHandler SelectedValueChanged;

        [Category("Behavior")]
        [Description("下拉列表打开时触发")]
        public event EventHandler DropDown;

        [Category("Behavior")]
        [Description("下拉列表关闭时触发")]
        public event EventHandler DropDownClosed;

        #endregion

        #region 构造函数

        public FluentComboBox()
        {
            items = new ItemCollection(this);
            InitializeComponents();

            Height = 32;
            Width = 200;
        }

        private void InitializeComponents()
        {
            // 文本框
            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(4, 6),
                Width = Width - 36,
                Height = 20,
                Font = new Font("Microsoft YaHei", 9f)
            };
            textBox.TextChanged += OnTextBoxTextChanged;
            textBox.KeyDown += OnTextBoxKeyDown;
            textBox.GotFocus += (s, e) => Invalidate();
            textBox.LostFocus += (s, e) =>
            {
                Invalidate();
                // 失去焦点时停止搜索模式
                if (isSearching && !isDroppedDown)
                {
                    isSearching = false;
                }
            };

            // 下拉按钮
            dropButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Text = "▼",
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            dropButton.FlatAppearance.BorderSize = 0;
            dropButton.Click += OnDropButtonClick;

            // 下拉列表
            dropDownList = new FluentDropDownList(this)
            {
                Visible = false
            };
            dropDownList.ItemClick += OnDropDownItemClick;
            dropDownList.MultiSelectChanged += OnDropDownMultiSelectChanged;

            Controls.Add(textBox);
            Controls.Add(dropButton);

            UpdateLayout();
        }

        #endregion

        #region 属性

        [Category("Data")]
        [Description("项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(UITypeEditor))]
        public ItemCollection Items
        {
            get => items;
        }

        [Category("Data")]
        [Description("数据源")]
        [DefaultValue(null)]
        [AttributeProvider(typeof(IListSource))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Browsable(false)]
        public IList DataSource
        {
            get => externalDataSource;
            set
            {
                if (externalDataSource != value)
                {
                    externalDataSource = value;
                    RefreshDataSource();
                }
            }
        }

        [Category("Data")]
        [Description("显示成员")]
        [DefaultValue("")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get => displayMember;
            set
            {
                displayMember = value;
                if (externalDataSource != null)
                {
                    RefreshDataSource();
                }
            }
        }

        [Category("Data")]
        [Description("值成员")]
        [DefaultValue("")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", typeof(UITypeEditor))]
        public string ValueMember
        {
            get => valueMember;
            set
            {
                valueMember = value;
                if (externalDataSource != null)
                {
                    RefreshDataSource();
                }
            }
        }

        [Category("Behavior")]
        [Description("选择模式")]
        [DefaultValue(ComboBoxSelectionStyle.Single)]
        public ComboBoxSelectionStyle SelectionStyle
        {
            get => selectionStyle;
            set
            {
                selectionStyle = value;
                dropDownList.SelectionStyle = value;

                if (value == ComboBoxSelectionStyle.Multiple)
                {
                    Searchable = false;
                }

                RefreshItems();
            }
        }

        [Category("Behavior")]
        [Description("仅允许选择")]
        [DefaultValue(false)]
        public bool OnlySelection
        {
            get => onlySelection;
            set
            {
                onlySelection = value;
                textBox.ReadOnly = value;

                if (searchable)
                {
                    onlySelection = false;
                }
            }
        }

        [Category("Behavior")]
        [Description("支持搜索")]
        [DefaultValue(false)]
        public bool Searchable
        {
            get => searchable;
            set
            {
                searchable = value;
                if (value)
                {
                    onlySelection = false;
                }
            }
        }

        [Category("Appearance")]
        [Description("下拉动画持续时间(毫秒)")]
        [DefaultValue(100)]
        public int DropDownAnimationDuration
        {
            get => dropDownAnimationDuration;
            set => dropDownAnimationDuration = Math.Max(0, Math.Min(1000, value));
        }

        [Category("Appearance")]
        [Description("下拉项的内边距")]
        public Padding DropDownItemPadding
        {
            get => dropDownList.Padding;
            set
            {
                if (dropDownList != null)
                {
                    dropDownList.Padding = value;
                }
            }
        }

        [Category("Appearance")]
        [Description("下拉项显示数量")]
        [DefaultValue(8)]
        public int DropDownItemCount
        {
            get => dropDownItemCount;
            set
            {
                dropDownItemCount = Math.Max(1, value);
                UpdateDropDownSize();
            }
        }

        [Category("Appearance")]
        [Description("项高度")]
        [DefaultValue(32)]
        public int ItemHeight
        {
            get => itemHeight;
            set
            {
                itemHeight = Math.Max(20, value);
                dropDownList.ItemHeight = itemHeight;
                UpdateDropDownSize();
            }
        }

        [Category("Appearance")]
        [Description("下拉列表宽度")]
        [DefaultValue(0)]
        public int DropDownWidth
        {
            get => dropDownWidth;
            set
            {
                dropDownWidth = value;
                UpdateDropDownSize();
            }
        }

        [Category("Appearance")]
        [Description("多选分隔符")]
        [DefaultValue(", ")]
        public string Separator
        {
            get => separator;
            set => separator = value;
        }


        [Browsable(false)]
        public int SelectedIndex
        {
            get
            {
                if (selectedItems.Count > 0)
                {
                    var itemList = GetCurrentItemList();
                    return itemList.IndexOf(selectedItems[0]);
                }
                return -1;
            }
            set
            {
                var itemList = GetCurrentItemList();
                if (value >= 0 && value < itemList.Count)
                {
                    SelectedItem = itemList[value];
                }
            }
        }

        [Browsable(false)]
        public object SelectedItem
        {
            get => selectedItems.FirstOrDefault();
            set
            {
                var previousItem = SelectedItem;
                selectedItems.Clear();
                if (value != null)
                {
                    selectedItems.Add(value);
                    UpdateTextBox();
                }

                // 触发事件
                if (!Equals(previousItem, value))
                {
                    OnSelectedIndexChanged();
                }
            }
        }

        [Browsable(false)]
        public object SelectedValue
        {
            get
            {
                if (SelectedItem == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(valueMember))
                {
                    return SelectedItem;
                }

                var property = SelectedItem.GetType().GetProperty(valueMember);
                return property?.GetValue(SelectedItem);
            }
            set
            {
                if (value == null)
                {
                    SelectedItem = null;
                    return;
                }

                var itemList = GetCurrentItemList();
                foreach (var item in itemList)
                {
                    var itemValue = string.IsNullOrEmpty(valueMember)
                        ? item
                        : item.GetType().GetProperty(valueMember)?.GetValue(item);

                    if (Equals(itemValue, value))
                    {
                        SelectedItem = item;
                        break;
                    }
                }
            }
        }

        [Browsable(false)]
        public List<object> SelectedItems => new List<object>(selectedItems);

        [Browsable(false)]
        public new string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        [Browsable(false)]
        public bool DroppedDown => isDroppedDown;

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加项
        /// </summary>
        public void AddItem(object item)
        {
            items.Add(item);
        }

        /// <summary>
        /// 移除项
        /// </summary>
        public void RemoveItem(object item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// 清空所有项
        /// </summary>
        public void ClearItems()
        {
            items.Clear();
        }

        /// <summary>
        /// 获取所有选中的文本
        /// </summary>
        public string[] GetSelectedTexts()
        {
            return selectedItems.Select(GetItemText).ToArray();
        }

        /// <summary>
        /// 获取所有选中的值
        /// </summary>
        public object[] GetSelectedValues()
        {
            if (string.IsNullOrEmpty(valueMember))
            {
                return selectedItems.ToArray();
            }

            return selectedItems.Select(item =>
            {
                var property = item.GetType().GetProperty(valueMember);
                return property?.GetValue(item) ?? item;
            }).ToArray();
        }

        /// <summary>
        /// 手动打开下拉列表
        /// </summary>
        public void ShowDropDown()
        {
            if (!isDroppedDown)
            {
                OnDropButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 手动关闭下拉列表
        /// </summary>
        public void CloseDropDown()
        {
            if (isDroppedDown)
            {
                HideDropDown();
            }
        }

        /// <summary>
        /// 设置项图标
        /// </summary>
        public void SetItemIcon(int index, Image icon)
        {
            if (index >= 0 && index < dropDownList.Items.Count)
            {
                dropDownList.Items[index].Icon = icon;
                dropDownList.Invalidate();
            }
        }

        public void SetItemIcon(object item, Image icon)
        {
            var dropItem = dropDownList.Items.FirstOrDefault(i => i.Value == item);
            if (dropItem != null)
            {
                dropItem.Icon = icon;
                dropDownList.Invalidate();
            }
        }

        #endregion

        #region 重写方法

        protected override void DrawBackground(Graphics g)
        {
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var cornerRadius = 4;

            using (var path = GetRoundedRectPath(rect, cornerRadius))
            {
                var backColor = textBox.Focused ? Color.White : GetThemeColor(c => c.Surface, BackColor);
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 内容由子控件(textBox和dropButton)处理
        }

        protected override void DrawBorder(Graphics g)
        {
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var cornerRadius = 4;

            var borderColor = textBox.Focused
                ? GetThemeColor(c => c.Primary, SystemColors.Highlight)
                : GetThemeColor(c => c.Border, SystemColors.ActiveBorder);
            var borderWidth = textBox.Focused ? 2 : 1;

            using (var path = GetRoundedRectPath(rect, cornerRadius))
            using (var pen = new Pen(borderColor, borderWidth))
            {
                g.DrawPath(pen, path);
            }
        }

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            ApplyThemeStyles();
        }

        protected override void ApplyThemeStyles()
        {
            if (!UseTheme || Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;

            textBox.BackColor = BackColor;
            textBox.ForeColor = ForeColor;
            textBox.Font = Theme.Typography.Body;

            dropButton.BackColor = BackColor;
            dropButton.ForeColor = Theme.Colors.TextSecondary;

            dropDownList.ApplyTheme(Theme);

            Invalidate();
        }

        #endregion

        #region 重写Control方法

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null && dropDownList.Parent == null)
            {
                Parent.Controls.Add(dropDownList);
            }
        }

        #endregion

        #region 私有方法

        private void UpdateLayout()
        {
            var buttonWidth = 28;
            var padding = 4;

            textBox.Location = new Point(padding, (Height - textBox.Height) / 2);
            textBox.Width = Width - buttonWidth - padding * 2;

            dropButton.Size = new Size(buttonWidth, Height - 2);
            dropButton.Location = new Point(Width - buttonWidth - 1, 1);
        }

        private void UpdateDropDownSize()
        {
            if (dropDownList == null)
            {
                return;
            }

            var width = dropDownWidth > 0 ? dropDownWidth : Width;

            // 计算高度：显示dropDownItemCount个项, 但不超过实际项数
            var visibleItems = Math.Min(dropDownItemCount, dropDownList.Items.Count);
            var height = visibleItems * itemHeight + 2;

            // 确保至少显示一个项的高度
            if (height < itemHeight + 2)
            {
                height = itemHeight + 2;
            }

            dropDownList.Size = new Size(width, height);
            dropDownList.UpdateScrollBar();
        }

        private List<object> GetCurrentItemList()
        {
            if (externalDataSource != null)
            {
                var list = new List<object>();
                foreach (var item in externalDataSource)
                {
                    list.Add(item);
                }

                return list;
            }
            return items.ToList();
        }

        private void RefreshDataSource()
        {
            RefreshItems();
        }

        private void RefreshItems()
        {
            dropDownList.Items.Clear();

            var itemList = GetCurrentItemList();

            foreach (var item in itemList)
            {
                dropDownList.Items.Add(new DropDownItem
                {
                    Value = item,
                    Text = GetItemText(item),
                    IsSelected = selectedItems.Contains(item)
                });
            }

            UpdateDropDownSize();
        }

        private string GetItemText(object item)
        {
            if (item == null)
            {
                return "";
            }

            if (!string.IsNullOrEmpty(displayMember) && externalDataSource != null)
            {
                var property = item.GetType().GetProperty(displayMember);
                return property?.GetValue(item)?.ToString() ?? item.ToString();
            }

            return item.ToString();
        }

        private void UpdateTextBox()
        {
            if (selectionStyle == ComboBoxSelectionStyle.Single)
            {
                textBox.Text = selectedItems.Count > 0 ? GetItemText(selectedItems[0]) : "";
            }
            else
            {
                var texts = selectedItems.Select(GetItemText);
                textBox.Text = string.Join(separator, texts);
            }
        }

        protected virtual void OnSelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            OnSelectedValueChanged();
        }

        protected virtual void OnSelectedValueChanged()
        {
            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ShowDropDownInternal()
        {
            if (isDroppedDown)
            {
                return;
            }

            isDroppedDown = true;

            // 如果是搜索模式, 显示所有项
            if (isSearching)
            {
                RefreshItems();
                isSearching = false;

                // 找到并选中匹配项
                var searchText = textBox.Text;
                if (!string.IsNullOrEmpty(searchText))
                {
                    for (int i = 0; i < dropDownList.Items.Count; i++)
                    {
                        if (dropDownList.Items[i].Text.IndexOf(searchText,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            dropDownList.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }

            // 计算位置
            var pt = Parent.PointToClient(PointToScreen(new Point(0, Height)));
            dropDownList.Location = pt;

            dropDownList.LockScrollBar();

            // 显示动画
            dropDownList.Height = 0;
            dropDownList.Visible = true;
            dropDownList.BringToFront();

            if (dropDownAnimationDuration > 0)
            {
                AnimationManager.AnimateSize(dropDownList,
                    new Size(dropDownList.Width, GetDropDownHeight()),
                    dropDownAnimationDuration, Easing.CubicOut, () =>
                    {
                        // 修正动画显示时出现滚动条的问题
                        dropDownList.UnlockScrollBar();
                    });
            }
            else
            {
                // 无动画直接显示
                dropDownList.Size = new Size(dropDownList.Width, GetDropDownHeight());
            }

            dropButton.Text = "▲";
            DropDown?.Invoke(this, EventArgs.Empty);
        }

        private void HideDropDown()
        {
            if (!isDroppedDown)
            {
                return;
            }

            isDroppedDown = false;

            dropDownList.LockScrollBar();

            if (dropDownAnimationDuration > 0)
            {
                AnimationManager.AnimateSize(dropDownList,
                    new Size(dropDownList.Width, 0),
                    dropDownAnimationDuration, Easing.CubicIn,
                    () =>
                    {
                        dropDownList.Visible = false;
                        dropDownList.UnlockScrollBar();
                    });
            }
            else
            {
                // 无动画直接隐藏
                dropDownList.Visible = false;
            }

            dropButton.Text = "▼";
            DropDownClosed?.Invoke(this, EventArgs.Empty);
        }

        private int GetDropDownHeight()
        {
            if (dropDownList.Items.Count == 0)
            {
                return itemHeight + 2; // 至少显示一项的高度
            }

            // 显示dropDownItemCount个项的高度, 但不超过实际项数
            var visibleItems = Math.Min(dropDownItemCount, dropDownList.Items.Count);
            return visibleItems * itemHeight + 2;
        }

        private void FilterItems(string searchText)
        {
            if (!searchable || string.IsNullOrEmpty(searchText))
            {
                RefreshItems();
                isSearching = false;
                return;
            }

            isSearching = true;
            lastSearchText = searchText;

            dropDownList.Items.Clear();

            var itemList = GetCurrentItemList();
            foreach (var item in itemList)
            {
                var text = GetItemText(item);
                if (text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    dropDownList.Items.Add(new DropDownItem
                    {
                        Value = item,
                        Text = text,
                        IsSelected = selectedItems.Contains(item)
                    });
                }
            }

            if (dropDownList.Items.Count > 0)
            {
                dropDownList.SelectedIndex = 0;

                // 如果只有一项完全匹配, 可以自动选中
                if (dropDownList.Items.Count == 1 &&
                    dropDownList.Items[0].Text.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    if (selectionStyle == ComboBoxSelectionStyle.Single)
                    {
                        SelectedItem = dropDownList.Items[0].Value;
                    }
                }
            }

            UpdateDropDownSize();
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion

        #region 事件处理

        private void OnDropButtonClick(object sender, EventArgs e)
        {
            if (isDroppedDown)
            {
                HideDropDown();
            }
            else
            {
                ShowDropDownInternal();
            }
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            if (searchable && !onlySelection)
            {
                FilterItems(textBox.Text);

                // 如果有匹配项且下拉列表未显示, 自动显示
                if (dropDownList.Items.Count > 0 && !isDroppedDown)
                {
                    ShowDropDownInternal();
                }
            }
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (!isDroppedDown)
                {
                    ShowDropDownInternal();
                }
                else if (dropDownList.SelectedIndex < dropDownList.Items.Count - 1)
                {
                    dropDownList.SelectedIndex++;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (isDroppedDown && dropDownList.SelectedIndex > 0)
                {
                    dropDownList.SelectedIndex--;
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (isDroppedDown && dropDownList.SelectedIndex >= 0)
                {
                    if (selectionStyle == ComboBoxSelectionStyle.Single)
                    {
                        SelectedItem = dropDownList.Items[dropDownList.SelectedIndex].Value;
                        HideDropDown();
                    }
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (isDroppedDown)
                {
                    HideDropDown();
                }
            }
        }

        private void OnDropDownItemClick(object sender, DropDownItemEventArgs e)
        {
            if (selectionStyle == ComboBoxSelectionStyle.Single)
            {
                SelectedItem = e.Item.Value;
                HideDropDown();
            }
        }

        private void OnDropDownMultiSelectChanged(object sender, EventArgs e)
        {
            var previousCount = selectedItems.Count;
            selectedItems.Clear();
            foreach (DropDownItem item in dropDownList.Items)
            {
                if (item.IsSelected)
                {
                    selectedItems.Add(item.Value);
                }
            }

            UpdateTextBox();

            // 触发事件
            if (selectedItems.Count != previousCount)
            {
                OnSelectedIndexChanged();
            }
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 项集合类
        /// </summary>
        [ListBindable(false)]
        public class ItemCollection : IList, ICollection, IEnumerable
        {
            private FluentComboBox owner;
            private List<object> innerList;

            internal ItemCollection(FluentComboBox owner)
            {
                this.owner = owner;
                this.innerList = new List<object>();
            }

            public int Count => innerList.Count;

            public bool IsReadOnly => false;

            public bool IsFixedSize => false;

            public bool IsSynchronized => false;

            public object SyncRoot => this;

            public object this[int index]
            {
                get => innerList[index];
                set
                {
                    innerList[index] = value;
                    owner.RefreshItems();
                }
            }

            public int Add(object item)
            {
                innerList.Add(item);
                owner.RefreshItems();
                return innerList.Count - 1;
            }

            public void AddRange(object[] items)
            {
                innerList.AddRange(items);
                owner.RefreshItems();
            }

            public void Clear()
            {
                innerList.Clear();
                owner.selectedItems.Clear();
                owner.UpdateTextBox();
                owner.RefreshItems();
            }

            public bool Contains(object item)
            {
                return innerList.Contains(item);
            }

            public int IndexOf(object item)
            {
                return innerList.IndexOf(item);
            }

            public void Insert(int index, object item)
            {
                innerList.Insert(index, item);
                owner.RefreshItems();
            }

            public void Remove(object item)
            {
                innerList.Remove(item);
                owner.selectedItems.Remove(item);
                owner.UpdateTextBox();
                owner.RefreshItems();
            }

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < innerList.Count)
                {
                    var item = innerList[index];
                    Remove(item);
                }
            }

            public void CopyTo(Array array, int index)
            {
                ((ICollection)innerList).CopyTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return innerList.GetEnumerator();
            }

            internal List<object> ToList()
            {
                return new List<object>(innerList);
            }
        }

        /// <summary>
        /// 下拉列表
        /// </summary>
        private class FluentDropDownList : Control
        {
            private FluentComboBox parent;
            private List<DropDownItem> items = new List<DropDownItem>();
            private int selectedIndex = -1;
            private int hoveredIndex = -1;
            private VScrollBar scrollBar;
            private int scrollOffset = 0;

            private bool scrollBarLocked = false;
            private bool cachedScrollBarVisible = false;
            private int cachedScrollOffset = 0;

            public event EventHandler<DropDownItemEventArgs> ItemClick;
            public event EventHandler MultiSelectChanged;

            public FluentDropDownList(FluentComboBox parent)
            {
                this.parent = parent;

                SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.ResizeRedraw |
                        ControlStyles.OptimizedDoubleBuffer, true);

                scrollBar = new VScrollBar
                {
                    Width = 12,
                    Visible = false
                };
                scrollBar.Scroll += (s, e) =>
                {
                    scrollOffset = scrollBar.Value;
                    Invalidate();
                };

                Controls.Add(scrollBar);

                scrollBar.Visible = false;
            }

            public List<DropDownItem> Items => items;

            public int SelectedIndex
            {
                get => selectedIndex;
                set
                {
                    if (value >= -1 && value < items.Count)
                    {
                        selectedIndex = value;

                        // 确保选中项可见
                        if (selectedIndex >= 0)
                        {
                            EnsureVisible(selectedIndex);
                        }

                        Invalidate();
                    }
                }
            }

            public int ItemHeight { get; set; } = 32;

            public ComboBoxSelectionStyle SelectionStyle { get; set; } = ComboBoxSelectionStyle.Single;

            public void ApplyTheme(IFluentTheme theme)
            {
                if (theme == null)
                {
                    return;
                }

                BackColor = theme.Colors.Background;
                ForeColor = theme.Colors.TextPrimary;
                Font = theme.Typography.Body;
                Invalidate();
            }

            private void EnsureVisible(int index)
            {
                if (scrollBar.Visible)
                {
                    var itemTop = index * ItemHeight;
                    var itemBottom = itemTop + ItemHeight;

                    if (itemTop < scrollOffset)
                    {
                        scrollBar.Value = itemTop;
                        scrollOffset = itemTop;
                    }
                    else if (itemBottom > scrollOffset + Height - 2)
                    {
                        var newOffset = itemBottom - Height + 2;
                        scrollBar.Value = Math.Min(newOffset, scrollBar.Maximum);
                        scrollOffset = scrollBar.Value;
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 背景
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }

                // 设置裁剪区域, 避免绘制超出边界
                var clipRect = new Rectangle(1, 1, Width - 2, Height - 2);
                if (scrollBar.Visible)
                {
                    clipRect.Width -= scrollBar.Width;
                }
                g.SetClip(clipRect);

                // 绘制项
                var y = 1 - scrollOffset; // 从边框内开始
                for (int i = 0; i < items.Count; i++)
                {
                    var itemBounds = new Rectangle(Padding.Left, y, clipRect.Width, ItemHeight);

                    // 只绘制可见的项
                    if (itemBounds.Bottom > 0 && itemBounds.Top < Height)
                    {
                        DrawItem(g, i, itemBounds);
                    }

                    y += ItemHeight;

                    // 如果已经超出可见区域底部, 停止绘制
                    if (y > Height)
                    {
                        break;
                    }
                }

                // 重置裁剪
                g.ResetClip();

                // 边框
                var borderColor = parent.UseTheme && parent.Theme != null
                    ? parent.Theme.Colors.Border
                    : SystemColors.ActiveBorder;

                using (var pen = new Pen(borderColor))
                {
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }

            private void DrawItem(Graphics g, int index, Rectangle bounds)
            {
                var item = items[index];
                var isHovered = index == hoveredIndex;
                var isSelected = index == selectedIndex;

                // 获取主题颜色
                Color hoverColor, selectedColor, textColor, borderColor; // 添加 borderColor 定义
                if (parent.UseTheme && parent.Theme != null)
                {
                    hoverColor = parent.Theme.Colors.SurfaceHover;
                    selectedColor = parent.Theme.Colors.GetColorWithOpacity(parent.Theme.Colors.Primary, 0.1f);
                    textColor = parent.Theme.Colors.TextPrimary;
                    borderColor = parent.Theme.Colors.Border; // 设置边框颜色
                }
                else
                {
                    hoverColor = SystemColors.ControlLight;
                    selectedColor = Color.FromArgb(25, SystemColors.Highlight);
                    textColor = SystemColors.ControlText;
                    borderColor = SystemColors.ActiveBorder; // 设置默认边框颜色
                }

                // 背景
                if (isHovered)
                {
                    using (var brush = new SolidBrush(hoverColor))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                }
                else if (isSelected)
                {
                    using (var brush = new SolidBrush(selectedColor))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                }

                var currentX = bounds.X + 8;

                // 复选框
                if (SelectionStyle == ComboBoxSelectionStyle.Multiple)
                {
                    var checkBoxRect = new Rectangle(currentX, bounds.Y + (bounds.Height - 16) / 2, 16, 16);
                    DrawCheckBox(g, checkBoxRect, item.IsSelected);
                    currentX = checkBoxRect.Right + 6;
                }

                // 图标
                if (item.Icon != null)
                {
                    var iconSize = 16;
                    var iconRect = new Rectangle(currentX, bounds.Y + (bounds.Height - iconSize) / 2, iconSize, iconSize);

                    try
                    {
                        g.DrawImage(item.Icon, iconRect);
                    }
                    catch
                    {
                        // 图标绘制失败时使用边框颜色绘制占位符
                        using (var pen = new Pen(borderColor))
                        {
                            g.DrawRectangle(pen, iconRect);
                        }
                    }

                    currentX = iconRect.Right + 6;
                }

                // 文本
                using (var brush = new SolidBrush(textColor))
                {
                    var textRect = new Rectangle(currentX, bounds.Y, bounds.Right - currentX - 8, bounds.Height);

                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };

                    g.DrawString(item.Text, Font, brush, textRect, sf);
                }
            }

            private void DrawCheckBox(Graphics g, Rectangle rect, bool isChecked)
            {
                Color checkColor, borderColor;
                if (parent.UseTheme && parent.Theme != null)
                {
                    checkColor = parent.Theme.Colors.Primary;
                    borderColor = isChecked ? parent.Theme.Colors.Primary : parent.Theme.Colors.Border;
                }
                else
                {
                    checkColor = SystemColors.Highlight;
                    borderColor = isChecked ? SystemColors.Highlight : SystemColors.ActiveBorder;
                }

                // 背景
                using (var brush = new SolidBrush(isChecked ? checkColor : BackColor))
                {
                    g.FillRectangle(brush, rect);
                }

                // 边框
                using (var pen = new Pen(borderColor))
                {
                    g.DrawRectangle(pen, rect);
                }

                // 选中标记
                if (isChecked)
                {
                    using (var pen = new Pen(Color.White, 2))
                    {
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                        g.DrawLine(pen,
                            rect.X + 4, rect.Y + 8,
                            rect.X + 7, rect.Y + 11);
                        g.DrawLine(pen,
                            rect.X + 7, rect.Y + 11,
                            rect.X + 12, rect.Y + 5);
                    }
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                var index = (e.Y - 1 + scrollOffset) / ItemHeight;
                if (index >= 0 && index < items.Count)
                {
                    if (hoveredIndex != index)
                    {
                        hoveredIndex = index;
                        Invalidate();
                    }
                }
                else
                {
                    if (hoveredIndex != -1)
                    {
                        hoveredIndex = -1;
                        Invalidate();
                    }
                }
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);

                var index = (e.Y - 1 + scrollOffset) / ItemHeight;
                if (index >= 0 && index < items.Count)
                {
                    if (SelectionStyle == ComboBoxSelectionStyle.Single)
                    {
                        selectedIndex = index;
                        ItemClick?.Invoke(this, new DropDownItemEventArgs(items[index]));
                    }
                    else
                    {
                        var checkBoxX = 8;
                        var checkBoxWidth = 16 + 6;

                        if (e.X >= checkBoxX && e.X <= checkBoxX + checkBoxWidth)
                        {
                            items[index].IsSelected = !items[index].IsSelected;
                            MultiSelectChanged?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            items[index].IsSelected = !items[index].IsSelected;
                            MultiSelectChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hoveredIndex = -1;
                Invalidate();
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);

                // 只有在没有锁定时才更新滚动条
                if (!scrollBarLocked)
                {
                    UpdateScrollBar();
                }
            }

            internal void UpdateScrollBar()
            {
                // 如果滚动条被锁定则不更新
                if (scrollBarLocked)
                {
                    return;
                }

                if (items.Count == 0)
                {
                    scrollBar.Visible = false;
                    scrollOffset = 0;
                    return;
                }

                var totalHeight = items.Count * ItemHeight;
                var visibleHeight = Math.Max(0, Height - 2); // 减去上下边框

                // 只有在高度有效时才更新滚动条
                if (visibleHeight <= 0)
                {
                    return;
                }

                if (totalHeight > visibleHeight)
                {
                    scrollBar.Visible = true;
                    scrollBar.Location = new Point(Width - scrollBar.Width - 1, 1);
                    scrollBar.Height = Height - 2;

                    // 设置滚动条参数
                    scrollBar.Maximum = totalHeight - visibleHeight;
                    scrollBar.LargeChange = visibleHeight;
                    scrollBar.SmallChange = ItemHeight;

                    // 确保当前滚动位置有效
                    if (scrollOffset > scrollBar.Maximum)
                    {
                        scrollOffset = scrollBar.Maximum;
                        scrollBar.Value = scrollOffset;
                    }
                }
                else
                {
                    scrollBar.Visible = false;
                    scrollOffset = 0;
                }

                Invalidate();
            }

            /// <summary>
            /// 锁定滚动条状态
            /// </summary>
            public void LockScrollBar()
            {
                scrollBarLocked = true;
                cachedScrollBarVisible = scrollBar.Visible;
                cachedScrollOffset = scrollOffset;

                // 动画期间隐藏滚动条, 避免闪烁
                scrollBar.Visible = false;
            }

            /// <summary>
            /// 解锁滚动条状态
            /// </summary>
            public void UnlockScrollBar()
            {
                scrollBarLocked = false;

                // 恢复滚动条状态
                UpdateScrollBar();

                // 如果之前有滚动位置, 恢复它
                if (cachedScrollBarVisible && scrollBar.Visible)
                {
                    scrollOffset = Math.Min(cachedScrollOffset, scrollBar.Maximum);
                    scrollBar.Value = scrollOffset;
                }
            }
        }

        /// <summary>
        /// 下拉项
        /// </summary>
        private class DropDownItem
        {
            public object Value { get; set; }
            public string Text { get; set; }
            public bool IsSelected { get; set; }
            public Image Icon { get; set; }
        }

        /// <summary>
        /// 下拉项事件参数
        /// </summary>
        private class DropDownItemEventArgs : EventArgs
        {
            public DropDownItem Item { get; }

            public DropDownItemEventArgs(DropDownItem item)
            {
                Item = item;
            }
        }

        #endregion
    }

    public enum ComboBoxSelectionStyle
    {
        Single,
        Multiple
    }
}
