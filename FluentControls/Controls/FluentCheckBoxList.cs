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
    [Designer(typeof(FluentCheckBoxListDesigner))]
    public class FluentCheckBoxList : FluentContainerBase
    {
        private List<FluentCheckBox> checkBoxControls;
        private FluentCheckBoxItemCollection designTimeItems;
        private ListOrientation orientation;
        private bool autoSizeItems;
        private Size fixedItemSize;

        private int itemSpacing;
        private Panel containerPanel;
        private int maxVisibleItems;
        private bool autoScroll;
        private bool showItemAnimation;

        public FluentCheckBoxList()
        {
            checkBoxControls = new List<FluentCheckBox>();
            designTimeItems = new FluentCheckBoxItemCollection(this);
            orientation = ListOrientation.Vertical;
            itemSpacing = 8;
            maxVisibleItems = 0;
            fixedItemSize = new Size(120, 32);
            autoScroll = true;
            showItemAnimation = true;
            autoSizeItems = true;

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

        /// <summary>
        /// 复选框集合
        /// </summary>
        [Category("Data")]
        [Description("复选框项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public FluentCheckBoxItemCollection Items
        {
            get { return designTimeItems; }
        }

        /// <summary>
        /// 运行时复选框控件集合
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<FluentCheckBox> CheckBoxes
        {
            get { return checkBoxControls; }
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
            foreach (FluentCheckBox control in checkBoxControls)
            {
                containerPanel.Controls.Remove(control);
                control.Dispose();
            }
            checkBoxControls.Clear();

            // 根据设计时数据创建控件
            for (int i = 0; i < designTimeItems.Count; i++)
            {
                FluentCheckBoxItem item = designTimeItems[i];
                FluentCheckBox checkBox = CreateCheckBoxFromItem(item);
                checkBoxControls.Add(checkBox);
                containerPanel.Controls.Add(checkBox);
            }

            LayoutControls();
        }

        /// <summary>
        /// 从数据项创建控件
        /// </summary>
        private FluentCheckBox CreateCheckBoxFromItem(FluentCheckBoxItem item)
        {
            FluentCheckBox checkBox = new FluentCheckBox();
            checkBox.Text = item.Text;
            checkBox.Checked = item.Checked;
            checkBox.CheckBoxStyle = item.Style;

            // 如果列表启用了主题, 则应用主题到子控件
            if (UseTheme && Theme != null)
            {
                checkBox.UseTheme = true;
                checkBox.ThemeName = this.ThemeName;
            }
            else
            {
                // 使用项的自定义样式或列表样式
                checkBox.Font = item.Font ?? this.Font;
                checkBox.ForeColor = item.ForeColor != Color.Empty ? item.ForeColor : this.ForeColor;
            }

            checkBox.EnableAnimation = this.EnableAnimation;
            checkBox.BackColor = this.BackColor;

            return checkBox;
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

            // 应用主题到所有复选框控件
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                foreach (var checkBox in checkBoxControls)
                {
                    ApplyThemeToControl(checkBox, false);
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

            // 应用主题到所有复选框控件
            foreach (var checkBox in checkBoxControls)
            {
                ApplyThemeToControl(checkBox, recursive);
            }
        }

        #endregion

        #region 布局

        /// <summary>
        /// 布局控件
        /// </summary>
        private void LayoutControls()
        {
            if (checkBoxControls.Count == 0)
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

            for (int i = 0; i < checkBoxControls.Count; i++)
            {
                FluentCheckBox control = checkBoxControls[i];

                // 计算实际项目宽度(可以根据文本长度动态调整)
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
                if (i == checkBoxControls.Count - 1 ||
                    (i < checkBoxControls.Count - 1 && currentX + CalculateItemWidth(checkBoxControls[i + 1], itemWidth) > availableWidth + Padding.Left))
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

            for (int i = 0; i < checkBoxControls.Count; i++)
            {
                FluentCheckBox control = checkBoxControls[i];

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
                if (i == checkBoxControls.Count - 1 ||
                    (i < checkBoxControls.Count - 1 && currentY + itemHeight > availableHeight + Padding.Top))
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
        private int CalculateItemWidth(FluentCheckBox control, int defaultWidth)
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
                int checkBoxSize = control.CheckBoxStyle == CheckBoxStyle.Switch ? 44 : 20;
                int calculatedWidth = (int)Math.Ceiling(textSize.Width) + checkBoxSize + control.Spacing + 20; // 额外留20像素边距

                // 限制最小和最大宽度
                return Math.Max(80, Math.Min(300, calculatedWidth));
            }
        }

        #endregion

        #region 重写方法

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            LayoutControls();
        }

        protected override void ApplyThemeStyles()
        {
            if (Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;
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
            // 绘制列表背景
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
            // 绘制边框
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
        /// 添加复选框项(运行时)
        /// </summary>
        public FluentCheckBox AddItem(string text, bool isChecked = false)
        {
            FluentCheckBoxItem item = new FluentCheckBoxItem();
            item.Text = text;
            item.Checked = isChecked;

            designTimeItems.Add(item);

            FluentCheckBox checkBox = CreateCheckBoxFromItem(item);
            checkBoxControls.Add(checkBox);
            containerPanel.Controls.Add(checkBox);

            LayoutControls();

            return checkBox;
        }

        /// <summary>
        /// 移除复选框项
        /// </summary>
        public void RemoveItem(FluentCheckBox control)
        {
            int index = checkBoxControls.IndexOf(control);
            if (index >= 0)
            {
                designTimeItems.RemoveAt(index);
                checkBoxControls.RemoveAt(index);
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
            if (index >= 0 && index < checkBoxControls.Count)
            {
                FluentCheckBox control = checkBoxControls[index];
                RemoveItem(control);
            }
        }

        /// <summary>
        /// 清空所有项
        /// </summary>
        public void Clear()
        {
            designTimeItems.Clear();
            foreach (FluentCheckBox control in checkBoxControls)
            {
                containerPanel.Controls.Remove(control);
                control.Dispose();
            }
            checkBoxControls.Clear();
            LayoutControls();
        }

        /// <summary>
        /// 获取所有选中的项
        /// </summary>
        public List<FluentCheckBox> GetCheckedItems()
        {
            List<FluentCheckBox> result = new List<FluentCheckBox>();
            foreach (FluentCheckBox control in checkBoxControls)
            {
                if (control.Checked)
                {
                    result.Add(control);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有选中项的文本
        /// </summary>
        public List<string> GetCheckedTexts()
        {
            List<string> result = new List<string>();
            foreach (FluentCheckBox control in checkBoxControls)
            {
                if (control.Checked)
                {
                    result.Add(control.Text);
                }
            }
            return result;
        }

        /// <summary>
        /// 设置所有项的选中状态
        /// </summary>
        public void SetAllChecked(bool isChecked)
        {
            for (int i = 0; i < designTimeItems.Count; i++)
            {
                designTimeItems[i].Checked = isChecked;
            }

            foreach (FluentCheckBox control in checkBoxControls)
            {
                control.Checked = isChecked;
            }
        }

        #endregion

        #region 动画

        private void AnimateItemEntry(FluentCheckBox control, int index)
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
    /// 复选框项集合
    /// </summary>
    public class FluentCheckBoxItemCollection : CollectionBase
    {
        private FluentCheckBoxList owner;

        public FluentCheckBoxItemCollection(FluentCheckBoxList owner)
        {
            this.owner = owner;
        }

        [Browsable(false)]
        public FluentCheckBoxItem this[int index]
        {
            get { return (FluentCheckBoxItem)List[index]; }
            set { List[index] = value; }
        }

        public int Add(FluentCheckBoxItem item)
        {
            return List.Add(item);
        }

        public void AddRange(FluentCheckBoxItem[] items)
        {
            foreach (FluentCheckBoxItem item in items)
            {
                List.Add(item);
            }
        }

        public void Remove(FluentCheckBoxItem item)
        {
            List.Remove(item);
        }

        public bool Contains(FluentCheckBoxItem item)
        {
            return List.Contains(item);
        }

        public int IndexOf(FluentCheckBoxItem item)
        {
            return List.IndexOf(item);
        }

        public void Insert(int index, FluentCheckBoxItem item)
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

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FluentCheckBoxItem
    {
        private string text;
        private bool isChecked;
        private CheckBoxStyle style;
        private Font font;
        private Color foreColor;

        public FluentCheckBoxItem()
        {
            text = "CheckBox Item";
            isChecked = false;
            style = CheckBoxStyle.Standard;
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
        [Description("复选框样式")]
        [DefaultValue(CheckBoxStyle.Standard)]
        public CheckBoxStyle Style
        {
            get { return style; }
            set { style = value; }
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

    public class FluentCheckBoxListDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentCheckBoxListActionList(this.Component));
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
                        typeof(FluentCheckBoxList),
                        prop,
                        new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }
                    );
                }
            }
        }
    }

    public class FluentCheckBoxListActionList : DesignerActionList
    {
        private FluentCheckBoxList control;
        private IDesignerHost designerHost;
        private IComponentChangeService changeService;

        public FluentCheckBoxListActionList(IComponent component) : base(component)
        {
            control = component as FluentCheckBoxList;
            designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();

            // 项目管理
            items.Add(new DesignerActionHeaderItem("项目管理"));
            //items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑项目集合...", "项目管理", true));
            items.Add(new DesignerActionMethodItem(this, "AddNewItem", "添加新项目", "项目管理", true));
            items.Add(new DesignerActionMethodItem(this, "ClearItems", "清空所有项目", "项目管理", true));

            // 布局
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Orientation", "排列方向", "布局", "控件的排列方向"));
            items.Add(new DesignerActionPropertyItem("ItemSpacing", "项目间距", "布局", "各项目之间的间距"));
            items.Add(new DesignerActionPropertyItem("AutoSizeItems", "自动调整尺寸", "布局", "是否自动调整项目宽度"));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowItemAnimation", "显示动画", "外观", "切换时是否显示动画效果"));

            return items;
        }

        #region 属性

        public ListOrientation Orientation
        {
            get { return control.Orientation; }
            set
            {
                SetProperty("Orientation", value);
            }
        }

        public int ItemSpacing
        {
            get { return control.ItemSpacing; }
            set
            {
                SetProperty("ItemSpacing", value);
            }
        }

        public bool AutoSizeItems
        {
            get { return control.AutoSizeItems; }
            set
            {
                SetProperty("AutoSizeItems", value);
            }
        }

        public bool ShowItemAnimation
        {
            get { return control.ShowItemAnimation; }
            set
            {
                SetProperty("ShowItemAnimation", value);
            }
        }

        #endregion

        #region 方法

        public void AddNewItem()
        {
            if (designerHost != null)
            {
                DesignerTransaction transaction = designerHost.CreateTransaction("添加新项目");
                try
                {
                    if (changeService != null)
                    {
                        changeService.OnComponentChanging(control, TypeDescriptor.GetProperties(control)["Items"]);
                    }

                    FluentCheckBoxItem newItem = new FluentCheckBoxItem();
                    newItem.Text = "Item " + (control.Items.Count + 1);
                    control.Items.Add(newItem);

                    if (changeService != null)
                    {
                        changeService.OnComponentChanged(control, TypeDescriptor.GetProperties(control)["Items"], null, null);
                    }

                    transaction.Commit();
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
            if (MessageBox.Show("确定要清空所有项目吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (designerHost != null)
                {
                    DesignerTransaction transaction = designerHost.CreateTransaction("清空项目");
                    try
                    {
                        if (changeService != null)
                        {
                            changeService.OnComponentChanging(control, TypeDescriptor.GetProperties(control)["Items"]);
                        }

                        control.Clear();

                        if (changeService != null)
                        {
                            changeService.OnComponentChanged(control, TypeDescriptor.GetProperties(control)["Items"], null, null);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Cancel();
                        throw;
                    }
                }
            }
        }

        #endregion


        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(control)[propertyName];
            if (property != null && designerHost != null)
            {
                DesignerTransaction transaction = designerHost.CreateTransaction("设置 " + propertyName);
                try
                {
                    if (changeService != null)
                    {
                        changeService.OnComponentChanging(control, property);
                    }

                    property.SetValue(control, value);

                    if (changeService != null)
                    {
                        changeService.OnComponentChanged(control, property, null, null);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Cancel();
                    throw;
                }
            }
        }

    }

    #endregion

}
