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
                    var oldItem = selectedItem;
                    selectedItem = value;
                    OnSelectedItemChanged(new FluentToolListItemEventArgs(selectedItem));

                    if (!DesignMode)
                    {
                        EnsureVisible(selectedItem);
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

        #region 绘制方法

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
            // 背景色 - 优先使用自定义颜色
            var bgColor = categoryBackColor ?? GetThemeColor(
                c => c.BackgroundSecondary,
                SystemColors.ControlLight);

            var textColor = categoryForeColor ?? GetThemeColor(
                c => c.TextPrimary,
                SystemColors.ControlText);

            // 绘制背景
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 4;

            using (var path = GetRoundedRectangle(rect, cornerRadius))
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
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

            // 字体 - 优先使用自定义字体
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

            // 背景色 - 优先使用自定义颜色
            Color bgColor = Color.Transparent;
            if (isSelected)
            {
                bgColor = selectedItemBackColor ?? GetThemeColor(
                    c => c.SurfacePressed,
                    SystemColors.Highlight);
            }
            else if (isHover)
            {
                bgColor = hoverItemBackColor ?? GetThemeColor(
                    c => c.SurfaceHover,
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

            // 文本颜色 - 优先使用自定义颜色
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
                    c => c.TextPrimary,
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

            // 绘制图标(如果有)
            int contentLeft = rect.Left + 8;
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

            // 字体 - 优先使用自定义字体
            var font = itemFont ?? Font;

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
        }

        #endregion

        #region 交互处理

        protected override void OnMouseDown(MouseEventArgs e)
        {
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
                int delta = e.Delta / 120 * scrollBar.SmallChange;
                int newValue = scrollBar.Value - delta;
                newValue = Math.Max(scrollBar.Minimum, Math.Min(scrollBar.Maximum - scrollBar.LargeChange + 1, newValue));

                if (scrollBar.Value != newValue)
                {
                    scrollBar.Value = newValue;
                    scrollOffset = newValue;
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
            if (item == null || !scrollBar.Visible)
            {
                return;
            }

            var itemBounds = GetItemBounds(item);
            if (itemBounds == Rectangle.Empty)
            {
                return;
            }

            var visibleRect = GetVisibleRectangle();
            int itemTop = itemBounds.Top + scrollOffset;
            int itemBottom = itemBounds.Bottom + scrollOffset;

            if (itemTop < scrollOffset)
            {
                // 项目在可见区域上方
                scrollBar.Value = itemTop;
                scrollOffset = scrollBar.Value;
                Invalidate();
            }
            else if (itemBottom > scrollOffset + visibleRect.Height)
            {
                // 项目在可见区域下方
                int newValue = itemBottom - visibleRect.Height;
                scrollBar.Value = Math.Min(newValue, scrollBar.Maximum - scrollBar.LargeChange + 1);
                scrollOffset = scrollBar.Value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取项目边界
        /// </summary>
        private Rectangle GetItemBounds(FluentToolListItem item)
        {
            int top = itemSpacing;
            int left = itemSpacing;
            var visibleRect = GetVisibleRectangle();
            int right = visibleRect.Width - itemSpacing;

            foreach (var category in categories)
            {
                if (category == item)
                {
                    return new Rectangle(left, top, right - left, categoryHeight);
                }

                top += categoryHeight + itemSpacing;

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

                float delta = (target - current) * 0.2f; // 缓动系数

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

            if (needsUpdate || completedCategories.Count > 0)
            {
                UpdateScrollBar();
                Invalidate();
            }

            if (expandAnimations.Count == 0)
            {
                expandTimer.Stop();
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

                if (category.IsExpanded || expandAnimations.ContainsKey(category))
                {
                    float progress = expandAnimations.ContainsKey(category)
                        ? expandAnimations[category]
                        : 1.0f;

                    int itemsHeight = category.Items.Count * (itemHeight + itemSpacing);
                    total += (int)(itemsHeight * progress);
                }
            }

            return total;
        }

        /// <summary>
        /// 更新滚动条
        /// </summary>
        private void UpdateScrollBar()
        {
            var visibleRect = GetVisibleRectangle();
            int totalHeight = GetTotalHeight();

            if (totalHeight > visibleRect.Height)
            {
                scrollBar.Visible = true;
                scrollBar.Maximum = totalHeight - visibleRect.Height + scrollBar.LargeChange - 1;
                scrollBar.LargeChange = visibleRect.Height / 2;
                scrollBar.SmallChange = itemHeight;

                if (scrollOffset > scrollBar.Maximum - scrollBar.LargeChange + 1)
                {
                    scrollOffset = Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
                    scrollBar.Value = scrollOffset;
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

            UpdateScrollBar();
            Invalidate();
        }

        private void OnCategoryItemsChanged(object sender, CollectionChangeEventArgs e)
        {
            UpdateScrollBar();
            Invalidate();
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
