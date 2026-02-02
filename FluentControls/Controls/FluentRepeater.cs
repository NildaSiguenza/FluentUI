using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public class FluentRepeater : FluentContainerBase
    {
        #region 字段

        private Func<Control> itemFactory;
        private Size itemDefaultSize = new Size(200, 100);
        private RepeaterLayoutMode layoutMode = RepeaterLayoutMode.Auto;
        private int maxItemCount = int.MaxValue;
        private Padding itemPadding = new Padding(8);

        private Panel scrollablePanel;
        private Panel containerPanel;
        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;

        private List<RepeaterItemWrapper> items = new List<RepeaterItemWrapper>();

        private Button addButton;
        private RepeaterItemWrapper hoveredItem;
        private bool isHoveringEmptySpace;
        private Point lastMousePosition;

        private const int ICON_SIZE = 30;
        private const int ICON_SPACING = 4;
        private const int SCROLLBAR_SIZE = 17;
        private int deleteIconSize = 24; // 删除按钮大小


        private bool autoScroll = true;
        private Color borderColor = Color.FromArgb(200, 200, 200);
        private int borderWidth = 1;
        private int cornerRadius = 4;

        // 浮动图标按钮
        private Timer hoverTimer;
        private const int HOVER_DELAY = 300; // 悬停延迟毫秒数
        private RepeaterItemWrapper pendingHoverItem;
        private bool pendingAddButtonShow;
        private Point addButtonTargetPosition; // 添加按钮目标位置

        // 事件
        public event EventHandler<RepeaterItemEventArgs> ItemAdded;
        public event EventHandler<RepeaterItemEventArgs> ItemRemoved;

        #endregion

        #region 构造函数

        public FluentRepeater()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 基础设置
            this.SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.FromArgb(250, 250, 250);
            this.Padding = new Padding(10);
            this.Size = new Size(400, 300);

            // 创建可滚动面板
            scrollablePanel = new Panel
            {
                Location = new Point(this.Padding.Left, this.Padding.Top),
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            this.Controls.Add(scrollablePanel);

            // 创建容器面板
            containerPanel = new Panel
            {
                Location = Point.Empty,
                AutoSize = false,
                BackColor = Color.Transparent
            };
            scrollablePanel.Controls.Add(containerPanel);

            // 创建垂直滚动条
            vScrollBar = new VScrollBar
            {
                Visible = false,
                SmallChange = 20,
                LargeChange = 100
            };
            vScrollBar.Scroll += VScrollBar_Scroll;
            this.Controls.Add(vScrollBar);

            // 创建水平滚动条
            hScrollBar = new HScrollBar
            {
                Visible = false,
                SmallChange = 20,
                LargeChange = 100
            };
            hScrollBar.Scroll += HScrollBar_Scroll;
            this.Controls.Add(hScrollBar);

            // 创建添加按钮
            addButton = CreateIconButton("+", "添加新项", Color.FromArgb(0, 120, 212));
            addButton.Click += AddButton_Click;
            addButton.Visible = false;
            this.Controls.Add(addButton);

            // 初始化悬停定时器
            hoverTimer = new Timer();
            hoverTimer.Interval = HOVER_DELAY;
            hoverTimer.Tick += HoverTimer_Tick;

            // 事件订阅
            this.SizeChanged += FluentRepeater_SizeChanged;
            this.MouseMove += FluentRepeater_MouseMove;
            this.MouseLeave += FluentRepeater_MouseLeave;
            this.MouseWheel += FluentRepeater_MouseWheel;
            containerPanel.MouseMove += ContainerPanel_MouseMove;
            containerPanel.MouseLeave += ContainerPanel_MouseLeave;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 内部控件默认大小
        /// </summary>
        [Category("Repeater")]
        [Description("内部控件的默认大小")]
        public Size ItemDefaultSize
        {
            get => itemDefaultSize;
            set
            {
                itemDefaultSize = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 布局模式
        /// </summary>
        [Category("Repeater")]
        [Description("控件的布局模式")]
        [DefaultValue(RepeaterLayoutMode.Auto)]
        public RepeaterLayoutMode LayoutMode
        {
            get => layoutMode;
            set
            {
                if (layoutMode != value)
                {
                    layoutMode = value;
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// 控件间距
        /// </summary>
        [Category("Repeater")]
        [Description("内部控件之间的间距")]
        public Padding ItemPadding
        {
            get => itemPadding;
            set
            {
                itemPadding = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 最大控件数量
        /// </summary>
        [Category("Repeater")]
        [Description("容器可包含的最大控件数量")]
        public int MaxItemCount
        {
            get => maxItemCount;
            set
            {
                maxItemCount = value;
                UpdateAddButtonVisibility();
            }
        }

        /// <summary>
        /// 当前控件数量
        /// </summary>
        [Browsable(false)]
        public int ItemCount => items.Count;

        /// <summary>
        /// 是否已达容量上限
        /// </summary>
        [Browsable(false)]
        public bool IsFull => items.Count >= maxItemCount;

        /// <summary>
        /// 是否启用自动滚动
        /// </summary>
        [Category("Repeater")]
        [Description("是否启用自动滚动")]
        [DefaultValue(true)]
        public bool AutoScroll
        {
            get => autoScroll;
            set
            {
                if (autoScroll != value)
                {
                    autoScroll = value;
                    UpdateScrollBars();
                }
            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        [Category("Repeater")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 边框宽度
        /// </summary>
        [Category("Repeater")]
        [Description("边框宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth != value)
                {
                    borderWidth = Math.Max(0, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        [Category("Repeater")]
        [Description("圆角半径")]
        [DefaultValue(4)]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                if (cornerRadius != value)
                {
                    cornerRadius = Math.Max(0, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 删除图标大小
        /// </summary>
        [Category("Repeater")]
        [Description("删除图标按钮的大小")]
        [DefaultValue(24)]
        public int DeleteIconSize
        {
            get => deleteIconSize;
            set
            {
                if (deleteIconSize != value && value > 0)
                {
                    deleteIconSize = value;
                    // 重新创建所有wrapper的删除按钮
                    foreach (var wrapper in items)
                    {
                        wrapper.UpdateDeleteButtonSize(value);
                    }
                }
            }
        }

        #endregion

        #region 抽象方法实现

        protected override void DrawBackground(Graphics g)
        {
            if (g == null)
            {
                return;
            }

            // 绘制背景
            using (var brush = new SolidBrush(this.BackColor))
            {
                if (cornerRadius > 0)
                {
                    using (var path = GetRoundedRectPath(this.ClientRectangle, cornerRadius))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    g.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (g == null || borderWidth <= 0)
            {
                return;
            }

            // 绘制边框
            using (var pen = new Pen(borderColor, borderWidth))
            {
                pen.Alignment = PenAlignment.Inset;

                if (cornerRadius > 0)
                {
                    var rect = this.ClientRectangle;
                    rect.Inflate(-borderWidth / 2, -borderWidth / 2);
                    using (var path = GetRoundedRectPath(rect, cornerRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    var rect = this.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 内容由子控件自己绘制，这里不需要额外绘制
            // 可以在这里绘制一些装饰性元素或提示文本
            if (items.Count == 0 && itemFactory == null)
            {
                // 绘制提示文本
                string hintText = "请先设置项目工厂或模板\n然后鼠标悬停添加项目";
                using (var font = new Font("Microsoft YaHei UI", 10))
                using (var brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(hintText, font, brush, this.ClientRectangle, format);
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置项目工厂
        /// e.g. repeater.SetItemFactory<Button>();
        /// </summary>
        public void SetItemFactory<T>() where T : Control, new()
        {
            itemFactory = () => new T();
        }

        /// <summary>
        /// 设置项目工厂
        /// e.g. repeater.SetItemFactory(() => new Button() { Text = "Button" });
        /// </summary>
        public void SetItemFactory(Func<Control> factory)
        {
            itemFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// 设置项目模板
        /// e.g.
        ///     var template = new Panel();
        ///     repeater.SetItemTemplate(template);
        /// </summary>
        public void SetItemTemplate(Control template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            itemFactory = () => CloneControl(template);
        }

        /// <summary>
        /// 手动添加项目
        /// </summary>
        public Control AddItem()
        {
            if (IsFull)
            {
                MessageBox.Show($"已达到最大容量限制({maxItemCount}项)", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            if (itemFactory == null)
            {
                MessageBox.Show("请先设置项目工厂或模板", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var control = itemFactory();
            if (control != null)
            {
                control.Size = itemDefaultSize;
                AddItemInternal(control);
            }

            return control;
        }

        /// <summary>
        /// 移除指定项目
        /// </summary>
        public void RemoveItem(Control item)
        {
            var wrapper = items.FirstOrDefault(w => w.ItemControl == item);
            if (wrapper != null)
            {
                RemoveItemInternal(wrapper);
            }
        }

        /// <summary>
        /// 移除指定索引的项目
        /// </summary>
        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                RemoveItemInternal(items[index]);
            }
        }

        /// <summary>
        /// 清除所有项目
        /// </summary>
        public void ClearItems()
        {
            while (items.Count > 0)
            {
                RemoveItemInternal(items[0]);
            }
        }

        /// <summary>
        /// 获取所有项目控件
        /// </summary>
        public IEnumerable<Control> GetItems()
        {
            return items.Select(w => w.ItemControl);
        }

        /// <summary>
        /// 获取指定索引的项目
        /// </summary>
        public Control GetItemAt(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                return items[index].ItemControl;
            }

            return null;
        }

        #endregion

        #region 项目管理

        private void AddItemInternal(Control control)
        {
            var wrapper = new RepeaterItemWrapper(control, ICON_SIZE, deleteIconSize); // 传递删除按钮大小
            wrapper.DeleteButton.Click += (s, e) => RemoveItemInternal(wrapper);
            wrapper.MouseEnter += ItemWrapper_MouseEnter;
            wrapper.MouseLeave += ItemWrapper_MouseLeave;
            wrapper.ItemControl.MouseEnter += ItemControl_MouseEnter;
            wrapper.ItemControl.MouseLeave += ItemControl_MouseLeave;

            items.Add(wrapper);
            containerPanel.Controls.Add(wrapper);

            // 应用主题到新添加的控件
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                ApplyThemeToControl(control, true);
            }

            OnItemAdded(new RepeaterItemEventArgs(control, items.Count - 1));

            UpdateLayout();
        }


        private void RemoveItemInternal(RepeaterItemWrapper wrapper)
        {
            int index = items.IndexOf(wrapper);
            var control = wrapper.ItemControl;

            items.Remove(wrapper);
            containerPanel.Controls.Remove(wrapper);
            wrapper.Dispose();

            OnItemRemoved(new RepeaterItemEventArgs(control, index));

            UpdateLayout();
            UpdateAddButtonVisibility();
        }

        private Control CloneControl(Control template)
        {
            var clone = Activator.CreateInstance(template.GetType()) as Control;
            if (clone != null)
            {
                clone.Size = template.Size;
                clone.BackColor = template.BackColor;
                clone.ForeColor = template.ForeColor;
                clone.Font = new Font(template.Font, template.Font.Style);
                clone.Text = template.Text;
                clone.Padding = template.Padding;
                clone.Margin = template.Margin;

                // 复制子控件
                foreach (Control child in template.Controls)
                {
                    var childClone = CloneControl(child);
                    if (childClone != null)
                    {
                        clone.Controls.Add(childClone);
                    }
                }
            }

            return clone;
        }

        #endregion

        #region 布局

        private void UpdateLayout()
        {
            if (items.Count == 0)
            {
                containerPanel.Size = GetViewportSize();
                UpdateScrollBars();
                Invalidate();
                return;
            }

            this.SuspendLayout();

            switch (layoutMode)
            {
                case RepeaterLayoutMode.Horizontal:
                    LayoutHorizontal();
                    break;
                case RepeaterLayoutMode.Vertical:
                    LayoutVertical();
                    break;
                case RepeaterLayoutMode.Auto:
                    LayoutAuto();
                    break;
            }

            UpdateScrollBars();
            this.ResumeLayout();
        }

        private void LayoutHorizontal()
        {
            int x = 0;
            int maxHeight = 0;

            foreach (var wrapper in items)
            {
                wrapper.Location = new Point(x, 0);
                // 不需要额外空间给添加按钮了
                wrapper.Size = new Size(itemDefaultSize.Width, itemDefaultSize.Height);
                wrapper.LayoutIcons(RepeaterLayoutMode.Horizontal);

                x += itemDefaultSize.Width + itemPadding.Left + itemPadding.Right;
                maxHeight = Math.Max(maxHeight, wrapper.Height);
            }

            containerPanel.Size = new Size(
                x + itemPadding.Right,
                maxHeight + itemPadding.Top + itemPadding.Bottom
            );
        }

        private void LayoutVertical()
        {
            int y = 0;
            int maxWidth = 0;

            foreach (var wrapper in items)
            {
                wrapper.Location = new Point(0, y);
                // 不需要额外空间给添加按钮了
                wrapper.Size = new Size(itemDefaultSize.Width, itemDefaultSize.Height);
                wrapper.LayoutIcons(RepeaterLayoutMode.Vertical);

                y += itemDefaultSize.Height + itemPadding.Top + itemPadding.Bottom;
                maxWidth = Math.Max(maxWidth, wrapper.Width);
            }

            containerPanel.Size = new Size(
                maxWidth + itemPadding.Left + itemPadding.Right,
                y + itemPadding.Bottom
            );
        }

        private void LayoutAuto()
        {
            var viewportSize = GetViewportSize();
            int availableWidth = viewportSize.Width;
            int itemWidthWithPadding = itemDefaultSize.Width + itemPadding.Left + itemPadding.Right;
            int itemHeightWithPadding = itemDefaultSize.Height + itemPadding.Top + itemPadding.Bottom;

            int itemsPerRow = Math.Max(1, availableWidth / itemWidthWithPadding);

            int x = 0;
            int y = 0;
            int rowHeight = 0;
            int itemsInCurrentRow = 0;
            int maxWidth = 0;

            foreach (var wrapper in items)
            {
                if (itemsInCurrentRow >= itemsPerRow)
                {
                    x = 0;
                    y += rowHeight + itemPadding.Top + itemPadding.Bottom;
                    rowHeight = 0;
                    itemsInCurrentRow = 0;
                }

                wrapper.Location = new Point(x, y);
                // 不需要额外空间给添加按钮了
                wrapper.Size = new Size(itemDefaultSize.Width, itemDefaultSize.Height);
                wrapper.LayoutIcons(RepeaterLayoutMode.Auto);

                x += wrapper.Width + itemPadding.Left + itemPadding.Right;
                rowHeight = Math.Max(rowHeight, wrapper.Height);
                maxWidth = Math.Max(maxWidth, x);
                itemsInCurrentRow++;
            }

            containerPanel.Size = new Size(
                Math.Max(availableWidth, maxWidth),
                y + rowHeight + itemPadding.Bottom
            );
        }

        private Size GetViewportSize()
        {
            int width = this.ClientSize.Width - this.Padding.Left - this.Padding.Right;
            int height = this.ClientSize.Height - this.Padding.Top - this.Padding.Bottom;

            if (vScrollBar.Visible)
            {
                width -= SCROLLBAR_SIZE;
            }

            if (hScrollBar.Visible)
            {
                height -= SCROLLBAR_SIZE;
            }

            return new Size(Math.Max(0, width), Math.Max(0, height));
        }

        #endregion

        #region 滚动条

        private void UpdateScrollBars()
        {
            if (!autoScroll)
            {
                vScrollBar.Visible = false;
                hScrollBar.Visible = false;
                return;
            }

            var viewportSize = GetViewportSize();
            var contentSize = containerPanel.Size;

            // 判断是否需要滚动条
            bool needVScroll = contentSize.Height > viewportSize.Height;
            bool needHScroll = false;

            switch (layoutMode)
            {
                case RepeaterLayoutMode.Horizontal:
                    needHScroll = contentSize.Width > viewportSize.Width;
                    needVScroll = false;
                    break;
                case RepeaterLayoutMode.Vertical:
                    needHScroll = false;
                    needVScroll = contentSize.Height > viewportSize.Height;
                    break;
                case RepeaterLayoutMode.Auto:
                    needVScroll = contentSize.Height > viewportSize.Height;
                    needHScroll = contentSize.Width > viewportSize.Width;
                    break;
            }

            // 更新垂直滚动条
            if (needVScroll)
            {
                vScrollBar.Visible = true;
                vScrollBar.Bounds = new Rectangle(
                    this.ClientSize.Width - SCROLLBAR_SIZE,
                    this.Padding.Top,
                    SCROLLBAR_SIZE,
                    this.ClientSize.Height - this.Padding.Top - this.Padding.Bottom - (needHScroll ? SCROLLBAR_SIZE : 0)
                );
                vScrollBar.Maximum = contentSize.Height;
                vScrollBar.LargeChange = viewportSize.Height;
                vScrollBar.BringToFront();
            }
            else
            {
                vScrollBar.Visible = false;
                vScrollBar.Value = 0;
            }

            // 更新水平滚动条
            if (needHScroll)
            {
                hScrollBar.Visible = true;
                hScrollBar.Bounds = new Rectangle(
                    this.Padding.Left,
                    this.ClientSize.Height - SCROLLBAR_SIZE,
                    this.ClientSize.Width - this.Padding.Left - this.Padding.Right - (needVScroll ? SCROLLBAR_SIZE : 0),
                    SCROLLBAR_SIZE
                );
                hScrollBar.Maximum = contentSize.Width;
                hScrollBar.LargeChange = viewportSize.Width;
                hScrollBar.BringToFront();
            }
            else
            {
                hScrollBar.Visible = false;
                hScrollBar.Value = 0;
            }

            // 更新滚动面板大小和位置
            scrollablePanel.Bounds = new Rectangle(
                this.Padding.Left,
                this.Padding.Top,
                viewportSize.Width,
                viewportSize.Height
            );

            UpdateContainerPosition();
        }

        private void UpdateContainerPosition()
        {
            containerPanel.Location = new Point(-hScrollBar.Value, -vScrollBar.Value);
        }

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateContainerPosition();
            UpdateAddButtonVisibility();
        }

        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateContainerPosition();
            UpdateAddButtonVisibility();
        }

        #endregion

        #region 鼠标交互


        private void FluentRepeater_MouseMove(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            CheckEmptySpaceHover(e.Location);
        }

        private void FluentRepeater_MouseLeave(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            HideAllIcons();
        }

        private void ContainerPanel_MouseMove(object sender, MouseEventArgs e)
        {
            var screenPoint = containerPanel.PointToScreen(e.Location);
            var clientPoint = this.PointToClient(screenPoint);
            CheckEmptySpaceHover(clientPoint);
        }

        private void ContainerPanel_MouseLeave(object sender, EventArgs e)
        {
            // 不立即隐藏，等待鼠标真正离开
        }

        private void CheckEmptySpaceHover(Point location)
        {
            if (IsFull)
            {
                hoverTimer.Stop();
                HideAllIcons();
                return;
            }

            var containerLocation = new Point(
                location.X - containerPanel.Left - this.Padding.Left,
                location.Y - containerPanel.Top - this.Padding.Top
            );

            bool isOverItem = items.Any(w => w.Bounds.Contains(containerLocation));
            bool isInScrollableArea = scrollablePanel.ClientRectangle.Contains(
                new Point(location.X - this.Padding.Left, location.Y - this.Padding.Top));

            if (!isOverItem && isInScrollableArea)
            {
                // 在空白区域，记录鼠标位置
                addButtonTargetPosition = location;
                StartHoverTimer(null, true);
            }
        }


        private void ItemWrapper_MouseEnter(object sender, EventArgs e)
        {
            var wrapper = sender as RepeaterItemWrapper;
            if (wrapper != null)
            {
                StartHoverTimer(wrapper, false);
            }
        }

        private void ItemWrapper_MouseLeave(object sender, EventArgs e)
        {
            var wrapper = sender as RepeaterItemWrapper;
            if (wrapper != null)
            {
                var screenPos = Cursor.Position;
                var clientPos = wrapper.PointToClient(screenPos);

                if (!wrapper.ClientRectangle.Contains(clientPos))
                {
                    hoverTimer.Stop();
                    wrapper.HideDeleteIcon();
                }
            }
        }

        private void ItemControl_MouseEnter(object sender, EventArgs e)
        {
            // 当鼠标进入内部控件时，也触发显示删除按钮
            var control = sender as Control;
            if (control != null)
            {
                var wrapper = items.FirstOrDefault(w => w.ItemControl == control);
                if (wrapper != null)
                {
                    StartHoverTimer(wrapper, false);
                }
            }
        }

        private void ItemControl_MouseLeave(object sender, EventArgs e)
        {
            // 检查是否真的离开了wrapper区域
            var control = sender as Control;
            if (control != null)
            {
                var wrapper = items.FirstOrDefault(w => w.ItemControl == control);
                if (wrapper != null)
                {
                    var screenPos = Cursor.Position;
                    var clientPos = wrapper.PointToClient(screenPos);

                    if (!wrapper.ClientRectangle.Contains(clientPos))
                    {
                        hoverTimer.Stop();
                        wrapper.HideDeleteIcon();
                    }
                }
            }
        }

        #endregion

        #region 事件处理

        private void UpdateAddButtonVisibility()
        {

        }

        private void PositionAddButton()
        {
            Point position;

            if (items.Count == 0)
            {
                // 没有项时，使用鼠标位置或默认位置
                if (addButtonTargetPosition != Point.Empty)
                {
                    position = addButtonTargetPosition;
                }
                else
                {
                    position = new Point(
                        this.Padding.Left + 10,
                        this.Padding.Top + 10
                    );
                }
            }
            else
            {
                // 有项时，在鼠标位置附近显示，但避免与现有项重叠
                position = FindNearestEmptyPosition(addButtonTargetPosition);
            }

            addButton.Location = position;
            addButton.BringToFront();
        }

        /// <summary>
        /// 查找最接近目标位置的空白位置
        /// </summary>
        private Point FindNearestEmptyPosition(Point targetPosition)
        {
            // 如果目标位置为空，返回默认位置
            if (targetPosition == Point.Empty)
            {
                return new Point(this.Padding.Left + 10, this.Padding.Top + 10);
            }

            // 转换为容器内的坐标
            var containerPoint = new Point(
                targetPosition.X - containerPanel.Left - this.Padding.Left,
                targetPosition.Y - containerPanel.Top - this.Padding.Top
            );

            // 检查目标位置是否与任何项重叠
            bool overlaps = items.Any(w => IsOverlapping(
                new Rectangle(containerPoint, addButton.Size),
                w.Bounds));

            if (!overlaps)
            {
                // 不重叠，直接使用目标位置
                return targetPosition;
            }

            // 重叠了，寻找附近的空位
            // 尝试的偏移方向：右、下、左、上、右下、右上、左下、左上
            var offsets = new[]
            {
                new Point(addButton.Width + 10, 0),                              // 右
                new Point(0, addButton.Height + 10),                             // 下
                new Point(-(addButton.Width + 10), 0),                           // 左
                new Point(0, -(addButton.Height + 10)),                          // 上
                new Point(addButton.Width + 10, addButton.Height + 10),          // 右下
                new Point(addButton.Width + 10, -(addButton.Height + 10)),       // 右上
                new Point(-(addButton.Width + 10), addButton.Height + 10),       // 左下
                new Point(-(addButton.Width + 10), -(addButton.Height + 10))    // 左上
            };

            foreach (var offset in offsets)
            {
                var testPoint = new Point(containerPoint.X + offset.X, containerPoint.Y + offset.Y);
                var testRect = new Rectangle(testPoint, addButton.Size);

                // 检查是否在可视区域内
                if (!IsInViewport(testRect))
                {
                    continue;
                }

                // 检查是否与项重叠
                bool testOverlaps = items.Any(w => IsOverlapping(testRect, w.Bounds));
                if (!testOverlaps)
                {
                    // 找到合适位置，转换回控件坐标
                    return new Point(
                        testPoint.X + containerPanel.Left + this.Padding.Left,
                        testPoint.Y + containerPanel.Top + this.Padding.Top
                    );
                }
            }

            // 如果所有方向都重叠，使用默认策略：放在最后一项的后面
            return GetPositionAfterLastItem();
        }

        /// <summary>
        /// 检查两个矩形是否重叠(带边距)
        /// </summary>
        private bool IsOverlapping(Rectangle rect1, Rectangle rect2)
        {
            // 添加一些边距，避免太靠近
            int margin = 5;
            rect2.Inflate(margin, margin);
            return rect1.IntersectsWith(rect2);
        }

        /// <summary>
        /// 检查矩形是否在可视区域内
        /// </summary>
        private bool IsInViewport(Rectangle rect)
        {
            var viewportSize = GetViewportSize();
            var viewportRect = new Rectangle(0, 0, viewportSize.Width, viewportSize.Height);

            // 至少要有一半在可视区域内
            var intersection = Rectangle.Intersect(rect, viewportRect);
            return intersection.Width >= rect.Width / 2 && intersection.Height >= rect.Height / 2;
        }

        /// <summary>
        /// 获取最后一项之后的位置
        /// </summary>
        private Point GetPositionAfterLastItem()
        {
            if (items.Count == 0)
            {
                return new Point(this.Padding.Left + 10, this.Padding.Top + 10);
            }

            var lastWrapper = items.Last();
            Point position;

            switch (layoutMode)
            {
                case RepeaterLayoutMode.Horizontal:
                    position = new Point(
                        lastWrapper.Right + itemPadding.Right + 10,
                        lastWrapper.Top + (lastWrapper.Height - addButton.Height) / 2
                    );
                    break;

                case RepeaterLayoutMode.Vertical:
                    position = new Point(
                        lastWrapper.Left + (lastWrapper.Width - addButton.Width) / 2,
                        lastWrapper.Bottom + itemPadding.Bottom + 10
                    );
                    break;

                case RepeaterLayoutMode.Auto:
                    var viewportSize = GetViewportSize();
                    int itemWidthWithPadding = itemDefaultSize.Width + itemPadding.Left + itemPadding.Right;
                    int itemsPerRow = Math.Max(1, viewportSize.Width / itemWidthWithPadding);

                    int currentRowItems = items.Count % itemsPerRow;
                    if (currentRowItems == 0)
                    {
                        currentRowItems = itemsPerRow;
                    }

                    int viewportRight = scrollablePanel.Width;
                    int buttonRightEdge = lastWrapper.Right + itemPadding.Right + addButton.Width + 20 - containerPanel.Left;

                    if (currentRowItems < itemsPerRow && buttonRightEdge < viewportRight)
                    {
                        // 同一行
                        position = new Point(
                            lastWrapper.Right + itemPadding.Right + 10,
                            lastWrapper.Top + (lastWrapper.Height - addButton.Height) / 2
                        );
                    }
                    else
                    {
                        // 新起一行
                        position = new Point(
                            itemPadding.Left + 10,
                            lastWrapper.Bottom + itemPadding.Bottom + 10
                        );
                    }
                    break;

                default:
                    position = new Point(this.Padding.Left + 10, this.Padding.Top + 10);
                    break;
            }

            // 转换为控件坐标
            return new Point(
                position.X + containerPanel.Left + this.Padding.Left,
                position.Y + containerPanel.Top + this.Padding.Top
            );
        }

        private void FluentRepeater_SizeChanged(object sender, EventArgs e)
        {
            if (layoutMode == RepeaterLayoutMode.Auto)
            {
                UpdateLayout();
            }
            else
            {
                UpdateScrollBars();
            }
        }

        private void FluentRepeater_MouseWheel(object sender, MouseEventArgs e)
        {
            if (vScrollBar.Visible)
            {
                int newValue = vScrollBar.Value - e.Delta / 3;
                newValue = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange + 1, newValue));
                vScrollBar.Value = newValue;
                UpdateContainerPosition();
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddItem();

            // 清除目标位置，避免重复使用
            addButtonTargetPosition = Point.Empty;

            // 隐藏添加按钮
            addButton.Visible = false;
        }

        #endregion

        #region 悬停定时器

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            hoverTimer.Stop();

            if (pendingHoverItem != null)
            {
                // 显示删除图标
                pendingHoverItem.ShowDeleteIcon();
                pendingHoverItem = null;
            }
            else if (pendingAddButtonShow)
            {
                // 显示添加按钮
                addButton.Visible = true;
                PositionAddButton();
                addButton.BringToFront();
                pendingAddButtonShow = false;
            }
        }

        private void StartHoverTimer(RepeaterItemWrapper item = null, bool showAddButton = false)
        {
            hoverTimer.Stop();

            // 立即隐藏所有图标
            HideAllIcons();

            pendingHoverItem = item;
            pendingAddButtonShow = showAddButton;

            if (item != null || showAddButton)
            {
                hoverTimer.Start();
            }
        }

        private void HideAllIcons()
        {
            foreach (var wrapper in items)
            {
                wrapper.HideDeleteIcon();
            }
            addButton.Visible = false;
        }

        #endregion

        #region 辅助方法

        private Button CreateIconButton(string text, string tooltip, Color color)
        {
            var button = new Button
            {
                Size = new Size(ICON_SIZE, ICON_SIZE),
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.2f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);

            var toolTip = new ToolTip();
            toolTip.SetToolTip(button, tooltip);

            return button;
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

        #region 事件

        protected virtual void OnItemAdded(RepeaterItemEventArgs e)
        {
            ItemAdded?.Invoke(this, e);
        }

        protected virtual void OnItemRemoved(RepeaterItemEventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hoverTimer?.Stop();
                hoverTimer?.Dispose();
                ClearItems();
                addButton?.Dispose();
                containerPanel?.Dispose();
                scrollablePanel?.Dispose();
                vScrollBar?.Dispose();
                hScrollBar?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 项包装器

    [ToolboxItem(false)]
    public class RepeaterItemWrapper : Panel
    {
        private int iconSize;
        private int deleteIconSize;
        private RepeaterLayoutMode currentLayoutMode;
        private const int ICON_MARGIN = 4; // 图标边距

        public RepeaterItemWrapper(Control itemControl, int iconSize, int deleteIconSize)
        {
            ItemControl = itemControl;
            this.iconSize = iconSize;
            this.deleteIconSize = deleteIconSize;

            this.BackColor = Color.White;
            this.Padding = new Padding(4);

            // 绘制边框
            this.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    var rect = this.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            ItemControl.Location = new Point(this.Padding.Left, this.Padding.Top);
            this.Controls.Add(ItemControl);

            // 创建删除按钮
            DeleteButton = CreateIconButton("×", "删除此项", Color.FromArgb(180, 232, 17, 35));
            DeleteButton.Visible = false;
            this.Controls.Add(DeleteButton);
        }

        public Control ItemControl { get; }
        public FluentButton DeleteButton { get; }

        /// <summary>
        /// 更新删除按钮大小
        /// </summary>
        public void UpdateDeleteButtonSize(int size)
        {
            deleteIconSize = size;
            DeleteButton.Size = new Size(size, size);
            LayoutIcons(currentLayoutMode);
        }

        public void LayoutIcons(RepeaterLayoutMode layoutMode)
        {
            currentLayoutMode = layoutMode;

            // 删除按钮始终在右上角
            DeleteButton.Location = new Point(
                this.Width - deleteIconSize - ICON_MARGIN,
                ICON_MARGIN
            );
        }

        public void ShowDeleteIcon()
        {
            DeleteButton.Visible = true;
            DeleteButton.BringToFront(); // 确保在最上层
        }

        public void HideDeleteIcon()
        {
            DeleteButton.Visible = false;
        }

        private FluentButton CreateIconButton(string text, string tooltip, Color color)
        {
            var button = new FluentButton
            {
                Size = new Size(deleteIconSize, deleteIconSize),
                Text = text,
                Font = new Font("Microsoft YaHei", deleteIconSize * 0.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = color,
                Cursor = Cursors.Hand,
                TabStop = false,
                HoverBackColor = ControlPaint.Light(color, 0.2f),
                PressedBackColor = ControlPaint.Dark(color, 0.1f)
            };

            var toolTip = new ToolTip();
            toolTip.SetToolTip(button, tooltip);

            return button;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ItemControl?.Dispose();
                DeleteButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    #endregion

    #region 枚举与辅助类

    /// <summary>
    /// 布局模式
    /// </summary>
    public enum RepeaterLayoutMode
    {
        Auto,
        Horizontal,
        Vertical
    }

    public class RepeaterItemEventArgs : EventArgs
    {
        public RepeaterItemEventArgs(Control item, int index)
        {
            Item = item;
            Index = index;
        }

        public Control Item { get; }

        public int Index { get; }

    }

    #endregion
}
