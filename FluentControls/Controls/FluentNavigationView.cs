using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{
    [DefaultEvent("ItemInvoked")]
    [DefaultProperty("MenuItems")]
    [Designer(typeof(FluentNavigationViewDesigner))]
    public class FluentNavigationView : FluentContainerBase
    {
        #region 字段

        private bool isExpanded = true;
        private bool showSearchBar = true;
        private bool showNavigationIcon = true;
        private bool showNavigationTitle = true;
        private NavigationViewDisplayMode displayMode = NavigationViewDisplayMode.Left;

        // 区域
        private Rectangle headerBounds;
        private Rectangle searchBounds;
        private Rectangle menuBounds;
        private Rectangle footerBounds;
        private Rectangle toggleButtonBounds;
        private Rectangle navIconBounds;

        // 集合
        private NavigationItemCollection menuItems;
        private NavigationItemCollection footerItems;

        // 配置
        private Image navigationIcon;
        private string navigationTitle = "Navigation";
        private int expandedWidth = 320;
        private int compactWidth = 48;
        private int footerItemCapacity = 3;
        private int headerHeight = 48;
        private int itemHeight = 40;
        private int iconSize = 16;
        private int menuItemSpacing = 0;
        private int footerItemSpacing = 0;

        // 当前选中项和悬停项
        private NavigationItem selectedItem;
        private NavigationItem hoveredItem;
        private NavigationCategory hoveredCategory;

        // 搜索 - 使用 FluentTextBox
        private FluentTextBox searchBox;
        private string searchText = string.Empty;
        private List<NavigationItem> filteredItems;

        // 动画
        private Timer expandTimer;
        private int currentWidth;
        private int targetWidth;
        private bool isAnimatingExpand;

        // 用于跟踪宽度变化
        private int lastWidth;

        #endregion

        #region 构造函数

        public FluentNavigationView()
        {
            menuItems = new NavigationItemCollection();
            footerItems = new NavigationItemCollection();
            filteredItems = new List<NavigationItem>();

            menuItems.CollectionChanged += (s, e) =>
            {
                UpdateLayout();
                Invalidate();
            };

            footerItems.CollectionChanged += (s, e) =>
            {
                UpdateLayout();
                Invalidate();
            };

            InitializeSearchBox();
            InitializeExpandAnimation();

            Size = new Size(expandedWidth, 600);
            currentWidth = expandedWidth;
            targetWidth = expandedWidth;
            lastWidth = expandedWidth;

            UpdateLayout();
        }

        private void InitializeSearchBox()
        {
            searchBox = new FluentTextBox
            {
                Visible = false,
                Placeholder = "搜索...",
                ShowBorder = true,
                BorderSize = 1,
                MinHeight = 32,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(8, 4, 8, 4)
            };

            searchBox.TextChanged += SearchBox_TextChanged;
            searchBox.GotFocus += (s, e) => Invalidate();
            searchBox.LostFocus += (s, e) => Invalidate();

            // 确保搜索框继承主题
            if (UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                searchBox.UseTheme = true;
                searchBox.ThemeName = ThemeName;
            }

            Controls.Add(searchBox);
        }

        private void InitializeExpandAnimation()
        {
            expandTimer = new Timer { Interval = 16 };
            expandTimer.Tick += ExpandTimer_Tick;
        }

        #endregion

        #region 属性

        [Category("Navigation")]
        [Description("导航栏显示位置")]
        [DefaultValue(NavigationViewDisplayMode.Left)]
        public NavigationViewDisplayMode DisplayMode
        {
            get => displayMode;
            set
            {
                if (displayMode != value)
                {
                    displayMode = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("是否展开导航栏")]
        [DefaultValue(true)]
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    AnimateExpand(value);
                    OnExpandedChanged(new NavigationViewExpandedEventArgs(value));
                }
            }
        }

        [Category("Navigation")]
        [Description("是否显示搜索栏")]
        [DefaultValue(true)]
        public bool ShowSearchBar
        {
            get => showSearchBar;
            set
            {
                if (showSearchBar != value)
                {
                    showSearchBar = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("是否显示导航图标")]
        [DefaultValue(true)]
        public bool ShowNavigationIcon
        {
            get => showNavigationIcon;
            set
            {
                if (showNavigationIcon != value)
                {
                    showNavigationIcon = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("是否显示导航标题")]
        [DefaultValue(true)]
        public bool ShowNavigationTitle
        {
            get => showNavigationTitle;
            set
            {
                if (showNavigationTitle != value)
                {
                    showNavigationTitle = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("导航图标")]
        [DefaultValue(null)]
        public Image NavigationIcon
        {
            get => navigationIcon;
            set
            {
                if (navigationIcon != value)
                {
                    navigationIcon = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("导航标题")]
        [DefaultValue("Navigation")]
        public string NavigationTitle
        {
            get => navigationTitle;
            set
            {
                if (navigationTitle != value)
                {
                    navigationTitle = value ?? string.Empty;
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("展开状态宽度")]
        [DefaultValue(320)]
        public int ExpandedWidth
        {
            get => expandedWidth;
            set
            {
                if (expandedWidth != value && value > compactWidth)
                {
                    expandedWidth = value;
                    if (isExpanded)
                    {
                        currentWidth = value;
                        targetWidth = value;
                        Width = value;
                        UpdateLayout();
                    }
                }
            }
        }

        [Category("Navigation")]
        [Description("紧凑状态宽度")]
        [DefaultValue(48)]
        public int CompactWidth
        {
            get => compactWidth;
            set
            {
                if (compactWidth != value && value < expandedWidth && value > 0)
                {
                    compactWidth = value;
                    if (!isExpanded)
                    {
                        currentWidth = value;
                        targetWidth = value;
                        Width = value;
                        UpdateLayout();
                    }
                }
            }
        }

        [Category("Navigation")]
        [Description("底部菜单项最大容量")]
        [DefaultValue(3)]
        public int FooterItemCapacity
        {
            get => footerItemCapacity;
            set
            {
                if (footerItemCapacity != value && value >= 0)
                {
                    footerItemCapacity = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("标题栏高度")]
        [DefaultValue(48)]
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

        [Category("Navigation")]
        [Description("菜单项高度")]
        [DefaultValue(40)]
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

        [Category("Navigation")]
        [Description("菜单项间距")]
        [DefaultValue(0)]
        public int MenuItemSpacing
        {
            get => menuItemSpacing;
            set
            {
                if (menuItemSpacing != value && value >= 0)
                {
                    menuItemSpacing = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("底部菜单项间距")]
        [DefaultValue(0)]
        public int FooterItemSpacing
        {
            get => footerItemSpacing;
            set
            {
                if (footerItemSpacing != value && value >= 0)
                {
                    footerItemSpacing = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("图标大小")]
        [DefaultValue(16)]
        public int IconSize
        {
            get => iconSize;
            set
            {
                if (iconSize != value && value > 0)
                {
                    iconSize = value;
                    Invalidate();
                }
            }
        }

        [Category("Navigation")]
        [Description("菜单项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(NavigationItemCollectionEditor), typeof(UITypeEditor))]
        public NavigationItemCollection MenuItems => menuItems;

        [Category("Navigation")]
        [Description("底部菜单项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FooterItemCollectionEditor), typeof(UITypeEditor))]
        public NavigationItemCollection FooterItems => footerItems;

        [Category("Navigation")]
        [Description("当前选中项")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NavigationItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    Invalidate();
                }
            }
        }

        // 重写 Width 属性以确保设计时也能正确更新
        public new int Width
        {
            get => base.Width;
            set
            {
                if (base.Width != value)
                {
                    int oldWidth = base.Width;
                    base.Width = value;

                    // 只在非动画状态下同步更新
                    if (!isAnimatingExpand)
                    {
                        currentWidth = value;
                        targetWidth = value;

                        // 只在手动修改宽度时同步 expandedWidth 或 compactWidth
                        if (value != expandedWidth && value != compactWidth)
                        {
                            if (isExpanded)
                            {
                                expandedWidth = value;
                            }
                            else
                            {
                                compactWidth = value;
                            }
                        }
                    }

                    if (oldWidth != value)
                    {
                        UpdateLayout();
                        Invalidate();
                    }
                }
            }
        }

        #endregion

        #region 事件

        [Category("Navigation")]
        [Description("导航项被点击时触发")]
        public event EventHandler<NavigationItemEventArgs> ItemInvoked;

        [Category("Navigation")]
        [Description("展开状态改变时触发")]
        public event EventHandler<NavigationViewExpandedEventArgs> ExpandedChanged;

        protected virtual void OnItemInvoked(NavigationItemEventArgs e)
        {
            ItemInvoked?.Invoke(this, e);
        }

        protected virtual void OnExpandedChanged(NavigationViewExpandedEventArgs e)
        {
            ExpandedChanged?.Invoke(this, e);
        }

        #endregion

        #region 布局管理

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 检测宽度是否改变
            if (Width != lastWidth)
            {
                lastWidth = Width;
                UpdateLayout();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateLayout();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);

            // 当边界改变时更新布局
            if ((specified & BoundsSpecified.Width) != 0 || (specified & BoundsSpecified.Height) != 0)
            {
                UpdateLayout();
            }
        }

        protected override void OnBoundsChanged()
        {
            base.OnBoundsChanged();
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            int width = Width; // 使用当前实际宽度
            int y = 0;

            // 标题区域
            headerBounds = new Rectangle(0, y, width, headerHeight);
            y += headerHeight;

            // 切换按钮位置
            int toggleSize = 32;
            if (isExpanded)
            {
                toggleButtonBounds = new Rectangle(width - toggleSize - 8, 8, toggleSize, toggleSize);
                if (showNavigationIcon && navigationIcon != null)
                {
                    navIconBounds = new Rectangle(8, 8, toggleSize, toggleSize);
                }
                else
                {
                    navIconBounds = Rectangle.Empty;
                }
            }
            else
            {
                // 折叠状态
                if (showNavigationIcon && navigationIcon != null)
                {
                    navIconBounds = new Rectangle((width - toggleSize) / 2, 8, toggleSize, toggleSize);
                    toggleButtonBounds = Rectangle.Empty; // 隐藏，点击navIcon也能展开
                }
                else
                {
                    toggleButtonBounds = new Rectangle((width - toggleSize) / 2, 8, toggleSize, toggleSize);
                    navIconBounds = Rectangle.Empty;
                }
            }

            // 搜索栏区域
            if (showSearchBar && isExpanded)
            {
                int searchHeight = 32;
                searchBounds = new Rectangle(8, y, width - 16, searchHeight);

                if (searchBox != null)
                {
                    searchBox.Bounds = searchBounds;
                    searchBox.Visible = true;
                }

                y += searchBounds.Height + 8;
            }
            else
            {
                searchBounds = Rectangle.Empty;
                if (searchBox != null)
                {
                    searchBox.Visible = false;
                }
                y += 8;
            }

            // 底部菜单区域高度
            int footerHeight = 0;
            if (footerItemCapacity > 0 && footerItems.Count > 0)
            {
                int visibleFooterCount = Math.Min(footerItemCapacity, footerItems.Count(item => item.IsVisible));
                // 考虑间距：n个项目有n-1个间距，再加上上下padding
                footerHeight = visibleFooterCount * itemHeight + Math.Max(0, visibleFooterCount - 1) * footerItemSpacing + 16;
            }

            // 菜单区域
            menuBounds = new Rectangle(0, y, width, Height - y - footerHeight);

            // 底部菜单区域
            if (footerHeight > 0)
            {
                footerBounds = new Rectangle(0, Height - footerHeight, width, footerHeight);
            }
            else
            {
                footerBounds = Rectangle.Empty;
            }

            // 更新菜单项位置
            UpdateMenuItemBounds();

            // 更新底部菜单项位置
            UpdateFooterItemBounds();
        }

        private void UpdateMenuItemBounds()
        {
            if (menuBounds.IsEmpty)
            {
                return;
            }

            int y = menuBounds.Y;
            var itemsToDisplay = string.IsNullOrEmpty(searchText)
                ? GetAllMenuItems()
                : filteredItems;

            foreach (var item in itemsToDisplay)
            {
                if (!item.IsVisible)
                {
                    continue;
                }

                if (item is NavigationCategory category)
                {
                    // 分类标题
                    category.Bounds = new Rectangle(menuBounds.X, y, menuBounds.Width, itemHeight);
                    y += itemHeight + menuItemSpacing; // 添加间距

                    // 分类子项
                    if (category.IsExpanded)
                    {
                        foreach (var subItem in category.Items)
                        {
                            if (!subItem.IsVisible)
                            {
                                continue;
                            }

                            // 折叠模式下只显示有图标的项
                            if (!isExpanded && subItem.Icon == null)
                            {
                                continue;
                            }

                            subItem.Bounds = new Rectangle(
                                menuBounds.X + (isExpanded ? 16 : 0),
                                y,
                                menuBounds.Width - (isExpanded ? 16 : 0),
                                itemHeight);
                            y += itemHeight + menuItemSpacing; // 添加间距

                            if (y > menuBounds.Bottom)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // 折叠模式下只显示有图标的项
                    if (!isExpanded && item.Icon == null)
                    {
                        continue;
                    }

                    item.Bounds = new Rectangle(menuBounds.X, y, menuBounds.Width, itemHeight);
                    y += itemHeight + menuItemSpacing; // 添加间距
                }

                if (y > menuBounds.Bottom)
                {
                    break;
                }
            }
        }

        private void UpdateFooterItemBounds()
        {
            if (footerBounds.IsEmpty || footerItems.Count == 0)
            {
                return;
            }

            int y = footerBounds.Y + 8;
            int count = 0;

            foreach (var item in footerItems)
            {
                if (!item.IsVisible || count >= footerItemCapacity)
                {
                    break;
                }

                // 折叠模式下只显示有图标的项
                if (!isExpanded && item.Icon == null)
                {
                    continue;
                }

                item.Bounds = new Rectangle(footerBounds.X, y, footerBounds.Width, itemHeight);
                y += itemHeight + footerItemSpacing; // 添加间距
                count++;
            }
        }

        private List<NavigationItem> GetAllMenuItems()
        {
            var allItems = new List<NavigationItem>();
            foreach (var item in menuItems)
            {
                allItems.Add(item);
            }
            return allItems;
        }


        private void RefreshWidth()
        {
            isAnimatingExpand = true;
            int width = isExpanded ? expandedWidth : compactWidth;
            base.Width = width;
            currentWidth = width;
            targetWidth = width;
            isAnimatingExpand = false;
            UpdateLayout();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var bgColor = GetThemeColor(c => c.Surface, SystemColors.Control);
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制标题区域
            DrawHeader(g);

            // FluentTextBox 会自己绘制，不需要在这里绘制

            // 绘制菜单项
            DrawMenuItems(g);

            // 绘制底部菜单
            if (!footerBounds.IsEmpty)
            {
                DrawFooterItems(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            using (var pen = new Pen(borderColor, 1))
            {
                int x = displayMode == NavigationViewDisplayMode.Left ? Width - 1 : 0;
                g.DrawLine(pen, x, 0, x, Height);
            }
        }

        private void DrawHeader(Graphics g)
        {
            // 绘制导航图标
            if (!navIconBounds.IsEmpty && navigationIcon != null)
            {
                var iconBounds = navIconBounds;
                bool isHovered = iconBounds.Contains(PointToClient(MousePosition));

                // 绘制悬停效果
                if (isHovered)
                {
                    var hoverColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
                    using (var brush = new SolidBrush(hoverColor))
                    using (var path = GetRoundedRectangle(iconBounds, 4))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // 绘制图标
                var imgRect = new Rectangle(
                    iconBounds.X + (iconBounds.Width - iconSize) / 2,
                    iconBounds.Y + (iconBounds.Height - iconSize) / 2,
                    iconSize, iconSize);
                g.DrawImage(navigationIcon, imgRect);
            }

            // 绘制标题
            if (isExpanded && showNavigationTitle && !string.IsNullOrEmpty(navigationTitle))
            {
                var textColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                var font = GetThemeFont(t => t.Title, Font);

                using (var brush = new SolidBrush(textColor))
                using (var format = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    int textX = navIconBounds.IsEmpty ? 16 : navIconBounds.Right + 8;
                    int textWidth = toggleButtonBounds.IsEmpty ?
                        headerBounds.Width - textX - 16 :
                        toggleButtonBounds.X - textX - 8;

                    var textRect = new Rectangle(textX, headerBounds.Y, textWidth, headerBounds.Height);
                    g.DrawString(navigationTitle, font, brush, textRect, format);
                }
            }

            // 绘制折叠/展开按钮
            if (!toggleButtonBounds.IsEmpty)
            {
                bool isHovered = toggleButtonBounds.Contains(PointToClient(MousePosition));

                // 绘制悬停效果
                if (isHovered)
                {
                    var hoverColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
                    using (var brush = new SolidBrush(hoverColor))
                    using (var path = GetRoundedRectangle(toggleButtonBounds, 4))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // 绘制图标(三横线)
                var iconColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                using (var pen = new Pen(iconColor, 2))
                {
                    int centerX = toggleButtonBounds.X + toggleButtonBounds.Width / 2;
                    int centerY = toggleButtonBounds.Y + toggleButtonBounds.Height / 2;
                    int lineWidth = 16;
                    int lineSpacing = 5;

                    for (int i = -1; i <= 1; i++)
                    {
                        int y = centerY + i * lineSpacing;
                        g.DrawLine(pen,
                            centerX - lineWidth / 2, y,
                            centerX + lineWidth / 2, y);
                    }
                }
            }

            // 绘制分隔线
            var separatorColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            using (var pen = new Pen(separatorColor, 1))
            {
                g.DrawLine(pen, 0, headerBounds.Bottom - 1, Width, headerBounds.Bottom - 1);
            }
        }

        private void DrawMenuItems(Graphics g)
        {
            if (menuBounds.IsEmpty)
            {
                return;
            }

            // 设置裁剪区域
            var oldClip = g.Clip;
            g.SetClip(menuBounds);

            var itemsToDisplay = string.IsNullOrEmpty(searchText)
                ? GetAllMenuItems()
                : filteredItems;

            foreach (var item in itemsToDisplay)
            {
                if (!item.IsVisible || item.Bounds.IsEmpty)
                {
                    continue;
                }

                if (!menuBounds.IntersectsWith(item.Bounds))
                {
                    continue;
                }

                // 折叠模式下只绘制有图标的项
                if (!isExpanded && item.Icon == null)
                {
                    continue;
                }

                if (item is NavigationCategory category)
                {
                    DrawCategoryItem(g, category);

                    if (category.IsExpanded)
                    {
                        foreach (var subItem in category.Items)
                        {
                            if (!subItem.IsVisible || subItem.Bounds.IsEmpty)
                            {
                                continue;
                            }

                            if (!isExpanded && subItem.Icon == null)
                            {
                                continue;
                            }

                            DrawMenuItem(g, subItem, true);
                        }
                    }
                }
                else
                {
                    DrawMenuItem(g, item, false);
                }
            }

            g.Clip = oldClip;
        }

        private void DrawMenuItem(Graphics g, NavigationItem item, bool isSubItem)
        {
            var bounds = item.Bounds;
            bool isSelected = item == selectedItem;
            bool isHovered = item == hoveredItem;

            // 绘制背景
            if (isSelected || isHovered)
            {
                var bgColor = isSelected
                    ? GetThemeColor(c => c.SurfacePressed, SystemColors.ControlDark)
                    : GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);

                using (var brush = new SolidBrush(bgColor))
                using (var path = GetRoundedRectangle(
                    new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4), 4))
                {
                    g.FillPath(brush, path);
                }
            }

            // 绘制选中指示器
            if (isSelected)
            {
                var accentColor = GetThemeColor(c => c.Accent, SystemColors.Highlight);
                using (var brush = new SolidBrush(accentColor))
                {
                    int indicatorX = displayMode == NavigationViewDisplayMode.Left ? 0 : bounds.Right - 3;
                    g.FillRectangle(brush, indicatorX, bounds.Y + 8, 3, bounds.Height - 16);
                }
            }

            int contentX = bounds.X + 8;

            // 绘制图标
            if (item.Icon != null)
            {
                var iconRect = new Rectangle(
                    contentX + (isExpanded && isSubItem ? 8 : 0),
                    bounds.Y + (bounds.Height - iconSize) / 2,
                    iconSize, iconSize);

                if (item.IsEnabled)
                {
                    g.DrawImage(item.Icon, iconRect);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(g, item.Icon,
                        iconRect.X, iconRect.Y, Color.Transparent);
                }

                contentX = iconRect.Right + 12;
            }
            else if (isExpanded && isSubItem)
            {
                contentX += 8;
            }

            // 绘制文本
            if (isExpanded && !string.IsNullOrEmpty(item.Text))
            {
                var textColor = item.IsEnabled
                    ? GetThemeColor(c => c.TextPrimary, SystemColors.ControlText)
                    : GetThemeColor(c => c.TextDisabled, SystemColors.GrayText);

                var font = GetThemeFont(t => t.Body, Font);

                using (var brush = new SolidBrush(textColor))
                using (var format = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    var textRect = new Rectangle(
                        contentX,
                        bounds.Y,
                        bounds.Right - contentX - 8,
                        bounds.Height);

                    g.DrawString(item.Text, font, brush, textRect, format);
                }
            }
        }

        private void DrawCategoryItem(Graphics g, NavigationCategory category)
        {
            var bounds = category.Bounds;
            bool isHovered = category == hoveredCategory;

            // 绘制悬停背景
            if (isHovered)
            {
                var hoverColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
                using (var brush = new SolidBrush(hoverColor))
                using (var path = GetRoundedRectangle(
                    new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4), 4))
                {
                    g.FillPath(brush, path);
                }
            }

            int contentX = bounds.X + 8;

            // 绘制展开/折叠图标
            if (isExpanded)
            {
                var iconColor = GetThemeColor(c => c.TextSecondary, SystemColors.GrayText);
                using (var brush = new SolidBrush(iconColor))
                {
                    var points = category.IsExpanded
                        ? new Point[] // 向下箭头
                        {
                            new Point(contentX + 4, bounds.Y + bounds.Height / 2 - 2),
                            new Point(contentX + 8, bounds.Y + bounds.Height / 2 + 2),
                            new Point(contentX + 12, bounds.Y + bounds.Height / 2 - 2)
                        }
                        : new Point[] // 向右箭头
                        {
                            new Point(contentX + 4, bounds.Y + bounds.Height / 2 - 4),
                            new Point(contentX + 10, bounds.Y + bounds.Height / 2),
                            new Point(contentX + 4, bounds.Y + bounds.Height / 2 + 4)
                        };

                    g.FillPolygon(brush, points);
                }

                contentX += 20;
            }

            // 绘制图标
            if (category.Icon != null)
            {
                var iconRect = new Rectangle(
                    contentX,
                    bounds.Y + (bounds.Height - iconSize) / 2,
                    iconSize, iconSize);

                g.DrawImage(category.Icon, iconRect);
                contentX = iconRect.Right + 12;
            }

            // 绘制文本
            if (isExpanded && !string.IsNullOrEmpty(category.Text))
            {
                var textColor = GetThemeColor(c => c.TextSecondary, SystemColors.GrayText);
                var font = GetThemeFont(t => t.Caption, new Font(Font.FontFamily, Font.Size - 1, FontStyle.Bold));

                using (var brush = new SolidBrush(textColor))
                using (var format = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    var textRect = new Rectangle(
                        contentX,
                        bounds.Y,
                        bounds.Right - contentX - 8,
                        bounds.Height);

                    g.DrawString(category.Text, font, brush, textRect, format);
                }
            }
        }

        private void DrawFooterItems(Graphics g)
        {
            // 绘制分隔线
            var separatorColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            using (var pen = new Pen(separatorColor, 1))
            {
                g.DrawLine(pen, 0, footerBounds.Y, Width, footerBounds.Y);
            }

            // 绘制底部菜单项
            int count = 0;
            foreach (var item in footerItems)
            {
                if (!item.IsVisible || count >= footerItemCapacity || item.Bounds.IsEmpty)
                {
                    continue;
                }

                if (!isExpanded && item.Icon == null)
                {
                    continue;
                }

                DrawMenuItem(g, item, false);
                count++;
            }
        }

        #endregion

        #region 鼠标事件处理

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            NavigationItem newHoveredItem = null;
            NavigationCategory newHoveredCategory = null;

            // 检查菜单项
            foreach (var item in GetAllMenuItems())
            {
                if (!item.IsVisible || item.Bounds.IsEmpty)
                {
                    continue;
                }

                if (item.Bounds.Contains(e.Location))
                {
                    if (item is NavigationCategory cat)
                    {
                        newHoveredCategory = cat;
                    }
                    else
                    {
                        newHoveredItem = item;
                    }

                    break;
                }

                if (item is NavigationCategory category && category.IsExpanded)
                {
                    foreach (var subItem in category.Items)
                    {
                        if (subItem.IsVisible && subItem.Bounds.Contains(e.Location))
                        {
                            newHoveredItem = subItem;
                            break;
                        }
                    }
                }
            }

            // 检查底部菜单项
            if (newHoveredItem == null && !footerBounds.IsEmpty)
            {
                foreach (var item in footerItems)
                {
                    if (item.IsVisible && item.Bounds.Contains(e.Location))
                    {
                        newHoveredItem = item;
                        break;
                    }
                }
            }

            if (newHoveredItem != hoveredItem || newHoveredCategory != hoveredCategory)
            {
                hoveredItem = newHoveredItem;
                hoveredCategory = newHoveredCategory;
                Invalidate();
            }

            // 设置光标
            bool overInteractive = newHoveredItem != null ||
                                  newHoveredCategory != null ||
                                  toggleButtonBounds.Contains(e.Location) ||
                                  navIconBounds.Contains(e.Location);
            Cursor = overInteractive ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredItem = null;
            hoveredCategory = null;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 点击折叠/展开按钮
            if (!toggleButtonBounds.IsEmpty && toggleButtonBounds.Contains(e.Location))
            {
                IsExpanded = !IsExpanded;
                return;
            }

            // 点击导航图标(仅在折叠状态下展开)
            if (!navIconBounds.IsEmpty && navIconBounds.Contains(e.Location))
            {
                if (!isExpanded)
                {
                    IsExpanded = true;
                }
                return;
            }

            // 点击分类项
            if (hoveredCategory != null && isExpanded)
            {
                hoveredCategory.IsExpanded = !hoveredCategory.IsExpanded;
                UpdateLayout();
                Invalidate();
                return;
            }

            // 点击菜单项
            if (hoveredItem != null && hoveredItem.IsEnabled)
            {
                SelectedItem = hoveredItem;

                if (hoveredItem is NavigationMenuItem menuItem)
                {
                    menuItem.OnClick(EventArgs.Empty);
                    OnItemInvoked(new NavigationItemEventArgs(menuItem));
                }
                else if (hoveredItem is NavigationFooterItem footerItem)
                {
                    OnItemInvoked(new NavigationItemEventArgs(footerItem));
                }

                Invalidate();
            }
        }

        #endregion

        #region 搜索功能

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            searchText = searchBox.Text?.Trim() ?? string.Empty;
            PerformSearch();
        }

        private void PerformSearch()
        {
            filteredItems.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                UpdateLayout();
                Invalidate();
                return;
            }

            string searchLower = searchText.ToLowerInvariant();

            foreach (var item in GetAllMenuItems())
            {
                if (MatchesSearch(item, searchLower))
                {
                    filteredItems.Add(item);
                }

                if (item is NavigationCategory category)
                {
                    foreach (var subItem in category.Items)
                    {
                        if (MatchesSearch(subItem, searchLower))
                        {
                            filteredItems.Add(subItem);
                        }
                    }
                }
            }

            UpdateLayout();
            Invalidate();
        }

        private bool MatchesSearch(NavigationItem item, string searchLower)
        {
            if (string.IsNullOrEmpty(item.Text))
            {
                return false;
            }

            // 文本匹配
            if (item.Text.ToLowerInvariant().Contains(searchLower))
            {
                return true;
            }

            // 搜索文本匹配(拼音等)
            if (!string.IsNullOrEmpty(item.SearchText) &&
                item.SearchText.ToLowerInvariant().Contains(searchLower))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region 展开动画

        private void AnimateExpand(bool expand)
        {
            targetWidth = expand ? expandedWidth : compactWidth;

            if (EnableAnimation)
            {
                isAnimatingExpand = true;
                expandTimer.Start();
            }
            else
            {
                isAnimatingExpand = true;
                currentWidth = targetWidth;
                base.Width = targetWidth;
                isAnimatingExpand = false;
                UpdateLayout();
                Invalidate();
            }
        }

        private void ExpandTimer_Tick(object sender, EventArgs e)
        {
            int step = Math.Max(1, Math.Abs(targetWidth - currentWidth) / 10);

            if (currentWidth < targetWidth)
            {
                currentWidth = Math.Min(currentWidth + step, targetWidth);
            }
            else if (currentWidth > targetWidth)
            {
                currentWidth = Math.Max(currentWidth - step, targetWidth);
            }

            base.Width = currentWidth;

            if (currentWidth == targetWidth)
            {
                expandTimer.Stop();
                isAnimatingExpand = false;
                UpdateLayout(); // 确保最终状态正确
            }
            else
            {
                UpdateLayout();
            }

            Invalidate();
        }

        #endregion

        #region 主题应用

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();

            // 同步搜索框主题
            if (searchBox != null && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                searchBox.UseTheme = true;
                searchBox.ThemeName = ThemeName;
            }
        }

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                BackColor = GetThemeColor(c => c.Surface, SystemColors.Control);
                ForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

                // 应用主题到搜索框
                if (searchBox != null)
                {
                    searchBox.InnerBackColor = GetThemeColor(c => c.Surface, SystemColors.Window);
                    searchBox.InnerTextColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                    searchBox.BorderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
                }
            }

            Invalidate();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                expandTimer?.Dispose();
                searchBox?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 导航项相关类

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class NavigationItem
    {
        private string text;
        private Image icon;
        private object tag;
        private bool isEnabled = true;
        private bool isVisible = true;
        private string searchText;

        [Category("Appearance")]
        [Description("显示文本")]
        [DefaultValue("")]
        public string Text
        {
            get => text;
            set => text = value;
        }

        [Category("Appearance")]
        [Description("图标")]
        [DefaultValue(null)]
        public Image Icon
        {
            get => icon;
            set => icon = value;
        }

        [Category("Data")]
        [Description("用户数据")]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        public object Tag
        {
            get => tag;
            set => tag = value;
        }

        [Category("Behavior")]
        [Description("是否启用")]
        [DefaultValue(true)]
        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        [Category("Behavior")]
        [Description("是否可见")]
        [DefaultValue(true)]
        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        [Category("Data")]
        [Description("搜索文本(支持拼音等)")]
        [DefaultValue("")]
        public string SearchText
        {
            get => searchText;
            set => searchText = value;
        }

        [Browsable(false)]
        internal Rectangle Bounds { get; set; }

        [Browsable(false)]
        internal bool IsHovered { get; set; }

        [Browsable(false)]
        internal bool IsPressed { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Text) ? GetType().Name : Text;
        }
    }

    /// <summary>
    /// 普通导航菜单项
    /// </summary>
    [ToolboxItem(false)]
    public class NavigationMenuItem : NavigationItem
    {
        [Browsable(false)]
        public event EventHandler Click;

        internal void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 导航分类项
    /// </summary>
    [ToolboxItem(false)]
    public class NavigationCategory : NavigationItem
    {
        private NavigationItemCollection items;
        private bool isExpanded = true;

        public NavigationCategory()
        {
            items = new NavigationItemCollection();
        }

        [Category("Behavior")]
        [Description("分类子项")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(CategoryItemCollectionEditor), typeof(UITypeEditor))]
        public NavigationItemCollection Items
        {
            get => items;
            set => items = value ?? new NavigationItemCollection();
        }

        [Category("Behavior")]
        [Description("是否展开")]
        [DefaultValue(true)]
        public bool IsExpanded
        {
            get => isExpanded;
            set => isExpanded = value;
        }
    }

    /// <summary>
    /// 底部菜单项
    /// </summary>
    [ToolboxItem(false)]
    public class NavigationFooterItem : NavigationItem
    {
        private Control customButton;

        [Category("Appearance")]
        [Description("自定义按钮控件")]
        [DefaultValue(null)]
        public Control CustomButton
        {
            get => customButton;
            set => customButton = value;
        }

        [Browsable(false)]
        public event EventHandler ButtonClick;

        internal void OnButtonClick(EventArgs e)
        {
            ButtonClick?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 导航项集合
    /// </summary>
    [Editor(typeof(NavigationItemCollectionEditor), typeof(UITypeEditor))]
    public class NavigationItemCollection : ObservableCollection<NavigationItem>
    {
        public NavigationItemCollection() : base()
        {
        }

        public NavigationItemCollection(IEnumerable<NavigationItem> items) : base(items)
        {
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 导航栏显示位置
    /// </summary>
    public enum NavigationViewDisplayMode
    {
        Left,
        Right
    }

    /// <summary>
    /// 导航栏展开模式
    /// </summary>
    public enum NavigationViewPaneDisplayMode
    {
        Auto,       // 自动
        Expanded,   // 完全展开
        Compact     // 紧凑模式
    }

    /// <summary>
    /// 导航项点击事件参数
    /// </summary>
    public class NavigationItemEventArgs : EventArgs
    {
        public NavigationItemEventArgs(NavigationItem item, bool isSettings = false)
        {
            Item = item;
            IsSettingsItem = isSettings;
        }

        public NavigationItem Item { get; set; }

        public bool IsSettingsItem { get; set; }
    }

    /// <summary>
    /// 导航栏展开状态变更事件参数
    /// </summary>
    public class NavigationViewExpandedEventArgs : EventArgs
    {
        public NavigationViewExpandedEventArgs(bool isExpanded)
        {
            IsExpanded = isExpanded;
        }

        public bool IsExpanded { get; set; }
    }

    #endregion

    #region 设计时支持

    public class FluentNavigationViewDesigner : ControlDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // 允许在设计时修改子控件
            if (Control is FluentNavigationView navView)
            {
                foreach (Control ctrl in navView.Controls)
                {
                    if (ctrl is FluentTextBox)
                    {
                        EnableDesignMode(ctrl, "searchBox");
                        break;
                    }
                }
            }
        }

        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            base.PreFilterProperties(properties);
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                return new DesignerVerbCollection(new[]
                {
                    new DesignerVerb("添加菜单项", (s, e) => AddMenuItem()),
                    new DesignerVerb("添加分类", (s, e) => AddCategory()),
                    new DesignerVerb("添加底部菜单项", (s, e) => AddFooterItem()),
                    new DesignerVerb("切换展开状态", (s, e) => ToggleExpanded())
                });
            }
        }

        private void AddMenuItem()
        {
            if (Control is FluentNavigationView navView)
            {
                var item = new NavigationMenuItem { Text = "New Menu Item" };
                navView.MenuItems.Add(item);

                RaiseComponentChanged(TypeDescriptor.GetProperties(navView)["MenuItems"], null, null);
            }
        }

        private void AddCategory()
        {
            if (Control is FluentNavigationView navView)
            {
                var category = new NavigationCategory { Text = "New Category" };
                navView.MenuItems.Add(category);

                RaiseComponentChanged(TypeDescriptor.GetProperties(navView)["MenuItems"], null, null);
            }
        }

        private void AddFooterItem()
        {
            if (Control is FluentNavigationView navView)
            {
                var item = new NavigationFooterItem { Text = "New Footer Item" };
                navView.FooterItems.Add(item);

                RaiseComponentChanged(TypeDescriptor.GetProperties(navView)["FooterItems"], null, null);
            }
        }

        private void ToggleExpanded()
        {
            if (Control is FluentNavigationView navView)
            {
                navView.IsExpanded = !navView.IsExpanded;
                RaiseComponentChanged(TypeDescriptor.GetProperties(navView)["IsExpanded"], null, null);
            }
        }

        private void RaiseComponentChanged(PropertyDescriptor property, object oldValue, object newValue)
        {
            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            changeService?.OnComponentChanged(Control, property, oldValue, newValue);
        }
    }

    /// <summary>
    /// 导航项集合编辑器
    /// </summary>
    public class NavigationItemCollectionEditor : CollectionEditor
    {
        public NavigationItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(NavigationMenuItem),
                typeof(NavigationCategory),
                typeof(NavigationFooterItem)
            };
        }

        protected override string GetDisplayText(object value)
        {
            if (value is NavigationItem item)
            {
                string typeName = value.GetType().Name;
                string text = string.IsNullOrEmpty(item.Text) ? "(unnamed)" : item.Text;
                return $"{text} ({typeName})";
            }
            return base.GetDisplayText(value);
        }

        protected override object CreateInstance(Type itemType)
        {
            object instance = base.CreateInstance(itemType);

            if (instance is NavigationMenuItem menuItem)
            {
                menuItem.Text = "Menu Item";
            }
            else if (instance is NavigationCategory category)
            {
                category.Text = "Category";
                category.IsExpanded = true;
            }
            else if (instance is NavigationFooterItem footerItem)
            {
                footerItem.Text = "Footer Item";
            }

            return instance;
        }
    }

    /// <summary>
    /// 底部菜单项集合编辑器
    /// </summary>
    public class FooterItemCollectionEditor : CollectionEditor
    {
        public FooterItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(NavigationFooterItem),
                typeof(NavigationMenuItem)
            };
        }

        protected override string GetDisplayText(object value)
        {
            if (value is NavigationItem item)
            {
                string typeName = value.GetType().Name;
                string text = string.IsNullOrEmpty(item.Text) ? "(unnamed)" : item.Text;
                return $"{text} ({typeName})";
            }
            return base.GetDisplayText(value);
        }

        protected override object CreateInstance(Type itemType)
        {
            object instance = base.CreateInstance(itemType);

            if (instance is NavigationFooterItem footerItem)
            {
                footerItem.Text = "Footer Item";
            }
            else if (instance is NavigationMenuItem menuItem)
            {
                menuItem.Text = "Menu Item";
            }

            return instance;
        }
    }

    /// <summary>
    /// 分类子项集合编辑器
    /// </summary>
    public class CategoryItemCollectionEditor : CollectionEditor
    {
        public CategoryItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(NavigationMenuItem)
            };
        }

        protected override string GetDisplayText(object value)
        {
            if (value is NavigationMenuItem item)
            {
                string text = string.IsNullOrEmpty(item.Text) ? "(unnamed)" : item.Text;
                return text;
            }
            return base.GetDisplayText(value);
        }

        protected override object CreateInstance(Type itemType)
        {
            var instance = base.CreateInstance(itemType) as NavigationMenuItem;
            if (instance != null)
            {
                instance.Text = "Sub Item";
            }
            return instance;
        }
    }

    #endregion

}
