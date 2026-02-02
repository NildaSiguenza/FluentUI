using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Collections;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using Infrastructure;

namespace FluentControls.Controls
{
    [DefaultEvent("SelectedIndexChanged")]
    [DefaultProperty("Items")]
    [Designer(typeof(FluentListViewDesigner))]
    public class FluentListView : FluentControlBase
    {
        #region 字段

        private FluentListViewMode viewMode = FluentListViewMode.Details;
        private FluentListViewColumnCollection columns;
        private FluentListViewItemCollection items;
        private ImageList largeImageList;
        private ImageList smallImageList;

        private bool showColumnHeaders = true;
        private bool fullRowSelect = true;
        private bool gridLines = false;
        private bool checkBoxes = false;
        private bool multiSelect = true;
        private bool hoverSelection = false;
        private bool allowColumnReorder = false;

        private int headerHeight = 28;
        private int itemHeight = 24;
        private int largeIconSize = 48;
        private int smallIconSize = 16;
        private int itemSpacing = 4;
        private ListViewCheckBoxStyle checkBoxStyle = ListViewCheckBoxStyle.Check;
        private Padding contentPadding = new Padding(4);
        private int tileItemHeight = 64;

        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;
        private int scrollOffsetY = 0;
        private int scrollOffsetX = 0;

        private FluentListViewItem hoveredItem;
        private FluentListViewColumn hoveredColumn;
        private List<FluentListViewItem> selectedItems;
        private FluentListViewItem focusedItem;

        private Point mouseDownPoint;
        private bool isDraggingColumn;
        private FluentListViewColumn draggingColumn;

        private int smallIconItemWidth = 100;
        private int largeIconItemWidth = 120;

        // 排序
        private int sortColumnIndex = -1;
        private ColumnSortOrder sortOrder = ColumnSortOrder.None;
        private IComparer itemComparer;

        // 列宽调整
        private bool allowColumnResize = true;
        private bool isResizingColumn = false;
        private FluentListViewColumn resizingColumn;
        private int resizeStartX;
        private int resizeStartWidth;
        private const int ResizeHitArea = 4; // 列边界热区宽度

        // 显示项边界(调试)
        private bool showItemBounds = false;

        #endregion

        #region 构造函数

        public FluentListView()
        {
            columns = new FluentListViewColumnCollection(this);
            items = new FluentListViewItemCollection(this);
            selectedItems = new List<FluentListViewItem>();

            columns.CollectionChanged += (s, e) => { UpdateLayout(); Invalidate(); };

            InitializeScrollBars();

            Size = new Size(400, 300);
            UpdateLayout();
        }

        private void InitializeScrollBars()
        {
            vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            vScrollBar.Scroll += (s, e) =>
            {
                scrollOffsetY = e.NewValue;
                Invalidate();
            };

            hScrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                Visible = false
            };
            hScrollBar.Scroll += (s, e) =>
            {
                scrollOffsetX = e.NewValue;
                Invalidate();
            };

            Controls.Add(vScrollBar);
            Controls.Add(hScrollBar);
        }

        #endregion

        #region 属性

        [Category("ListView")]
        [Description("视图模式")]
        [DefaultValue(FluentListViewMode.Details)]
        public FluentListViewMode View
        {
            get => viewMode;
            set
            {
                if (viewMode != value)
                {
                    viewMode = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("列集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentListViewColumnCollectionEditor), typeof(UITypeEditor))]
        public FluentListViewColumnCollection Columns => columns;

        [Category("ListView")]
        [Description("项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentListViewItemCollectionEditor), typeof(UITypeEditor))]
        public FluentListViewItemCollection Items => items;

        [Category("ListView")]
        [Description("大图标列表")]
        public ImageList LargeImageList
        {
            get => largeImageList;
            set
            {
                largeImageList = value;
                Invalidate();
            }
        }

        [Category("ListView")]
        [Description("小图标列表")]
        public ImageList SmallImageList
        {
            get => smallImageList;
            set
            {
                smallImageList = value;
                Invalidate();
            }
        }

        [Category("ListView")]
        [Description("是否显示列标题")]
        [DefaultValue(true)]
        public bool ShowColumnHeaders
        {
            get => showColumnHeaders;
            set
            {
                if (showColumnHeaders != value)
                {
                    showColumnHeaders = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("是否选中整行")]
        [DefaultValue(true)]
        public bool FullRowSelect
        {
            get => fullRowSelect;
            set
            {
                fullRowSelect = value;
                Invalidate();
            }
        }

        [Category("ListView")]
        [Description("是否显示网格线")]
        [DefaultValue(false)]
        public bool GridLines
        {
            get => gridLines;
            set
            {
                gridLines = value;
                Invalidate();
            }
        }

        [Category("ListView")]
        [Description("是否显示复选框")]
        [DefaultValue(false)]
        public bool CheckBoxes
        {
            get => checkBoxes;
            set
            {
                checkBoxes = value;
                UpdateLayout();
                Invalidate();
            }
        }

        [Category("ListView")]
        [Description("是否允许多选")]
        [DefaultValue(true)]
        public bool MultiSelect
        {
            get => multiSelect;
            set => multiSelect = value;
        }

        [Category("ListView")]
        [Description("悬停时选中")]
        [DefaultValue(false)]
        public bool HoverSelection
        {
            get => hoverSelection;
            set => hoverSelection = value;
        }

        [Category("ListView")]
        [Description("允许列重排序")]
        [DefaultValue(false)]
        public bool AllowColumnReorder
        {
            get => allowColumnReorder;
            set => allowColumnReorder = value;
        }

        [Category("ListView")]
        [Description("标题栏高度")]
        [DefaultValue(28)]
        public int HeaderHeight
        {
            get => headerHeight;
            set
            {
                if (headerHeight != value && value > 0)
                {
                    headerHeight = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("项高度")]
        [DefaultValue(24)]
        public int ItemHeight
        {
            get => itemHeight;
            set
            {
                if (itemHeight != value && value > 0)
                {
                    itemHeight = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("大图标尺寸")]
        [DefaultValue(48)]
        public int LargeIconSize
        {
            get => largeIconSize;
            set
            {
                if (largeIconSize != value && value > 0)
                {
                    largeIconSize = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("小图标尺寸")]
        [DefaultValue(16)]
        public int SmallIconSize
        {
            get => smallIconSize;
            set
            {
                if (smallIconSize != value && value > 0)
                {
                    smallIconSize = value;
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("项间距")]
        [DefaultValue(4)]
        public int ItemSpacing
        {
            get => itemSpacing;
            set
            {
                if (itemSpacing != value && value >= 0)
                {
                    itemSpacing = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("是否允许调整列宽")]
        [DefaultValue(true)]
        public bool AllowColumnResize
        {
            get => allowColumnResize;
            set
            {
                if (allowColumnResize != value)
                {
                    allowColumnResize = value;
                }
            }
        }

        [Category("ListView")]
        [Description("小图标模式项宽度")]
        [DefaultValue(100)]
        public int SmallIconItemWidth
        {
            get => smallIconItemWidth;
            set
            {
                if (smallIconItemWidth != value && value > 0)
                {
                    smallIconItemWidth = value;
                    if (viewMode == FluentListViewMode.SmallIcon)
                    {
                        UpdateLayout();
                        Invalidate();
                    }
                }
            }
        }

        [Category("ListView")]
        [Description("大图标模式项宽度")]
        [DefaultValue(120)]
        public int LargeIconItemWidth
        {
            get => largeIconItemWidth;
            set
            {
                if (largeIconItemWidth != value && value > 0)
                {
                    largeIconItemWidth = value;
                    if (viewMode == FluentListViewMode.LargeIcon)
                    {
                        UpdateLayout();
                        Invalidate();
                    }
                }
            }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<FluentListViewItem> SelectedItems => selectedItems;

        [Browsable(false)]
        public FluentListViewItem FocusedItem
        {
            get => focusedItem;
            set
            {
                if (focusedItem != value)
                {
                    if (focusedItem != null)
                    {
                        focusedItem.Focused = false;
                    }

                    focusedItem = value;

                    if (focusedItem != null)
                    {
                        focusedItem.Focused = true;
                    }

                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("排序列索引")]
        [DefaultValue(-1)]
        public int SortColumnIndex
        {
            get => sortColumnIndex;
            set
            {
                if (sortColumnIndex != value)
                {
                    sortColumnIndex = value;
                    PerformSort();
                }
            }
        }

        [Category("ListView")]
        [Description("排序方式")]
        [DefaultValue(ColumnSortOrder.None)]
        public ColumnSortOrder Sorting
        {
            get => sortOrder;
            set
            {
                if (sortOrder != value)
                {
                    sortOrder = value;
                    PerformSort();
                }
            }
        }

        [Category("ListView")]
        [Description("复选框样式")]
        [DefaultValue(ListViewCheckBoxStyle.Check)]
        public ListViewCheckBoxStyle CheckBoxStyle
        {
            get => checkBoxStyle;
            set
            {
                if (checkBoxStyle != value)
                {
                    checkBoxStyle = value;
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("内容边距")]
        public new Padding Padding
        {
            get => contentPadding;
            set
            {
                if (contentPadding != value)
                {
                    contentPadding = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("ListView")]
        [Description("Tile模式项高度")]
        [DefaultValue(64)]
        public int TileItemHeight
        {
            get => tileItemHeight;
            set
            {
                if (tileItemHeight != value && value > 0)
                {
                    tileItemHeight = value;
                    if (viewMode == FluentListViewMode.Tile)
                    {
                        UpdateLayout();
                        Invalidate();
                    }
                }
            }
        }

        [Category("Debug")]
        [Description("显示项边界框(调试用)")]
        [DefaultValue(false)]
        [Browsable(false)]
        public bool ShowItemBounds
        {
            get => showItemBounds;
            set
            {
                if (showItemBounds != value)
                {
                    showItemBounds = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public IComparer ItemSorter
        {
            get => itemComparer;
            set
            {
                itemComparer = value;
                PerformSort();
            }
        }

        #endregion

        #region 事件

        [Category("ListView")]
        public event EventHandler SelectedIndexChanged;

        [Category("ListView")]
        public event EventHandler<FluentListViewItemEventArgs> ItemSelectionChanged;

        [Category("ListView")]
        public event EventHandler<FluentListViewColumnClickEventArgs> ColumnClick;

        [Category("ListView")]
        public event EventHandler<FluentListViewItemEventArgs> ItemChecked;

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        protected virtual void OnItemSelectionChanged(FluentListViewItemEventArgs e)
        {
            ItemSelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnColumnClick(FluentListViewColumnClickEventArgs e)
        {
            ColumnClick?.Invoke(this, e);
        }

        protected virtual void OnItemChecked(FluentListViewItemEventArgs e)
        {
            ItemChecked?.Invoke(this, e);
        }

        #endregion

        #region 布局

        internal void UpdateLayout()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            int contentWidth = 0;
            int contentHeight = 0;

            switch (viewMode)
            {
                case FluentListViewMode.Details:
                    UpdateDetailsLayout(out contentWidth, out contentHeight);
                    break;

                case FluentListViewMode.List:
                    UpdateListLayout(out contentWidth, out contentHeight);
                    break;

                case FluentListViewMode.LargeIcon:
                case FluentListViewMode.SmallIcon:
                case FluentListViewMode.Tile:
                    UpdateIconLayout(out contentWidth, out contentHeight);
                    break;
            }

            UpdateScrollBars(contentWidth, contentHeight);
        }

        private void UpdateDetailsLayout(out int contentWidth, out int contentHeight)
        {
            int y = (showColumnHeaders ? headerHeight : 0) + contentPadding.Top;
            int startX = contentPadding.Left;

            // 第一列的起始位置需要考虑复选框
            if (checkBoxes)
            {
                startX += 20;
            }

            // 计算列位置 - 确保列头和数据行使用相同的起始位置
            int x = startX;
            foreach (var column in columns.Where(c => c.Visible))
            {
                column.Bounds = new Rectangle(x, 0, column.Width, headerHeight);
                x += column.Width;
            }

            contentWidth = x + contentPadding.Right;

            // 计算项位置
            foreach (var item in items)
            {
                item.Bounds = new Rectangle(contentPadding.Left, y, contentWidth - contentPadding.Horizontal, itemHeight);
                y += itemHeight;
            }

            contentHeight = y + contentPadding.Bottom;
        }

        private void UpdateListLayout(out int contentWidth, out int contentHeight)
        {
            int x = contentPadding.Left;
            int y = contentPadding.Top;
            int maxY = 0;
            int columnWidth = 200;
            int viewHeight = Height - (hScrollBar.Visible ? hScrollBar.Height : 0) - contentPadding.Vertical;

            foreach (var item in items)
            {
                if (y + itemHeight > viewHeight && y > contentPadding.Top)
                {
                    // 换列
                    x += columnWidth + itemSpacing;
                    y = contentPadding.Top;
                }

                item.Bounds = new Rectangle(x, y, columnWidth, itemHeight);
                y += itemHeight + itemSpacing;
                maxY = Math.Max(maxY, y);
            }

            contentWidth = x + columnWidth + contentPadding.Right;
            contentHeight = maxY + contentPadding.Bottom;
        }

        private void UpdateIconLayout(out int contentWidth, out int contentHeight)
        {
            int iconSize = viewMode == FluentListViewMode.LargeIcon ? largeIconSize : smallIconSize;
            int itemWidth;
            int itemH;

            // 根据视图模式确定项尺寸
            if (viewMode == FluentListViewMode.Tile)
            {
                itemWidth = 250;
                itemH = tileItemHeight;
            }
            else if (viewMode == FluentListViewMode.LargeIcon)
            {
                itemWidth = largeIconItemWidth; // 使用配置的宽度

                // 计算高度：图标 + 间距 + 文字
                using (var g = CreateGraphics())
                {
                    var textHeight = (int)Math.Ceiling(Font.GetHeight(g));
                    itemH = iconSize + 8 + textHeight + 8;
                }
            }
            else // SmallIcon
            {
                itemWidth = smallIconItemWidth; // 使用配置的宽度

                // 计算高度：图标 + 间距 + 文字
                using (var g = CreateGraphics())
                {
                    var textHeight = (int)Math.Ceiling(Font.GetHeight(g));
                    itemH = iconSize + 8 + textHeight + 8;
                }
            }

            int x = itemSpacing + contentPadding.Left;
            int y = itemSpacing + contentPadding.Top;
            int maxX = 0;
            int viewWidth = Width - (vScrollBar.Visible ? vScrollBar.Width : 0) - contentPadding.Horizontal;

            foreach (var item in items)
            {
                if (x + itemWidth > viewWidth && x > itemSpacing + contentPadding.Left)
                {
                    // 换行
                    x = itemSpacing + contentPadding.Left;
                    y += itemH + itemSpacing;
                }

                item.Bounds = new Rectangle(x, y, itemWidth, itemH);
                x += itemWidth + itemSpacing;
                maxX = Math.Max(maxX, x);
            }

            contentWidth = maxX + contentPadding.Right;
            contentHeight = y + itemH + itemSpacing + contentPadding.Bottom;
        }

        private void UpdateScrollBars(int contentWidth, int contentHeight)
        {
            int clientWidth = Width;
            int clientHeight = Height;

            // 垂直滚动条
            bool needVScroll = contentHeight > clientHeight;
            if (needVScroll)
            {
                clientWidth -= vScrollBar.Width;
                vScrollBar.Maximum = contentHeight;
                vScrollBar.LargeChange = clientHeight;
                vScrollBar.SmallChange = itemHeight;
                vScrollBar.Visible = true;
            }
            else
            {
                vScrollBar.Visible = false;
                scrollOffsetY = 0;
            }

            // 水平滚动条
            bool needHScroll = contentWidth > clientWidth;
            if (needHScroll)
            {
                clientHeight -= hScrollBar.Height;
                hScrollBar.Maximum = contentWidth;
                hScrollBar.LargeChange = clientWidth;
                hScrollBar.SmallChange = 20;
                hScrollBar.Visible = true;
            }
            else
            {
                hScrollBar.Visible = false;
                scrollOffsetX = 0;
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var bgColor = GetThemeColor(c => c.Surface, SystemColors.Window);
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            g.TranslateTransform(-scrollOffsetX, -scrollOffsetY);

            switch (viewMode)
            {
                case FluentListViewMode.Details:
                    DrawDetailsView(g);
                    break;

                case FluentListViewMode.List:
                    DrawListView(g);
                    break;

                case FluentListViewMode.LargeIcon:
                case FluentListViewMode.SmallIcon:
                case FluentListViewMode.Tile:
                    DrawIconView(g);
                    break;
            }

            g.ResetTransform();
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawDetailsView(Graphics g)
        {
            // 绘制列标题
            if (showColumnHeaders)
            {
                DrawColumnHeaders(g);
            }

            // 绘制网格线
            if (gridLines)
            {
                DrawGridLines(g);
            }

            // 绘制项
            int startY = showColumnHeaders ? headerHeight : 0;
            var viewRect = new Rectangle(scrollOffsetX, scrollOffsetY,
                Width - (vScrollBar.Visible ? vScrollBar.Width : 0),
                Height - (hScrollBar.Visible ? hScrollBar.Height : 0));

            foreach (var item in items)
            {
                if (!item.Bounds.IntersectsWith(viewRect))
                {
                    continue;
                }

                DrawDetailsItem(g, item);
            }
        }

        private void DrawColumnHeaders(Graphics g)
        {
            var headerBgColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.Control);
            var headerTextColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
            var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);

            using (var bgBrush = new SolidBrush(headerBgColor))
            using (var textBrush = new SolidBrush(headerTextColor))
            using (var borderPen = new Pen(borderColor, 1))
            {
                // 先绘制整个标题栏背景
                var headerRect = new Rectangle(0, 0, Width, headerHeight);
                g.FillRectangle(bgBrush, headerRect);

                // 绘制底部分隔线
                g.DrawLine(borderPen, 0, headerHeight - 1, Width, headerHeight - 1);

                foreach (var column in columns.Where(c => c.Visible))
                {
                    var bounds = column.Bounds;
                    bool isHovered = column == hoveredColumn;
                    bool hasSortIndicator = column.SortOrder != ColumnSortOrder.None;

                    // 绘制悬停背景
                    if (isHovered)
                    {
                        var hoverColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
                        using (var hoverBrush = new SolidBrush(hoverColor))
                        {
                            g.FillRectangle(hoverBrush, bounds);
                        }
                    }

                    // 计算排序箭头占用的空间
                    int sortIndicatorWidth = hasSortIndicator ? 20 : 0;
                    int textPadding = 4;

                    // 根据对齐方式和排序指示器调整文本区域
                    Rectangle textRect;

                    if (column.TextAlign == HorizontalAlignment.Right)
                    {
                        if (hasSortIndicator)
                        {
                            textRect = new Rectangle(
                                bounds.X + textPadding,
                                bounds.Y,
                                bounds.Width - sortIndicatorWidth - textPadding * 2,
                                bounds.Height);
                        }
                        else
                        {
                            textRect = new Rectangle(
                                bounds.X + textPadding,
                                bounds.Y,
                                bounds.Width - textPadding * 2,
                                bounds.Height);
                        }
                    }
                    else if (column.TextAlign == HorizontalAlignment.Center)
                    {
                        if (hasSortIndicator)
                        {
                            textRect = new Rectangle(
                                bounds.X + textPadding,
                                bounds.Y,
                                bounds.Width - sortIndicatorWidth - textPadding * 2,
                                bounds.Height);
                        }
                        else
                        {
                            textRect = new Rectangle(
                                bounds.X + textPadding,
                                bounds.Y,
                                bounds.Width - textPadding * 2,
                                bounds.Height);
                        }
                    }
                    else // Left
                    {
                        if (hasSortIndicator)
                        {
                            textRect = new Rectangle(
                                bounds.X + textPadding,
                                bounds.Y,
                                bounds.Width - sortIndicatorWidth - textPadding * 2,
                                bounds.Height);
                        }
                        else
                        {
                            textRect = new Rectangle(
                                bounds.X + textPadding,
                                bounds.Y,
                                bounds.Width - textPadding * 2,
                                bounds.Height);
                        }
                    }

                    // 绘制文本
                    var format = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = column.TextAlign == HorizontalAlignment.Center ? StringAlignment.Center :
                                   column.TextAlign == HorizontalAlignment.Right ? StringAlignment.Far :
                                   StringAlignment.Near,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };

                    g.DrawString(column.Text, Font, textBrush, textRect, format);

                    // 绘制排序指示器
                    if (hasSortIndicator)
                    {
                        DrawSortIndicator(g, bounds, column.SortOrder);
                    }
                }

                // 最后绘制垂直分隔线(与网格线对齐)
                if (gridLines)
                {
                    int x = contentPadding.Left;
                    if (checkBoxes)
                    {
                        x += 20;
                    }

                    foreach (var column in columns.Where(c => c.Visible))
                    {
                        x += column.Width;
                        // 绘制列分隔线，与网格线位置完全一致
                        g.DrawLine(borderPen, x, 0, x, headerHeight);
                    }
                }
            }
        }

        private void DrawSortIndicator(Graphics g, Rectangle columnBounds, ColumnSortOrder order)
        {
            var color = GetThemeColor(c => c.Accent, SystemColors.Highlight);
            var shadowColor = Color.FromArgb(50, color);

            // 箭头尺寸
            int arrowWidth = 7;
            int arrowHeight = 4;

            // 固定在列的最右侧
            int rightPadding = 8;
            int x = columnBounds.Right - rightPadding - arrowWidth;
            int y = columnBounds.Y + (columnBounds.Height - arrowHeight) / 2;

            Point[] points;

            if (order == ColumnSortOrder.Ascending)
            {
                // 向上箭头 ▲
                points = new Point[]
                {
                    new Point(x + arrowWidth / 2, y),
                    new Point(x, y + arrowHeight),
                    new Point(x + arrowWidth, y + arrowHeight)
                };
            }
            else // Descending
            {
                // 向下箭头 ▼
                points = new Point[]
                {
                    new Point(x, y),
                    new Point(x + arrowWidth, y),
                    new Point(x + arrowWidth / 2, y + arrowHeight)
                };
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制阴影(可选，增加立体感)
            var shadowPoints = points.Select(p => new Point(p.X + 1, p.Y + 1)).ToArray();
            using (var shadowBrush = new SolidBrush(shadowColor))
            {
                g.FillPolygon(shadowBrush, shadowPoints);
            }

            // 绘制主箭头
            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }

            g.SmoothingMode = SmoothingMode.Default;
        }

        private void DrawGridLines(Graphics g)
        {
            var gridColor = GetThemeColor(c => c.BorderLight, SystemColors.ControlLight);
            using (var pen = new Pen(gridColor))
            {
                int y = showColumnHeaders ? headerHeight : 0;

                // 水平线
                foreach (var item in items)
                {
                    y += itemHeight;
                    g.DrawLine(pen, 0, y, columns.Sum(c => c.Visible ? c.Width : 0), y);
                }

                // 垂直线
                int x = 0;
                foreach (var column in columns.Where(c => c.Visible))
                {
                    x += column.Width;
                    g.DrawLine(pen, x, 0, x, items.Count * itemHeight + (showColumnHeaders ? headerHeight : 0));
                }
            }
        }

        private void DrawDetailsItem(Graphics g, FluentListViewItem item)
        {
            var bounds = item.Bounds;
            bool isSelected = item.Selected;
            bool isHovered = item.Hovered;

            // 先绘制项的自定义背景色(如果有的话)
            if (!item.BackColor.IsEmpty && item.BackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(item.BackColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // 然后在上面绘制选中/悬停效果(半透明)
            if (isSelected || isHovered)
            {
                Color overlayColor;
                if (isSelected)
                {
                    overlayColor = Color.FromArgb(100, GetThemeColor(c => c.Accent, SystemColors.Highlight));
                }
                else // isHovered
                {
                    overlayColor = Color.FromArgb(50, GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight));
                }

                using (var brush = new SolidBrush(overlayColor))
                using (var path = GetRoundedRectangle(
                    new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4), 4))
                {
                    g.FillPath(brush, path);
                }
            }

            // 绘制选中指示器(修正：去掉错误的 displayMode 变量)
            if (isSelected)
            {
                var accentColor = GetThemeColor(c => c.Accent, SystemColors.Highlight);
                using (var brush = new SolidBrush(accentColor))
                {
                    // 选中指示器始终在左侧
                    g.FillRectangle(brush, bounds.X, bounds.Y + 8, 3, bounds.Height - 16);
                }
            }

            int x = bounds.X + contentPadding.Left;

            // 绘制复选框
            if (checkBoxes)
            {
                DrawCheckBox(g, new Rectangle(x + 2, bounds.Y + (bounds.Height - 16) / 2, 16, 16), item.Checked);
                x += 20;
            }

            // 绘制各列
            for (int i = 0; i < columns.Count && i < item.SubItems.Count; i++)
            {
                if (!columns[i].Visible)
                {
                    continue;
                }

                var column = columns[i];
                var subItem = item.SubItems[i];

                // 修正：使用 column.Bounds.X 而不是额外偏移
                var cellBounds = new Rectangle(
                    column.Bounds.X,
                    bounds.Y,
                    column.Width,
                    bounds.Height);

                // 第一列需要考虑复选框和图标的偏移
                if (i == 0)
                {
                    int offset = contentPadding.Left;
                    if (checkBoxes)
                    {
                        offset += 20;
                    }

                    // 绘制图标
                    var icon = GetItemIcon(item);
                    if (icon != null)
                    {
                        var iconRect = new Rectangle(
                            cellBounds.X + offset + 2,
                            cellBounds.Y + (cellBounds.Height - smallIconSize) / 2,
                            smallIconSize, smallIconSize);
                        g.DrawImage(icon, iconRect);
                        offset += smallIconSize + 4;
                    }

                    cellBounds.X += offset;
                    cellBounds.Width -= offset;
                }

                // 使用子项的前景色(如果有)，否则使用项的前景色，最后使用默认色
                Color textColor;
                if (!subItem.ForeColor.IsEmpty)
                {
                    textColor = subItem.ForeColor;
                }
                else if (!item.ForeColor.IsEmpty)
                {
                    textColor = item.ForeColor;
                }
                else
                {
                    textColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                }

                var textFont = subItem.Font ?? item.Font ?? Font;

                using (var brush = new SolidBrush(textColor))
                {
                    var format = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = column.TextAlign == HorizontalAlignment.Center ? StringAlignment.Center :
                                   column.TextAlign == HorizontalAlignment.Right ? StringAlignment.Far :
                                   StringAlignment.Near,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };

                    var textBounds = cellBounds;
                    textBounds.Inflate(-4, 0);
                    g.DrawString(subItem.Text, textFont, brush, textBounds, format);
                }
            }

            // 绘制焦点框
            if (item.Focused && Focused)
            {
                ControlPaint.DrawFocusRectangle(g, bounds);
            }
        }

        private void DrawListView(Graphics g)
        {
            foreach (var item in items)
            {
                DrawListItem(g, item);
            }
        }

        private void DrawListItem(Graphics g, FluentListViewItem item)
        {
            var bounds = item.Bounds;
            bool isSelected = item.Selected;
            bool isHovered = item.Hovered;

            // 先绘制项的自定义背景色
            if (!item.BackColor.IsEmpty && item.BackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(item.BackColor))
                using (var path = GetRoundedRectangle(bounds, 4))
                {
                    g.FillPath(brush, path);
                }
            }

            // 然后绘制选中/悬停效果(半透明覆盖)
            if (isSelected || isHovered)
            {
                Color overlayColor;
                if (isSelected)
                {
                    overlayColor = Color.FromArgb(100, GetThemeColor(c => c.Accent, SystemColors.Highlight));
                }
                else
                {
                    overlayColor = Color.FromArgb(50, GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight));
                }

                using (var brush = new SolidBrush(overlayColor))
                using (var path = GetRoundedRectangle(bounds, 4))
                {
                    g.FillPath(brush, path);
                }
            }

            int x = bounds.X + contentPadding.Left;

            // 绘制复选框
            if (checkBoxes)
            {
                DrawCheckBox(g, new Rectangle(x, bounds.Y + (bounds.Height - 16) / 2, 16, 16), item.Checked);
                x += 20;
            }

            // 绘制图标
            var icon = GetItemIcon(item);
            if (icon != null)
            {
                var iconRect = new Rectangle(x, bounds.Y + (bounds.Height - smallIconSize) / 2, smallIconSize, smallIconSize);
                g.DrawImage(icon, iconRect);
                x += smallIconSize + 4;
            }

            // 使用项的前景色
            Color textColor = !item.ForeColor.IsEmpty ? item.ForeColor :
                             GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

            using (var brush = new SolidBrush(textColor))
            {
                var textRect = new Rectangle(x, bounds.Y, bounds.Right - x - 4, bounds.Height);
                var format = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(item.Text, item.Font ?? Font, brush, textRect, format);
            }

            // 绘制焦点框
            if (item.Focused && Focused)
            {
                ControlPaint.DrawFocusRectangle(g, bounds);
            }
        }

        private void DrawIconView(Graphics g)
        {
            int iconSize = viewMode == FluentListViewMode.LargeIcon ? largeIconSize : smallIconSize;

            foreach (var item in items)
            {
                DrawIconItem(g, item, iconSize);
            }
        }

        private void DrawIconItem(Graphics g, FluentListViewItem item, int iconSize)
        {
            var bounds = item.Bounds;
            bool isSelected = item.Selected;
            bool isHovered = item.Hovered;

            // 先绘制项的自定义背景色
            if (!item.BackColor.IsEmpty && item.BackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(item.BackColor))
                using (var path = GetRoundedRectangle(bounds, 4))
                {
                    g.FillPath(brush, path);
                }
            }

            // 然后绘制选中/悬停效果(覆盖整个项区域)
            if (isSelected || isHovered)
            {
                Color overlayColor;
                if (isSelected)
                {
                    overlayColor = Color.FromArgb(100, GetThemeColor(c => c.Accent, SystemColors.Highlight));
                }
                else
                {
                    overlayColor = Color.FromArgb(50, GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight));
                }

                // 使用整个 bounds 绘制选中区域
                using (var brush = new SolidBrush(overlayColor))
                using (var path = GetRoundedRectangle(bounds, 4))
                {
                    g.FillPath(brush, path);
                }
            }

            if (viewMode == FluentListViewMode.Tile)
            {
                DrawTileItem(g, item, bounds);
            }
            else
            {
                // 计算图标和文本的位置
                int iconX = bounds.X + (bounds.Width - iconSize) / 2;
                int iconY = bounds.Y + 4;

                // 绘制图标
                var icon = GetItemIcon(item, viewMode == FluentListViewMode.LargeIcon);
                if (icon != null)
                {
                    var iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);

                    try
                    {
                        g.DrawImage(icon, iconRect);
                    }
                    catch
                    {
                        // 如果图标绘制失败，绘制占位符
                        using (var pen = new Pen(Color.Gray, 1))
                        {
                            g.DrawRectangle(pen, iconRect);
                        }
                    }
                }
                else
                {
                    // 没有图标时绘制占位符(可选)
                    if (viewMode == FluentListViewMode.LargeIcon)
                    {
                        var iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);
                        using (var pen = new Pen(Color.LightGray, 1))
                        {
                            g.DrawRectangle(pen, iconRect);
                        }
                    }
                }

                // 绘制文本(在图标下方)
                Color textColor = !item.ForeColor.IsEmpty ? item.ForeColor :
                                 GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

                using (var brush = new SolidBrush(textColor))
                {
                    // 文本区域在图标下方
                    int textY = iconY + iconSize + 4;
                    int textHeight = bounds.Bottom - textY;

                    var textRect = new Rectangle(
                        bounds.X + 2,
                        textY,
                        bounds.Width - 4,
                        textHeight);

                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };

                    g.DrawString(item.Text, item.Font ?? Font, brush, textRect, format);
                }
            }

            // 绘制焦点框
            if (item.Focused && Focused)
            {
                // 焦点框绘制在整个项区域
                var focusRect = bounds;
                focusRect.Inflate(-1, -1);
                ControlPaint.DrawFocusRectangle(g, focusRect);
            }

            // 调试：绘制项边界
            if (showItemBounds)
            {
                using (var pen = new Pen(Color.Red, 1))
                {
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawRectangle(pen, bounds);
                }
            }
        }

        private void DrawTileItem(Graphics g, FluentListViewItem item, Rectangle bounds)
        {
            int padding = 4;
            int x = bounds.X + padding;
            int y = bounds.Y + padding;
            int iconSize = smallIconSize * 2;

            // 绘制图标
            var icon = GetItemIcon(item);
            if (icon != null)
            {
                var iconRect = new Rectangle(x, y, iconSize, iconSize);
                g.DrawImage(icon, iconRect);
                x += iconSize + 8;
            }

            // 计算文本区域
            int textAreaWidth = bounds.Right - x - padding;
            int textY = y;

            // 使用项的前景色
            Color textColor = !item.ForeColor.IsEmpty ? item.ForeColor :
                             GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

            var titleFont = item.Font ?? Font;
            if (titleFont.Style == FontStyle.Regular)
            {
                titleFont = new Font(titleFont, FontStyle.Bold);
            }

            using (var brush = new SolidBrush(textColor))
            {
                var titleRect = new Rectangle(x, textY, textAreaWidth, (int)titleFont.GetHeight(g) + 2);
                var format = new StringFormat
                {
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                g.DrawString(item.Text, titleFont, brush, titleRect, format);
                textY += (int)titleFont.GetHeight(g) + 4;
            }

            // 绘制子项
            var subTextColor = GetThemeColor(c => c.TextSecondary, SystemColors.GrayText);
            using (var subBrush = new SolidBrush(subTextColor))
            {
                var subFont = Font;
                var lineHeight = (int)subFont.GetHeight(g);

                // 计算可以显示的子项数量
                int availableHeight = bounds.Bottom - textY - padding;
                int maxSubItems = Math.Max(1, availableHeight / (lineHeight + 2));

                for (int i = 1; i < Math.Min(item.SubItems.Count, maxSubItems + 1); i++)
                {
                    if (textY + lineHeight > bounds.Bottom - padding)
                    {
                        break;
                    }

                    var subTextRect = new Rectangle(x, textY, textAreaWidth, lineHeight);
                    var format = new StringFormat
                    {
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };

                    g.DrawString(item.SubItems[i].Text, subFont, subBrush, subTextRect, format);
                    textY += lineHeight + 2;
                }
            }
        }

        private void DrawCheckBox(Graphics g, Rectangle bounds, bool isChecked)
        {
            var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            var checkColor = GetThemeColor(c => c.Accent, SystemColors.Highlight);
            var bgColor = GetThemeColor(c => c.Surface, Color.White);

            using (var borderPen = new Pen(borderColor, 1))
            using (var bgBrush = new SolidBrush(bgColor))
            {
                switch (checkBoxStyle)
                {
                    case ListViewCheckBoxStyle.FilledSquare:
                        // 方形样式
                        g.FillRectangle(bgBrush, bounds);
                        g.DrawRectangle(borderPen, bounds);

                        if (isChecked)
                        {
                            using (var checkBrush = new SolidBrush(checkColor))
                            {
                                var checkBounds = bounds;
                                checkBounds.Inflate(-3, -3);
                                g.FillRectangle(checkBrush, checkBounds);
                            }
                        }
                        break;

                    case ListViewCheckBoxStyle.Check:
                        // 对勾样式
                        g.FillRectangle(bgBrush, bounds);
                        g.DrawRectangle(borderPen, bounds);

                        if (isChecked)
                        {
                            using (var checkPen = new Pen(checkColor, 2))
                            {
                                checkPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                                checkPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                                // 绘制对勾
                                var points = new PointF[]
                                {
                            new PointF(bounds.X + 3, bounds.Y + bounds.Height / 2),
                            new PointF(bounds.X + bounds.Width / 2 - 1, bounds.Y + bounds.Height - 4),
                            new PointF(bounds.X + bounds.Width - 3, bounds.Y + 3)
                                };

                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.DrawLines(checkPen, points);
                                g.SmoothingMode = SmoothingMode.Default;
                            }
                        }
                        break;

                    case ListViewCheckBoxStyle.CheckCircle:
                        // 圆形对勾样式
                        var circleBounds = bounds;
                        circleBounds.Inflate(-1, -1);

                        g.SmoothingMode = SmoothingMode.AntiAlias;

                        // 绘制圆形背景
                        if (isChecked)
                        {
                            using (var checkBrush = new SolidBrush(checkColor))
                            {
                                g.FillEllipse(checkBrush, circleBounds);
                            }
                        }
                        else
                        {
                            g.FillEllipse(bgBrush, circleBounds);
                            using (var circlePen = new Pen(borderColor, 1))
                            {
                                g.DrawEllipse(circlePen, circleBounds);
                            }
                        }

                        if (isChecked)
                        {
                            // 绘制白色对勾
                            using (var checkPen = new Pen(Color.White, 2))
                            {
                                checkPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                                checkPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                                var points = new PointF[]
                                {
                            new PointF(bounds.X + 3, bounds.Y + bounds.Height / 2),
                            new PointF(bounds.X + bounds.Width / 2 - 1, bounds.Y + bounds.Height - 4),
                            new PointF(bounds.X + bounds.Width - 3, bounds.Y + 3)
                                };

                                g.DrawLines(checkPen, points);
                            }
                        }

                        g.SmoothingMode = SmoothingMode.Default;
                        break;
                }
            }
        }


        private Image GetItemIcon(FluentListViewItem item, bool useLarge = false)
        {
            if (item.Icon != null)
            {
                return item.Icon;
            }

            var imageList = useLarge ? largeImageList : smallImageList;
            if (imageList == null)
            {
                return null;
            }

            if (item.ImageIndex >= 0 && item.ImageIndex < imageList.Images.Count)
            {
                return imageList.Images[item.ImageIndex];
            }

            if (!string.IsNullOrEmpty(item.ImageKey) && imageList.Images.ContainsKey(item.ImageKey))
            {
                return imageList.Images[item.ImageKey];
            }

            return null;
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var adjustedPoint = new Point(e.X + scrollOffsetX, e.Y + scrollOffsetY);

            // 如果正在调整列宽
            if (isResizingColumn && resizingColumn != null)
            {
                int delta = e.X - resizeStartX;
                int newWidth = Math.Max(20, resizeStartWidth + delta); // 最小宽度20
                resizingColumn.Width = newWidth;
                UpdateLayout();
                Invalidate();
                return;
            }

            // 检查是否在列边界上(可以调整列宽)
            if (viewMode == FluentListViewMode.Details && showColumnHeaders &&
                allowColumnResize && e.Y < headerHeight)
            {
                bool onResizeBorder = false;

                foreach (var column in columns.Where(c => c.Visible))
                {
                    int rightEdge = column.Bounds.Right - scrollOffsetX;

                    // 检查是否在列右边界的热区内
                    if (Math.Abs(e.X - rightEdge) <= ResizeHitArea)
                    {
                        Cursor = Cursors.VSplit;
                        onResizeBorder = true;
                        break;
                    }
                }

                if (!onResizeBorder)
                {
                    Cursor = Cursors.Default;
                }
            }

            // 检查列标题悬停
            FluentListViewColumn newHoveredColumn = null;
            if (viewMode == FluentListViewMode.Details && showColumnHeaders && e.Y < headerHeight)
            {
                foreach (var column in columns.Where(c => c.Visible))
                {
                    if (column.Bounds.Contains(adjustedPoint))
                    {
                        newHoveredColumn = column;
                        break;
                    }
                }
            }

            if (newHoveredColumn != hoveredColumn)
            {
                hoveredColumn = newHoveredColumn;
                Invalidate();
            }

            // 检查项悬停
            FluentListViewItem newHoveredItem = null;
            foreach (var item in items)
            {
                if (item.Bounds.IsEmpty)
                {
                    continue;
                }

                if (item.Bounds.Contains(adjustedPoint))
                {
                    newHoveredItem = item;
                    break;
                }
            }

            if (newHoveredItem != hoveredItem)
            {
                if (hoveredItem != null)
                {
                    hoveredItem.Hovered = false;
                }

                hoveredItem = newHoveredItem;

                if (hoveredItem != null)
                {
                    hoveredItem.Hovered = true;

                    if (hoverSelection && !hoveredItem.Selected)
                    {
                        SelectItem(hoveredItem, false);
                    }
                }

                Invalidate();
            }

            // 设置光标
            if (!isResizingColumn && Cursor != Cursors.VSplit)
            {
                bool overInteractive = newHoveredItem != null || newHoveredColumn != null;
                Cursor = overInteractive ? Cursors.Hand : Cursors.Default;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredItem != null)
            {
                hoveredItem.Hovered = false;
                hoveredItem = null;
                Invalidate();
            }

            if (hoveredColumn != null)
            {
                hoveredColumn = null;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouseDownPoint = e.Location;

            Focus();

            // 检查是否在列边界上准备调整列宽
            if (e.Button == MouseButtons.Left && viewMode == FluentListViewMode.Details &&
                showColumnHeaders && allowColumnResize && e.Y < headerHeight)
            {
                foreach (var column in columns.Where(c => c.Visible))
                {
                    int rightEdge = column.Bounds.Right - scrollOffsetX;

                    if (Math.Abs(e.X - rightEdge) <= ResizeHitArea)
                    {
                        isResizingColumn = true;
                        resizingColumn = column;
                        resizeStartX = e.X;
                        resizeStartWidth = column.Width;
                        Cursor = Cursors.VSplit;
                        return;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // 结束列宽调整
            if (isResizingColumn)
            {
                isResizingColumn = false;
                resizingColumn = null;
                Cursor = Cursors.Default;
                return;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 如果正在或刚完成列宽调整，不处理点击
            if (isResizingColumn)
            {
                return;
            }

            var adjustedPoint = new Point(e.X + scrollOffsetX, e.Y + scrollOffsetY);

            // 点击列标题
            if (viewMode == FluentListViewMode.Details && showColumnHeaders && e.Y < headerHeight)
            {
                foreach (var column in columns.Where(c => c.Visible))
                {
                    if (column.Bounds.Contains(adjustedPoint))
                    {
                        HandleColumnClick(column);
                        return;
                    }
                }
            }

            // 点击项
            foreach (var item in items)
            {
                if (item.Bounds.Contains(adjustedPoint))
                {
                    // 检查是否点击复选框
                    if (checkBoxes)
                    {
                        var checkBounds = new Rectangle(item.Bounds.X + 2, item.Bounds.Y + (item.Bounds.Height - 16) / 2, 16, 16);
                        if (checkBounds.Contains(adjustedPoint))
                        {
                            item.Checked = !item.Checked;
                            OnItemChecked(new FluentListViewItemEventArgs(item));
                            Invalidate();
                            return;
                        }
                    }

                    bool ctrlPressed = ModifierKeys.HasFlag(Keys.Control);
                    bool shiftPressed = ModifierKeys.HasFlag(Keys.Shift);

                    SelectItem(item, ctrlPressed || !multiSelect);
                    FocusedItem = item;
                    Invalidate();
                    return;
                }
            }

            // 点击空白处清除选择
            if (!ModifierKeys.HasFlag(Keys.Control))
            {
                ClearSelection();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (hoveredItem != null)
            {
                // 可以触发自定义的双击事件
            }
        }

        private void HandleColumnClick(FluentListViewColumn column)
        {
            // 切换排序
            if (column.SortOrder == ColumnSortOrder.None || column.SortOrder == ColumnSortOrder.Descending)
            {
                // 清除其他列的排序
                foreach (var col in columns)
                {
                    col.SortOrder = ColumnSortOrder.None;
                }

                column.SortOrder = ColumnSortOrder.Ascending;
            }
            else
            {
                column.SortOrder = ColumnSortOrder.Descending;
            }

            sortColumnIndex = column.Index;
            sortOrder = column.SortOrder;

            PerformSort();
            OnColumnClick(new FluentListViewColumnClickEventArgs(column, column.Index));
            Invalidate();
        }

        #endregion

        #region 选择管理

        private void SelectItem(FluentListViewItem item, bool toggle)
        {
            if (!multiSelect)
            {
                ClearSelection();
                item.Selected = true;
                selectedItems.Add(item);
            }
            else if (toggle)
            {
                item.Selected = !item.Selected;
                if (item.Selected)
                {
                    if (!selectedItems.Contains(item))
                    {
                        selectedItems.Add(item);
                    }
                }
                else
                {
                    selectedItems.Remove(item);
                }
            }
            else
            {
                ClearSelection();
                item.Selected = true;
                selectedItems.Add(item);
            }

            OnItemSelectionChanged(new FluentListViewItemEventArgs(item));
            OnSelectedIndexChanged(EventArgs.Empty);
        }

        private void ClearSelection()
        {
            foreach (var item in selectedItems)
            {
                item.Selected = false;
            }
            selectedItems.Clear();
            OnSelectedIndexChanged(EventArgs.Empty);
        }

        #endregion

        #region 排序

        private void PerformSort()
        {
            if (sortOrder == ColumnSortOrder.None || items.Count == 0)
            {
                return;
            }

            var itemList = items.ToList();

            if (itemComparer != null)
            {
                // 使用自定义比较器
                var adapter = new ComparerAdapter<FluentListViewItem>(itemComparer);
                itemList.Sort(adapter);

                if (sortOrder == ColumnSortOrder.Descending)
                {
                    itemList.Reverse();
                }
            }
            else if (sortColumnIndex >= 0 && sortColumnIndex < columns.Count)
            {
                // 默认字符串排序
                itemList.Sort((a, b) =>
                {
                    string textA = sortColumnIndex < a.SubItems.Count ? a.SubItems[sortColumnIndex].Text : "";
                    string textB = sortColumnIndex < b.SubItems.Count ? b.SubItems[sortColumnIndex].Text : "";

                    int result = string.Compare(textA, textB, StringComparison.CurrentCultureIgnoreCase);

                    return sortOrder == ColumnSortOrder.Ascending ? result : -result;
                });
            }

            // 重新填充集合
            items.Clear();
            foreach (var item in itemList)
            {
                items.Add(item);
            }

            UpdateLayout();
            Invalidate();
        }

        public void Sort()
        {
            PerformSort();
        }

        #endregion

        #region 键盘事件

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (items.Count == 0)
            {
                return;
            }

            int currentIndex = focusedItem?.Index ?? -1;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (currentIndex > 0)
                    {
                        var item = items[currentIndex - 1];
                        SelectItem(item, false);
                        FocusedItem = item;
                        EnsureVisible(item);
                    }
                    e.Handled = true;
                    break;

                case Keys.Down:
                    if (currentIndex < items.Count - 1)
                    {
                        var item = items[currentIndex + 1];
                        SelectItem(item, false);
                        FocusedItem = item;
                        EnsureVisible(item);
                    }
                    e.Handled = true;
                    break;

                case Keys.Space:
                    if (focusedItem != null && checkBoxes)
                    {
                        focusedItem.Checked = !focusedItem.Checked;
                        OnItemChecked(new FluentListViewItemEventArgs(focusedItem));
                        Invalidate();
                    }
                    e.Handled = true;
                    break;

                case Keys.A:
                    if (e.Control && multiSelect)
                    {
                        foreach (var item in items)
                        {
                            item.Selected = true;
                            if (!selectedItems.Contains(item))
                            {
                                selectedItems.Add(item);
                            }
                        }
                        OnSelectedIndexChanged(EventArgs.Empty);
                        Invalidate();
                    }
                    break;
            }
        }

        private void EnsureVisible(FluentListViewItem item)
        {
            if (item.Bounds.Top < scrollOffsetY)
            {
                scrollOffsetY = item.Bounds.Top;
                vScrollBar.Value = scrollOffsetY;
                Invalidate();
            }
            else if (item.Bounds.Bottom > scrollOffsetY + Height - (hScrollBar.Visible ? hScrollBar.Height : 0))
            {
                scrollOffsetY = item.Bounds.Bottom - Height + (hScrollBar.Visible ? hScrollBar.Height : 0);
                vScrollBar.Value = Math.Min(scrollOffsetY, vScrollBar.Maximum - vScrollBar.LargeChange);
                Invalidate();
            }
        }

        #endregion

        #region 主题应用

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                BackColor = GetThemeColor(c => c.Surface, SystemColors.Window);
                ForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
            }

            Invalidate();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 确保指定索引的项可见
        /// </summary>
        public void EnsureVisible(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                EnsureVisible(items[index]);
            }
        }

        /// <summary>
        /// 清除所有选择
        /// </summary>
        public void ClearSelected()
        {
            ClearSelection();
            Invalidate();
        }

        #endregion
    }

    #region 列和项类

    /// <summary>
    /// 列定义
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ToolboxItem(false)]
    public class FluentListViewColumn
    {
        private string text = "Column";
        private int width = 100;
        private HorizontalAlignment textAlign = HorizontalAlignment.Left;
        private bool visible = true;
        private int displayIndex = -1;

        [Category("Appearance")]
        [Description("列标题文本")]
        [DefaultValue("Column")]
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("列宽度")]
        [DefaultValue(100)]
        public int Width
        {
            get => width;
            set
            {
                if (value >= 0)
                {
                    width = value;
                    OnChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("文本对齐方式")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get => textAlign;
            set
            {
                textAlign = value;
                OnChanged();
            }
        }

        [Category("Behavior")]
        [Description("是否可见")]
        [DefaultValue(true)]
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                OnChanged();
            }
        }

        [Category("Behavior")]
        [Description("显示顺序索引")]
        [DefaultValue(-1)]
        public int DisplayIndex
        {
            get => displayIndex;
            set
            {
                displayIndex = value;
                OnChanged();
            }
        }

        [Category("Data")]
        [Description("用户数据")]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public object Tag { get; set; }

        [Category("Data")]
        [Description("Tag字符串值(设计时辅助)")]
        [DefaultValue("")]
        public string TagString
        {
            get => Tag?.ToString() ?? "";
            set => Tag = value;
        }

        [Browsable(false)]
        public int Index { get; internal set; }

        [Browsable(false)]
        internal Rectangle Bounds { get; set; }

        [Browsable(false)]
        internal ColumnSortOrder SortOrder { get; set; } = ColumnSortOrder.None;

        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return text;
        }
    }

    /// <summary>
    /// 列集合
    /// </summary>
    [Editor(typeof(FluentListViewColumnCollectionEditor), typeof(UITypeEditor))]
    public class FluentListViewColumnCollection : ObservableCollection<FluentListViewColumn>
    {
        private FluentListView owner;

        public FluentListViewColumnCollection(FluentListView owner)
        {
            this.owner = owner;
        }

        protected override void InsertItem(int index, FluentListViewColumn item)
        {
            base.InsertItem(index, item);
            item.Index = index;
            item.Changed += (s, e) => owner?.Invalidate();
            UpdateIndices();
            owner?.Invalidate();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            UpdateIndices();
            owner?.Invalidate();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            owner?.Invalidate();
        }

        private void UpdateIndices()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Index = i;
            }
        }
    }

    /// <summary>
    /// 子项
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FluentListViewSubItem
    {
        private string text = "";
        private Color foreColor = Color.Empty;
        private Color backColor = Color.Empty;
        private Font font;

        public FluentListViewSubItem()
        {
        }

        public FluentListViewSubItem(string text)
        {
            this.text = text;
        }

        [Category("Appearance")]
        [Description("子项文本")]
        [DefaultValue("")]
        public string Text
        {
            get => text;
            set => text = value ?? "";
        }

        [Category("Appearance")]
        [Description("前景色")]
        public Color ForeColor
        {
            get => foreColor;
            set => foreColor = value;
        }

        [Category("Appearance")]
        [Description("背景色")]
        public Color BackColor
        {
            get => backColor;
            set => backColor = value;
        }

        [Category("Appearance")]
        [Description("字体")]
        public Font Font
        {
            get => font;
            set => font = value;
        }

        [Category("Data")]
        [Description("用户数据")]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public object Tag { get; set; }

        // 添加辅助属性用于设计时编辑
        [Category("Data")]
        [Description("Tag字符串值(设计时辅助)")]
        [DefaultValue("")]
        public string TagString
        {
            get => Tag?.ToString() ?? "";
            set => Tag = value;
        }

        public override string ToString()
        {
            return text;
        }
    }

    /// <summary>
    /// 子项集合
    /// </summary>
    public class FluentListViewSubItemCollection : ObservableCollection<FluentListViewSubItem>
    {
        public FluentListViewSubItemCollection()
        {
        }

        public void Add(string text)
        {
            Add(new FluentListViewSubItem(text));
        }
    }

    /// <summary>
    /// 列表项
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ToolboxItem(false)]
    public class FluentListViewItem
    {
        private string text = "";
        private Image icon;
        private int imageIndex = -1;
        private string imageKey = "";
        private bool selected;
        private bool focused;
        private Color foreColor = Color.Empty;
        private Color backColor = Color.Empty;
        private Font font;
        private bool isChecked;
        private string group = "";

        public FluentListViewItem()
        {
            SubItems = new FluentListViewSubItemCollection();
            SubItems.Add(new FluentListViewSubItem(text));
        }

        public FluentListViewItem(string text) : this()
        {
            this.text = text;
            SubItems[0].Text = text;
        }

        public FluentListViewItem(string text, int imageIndex) : this(text)
        {
            this.imageIndex = imageIndex;
        }

        public FluentListViewItem(string[] items) : this()
        {
            if (items != null && items.Length > 0)
            {
                text = items[0];
                SubItems[0].Text = text;

                for (int i = 1; i < items.Length; i++)
                {
                    SubItems.Add(items[i]);
                }
            }
        }

        [Category("Appearance")]
        [Description("项文本")]
        [DefaultValue("")]
        public string Text
        {
            get => text;
            set
            {
                text = value ?? "";
                if (SubItems.Count > 0)
                {
                    SubItems[0].Text = text;
                }
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("图标")]
        public Image Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("图标索引")]
        [DefaultValue(-1)]
        public int ImageIndex
        {
            get => imageIndex;
            set
            {
                imageIndex = value;
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("图标键")]
        [DefaultValue("")]
        public string ImageKey
        {
            get => imageKey;
            set
            {
                imageKey = value ?? "";
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("前景色")]
        public Color ForeColor
        {
            get => foreColor;
            set
            {
                foreColor = value;
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("背景色")]
        public Color BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                OnChanged();
            }
        }

        [Category("Appearance")]
        [Description("字体")]
        public Font Font
        {
            get => font;
            set
            {
                font = value;
                OnChanged();
            }
        }

        [Category("Behavior")]
        [Description("是否选中")]
        [DefaultValue(false)]
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnChanged();
            }
        }

        [Category("Behavior")]
        [Description("是否勾选")]
        [DefaultValue(false)]
        public bool Checked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnChanged();
            }
        }

        [Category("Data")]
        [Description("分组名称")]
        [DefaultValue("")]
        public string Group
        {
            get => group;
            set
            {
                group = value ?? "";
                OnChanged();
            }
        }

        [Category("Data")]
        [Description("用户数据")]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public object Tag { get; set; }

        [Category("Data")]
        [Description("Tag字符串值(设计时辅助)")]
        [DefaultValue("")]
        public string TagString
        {
            get => Tag?.ToString() ?? "";
            set => Tag = value;
        }

        [Category("Data")]
        [Description("子项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentListViewSubItemCollection SubItems { get; }

        [Browsable(false)]
        public int Index { get; internal set; }

        [Browsable(false)]
        internal Rectangle Bounds { get; set; }

        [Browsable(false)]
        internal Rectangle IconBounds { get; set; }

        [Browsable(false)]
        internal Rectangle TextBounds { get; set; }

        [Browsable(false)]
        internal bool Hovered { get; set; }

        [Browsable(false)]
        internal bool Focused
        {
            get => focused;
            set => focused = value;
        }

        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return text;
        }
    }

    /// <summary>
    /// 项集合
    /// </summary>
    [Editor(typeof(FluentListViewItemCollectionEditor), typeof(UITypeEditor))]
    public class FluentListViewItemCollection : ObservableCollection<FluentListViewItem>
    {
        private FluentListView owner;

        public FluentListViewItemCollection(FluentListView owner)
        {
            this.owner = owner;
        }

        protected override void InsertItem(int index, FluentListViewItem item)
        {
            base.InsertItem(index, item);
            item.Index = index;
            item.Changed += (s, e) => owner?.Invalidate();
            UpdateIndices();
            owner?.UpdateLayout();
            owner?.Invalidate();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            UpdateIndices();
            owner?.UpdateLayout();
            owner?.Invalidate();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            owner?.UpdateLayout();
            owner?.Invalidate();
        }

        private void UpdateIndices()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Index = i;
            }
        }

        public void Add(string text)
        {
            Add(new FluentListViewItem(text));
        }

        public void Add(string text, int imageIndex)
        {
            Add(new FluentListViewItem(text, imageIndex));
        }

        public void Add(string[] items)
        {
            Add(new FluentListViewItem(items));
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 视图模式
    /// </summary>
    public enum FluentListViewMode
    {
        Details,        // 详细信息视图
        List,           // 列表视图
        LargeIcon,      // 大图标视图
        SmallIcon,      // 小图标视图
        Tile            // 平铺视图
    }

    /// <summary>
    /// 列排序方式
    /// </summary>
    public enum ColumnSortOrder
    {
        None,
        Ascending,
        Descending
    }

    /// <summary>
    /// 复选框样式
    /// </summary>
    public enum ListViewCheckBoxStyle
    {
        FilledSquare,       // 填充方形
        Check,              // 对勾
        CheckCircle         // 圆形对勾
    }

    /// <summary>
    /// 项事件参数
    /// </summary>
    public class FluentListViewItemEventArgs : EventArgs
    {
        public FluentListViewItem Item { get; }

        public FluentListViewItemEventArgs(FluentListViewItem item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// 列点击事件参数
    /// </summary>
    public class FluentListViewColumnClickEventArgs : EventArgs
    {
        public FluentListViewColumn Column { get; }
        public int ColumnIndex { get; }

        public FluentListViewColumnClickEventArgs(FluentListViewColumn column, int columnIndex)
        {
            Column = column;
            ColumnIndex = columnIndex;
        }
    }

    #endregion

    #region 设计时支持

    public class FluentListViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentListViewActionList(this.Component));
                }
                return actionLists;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
        }
    }


    public class FluentListViewActionList : DesignerActionList
    {
        private FluentListView listView;
        private DesignerActionUIService designerService;

        public FluentListViewActionList(IComponent component) : base(component)
        {
            listView = component as FluentListView;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            items.Add(new DesignerActionHeaderItem("操作"));

            if (listView.Dock == DockStyle.Fill)
            {
                items.Add(new DesignerActionMethodItem(this, "UndockControl", "取消停靠", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "DockFill", "在父容器中停靠", "操作", true));
            }

            if (listView.ShowColumnHeaders)
            {
                items.Add(new DesignerActionMethodItem(this, "HideColumnHeaders", "隐藏列标题", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "ShowColumnHeaders", "显示列标题", "操作", true));
            }

            if (listView.GridLines)
            {
                items.Add(new DesignerActionMethodItem(this, "HideGridLines", "隐藏网格线", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "ShowGridLines", "显示网格线", "操作", true));
            }

            if (listView.CheckBoxes)
            {
                items.Add(new DesignerActionMethodItem(this, "HideCheckBoxes", "隐藏复选框", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "ShowCheckBoxes", "显示复选框", "操作", true));
            }

            items.Add(new DesignerActionHeaderItem("显示"));
            items.Add(new DesignerActionPropertyItem("View", "视图模式", "显示设置", "设置ListView显示模式"));

            items.Add(new DesignerActionHeaderItem("集合编辑"));
            items.Add(new DesignerActionMethodItem(this, "EditColumns", "编辑列(Columns)...", "集合编辑", "编辑列集合", true));
            items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑项(Items)...", "集合编辑", "编辑项集合", true));

            items.Add(new DesignerActionHeaderItem("快捷操作"));
            items.Add(new DesignerActionMethodItem(this, "AddColumn", "添加列", "快捷操作", "添加测试列", true));
            items.Add(new DesignerActionMethodItem(this, "AddItem", "添加项", "快捷操作", "添加测试项", true));
            items.Add(new DesignerActionMethodItem(this, "AddSampleData", "添加示例数据", "快捷操作", "添加示例列和项", false));
            items.Add(new DesignerActionMethodItem(this, "ClearColumns", "清空所有列", "快捷操作", "清空所有列", false));
            items.Add(new DesignerActionMethodItem(this, "ClearItems", "清空所有项", "快捷操作", "清空所有项", false));

            return items;
        }

        // 属性包装器

        [DisplayName("视图模式")]
        [Description("设置ListView显示模式")]
        public FluentListViewMode View
        {
            get => listView.View;
            set => SetProperty("View", value);
        }

        // 方法
        [DisplayName("在父容器中停靠")]
        [Description("将控件停靠填充父容器")]
        public void DockFill()
        {
            if (listView.Parent != null)
            {
                PropertyDescriptor prop = GetPropertyByName("Dock");
                prop.SetValue(listView, DockStyle.Fill);
            }
            designerService?.Refresh(Component);
        }

        [DisplayName("取消停靠")]
        [Description("取消控件的停靠")]
        public void UndockControl()
        {
            PropertyDescriptor prop = GetPropertyByName("Dock");
            prop.SetValue(listView, DockStyle.None);
            designerService?.Refresh(Component);
        }

        [DisplayName("隐藏列标题")]
        public void HideColumnHeaders()
        {
            PropertyDescriptor prop = GetPropertyByName("ShowColumnHeaders");
            prop.SetValue(listView, false);
            designerService?.Refresh(Component);
        }

        [DisplayName("显示列标题")]
        public void ShowColumnHeaders()
        {
            PropertyDescriptor prop = GetPropertyByName("ShowColumnHeaders");
            prop.SetValue(listView, true);
            designerService?.Refresh(Component);
        }

        [DisplayName("隐藏网格线")]
        public void HideGridLines()
        {
            PropertyDescriptor prop = GetPropertyByName("GridLines");
            prop.SetValue(listView, false);
            designerService?.Refresh(Component);
        }

        [DisplayName("显示网格线")]
        public void ShowGridLines()
        {
            PropertyDescriptor prop = GetPropertyByName("GridLines");
            prop.SetValue(listView, true);
            designerService?.Refresh(Component);
        }

        [DisplayName("隐藏复选框")]
        public void HideCheckBoxes()
        {
            PropertyDescriptor prop = GetPropertyByName("CheckBoxes");
            prop.SetValue(listView, false);
            designerService?.Refresh(Component);
        }

        [DisplayName("显示复选框")]
        public void ShowCheckBoxes()
        {
            PropertyDescriptor prop = GetPropertyByName("CheckBoxes");
            prop.SetValue(listView, true);
            designerService?.Refresh(Component);
        }

        [Description("编辑列集合")]
        public void EditColumns()
        {
            var designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost != null)
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(listView)["Columns"];
                var editor = new FluentListViewColumnCollectionEditor(typeof(FluentListViewColumnCollection));
                var context = new TypeDescriptorContext(listView, propertyDescriptor, designerHost);

                editor.EditValue(context, context, listView.Columns);
                RaiseComponentChanged(propertyDescriptor);
            }
        }

        [Description("编辑项集合")]
        public void EditItems()
        {
            var designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost != null)
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(listView)["Items"];
                var editor = new FluentListViewItemCollectionEditor(typeof(FluentListViewItemCollection));
                var context = new TypeDescriptorContext(listView, propertyDescriptor, designerHost);

                editor.EditValue(context, context, listView.Items);
                RaiseComponentChanged(propertyDescriptor);
            }
        }

        [Description("添加列")]
        public void AddColumn()
        {
            var column = new FluentListViewColumn
            {
                Text = $"Column{listView.Columns.Count + 1}",
                Width = 100
            };
            listView.Columns.Add(column);
            RaiseComponentChanged(TypeDescriptor.GetProperties(listView)["Columns"]);
        }

        [Description("添加项")]
        public void AddItem()
        {
            var item = new FluentListViewItem($"Item{listView.Items.Count + 1}");

            // 添加与列数匹配的子项
            for (int i = 1; i < listView.Columns.Count; i++)
            {
                item.SubItems.Add($"SubItem{i}");
            }

            listView.Items.Add(item);
            RaiseComponentChanged(TypeDescriptor.GetProperties(listView)["Items"]);
        }

        [Description("清空所有列")]
        public void ClearColumns()
        {
            if (MessageBox.Show("确定要清空所有列吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listView.Columns.Clear();
                RaiseComponentChanged(TypeDescriptor.GetProperties(listView)["Columns"]);
            }
        }

        [Description("清空所有项")]
        public void ClearItems()
        {
            if (MessageBox.Show("确定要清空所有项吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listView.Items.Clear();
                RaiseComponentChanged(TypeDescriptor.GetProperties(listView)["Items"]);
            }
        }

        [Description("添加示例数据")]
        public void AddSampleData()
        {
            // 如果没有列，先添加列
            if (listView.Columns.Count == 0)
            {
                listView.Columns.Add(new FluentListViewColumn { Text = "名称", Width = 150 });
                listView.Columns.Add(new FluentListViewColumn { Text = "大小", Width = 100, TextAlign = HorizontalAlignment.Right });
                listView.Columns.Add(new FluentListViewColumn { Text = "类型", Width = 120 });
                listView.Columns.Add(new FluentListViewColumn { Text = "修改日期", Width = 150 });
            }

            // 添加示例项
            var sampleItems = new[]
            {
                new FluentListViewItem(new[] { "文档.docx", "125 KB", "Word 文档", "2024-01-15" }),
                new FluentListViewItem(new[] { "图片.png", "2.5 MB", "PNG 图像", "2024-01-14" }),
                new FluentListViewItem(new[] { "数据.xlsx", "458 KB", "Excel 工作表", "2024-01-13" })
            };

            foreach (var item in sampleItems)
            {
                listView.Items.Add(item);
            }

            RaiseComponentChanged(TypeDescriptor.GetProperties(listView)["Columns"]);
            RaiseComponentChanged(TypeDescriptor.GetProperties(listView)["Items"]);
        }


        private PropertyDescriptor GetPropertyByName(string propName)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(listView)[propName];
            if (prop == null)
            {
                throw new ArgumentException("未找到属性", propName);
            }

            return prop;
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(listView)[propertyName];
            if (property != null)
            {
                property.SetValue(listView, value);
                designerService?.Refresh(listView);
            }
        }

        private void RaiseComponentChanged(PropertyDescriptor property)
        {
            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            changeService?.OnComponentChanged(listView, property, null, null);
        }
    }


    /// <summary>
    /// 列集合编辑器
    /// </summary>
    public class FluentListViewColumnCollectionEditor : CollectionEditor
    {
        public FluentListViewColumnCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentListViewColumn);
        }

        protected override object CreateInstance(Type itemType)
        {
            return new FluentListViewColumn { Text = "Column" };
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentListViewColumn column)
            {
                return string.IsNullOrEmpty(column.Text) ? "(unnamed)" : column.Text;
            }
            return base.GetDisplayText(value);
        }
    }

    /// <summary>
    /// 项集合编辑器
    /// </summary>
    public class FluentListViewItemCollectionEditor : CollectionEditor
    {
        public FluentListViewItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentListViewItem);
        }

        protected override object CreateInstance(Type itemType)
        {
            return new FluentListViewItem("New Item");
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentListViewItem item)
            {
                return string.IsNullOrEmpty(item.Text) ? "(unnamed)" : item.Text;
            }
            return base.GetDisplayText(value);
        }
    }

    #endregion

}
