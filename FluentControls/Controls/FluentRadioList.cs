using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using FluentControls.Animation;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentRadioListDesigner))]
    public class FluentRadioList : FluentContainerBase
    {
        private List<FluentRadio> radioControls;
        private FluentRadioItemCollection designTimeItems;
        private ListOrientation orientation;
        private bool autoSizeItems;
        private Size fixedItemSize;

        private int itemSpacing;
        private Panel containerPanel;
        private int maxVisibleItems;
        private bool autoScroll;
        private string groupName;
        private bool showItemAnimation;

        public event EventHandler SelectedIndexChanged;

        public FluentRadioList()
        {
            radioControls = new List<FluentRadio>();
            designTimeItems = new FluentRadioItemCollection(this);
            orientation = ListOrientation.Vertical;
            itemSpacing = 8;
            maxVisibleItems = 0;
            fixedItemSize = new Size(120, 32);

            autoScroll = true;
            showItemAnimation = true;
            autoSizeItems = true;

            groupName = "RadioGroup_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // 创建容器面板
            containerPanel = new Panel();
            containerPanel.AutoScroll = true;
            containerPanel.Dock = DockStyle.Fill;
            containerPanel.BackColor = Color.Transparent;

            containerPanel.AutoScrollMargin = new Size(0, 0);
            containerPanel.SizeChanged += (sender, e) => LayoutControls();

            // 监听容器面板的控件添加事件以支持主题继承
            containerPanel.ControlAdded += OnContainerControlAdded;

            base.Controls.Add(containerPanel);  // 使用base.Controls

            Size = new Size(200, 300);
            ShadowLevel = 0;
            Padding = new Padding(8);
        }

        #region 属性

        /// <summary>
        /// 重写Controls属性, 返回容器面板的Controls
        /// </summary>
        [Browsable(false)]
        public new ControlCollection Controls => containerPanel?.Controls;

        [Category("Data")]
        [Description("单选框项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public FluentRadioItemCollection Items
        {
            get { return designTimeItems; }
        }

        /// <summary>
        /// 运行时单选框控件集合
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<FluentRadio> RadioButtons
        {
            get { return radioControls; }
        }

        /// <summary>
        /// 排列方向
        /// </summary>
        [Category("Fluent")]
        [Description("控件的排列方向")]
        [DefaultValue(ListOrientation.Vertical)]
        public ListOrientation Orientation
        {
            get { return orientation; }
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    LayoutControls();
                }
            }
        }

        /// <summary>
        /// 固定项尺寸(当AutoSizeItems为false时使用)
        /// </summary>
        [Category("Fluent")]
        [Description("固定的项目尺寸(仅当AutoSizeItems为false时生效)")]
        public Size FixedItemSize
        {
            get { return fixedItemSize; }
            set
            {
                if (fixedItemSize != value)
                {
                    fixedItemSize = value;
                    if (!autoSizeItems)
                    {
                        LayoutControls();
                    }
                }
            }
        }

        /// <summary>
        /// 项目间距
        /// </summary>
        [Category("Fluent")]
        [Description("各项目之间的间距")]
        [DefaultValue(8)]
        public int ItemSpacing
        {
            get { return itemSpacing; }
            set
            {
                if (itemSpacing != value && value >= 0)
                {
                    itemSpacing = value;
                    LayoutControls();
                }
            }
        }

        /// <summary>
        /// 最大可见项数
        /// </summary>
        [Category("Fluent")]
        [Description("最大可见项目数量, 0表示不限制")]
        [DefaultValue(0)]
        public int MaxVisibleItems
        {
            get { return maxVisibleItems; }
            set
            {
                if (maxVisibleItems != value && value >= 0)
                {
                    maxVisibleItems = value;
                    LayoutControls();
                }
            }
        }

        /// <summary>
        /// 自动显示滚动条
        /// </summary>
        [Category("Fluent")]
        [Description("是否自动显示滚动条")]
        [DefaultValue(true)]
        public bool AutoScroll
        {
            get { return autoScroll; }
            set
            {
                if (autoScroll != value)
                {
                    autoScroll = value;
                    containerPanel.AutoScroll = value;
                }
            }
        }

        /// <summary>
        /// 是否显示项目动画
        /// </summary>
        [Category("Fluent")]
        [Description("项目进入时是否显示动画效果")]
        [DefaultValue(true)]
        public bool ShowItemAnimation
        {
            get { return showItemAnimation; }
            set { showItemAnimation = value; }
        }

        /// <summary>
        /// 分组名称
        /// </summary>
        [Category("Fluent")]
        [Description("单选框列表的分组名称")]
        [DefaultValue("RadioGroup")]
        public string GroupName
        {
            get { return groupName; }
            set
            {
                if (groupName != value)
                {
                    groupName = value ?? "RadioGroup";
                    UpdateGroupNames();
                }
            }
        }

        /// <summary>
        /// 选中项索引
        /// </summary>
        [Category("Fluent")]
        [Description("当前选中项的索引")]
        [DefaultValue(-1)]
        public int SelectedIndex
        {
            get
            {
                for (int i = 0; i < radioControls.Count; i++)
                {
                    if (radioControls[i].Checked)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set
            {
                if (value >= 0 && value < radioControls.Count)
                {
                    radioControls[value].Checked = true;
                }
            }
        }

        /// <summary>
        /// 选中项
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FluentRadio SelectedItem
        {
            get
            {
                int index = SelectedIndex;
                return index >= 0 ? radioControls[index] : null;
            }
        }

        /// <summary>
        /// 是否自动调整项目宽度
        /// </summary>
        [Category("Fluent")]
        [Description("是否根据文本内容自动调整项目宽度")]
        [DefaultValue(true)]
        public bool AutoSizeItems
        {
            get { return autoSizeItems; }
            set
            {
                if (autoSizeItems != value)
                {
                    autoSizeItems = value;
                    LayoutControls();
                }
            }
        }

        #endregion

        #region 事件

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            if (SelectedIndexChanged != null)
            {
                SelectedIndexChanged.Invoke(this, e);
            }
        }

        #endregion

        #region 设计时支持

        /// <summary>
        /// 项集合改变时调用
        /// </summary>
        internal void OnItemsChanged()
        {
            RecreateControls();
        }

        /// <summary>
        /// 根据设计时数据重新创建控件
        /// </summary>
        private void RecreateControls()
        {
            // 清除现有控件
            foreach (FluentRadio control in radioControls)
            {
                control.CheckedChanged -= Radio_CheckedChanged;
                containerPanel.Controls.Remove(control);
                control.Dispose();
            }
            radioControls.Clear();

            // 根据设计时数据创建控件
            for (int i = 0; i < designTimeItems.Count; i++)
            {
                FluentRadioItem item = designTimeItems[i];
                FluentRadio radio = CreateRadioFromItem(item);
                radioControls.Add(radio);
                containerPanel.Controls.Add(radio);
            }

            LayoutControls();
        }

        /// <summary>
        /// 从数据项创建控件
        /// </summary>
        private FluentRadio CreateRadioFromItem(FluentRadioItem item)
        {
            FluentRadio radio = new FluentRadio();
            radio.Text = item.Text;
            radio.Checked = item.Checked;
            radio.GroupName = this.groupName;

            // 如果列表启用了主题, 则应用主题到子控件
            if (UseTheme && Theme != null)
            {
                radio.UseTheme = true;
                radio.ThemeName = this.ThemeName;
            }
            else
            {
                // 使用项的自定义样式或列表样式
                radio.Font = item.Font ?? this.Font;
                radio.ForeColor = item.ForeColor != Color.Empty ? item.ForeColor : this.ForeColor;
            }

            radio.EnableAnimation = this.EnableAnimation;
            radio.BackColor = Color.Transparent;

            // 订阅选中事件
            radio.CheckedChanged += Radio_CheckedChanged;

            return radio;
        }

        /// <summary>
        /// 更新所有单选框的分组名称
        /// </summary>
        private void UpdateGroupNames()
        {
            foreach (FluentRadio radio in radioControls)
            {
                radio.GroupName = this.groupName;
            }
        }

        #endregion

        #region 主题继承支持

        /// <summary>
        /// 当控件添加到容器面板时
        /// </summary>
        private void OnContainerControlAdded(object sender, ControlEventArgs e)
        {
            // 如果启用了主题继承, 则应用主题到新添加的控件
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName) && e.Control != null)
            {
                ApplyThemeToControl(e.Control, true);
            }
        }

        /// <summary>
        /// 重写主题改变方法
        /// </summary>
        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();

            // 应用主题到所有单选框控件
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                foreach (var radio in radioControls)
                {
                    ApplyThemeToControl(radio, false);
                }
            }

            Invalidate();
        }

        /// <summary>
        /// 重写应用主题到子控件的方法
        /// </summary>
        public override void ApplyThemeToChildren(bool recursive)
        {
            if (!EnableChildThemeInheritance || !UseTheme || string.IsNullOrEmpty(ThemeName))
            {
                return;
            }

            // 应用主题到所有单选框控件
            foreach (var radio in radioControls)
            {
                ApplyThemeToControl(radio, recursive);
            }
        }

        #endregion

        #region 布局

        /// <summary>
        /// 布局控件
        /// </summary>
        private void LayoutControls()
        {
            if (radioControls.Count == 0)
            {
                return;
            }

            containerPanel.SuspendLayout();

            int itemHeight = autoSizeItems ? 32 : fixedItemSize.Height;
            int itemWidth = autoSizeItems ? 120 : fixedItemSize.Width;

            // 计算可用区域
            int availableWidth = containerPanel.ClientSize.Width - Padding.Left - Padding.Right;
            int availableHeight = containerPanel.ClientSize.Height - Padding.Top - Padding.Bottom;

            if (orientation == ListOrientation.Horizontal)
            {
                // 横向排列, 自动换行
                LayoutHorizontalFlow(itemWidth, itemHeight, availableWidth);
            }
            else
            {
                // 纵向排列, 自动换列
                LayoutVerticalFlow(itemWidth, itemHeight, availableHeight);
            }

            containerPanel.ResumeLayout();
        }

        /// <summary>
        /// 横向流式布局(优先横向排列, 自动换行)
        /// </summary>
        private void LayoutHorizontalFlow(int itemWidth, int itemHeight, int availableWidth)
        {
            int currentX = Padding.Left;
            int currentY = Padding.Top;
            int maxRowHeight = itemHeight;
            int totalHeight = 0;

            for (int i = 0; i < radioControls.Count; i++)
            {
                FluentRadio control = radioControls[i];

                // 计算实际项目宽度
                int actualItemWidth = CalculateItemWidth(control, itemWidth);

                // 检查是否需要换行
                if (currentX + actualItemWidth > availableWidth + Padding.Left && currentX > Padding.Left)
                {
                    // 换行
                    currentX = Padding.Left;
                    currentY += maxRowHeight + itemSpacing;
                    maxRowHeight = itemHeight;
                }

                // 设置控件位置和大小
                control.Location = new Point(currentX, currentY);
                control.Size = new Size(actualItemWidth, itemHeight);

                // 更新位置
                currentX += actualItemWidth + itemSpacing;
                maxRowHeight = Math.Max(maxRowHeight, itemHeight);

                // 计算总高度
                if (i == radioControls.Count - 1 ||
                    (i < radioControls.Count - 1 && currentX + CalculateItemWidth(radioControls[i + 1], itemWidth) > availableWidth + Padding.Left))
                {
                    totalHeight = currentY + maxRowHeight;
                }

                // 应用动画
                if (showItemAnimation && EnableAnimation && !DesignMode && control.Visible)
                {
                    AnimateItemEntry(control, i);
                }
            }

            // 设置容器面板的自动滚动最小尺寸
            containerPanel.AutoScrollMinSize = new Size(0, totalHeight + Padding.Bottom);
        }

        /// <summary>
        /// 纵向流式布局(优先纵向排列, 自动换列)
        /// </summary>
        private void LayoutVerticalFlow(int itemWidth, int itemHeight, int availableHeight)
        {
            int currentX = Padding.Left;
            int currentY = Padding.Top;
            int maxColumnWidth = itemWidth;
            int totalWidth = 0;

            for (int i = 0; i < radioControls.Count; i++)
            {
                FluentRadio control = radioControls[i];

                // 计算实际项目宽度
                int actualItemWidth = CalculateItemWidth(control, itemWidth);

                // 检查是否需要换列
                if (currentY + itemHeight > availableHeight + Padding.Top && currentY > Padding.Top)
                {
                    // 换列
                    currentX += maxColumnWidth + itemSpacing;
                    currentY = Padding.Top;
                    maxColumnWidth = actualItemWidth;
                }

                // 设置控件位置和大小
                control.Location = new Point(currentX, currentY);
                control.Size = new Size(actualItemWidth, itemHeight);

                // 更新位置
                currentY += itemHeight + itemSpacing;
                maxColumnWidth = Math.Max(maxColumnWidth, actualItemWidth);

                // 计算总宽度
                if (i == radioControls.Count - 1 ||
                    (i < radioControls.Count - 1 && currentY + itemHeight > availableHeight + Padding.Top))
                {
                    totalWidth = currentX + maxColumnWidth;
                }

                // 应用动画
                if (showItemAnimation && EnableAnimation && !DesignMode && control.Visible)
                {
                    AnimateItemEntry(control, i);
                }
            }

            // 设置容器面板的自动滚动最小尺寸
            containerPanel.AutoScrollMinSize = new Size(totalWidth + Padding.Right, 0);
        }

        /// <summary>
        /// 计算项目实际宽度
        /// </summary>
        private int CalculateItemWidth(FluentRadio control, int defaultWidth)
        {
            if (!autoSizeItems)
            {
                return defaultWidth;
            }

            if (string.IsNullOrEmpty(control.Text))
            {
                return defaultWidth;
            }

            // 根据文本内容和样式计算宽度
            using (Graphics g = control.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(control.Text, control.Font);
                int radioSize = 20;
                int calculatedWidth = (int)Math.Ceiling(textSize.Width) + radioSize + control.Spacing + 20; // 额外留20像素边距

                // 限制最小和最大宽度
                return Math.Max(80, Math.Min(300, calculatedWidth));
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 单选框选中事件处理
        /// </summary>
        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            FluentRadio radio = sender as FluentRadio;
            if (radio != null && radio.Checked)
            {
                // 同步到设计时数据
                int index = radioControls.IndexOf(radio);
                if (index >= 0)
                {
                    for (int i = 0; i < designTimeItems.Count; i++)
                    {
                        designTimeItems[i].Checked = (i == index);
                    }
                }

                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region 重写方法

        protected override void ApplyThemeStyles()
        {
            if (Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            LayoutControls();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (!DesignMode && !UseTheme)
            {
                RecreateControls();
            }
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            if (!DesignMode && !UseTheme)
            {
                RecreateControls();
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 内容由子控件绘制
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = GetThemeColor(c => c.Border, SystemColors.ActiveBorder);
            using (Pen pen = new Pen(borderColor, 1f))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        /// <summary>
        /// 容器尺寸改变时重新布局
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LayoutControls();
        }

        #endregion

        #region 运行时方法

        /// <summary>
        /// 添加单选框项(运行时)
        /// </summary>
        public FluentRadio AddItem(string text, bool isChecked = false)
        {
            FluentRadioItem item = new FluentRadioItem();
            item.Text = text;
            item.Checked = isChecked;

            designTimeItems.Add(item);

            FluentRadio radio = CreateRadioFromItem(item);
            radioControls.Add(radio);
            containerPanel.Controls.Add(radio);

            LayoutControls();

            return radio;
        }

        /// <summary>
        /// 移除单选框项
        /// </summary>
        public void RemoveItem(FluentRadio control)
        {
            int index = radioControls.IndexOf(control);
            if (index >= 0)
            {
                designTimeItems.RemoveAt(index);
                control.CheckedChanged -= Radio_CheckedChanged;
                radioControls.RemoveAt(index);
                containerPanel.Controls.Remove(control);
                control.Dispose();
                LayoutControls();
            }
        }

        /// <summary>
        /// 移除指定索引的项
        /// </summary>
        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < radioControls.Count)
            {
                FluentRadio control = radioControls[index];
                RemoveItem(control);
            }
        }

        /// <summary>
        /// 清空所有项
        /// </summary>
        public void Clear()
        {
            designTimeItems.Clear();
            foreach (FluentRadio control in radioControls)
            {
                control.CheckedChanged -= Radio_CheckedChanged;
                containerPanel.Controls.Remove(control);
                control.Dispose();
            }
            radioControls.Clear();
            LayoutControls();
        }

        /// <summary>
        /// 获取选中项的文本
        /// </summary>
        public string GetSelectedText()
        {
            FluentRadio selected = SelectedItem;
            return selected != null ? selected.Text : null;
        }

        #endregion

        #region 动画

        private void AnimateItemEntry(FluentRadio control, int index)
        {
            if (!EnableAnimation || Theme == null)
            {
                return;
            }

            int delay = Math.Max(1, index * 50);
            Point targetLocation = control.Location;

            if (orientation == ListOrientation.Vertical)
            {
                control.Location = new Point(targetLocation.X - 20, targetLocation.Y);
            }
            else
            {
                control.Location = new Point(targetLocation.X, targetLocation.Y - 20);
            }

            Timer delayTimer = new Timer();
            delayTimer.Interval = delay;
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();
                delayTimer.Dispose();

                AnimationManager.AnimateLocation(control, targetLocation,
                    Theme.Animation?.NormalDuration ?? 300, Easing.CubicOut, null);
            };
            delayTimer.Start();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
                if (containerPanel != null)
                {
                    containerPanel.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 子项

    /// <summary>
    /// 单选框项集合
    /// </summary>
    public class FluentRadioItemCollection : CollectionBase
    {
        private FluentRadioList owner;

        public FluentRadioItemCollection(FluentRadioList owner)
        {
            this.owner = owner;
        }

        [Browsable(false)]
        public FluentRadioItem this[int index]
        {
            get { return (FluentRadioItem)List[index]; }
            set { List[index] = value; }
        }

        public int Add(FluentRadioItem item)
        {
            return List.Add(item);
        }

        public void AddRange(FluentRadioItem[] items)
        {
            foreach (FluentRadioItem item in items)
            {
                List.Add(item);
            }
        }

        public void Remove(FluentRadioItem item)
        {
            List.Remove(item);
        }

        public bool Contains(FluentRadioItem item)
        {
            return List.Contains(item);
        }

        public int IndexOf(FluentRadioItem item)
        {
            return List.IndexOf(item);
        }

        public void Insert(int index, FluentRadioItem item)
        {
            List.Insert(index, item);
        }

        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            if (owner != null)
            {
                owner.OnItemsChanged();
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            if (owner != null)
            {
                owner.OnItemsChanged();
            }
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            base.OnSetComplete(index, oldValue, newValue);
            if (owner != null)
            {
                owner.OnItemsChanged();
            }
        }

        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            if (owner != null)
            {
                owner.OnItemsChanged();
            }
        }
    }

    /// <summary>
    /// 单选框项数据类(用于设计时)
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FluentRadioItem
    {
        private string text;
        private bool isChecked;
        private Font font;
        private Color foreColor;

        public FluentRadioItem()
        {
            text = "Radio Item";
            isChecked = false;
            font = null;
            foreColor = Color.Empty;
        }

        [Category("Appearance")]
        [Description("显示的文本")]
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        [Category("Behavior")]
        [Description("是否选中")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }

        [Category("Appearance")]
        [Description("字体(为空则使用父控件字体)")]
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        [Category("Appearance")]
        [Description("前景色(为空则使用父控件前景色)")]
        public Color ForeColor
        {
            get { return foreColor; }
            set { foreColor = value; }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(text) ? "(Empty)" : text;
        }
    }

    #endregion

    #region 设计器

    public class FluentRadioListDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentRadioListActionList(this.Component));
                }
                return actionLists;
            }
        }

        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 确保 Items 属性在设计时可见
            if (properties.Contains("Items"))
            {
                PropertyDescriptor prop = properties["Items"] as PropertyDescriptor;
                if (prop != null)
                {
                    properties["Items"] = TypeDescriptor.CreateProperty(
                        typeof(FluentRadioList),
                        prop,
                        new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }
                    );
                }
            }
        }
    }

    public class FluentRadioListActionList : DesignerActionList
    {
        private FluentRadioList control;
        private DesignerActionUIService designerService;

        public FluentRadioListActionList(IComponent component) : base(component)
        {
            control = component as FluentRadioList;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();

            // 添加快捷操作 - 注意分类名称要与属性匹配
            items.Add(new DesignerActionHeaderItem("项目管理"));
            items.Add(new DesignerActionMethodItem(this, "AddNewItem", "添加新项目", "项目管理", true));
            items.Add(new DesignerActionMethodItem(this, "ClearItems", "清空所有项目", "项目管理", true));

            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Orientation", "排列方向", "布局"));
            items.Add(new DesignerActionPropertyItem("ItemSpacing", "项目间距", "布局"));
            items.Add(new DesignerActionPropertyItem("AutoSizeItems", "自动调整尺寸", "布局"));

            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowItemAnimation", "显示动画", "外观"));

            return items;
        }

        // 属性访问器
        public ListOrientation Orientation
        {
            get { return control.Orientation; }
            set
            {
                TypeDescriptor.GetProperties(control)["Orientation"].SetValue(control, value);
                if (designerService != null)
                {
                    designerService.Refresh(control);
                }
            }
        }

        public int ItemSpacing
        {
            get { return control.ItemSpacing; }
            set
            {
                TypeDescriptor.GetProperties(control)["ItemSpacing"].SetValue(control, value);
                if (designerService != null)
                {
                    designerService.Refresh(control);
                }
            }
        }

        public bool AutoSizeItems
        {
            get { return control.AutoSizeItems; }
            set
            {
                TypeDescriptor.GetProperties(control)["AutoSizeItems"].SetValue(control, value);
                if (designerService != null)
                {
                    designerService.Refresh(control);
                }
            }
        }

        public bool ShowItemAnimation
        {
            get { return control.ShowItemAnimation; }
            set
            {
                TypeDescriptor.GetProperties(control)["ShowItemAnimation"].SetValue(control, value);
            }
        }

        public void AddNewItem()
        {
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                DesignerTransaction transaction = host.CreateTransaction("添加新项目");
                try
                {
                    FluentRadioItem newItem = new FluentRadioItem();
                    newItem.Text = "New Item " + (control.Items.Count + 1);
                    control.Items.Add(newItem);

                    transaction.Commit();

                    if (designerService != null)
                    {
                        designerService.Refresh(control);
                    }
                }
                catch
                {
                    transaction.Cancel();
                    throw;
                }
            }
        }

        public void ClearItems()
        {
            if (MessageBox.Show("确定要清空所有项目吗?", "确认", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    DesignerTransaction transaction = host.CreateTransaction("清空项目");
                    try
                    {
                        control.Clear();
                        transaction.Commit();

                        if (designerService != null)
                        {
                            designerService.Refresh(control);
                        }
                    }
                    catch
                    {
                        transaction.Cancel();
                        throw;
                    }
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 列表排列方向
    /// </summary>
    public enum ListOrientation
    {
        /// <summary>
        /// 垂直排列
        /// </summary>
        Vertical,

        /// <summary>
        /// 水平排列
        /// </summary>
        Horizontal
    }

}
