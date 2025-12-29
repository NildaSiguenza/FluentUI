using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Security.Permissions;
using System.Security.Policy;
using static FluentControls.Controls.FluentComboBox;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentToolStripDesigner))]
    [DefaultEvent("ItemClick")]
    [DefaultProperty("Items")]
    public class FluentToolStrip : FluentContainerBase, IFluentItemContainer
    {
        private FluentToolStripItemCollection items;
        private FluentToolStripLayoutManager layoutManager;
        private Orientation orientation = Orientation.Horizontal;
        private FluentToolStripRenderMode renderMode = FluentToolStripRenderMode.Professional;
        private bool showGripHandle = true;
        private bool canOverflow = true;
        private FluentToolStripItem hoveredItem;
        private FluentToolStripItem pressedItem;
        private FluentToolStripItem selectedItem;

        // 外观
        private int itemSpacing = 2;
        private Padding itemPadding = new Padding(2, 0, 2, 0);
        private Color separatorColor = Color.LightGray;
        private int separatorWidth = 1;
        internal int gripHandleWidth = 10;

        // 溢出按钮
        private bool isOverflowDropDownOpen = false;
        private ToolStripDropDown overflowDropDown;
        internal Rectangle overflowButtonBounds;
        internal bool showOverflowButton = false;

        // 事件
        public event EventHandler SelectedItemChanged;
        public event EventHandler<FluentToolStripItemEventArgs> ItemClick;
        public event EventHandler<FluentToolStripItemEventArgs> ItemAdded;
        public event EventHandler<FluentToolStripItemEventArgs> ItemRemoved;

        public FluentToolStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.Selectable, true);

            items = new FluentToolStripItemCollection(this);
            layoutManager = new FluentToolStripLayoutManager(this);

            Height = 36;
            Padding = new Padding(2);

            InitializeOverflowDropDown();
        }

        #region 属性

        [Category("Layout")]
        [DefaultValue(Orientation.Horizontal)]
        [Description("工具栏方向")]
        public Orientation Orientation
        {
            get => orientation;
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(FluentToolStripRenderMode.Professional)]
        [Description("渲染模式")]
        public FluentToolStripRenderMode RenderMode
        {
            get => renderMode;
            set
            {
                if (renderMode != value)
                {
                    renderMode = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否显示拖动手柄")]
        public bool ShowGripHandle
        {
            get => showGripHandle;
            set
            {
                if (showGripHandle != value)
                {
                    showGripHandle = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(true)]
        [Description("是否允许溢出")]
        public bool CanOverflow
        {
            get => canOverflow;
            set
            {
                if (canOverflow != value)
                {
                    canOverflow = value;
                    PerformLayout();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(2)]
        [Description("项目间距")]
        public int ItemSpacing
        {
            get => itemSpacing;
            set
            {
                if (itemSpacing != value)
                {
                    itemSpacing = Math.Max(0, value);
                    PerformLayout();
                }
            }
        }

        [Category("Layout")]
        [Description("项目内边距")]
        public Padding ItemPadding
        {
            get => itemPadding;
            set
            {
                if (itemPadding != value)
                {
                    itemPadding = value;
                    PerformLayout();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("Data")]
        [Description("工具栏项目集合")]
        [Editor(typeof(FluentToolStripItemCollectionEditor), typeof(UITypeEditor))]
        [MergableProperty(false)] // 禁用合并
        public FluentToolStripItemCollection Items => items;

        [Browsable(false)]
        public FluentToolStripItem HoveredItem => hoveredItem;

        [Browsable(false)]
        public FluentToolStripItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    var oldItem = selectedItem;
                    selectedItem = value;

                    oldItem?.Invalidate();
                    selectedItem?.Invalidate();

                    OnSelectedItemChanged(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region 事件

        internal virtual void OnSelectedItemChanged(EventArgs e)
        {
            SelectedItemChanged?.Invoke(this, e);
        }

        internal virtual void OnItemClick(FluentToolStripItemEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }

        internal virtual void OnItemAdded(FluentToolStripItemEventArgs e)
        {
            ItemAdded?.Invoke(this, e);
        }

        internal virtual void OnItemRemoved(FluentToolStripItemEventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        #endregion

        #region 项目管理

        internal void AddItem(FluentToolStripItem item)
        {
            if (item == null)
            {
                return;
            }

            PerformLayout();
            Invalidate();
        }

        internal void RemoveItem(FluentToolStripItem item)
        {
            if (item == null)
            {
                return;
            }

            if (hoveredItem == item)
            {
                hoveredItem = null;
            }

            if (pressedItem == item)
            {
                pressedItem = null;
            }

            if (selectedItem == item)
            {
                selectedItem = null;
            }

            PerformLayout();
            Invalidate();
        }

        public void ItemStateChanged(FluentToolStripItem item)
        {
            Invalidate(item.Bounds);
        }

        #endregion

        #region 布局

        public new void PerformLayout()
        {
            if (layoutManager != null)
            {
                layoutManager.Layout();
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PerformLayout();
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var item = GetItemAt(e.Location);
            if (item != hoveredItem)
            {
                hoveredItem?.OnMouseLeave(EventArgs.Empty);
                hoveredItem = item;
                hoveredItem?.OnMouseEnter(EventArgs.Empty);

                Cursor = item?.Enabled == true ? Cursors.Hand : Cursors.Default;
            }

            hoveredItem?.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                pressedItem = GetItemAt(e.Location);
                pressedItem?.OnMouseDown(e);

                // 检查溢出按钮
                if (showOverflowButton && overflowButtonBounds.Contains(e.Location))
                {
                    ShowOverflowDropDown();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && pressedItem != null)
            {
                pressedItem.OnMouseUp(e);

                if (pressedItem.Bounds.Contains(e.Location))
                {
                    pressedItem.OnClick(e);
                    OnItemClick(new FluentToolStripItemEventArgs(pressedItem));
                }

                pressedItem = null;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            hoveredItem?.OnMouseLeave(e);
            hoveredItem = null;
            Cursor = Cursors.Default;
        }

        private FluentToolStripItem GetItemAt(Point point)
        {
            foreach (var item in items)
            {
                if (item.Visible && item.Bounds.Contains(point))
                {
                    return item;
                }
            }
            return null;
        }

        #endregion

        #region 溢出处理

        private void InitializeOverflowDropDown()
        {
            overflowDropDown = new ToolStripDropDown();
            overflowDropDown.AutoSize = true;
        }

        private void ShowOverflowDropDown()
        {
            if (!canOverflow || isOverflowDropDownOpen)
            {
                return;
            }

            overflowDropDown.Items.Clear();

            // 添加溢出项
            foreach (var item in items)
            {
                if (item.Overflow == FluentToolStripItemOverflow.Always ||
                    (item.Overflow == FluentToolStripItemOverflow.AsNeeded && !item.Available))
                {
                    // 创建菜单项表示
                    var menuItem = new ToolStripMenuItem(item.Text)
                    {
                        Tag = item,
                        Enabled = item.Enabled,
                        Checked = item is FluentToolStripCheckBox chb && chb.Checked
                    };

                    menuItem.Click += (s, e) =>
                    {
                        var originalItem = (FluentToolStripItem)((ToolStripMenuItem)s).Tag;
                        originalItem.OnClick(EventArgs.Empty);
                        OnItemClick(new FluentToolStripItemEventArgs(originalItem));
                    };

                    overflowDropDown.Items.Add(menuItem);
                }
            }

            if (overflowDropDown.Items.Count > 0)
            {
                var showPoint = PointToScreen(new Point(overflowButtonBounds.Left, overflowButtonBounds.Bottom));
                overflowDropDown.Show(showPoint);
                isOverflowDropDownOpen = true;

                overflowDropDown.Closed += (s, e) => isOverflowDropDownOpen = false;
            }
        }

        #endregion

        #region 子项相关

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 确保布局被执行
            PerformLayout();

            // 确保所有控件宿主的控件可见
            foreach (var item in items)
            {
                if (item is FluentToolStripControlHost controlHost)
                {
                    if (!Controls.Contains(controlHost.Control))
                    {
                        Controls.Add(controlHost.Control);
                    }
                }
            }
        }

        /// <summary>
        /// 重写CreateHandle以确保正确初始化
        /// </summary>
        protected override void CreateHandle()
        {
            base.CreateHandle();

            if (!DesignMode)
            {
                // 运行时初始化
                InitializeRuntime();
            }
        }

        private void InitializeRuntime()
        {
            // 确保所有项目被正确初始化
            foreach (var item in items)
            {
                item.Owner = this;

                if (item is FluentToolStripControlHost controlHost)
                {
                    // 确保控件被添加并可见
                    if (!Controls.Contains(controlHost.Control))
                    {
                        Controls.Add(controlHost.Control);
                    }
                }
            }

            // 执行布局
            PerformLayout();
            Invalidate();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var rect = ClientRectangle;
            Color bgColor = BackColor;

            if (UseTheme && Theme != null)
            {
                bgColor = GetThemeColor(c => c.Surface, bgColor);
            }

            // 渐变背景
            if (renderMode == FluentToolStripRenderMode.Professional)
            {
                using (var brush = new LinearGradientBrush(rect, bgColor,
                    ControlPaint.Light(bgColor, 0.1f),
                    orientation == Orientation.Horizontal ? 90f : 0f))
                {
                    g.FillRectangle(brush, rect);
                }
            }
            else
            {
                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRectangle(brush, rect);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制拖动手柄
            if (showGripHandle)
            {
                DrawGripHandle(g);
            }

            // 绘制所有项目
            foreach (var item in items)
            {
                if (item.Visible && item.Available && !item.Bounds.IsEmpty)
                {
                    var itemGraphics = g;
                    var oldClip = g.Clip;

                    try
                    {
                        // 限制绘制区域
                        g.SetClip(item.Bounds);

                        // 调用项目的绘制方法
                        item.OnPaint(new PaintEventArgs(g, item.Bounds));
                    }
                    finally
                    {
                        g.Clip = oldClip;
                    }
                }
            }

            // 绘制溢出按钮
            if (showOverflowButton)
            {
                DrawOverflowButton(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            // ToolStrip通常不需要边框
        }

        private void DrawGripHandle(Graphics g)
        {
            var rect = orientation == Orientation.Horizontal
                ? new Rectangle(2, 4, 3, Height - 8)
                : new Rectangle(4, 2, Width - 8, 3);

            Color dotColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.Border, Color.Gray)
                : Color.Gray;

            using (var brush = new SolidBrush(dotColor))
            {
                if (orientation == Orientation.Horizontal)
                {
                    for (int y = rect.Y; y < rect.Bottom; y += 3)
                    {
                        g.FillRectangle(brush, rect.X, y, 2, 2);
                    }
                }
                else
                {
                    for (int x = rect.X; x < rect.Right; x += 3)
                    {
                        g.FillRectangle(brush, x, rect.Y, 2, 2);
                    }
                }
            }
        }

        private void DrawOverflowButton(Graphics g)
        {
            Color arrowColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.TextPrimary, Color.Black)
                : Color.Black;

            if (isOverflowDropDownOpen)
            {
                using (var brush = new SolidBrush(GetThemeColor(c => c.SurfacePressed, Color.LightGray)))
                {
                    g.FillRectangle(brush, overflowButtonBounds);
                }
            }

            // 绘制箭头
            int centerX = overflowButtonBounds.X + overflowButtonBounds.Width / 2;
            int centerY = overflowButtonBounds.Y + overflowButtonBounds.Height / 2;

            Point[] arrow = orientation == Orientation.Horizontal
                ? new Point[]
                {
                    new Point(centerX - 3, centerY - 2),
                    new Point(centerX + 3, centerY - 2),
                    new Point(centerX, centerY + 2)
                }
                : new Point[]
                {
                    new Point(centerX - 2, centerY - 3),
                    new Point(centerX - 2, centerY + 3),
                    new Point(centerX + 2, centerY)
                };

            using (var brush = new SolidBrush(arrowColor))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        #endregion

        #region 主题

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
        }

        protected override void ApplyThemeStyles()
        {
            if (Theme == null)
            {
                return;
            }

            BackColor = GetThemeColor(c => c.Surface, BackColor);
            ForeColor = GetThemeColor(c => c.TextPrimary, ForeColor);
            Font = GetThemeFont(t => t.Body, Font);

            // 更新所有项目的主题
            foreach (var item in items)
            {
                item.ApplyTheme(Theme);
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

        #region 资源释放

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 确保所有项目被正确清理
                if (items != null)
                {
                    var itemsCopy = items.ToArray();
                    items.Clear();

                    foreach (var item in itemsCopy)
                    {
                        // 如果是控件宿主，移除控件
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

                overflowDropDown?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }


    #region FluentToolStripItem

    [TypeConverter(typeof(FluentToolStripItemTypeConverter))]
    [ToolboxItem(false)]
    public abstract class FluentToolStripItem : IComponent
    {
        private IFluentItemContainer owner;
        private string name = "";
        private string text = "";
        private string toolTipText = "";
        private Image image;
        private bool enabled = true;
        private bool visible = true;
        private bool available = true;
        private Rectangle bounds;
        private Size size = Size.Empty;
        private Padding padding = new Padding(2, 0, 2, 0);
        private Padding margin = new Padding(0);
        private FluentToolStripItemAlignment alignment = FluentToolStripItemAlignment.Left;
        private FluentToolStripItemOverflow overflow = FluentToolStripItemOverflow.AsNeeded;
        private object tag;
        private IFluentTheme theme;
        private ISite site;

        // 事件
        public event EventHandler Click;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;
        public event PaintEventHandler Paint;

        #region 属性

        /// <summary>
        /// 项目名称(设计时使用)
        /// </summary>
        [Category("Design")]
        [Description("项目的名称")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Name
        {
            get
            {
                if (Site != null && !string.IsNullOrEmpty(Site.Name))
                {
                    return Site.Name;
                }
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value ?? "";
                    if (Site != null)
                    {
                        // 通过设计器更新站点名称
                        try
                        {
                            var nameProperty = TypeDescriptor.GetProperties(Site)["Name"];
                            nameProperty?.SetValue(Site, value);
                        }
                        catch
                        {
                            // 忽略设计时异常
                        }
                    }
                }
            }
        }

        [Browsable(false)]
        public IFluentItemContainer Owner
        {
            get => owner;
            internal set
            {
                owner = value;
                OnOwnerChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [Description("显示文本")]
        [DefaultValue("")]
        public virtual string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value ?? "";
                    InvalidateItem();
                }
            }
        }

        [Category("Appearance")]
        [Description("工具提示文本")]
        [DefaultValue("")]
        public string ToolTipText
        {
            get => toolTipText;
            set => toolTipText = value ?? "";
        }

        [Category("Appearance")]
        [Description("图像")]
        [DefaultValue(null)]
        public Image Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    InvalidateItem();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否启用")]
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    InvalidateItem();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否可见")]
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    owner?.PerformLayout();
                }
            }
        }

        [Browsable(false)]
        public bool Available
        {
            get => available;
            internal set => available = value;
        }

        [Browsable(false)]
        public Rectangle Bounds
        {
            get => bounds;
            internal set => bounds = value;
        }

        [Category("Layout")]
        [Description("大小")]
        public Size Size
        {
            get => size.IsEmpty ? GetPreferredSize() : size;
            set
            {
                if (size != value)
                {
                    size = value;
                    owner?.PerformLayout();
                }
            }
        }

        [Category("Layout")]
        [Description("内边距")]
        public Padding Padding
        {
            get => padding;
            set
            {
                if (padding != value)
                {
                    padding = value;
                    owner?.PerformLayout();
                }
            }
        }

        [Category("Layout")]
        [Description("外边距")]
        public Padding Margin
        {
            get => margin;
            set
            {
                if (margin != value)
                {
                    margin = value;
                    owner?.PerformLayout();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(FluentToolStripItemAlignment.Left)]
        [Description("对齐方式")]
        public FluentToolStripItemAlignment Alignment
        {
            get => alignment;
            set
            {
                if (alignment != value)
                {
                    alignment = value;
                    owner?.PerformLayout();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(FluentToolStripItemOverflow.AsNeeded)]
        [Description("溢出行为")]
        public FluentToolStripItemOverflow Overflow
        {
            get => overflow;
            set
            {
                if (overflow != value)
                {
                    overflow = value;
                    owner?.PerformLayout();
                }
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public object Tag
        {
            get => tag;
            set => tag = value;
        }

        #endregion

        #region 事件

        internal virtual void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }

        internal virtual void OnMouseDown(MouseEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        internal virtual void OnMouseUp(MouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        internal virtual void OnMouseMove(MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        internal virtual void OnMouseEnter(EventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        internal virtual void OnMouseLeave(EventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        protected virtual void OnOwnerChanged(EventArgs e)
        {
        }

        #endregion

        #region 方法

        internal virtual void OnPaint(PaintEventArgs e)
        {
            Paint?.Invoke(this, e);
            DrawItem(e.Graphics);
        }

        internal virtual void ApplyTheme(IFluentTheme theme)
        {
            this.theme = theme;
            InvalidateItem();
        }

        protected abstract void DrawItem(Graphics g);

        internal virtual Size GetPreferredSize()
        {
            return new Size(100, 28);
        }

        protected void InvalidateItem()
        {
            owner?.ItemStateChanged(this);
        }

        public void Invalidate()
        {
            InvalidateItem();
        }

        /// <summary>
        /// 获取显示名称(用于设计器和调试)
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    return $"{Name} ({Text})";
                }
                return Name;
            }
            else if (!string.IsNullOrEmpty(Text))
            {
                return Text;
            }
            else
            {
                return GetType().Name;
            }
        }

        #endregion

        #region IComponent 实现

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISite Site
        {
            get => site;
            set
            {
                site = value;
                if (site != null && string.IsNullOrEmpty(name))
                {
                    name = site.Name;
                }
            }
        }

        public event EventHandler Disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (site != null && site.Container != null)
                {
                    site.Container.Remove(this);
                }
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }

    public class FluentToolStripItemTypeConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(value, attributes);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is FluentToolStripItem item)
            {
                return item.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion

    #region FluentToolStripItemCollection

    public class FluentToolStripItemCollection : IList<FluentToolStripItem>, IList
    {
        private List<FluentToolStripItem> items = new List<FluentToolStripItem>();
        private FluentToolStrip owner;

        internal FluentToolStripItemCollection(FluentToolStrip owner)
        {
            this.owner = owner;
        }

        public FluentToolStripItem this[int index]
        {
            get => items[index];
            set
            {
                if (items[index] != value)
                {
                    var oldItem = items[index];
                    RemoveInternal(oldItem, index);
                    items[index] = value;
                    AddInternal(value, index);
                }
            }
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;

        public void Add(FluentToolStripItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (items.Contains(item))
            {
                return;
            }

            items.Add(item);
            AddInternal(item, items.Count - 1);
        }

        public void AddRange(params FluentToolStripItem[] items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Insert(int index, FluentToolStripItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            items.Insert(index, item);
            AddInternal(item, index);
        }

        public bool Remove(FluentToolStripItem item)
        {
            int index = items.IndexOf(item);
            if (index >= 0)
            {
                RemoveInternal(item, index);
                items.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var item = items[index];
            RemoveInternal(item, index);
            items.RemoveAt(index);
        }

        public void Clear()
        {
            // 从后往前删除，避免索引问题
            for (int i = items.Count - 1; i >= 0; i--)
            {
                RemoveInternal(items[i], i);
            }
            items.Clear();
        }

        private void AddInternal(FluentToolStripItem item, int index)
        {
            if (item == null || owner == null)
            {
                return;
            }

            // 1. 确保项目有站点
            if (owner.Site != null && item.Site == null)
            {
                owner.Site.Container?.Add(item);
            }

            // 2. 设置Owner
            item.Owner = owner;

            // 3. 如果是控件宿主，添加控件
            if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
            {
                if (!owner.Controls.Contains(controlHost.Control))
                {
                    owner.Controls.Add(controlHost.Control);
                }
            }

            // 4. 触发添加事件
            owner?.OnItemAdded(new FluentToolStripItemEventArgs(item));

            // 5. 通知组件变更服务
            NotifyComponentChange();

            // 6. 重新布局
            owner?.PerformLayout();
        }

        private void RemoveInternal(FluentToolStripItem item, int index)
        {
            if (item == null || owner == null)
            {
                return;
            }

            // 1. 触发移除事件
            owner?.OnItemRemoved(new FluentToolStripItemEventArgs(item));

            // 2. 如果是控件宿主，移除控件
            if (item is FluentToolStripControlHost controlHost && controlHost.Control != null)
            {
                if (owner.Controls.Contains(controlHost.Control))
                {
                    owner.Controls.Remove(controlHost.Control);
                }
            }

            // 3. 清除Owner
            item.Owner = null;

            // 4. 从设计器容器中移除(设计时)
            if (owner.Site != null && item.Site != null)
            {
                item.Site.Container?.Remove(item);
            }

            // 5. 通知组件变更服务
            NotifyComponentChange();

            // 6. 重新布局
            owner?.PerformLayout();
        }

        private void NotifyComponentChange()
        {
            if (owner?.Site == null)
            {
                return;
            }

            var changeService = owner.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
            {
                var memberDescriptor = TypeDescriptor.GetProperties(owner)["Items"];
                if (memberDescriptor != null)
                {
                    try
                    {
                        changeService.OnComponentChanging(owner, memberDescriptor);
                        changeService.OnComponentChanged(owner, memberDescriptor, null, null);
                    }
                    catch
                    {
                        // 忽略设计时异常
                    }
                }
            }
        }

        #region IList实现

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (FluentToolStripItem)value;
        }

        int IList.Add(object value)
        {
            Add((FluentToolStripItem)value);
            return items.Count - 1;
        }

        bool IList.Contains(object value) => Contains((FluentToolStripItem)value);
        int IList.IndexOf(object value) => IndexOf((FluentToolStripItem)value);
        void IList.Insert(int index, object value) => Insert(index, (FluentToolStripItem)value);
        void IList.Remove(object value) => Remove((FluentToolStripItem)value);
        bool IList.IsFixedSize => false;

        void ICollection.CopyTo(Array array, int index) => ((ICollection)items).CopyTo(array, index);
        object ICollection.SyncRoot => ((ICollection)items).SyncRoot;
        bool ICollection.IsSynchronized => false;

        #endregion

        #region IList<T> 实现

        public bool Contains(FluentToolStripItem item) => items.Contains(item);
        public int IndexOf(FluentToolStripItem item) => items.IndexOf(item);
        public void CopyTo(FluentToolStripItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public IEnumerator<FluentToolStripItem> GetEnumerator() => items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
    #endregion

    #region FluentToolStripLayoutManager

    public class FluentToolStripLayoutManager
    {
        private FluentToolStrip owner;

        public FluentToolStripLayoutManager(FluentToolStrip owner)
        {
            this.owner = owner;
        }

        public void Layout()
        {
            if (owner == null || owner.Items.Count == 0)
            {
                return;
            }

            var clientRect = owner.ClientRectangle;
            clientRect.Inflate(-owner.Padding.Left, -owner.Padding.Top);
            clientRect.Width -= owner.Padding.Right;
            clientRect.Height -= owner.Padding.Bottom;

            // 计算起始位置
            int startX = clientRect.X;
            int startY = clientRect.Y;

            // 如果显示拖动手柄，调整起始位置
            if (owner.ShowGripHandle)
            {
                if (owner.Orientation == Orientation.Horizontal)
                {
                    startX += owner.gripHandleWidth;
                }
                else
                {
                    startY += owner.gripHandleWidth;
                }
            }

            // 分离左对齐和右对齐的项目
            var leftItems = new List<FluentToolStripItem>();
            var rightItems = new List<FluentToolStripItem>();

            foreach (var item in owner.Items)
            {
                if (!item.Visible)
                {
                    continue;
                }

                if (item.Alignment == FluentToolStripItemAlignment.Right)
                {
                    rightItems.Add(item);
                }
                else
                {
                    leftItems.Add(item);
                }
            }

            // 布局左对齐项目
            int currentX = startX;
            int currentY = startY;
            int maxHeight = 0;
            int maxWidth = 0;

            foreach (var item in leftItems)
            {
                var preferredSize = item.Size;

                // 添加外边距
                var itemBounds = new Rectangle(
                    currentX + item.Margin.Left,
                    currentY + item.Margin.Top,
                    preferredSize.Width,
                    preferredSize.Height);

                // 检查是否需要溢出
                bool overflow = false;
                if (owner.CanOverflow)
                {
                    if (owner.Orientation == Orientation.Horizontal)
                    {
                        overflow = (itemBounds.Right + item.Margin.Right > clientRect.Right - GetRightItemsWidth(rightItems));
                    }
                    else
                    {
                        overflow = (itemBounds.Bottom + item.Margin.Bottom > clientRect.Bottom - GetRightItemsHeight(rightItems));
                    }
                }

                if (overflow && item.Overflow != FluentToolStripItemOverflow.Never)
                {
                    item.Available = false;
                    item.Bounds = Rectangle.Empty;
                }
                else
                {
                    item.Available = true;
                    item.Bounds = itemBounds;

                    if (owner.Orientation == Orientation.Horizontal)
                    {
                        currentX = itemBounds.Right + item.Margin.Right + owner.ItemSpacing;
                        maxHeight = Math.Max(maxHeight, itemBounds.Height + item.Margin.Vertical);
                    }
                    else
                    {
                        currentY = itemBounds.Bottom + item.Margin.Bottom + owner.ItemSpacing;
                        maxWidth = Math.Max(maxWidth, itemBounds.Width + item.Margin.Horizontal);
                    }
                }
            }

            // 布局右对齐项目
            if (rightItems.Count > 0)
            {
                if (owner.Orientation == Orientation.Horizontal)
                {
                    currentX = clientRect.Right;
                    foreach (var item in rightItems.AsEnumerable().Reverse())
                    {
                        var preferredSize = item.Size;
                        currentX -= (preferredSize.Width + item.Margin.Right);

                        var itemBounds = new Rectangle(
                            currentX - item.Margin.Left,
                            startY + item.Margin.Top,
                            preferredSize.Width,
                            preferredSize.Height);

                        item.Available = true;
                        item.Bounds = itemBounds;

                        currentX -= (item.Margin.Left + owner.ItemSpacing);
                    }
                }
                else
                {
                    currentY = clientRect.Bottom;
                    foreach (var item in rightItems.AsEnumerable().Reverse())
                    {
                        var preferredSize = item.Size;
                        currentY -= (preferredSize.Height + item.Margin.Bottom);

                        var itemBounds = new Rectangle(
                            startX + item.Margin.Left,
                            currentY - item.Margin.Top,
                            preferredSize.Width,
                            preferredSize.Height);

                        item.Available = true;
                        item.Bounds = itemBounds;

                        currentY -= (item.Margin.Top + owner.ItemSpacing);
                    }
                }
            }

            // 检查是否需要显示溢出按钮
            owner.showOverflowButton = false;
            if (owner.CanOverflow)
            {
                foreach (var item in owner.Items)
                {
                    if (!item.Available && item.Overflow != FluentToolStripItemOverflow.Never)
                    {
                        owner.showOverflowButton = true;
                        break;
                    }
                }
            }

            // 设置溢出按钮位置
            if (owner.showOverflowButton)
            {
                if (owner.Orientation == Orientation.Horizontal)
                {
                    owner.overflowButtonBounds = new Rectangle(
                        clientRect.Right - 16,
                        clientRect.Y + (clientRect.Height - 20) / 2,
                        16, 20);
                }
                else
                {
                    owner.overflowButtonBounds = new Rectangle(
                        clientRect.X + (clientRect.Width - 20) / 2,
                        clientRect.Bottom - 16,
                        20, 16);
                }
            }
            else
            {
                owner.overflowButtonBounds = Rectangle.Empty;
            }
        }

        private int GetRightItemsWidth(List<FluentToolStripItem> rightItems)
        {
            int width = 0;
            foreach (var item in rightItems)
            {
                if (item.Available)
                {
                    width += item.Size.Width + item.Margin.Horizontal + owner.ItemSpacing;
                }
            }
            return width;
        }

        private int GetRightItemsHeight(List<FluentToolStripItem> rightItems)
        {
            int height = 0;
            foreach (var item in rightItems)
            {
                if (item.Available)
                {
                    height += item.Size.Height + item.Margin.Vertical + owner.ItemSpacing;
                }
            }
            return height;
        }
    }

    #endregion

    #region FluentToolStripControlHost (ToolStrip宿主基类)

    public class FluentToolStripControlHost : FluentToolStripItem
    {
        private Control control;
        private ContentAlignment controlAlign = ContentAlignment.MiddleCenter;
        private bool controlAdded = false;

        public FluentToolStripControlHost(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            this.control = control;

            // 确保控件初始化正确
            if (!control.IsHandleCreated && !control.IsDisposed)
            {
                control.CreateControl();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control Control => control;

        [Category("Layout")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        [Description("控件对齐方式")]
        public ContentAlignment ControlAlign
        {
            get => controlAlign;
            set
            {
                if (controlAlign != value)
                {
                    controlAlign = value;
                    UpdateControlPosition();
                }
            }
        }

        protected override void OnOwnerChanged(EventArgs e)
        {
            base.OnOwnerChanged(e);

            if (Owner != null)
            {
                // 延迟添加控件，确保Owner已经完全初始化
                if (Owner.IsHandleCreated)
                {
                    AddControlToOwner();
                }
                else
                {
                    Owner.HandleCreated += Owner_HandleCreated;
                }
            }
            else
            {
                RemoveControlFromOwner();
            }
        }

        private void Owner_HandleCreated(object sender, EventArgs e)
        {
            Owner.HandleCreated -= Owner_HandleCreated;
            AddControlToOwner();
        }

        private void AddControlToOwner()
        {
            if (Owner != null && control != null && !Owner.Controls.Contains(control))
            {
                Owner.Controls.Add(control);
                UpdateControlPosition();
            }
        }

        private void RemoveControlFromOwner()
        {
            if (control?.Parent != null)
            {
                control.Parent.Controls.Remove(control);
            }
        }

        internal override void OnPaint(PaintEventArgs e)
        {
            // 更新控件位置
            UpdateControlPosition();

            // 如果控件不可见，绘制占位符
            if (!control.Visible && Owner != null)
            {
                DrawPlaceholder(e.Graphics);
            }
        }

        private void DrawPlaceholder(Graphics g)
        {
            using (var pen = new Pen(Color.Gray, 1))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawRectangle(pen, Bounds);
            }

            using (var brush = new SolidBrush(Color.Gray))
            {
                var text = this.Name ?? this.GetType().Name;
                var font = SystemFonts.DefaultFont;
                var textSize = g.MeasureString(text, font);
                var x = Bounds.X + (Bounds.Width - textSize.Width) / 2;
                var y = Bounds.Y + (Bounds.Height - textSize.Height) / 2;
                g.DrawString(text, font, brush, x, y);
            }
        }

        private void UpdateControlPosition()
        {
            if (control == null || Owner == null || Bounds.IsEmpty)
            {
                if (control != null)
                {
                    control.Visible = false;
                }

                return;
            }

            // 确保控件在正确的父容器中
            if (control.Parent != Owner)
            {
                if (!Owner.Controls.Contains(control))
                {
                    Owner.Controls.Add(control);
                }
            }

            // 计算控件边界
            var controlBounds = new Rectangle(
                Bounds.X + Padding.Left,
                Bounds.Y + Padding.Top,
                Bounds.Width - Padding.Horizontal,
                Bounds.Height - Padding.Vertical);

            // 设置控件位置和大小
            control.SetBounds(
                controlBounds.X,
                controlBounds.Y,
                controlBounds.Width,
                controlBounds.Height);

            // 设置可见性
            control.Visible = Visible && Available;

            // 确保控件在最前面
            control.BringToFront();
        }

        protected override void DrawItem(Graphics g)
        {

        }

        internal override Size GetPreferredSize()
        {
            if (control != null)
            {
                var size = control.PreferredSize;
                return new Size(
                    size.Width + Padding.Horizontal,
                    size.Height + Padding.Vertical);
            }
            return base.GetPreferredSize();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveControlFromOwner();
                control?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    #endregion

    #region FluentToolStripSeparator

    public class FluentToolStripSeparator : FluentToolStripItem
    {
        private FluentToolStripSeparatorStyle style = FluentToolStripSeparatorStyle.Vertical;
        private int thickness = 1;
        private Color lineColor = Color.LightGray;

        public FluentToolStripSeparator()
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
                ? (Owner as FluentToolStrip).GetThemeColor(c => c.Border, lineColor)
                : lineColor;

            using (var pen = new Pen(color, thickness))
            {
                if ((Owner as FluentToolStrip)?.Orientation == Orientation.Horizontal)
                {
                    // 垂直线
                    int x = bounds.X + bounds.Width / 2;
                    g.DrawLine(pen, x, bounds.Y + 4, x, bounds.Bottom - 4);
                }
                else
                {
                    // 水平线
                    int y = bounds.Y + bounds.Height / 2;
                    g.DrawLine(pen, bounds.X + 4, y, bounds.Right - 4, y);
                }
            }
        }

        internal override Size GetPreferredSize()
        {
            if ((Owner as FluentToolStrip)?.Orientation == Orientation.Horizontal)
            {
                return new Size(thickness + Padding.Horizontal + 4, 28);
            }
            else
            {
                return new Size(28, thickness + Padding.Vertical + 4);
            }
        }

        internal override void OnClick(EventArgs e)
        {
            // 分隔符不响应点击
        }
    }

    #endregion

    #region 基于FluentToolStripControlHost的控件实现

    #region FluentToolStripDateTimePicker

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripDateTimePicker : FluentToolStripControlHost
    {
        public FluentToolStripDateTimePicker() : base(new FluentDateTimePicker())
        {
            DateTimePicker.Size = new Size(150, 28);
            DateTimePicker.Font = new Font("Microsoft YaHei", 9.0f, FontStyle.Regular);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentDateTimePicker DateTimePicker => (FluentDateTimePicker)Control;

        public DateTime Value
        {
            get => DateTimePicker.Value;
            set => DateTimePicker.Value = value;
        }

        public DateTimePickerMode Mode
        {
            get => DateTimePicker.Mode;
            set => DateTimePicker.Mode = value;
        }
    }

    #endregion

    #region FluentToolStripButton

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripButton : FluentToolStripControlHost
    {
        public FluentToolStripButton() : base(new FluentButton())
        {
            Button.Size = new Size(120, 28);
            Button.Font = new Font("Microsoft YaHei", 9.0f, FontStyle.Regular);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentButton Button => (FluentButton)Control;

        public new string Text
        {
            get => Button.Text;
            set => Button.Text = value;
        }
    }

    #endregion

    #region FluentToolStripLabel

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripLabel : FluentToolStripControlHost
    {
        public FluentToolStripLabel() : base(new FluentLabel())
        {
            Label.Size = new Size(100, 28);
            Label.Font = new Font("Microsoft YaHei", 9.0f, FontStyle.Regular);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentLabel Label => (FluentLabel)Control;

        public new string Text
        {
            get => Label.Text;
            set => Label.Text = value;
        }
    }

    #endregion

    #region FluentToolStripCheckBox

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripCheckBox : FluentToolStripControlHost
    {
        public FluentToolStripCheckBox() : base(new FluentCheckBox())
        {
            CheckBox.Size = new Size(120, 28);
            CheckBox.Spacing = 4;
            CheckBox.Font = new Font("Microsoft YaHei", 9.0f, FontStyle.Regular);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentCheckBox CheckBox => (FluentCheckBox)Control;

        public new string Text
        {
            get => CheckBox.Text;
            set => CheckBox.Text = value;
        }

        public bool Checked
        {
            get => CheckBox.Checked;
            set => CheckBox.Checked = value;
        }
    }

    #endregion

    #region FluentToolStripComboBox

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripComboBox : FluentToolStripControlHost
    {
        public FluentToolStripComboBox() : base(new FluentComboBox())
        {
            ComboBox.Size = new Size(120, 28);
            ComboBox.Font = new Font("Microsoft YaHei", 9.0f, FontStyle.Regular);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentComboBox ComboBox => (FluentComboBox)Control;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(UITypeEditor))]
        public ItemCollection Items
        {
            get => ComboBox.Items;
        }
    }

    #endregion

    #region FluentToolStripTextBox

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class FluentToolStripTextBox : FluentToolStripControlHost
    {
        public FluentToolStripTextBox() : base(new FluentTextBox())
        {
            TextBox.Size = new Size(120, 28);
            TextBox.Font = new Font("Microsoft YaHei", 9.0f, FontStyle.Regular);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentTextBox TextBox => (FluentTextBox)Control;

        public new string Text
        {
            get => TextBox.Text;
            set => TextBox.Text = value;
        }

        public string Prefix
        {
            get => TextBox.Prefix;
            set => TextBox.Prefix = value;
        }

        public string Suffix
        {
            get => TextBox.Suffix;
            set => TextBox.Suffix = value;
        }

        public InputFormat InputFormat
        {
            get => TextBox.InputFormat;
            set => TextBox.InputFormat = value;
        }
    }

    #endregion

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// ToolStrip项目对齐方式
    /// </summary>
    public enum FluentToolStripItemAlignment
    {
        Left,
        Right
    }

    /// <summary>
    /// ToolStrip项目溢出行为
    /// </summary>
    public enum FluentToolStripItemOverflow
    {
        Never,
        Always,
        AsNeeded
    }

    /// <summary>
    /// ToolStrip项目显示样式
    /// </summary>
    public enum FluentToolStripItemDisplayStyle
    {
        None,
        Text,
        Image,
        ImageAndText
    }

    /// <summary>
    /// 分隔符样式
    /// </summary>
    public enum FluentToolStripSeparatorStyle
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// 渲染模式
    /// </summary>
    public enum FluentToolStripRenderMode
    {
        System,
        Professional,
        Custom
    }

    /// <summary>
    /// ToolStrip项目事件参数
    /// </summary>
    public class FluentToolStripItemEventArgs : EventArgs
    {
        public FluentToolStripItem Item { get; }

        public FluentToolStripItemEventArgs(FluentToolStripItem item)
        {
            Item = item;
        }
    }

    #endregion

    #region 设计器支持

    #region FluentToolStripDesigner

    /// <summary>
    /// FluentToolStrip设计器
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class FluentToolStripDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private ISelectionService selectionService;
        private IDesignerHost designerHost;
        private IServiceProvider serviceProvider;
        private DesignerVerbCollection verbs;
        private ContextMenuStrip contextMenu;

        public FluentToolStrip ToolStrip => (FluentToolStrip)Control;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (component is FluentToolStrip toolStrip)
            {
                // 获取设计时服务
                serviceProvider = component.Site;  // 获取服务提供者
                selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));

                // 初始化上下文菜单
                InitializeContextMenu();

                // 启用拖放
                toolStrip.AllowDrop = true;
            }
        }

        #region 设计器动词

        //public override DesignerVerbCollection Verbs
        //{
        //    get
        //    {
        //        if (verbs == null)
        //        {
        //            verbs = new DesignerVerbCollection();

        //            // 添加项目
        //            verbs.Add(new DesignerVerb("添加按钮", (s, e) => AddItem(typeof(FluentToolStripButton))));
        //            verbs.Add(new DesignerVerb("添加标签", (s, e) => AddItem(typeof(FluentToolStripLabel))));
        //            verbs.Add(new DesignerVerb("添加分隔符", (s, e) => AddItem(typeof(FluentToolStripSeparator))));
        //            verbs.Add(new DesignerVerb("添加文本框", (s, e) => AddItem(typeof(FluentToolStripTextBox))));
        //            verbs.Add(new DesignerVerb("添加组合框", (s, e) => AddItem(typeof(FluentToolStripComboBox))));
        //            verbs.Add(new DesignerVerb("编辑项目...", (s, e) => ShowItemsEditor()));
        //        }
        //        return verbs;
        //    }
        //}

        #endregion

        #region 智能标记

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentToolStripActionList(this));
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
            addItemMenu.DropDownItems.Add("分隔符", null, (s, e) => AddItem(typeof(FluentToolStripSeparator)));
            addItemMenu.DropDownItems.Add("文本框", null, (s, e) => AddItem(typeof(FluentToolStripTextBox)));
            addItemMenu.DropDownItems.Add("列表框", null, (s, e) => AddItem(typeof(FluentToolStripComboBox)));
            addItemMenu.DropDownItems.Add("复选框", null, (s, e) => AddItem(typeof(FluentToolStripCheckBox)));
            addItemMenu.DropDownItems.Add("日期时间选择器", null, (s, e) => AddItem(typeof(FluentToolStripDateTimePicker)));

            contextMenu.Items.Add(addItemMenu);
            contextMenu.Items.Add(new ToolStripSeparator());

            // 编辑项目
            contextMenu.Items.Add("编辑项目...", null, (s, e) => ShowItemsEditor());
            contextMenu.Items.Add(new ToolStripSeparator());

            // 常用属性
            contextMenu.Items.Add("停靠在顶部", null, (s, e) => SetDock(DockStyle.Top));
            contextMenu.Items.Add("停靠在底部", null, (s, e) => SetDock(DockStyle.Bottom));
            contextMenu.Items.Add("停靠在左侧", null, (s, e) => SetDock(DockStyle.Left));
            contextMenu.Items.Add("停靠在右侧", null, (s, e) => SetDock(DockStyle.Right));
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
                ToolStrip.Items.Add(item);

                // 如果是控件宿主，确保控件被正确初始化
                if (item is FluentToolStripControlHost controlHost)
                {
                    // 强制更新布局
                    ToolStrip.PerformLayout();

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
                ToolStrip.Invalidate();

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
                button.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripLabel label)
            {
                label.Text = label.Name;
                label.Size = new Size(100, 28);
            }
            else if (item is FluentToolStripTextBox textBox)
            {
                textBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripCheckBox checkBox)
            {
                checkBox.Text = checkBox.Name;
                checkBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripComboBox comboBox)
            {
                comboBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripDateTimePicker dateTimePicker)
            {
                dateTimePicker.Size = new Size(150, 28);
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
                transaction = host?.CreateTransaction("Edit ToolStrip Items");

                // 创建项目集合的副本
                var itemsCopy = new List<FluentToolStripItem>();
                foreach (var item in ToolStrip.Items)
                {
                    itemsCopy.Add(item);
                }

                using (var editorForm = new FluentToolStripItemCollectionEditorForm(ToolStrip, itemsCopy, Component.Site))
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
            foreach (var item in ToolStrip.Items)
            {
                if (!newItems.Contains(item))
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                ToolStrip.Items.Remove(item);
                if (item.Site != null)
                {
                    host.DestroyComponent(item);
                }
            }

            // 重新排序项目
            ToolStrip.Items.Clear();
            foreach (var item in newItems)
            {
                ToolStrip.Items.Add(item);
            }

            // 刷新设计器
            RaiseComponentChanged(TypeDescriptor.GetProperties(ToolStrip)["Items"], null, ToolStrip.Items);
        }

        internal void ShowItemsEditorDialog()
        {
            var itemsCopy = new List<FluentToolStripItem>();
            foreach (var item in ToolStrip.Items)
            {
                itemsCopy.Add(item);
            }

            // 直接创建并显示自定义的编辑器窗体
            using (var editorForm = new FluentToolStripItemCollectionEditorForm(ToolStrip, itemsCopy, Component.Site))
            {
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    // 刷新设计器
                    if (designerHost != null)
                    {
                        using (var transaction = designerHost.CreateTransaction("Edit Items"))
                        {
                            // 触发变更通知
                            PropertyDescriptor itemsProperty = TypeDescriptor.GetProperties(ToolStrip)["Items"];
                            itemsProperty?.SetValue(ToolStrip, ToolStrip.Items);
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        private void SetDock(DockStyle dock)
        {
            if (designerHost == null)
            {
                return;
            }

            using (var transaction = designerHost.CreateTransaction("Set Dock"))
            {
                PropertyDescriptor dockProperty = TypeDescriptor.GetProperties(ToolStrip)["Dock"];
                dockProperty?.SetValue(ToolStrip, dock);
                transaction.Commit();
            }
        }

        #endregion

        #region 名称生成

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 确保Items属性在设计时可编辑
            PropertyDescriptor itemsProp = (PropertyDescriptor)properties["Items"];
            if (itemsProp != null)
            {
                properties["Items"] = TypeDescriptor.CreateProperty(
                    typeof(FluentToolStrip),
                    itemsProp,
                    new EditorAttribute(typeof(FluentToolStripItemCollectionEditor), typeof(UITypeEditor)));
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            // 设置默认属性
            if (Component is FluentToolStrip toolStrip)
            {
                toolStrip.Dock = DockStyle.Top;
                toolStrip.Height = 32;
                toolStrip.ShowGripHandle = true;
                toolStrip.RenderMode = FluentToolStripRenderMode.Professional;
            }
        }

        #endregion
    }

    /// <summary>
    /// 项目集合编辑器上下文
    /// </summary>
    internal class ItemCollectionEditorContext : ITypeDescriptorContext, IServiceProvider
    {
        private readonly FluentToolStrip toolStrip;
        private readonly IServiceProvider serviceProvider;

        public ItemCollectionEditorContext(FluentToolStrip toolStrip, IServiceProvider serviceProvider)
        {
            this.toolStrip = toolStrip;
            this.serviceProvider = serviceProvider;
        }

        public IContainer Container => serviceProvider?.GetService(typeof(IContainer)) as IContainer;

        public object Instance => toolStrip;

        public PropertyDescriptor PropertyDescriptor => TypeDescriptor.GetProperties(toolStrip)["Items"];

        public bool OnComponentChanging()
        {
            try
            {
                var componentChangeService = serviceProvider?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                componentChangeService?.OnComponentChanging(toolStrip, PropertyDescriptor);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void OnComponentChanged()
        {
            try
            {
                var componentChangeService = serviceProvider?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                componentChangeService?.OnComponentChanged(toolStrip, PropertyDescriptor, null, toolStrip.Items);
            }
            catch
            {
                // 忽略异常
            }
        }

        public object GetService(Type serviceType)
        {
            return serviceProvider?.GetService(serviceType);
        }
    }

    #endregion

    #region FluentToolStripActionList

    public class FluentToolStripActionList : DesignerActionList
    {
        private FluentToolStripDesigner designer;
        private FluentToolStrip toolStrip;

        public FluentToolStripActionList(FluentToolStripDesigner designer) : base(designer.Component)
        {
            this.designer = designer;
            this.toolStrip = designer.ToolStrip;
        }

        public DockStyle Dock
        {
            get => toolStrip.Dock;
            set => SetProperty("Dock", value);
        }

        public Orientation Orientation
        {
            get => toolStrip.Orientation;
            set => SetProperty("Orientation", value);
        }

        public bool ShowGripHandle
        {
            get => toolStrip.ShowGripHandle;
            set => SetProperty("ShowGripHandle", value);
        }

        public FluentToolStripRenderMode RenderMode
        {
            get => toolStrip.RenderMode;
            set => SetProperty("RenderMode", value);
        }

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
            designer.AddItem(typeof(FluentToolStripSeparator));
        }

        public void AddTextBox()
        {
            designer.AddItem(typeof(FluentToolStripTextBox));
        }

        public void AddCheckBox()
        {
            designer.AddItem(typeof(FluentToolStripCheckBox));
        }

        public void AddComboBox()
        {
            designer.AddItem(typeof(FluentToolStripComboBox));
        }

        public void AddDateTimePicker()
        {
            designer.AddItem(typeof(FluentToolStripDateTimePicker));
        }

        public void EditItems()
        {
            designer.ShowItemsEditorDialog();
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(toolStrip)[propertyName];
            property?.SetValue(toolStrip, value);
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 布局
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Dock", "停靠", "布局"));
            items.Add(new DesignerActionPropertyItem("Orientation", "方向", "布局"));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowGripHandle", "显示拖动手柄", "外观"));
            items.Add(new DesignerActionPropertyItem("RenderMode", "渲染模式", "外观"));

            // 操作
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "AddButton", "添加按钮", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddLabel", "添加标签", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddSeparator", "添加分隔符", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddTextBox", "添加文本框", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddCheckBox", "添加复选框", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddComboBox", "添加列表框", "操作"));
            items.Add(new DesignerActionMethodItem(this, "AddDateTimePicker", "添加日期时间选择器", "操作"));
            items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑项目...", "操作"));

            return items;
        }
    }

    #endregion

    #region FluentToolStripItemCollectionEditor

    /// <summary>
    /// FluentToolStrip项目集合编辑器
    /// </summary>
    public class FluentToolStripItemCollectionEditor : CollectionEditor
    {
        public FluentToolStripItemCollectionEditor() : base(typeof(FluentToolStripItemCollection))
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(FluentToolStripButton),
                typeof(FluentToolStripLabel),
                typeof(FluentToolStripSeparator),
                typeof(FluentToolStripTextBox),
                typeof(FluentToolStripComboBox),
                typeof(FluentToolStripCheckBox),
                typeof(FluentToolStripDateTimePicker)
            };
        }

        protected override object CreateInstance(Type itemType)
        {

            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                // 创建组件并分配唯一名称
                var component = host.CreateComponent(itemType) as FluentToolStripItem;
                if (component != null)
                {
                    // 设置默认属性
                    SetDefaultItemProperties(component);
                }
                return component;
            }

            // 备用方案：直接创建实例
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
                    // 通过设计器主机销毁
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
                // 创建设计器事务
                if (host != null)
                {
                    transaction = host.CreateTransaction("编辑FluentToolStrip项");
                }

                // 调用基类方法
                var result = base.SetItems(editValue, value);

                // 提交事务
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

        // 设置默认属性
        private void SetDefaultItemProperties(FluentToolStripItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item is FluentToolStripButton button)
            {
                button.Text = button.Text ?? button.Name;
                button.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripLabel label)
            {
                label.Text = label.Text ?? label.Name;
                label.Size = new Size(100, 28);
            }
            else if (item is FluentToolStripTextBox textBox)
            {
                textBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripCheckBox checkBox)
            {
                checkBox.Text = checkBox.Text ?? checkBox.Name;
                checkBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripComboBox comboBox)
            {
                comboBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripDateTimePicker dateTimePicker)
            {
                dateTimePicker.Size = new Size(150, 28);
            }
        }


        //private string GetUniqueName(Type itemType)
        //{
        //    IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
        //    if (host == null)
        //    {
        //        return null;
        //    }

        //    // 获取类型的基础名称
        //    string baseName = GetBaseName(itemType);

        //    // 查找唯一名称
        //    int index = 1;
        //    string name;
        //    do
        //    {
        //        name = $"{baseName}{index}";
        //        index++;
        //    } while (host.Container.Components[name] != null);

        //    return name;
        //}

    }

    #endregion

    #region FluentToolStripItemCollectionEditorForm

    /// <summary>
    /// 项目集合编辑器窗体
    /// </summary>
    public class FluentToolStripItemCollectionEditorForm : Form
    {
        private FluentToolStripItemCollection collection;
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

        private FluentToolStrip toolStrip;
        private List<FluentToolStripItem> workingItems;
        private List<FluentToolStripItem> itemsToDestroy;

        public FluentToolStripItemCollectionEditorForm(FluentToolStrip toolStrip, List<FluentToolStripItem> items,
            IServiceProvider serviceProvider)
        {
            this.toolStrip = toolStrip;
            this.collection = toolStrip.Items;
            this.workingItems = new List<FluentToolStripItem>(items);
            this.itemsToDestroy = new List<FluentToolStripItem>();
            this.serviceProvider = serviceProvider;
            InitializeComponents();
            LoadItems();
        }

        private void InitializeComponents()
        {
            Text = "FluentToolStrip 项目集合编辑器";
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

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));  // 标签
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 列表框
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));  // 按钮区域

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

            // 使用FlowLayoutPanel来组织按钮
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
                "TextBox",
                "ComboBox",
                "CheckBox",
                "DateTimePicker"
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

            // 底部操作按钮
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

            // 组装左侧面板
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

            // 创建右侧的TableLayoutPanel
            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));  // 标签
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 属性网格

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

            //底部按钮区
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

            // 使用FlowLayoutPanel来排列确定/取消按钮
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
            mainLayout.SetColumnSpan(dialogButtonPanel, 2);  // 底部按钮跨两列
            mainLayout.Controls.Add(dialogButtonPanel, 0, 1);

            // 添加到窗体
            Controls.Add(mainLayout);

            // 设置窗体属性
            AcceptButton = okButton;
            CancelButton = cancelButton;

            // 初始化按钮状态
            UpdateButtonStates();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                // 在关闭前应用更改
                ApplyChanges();
            }
            base.OnFormClosing(e);
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

            // 准备显示文本
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
            else if (item is FluentToolStripSeparator)
            {
                typeText = "[---]";
                typeColor = Color.Gray;
            }
            else if (item is FluentToolStripTextBox)
            {
                typeText = "[TextBox]";
                typeColor = Color.Purple;
            }
            else if (item is FluentToolStripComboBox)
            {
                typeText = "[Combo]";
                typeColor = Color.DarkOrange;
            }
            else if (item is FluentToolStripCheckBox)
            {
                typeText = "[Check]";
                typeColor = Color.DarkGreen;
            }
            else if (item is FluentToolStripDateTimePicker)
            {
                typeText = "[Date]";
                typeColor = Color.Brown;
            }

            // 组合显示文本
            displayText = item.Name;
            if (!string.IsNullOrEmpty(item.Text) && item.Text != item.Name)
            {
                displayText += $" - \"{item.Text}\"";
            }

            // 绘制类型标识(固定宽度)
            var typeRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 3, 60, e.Bounds.Height);
            using (var typeBrush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ?
                SystemColors.HighlightText : typeColor))
            {
                e.Graphics.DrawString(typeText, e.Font, typeBrush, typeRect);
            }

            // 绘制名称和文本
            var textRect = new Rectangle(e.Bounds.X + 65, e.Bounds.Y + 3, e.Bounds.Width - 65, e.Bounds.Height);
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
                    // 获取唯一名称
                    string uniqueName = GetUniqueNameFromHost(host, itemType);

                    // 创建组件
                    var item = (FluentToolStripItem)host.CreateComponent(itemType, uniqueName);

                    // 设置默认属性
                    SetDefaultProperties(item);

                    // 添加到工作列表
                    workingItems?.Add(item);

                    // 添加到集合
                    collection?.Add(item);
                    itemsListBox.Items.Add(item);
                    itemsListBox.SelectedItem = item;

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"添加项目失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // 备用方案：不使用设计器主机
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

            // 检查设计器主机中的所有组件和当前集合中的项目
            do
            {
                name = $"{baseName}{index}";
                index++;

                // 检查是否在设计器主机中已存在
                bool existsInHost = host.Container.Components[name] != null;

                // 检查是否在当前集合中已存在(不区分大小写)
                bool existsInCollection = collection.Cast<FluentToolStripItem>()
                    .Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));

                if (!existsInHost && !existsInCollection)
                {
                    break;
                }
            }
            while (index < 1000);  // 防止无限循环

            return name;
        }

        private string GetSimpleUniqueName(Type itemType)
        {
            string baseName = GetBaseName(itemType);
            int index = 1;
            string name;

            // 获取所有现有项目的名称
            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FluentToolStripItem item in collection)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    existingNames.Add(item.Name);
                }
            }

            // 查找唯一名称
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
            // 使用首字母小写的命名约定
            if (itemType == typeof(FluentToolStripButton))
            {
                return "toolStripButton";
            }
            else if (itemType == typeof(FluentToolStripLabel))
            {
                return "toolStripLabel";
            }
            else if (itemType == typeof(FluentToolStripSeparator))
            {
                return "toolStripSeparator";
            }
            else if (itemType == typeof(FluentToolStripTextBox))
            {
                return "toolStripTextBox";
            }
            else if (itemType == typeof(FluentToolStripComboBox))
            {
                return "toolStripComboBox";
            }
            else if (itemType == typeof(FluentToolStripCheckBox))
            {
                return "toolStripCheckBox";
            }
            else if (itemType == typeof(FluentToolStripDateTimePicker))
            {
                return "toolStripDateTimePicker";
            }
            else
            {
                return "toolStripItem";
            }
        }

        private void OnRemoveItem(object sender, EventArgs e)
        {
            if (itemsListBox.SelectedItem is FluentToolStripItem item)
            {
                int index = itemsListBox.SelectedIndex;

                // 从工作列表中移除
                workingItems?.Remove(item);
                itemsListBox.Items.Remove(item);

                // 标记为待销毁(在确定时才真正销毁)
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

            //if (itemsListBox.SelectedItem is FluentToolStripItem item)
            //{
            //    int index = itemsListBox.SelectedIndex;

            //    // 从集合中移除
            //    workingItems?.Remove(item);
            //    collection?.Remove(item);
            //    itemsListBox.Items.Remove(item);

            //    // 如果使用设计器主机，销毁组件
            //    IDesignerHost host = serviceProvider?.GetService(typeof(IDesignerHost)) as IDesignerHost;
            //    if (host != null && item.Site != null)
            //    {
            //        try
            //        {
            //            host.DestroyComponent(item);
            //        }
            //        catch
            //        {
            //            // 忽略销毁错误
            //        }
            //    }

            //    // 选择下一个项目
            //    if (itemsListBox.Items.Count > 0)
            //    {
            //        itemsListBox.SelectedIndex = Math.Min(index, itemsListBox.Items.Count - 1);
            //    }

            //    UpdateButtonStates();
            //}
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
                    catch
                    {
                        // 忽略销毁错误
                    }
                }
            }

            // 清空原始集合
            toolStrip.Items.Clear();

            // 添加工作列表中的项目
            foreach (var item in workingItems)
            {
                toolStrip.Items.Add(item);
            }
        }

        private Type GetSelectedItemType()
        {
            switch (itemTypeComboBox.SelectedItem?.ToString())
            {
                case "Button": return typeof(FluentToolStripButton);
                case "Label": return typeof(FluentToolStripLabel);
                case "Separator": return typeof(FluentToolStripSeparator);
                case "TextBox": return typeof(FluentToolStripTextBox);
                case "ComboBox": return typeof(FluentToolStripComboBox);
                case "CheckBox": return typeof(FluentToolStripCheckBox);
                case "DateTimePicker": return typeof(FluentToolStripDateTimePicker);
                default: return null;
            }
        }

        private void SetDefaultProperties(FluentToolStripItem item)
        {
            if (item is FluentToolStripButton button)
            {
                button.Text = button.Name;
                button.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripLabel label)
            {
                label.Text = label.Name;
                label.Size = new Size(100, 28);
            }
            else if (item is FluentToolStripTextBox textBox)
            {
                textBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripCheckBox checkBox)
            {
                checkBox.Text = checkBox.Name;
                checkBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripComboBox comboBox)
            {
                comboBox.Size = new Size(120, 28);
            }
            else if (item is FluentToolStripDateTimePicker dateTimePicker)
            {
                dateTimePicker.Size = new Size(150, 28);
            }
        }
    }

    #endregion

    #endregion
}

