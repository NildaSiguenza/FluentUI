using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{

    [ToolboxItem(true)]
    [Designer(typeof(FluentStatusStripDesigner))]
    [DefaultProperty("Items")]
    public class FluentStatusStrip : FluentContainerBase, IFluentItemContainer
    {
        private readonly FluentStatusStripItemCollection items;
        private StatusStripGripStyle gripStyle = StatusStripGripStyle.Visible;
        private int gripMargin = 4;
        private Rectangle gripBounds;
        private FluentToolStripItem hoveredItem;
        private FluentToolStripItem pressedItem;
        private readonly ToolTip toolTip;
        private bool showItemToolTips = true;

        public FluentStatusStrip()
        {
            items = new FluentStatusStripItemCollection(this);
            toolTip = new ToolTip();

            Dock = DockStyle.Bottom;
            Height = 30;
            Padding = new Padding(1, 2, 1, 2);

            SetStyle(ControlStyles.Selectable, false);
        }

        #region 属性

        /// <summary>
        /// 状态栏项集合
        /// </summary>
        [Category("内容")]
        [Description("状态栏项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentStatusStripItemCollectionEditor), typeof(UITypeEditor))]
        public FluentStatusStripItemCollection Items => items;

        /// <summary>
        /// 握柄样式
        /// </summary>
        [Category("外观")]
        [Description("握柄样式")]
        [DefaultValue(StatusStripGripStyle.Visible)]
        public StatusStripGripStyle GripStyle
        {
            get => gripStyle;
            set
            {
                if (gripStyle != value)
                {
                    gripStyle = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示项的工具提示
        /// </summary>
        [Category("行为")]
        [Description("是否显示项的工具提示")]
        [DefaultValue(true)]
        public bool ShowItemToolTips
        {
            get => showItemToolTips;
            set => showItemToolTips = value;
        }

        #endregion

        #region IFluentItemContainer 实现

        bool IFluentItemContainer.UseTheme => UseTheme;
        IFluentTheme IFluentItemContainer.Theme => Theme;
        Font IFluentItemContainer.Font => Font;
        Control.ControlCollection IFluentItemContainer.Controls => Controls;
        ISite IFluentItemContainer.Site => Site;
        bool IFluentItemContainer.IsHandleCreated => IsHandleCreated;

        event EventHandler IFluentItemContainer.HandleCreated
        {
            add => HandleCreated += value;
            remove => HandleCreated -= value;
        }

        void IFluentItemContainer.PerformLayout()
        {
            PerformLayout();
        }

        void IFluentItemContainer.Invalidate()
        {
            Invalidate();
        }

        void IFluentItemContainer.ItemStateChanged(FluentToolStripItem item)
        {
            Invalidate();
        }

        Point IFluentItemContainer.PointToScreen(Point point)
        {
            return PointToScreen(point);
        }

        #endregion

        #region 布局

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            LayoutItems();
        }

        private void LayoutItems()
        {
            if (items.Count == 0)
            {
                return;
            }

            int gripWidth = gripStyle == StatusStripGripStyle.Visible ? 10 : 0;

            var clientArea = new Rectangle(
            Padding.Left + gripWidth,
            Padding.Top,
            Width - Padding.Horizontal - gripWidth,
            Height - Padding.Vertical);

            // 计算弹簧项 - StatusStrip中Spring表现为填充剩余空间
            var springItems = items.Where(i => i.Visible && i.Alignment == FluentToolStripItemAlignment.Left).ToList();
            var normalItems = items.Where(i => i.Visible && i.Alignment != FluentToolStripItemAlignment.Left).ToList();

            // 左对齐项
            int leftX = clientArea.X;
            foreach (var item in springItems)
            {
                var size = item.Size;
                if (size.IsEmpty)
                    size = item.GetPreferredSize();

                int itemWidth = size.Width + item.Margin.Horizontal;
                item.Bounds = new Rectangle(
                    leftX + item.Margin.Left,
                    clientArea.Y + item.Margin.Top,
                    size.Width,
                    clientArea.Height - item.Margin.Vertical);
                leftX += itemWidth;
            }

            // 右对齐项 - 确保不占用握柄位置
            var rightItems = normalItems.Where(i => i.Alignment == FluentToolStripItemAlignment.Right).ToList();
            int rightX = clientArea.Right; // clientArea已经排除了握柄空间
            for (int i = rightItems.Count - 1; i >= 0; i--)
            {
                var item = rightItems[i];
                var size = item.Size;
                if (size.IsEmpty)
                    size = item.GetPreferredSize();

                int itemWidth = size.Width + item.Margin.Horizontal;
                rightX -= itemWidth;
                item.Bounds = new Rectangle(
                    rightX + item.Margin.Left,
                    clientArea.Y + item.Margin.Top,
                    size.Width,
                    clientArea.Height - item.Margin.Vertical);
            }

        }

        #endregion

        #region 鼠标处理

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            FluentToolStripItem newHoveredItem = null;
            foreach (var item in items)
            {
                if (item.Visible && item.Bounds.Contains(e.Location))
                {
                    newHoveredItem = item;
                    break;
                }
            }

            if (newHoveredItem != hoveredItem)
            {
                if (hoveredItem != null)
                {
                    hoveredItem.OnMouseLeave(EventArgs.Empty);
                    toolTip.Hide(this);
                }

                hoveredItem = newHoveredItem;

                if (hoveredItem != null)
                {
                    hoveredItem.OnMouseEnter(EventArgs.Empty);

                    if (showItemToolTips && !string.IsNullOrEmpty(hoveredItem.ToolTipText))
                    {
                        toolTip.Show(hoveredItem.ToolTipText, this, e.Location.X, e.Location.Y - 20);
                    }
                }

                Invalidate();
            }

            if (gripBounds.Contains(e.Location))
            {
                Cursor = Cursors.SizeNWSE;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredItem != null)
            {
                hoveredItem.OnMouseLeave(EventArgs.Empty);
                hoveredItem = null;
                toolTip.Hide(this);
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (hoveredItem != null && hoveredItem.Enabled)
            {
                pressedItem = hoveredItem;
                hoveredItem.OnMouseDown(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (pressedItem != null && pressedItem.Enabled)
            {
                pressedItem.OnMouseUp(e);
                pressedItem = null;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (hoveredItem != null && hoveredItem.Enabled && hoveredItem.Bounds.Contains(e.Location))
            {
                hoveredItem.OnClick(EventArgs.Empty);
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var bgColor = GetThemeColor(c => c.Surface, BackColor);
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            using (var pen = new Pen(borderColor))
            {
                g.DrawLine(pen, 0, 0, Width, 0);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (gripStyle == StatusStripGripStyle.Visible)
            {
                DrawGrip(g);
            }

            // 绘制所有项
            foreach (var item in items)
            {
                if (item.Visible && item.Available)
                {
                    var e = new PaintEventArgs(g, item.Bounds);
                    item.OnPaint(e);
                }
            }

        }

        protected override void DrawBorder(Graphics g)
        {
            // 状态栏通常不需要额外的边框
        }

        private void DrawGrip(Graphics g)
        {
            int x = Padding.Left + gripMargin;
            int y = Height / 2 - 3;

            var gripColor = GetThemeColor(c => c.TextSecondary, SystemColors.ControlDark);
            using (var brush = new SolidBrush(gripColor))
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        g.FillRectangle(brush, x + i * 3, y + j * 3, 2, 2);
                    }
                }
            }
        }

        #endregion

        #region 主题

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                BackColor = GetThemeColor(c => c.Surface, SystemColors.Control);
                ForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

                // 将主题应用到项
                foreach (var item in items)
                {
                    item.ApplyTheme(Theme);

                    if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
                    {
                        if (controlHost.Control is FluentControlBase fluentControl)
                        {
                            fluentControl.InheritThemeFrom(this);
                        }
                    }
                }

                Invalidate();
            }
        }

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();

            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                foreach (var item in items)
                {
                    item.ApplyTheme(Theme);

                    if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
                    {
                        if (controlHost.Control is FluentControlBase fluentControl)
                        {
                            fluentControl.InheritThemeFrom(this);
                        }
                    }
                }
            }
        }

        public new Color GetThemeColor(Func<IColorPalette, Color> colorSelector, Color defaultColor)
        {
            return base.GetThemeColor(colorSelector, defaultColor);
        }

        public new Font GetThemeFont(Func<ITypography, Font> fontSelector, Font defaultFont)
        {
            return base.GetThemeFont(fontSelector, defaultFont);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (items != null)
                {
                    var itemsCopy = items.ToArray();
                    items.Clear();

                    // 移除状态栏时同时移除所有状态栏项
                    foreach (var item in itemsCopy)
                    {
                        // 如果是控件宿主, 移除控件
                        if (item is FluentToolStripControlHost controlHost)
                        {
                            if (Controls.Contains(controlHost.Control))
                            {
                                Controls.Remove(controlHost.Control);
                            }
                        }
                        item.Dispose();
                    }
                }

                toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 状态栏项

    /// <summary>
    /// 状态栏分隔符
    /// </summary>
    public class FluentStatusStripSeparator : FluentToolStripItem
    {
        private FluentToolStripSeparatorStyle style = FluentToolStripSeparatorStyle.Vertical;
        private int thickness = 1;
        private Color lineColor = Color.LightGray;

        public FluentStatusStripSeparator()
        {
            Padding = new Padding(2);
        }

        [Category("Appearance")]
        [DefaultValue(FluentToolStripSeparatorStyle.Vertical)]
        [Description("分隔符样式")]
        public FluentToolStripSeparatorStyle Style
        {
            get => style;
            set
            {
                if (style != value)
                {
                    style = value;
                    Owner?.PerformLayout();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(1)]
        [Description("线条粗细")]
        public int Thickness
        {
            get => thickness;
            set
            {
                if (thickness != value)
                {
                    thickness = Math.Max(1, value);
                    InvalidateItem();
                }
            }
        }

        [Category("Appearance")]
        [Description("线条颜色")]
        public Color LineColor
        {
            get => lineColor;
            set
            {
                if (lineColor != value)
                {
                    lineColor = value;
                    InvalidateItem();
                }
            }
        }

        protected override void DrawItem(Graphics g)
        {
            var bounds = Bounds;
            if (bounds.IsEmpty)
            {
                return;
            }

            Color color = Owner?.UseTheme == true && Owner.Theme != null
                ? (Owner as FluentStatusStrip).GetThemeColor(c => c.Border, lineColor)
                : lineColor;

            using (var pen = new Pen(color, thickness))
            {
                // 垂直线
                int x = bounds.X + bounds.Width / 2;
                g.DrawLine(pen, x, bounds.Y + 4, x, bounds.Bottom - 4);
            }
        }

        internal override Size GetPreferredSize()
        {
            return new Size(thickness + Padding.Horizontal + 4, 28);
        }

        internal override void OnClick(EventArgs e)
        {
            // 分隔符不响应点击
        }
    }

    /// <summary>
    /// 状态栏进度条项
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripProgressBar : FluentToolStripControlHost
    {
        public FluentToolStripProgressBar() : base(new FluentProgress())
        {
            ProgressBar.Size = new Size(150, 20);
            ProgressBar.ShowProgressText = true;
        }

        /// <summary>
        /// 进度条控件
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        public FluentProgress ProgressBar => (FluentProgress)Control;

        /// <summary>
        /// 进度值
        /// </summary>
        [Category("进度")]
        [Description("进度值(0-100)")]
        [DefaultValue(0.0)]
        public double Value
        {
            get => ProgressBar.Progress;
            set => ProgressBar.Progress = value;
        }

        /// <summary>
        /// 最小值
        /// </summary>
        [Category("进度")]
        [Description("最小值")]
        [DefaultValue(0.0)]
        public double Minimum
        {
            get => ProgressBar.Minimum;
            set => ProgressBar.Minimum = value;
        }

        /// <summary>
        /// 最大值
        /// </summary>
        [Category("进度")]
        [Description("最大值")]
        [DefaultValue(100.0)]
        public double Maximum
        {
            get => ProgressBar.Maximum;
            set => ProgressBar.Maximum = value;
        }

        /// <summary>
        /// 进度条模式
        /// </summary>
        [Category("进度")]
        [Description("进度条模式")]
        [DefaultValue(ProgressMode.Determinate)]
        public ProgressMode Mode
        {
            get => ProgressBar.Mode;
            set => ProgressBar.Mode = value;
        }

        /// <summary>
        /// 进度条样式
        /// </summary>
        [Category("进度")]
        [Description("进度条样式")]
        [DefaultValue(ProgressStyle.Linear)]
        public ProgressStyle Style
        {
            get => ProgressBar.Style;
            set => ProgressBar.Style = value;
        }

        /// <summary>
        /// 是否显示进度文本
        /// </summary>
        [Category("进度")]
        [Description("是否显示进度文本")]
        [DefaultValue(true)]
        public bool ShowProgressText
        {
            get => ProgressBar.ShowProgressText;
            set => ProgressBar.ShowProgressText = value;
        }

        /// <summary>
        /// 自定义文本
        /// </summary>
        [Category("进度")]
        [Description("自定义显示文本")]
        [DefaultValue("")]
        public string CustomText
        {
            get => ProgressBar.CustomText;
            set => ProgressBar.CustomText = value;
        }

        /// <summary>
        /// 是否使用渐变
        /// </summary>
        [Category("外观")]
        [Description("是否使用渐变效果")]
        [DefaultValue(true)]
        public bool UseGradient
        {
            get => ProgressBar.UseGradient;
            set => ProgressBar.UseGradient = value;
        }
    }

    /// <summary>
    /// 状态栏颜色选择器项
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripColorPicker : FluentToolStripControlHost
    {
        public FluentToolStripColorPicker() : base(new FluentColorComboBox())
        {
            ColorPicker.Size = new Size(100, 24);
            ColorPicker.ShowColorText = false;
        }

        /// <summary>
        /// 颜色选择器控件
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        public FluentColorComboBox ColorPicker => (FluentColorComboBox)Control;

        /// <summary>
        /// 选中的颜色
        /// </summary>
        [Category("颜色")]
        [Description("当前选中的颜色")]
        public Color SelectedColor
        {
            get => ColorPicker.SelectedColor;
            set => ColorPicker.SelectedColor = value;
        }

        /// <summary>
        /// 颜色改变事件
        /// </summary>
        [Category("颜色")]
        [Description("颜色改变时触发")]
        public event EventHandler ColorChanged
        {
            add
            {
                ColorPicker.ColorChanged += value;
            }
            remove
            {
                ColorPicker.ColorChanged -= value;
            }
        }
    }

    #endregion

    #region 状态栏项集合

    /// <summary>
    /// 状态栏项集合
    /// </summary>
    public class FluentStatusStripItemCollection : List<FluentToolStripItem>
    {
        private readonly IFluentItemContainer owner;

        internal FluentStatusStripItemCollection(IFluentItemContainer owner)
        {
            this.owner = owner;
        }

        public new void Add(FluentToolStripItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.Owner = owner;
            base.Add(item);

            // 如果是控件宿主，添加控件到容器
            if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
            {
                if (!owner.Controls.Contains(controlHost.Control))
                {
                    owner.Controls.Add(controlHost.Control);

                    // 应用主题
                    if (controlHost.Control is FluentControlBase fluentControl &&
                        owner is FluentControlBase ownerControl)
                    {
                        if (ownerControl.UseTheme && !string.IsNullOrEmpty(ownerControl.ThemeName))
                        {
                            fluentControl.InheritThemeFrom(ownerControl);
                        }
                    }
                }
            }

            OnItemsChanged();
        }

        public void AddRange(FluentToolStripItem[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var item in items)
            {
                if (item != null)
                {
                    item.Owner = owner;
                    base.Add(item);

                    if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
                    {
                        if (!owner.Controls.Contains(controlHost.Control))
                        {
                            owner.Controls.Add(controlHost.Control);

                            if (controlHost.Control is FluentControlBase fluentControl &&
                                owner is FluentControlBase ownerControl)
                            {
                                if (ownerControl.UseTheme && !string.IsNullOrEmpty(ownerControl.ThemeName))
                                {
                                    fluentControl.InheritThemeFrom(ownerControl);
                                }
                            }
                        }
                    }
                }
            }
            OnItemsChanged();
        }

        public new void Insert(int index, FluentToolStripItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.Owner = owner;
            base.Insert(index, item);

            if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
            {
                if (!owner.Controls.Contains(controlHost.Control))
                {
                    owner.Controls.Add(controlHost.Control);

                    if (controlHost.Control is FluentControlBase fluentControl &&
                        owner is FluentControlBase ownerControl)
                    {
                        if (ownerControl.UseTheme && !string.IsNullOrEmpty(ownerControl.ThemeName))
                        {
                            fluentControl.InheritThemeFrom(ownerControl);
                        }
                    }
                }
            }

            OnItemsChanged();
        }

        public new bool Remove(FluentToolStripItem item)
        {
            var result = base.Remove(item);
            if (result)
            {
                item.Owner = null;

                if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
                {
                    owner.Controls.Remove(controlHost.Control);
                }

                OnItemsChanged();
            }
            return result;
        }

        public new void RemoveAt(int index)
        {
            var item = this[index];
            base.RemoveAt(index);

            item.Owner = null;

            if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
            {
                owner.Controls.Remove(controlHost.Control);
            }

            OnItemsChanged();
        }

        public new void Clear()
        {
            foreach (var item in this)
            {
                item.Owner = null;

                if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
                {
                    owner.Controls.Remove(controlHost.Control);
                }
            }
            base.Clear();
            OnItemsChanged();
        }

        private void OnItemsChanged()
        {
            owner?.PerformLayout();
            owner?.Invalidate();
        }
    }
    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 状态栏握柄样式
    /// </summary>
    public enum StatusStripGripStyle
    {
        Hidden,
        Visible
    }

    #endregion

    #region 设计器支持

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class FluentStatusStripDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private ISelectionService selectionService;
        private IDesignerHost designerHost;
        private IServiceProvider serviceProvider;
        private DesignerVerbCollection verbs;
        private ContextMenuStrip contextMenu;

        public FluentStatusStrip StatusStrip => (FluentStatusStrip)Control;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (component is FluentStatusStrip statusStrip)
            {
                // 获取设计时服务
                serviceProvider = component.Site;
                selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));

                // 初始化上下文菜单
                InitializeContextMenu();

                // 启用拖放
                statusStrip.AllowDrop = true;
            }
        }

        #region 智能标记

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentStatusStripActionList(this));
                }
                return actionLists;
            }
        }

        #endregion

        #region 上下文菜单

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            // 添加项目菜单
            var addItemMenu = new ToolStripMenuItem("添加项目");
            addItemMenu.DropDownItems.Add("按钮", null, (s, e) => AddItem(typeof(FluentToolStripButton)));
            addItemMenu.DropDownItems.Add("标签", null, (s, e) => AddItem(typeof(FluentToolStripLabel)));
            addItemMenu.DropDownItems.Add("分隔符", null, (s, e) => AddItem(typeof(FluentStatusStripSeparator)));
            addItemMenu.DropDownItems.Add("进度条", null, (s, e) => AddItem(typeof(FluentToolStripProgressBar)));
            addItemMenu.DropDownItems.Add("下拉框", null, (s, e) => AddItem(typeof(FluentToolStripComboBox)));
            addItemMenu.DropDownItems.Add("颜色选择器", null, (s, e) => AddItem(typeof(FluentToolStripColorPicker)));

            contextMenu.Items.Add(addItemMenu);
            contextMenu.Items.Add(new ToolStripSeparator());

            // 编辑项目
            contextMenu.Items.Add("编辑项目...", null, (s, e) => ShowItemsEditor());
            contextMenu.Items.Add(new ToolStripSeparator());

            // 常用属性
            contextMenu.Items.Add("显示握柄", null, (s, e) => ToggleGripStyle());
        }

        protected void OnContextMenuRequested(int x, int y)
        {
            if (contextMenu != null)
            {
                contextMenu.Show(Control.PointToScreen(new Point(x, y)));
            }
        }

        #endregion

        #region 项目管理

        internal void AddItem(Type itemType)
        {
            if (designerHost == null)
            {
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = designerHost.CreateTransaction($"Add {itemType.Name}");

                // 创建新项目
                var item = (FluentToolStripItem)designerHost.CreateComponent(itemType);

                // 设置默认属性
                SetDefaultProperties(item);

                // 添加到集合
                StatusStrip.Items.Add(item);

                // 如果是控件宿主, 确保控件被正确初始化
                if (item is FluentToolStripControlHost controlHost)
                {
                    // 强制更新布局
                    StatusStrip.PerformLayout();

                    // 确保控件可见
                    if (controlHost.Control != null)
                    {
                        controlHost.Control.Visible = true;
                    }
                }

                // 选中新项目
                if (selectionService != null)
                {
                    selectionService.SetSelectedComponents(new object[] { item });
                }

                // 刷新设计器
                StatusStrip.Invalidate();

                transaction.Commit();
            }
            catch
            {
                transaction?.Cancel();
                throw;
            }
        }

        private void SetDefaultProperties(FluentToolStripItem item)
        {
            // 根据类型设置默认属性
            if (item is FluentToolStripButton button)
            {
                button.Text = button.Name;
                button.Size = new Size(80, 20);
            }
            else if (item is FluentToolStripLabel label)
            {
                label.Text = label.Name;
                label.Size = new Size(100, 20);
            }
            else if (item is FluentToolStripProgressBar progressBar)
            {
                progressBar.Size = new Size(150, 18);
                progressBar.Alignment = FluentToolStripItemAlignment.Right;
            }
            else if (item is FluentToolStripComboBox comboBox)
            {
                comboBox.Size = new Size(120, 20);
                comboBox.Alignment = FluentToolStripItemAlignment.Right;
            }
            else if (item is FluentToolStripColorPicker colorPicker)
            {
                colorPicker.Size = new Size(60, 20);
                colorPicker.Alignment = FluentToolStripItemAlignment.Right;
            }
        }

        internal void ShowItemsEditor()
        {
            ShowCustomItemsEditor();
        }

        private void ShowCustomItemsEditor()
        {
            if (Component.Site == null)
            {
                return;
            }

            // 创建事务以跟踪所有更改
            IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
            DesignerTransaction transaction = null;

            try
            {
                transaction = host?.CreateTransaction("Edit StatusStrip Items");

                // 创建项目集合的副本
                var itemsCopy = new List<FluentToolStripItem>();
                foreach (var item in StatusStrip.Items)
                {
                    itemsCopy.Add(item);
                }

                using (var editorForm = new FluentStatusStripItemCollectionEditorForm(StatusStrip, itemsCopy, Component.Site))
                {
                    if (editorForm.ShowDialog() == DialogResult.OK)
                    {
                        // 应用更改
                        ApplyItemChanges(editorForm.GetResultItems());
                        transaction?.Commit();
                    }
                    else
                    {
                        transaction?.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"编辑项目时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyItemChanges(List<FluentToolStripItem> newItems)
        {
            IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (host == null)
            {
                return;
            }

            // 移除不在新列表中的项目
            var itemsToRemove = new List<FluentToolStripItem>();
            foreach (var item in StatusStrip.Items)
            {
                if (!newItems.Contains(item))
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                StatusStrip.Items.Remove(item);
                if (item.Site != null)
                {
                    host.DestroyComponent(item);
                }
            }

            // 重新排序项目
            StatusStrip.Items.Clear();
            foreach (var item in newItems)
            {
                StatusStrip.Items.Add(item);
            }

            // 刷新设计器
            RaiseComponentChanged(TypeDescriptor.GetProperties(StatusStrip)["Items"], null, StatusStrip.Items);
        }

        internal void ShowItemsEditorDialog()
        {
            var itemsCopy = new List<FluentToolStripItem>();
            foreach (var item in StatusStrip.Items)
            {
                itemsCopy.Add(item);
            }

            // 直接创建并显示自定义的编辑器窗体
            using (var editorForm = new FluentStatusStripItemCollectionEditorForm(StatusStrip, itemsCopy, Component.Site))
            {
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    // 刷新设计器
                    if (designerHost != null)
                    {
                        using (var transaction = designerHost.CreateTransaction("Edit Items"))
                        {
                            // 触发变更通知
                            PropertyDescriptor itemsProperty = TypeDescriptor.GetProperties(StatusStrip)["Items"];
                            itemsProperty?.SetValue(StatusStrip, StatusStrip.Items);
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        private void ToggleGripStyle()
        {
            if (designerHost == null)
            {
                return;
            }

            using (var transaction = designerHost.CreateTransaction("Toggle Grip Style"))
            {
                PropertyDescriptor property = TypeDescriptor.GetProperties(StatusStrip)["GripStyle"];
                var newValue = StatusStrip.GripStyle == StatusStripGripStyle.Visible
                    ? StatusStripGripStyle.Hidden
                    : StatusStripGripStyle.Visible;
                property?.SetValue(StatusStrip, newValue);
                transaction.Commit();
            }
        }

        #endregion

        #region 属性过滤

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 确保Items属性在设计时可编辑
            PropertyDescriptor itemsProp = (PropertyDescriptor)properties["Items"];
            if (itemsProp != null)
            {
                properties["Items"] = TypeDescriptor.CreateProperty(
                    typeof(FluentStatusStrip),
                    itemsProp,
                    new EditorAttribute(typeof(FluentStatusStripItemCollectionEditor), typeof(UITypeEditor)));
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            // 设置默认属性
            if (Component is FluentStatusStrip statusStrip)
            {
                statusStrip.Dock = DockStyle.Bottom;
                statusStrip.Height = 30;
                statusStrip.GripStyle = StatusStripGripStyle.Visible;
            }
        }

        #endregion

        #region 选择规则

        public override SelectionRules SelectionRules
        {
            get
            {
                var statusStrip = Control as FluentStatusStrip;
                if (statusStrip != null && statusStrip.Dock == DockStyle.Bottom)
                {
                    return SelectionRules.Visible | SelectionRules.Moveable |
                           SelectionRules.LeftSizeable | SelectionRules.RightSizeable;
                }
                return base.SelectionRules;
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (contextMenu != null)
                {
                    contextMenu.Dispose();
                    contextMenu = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    public class FluentStatusStripActionList : DesignerActionList
    {
        private FluentStatusStripDesigner designer;
        private FluentStatusStrip statusStrip;

        public FluentStatusStripActionList(FluentStatusStripDesigner designer) : base(designer.Component)
        {
            this.designer = designer;
            this.statusStrip = designer.StatusStrip;
        }

        #region 属性

        [Category("外观")]
        [Description("握柄样式")]
        public StatusStripGripStyle GripStyle
        {
            get => statusStrip.GripStyle;
            set => SetProperty("GripStyle", value);
        }

        [Category("行为")]
        [Description("是否显示项的工具提示")]
        public bool ShowItemToolTips
        {
            get => statusStrip.ShowItemToolTips;
            set => SetProperty("ShowItemToolTips", value);
        }

        #endregion

        #region 方法

        public void AddButton()
        {
            designer.AddItem(typeof(FluentToolStripButton));
        }

        public void AddLabel()
        {
            designer.AddItem(typeof(FluentToolStripLabel));
        }

        public void AddSeparator()
        {
            designer.AddItem(typeof(FluentStatusStripSeparator));
        }

        public void AddProgressBar()
        {
            designer.AddItem(typeof(FluentToolStripProgressBar));
        }

        public void AddComboBox()
        {
            designer.AddItem(typeof(FluentToolStripComboBox));
        }

        public void AddColorPicker()
        {
            designer.AddItem(typeof(FluentToolStripColorPicker));
        }

        public void EditItems()
        {
            designer.ShowItemsEditorDialog();
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(statusStrip)[propertyName];
            property?.SetValue(statusStrip, value);
        }

        #endregion

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("GripStyle", "握柄样式", "外观"));

            // 行为
            items.Add(new DesignerActionHeaderItem("行为"));
            items.Add(new DesignerActionPropertyItem("ShowItemToolTips", "显示项工具提示", "行为"));

            // 操作
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "AddButton", "添加按钮", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddLabel", "添加标签", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddSeparator", "添加分隔符", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddProgressBar", "添加进度条", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddComboBox", "添加下拉框", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddColorPicker", "添加颜色选择器", "操作"));
            items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑项目...", "操作"));

            return items;
        }
    }

    public class FluentStatusStripItemCollectionEditorForm : Form
    {
        private FluentStatusStripItemCollection collection;
        private IServiceProvider serviceProvider;
        private ListBox itemsListBox;
        private PropertyGrid propertyGrid;
        private Button addButton;
        private Button removeButton;
        private Button moveUpButton;
        private Button moveDownButton;
        private Button okButton;
        private Button cancelButton;
        private ComboBox itemTypeComboBox;

        private FluentStatusStrip statusStrip;
        private List<FluentToolStripItem> workingItems;
        private List<FluentToolStripItem> itemsToDestroy;

        public FluentStatusStripItemCollectionEditorForm(FluentStatusStrip statusStrip,
            List<FluentToolStripItem> items, IServiceProvider serviceProvider)
        {
            this.statusStrip = statusStrip;
            this.collection = statusStrip.Items;
            this.workingItems = new List<FluentToolStripItem>(items);
            this.itemsToDestroy = new List<FluentToolStripItem>();
            this.serviceProvider = serviceProvider;
            InitializeComponents();
            LoadItems();
        }

        private void InitializeComponents()
        {
            Text = "FluentStatusStrip 项目集合编辑器";
            Size = new Size(720, 480);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 400);

            // 创建主TableLayoutPanel
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(5)
            };

            // 设置列宽度
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // 设置行高度
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));

            // 项列表
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));

            var itemsLabel = new Label
            {
                Text = "成员:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 项目列表框
            itemsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 22,
                IntegralHeight = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            itemsListBox.DrawItem += OnDrawItem;
            itemsListBox.SelectedIndexChanged += OnSelectedItemChanged;

            // 底部按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            var topButtonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            itemTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Margin = new Padding(0, 2, 5, 0)
            };
            itemTypeComboBox.Items.AddRange(new string[]
            {
                "Button",
                "Label",
                "Separator",
                "ProgressBar",
                "ComboBox",
                "ColorPicker"
            });
            itemTypeComboBox.SelectedIndex = 0;

            addButton = new Button
            {
                Text = "添加",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 0, 0)
            };
            addButton.Click += OnAddItem;

            topButtonFlow.Controls.Add(itemTypeComboBox);
            topButtonFlow.Controls.Add(addButton);

            var bottomButtonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            removeButton = new Button
            {
                Text = "移除",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 5, 0)
            };
            removeButton.Click += OnRemoveItem;

            moveUpButton = new Button
            {
                Text = "上移 ↑",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 5, 0)
            };
            moveUpButton.Click += OnMoveUp;

            moveDownButton = new Button
            {
                Text = "下移 ↓",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 0, 0)
            };
            moveDownButton.Click += OnMoveDown;

            bottomButtonFlow.Controls.Add(removeButton);
            bottomButtonFlow.Controls.Add(moveUpButton);
            bottomButtonFlow.Controls.Add(moveDownButton);

            buttonPanel.Controls.Add(bottomButtonFlow);
            buttonPanel.Controls.Add(topButtonFlow);

            leftLayout.Controls.Add(itemsLabel, 0, 0);
            leftLayout.Controls.Add(itemsListBox, 0, 1);
            leftLayout.Controls.Add(buttonPanel, 0, 2);

            leftPanel.Controls.Add(leftLayout);

            // 属性网格
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var propertiesLabel = new Label
            {
                Text = "属性:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.Categorized,
                ToolbarVisible = true
            };

            rightLayout.Controls.Add(propertiesLabel, 0, 0);
            rightLayout.Controls.Add(propertyGrid, 0, 1);

            rightPanel.Controls.Add(rightLayout);

            // 底部按钮区
            var dialogButtonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 5, 5)
            };

            okButton = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Size = new Size(75, 25),
                Anchor = AnchorStyles.Right
            };

            cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Size = new Size(75, 25),
                Anchor = AnchorStyles.Right
            };

            var dialogButtonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            dialogButtonFlow.Controls.Add(cancelButton);
            dialogButtonFlow.Controls.Add(okButton);

            dialogButtonPanel.Controls.Add(dialogButtonFlow);

            // 添加到主TableLayoutPanel
            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);
            mainLayout.SetColumnSpan(dialogButtonPanel, 2);
            mainLayout.Controls.Add(dialogButtonPanel, 0, 1);

            Controls.Add(mainLayout);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            UpdateButtonStates();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                ApplyChanges();
            }

            // 清理资源
            if (propertyGrid != null)
            {
                propertyGrid.SelectedObject = null;
            }

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 确保清理PropertyGrid选择的对象
                if (propertyGrid != null)
                {
                    propertyGrid.SelectedObject = null;
                }

                // 清理列表
                itemsToDestroy?.Clear();
                workingItems?.Clear();
            }
            base.Dispose(disposing);
        }

        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var item = itemsListBox.Items[e.Index] as FluentToolStripItem;
            if (item == null)
            {
                return;
            }

            e.DrawBackground();

            string displayText = "";
            string typeText = "";
            Color typeColor = Color.Gray;

            // 获取类型标识
            if (item is FluentToolStripButton)
            {
                typeText = "[Button]";
                typeColor = Color.Blue;
            }
            else if (item is FluentToolStripLabel)
            {
                typeText = "[Label]";
                typeColor = Color.Green;
            }
            else if (item is FluentStatusStripSeparator)
            {
                typeText = "[---]";
                typeColor = Color.Gray;
            }
            else if (item is FluentToolStripProgressBar)
            {
                typeText = "[Progress]";
                typeColor = Color.Purple;
            }
            else if (item is FluentToolStripComboBox)
            {
                typeText = "[Combo]";
                typeColor = Color.DarkOrange;
            }
            else if (item is FluentToolStripColorPicker)
            {
                typeText = "[Color]";
                typeColor = Color.Brown;
            }

            displayText = item.Name;
            if (!string.IsNullOrEmpty(item.Text) && item.Text != item.Name)
            {
                displayText += $" - \"{item.Text}\"";
            }

            // 绘制类型标识
            var typeRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 3, 70, e.Bounds.Height);
            using (var typeBrush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ?
                SystemColors.HighlightText : typeColor))
            {
                e.Graphics.DrawString(typeText, e.Font, typeBrush, typeRect);
            }

            // 绘制名称和文本
            var textRect = new Rectangle(e.Bounds.X + 75, e.Bounds.Y + 3, e.Bounds.Width - 75, e.Bounds.Height);
            using (var textBrush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ?
                SystemColors.HighlightText : SystemColors.ControlText))
            {
                var format = new StringFormat
                {
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                e.Graphics.DrawString(displayText, e.Font, textBrush, textRect, format);
            }

            e.DrawFocusRectangle();
        }

        private void LoadItems()
        {
            itemsListBox.Items.Clear();
            foreach (var item in collection)
            {
                itemsListBox.Items.Add(item);
            }

            if (itemsListBox.Items.Count > 0)
            {
                itemsListBox.SelectedIndex = 0;
            }

            UpdateButtonStates();
        }

        public List<FluentToolStripItem> GetResultItems()
        {
            return new List<FluentToolStripItem>(workingItems);
        }

        private void OnSelectedItemChanged(object sender, EventArgs e)
        {
            if (itemsListBox.SelectedItem is FluentToolStripItem item)
            {
                propertyGrid.SelectedObject = item;
            }
            else
            {
                propertyGrid.SelectedObject = null;
            }

            UpdateButtonStates();
        }

        private void OnAddItem(object sender, EventArgs e)
        {
            Type itemType = GetSelectedItemType();
            if (itemType == null)
            {
                return;
            }

            IDesignerHost host = serviceProvider?.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                DesignerTransaction transaction = host.CreateTransaction($"Add {itemType.Name}");
                try
                {
                    string uniqueName = GetUniqueNameFromHost(host, itemType);
                    var item = (FluentToolStripItem)host.CreateComponent(itemType, uniqueName);

                    SetDefaultProperties(item);

                    workingItems?.Add(item);
                    collection?.Add(item);
                    itemsListBox.Items.Add(item);
                    itemsListBox.SelectedItem = item;

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction?.Cancel();
                    MessageBox.Show($"添加项目失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    var item = (FluentToolStripItem)Activator.CreateInstance(itemType);
                    item.Name = GetSimpleUniqueName(itemType);
                    SetDefaultProperties(item);

                    collection?.Add(item);
                    itemsListBox.Items.Add(item);
                    itemsListBox.SelectedItem = item;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"添加项目失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetUniqueNameFromHost(IDesignerHost host, Type itemType)
        {
            if (host == null)
            {
                return GetSimpleUniqueName(itemType);
            }

            string baseName = GetBaseName(itemType);
            string name;
            int index = 1;

            do
            {
                name = $"{baseName}{index}";
                index++;

                bool existsInHost = host.Container.Components[name] != null;
                bool existsInCollection = collection.Cast<FluentToolStripItem>()
                    .Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));

                if (!existsInHost && !existsInCollection)
                {
                    break;
                }
            }
            while (index < 1000);

            return name;
        }

        private string GetSimpleUniqueName(Type itemType)
        {
            string baseName = GetBaseName(itemType);
            int index = 1;
            string name;

            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FluentToolStripItem item in collection)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    existingNames.Add(item.Name);
                }
            }

            do
            {
                name = $"{baseName}{index}";
                index++;
            }
            while (existingNames.Contains(name) && index < 1000);

            return name;
        }

        private string GetBaseName(Type itemType)
        {
            if (itemType == typeof(FluentToolStripButton))
            {
                return "statusStripButton";
            }
            else if (itemType == typeof(FluentToolStripLabel))
            {
                return "statusStripLabel";
            }
            else if (itemType == typeof(FluentStatusStripSeparator))
            {
                return "statusStripSeparator";
            }
            else if (itemType == typeof(FluentToolStripProgressBar))
            {
                return "statusStripProgressBar";
            }
            else if (itemType == typeof(FluentToolStripComboBox))
            {
                return "statusStripComboBox";
            }
            else if (itemType == typeof(FluentToolStripColorPicker))
            {
                return "statusStripColorPicker";
            }
            else
            {
                return "statusStripItem";
            }
        }

        private void OnRemoveItem(object sender, EventArgs e)
        {
            if (itemsListBox.SelectedItem is FluentToolStripItem item)
            {
                int index = itemsListBox.SelectedIndex;

                workingItems?.Remove(item);
                itemsListBox.Items.Remove(item);

                if (!itemsToDestroy.Contains(item))
                {
                    itemsToDestroy.Add(item);
                }

                if (itemsListBox.Items.Count > 0)
                {
                    itemsListBox.SelectedIndex = Math.Min(index, itemsListBox.Items.Count - 1);
                }

                UpdateButtonStates();
            }
        }

        private void OnMoveUp(object sender, EventArgs e)
        {
            int index = itemsListBox.SelectedIndex;
            if (index > 0)
            {
                var item = collection[index];
                collection.RemoveAt(index);
                collection.Insert(index - 1, item);
                LoadItems();
                itemsListBox.SelectedIndex = index - 1;
            }
        }

        private void OnMoveDown(object sender, EventArgs e)
        {
            int index = itemsListBox.SelectedIndex;
            if (index >= 0 && index < itemsListBox.Items.Count - 1)
            {
                var item = collection[index];
                collection.RemoveAt(index);
                collection.Insert(index + 1, item);
                LoadItems();
                itemsListBox.SelectedIndex = index + 1;
            }
        }

        private void UpdateButtonStates()
        {
            int index = itemsListBox.SelectedIndex;
            removeButton.Enabled = index >= 0;
            moveUpButton.Enabled = index > 0;
            moveDownButton.Enabled = index >= 0 && index < itemsListBox.Items.Count - 1;
        }

        public void ApplyChanges()
        {
            IDesignerHost host = serviceProvider?.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                return;
            }

            // 销毁标记为删除的组件
            foreach (var item in itemsToDestroy)
            {
                if (item.Site != null)
                {
                    try
                    {
                        host.DestroyComponent(item);
                    }
                    catch { }
                }
            }

            // 清空原始集合
            statusStrip.Items.Clear();

            // 添加工作列表中的项目
            foreach (var item in workingItems)
            {
                statusStrip.Items.Add(item);
            }
        }

        private Type GetSelectedItemType()
        {
            switch (itemTypeComboBox.SelectedItem?.ToString())
            {
                case "Button": return typeof(FluentToolStripButton);
                case "Label": return typeof(FluentToolStripLabel);
                case "Separator": return typeof(FluentStatusStripSeparator);
                case "ProgressBar": return typeof(FluentToolStripProgressBar);
                case "ComboBox": return typeof(FluentToolStripComboBox);
                case "ColorPicker": return typeof(FluentToolStripColorPicker);
                default: return null;
            }
        }

        private void SetDefaultProperties(FluentToolStripItem item)
        {
            if (item is FluentToolStripButton button)
            {
                button.Text = button.Name;
                button.Size = new Size(80, 20);
            }
            else if (item is FluentToolStripLabel label)
            {
                label.Text = label.Name;
                label.Size = new Size(100, 20);
            }
            else if (item is FluentToolStripProgressBar progressBar)
            {
                progressBar.Size = new Size(150, 18);
                progressBar.Alignment = FluentToolStripItemAlignment.Right;
            }
            else if (item is FluentToolStripComboBox comboBox)
            {
                comboBox.Size = new Size(120, 20);
                comboBox.Alignment = FluentToolStripItemAlignment.Right;
            }
            else if (item is FluentToolStripColorPicker colorPicker)
            {
                colorPicker.Size = new Size(60, 20);
                colorPicker.Alignment = FluentToolStripItemAlignment.Right;
            }
        }
    }

    #endregion

    #region FluentStatusStripItemCollectionEditor

    /// <summary>
    /// FluentStatusStrip项目集合编辑器
    /// </summary>
    public class FluentStatusStripItemCollectionEditor : CollectionEditor
    {
        public FluentStatusStripItemCollectionEditor() : base(typeof(FluentStatusStripItemCollection))
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(FluentToolStripButton),
                typeof(FluentToolStripLabel),
                typeof(FluentStatusStripSeparator),
                typeof(FluentToolStripProgressBar),
                typeof(FluentToolStripComboBox),
                typeof(FluentToolStripColorPicker)
            };
        }

        protected override object CreateInstance(Type itemType)
        {
            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                var component = host.CreateComponent(itemType) as FluentToolStripItem;
                if (component != null)
                {
                    SetDefaultItemProperties(component);
                }
                return component;
            }

            var item = base.CreateInstance(itemType) as FluentToolStripItem;
            if (item != null)
            {
                SetDefaultItemProperties(item);
            }
            return item;
        }

        protected override void DestroyInstance(object instance)
        {
            if (instance is FluentToolStripItem item)
            {
                var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null && item.Site != null)
                {
                    host.DestroyComponent(item);
                    return;
                }
            }

            base.DestroyInstance(instance);
        }

        protected override object SetItems(object editValue, object[] value)
        {
            if (editValue == null || value == null)
            {
                return base.SetItems(editValue, value);
            }

            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;

            try
            {
                if (host != null)
                {
                    transaction = host.CreateTransaction("编辑FluentStatusStrip项");
                }

                var result = base.SetItems(editValue, value);
                transaction?.Commit();
                return result;
            }
            catch (Exception)
            {
                transaction?.Cancel();
                throw;
            }
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentToolStripItem item)
            {
                return item.ToString();
            }
            return base.GetDisplayText(value);
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentToolStripItem);
        }

        private void SetDefaultItemProperties(FluentToolStripItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item is FluentToolStripButton button)
            {
                button.Text = button.Text ?? button.Name;
                button.Size = new Size(80, 20);
            }
            else if (item is FluentToolStripLabel label)
            {
                label.Text = label.Text ?? label.Name;
                label.Size = new Size(100, 20);
            }
            else if (item is FluentToolStripProgressBar progressBar)
            {
                progressBar.Size = new Size(150, 18);
                progressBar.Alignment = FluentToolStripItemAlignment.Right;
            }
            else if (item is FluentToolStripComboBox comboBox)
            {
                comboBox.Size = new Size(120, 20);
                comboBox.Alignment = FluentToolStripItemAlignment.Right;
            }
            else if (item is FluentToolStripColorPicker colorPicker)
            {
                colorPicker.Size = new Size(60, 20);
                colorPicker.Alignment = FluentToolStripItemAlignment.Right;
            }
        }
    }

    #endregion
}
