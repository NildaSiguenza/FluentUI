using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using FluentControls.IconFonts;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentToolListDesigner))]
    [DefaultEvent("SelectedItemChanged")]
    [DefaultProperty("Categories")]
    public class FluentToolList : FluentControlBase
    {
        private FluentToolListCategoryCollection categories;
        private FluentToolListItem selectedItem;
        private FluentToolListItem hoverItem;

        private VScrollBar scrollBar;
        private int scrollOffset = 0;

        // 尺寸相关
        private int itemHeight = 32;
        private int categoryHeight = 36;
        private int itemSpacing = 2;
        private int expandIconSize = 16;

        // 字体颜色
        private Color? categoryBackColor = null;
        private Color? categoryForeColor = null;
        private Font categoryFont = null;

        private Color? itemBackColor = null;
        private Color? itemForeColor = null;
        private Font itemFont = null;

        private Color? selectedItemBackColor = null;
        private Color? selectedItemForeColor = null;
        private Color? hoverItemBackColor = null;
        private Color? hoverItemForeColor = null;

        private int imageTextSpacing = 8;
        private bool isFocused = false;
        private bool isFirstPaint = true;

        // 动画相关
        private Dictionary<FluentToolListCategory, float> expandAnimations = new Dictionary<FluentToolListCategory, float>();
        private Timer expandTimer;

        // DPI 缩放
        private float dpiScale = 1.0f;

        // 事件
        public event EventHandler<FluentToolListItemEventArgs> SelectedItemChanged;
        public event EventHandler<FluentToolListItemEventArgs> ItemClick;
        public event EventHandler<FluentToolListItemEventArgs> ItemDoubleClick;


        #region 构造函数

        public FluentToolList()
        {
            categories = new FluentToolListCategoryCollection();
            categories.ItemChanged += OnCategoryCollectionChanged;

            InitializeScrollBar();
            InitializeDpiScale();
            InitializeExpandTimer();

            Size = new Size(200, 400);

            // 启用键盘支持
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        private void InitializeScrollBar()
        {
            scrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false,
                SmallChange = itemHeight,
                LargeChange = itemHeight * 3
            };
            scrollBar.Scroll += OnScrollBarScroll;
            Controls.Add(scrollBar);
        }

        private void InitializeDpiScale()
        {
            int dpi = DpiHelper.GetDpiForWindowSafe(this.Handle);
            dpiScale = dpi / 96f;

            itemHeight = (int)(itemHeight * dpiScale);
            categoryHeight = (int)(categoryHeight * dpiScale);
            itemSpacing = (int)(itemSpacing * dpiScale);
            expandIconSize = (int)(expandIconSize * dpiScale);
        }

        private void InitializeExpandTimer()
        {
            expandTimer = new Timer { Interval = 16 }; // 60 FPS
            expandTimer.Tick += OnExpandTimerTick;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 分组集合
        /// </summary>
        [Category("Fluent")]
        [Description("工具列表的分组集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentToolListCategoryCollectionEditor), typeof(UITypeEditor))]
        public FluentToolListCategoryCollection Categories
        {
            get => categories;
        }

        /// <summary>
        /// 选中的项目
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FluentToolListItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnSelectedItemChanged(new FluentToolListItemEventArgs(selectedItem));

                    if (!DesignMode && value != null)
                    {
                        // 使用 BeginInvoke 延迟执行, 确保布局完成
                        if (IsHandleCreated)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                EnsureVisible(value);
                            }));
                        }
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 项目高度
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(32)]
        public int ItemHeight
        {
            get => itemHeight;
            set
            {
                if (itemHeight != value && value > 0)
                {
                    itemHeight = (int)(value * dpiScale);
                    UpdateScrollBar();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 分组标题高度
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(36)]
        public int CategoryHeight
        {
            get => categoryHeight;
            set
            {
                if (categoryHeight != value && value > 0)
                {
                    categoryHeight = (int)(value * dpiScale);
                    UpdateScrollBar();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 项目间距
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(2)]
        public int ItemSpacing
        {
            get => itemSpacing;
            set
            {
                if (itemSpacing != value && value >= 0)
                {
                    itemSpacing = (int)(value * dpiScale);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 分组背景色
        /// </summary>
        [Category("Fluent")]
        [Description("分组标题的背景色, 留空则使用主题")]
        public Color CategoryBackColor
        {
            get => categoryBackColor ?? Color.Empty;
            set
            {
                categoryBackColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeCategoryBackColor()
        {
            return categoryBackColor.HasValue;
        }

        private void ResetCategoryBackColor()
        {
            categoryBackColor = null;
        }

        /// <summary>
        /// 分组前景色
        /// </summary>
        [Category("Fluent")]
        [Description("分组标题的前景色, 留空则使用主题")]
        public Color CategoryForeColor
        {
            get => categoryForeColor ?? Color.Empty;
            set
            {
                categoryForeColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeCategoryForeColor()
        {
            return categoryForeColor.HasValue;
        }

        private void ResetCategoryForeColor()
        {
            categoryForeColor = null;
        }

        /// <summary>
        /// 分组字体
        /// </summary>
        [Category("Fluent")]
        [Description("分组标题的字体, 留空则使用主题")]
        public Font CategoryFont
        {
            get => categoryFont;
            set
            {
                categoryFont = value;
                Invalidate();
            }
        }

        private bool ShouldSerializeCategoryFont()
        {
            return categoryFont != null;
        }

        private void ResetCategoryFont()
        {
            categoryFont = null;
        }

        /// <summary>
        /// 项目背景色
        /// </summary>
        [Category("Fluent")]
        [Description("普通项目的背景色, 留空则使用主题")]
        public Color ItemBackColor
        {
            get => itemBackColor ?? Color.Empty;
            set
            {
                itemBackColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeItemBackColor()
        {
            return itemBackColor.HasValue;
        }

        private void ResetItemBackColor()
        {
            itemBackColor = null;
        }

        /// <summary>
        /// 项目前景色
        /// </summary>
        [Category("Fluent")]
        [Description("普通项目的前景色, 留空则使用主题")]
        public Color ItemForeColor
        {
            get => itemForeColor ?? Color.Empty;
            set
            {
                itemForeColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeItemForeColor()
        {
            return itemForeColor.HasValue;
        }

        private void ResetItemForeColor()
        {
            itemForeColor = null;
        }

        /// <summary>
        /// 项目字体
        /// </summary>
        [Category("Fluent")]
        [Description("项目的字体, 留空则使用主题")]
        public Font ItemFont
        {
            get => itemFont;
            set
            {
                itemFont = value;
                Invalidate();
            }
        }

        private bool ShouldSerializeItemFont()
        {
            return itemFont != null;
        }

        private void ResetItemFont()
        {
            itemFont = null;
        }

        /// <summary>
        /// 选中项目背景色
        /// </summary>
        [Category("Fluent")]
        [Description("选中项目的背景色, 留空则使用主题")]
        public Color SelectedItemBackColor
        {
            get => selectedItemBackColor ?? Color.Empty;
            set
            {
                selectedItemBackColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeSelectedItemBackColor()
        {
            return selectedItemBackColor.HasValue;
        }

        private void ResetSelectedItemBackColor()
        {
            selectedItemBackColor = null;
        }

        /// <summary>
        /// 选中项目前景色
        /// </summary>
        [Category("Fluent")]
        [Description("选中项目的前景色, 留空则使用主题")]
        public Color SelectedItemForeColor
        {
            get => selectedItemForeColor ?? Color.Empty;
            set
            {
                selectedItemForeColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeSelectedItemForeColor()
        {
            return selectedItemForeColor.HasValue;
        }

        private void ResetSelectedItemForeColor()
        {
            selectedItemForeColor = null;
        }

        /// <summary>
        /// 悬停项目背景色
        /// </summary>
        [Category("Fluent")]
        [Description("悬停项目的背景色, 留空则使用主题")]
        public Color HoverItemBackColor
        {
            get => hoverItemBackColor ?? Color.Empty;
            set
            {
                hoverItemBackColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeHoverItemBackColor()
        {
            return hoverItemBackColor.HasValue;
        }

        private void ResetHoverItemBackColor()
        {
            hoverItemBackColor = null;
        }

        /// <summary>
        /// 悬停项目前景色
        /// </summary>
        [Category("Fluent")]
        [Description("悬停项目的前景色, 留空则使用主题")]
        public Color HoverItemForeColor
        {
            get => hoverItemForeColor ?? Color.Empty;
            set
            {
                hoverItemForeColor = value == Color.Empty ? (Color?)null : value;
                Invalidate();
            }
        }

        private bool ShouldSerializeHoverItemForeColor()
        {
            return hoverItemForeColor.HasValue;
        }

        private void ResetHoverItemForeColor()
        {
            hoverItemForeColor = null;
        }

        /// <summary>
        /// 图标和文本之间的间距
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(8)]
        [Description("图标和文本之间的间距(像素)")]
        public int ImageTextSpacing
        {
            get => imageTextSpacing;
            set
            {
                if (imageTextSpacing != value && value >= 0)
                {
                    imageTextSpacing = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region 事件

        protected virtual void OnSelectedItemChanged(FluentToolListItemEventArgs e)
        {
            SelectedItemChanged?.Invoke(this, e);
        }

        protected virtual void OnItemClick(FluentToolListItemEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }

        protected virtual void OnItemDoubleClick(FluentToolListItemEventArgs e)
        {
            ItemDoubleClick?.Invoke(this, e);
        }

        #endregion

        #region 键盘支持

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!Enabled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Up:
                    SelectPreviousItem();
                    e.Handled = true;
                    break;

                case Keys.Down:
                    SelectNextItem();
                    e.Handled = true;
                    break;

                case Keys.Home:
                    SelectFirstItem();
                    e.Handled = true;
                    break;

                case Keys.End:
                    SelectLastItem();
                    e.Handled = true;
                    break;

                case Keys.Enter:
                case Keys.Space:
                    // 如果选中的是分组, 切换展开状态
                    if (selectedItem is FluentToolListCategory category)
                    {
                        ToggleCategory(category);
                        e.Handled = true;
                    }
                    // 如果是普通项, 触发点击事件
                    else if (selectedItem != null)
                    {
                        OnItemClick(new FluentToolListItemEventArgs(selectedItem, MouseButtons.Left, Point.Empty));
                        e.Handled = true;
                    }
                    break;

                case Keys.Left:
                    // 折叠当前分组或跳转到父分组
                    CollapseCurrentOrSelectParent();
                    e.Handled = true;
                    break;

                case Keys.Right:
                    // 展开当前分组或进入子项
                    ExpandCurrentOrSelectChild();
                    e.Handled = true;
                    break;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // 确保箭头键等被视为输入键
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.Enter:
                case Keys.Space:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            isFocused = true;

            // 如果没有选中项, 选中第一个
            if (selectedItem == null)
            {
                SelectFirstItem();
            }

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            isFocused = false;
            Invalidate();
        }

        /// <summary>
        /// 获取所有可见项的有序列表
        /// </summary>
        private List<FluentToolListItem> GetVisibleItems()
        {
            var items = new List<FluentToolListItem>();

            foreach (var category in categories)
            {
                // 添加分组标题
                items.Add(category);

                // 如果分组展开, 添加子项
                if (category.IsExpanded)
                {
                    foreach (var item in category.Items)
                    {
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// 选中上一项
        /// </summary>
        private void SelectPreviousItem()
        {
            var visibleItems = GetVisibleItems();
            if (visibleItems.Count == 0)
            {
                return;
            }

            if (selectedItem == null)
            {
                SelectItem(visibleItems[visibleItems.Count - 1]);
                return;
            }

            int currentIndex = visibleItems.IndexOf(selectedItem);
            if (currentIndex > 0)
            {
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    if (visibleItems[i].Enabled)
                    {
                        SelectItem(visibleItems[i]);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 选中下一项
        /// </summary>
        private void SelectNextItem()
        {
            var visibleItems = GetVisibleItems();
            if (visibleItems.Count == 0)
            {
                return;
            }

            if (selectedItem == null)
            {
                SelectFirstItem();
                return;
            }

            int currentIndex = visibleItems.IndexOf(selectedItem);
            if (currentIndex >= 0 && currentIndex < visibleItems.Count - 1)
            {
                for (int i = currentIndex + 1; i < visibleItems.Count; i++)
                {
                    if (visibleItems[i].Enabled)
                    {
                        SelectItem(visibleItems[i]);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 选中第一项
        /// </summary>
        private void SelectFirstItem()
        {
            var visibleItems = GetVisibleItems();
            if (visibleItems.Count == 0)
            {
                return;
            }

            foreach (var item in visibleItems)
            {
                if (item.Enabled)
                {
                    SelectItem(item);
                    return;
                }
            }
        }

        /// <summary>
        /// 选中最后一项
        /// </summary>
        private void SelectLastItem()
        {
            var visibleItems = GetVisibleItems();
            if (visibleItems.Count == 0)
            {
                return;
            }

            for (int i = visibleItems.Count - 1; i >= 0; i--)
            {
                if (visibleItems[i].Enabled)
                {
                    SelectItem(visibleItems[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// 折叠当前分组或跳转到父分组
        /// </summary>
        private void CollapseCurrentOrSelectParent()
        {
            if (selectedItem == null)
            {
                return;
            }

            // 如果选中的是展开的分组, 折叠它
            if (selectedItem is FluentToolListCategory category && category.IsExpanded)
            {
                ToggleCategory(category);
                return;
            }

            // 如果选中的是子项, 跳转到父分组
            if (!(selectedItem is FluentToolListCategory))
            {
                var parent = selectedItem.Parent;
                if (parent != null)
                {
                    SelectItem(parent);
                }
            }
            // 如果选中的是已折叠的分组, 跳转到上一个分组
            else if (selectedItem is FluentToolListCategory collapsedCategory)
            {
                int index = categories.IndexOf(collapsedCategory);
                if (index > 0)
                {
                    SelectItem(categories[index - 1]);
                }
            }
        }

        /// <summary>
        /// 展开当前分组或进入子项
        /// </summary>
        private void ExpandCurrentOrSelectChild()
        {
            if (selectedItem == null)
            {
                return;
            }

            // 如果选中的是折叠的分组, 展开它
            if (selectedItem is FluentToolListCategory category)
            {
                if (!category.IsExpanded)
                {
                    ToggleCategory(category);
                }
                // 如果已展开且有子项, 选中第一个子项
                else if (category.Items.Count > 0)
                {
                    // 查找第一个可用子项
                    foreach (var item in category.Items)
                    {
                        if (item.Enabled)
                        {
                            SelectItem(item);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 选中指定项并确保可见
        /// </summary>
        private void SelectItem(FluentToolListItem item)
        {
            if (item == null || !item.Enabled)
            {
                return;
            }

            // 先设置选中项
            var oldItem = selectedItem;
            selectedItem = item;

            // 触发事件
            if (oldItem != item)
            {
                OnSelectedItemChanged(new FluentToolListItemEventArgs(item));
            }

            // 确保可见
            EnsureVisible(item);

            // 重绘
            Invalidate();
        }

        #endregion

        #region 重写方法

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 句柄创建后延迟更新滚动条
            BeginInvoke(new Action(() =>
            {
                UpdateScrollBar();
            }));
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible && IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    UpdateScrollBar();
                }));
            }
        }

        #endregion

        #region 绘制方法

        protected override void OnPaint(PaintEventArgs e)
        {
            // 首次绘制时强制更新滚动条
            if (isFirstPaint)
            {
                isFirstPaint = false;
                // 延迟更新滚动条, 确保布局完成
                BeginInvoke(new Action(() =>
                {
                    UpdateScrollBar();
                    Invalidate();
                }));
            }

            base.OnPaint(e);
        }

        protected override void DrawBackground(Graphics g)
        {
            var bgColor = GetThemeColor(
                c => c.Background,
                SystemColors.Window);

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            var visibleRect = GetVisibleRectangle();
            int top = itemSpacing - scrollOffset;
            int left = itemSpacing;
            int right = visibleRect.Width - itemSpacing;

            foreach (var category in categories)
            {
                // 绘制分组标题
                var categoryRect = new Rectangle(left, top, right - left, categoryHeight);

                if (categoryRect.IntersectsWith(visibleRect))
                {
                    DrawCategory(g, category, categoryRect);
                }

                top += categoryHeight + itemSpacing;

                // 绘制分组项目(带展开动画)
                if (category.IsExpanded || expandAnimations.ContainsKey(category))
                {
                    float progress = 1.0f;
                    if (expandAnimations.ContainsKey(category))
                    {
                        progress = expandAnimations[category];
                    }

                    int itemsHeight = category.Items.Count * (itemHeight + itemSpacing);
                    int animatedHeight = (int)(itemsHeight * progress);

                    if (animatedHeight > 0)
                    {
                        // 创建剪切区域以实现展开动画
                        var clipRect = new Rectangle(left, top, right - left, animatedHeight);
                        var oldClip = g.Clip;
                        g.SetClip(clipRect, CombineMode.Intersect);

                        int itemTop = top;
                        foreach (var item in category.Items)
                        {
                            var itemRect = new Rectangle(
                                left + expandIconSize,
                                itemTop,
                                right - left - expandIconSize,
                                itemHeight);

                            if (itemRect.IntersectsWith(visibleRect))
                            {
                                DrawItem(g, item, itemRect);
                            }

                            itemTop += itemHeight + itemSpacing;
                        }

                        g.Clip = oldClip;
                    }

                    top += animatedHeight;
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = GetThemeColor(
                c => c.Border,
                SystemColors.ControlDark);

            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        /// <summary>
        /// 绘制分组标题
        /// </summary>
        private void DrawCategory(Graphics g, FluentToolListCategory category, Rectangle rect)
        {
            bool isSelected = (category == selectedItem);

            // 背景色 - 优先使用自定义颜色
            var bgColor = categoryBackColor ?? GetThemeColor(
                c => c.BackgroundSecondary,
                SystemColors.ControlLight);

            var textColor = categoryForeColor ?? GetThemeColor(
                c => c.TextPrimary,
                SystemColors.ControlText);

            // 如果分组被选中, 使用不同的背景色
            if (isSelected)
            {
                bgColor = GetThemeColor(
                    c => c.SurfaceHover,
                    SystemColors.ControlLight);
            }

            // 绘制背景
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 4;

            using (var path = GetRoundedRectangle(rect, cornerRadius))
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
            }

            // 绘制键盘焦点框
            if (isSelected && isFocused)
            {
                var focusColor = GetThemeColor(c => c.Primary, SystemColors.Highlight);
                using (var pen = new Pen(focusColor, 1))
                {
                    pen.DashStyle = DashStyle.Dot;
                    var focusRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                    g.DrawRectangle(pen, focusRect);
                }
            }

            // 绘制展开/折叠图标
            var iconRect = new Rectangle(
                rect.Left + 8,
                rect.Top + (rect.Height - expandIconSize) / 2,
                expandIconSize,
                expandIconSize);

            DrawExpandIcon(g, category, iconRect, textColor);

            // 绘制文本
            var textRect = new Rectangle(
                iconRect.Right + 8,
                rect.Top,
                rect.Width - iconRect.Right - 16,
                rect.Height);

            // 字体
            var font = categoryFont ?? GetThemeFont(
                t => t.Title,
                new Font(Font, FontStyle.Bold));

            using (var brush = new SolidBrush(textColor))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                g.DrawString(category.Name, font, brush, textRect, sf);
            }
        }

        /// <summary>
        /// 绘制展开/折叠图标
        /// </summary>
        private void DrawExpandIcon(Graphics g, FluentToolListCategory category, Rectangle rect, Color iconColor)
        {
            float angle = 0f;
            if (expandAnimations.ContainsKey(category))
            {
                angle = expandAnimations[category] * 90f;
            }
            else if (category.IsExpanded)
            {
                angle = 90f;
            }

            using (var pen = new Pen(iconColor, 2))
            {
                var state = g.Save();
                g.TranslateTransform(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f);
                g.RotateTransform(angle);

                var arrowSize = rect.Width / 3f;
                var points = new PointF[]
                {
                new PointF(-arrowSize / 2, -arrowSize),
                new PointF(arrowSize / 2, 0),
                new PointF(-arrowSize / 2, arrowSize)
                };

                g.DrawLines(pen, points);
                g.Restore(state);
            }
        }

        /// <summary>
        /// 绘制项目
        /// </summary>
        private void DrawItem(Graphics g, FluentToolListItem item, Rectangle rect)
        {
            bool isSelected = (item == selectedItem);
            bool isHover = (item == hoverItem);

            // 背景色
            Color bgColor = Color.Transparent;
            if (isSelected)
            {
                bgColor = selectedItemBackColor ?? GetThemeColor(
                    c => c.Primary,
                    SystemColors.Highlight);
            }
            else if (isHover)
            {
                bgColor = hoverItemBackColor ?? GetThemeColor(
                    c => c.PrimaryLight,
                    SystemColors.ControlLight);
            }
            else if (itemBackColor.HasValue)
            {
                bgColor = itemBackColor.Value;
            }

            // 绘制背景
            if (bgColor != Color.Transparent)
            {
                int cornerRadius = UseTheme && Theme?.Elevation != null
                    ? Theme.Elevation.CornerRadiusSmall
                    : 3;

                using (var path = GetRoundedRectangle(rect, cornerRadius))
                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // 文本颜色
            Color textColor;
            if (isSelected)
            {
                textColor = selectedItemForeColor ?? GetThemeColor(
                    c => c.TextOnPrimary,
                    SystemColors.HighlightText);
            }
            else if (isHover)
            {
                textColor = hoverItemForeColor ?? itemForeColor ?? GetThemeColor(
                    c => c.TextOnPrimary,
                    SystemColors.ControlText);
            }
            else if (item.Enabled)
            {
                textColor = itemForeColor ?? GetThemeColor(
                    c => c.TextPrimary,
                    SystemColors.ControlText);
            }
            else
            {
                textColor = GetThemeColor(
                    c => c.TextDisabled,
                    SystemColors.GrayText);
            }

            // 绘制选中指示条
            if (isSelected)
            {
                var indicatorColor = GetThemeColor(c => c.Primary, SystemColors.Highlight);
                using (var brush = new SolidBrush(indicatorColor))
                {
                    g.FillRectangle(brush, rect.Left, rect.Top + 4, 3, rect.Height - 8);
                }
            }

            // 绘制键盘焦点框
            if (isSelected && isFocused)
            {
                var focusColor = GetThemeColor(c => c.Primary, SystemColors.Highlight);
                using (var pen = new Pen(focusColor, 1))
                {
                    pen.DashStyle = DashStyle.Dot;
                    var focusRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                    g.DrawRectangle(pen, focusRect);
                }
            }

            // 绘制图标(如果有)
            int contentLeft = rect.Left + 12;
            if (item.Image != null)
            {
                var iconRect = new Rectangle(
                    contentLeft,
                    rect.Top + (rect.Height - 16) / 2,
                    16, 16);

                if (item.Enabled)
                {
                    g.DrawImage(item.Image, iconRect);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(g, item.Image, iconRect.X, iconRect.Y, bgColor);
                }

                contentLeft = iconRect.Right + imageTextSpacing;
            }

            // 绘制文本
            var textRect = new Rectangle(
                contentLeft,
                rect.Top,
                rect.Right - contentLeft - 8,
                rect.Height);

            // 字体
            var font = itemFont ?? Font;
            if (isSelected)
            {
                font = new Font(font, FontStyle.Bold);
            }

            using (var brush = new SolidBrush(textColor))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                g.DrawString(item.Name, font, brush, textRect, sf);
            }

            // 释放临时创建的粗体字体
            if (isSelected && itemFont == null)
            {
                font.Dispose();
            }
        }

        #endregion

        #region 交互处理

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Focused && CanFocus)
            {
                Focus();
            }

            base.OnMouseDown(e);

            var item = GetItemAtPoint(e.Location);
            if (item != null)
            {
                if (item is FluentToolListCategory category)
                {
                    // 点击分组标题, 切换展开/折叠状态
                    ToggleCategory(category);
                }
                else
                {
                    // 点击普通项目
                    if (item.Enabled)
                    {
                        SelectedItem = item;
                        OnItemClick(new FluentToolListItemEventArgs(item, e.Button, e.Location));
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button != MouseButtons.None)
            {
                return;
            }

            var item = GetItemAtPoint(e.Location);
            if (hoverItem != item)
            {
                hoverItem = item;
                Invalidate();
            }

            // 更新光标
            if (item != null && item.Enabled)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoverItem != null)
            {
                hoverItem = null;
                Invalidate();
            }

            Cursor = Cursors.Default;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var item = GetItemAtPoint(e.Location);
            if (item != null && !(item is FluentToolListCategory) && item.Enabled)
            {
                OnItemDoubleClick(new FluentToolListItemEventArgs(item, e.Button, e.Location));
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (scrollBar.Visible)
            {
                // 计算滚动增量(一次滚动一个项目高度)
                int scrollAmount = itemHeight + itemSpacing;
                int delta = (e.Delta > 0) ? -scrollAmount : scrollAmount;

                // 计算新的滚动位置
                int newScrollOffset = scrollOffset + delta;

                // 限制范围
                var visibleRect = GetVisibleRectangle();
                int maxScroll = Math.Max(0, GetTotalHeight() - visibleRect.Height);
                newScrollOffset = Math.Max(0, Math.Min(newScrollOffset, maxScroll));

                if (newScrollOffset != scrollOffset)
                {
                    scrollOffset = newScrollOffset;

                    // 更新滚动条
                    int scrollValue = Math.Max(scrollBar.Minimum,
                        Math.Min(newScrollOffset, scrollBar.Maximum - scrollBar.LargeChange));
                    if (scrollValue >= 0)
                    {
                        scrollBar.Value = scrollValue;
                    }

                    Invalidate();
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        #endregion

        #region 项目定位

        /// <summary>
        /// 获取指定坐标处的项目
        /// </summary>
        private FluentToolListItem GetItemAtPoint(Point point)
        {
            var visibleRect = GetVisibleRectangle();
            if (!visibleRect.Contains(point))
            {
                return null;
            }

            int top = itemSpacing - scrollOffset;
            int left = itemSpacing;
            int right = visibleRect.Width - itemSpacing;

            foreach (var category in categories)
            {
                // 检查分组标题
                var categoryRect = new Rectangle(left, top, right - left, categoryHeight);
                if (categoryRect.Contains(point))
                {
                    return category;
                }

                top += categoryHeight + itemSpacing;

                // 检查分组项目
                if (category.IsExpanded || expandAnimations.ContainsKey(category))
                {
                    float progress = expandAnimations.ContainsKey(category)
                        ? expandAnimations[category]
                        : 1.0f;

                    int itemsHeight = category.Items.Count * (itemHeight + itemSpacing);
                    int animatedHeight = (int)(itemsHeight * progress);

                    if (animatedHeight > 0)
                    {
                        int itemTop = top;
                        foreach (var item in category.Items)
                        {
                            var itemRect = new Rectangle(
                                left + expandIconSize,
                                itemTop,
                                right - left - expandIconSize,
                                itemHeight);

                            if (itemRect.Contains(point))
                            {
                                return item;
                            }

                            itemTop += itemHeight + itemSpacing;
                        }
                    }

                    top += animatedHeight;
                }
            }

            return null;
        }

        /// <summary>
        /// 确保项目可见
        /// </summary>
        private void EnsureVisible(FluentToolListItem item)
        {
            if (item == null)
            {
                return;
            }

            // 获取项目在内容中的绝对位置
            var itemBounds = GetItemBounds(item);
            if (itemBounds == Rectangle.Empty)
            {
                return;
            }

            var visibleRect = GetVisibleRectangle();
            if (visibleRect.Height <= 0)
            {
                return;
            }

            // 项目顶部相对于可见区域顶部的位置
            int itemTopInView = itemBounds.Top - scrollOffset;
            // 项目底部相对于可见区域顶部的位置
            int itemBottomInView = itemBounds.Bottom - scrollOffset;

            int newScrollOffset = scrollOffset;

            // 如果项目顶部在可见区域上方, 向上滚动
            if (itemTopInView < 0)
            {
                newScrollOffset = itemBounds.Top - itemSpacing;
            }
            // 如果项目底部在可见区域下方, 向下滚动
            else if (itemBottomInView > visibleRect.Height)
            {
                newScrollOffset = itemBounds.Bottom - visibleRect.Height + itemSpacing;
            }

            // 确保滚动值在有效范围内
            newScrollOffset = Math.Max(0, newScrollOffset);

            int maxScroll = GetTotalHeight() - visibleRect.Height;
            if (maxScroll > 0)
            {
                newScrollOffset = Math.Min(newScrollOffset, maxScroll);
            }
            else
            {
                newScrollOffset = 0;
            }

            // 如果需要滚动
            if (newScrollOffset != scrollOffset)
            {
                scrollOffset = newScrollOffset;

                // 更新滚动条位置
                if (scrollBar.Visible)
                {
                    int scrollValue = Math.Max(scrollBar.Minimum,
                        Math.Min(newScrollOffset, scrollBar.Maximum - scrollBar.LargeChange + 1));

                    if (scrollValue >= 0 && scrollValue <= scrollBar.Maximum)
                    {
                        scrollBar.Value = scrollValue;
                    }
                }

                Invalidate();
            }
        }

        /// <summary>
        /// 获取项目边界
        /// </summary>
        private Rectangle GetItemBounds(FluentToolListItem item)
        {
            if (item == null)
            {
                return Rectangle.Empty;
            }

            int top = itemSpacing;
            int left = itemSpacing;
            var visibleRect = GetVisibleRectangle();
            int right = visibleRect.Width - itemSpacing;

            foreach (var category in categories)
            {
                // 检查是否是分组标题
                if (category == item)
                {
                    return new Rectangle(left, top, right - left, categoryHeight);
                }

                top += categoryHeight + itemSpacing;

                // 检查分组内的项目
                if (category.IsExpanded)
                {
                    foreach (var childItem in category.Items)
                    {
                        if (childItem == item)
                        {
                            return new Rectangle(
                                left + expandIconSize,
                                top,
                                right - left - expandIconSize,
                                itemHeight);
                        }

                        top += itemHeight + itemSpacing;
                    }
                }
                else if (expandAnimations.ContainsKey(category))
                {
                    // 正在动画中, 也需要计算
                    float progress = expandAnimations[category];
                    int itemsHeight = category.Items.Count * (itemHeight + itemSpacing);
                    top += (int)(itemsHeight * progress);
                }
            }

            return Rectangle.Empty;
        }

        #endregion

        #region 展开/折叠动画

        /// <summary>
        /// 切换分组展开/折叠状态
        /// </summary>
        private void ToggleCategory(FluentToolListCategory category)
        {
            if (category == null)
            {
                return;
            }

            category.IsExpanded = !category.IsExpanded;

            if (EnableAnimation && !DesignMode)
            {
                StartExpandAnimation(category);
            }
            else
            {
                UpdateScrollBar();
                Invalidate();
            }
        }

        /// <summary>
        /// 开始展开动画
        /// </summary>
        private void StartExpandAnimation(FluentToolListCategory category)
        {
            if (!expandAnimations.ContainsKey(category))
            {
                expandAnimations[category] = category.IsExpanded ? 0f : 1f;
            }

            if (!expandTimer.Enabled)
            {
                expandTimer.Start();
            }
        }

        /// <summary>
        /// 展开动画计时器
        /// </summary>
        private void OnExpandTimerTick(object sender, EventArgs e)
        {
            bool needsUpdate = false;
            var completedCategories = new List<FluentToolListCategory>();

            foreach (var kvp in expandAnimations.ToArray())
            {
                var category = kvp.Key;
                float current = kvp.Value;
                float target = category.IsExpanded ? 1f : 0f;

                float delta = (target - current) * 0.2f;

                if (Math.Abs(delta) < 0.01f)
                {
                    expandAnimations[category] = target;
                    completedCategories.Add(category);
                }
                else
                {
                    expandAnimations[category] = current + delta;
                    needsUpdate = true;
                }
            }

            // 移除已完成的动画
            foreach (var category in completedCategories)
            {
                expandAnimations.Remove(category);
            }

            // 更新滚动条和重绘
            UpdateScrollBar();
            Invalidate();

            if (expandAnimations.Count == 0)
            {
                expandTimer.Stop();
                // 动画完成后再次确保滚动条正确
                BeginInvoke(new Action(() =>
                {
                    UpdateScrollBar();
                }));
            }
        }

        #endregion

        #region 滚动条处理

        /// <summary>
        /// 获取可见矩形区域
        /// </summary>
        private Rectangle GetVisibleRectangle()
        {
            var rect = ClientRectangle;

            // 确保矩形有效
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                rect = new Rectangle(0, 0, Width > 0 ? Width : 200, Height > 0 ? Height : 400);
            }

            if (scrollBar.Visible)
            {
                rect.Width -= scrollBar.Width;
            }

            return rect;
        }

        /// <summary>
        /// 计算总高度
        /// </summary>
        private int GetTotalHeight()
        {
            int total = itemSpacing;

            foreach (var category in categories)
            {
                total += categoryHeight + itemSpacing;

                // 始终计算展开状态下的高度, 或者正在动画中的高度
                if (category.IsExpanded || expandAnimations.ContainsKey(category))
                {
                    float progress = 1.0f;
                    if (expandAnimations.ContainsKey(category))
                    {
                        progress = expandAnimations[category];
                    }
                    else if (category.IsExpanded)
                    {
                        progress = 1.0f;
                    }

                    int itemsHeight = category.Items.Count * (itemHeight + itemSpacing);
                    total += (int)(itemsHeight * progress);
                }
            }

            // 添加底部边距
            total += itemSpacing;

            return total;
        }

        /// <summary>
        /// 更新滚动条
        /// </summary>
        private void UpdateScrollBar()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            var visibleRect = GetVisibleRectangle();

            if (visibleRect.Height <= 0)
            {
                return;
            }

            int totalHeight = GetTotalHeight();

            if (totalHeight > visibleRect.Height)
            {
                // 计算滚动条参数
                int largeChange = visibleRect.Height;
                int smallChange = itemHeight + itemSpacing;
                int maximum = totalHeight;

                // 设置滚动条属性
                scrollBar.Minimum = 0;
                scrollBar.Maximum = maximum;
                scrollBar.LargeChange = largeChange;
                scrollBar.SmallChange = smallChange;
                scrollBar.Visible = true;

                // 确保当前滚动位置有效
                int maxScrollOffset = Math.Max(0, totalHeight - visibleRect.Height);
                if (scrollOffset > maxScrollOffset)
                {
                    scrollOffset = maxScrollOffset;
                }

                // 设置滚动条值
                int scrollValue = Math.Max(0, Math.Min(scrollOffset, maximum - largeChange));
                if (scrollValue >= 0)
                {
                    scrollBar.Value = scrollValue;
                }
            }
            else
            {
                scrollBar.Visible = false;
                scrollOffset = 0;
            }
        }

        /// <summary>
        /// 滚动条滚动事件
        /// </summary>
        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            scrollOffset = e.NewValue;
            Invalidate();
        }

        #endregion

        #region 集合变更处理

        private void OnCategoryCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Element is FluentToolListCategory category)
            {
                if (e.Action == CollectionChangeAction.Add)
                {
                    category.Items.ItemChanged += OnCategoryItemsChanged;
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    category.Items.ItemChanged -= OnCategoryItemsChanged;
                }
            }

            // 延迟更新滚动条
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    UpdateScrollBar();
                    Invalidate();
                }));
            }
            else
            {
                isFirstPaint = true;  // 标记需要在首次绘制时更新
            }
        }

        private void OnCategoryItemsChanged(object sender, CollectionChangeEventArgs e)
        {
            // 延迟更新滚动条
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    UpdateScrollBar();
                    Invalidate();
                }));
            }
            else
            {
                isFirstPaint = true;
            }
        }

        #endregion

        #region 主题应用

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                BackColor = GetThemeColor(c => c.Background, SystemColors.Window);
                ForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                Font = GetThemeFont(t => t.Body, SystemFonts.DefaultFont);
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                expandTimer?.Dispose();
                scrollBar?.Dispose();

                if (categories != null)
                {
                    categories.ItemChanged -= OnCategoryCollectionChanged;
                    foreach (var category in categories)
                    {
                        category.Items.ItemChanged -= OnCategoryItemsChanged;
                    }
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    #region 工具项

    public class FluentToolListItem
    {
        public FluentToolListItem()
        {
        }

        public FluentToolListItem(string name) : this()
        {
            Name = name;
        }

        public FluentToolListItem(string name, object tag) : this(name)
        {
            Tag = tag;
        }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图标索引
        /// </summary>
        public int ImageIndex { get; set; } = -1;

        /// <summary>
        /// 图标
        /// </summary>
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image Image { get; set; }

        /// <summary>
        /// 父分组
        /// </summary>
        [Browsable(false)]
        public FluentToolListCategory Parent { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        [Browsable(false)]
        public object Tag { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 工具提示
        /// </summary>
        public string ToolTipText { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FluentToolListCategory : FluentToolListItem
    {
        public FluentToolListCategory()
        {
            Items = new FluentToolListItemCollection();
        }

        public FluentToolListCategory(string name) : base(name)
        {
            Items = new FluentToolListItemCollection();
        }

        /// <summary>
        /// 子项集合
        /// </summary>
        [Category("Data")]
        [Description("分组中的项目集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentToolListItemCollectionEditor), typeof(UITypeEditor))]
        public FluentToolListItemCollection Items { get; set; }

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded { get; set; } = true;

        /// <summary>
        /// 添加子项
        /// </summary>
        public void Add(FluentToolListItem item)
        {
            if (Items != null && item != null)
            {
                item.Parent = this;
                Items.Add(item);
            }
        }
    }


    public class FluentToolListItemCollection : Collection<FluentToolListItem>
    {
        public event CollectionChangeEventHandler ItemChanged;

        protected override void InsertItem(int index, FluentToolListItem item)
        {
            if (item != null && this[item.Name] == null)
            {
                base.InsertItem(index, item);
                ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
            }
        }

        protected override void RemoveItem(int index)
        {
            FluentToolListItem item = base[index];
            base.RemoveItem(index);
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));
        }

        protected override void SetItem(int index, FluentToolListItem item)
        {
            base.SetItem(index, item);
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        /// <summary>
        /// 根据名称获取项目
        /// </summary>
        public FluentToolListItem this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                for (int i = 0; i < Count; i++)
                {
                    if (this[i].Name == name)
                    {
                        return this[i];
                    }
                }
                return null;
            }
        }
    }

    /// <summary>
    /// FluentToolList 分组集合
    /// </summary>
    public class FluentToolListCategoryCollection : Collection<FluentToolListCategory>
    {
        public event CollectionChangeEventHandler ItemChanged;

        protected override void InsertItem(int index, FluentToolListCategory item)
        {
            base.InsertItem(index, item);
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }

        protected override void RemoveItem(int index)
        {
            FluentToolListCategory item = base[index];
            base.RemoveItem(index);
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));
        }

        protected override void SetItem(int index, FluentToolListCategory item)
        {
            base.SetItem(index, item);
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        /// <summary>
        /// 根据 ID 获取分组
        /// </summary>
        public FluentToolListCategory this[string id]
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                for (int i = 0; i < Count; i++)
                {
                    if (this[i].Id == id)
                    {
                        return this[i];
                    }
                }
                return null;
            }
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 选中项变更事件参数
    /// </summary>
    public class FluentToolListItemEventArgs : EventArgs
    {
        public FluentToolListItemEventArgs(FluentToolListItem item)
        {
            Item = item;
        }

        public FluentToolListItemEventArgs(FluentToolListItem item, MouseButtons button, Point location)
        {
            Item = item;
            MouseButton = button;
            Location = location;
        }

        public FluentToolListItem Item { get; set; }
        public MouseButtons MouseButton { get; set; }
        public Point Location { get; set; }
    }

    #endregion

    #region 设计时支持

    /// <summary>
    /// FluentToolList 项目集合编辑器
    /// </summary>
    internal class FluentToolListItemCollectionEditor : CollectionEditor
    {
        public FluentToolListItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(FluentToolListItem) };
        }

        protected override object CreateInstance(Type itemType)
        {
            var item = base.CreateInstance(itemType) as FluentToolListItem;
            if (item != null)
            {
                item.Name = "Item" + DateTime.Now.Ticks.ToString().Substring(8);
            }
            return item;
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentToolListItem item)
            {
                return string.IsNullOrEmpty(item.Name) ? "(未命名)" : item.Name;
            }
            return base.GetDisplayText(value);
        }
    }

    /// <summary>
    /// FluentToolList 分组集合编辑器
    /// </summary>
    internal class FluentToolListCategoryCollectionEditor : CollectionEditor
    {
        public FluentToolListCategoryCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(FluentToolListCategory) };
        }

        protected override object CreateInstance(Type itemType)
        {
            var category = base.CreateInstance(itemType) as FluentToolListCategory;
            if (category != null)
            {
                category.Name = "Category" + DateTime.Now.Ticks.ToString().Substring(8);
                category.IsExpanded = true;
            }
            return category;
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentToolListCategory category)
            {
                string displayName = string.IsNullOrEmpty(category.Name) ? "(未命名)" : category.Name;
                return $"{displayName} ({category.Items.Count} 项)";
            }
            return base.GetDisplayText(value);
        }
    }

    /// <summary>
    /// FluentPanelList 面板集合编辑器
    /// </summary>
    internal class FluentPanelListItemCollectionEditor : CollectionEditor
    {
        public FluentPanelListItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(FluentPanelListItem) };
        }

        protected override object CreateInstance(Type itemType)
        {
            var panel = base.CreateInstance(itemType) as FluentPanelListItem;
            if (panel != null)
            {
                panel.Title = "Panel" + DateTime.Now.Ticks.ToString().Substring(8);
                panel.IsExpanded = true;
            }
            return panel;
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentPanelListItem panel)
            {
                string displayName = string.IsNullOrEmpty(panel.Title) ? "(未命名)" : panel.Title;
                return $"{displayName} ({(panel.IsExpanded ? "展开" : "折叠")})";
            }
            return base.GetDisplayText(value);
        }
    }

    public class FluentToolListDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentToolListActionList(Component));
                }
                return actionLists;
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // 启用拖放
            if (component is FluentToolList toolList)
            {
                foreach (Control control in toolList.Controls)
                {
                    if (control != null)
                    {
                        EnableDesignMode(control, control.Name);
                    }
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 移除不需要的属性
            properties.Remove("AutoScroll");
            properties.Remove("AutoScrollMargin");
            properties.Remove("AutoScrollMinSize");
        }
    }

    public class FluentToolListActionList : DesignerActionList
    {
        private FluentToolList control;
        private DesignerActionUIService designerService;

        public FluentToolListActionList(IComponent component) : base(component)
        {
            control = component as FluentToolList;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 布局分组
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Dock", "Dock", "布局", "设置控件Docking模式"));
            items.Add(new DesignerActionPropertyItem("ItemHeight", "项目高度", "布局"));
            items.Add(new DesignerActionPropertyItem("CategoryHeight", "分组高度", "布局"));
            items.Add(new DesignerActionPropertyItem("ItemSpacing", "项目间距", "布局"));

            // 操作分组
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "EditCategories", "编辑分组...", "操作", true));
            items.Add(new DesignerActionMethodItem(this, "AddCategory", "添加分组", "操作", false));
            items.Add(new DesignerActionMethodItem(this, "ExpandAll", "展开所有", "操作", false));
            items.Add(new DesignerActionMethodItem(this, "CollapseAll", "折叠所有", "操作", false));

            return items;
        }

        #region 属性

        public DockStyle Dock
        {
            get => control.Dock;
            set => SetProperty("Dock", value);
        }


        public int ItemHeight
        {
            get => control.ItemHeight;
            set
            {
                SetProperty("ItemHeight", value);
            }
        }

        public int CategoryHeight
        {
            get => control.CategoryHeight;
            set
            {
                SetProperty("CategoryHeight", value);
            }
        }

        public int ItemSpacing
        {
            get => control.ItemSpacing;
            set
            {
                SetProperty("ItemSpacing", value);
            }
        }

        #endregion

        #region 方法

        private void EditCategories()
        {
            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            var propertyDescriptor = TypeDescriptor.GetProperties(control)["Categories"];

            var context = new TypeDescriptorContext(control, propertyDescriptor, host);
            var editor = new FluentToolListCategoryCollectionEditor(typeof(FluentToolListCategoryCollection));

            // 使用 context 作为 IServiceProvider
            var result = editor.EditValue(context, context, control.Categories);

            // 如果有变化, 通知设计器
            if (result != null)
            {
                var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                changeService?.OnComponentChanged(control, propertyDescriptor, null, result);
                control.Invalidate();
            }
        }

        private void AddCategory()
        {
            var category = new FluentToolListCategory
            {
                Name = "新分组" + (control.Categories.Count + 1),
                IsExpanded = true
            };

            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            var propertyDescriptor = TypeDescriptor.GetProperties(control)["Categories"];

            changeService?.OnComponentChanging(control, propertyDescriptor);
            control.Categories.Add(category);
            changeService?.OnComponentChanged(control, propertyDescriptor, null, null);

            designerService?.Refresh(control);
        }

        private void ExpandAll()
        {
            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            var propertyDescriptor = TypeDescriptor.GetProperties(control)["Categories"];

            changeService?.OnComponentChanging(control, propertyDescriptor);

            foreach (var category in control.Categories)
            {
                category.IsExpanded = true;
            }

            changeService?.OnComponentChanged(control, propertyDescriptor, null, null);
            control.Invalidate();
        }

        private void CollapseAll()
        {
            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            var propertyDescriptor = TypeDescriptor.GetProperties(control)["Categories"];

            changeService?.OnComponentChanging(control, propertyDescriptor);

            foreach (var category in control.Categories)
            {
                category.IsExpanded = false;
            }

            changeService?.OnComponentChanged(control, propertyDescriptor, null, null);
            control.Invalidate();
        }

        #endregion

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(control)[propertyName];
            if (property != null)
            {
                var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                changeService?.OnComponentChanging(control, property);
                property.SetValue(control, value);
                changeService?.OnComponentChanged(control, property, null, null);
            }
        }
    }


    #endregion
}
