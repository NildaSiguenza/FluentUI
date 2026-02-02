using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms.Design;
using System.Security.Permissions;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FluentControls.Controls
{

    [DefaultEvent("SelectedIndexChanged")]
    [DefaultProperty("TabPages")]
    [Designer(typeof(FluentTabControlDesigner))]
    public class FluentTabControl : FluentContainerBase
    {
        private Control currentParent = null;
        private bool isInitialized = false;

        private const int SCROLL_BUTTON_WIDTH = 30; // 标签页滚动按钮宽度
        private const int CLOSE_BUTTON_SIZE = 16; // 关闭按钮大小
        private const int CLOSE_BUTTON_MARGIN = 22;  // 关闭按钮占用的总宽度

        private FluentTabPageCollection tabPagesCollection;
        private int selectedIndex = -1;
        private int hoveredIndex = -1;
        private TabAlignment tabAlignment = TabAlignment.Top;
        private BorderStyle borderStyle = BorderStyle.FixedSingle;

        private Rectangle tabStripBounds;
        private List<Rectangle> tabBounds = new List<Rectangle>();
        private List<Rectangle> closeBounds = new List<Rectangle>();
        private Rectangle leftScrollButton;
        private Rectangle rightScrollButton;

        private int scrollOffset = 0;
        private int maxScroll = 0;
        private bool showScrollButtons = false;

        private bool showCloseButton = true;
        private bool animateTabSwitch = true;
        private int tabHeight = 32;
        private int tabMinWidth = 100;
        private int tabMaxWidth = 200;
        private Padding tabPadding = new Padding(10, 5, 10, 5);

        private Color? tabStripBackColor;
        private Color? defaultTabBackColor;
        private Color? defaultTabForeColor;
        private Color? selectedTabBackColor;
        private Color? selectedTabForeColor;
        private Color? hoverTabBackColor;

        private Font statusFont;
        private Color statusForeColor = Color.Gray;
        private Point statusOffset = new Point(10, 10);

        private Timer animationTimer;
        private float animationProgress = 0;
        private int animationSourceIndex = -1;
        private int animationTargetIndex = -1;

        #region 构造函数

        public FluentTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.Selectable, true);

            Size = new Size(400, 300);

            tabPagesCollection = new FluentTabPageCollection(this);

            InitializeAnimation();
        }

        private void InitializeAnimation()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 16; // 60 FPS
            animationTimer.Tick += OnAnimationTick;
        }

        #endregion

        #region 属性

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(TabPageCollectionEditor), typeof(UITypeEditor))]
        [Description("标签页集合")]
        public FluentTabPageCollection TabPages => tabPagesCollection;

        [Category("Fluent")]
        [DefaultValue(TabAlignment.Top)]
        [Description("标签位置")]
        public TabAlignment TabAlignment
        {
            get => tabAlignment;
            set
            {
                if (tabAlignment != value)
                {
                    tabAlignment = value;
                    RecalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(32)]
        [Description("标签高度")]
        public int TabHeight
        {
            get => tabHeight;
            set
            {
                if (tabHeight != value && value > 0)
                {
                    tabHeight = value;
                    RecalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(100)]
        [Description("标签最小宽度")]
        public int TabMinWidth
        {
            get => tabMinWidth;
            set
            {
                if (tabMinWidth != value && value > 0)
                {
                    tabMinWidth = value;
                    RecalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(200)]
        [Description("标签最大宽度")]
        public int TabMaxWidth
        {
            get => tabMaxWidth;
            set
            {
                if (tabMaxWidth != value && value > tabMinWidth)
                {
                    tabMaxWidth = value;
                    RecalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("标签内边距")]
        public Padding TabPadding
        {
            get => tabPadding;
            set
            {
                if (tabPadding != value)
                {
                    tabPadding = value;
                    RecalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("是否显示关闭按钮")]
        public bool ShowCloseButton
        {
            get => showCloseButton;
            set
            {
                if (showCloseButton != value)
                {
                    showCloseButton = value;
                    RecalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("是否启用切换动画")]
        public bool AnimateTabSwitch
        {
            get => animateTabSwitch;
            set => animateTabSwitch = value;
        }

        [Category("Colors")]
        [Description("标签栏背景色")]
        public Color TabStripBackColor
        {
            get => GetThemeColor(c => c.Background, tabStripBackColor ?? SystemColors.Control);
            set
            {
                tabStripBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("默认标签背景色")]
        public Color DefaultTabBackColor
        {
            get => GetThemeColor(c => c.Surface, defaultTabBackColor ?? SystemColors.Control);
            set
            {
                defaultTabBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("默认标签前景色")]
        public Color DefaultTabForeColor
        {
            get => GetThemeColor(c => c.TextPrimary, defaultTabForeColor ?? SystemColors.ControlText);
            set
            {
                defaultTabForeColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("选中标签背景色")]
        public Color SelectedTabBackColor
        {
            get => GetThemeColor(c => c.Primary, selectedTabBackColor ?? SystemColors.Highlight);
            set
            {
                selectedTabBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("选中标签前景色")]
        public Color SelectedTabForeColor
        {
            get => GetThemeColor(c => c.TextOnPrimary, selectedTabForeColor ?? SystemColors.HighlightText);
            set
            {
                selectedTabForeColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("悬停标签背景色")]
        public Color HoverTabBackColor
        {
            get => GetThemeColor(c => c.SurfaceHover, hoverTabBackColor ?? SystemColors.ControlLight);
            set
            {
                hoverTabBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(BorderStyle.FixedSingle)]
        [Description("控件边框样式")]
        public BorderStyle BorderStyle
        {
            get => borderStyle;
            set
            {
                if (borderStyle != value)
                {
                    borderStyle = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < -1 || value >= tabPagesCollection.Count)
                {
                    selectedIndex = -1;
                }

                if (selectedIndex != value)
                {
                    var oldIndex = selectedIndex;
                    selectedIndex = value;

                    // 重新计算布局以更新关闭按钮
                    RecalculateLayout();

                    if (animateTabSwitch && oldIndex >= 0 && value >= 0)
                    {
                        StartTabAnimation(oldIndex, value);
                    }

                    UpdateTabVisibility();
                    OnSelectedIndexChanged();
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public FluentTabPage SelectedTab
        {
            get => selectedIndex >= 0 && selectedIndex < tabPagesCollection.Count ? tabPagesCollection[selectedIndex] : null;
            set
            {
                var index = tabPagesCollection.IndexOf(value);
                if (index >= 0)
                {
                    SelectedIndex = index;
                }
            }
        }

        [Browsable(false)]
        public int TabCount => tabPagesCollection.Count;

        #endregion

        #region 事件

        public event EventHandler SelectedIndexChanged;
        public event EventHandler<TabPageEventArgs> TabPageClosing;
        public event EventHandler<TabPageEventArgs> TabPageClosed;

        protected virtual void OnSelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnTabPageClosing(TabPageEventArgs e)
        {
            TabPageClosing?.Invoke(this, e);
        }

        protected virtual void OnTabPageClosed(TabPageEventArgs e)
        {
            TabPageClosed?.Invoke(this, e);
        }

        #endregion

        #region 标签页管理

        public void AddTab(FluentTabPage tabPage)
        {
            if (tabPage == null)
            {
                return;
            }

            TabPages.Add(tabPage);
        }

        public void AddTab(string text)
        {
            var tabPage = new FluentTabPage { TabText = text };
            AddTab(tabPage);
        }

        public void RemoveTab(FluentTabPage tabPage)
        {
            TabPages.Remove(tabPage);
        }

        public void CloseTab(int index)
        {
            if (index < 0 || index >= tabPagesCollection.Count)
            {
                return;
            }

            var tabPage = tabPagesCollection[index];
            if (tabPage.State == TabPageState.Locked)
            {
                return;
            }

            RemoveTab(index);
        }

        /// <summary>
        /// 移除指定索引的标签页
        /// </summary>
        public void RemoveTab(int index)
        {
            if (index < 0 || index >= tabPagesCollection.Count)
            {
                return;
            }

            // 触发关闭前事件
            var tabPage = tabPagesCollection[index];
            var args = new TabPageEventArgs(tabPage, index);
            OnTabPageClosing(args);

            if (args.Cancel)
            {
                return;
            }

            // 移除标签页
            TabPages.RemoveAt(index);

            int newIndex = index; // 默认使用下一个标签的索引作为当前索引
            if (newIndex == tabPagesCollection.Count)
            {
                newIndex = index - 1;
            }

            if (newIndex >= 0 && tabPagesCollection[newIndex].State != TabPageState.Hidden)
            {
                SelectedIndex = -1;
                SelectedIndex = newIndex;
            }
            else
            {
                // 设置从后往前第一个可见的标签页
                for (int i = index - 1; i >= 0; i--)
                {
                    if (tabPagesCollection[i].State != TabPageState.Hidden)
                    {
                        selectedIndex = -1;
                        SelectedIndex = i;
                        break;
                    }
                }
            }

            // 触发关闭后事件
            OnTabPageClosed(args);
        }

        public void ShowTab(int index)
        {
            if (index >= 0 && index < tabPagesCollection.Count)
            {
                tabPagesCollection[index].State = TabPageState.Normal;
                RecalculateLayout();
                Invalidate();
            }
        }

        public void ShowTab(FluentTabPage tabPage)
        {
            bool flag = ContainsTab(tabPage);

            if (!flag)
            {
                AddTab(tabPage);
            }

            SelectedTab = tabPage;
        }

        public void HideTab(int index)
        {
            if (index >= 0 && index < tabPagesCollection.Count)
            {
                tabPagesCollection[index].State = TabPageState.Hidden;

                if (selectedIndex == index)
                {
                    // 找下一个可见的标签页
                    for (int i = 0; i < tabPagesCollection.Count; i++)
                    {
                        if (i != index && tabPagesCollection[i].State != TabPageState.Hidden)
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                }

                RecalculateLayout();
                Invalidate();
            }
        }

        public void HideTab(FluentTabPage tabPage)
        {
            bool flag = ContainsTab(tabPage);

            if (flag)
            {
                HideTab(TabPages.IndexOf(tabPage));
            }
        }

        public void LockTab(int index, bool locked)
        {
            if (index >= 0 && index < tabPagesCollection.Count)
            {
                tabPagesCollection[index].State = locked ? TabPageState.Locked : TabPageState.Normal;
                Invalidate();
            }
        }

        internal void OnTabPageStateChanged(FluentTabPage tabPage)
        {
            RecalculateLayout();
            UpdateTabVisibility();
            Invalidate();
        }

        private void UpdateTabVisibility()
        {
            for (int i = 0; i < tabPagesCollection.Count; i++)
            {
                var tabPage = tabPagesCollection[i];
                tabPage.Visible = (i == selectedIndex && tabPage.State != TabPageState.Hidden);
            }
        }

        public bool ContainsTab(FluentTabPage tabPage)
        {
            return TabPages.Contains(tabPage);
        }

        public bool ContainsTab(string text)
        {
            if (FindTab(text) != null)
            {
                return true;
            }
            return false;
        }

        public FluentTabPage FindTab(string text)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                if (TabPages[i].Text == text)
                {
                    return TabPages[i];
                }
            }
            return null;
        }

        #endregion

        #region  标签页列表事件处理

        internal void OnTabPageAdded(FluentTabPage tabPage)
        {
            if (tabPage == null)
            {
                return;
            }

            Controls.Add(tabPage);

            // 应用主题(如果启用了主题继承)
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                ApplyThemeToControl(tabPage, true);
            }

            // 应用默认样式
            if (!tabPage.CustomBackColor.HasValue)
            {
                tabPage.BackColor = Theme?.Colors?.Surface ?? SystemColors.Window;
            }

            if (!tabPage.CustomForeColor.HasValue)
            {
                tabPage.ForeColor = DefaultTabForeColor;
            }

            if (tabPage.CustomFont == null)
            {
                tabPage.Font = Font;
            }

            RecalculateLayout();

            if (selectedIndex == -1 && tabPage.State != TabPageState.Hidden)
            {
                SelectedIndex = tabPagesCollection.Count - 1;
            }
            else
            {
                UpdateTabVisibility();
            }

            Invalidate();
        }

        internal void OnTabPageRemoved(FluentTabPage tabPage, int index)
        {
            if (tabPage == null)
            {
                return;
            }

            Controls.Remove(tabPage);

            if (selectedIndex == index)
            {
                SelectedIndex = Math.Min(index, tabPagesCollection.Count - 1);
            }
            else if (selectedIndex > index)
            {
                selectedIndex--;
            }

            RecalculateLayout();
            Invalidate();
        }

        internal void OnTabPageInserted(FluentTabPage tabPage, int index)
        {
            if (tabPage == null)
            {
                return;
            }

            Controls.Add(tabPage);
            Controls.SetChildIndex(tabPage, index);

            // 应用主题(如果启用了主题继承)
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                ApplyThemeToControl(tabPage, true);
            }

            // 应用默认样式
            if (!tabPage.CustomBackColor.HasValue)
            {
                tabPage.BackColor = Theme?.Colors?.Surface ?? SystemColors.Window;
            }

            if (!tabPage.CustomForeColor.HasValue)
            {
                tabPage.ForeColor = DefaultTabForeColor;
            }

            if (tabPage.CustomFont == null)
            {
                tabPage.Font = Font;
            }

            if (selectedIndex >= index)
            {
                selectedIndex++;
            }

            RecalculateLayout();
            UpdateTabVisibility();
            Invalidate();
        }

        internal void OnTabPageReplaced(FluentTabPage oldPage, FluentTabPage newPage, int index)
        {
            if (oldPage != null)
            {
                Controls.Remove(oldPage);
            }

            if (newPage != null)
            {
                Controls.Add(newPage);
                Controls.SetChildIndex(newPage, index);

                // 应用主题(如果启用了主题继承)
                if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
                {
                    ApplyThemeToControl(newPage, true);
                }

                // 应用默认样式
                if (!newPage.CustomBackColor.HasValue)
                {
                    newPage.BackColor = Theme?.Colors?.Surface ?? SystemColors.Window;
                }

                if (!newPage.CustomForeColor.HasValue)
                {
                    newPage.ForeColor = DefaultTabForeColor;
                }

                if (newPage.CustomFont == null)
                {
                    newPage.Font = Font;
                }
            }

            RecalculateLayout();
            UpdateTabVisibility();
            Invalidate();
        }

        internal void OnTabPagesCleared(FluentTabPage[] oldPages)
        {
            foreach (var page in oldPages)
            {
                Controls.Remove(page);
            }

            selectedIndex = -1;
            hoveredIndex = -1;
            scrollOffset = 0;

            RecalculateLayout();
            Invalidate();
        }

        #endregion

        #region 布局计算

        private void RecalculateLayout()
        {
            if (!isInitialized)
            {
                return;
            }

            if (tabPagesCollection == null)
            {
                return;
            }

            Rectangle clientRect = ClientRectangle;
            if (clientRect.Width <= 0 || clientRect.Height <= 0)
            {
                return;
            }

            if (tabPagesCollection.Count == 0)
            {
                tabStripBounds = Rectangle.Empty;
                tabBounds.Clear();
                closeBounds.Clear();
                return;
            }

            tabBounds.Clear();
            closeBounds.Clear();

            int availableWidth = clientRect.Width;
            int availableHeight = clientRect.Height;

            // 计算标签栏区域
            switch (tabAlignment)
            {
                case TabAlignment.Top:
                    tabStripBounds = new Rectangle(0, 0, availableWidth, tabHeight);
                    break;
                case TabAlignment.Bottom:
                    tabStripBounds = new Rectangle(0, availableHeight - tabHeight, availableWidth, tabHeight);
                    break;
                case TabAlignment.Left:
                    // 左侧时, 宽度应该更大以容纳垂直文本
                    int leftWidth = Math.Max(tabHeight, 100); // 至少100像素宽
                    tabStripBounds = new Rectangle(0, 0, leftWidth, availableHeight);
                    break;
                case TabAlignment.Right:
                    // 右侧时, 宽度应该更大以容纳垂直文本
                    int rightWidth = Math.Max(tabHeight, 100); // 至少100像素宽
                    tabStripBounds = new Rectangle(availableWidth - rightWidth, 0, rightWidth, availableHeight);
                    break;
            }

            // 计算每个标签的位置
            CalculateTabBounds();

            // 更新标签页内容区域
            UpdateTabPageBounds();
        }

        private void CalculateTabBounds()
        {
            int tabItemWidth = 100;  // 默认宽度
            int tabItemHeight = tabHeight; // 默认高度(通常是32)

            // 初始位置
            int currentX = 0;
            int currentY = 0;

            // 为滚动按钮预留空间
            if (showScrollButtons)
            {
                if (tabAlignment == TabAlignment.Top || tabAlignment == TabAlignment.Bottom)
                {
                    currentX = SCROLL_BUTTON_WIDTH;
                }
                else
                {
                    currentY = SCROLL_BUTTON_WIDTH;
                }
            }

            for (int i = 0; i < tabPagesCollection.Count; i++)
            {
                var tabPage = tabPagesCollection[i];

                if (tabPage.State == TabPageState.Hidden)
                {
                    tabBounds.Add(Rectangle.Empty);
                    closeBounds.Add(Rectangle.Empty);
                    continue;
                }

                // 计算文本大小来确定标签宽度
                var textSize = TextRenderer.MeasureText(tabPage.Text, Font);
                bool needCloseButton = CanShowCloseButton(tabPage) && i == selectedIndex;
                int closeButtonSpace = needCloseButton ? CLOSE_BUTTON_MARGIN : 0;

                // 根据文本大小调整标签宽度
                tabItemWidth = Math.Min(tabMaxWidth, Math.Max(tabMinWidth,
                    textSize.Width + tabPadding.Left + tabPadding.Right + closeButtonSpace));

                Rectangle tabRect = Rectangle.Empty;
                Rectangle closeRect = Rectangle.Empty;

                switch (tabAlignment)
                {
                    case TabAlignment.Top:
                        // 顶部：从左到右横向排列
                        tabRect = new Rectangle(
                            currentX - scrollOffset,
                            0,
                            tabItemWidth,
                            tabItemHeight);
                        currentX += tabItemWidth;
                        break;

                    case TabAlignment.Bottom:
                        // 底部：从左到右横向排列, Y坐标在底部
                        tabRect = new Rectangle(
                            currentX - scrollOffset,
                            tabStripBounds.Y,  // 使用标签栏的Y坐标
                            tabItemWidth,
                            tabItemHeight);
                        currentX += tabItemWidth;
                        break;

                    case TabAlignment.Left:
                        // 左侧：从上到下纵向排列
                        tabRect = new Rectangle(
                            0,  // X坐标固定在左侧
                            currentY - scrollOffset,
                            tabStripBounds.Width,  // 使用标签栏的宽度
                            tabItemHeight);  // 高度与Top/Bottom一致
                        currentY += tabItemHeight + 2; // 添加一点间距
                        break;

                    case TabAlignment.Right:
                        // 右侧：从上到下纵向排列, X坐标在右侧
                        tabRect = new Rectangle(
                            tabStripBounds.X,  // 使用标签栏的X坐标(右侧)
                            currentY - scrollOffset,
                            tabStripBounds.Width,  // 使用标签栏的宽度
                            tabItemHeight);  // 高度与Top/Bottom一致
                        currentY += tabItemHeight + 2; // 添加一点间距
                        break;
                }

                // 计算关闭按钮位置(如果需要)
                if (needCloseButton)
                {
                    closeRect = new Rectangle(
                        tabRect.Right - CLOSE_BUTTON_SIZE - 4,
                        tabRect.Y + (tabRect.Height - CLOSE_BUTTON_SIZE) / 2,
                        CLOSE_BUTTON_SIZE,
                        CLOSE_BUTTON_SIZE);
                }

                tabBounds.Add(tabRect);
                closeBounds.Add(closeRect);
            }

            // 更新滚动相关
            UpdateScrollButtons();
        }

        private void UpdateScrollButtons()
        {
            int totalSize = 0;
            int availableSize = 0;

            switch (tabAlignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    // 水平方向：计算总宽度
                    if (tabBounds.Count > 0)
                    {
                        var lastTab = tabBounds.LastOrDefault(r => !r.IsEmpty);
                        if (!lastTab.IsEmpty)
                        {
                            totalSize = lastTab.Right + scrollOffset;
                        }
                    }
                    availableSize = ClientRectangle.Width;
                    break;

                case TabAlignment.Left:
                case TabAlignment.Right:
                    // 垂直方向：计算总高度
                    if (tabBounds.Count > 0)
                    {
                        var lastTab = tabBounds.LastOrDefault(r => !r.IsEmpty);
                        if (!lastTab.IsEmpty)
                        {
                            totalSize = lastTab.Bottom + scrollOffset;
                        }
                    }
                    availableSize = ClientRectangle.Height;
                    break;
            }

            // 判断是否需要滚动按钮
            showScrollButtons = totalSize > availableSize;

            if (showScrollButtons)
            {
                maxScroll = Math.Max(0, totalSize - availableSize + SCROLL_BUTTON_WIDTH * 2);

                switch (tabAlignment)
                {
                    case TabAlignment.Top:
                    case TabAlignment.Bottom:
                        leftScrollButton = new Rectangle(0, tabStripBounds.Y, SCROLL_BUTTON_WIDTH, tabHeight);
                        rightScrollButton = new Rectangle(ClientRectangle.Width - SCROLL_BUTTON_WIDTH, tabStripBounds.Y, SCROLL_BUTTON_WIDTH, tabHeight);
                        break;

                    case TabAlignment.Left:
                        leftScrollButton = new Rectangle(0, 0, tabStripBounds.Width, SCROLL_BUTTON_WIDTH);
                        rightScrollButton = new Rectangle(0, ClientRectangle.Height - SCROLL_BUTTON_WIDTH, tabStripBounds.Width, SCROLL_BUTTON_WIDTH);
                        break;

                    case TabAlignment.Right:
                        leftScrollButton = new Rectangle(tabStripBounds.X, 0, tabStripBounds.Width, SCROLL_BUTTON_WIDTH);
                        rightScrollButton = new Rectangle(tabStripBounds.X, ClientRectangle.Height - SCROLL_BUTTON_WIDTH, tabStripBounds.Width, SCROLL_BUTTON_WIDTH);
                        break;
                }
            }
            else
            {
                scrollOffset = 0;
                maxScroll = 0;
                leftScrollButton = Rectangle.Empty;
                rightScrollButton = Rectangle.Empty;
            }
        }

        /// <summary>
        /// 判断标签页是否可以显示关闭按钮
        /// </summary>
        private bool CanShowCloseButton(FluentTabPage tabPage)
        {
            if (tabPage == null)
            {
                return false;
            }

            // 检查标签页状态
            if (tabPage.State == TabPageState.Locked || tabPage.State == TabPageState.NoCloseButton)
            {
                return false;
            }

            // 检查标签页的单独设置
            if (!tabPage.ShowCloseButton)
            {
                return false;
            }

            // 检查TabControl的全局设置
            return showCloseButton;
        }

        /// <summary>
        /// 确保指定的标签页可见
        /// </summary>
        private void EnsureTabVisible(int index)
        {
            if (!showScrollButtons || index < 0 || index >= tabBounds.Count)
            {
                return;
            }

            var tabRect = tabBounds[index];
            if (tabRect.IsEmpty)
            {
                return;
            }

            int tabStart, tabEnd, viewStart, viewEnd;

            if (tabAlignment == TabAlignment.Top || tabAlignment == TabAlignment.Bottom)
            {
                tabStart = tabRect.Left + scrollOffset;
                tabEnd = tabRect.Right + scrollOffset;
                viewStart = SCROLL_BUTTON_WIDTH;
                viewEnd = Width - SCROLL_BUTTON_WIDTH;
            }
            else
            {
                tabStart = tabRect.Top + scrollOffset;
                tabEnd = tabRect.Bottom + scrollOffset;
                viewStart = SCROLL_BUTTON_WIDTH;
                viewEnd = Height - SCROLL_BUTTON_WIDTH;
            }

            // 如果标签页不完全可见, 调整滚动偏移
            if (tabStart < viewStart)
            {
                scrollOffset = Math.Max(0, scrollOffset - (viewStart - tabStart));
            }
            else if (tabEnd > viewEnd)
            {
                scrollOffset = Math.Min(maxScroll, scrollOffset + (tabEnd - viewEnd));
            }
        }

        /// <summary>
        /// 判断是否应该显示关闭按钮
        /// </summary>
        private bool ShouldShowCloseButton(int index)
        {
            if (index < 0 || index >= tabPagesCollection.Count)
            {
                return false;
            }

            var tabPage = tabPagesCollection[index];

            // 检查标签页状态
            if (tabPage.State == TabPageState.Locked || tabPage.State == TabPageState.NoCloseButton)
            {
                return false;
            }

            // 只有当前选中的标签页才显示关闭按钮
            if (index != selectedIndex)
            {
                return false;
            }

            // 检查标签页的单独设置(优先级最高)
            if (!tabPage.ShowCloseButton)
            {
                return false;
            }

            // 检查TabControl的全局设置
            return showCloseButton;
        }

        private void UpdateTabPageBounds()
        {
            Rectangle clientRect = ClientRectangle;
            if (clientRect.Width <= 0 || clientRect.Height <= 0)
            {
                return;
            }

            Rectangle contentBounds;

            switch (tabAlignment)
            {
                case TabAlignment.Top:
                    contentBounds = new Rectangle(0, tabHeight, clientRect.Width, clientRect.Height - tabHeight);
                    break;
                case TabAlignment.Bottom:
                    contentBounds = new Rectangle(0, 0, clientRect.Width, clientRect.Height - tabHeight);
                    break;
                case TabAlignment.Left:
                    contentBounds = new Rectangle(tabStripBounds.Width, 0,
                        clientRect.Width - tabStripBounds.Width, clientRect.Height);
                    break;
                case TabAlignment.Right:
                    contentBounds = new Rectangle(0, 0,
                        clientRect.Width - tabStripBounds.Width, clientRect.Height);
                    break;
                default:
                    contentBounds = clientRect;
                    break;
            }

            foreach (var tabPage in tabPagesCollection)
            {
                tabPage.Bounds = contentBounds;
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            RecalculateLayout();
            Invalidate();
        }

        protected override void OnBoundsChanged()
        {
            base.OnBoundsChanged();
            RecalculateLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (isInitialized)
            {
                RecalculateLayout();
                Invalidate();
            }

        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // 让基类处理 Dock 逻辑
            base.SetBoundsCore(x, y, width, height, specified);

            // 确保在大小改变后重新计算布局
            if (isInitialized && (specified & BoundsSpecified.Size) != 0)
            {
                // 强制更新内部布局
                BeginInvoke(new Action(() =>
                {
                    RecalculateLayout();
                    Invalidate();
                }));
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (currentParent != null)
            {
                currentParent.SizeChanged -= Parent_SizeChanged;
                currentParent.Resize -= Parent_Resize;
                currentParent.Layout -= Parent_Layout;
            }

            currentParent = Parent;
            if (currentParent != null)
            {
                currentParent.SizeChanged += Parent_SizeChanged;
                currentParent.Resize += Parent_Resize;
                currentParent.Layout += Parent_Layout;

                if (Dock != DockStyle.None)
                {
                    UpdateDockBounds();
                }
            }
        }

        protected override void OnDockChanged(EventArgs e)
        {
            base.OnDockChanged(e);
            if (Parent != null && isInitialized)
            {
                UpdateDockBounds();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (isInitialized)
            {
                RecalculateLayout();
                Invalidate();
            }
        }


        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!isInitialized)
            {
                isInitialized = true;

                if (Parent != null && Dock != DockStyle.None)
                {
                    UpdateDockBounds();
                }
                else
                {
                    RecalculateLayout();
                }
            }
        }

        private void Parent_SizeChanged(object sender, EventArgs e)
        {
            if (Dock != DockStyle.None)
            {
                UpdateDockBounds();
            }
        }


        private void Parent_Resize(object sender, EventArgs e)
        {
            if (Dock != DockStyle.None)
            {
                UpdateDockBounds();
            }
        }

        private void Parent_Layout(object sender, LayoutEventArgs e)
        {
            // 如果布局影响到本控件, 更新边界
            if (Dock != DockStyle.None && (e.AffectedControl == this || e.AffectedControl == null))
            {
                UpdateDockBounds();
            }
        }

        /// <summary>
        /// 更新 Dock 边界
        /// </summary>
        private void UpdateDockBounds()
        {
            if (Parent == null || !isInitialized)
            {
                return;
            }

            // 获取父容器的显示区域
            Rectangle parentClient = Parent.ClientRectangle;
            Rectangle newBounds = Rectangle.Empty;

            // 考虑父容器的 Padding
            if (Parent is Control parentControl)
            {
                Padding padding = parentControl.Padding;
                parentClient = new Rectangle(
                    padding.Left,
                    padding.Top,
                    parentClient.Width - padding.Horizontal,
                    parentClient.Height - padding.Vertical);
            }

            // 根据 Dock 设置计算新边界
            switch (Dock)
            {
                case DockStyle.Fill:
                    // Fill模式需要考虑同级控件的占用空间
                    newBounds = CalculateFillBounds(parentClient);
                    break;

                case DockStyle.Top:
                    newBounds = new Rectangle(
                        parentClient.Left,
                        parentClient.Top,
                        parentClient.Width,
                        Height);
                    break;

                case DockStyle.Bottom:
                    newBounds = new Rectangle(
                        parentClient.Left,
                        parentClient.Bottom - Height,
                        parentClient.Width,
                        Height);
                    break;

                case DockStyle.Left:
                    newBounds = new Rectangle(
                        parentClient.Left,
                        parentClient.Top,
                        Width,
                        parentClient.Height);
                    break;

                case DockStyle.Right:
                    newBounds = new Rectangle(
                        parentClient.Right - Width,
                        parentClient.Top,
                        Width,
                        parentClient.Height);
                    break;

                case DockStyle.None:
                    return;
            }

            // 如果边界发生变化, 更新控件
            if (Bounds != newBounds)
            {
                // 暂停布局
                SuspendLayout();

                try
                {
                    Bounds = newBounds;
                    RecalculateLayout();
                }
                finally
                {
                    ResumeLayout(true);
                }

                Invalidate(true);
            }
        }

        /// <summary>
        /// 计算Fill模式下的可用边界，排除同级Dock控件占用的空间
        /// </summary>
        private Rectangle CalculateFillBounds(Rectangle parentClient)
        {
            int left = parentClient.Left;
            int top = parentClient.Top;
            int right = parentClient.Right;
            int bottom = parentClient.Bottom;

            // 遍历同级控件，计算它们占用的空间
            foreach (Control sibling in Parent.Controls)
            {
                // 跳过自己
                if (sibling == this)
                    continue;

                // 只考虑可见的控件
                if (!sibling.Visible)
                    continue;

                // 根据兄弟控件的Dock属性调整可用空间
                switch (sibling.Dock)
                {
                    case DockStyle.Left:
                        // 左侧控件占用的空间，从左边界向右推
                        left = Math.Max(left, sibling.Right);
                        break;

                    case DockStyle.Right:
                        // 右侧控件占用的空间，从右边界向左推
                        right = Math.Min(right, sibling.Left);
                        break;

                    case DockStyle.Top:
                        // 顶部控件占用的空间，从上边界向下推
                        top = Math.Max(top, sibling.Bottom);
                        break;

                    case DockStyle.Bottom:
                        // 底部控件占用的空间，从下边界向上推
                        bottom = Math.Min(bottom, sibling.Top);
                        break;

                        // DockStyle.Fill 和 DockStyle.None 不影响计算
                }
            }

            // 计算剩余可用空间
            int width = Math.Max(0, right - left);
            int height = Math.Max(0, bottom - top);

            return new Rectangle(left, top, width, height);
        }

        #endregion

        #region 鼠标事件处理

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int newHoveredIndex = GetTabIndexAt(e.Location);
            if (newHoveredIndex != hoveredIndex)
            {
                hoveredIndex = newHoveredIndex;
                Invalidate();
            }

            // 更新鼠标光标
            bool onCloseButton = GetCloseButtonAt(e.Location) >= 0;
            bool onScrollButton = leftScrollButton.Contains(e.Location) || rightScrollButton.Contains(e.Location);

            if (onCloseButton || onScrollButton)
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

            if (hoveredIndex != -1)
            {
                hoveredIndex = -1;
                Invalidate();
            }

            Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 检查是否点击关闭按钮
            int closeIndex = GetCloseButtonAt(e.Location);
            if (closeIndex >= 0)
            {
                CloseTab(closeIndex);
                return;
            }

            // 检查是否点击滚动按钮
            if (showScrollButtons)
            {
                if (leftScrollButton.Contains(e.Location))
                {
                    ScrollLeft();
                    return;
                }
                if (rightScrollButton.Contains(e.Location))
                {
                    ScrollRight();
                    return;
                }
            }

            // 检查是否点击标签
            int tabIndex = GetTabIndexAt(e.Location);
            if (tabIndex >= 0 && tabPagesCollection[tabIndex].State != TabPageState.Hidden)
            {
                SelectedIndex = tabIndex;
            }
        }

        private int GetTabIndexAt(Point location)
        {
            if (!tabStripBounds.Contains(location))
            {
                return -1;
            }

            for (int i = 0; i < tabBounds.Count; i++)
            {
                if (tabPagesCollection[i].State != TabPageState.Hidden)
                {
                    // 创建实际的标签矩形(考虑标签栏位置)
                    Rectangle actualRect = tabBounds[i];

                    if (actualRect.Contains(location))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private int GetCloseButtonAt(Point location)
        {
            // 只检查当前选中的标签页的关闭按钮
            if (selectedIndex >= 0 && selectedIndex < closeBounds.Count)
            {
                if (ShouldShowCloseButton(selectedIndex) && closeBounds[selectedIndex].Contains(location))
                {
                    return selectedIndex;
                }
            }

            return -1;
        }

        private void ScrollLeft()
        {
            scrollOffset = Math.Max(0, scrollOffset - 50);
            RecalculateLayout();
            Invalidate();
        }

        private void ScrollRight()
        {
            scrollOffset = Math.Min(maxScroll, scrollOffset + 50);
            RecalculateLayout();
            Invalidate();
        }

        #endregion

        #region 动画

        private void StartTabAnimation(int fromIndex, int toIndex)
        {
            if (!animateTabSwitch || fromIndex == toIndex)
            {
                return;
            }

            animationSourceIndex = fromIndex;
            animationTargetIndex = toIndex;
            animationProgress = 0;
            animationTimer.Start();
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            animationProgress += 0.1f;

            if (animationProgress >= 1.0f)
            {
                animationProgress = 1.0f;
                animationTimer.Stop();
                animationSourceIndex = -1;
                animationTargetIndex = -1;
            }

            Invalidate();
        }

        #endregion

        #region 样式

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();

            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                foreach (FluentTabPage tabPage in tabPagesCollection)
                {
                    ApplyThemeToControl(tabPage, true);
                }
            }
        }

        protected override void InitializeDefaultStyles()
        {
            base.InitializeDefaultStyles();
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
        }

        protected override void ApplyThemeStyles()
        {
            if (!isInitialized || Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;
        }

        /// <summary>
        /// 手动刷新所有标签页的主题
        /// </summary>
        public void RefreshTabPagesTheme()
        {
            if (!EnableChildThemeInheritance || !UseTheme || string.IsNullOrEmpty(ThemeName))
            {
                return;
            }

            foreach (FluentTabPage tabPage in tabPagesCollection)
            {
                ApplyThemeToControl(tabPage, true);
            }

            Invalidate();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 绘制整体背景
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (tabPagesCollection.Count == 0)
            {
                return;
            }

            // 绘制标签栏背景
            using (var brush = new SolidBrush(TabStripBackColor))
            {
                g.FillRectangle(brush, tabStripBounds);
            }

            // 绘制标签
            for (int i = 0; i < tabPagesCollection.Count; i++)
            {
                if (tabPagesCollection[i].State != TabPageState.Hidden)
                {
                    DrawTab(g, i);
                }
            }

            // 绘制滚动按钮
            if (showScrollButtons)
            {
                DrawScrollButtons(g);
            }
        }


        private void DrawTab(Graphics g, int index)
        {
            if (index >= tabBounds.Count || tabBounds[index].IsEmpty)
            {
                return;
            }

            var tabPage = tabPagesCollection[index];
            var tabRect = tabBounds[index];
            bool isSelected = (index == selectedIndex);
            bool isHovered = (index == hoveredIndex);

            // 确定颜色
            Color backColor = DefaultTabBackColor;
            Color foreColor = DefaultTabForeColor;

            if (tabPage.TabBackColor.HasValue)
            {
                backColor = tabPage.TabBackColor.Value;
            }
            else if (isSelected)
            {
                backColor = SelectedTabBackColor;
            }
            else if (isHovered)
            {
                backColor = HoverTabBackColor;
            }

            if (tabPage.TabForeColor.HasValue)
            {
                foreColor = tabPage.TabForeColor.Value;
            }
            else if (isSelected)
            {
                foreColor = SelectedTabForeColor;
            }

            // 绘制标签背景
            using (var brush = new SolidBrush(backColor))
            {
                if (Theme?.Elevation?.CornerRadius > 0)
                {
                    using (var path = GetRoundedRectangle(tabRect, Theme.Elevation.CornerRadiusSmall))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    g.FillRectangle(brush, tabRect);
                }
            }

            // 绘制选中指示器
            if (isSelected)
            {
                using (var pen = new Pen(Theme?.Colors?.Primary ?? SystemColors.Highlight, 3))
                {
                    switch (tabAlignment)
                    {
                        case TabAlignment.Top:
                            g.DrawLine(pen, tabRect.Left, tabRect.Bottom - 1, tabRect.Right, tabRect.Bottom - 1);
                            break;
                        case TabAlignment.Bottom:
                            g.DrawLine(pen, tabRect.Left, tabRect.Top + 1, tabRect.Right, tabRect.Top + 1);
                            break;
                        case TabAlignment.Left:
                            g.DrawLine(pen, tabRect.Right - 1, tabRect.Top, tabRect.Right - 1, tabRect.Bottom);
                            break;
                        case TabAlignment.Right:
                            g.DrawLine(pen, tabRect.Left + 1, tabRect.Top, tabRect.Left + 1, tabRect.Bottom);
                            break;
                    }
                }
            }

            // 绘制文本
            Font drawFont = tabPage.CustomFont ?? Font;
            var textRect = new Rectangle(
                tabRect.X + tabPadding.Left,
                tabRect.Y + tabPadding.Top,
                tabRect.Width - tabPadding.Horizontal - (ShouldShowCloseButton(index) ? CLOSE_BUTTON_MARGIN : 0),
                tabRect.Height - tabPadding.Vertical);

            TextRenderer.DrawText(g, tabPage.TabText, drawFont, textRect, foreColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // 绘制关闭按钮
            if (ShouldShowCloseButton(index) && !closeBounds[index].IsEmpty)
            {
                DrawCloseButton(g, closeBounds[index], isHovered && GetCloseButtonAt(MousePosition) == index);
            }

            // 绘制锁定图标
            if (tabPage.State == TabPageState.Locked)
            {
                var lockRect = new Rectangle(
                    tabRect.Right - CLOSE_BUTTON_SIZE - 4,
                    tabRect.Y + (tabRect.Height - CLOSE_BUTTON_SIZE) / 2,
                    CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE);
                DrawLockIcon(g, lockRect);
            }
        }

        private Rectangle GetLockIconRect(Rectangle tabRect)
        {
            switch (tabAlignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    return new Rectangle(
                        tabRect.Right - CLOSE_BUTTON_MARGIN + 3,
                        tabRect.Y + (tabRect.Height - CLOSE_BUTTON_SIZE) / 2,
                        CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE);

                case TabAlignment.Left:
                case TabAlignment.Right:
                    return new Rectangle(
                        tabRect.X + (tabRect.Width - CLOSE_BUTTON_SIZE) / 2,
                        tabRect.Bottom - CLOSE_BUTTON_MARGIN + 3,
                        CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE);

                default:
                    return Rectangle.Empty;
            }
        }

        private GraphicsPath GetTopRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            int diameter = radius * 2;

            switch (tabAlignment)
            {
                case TabAlignment.Top:
                    // 左上圆角
                    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                    // 右上圆角
                    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                    // 右边
                    path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom);
                    // 底边
                    path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
                    // 左边
                    path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Y + radius);
                    break;

                case TabAlignment.Bottom:
                    // 顶边
                    path.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
                    // 右边
                    path.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom - radius);
                    // 右下圆角
                    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                    // 左下圆角
                    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                    // 左边
                    path.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Top);
                    break;

                default:
                    path.AddRectangle(rect);
                    break;
            }

            path.CloseFigure();
            return path;
        }

        private void DrawCloseButton(Graphics g, Rectangle bounds, bool isHovered)
        {
            if (isHovered)
            {
                using (var brush = new SolidBrush(Color.FromArgb(150, Color.Red)))
                {
                    g.FillEllipse(brush, bounds);
                }
            }

            using (var pen = new Pen(isHovered ? Color.Red : Color.IndianRed, 2))
            {
                int offset = 4;
                g.DrawLine(pen,
                    bounds.Left + offset, bounds.Top + offset,
                    bounds.Right - offset, bounds.Bottom - offset);
                g.DrawLine(pen,
                    bounds.Right - offset, bounds.Top + offset,
                    bounds.Left + offset, bounds.Bottom - offset);
            }
        }

        private void DrawLockIcon(Graphics g, Rectangle bounds)
        {
            using (var pen = new Pen(Color.Gray, 1.5f))
            {
                // 绘制锁身
                var bodyRect = new Rectangle(bounds.X + 3, bounds.Y + 7, 10, 7);
                g.DrawRectangle(pen, bodyRect);

                // 绘制锁环
                g.DrawArc(pen, bounds.X + 4, bounds.Y + 2, 8, 8, 0, 180);
            }
        }

        private void DrawScrollButtons(Graphics g)
        {
            // 绘制左/上滚动按钮
            bool canScrollLeft = scrollOffset > 0;
            DrawScrollButton(g, leftScrollButton, true, canScrollLeft);

            // 绘制右/下滚动按钮
            bool canScrollRight = scrollOffset < maxScroll;
            DrawScrollButton(g, rightScrollButton, false, canScrollRight);
        }

        private void DrawScrollButton(Graphics g, Rectangle bounds, bool isLeft, bool enabled)
        {
            if (bounds.IsEmpty)
            {
                return;
            }

            // 绘制按钮背景(使其更明显)
            using (var bgBrush = new SolidBrush(Color.FromArgb(240, TabStripBackColor)))
            {
                g.FillRectangle(bgBrush, bounds);
            }

            // 绘制边框
            using (var pen = new Pen(Theme?.Colors?.Border ?? SystemColors.ControlDark))
            {
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }

            // 绘制箭头(更大)
            using (var brush = new SolidBrush(enabled ? Color.FromArgb(200, Color.Black) : Color.FromArgb(100, Color.Gray)))
            {
                Point[] arrow;
                int centerX = bounds.X + bounds.Width / 2;
                int centerY = bounds.Y + bounds.Height / 2;
                int arrowSize = 8;  // 增大箭头尺寸

                if (tabAlignment == TabAlignment.Top || tabAlignment == TabAlignment.Bottom)
                {
                    if (isLeft)
                    {
                        arrow = new Point[]
                        {
                            new Point(centerX + arrowSize/2, centerY - arrowSize),
                            new Point(centerX - arrowSize/2, centerY),
                            new Point(centerX + arrowSize/2, centerY + arrowSize)
                        };
                    }
                    else
                    {
                        arrow = new Point[]
                        {
                            new Point(centerX - arrowSize/2, centerY - arrowSize),
                            new Point(centerX + arrowSize/2, centerY),
                            new Point(centerX - arrowSize/2, centerY + arrowSize)
                        };
                    }
                }
                else
                {
                    if (isLeft)
                    {
                        arrow = new Point[]
                        {
                            new Point(centerX - arrowSize, centerY + arrowSize/2),
                            new Point(centerX, centerY - arrowSize/2),
                            new Point(centerX + arrowSize, centerY + arrowSize/2)
                        };
                    }
                    else
                    {
                        arrow = new Point[]
                        {
                            new Point(centerX - arrowSize, centerY - arrowSize/2),
                            new Point(centerX, centerY + arrowSize/2),
                            new Point(centerX + arrowSize, centerY - arrowSize/2)
                        };
                    }
                }

                g.FillPolygon(brush, arrow);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (borderStyle == BorderStyle.None)
            {
                return;
            }

            Rectangle rect = ClientRectangle;

            switch (borderStyle)
            {
                case BorderStyle.FixedSingle:
                    using (var pen = new Pen(Theme?.Colors?.Border ?? SystemColors.ControlDark))
                    {
                        g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                    }
                    break;

                case BorderStyle.Fixed3D:
                    ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Sunken);
                    break;
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
                foreach (var tabPage in tabPagesCollection)
                {
                    tabPage?.Dispose();
                }

                if (currentParent != null)
                {
                    currentParent.SizeChanged -= Parent_SizeChanged;
                    currentParent.Resize -= Parent_Resize;
                    currentParent.Layout -= Parent_Layout;
                    currentParent = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 列表项

    /// <summary>
    /// 标签页
    /// </summary>
    [ToolboxItem(false)]
    [DefaultProperty("Text")]
    [DesignTimeVisible(false)]
    [Designer(typeof(FluentTabPageDesigner))]
    public class FluentTabPage : Panel
    {
        private string text = "TabPage";
        private TabPageState state = TabPageState.Normal;

        private Color? tabBackColor;      // 标签背景色
        private Color? tabForeColor;      // 标签前景色
        private Color? customBackColor;   // 内容背景色
        private Color? customForeColor;   // 内容前景色

        private Font customFont;
        private string statusText = "";
        private int statusDuration = 0;
        private Timer statusTimer;
        private DateTime statusShowTime;
        private bool isStatusVisible = false;
        private object tag;
        private bool showCloseButton = true;

        // 状态文本相关属性
        private Font statusFont;
        private Color statusForeColor = Color.FromArgb(100, 100, 100);
        private Color statusBackColor = Color.FromArgb(240, 240, 240);
        private Point statusOffset = new Point(10, 10);
        private ContentAlignment statusAlignment = ContentAlignment.BottomLeft;

        public FluentTabPage()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            Padding = new Padding(3);
        }

        [Category("Appearance")]
        [Description("标签页显示文本")]
        public string TabText
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    OnTextChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(TabPageState.Normal)]
        [Description("标签页状态")]
        public TabPageState State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OnStateChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("是否显示关闭按钮")]
        [DefaultValue(true)]
        public bool ShowCloseButton
        {
            get => showCloseButton;
            set
            {
                if (showCloseButton != value)
                {
                    showCloseButton = value;
                    if (Parent is FluentTabControl tabControl)
                    {
                        tabControl.Invalidate();
                    }
                }
            }
        }

        [Category("Appearance")]
        [Description("标签背景色")]
        public Color? TabBackColor
        {
            get => tabBackColor;
            set
            {
                tabBackColor = value;
                if (Parent is FluentTabControl tabControl)
                {
                    tabControl.Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("标签前景色")]
        public Color? TabForeColor
        {
            get => tabForeColor;
            set
            {
                tabForeColor = value;
                if (Parent is FluentTabControl tabControl)
                {
                    tabControl.Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("内容区域背景色")]
        public Color? CustomBackColor
        {
            get => customBackColor;
            set
            {
                customBackColor = value;
                if (value.HasValue)
                {
                    base.BackColor = value.Value;
                }

                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("内容区域前景色")]
        public Color? CustomForeColor
        {
            get => customForeColor;
            set
            {
                customForeColor = value;
                if (value.HasValue)
                {
                    base.ForeColor = value.Value;
                }

                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("自定义字体")]
        public Font CustomFont
        {
            get => customFont;
            set
            {
                customFont = value;
                if (value != null)
                {
                    base.Font = value;
                }

                Invalidate();
            }
        }

        [Category("Status")]
        [Description("状态文本")]
        public string StatusText
        {
            get => statusText;
            set => statusText = value ?? "";
        }

        [Category("Status")]
        [DefaultValue(3000)]
        [Description("状态显示持续时间(毫秒, 0表示一直显示)")]
        public int StatusDuration
        {
            get => statusDuration;
            set => statusDuration = Math.Max(0, value);
        }

        [Category("Status")]
        [Description("状态文本字体")]
        public Font StatusFont
        {
            get => statusFont ?? new Font(Font.FontFamily, Font.Size * 0.85f);
            set
            {
                statusFont = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本前景色")]
        public Color StatusForeColor
        {
            get => statusForeColor;
            set
            {
                statusForeColor = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本背景色")]
        public Color StatusBackColor
        {
            get => statusBackColor;
            set
            {
                statusBackColor = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本偏移")]
        public Point StatusOffset
        {
            get => statusOffset;
            set
            {
                statusOffset = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Category("Status")]
        [Description("状态文本对齐方式")]
        [DefaultValue(ContentAlignment.BottomLeft)]
        public ContentAlignment StatusAlignment
        {
            get => statusAlignment;
            set
            {
                statusAlignment = value;
                if (isStatusVisible)
                {
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public bool IsStatusVisible => isStatusVisible;

        [Browsable(false)]
        public new object Tag
        {
            get => tag;
            set => tag = value;
        }

        public event EventHandler StateChanged;

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);

            if (Parent is FluentTabControl tabControl)
            {
                tabControl.OnTabPageStateChanged(this);
            }
        }

        public void ShowStatus()
        {
            ShowStatus(statusText, statusDuration);
        }

        public void ShowStatus(string text, int duration = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            statusText = text;
            statusDuration = duration;
            isStatusVisible = true;
            statusShowTime = DateTime.Now;

            // 停止之前的定时器
            if (statusTimer != null)
            {
                statusTimer.Stop();
                statusTimer.Dispose();
                statusTimer = null;
            }

            // 如果设置了持续时间, 启动定时器
            if (duration > 0)
            {
                statusTimer = new Timer();
                statusTimer.Interval = duration;
                statusTimer.Tick += (s, e) =>
                {
                    HideStatus();
                };
                statusTimer.Start();
            }

            // 触发重绘
            Invalidate();
        }

        public void HideStatus()
        {
            isStatusVisible = false;

            if (statusTimer != null)
            {
                statusTimer.Stop();
                statusTimer.Dispose();
                statusTimer = null;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 如果需要显示状态文本
            if (isStatusVisible && !string.IsNullOrEmpty(statusText))
            {
                DrawStatusText(e.Graphics);
            }
        }

        private void DrawStatusText(Graphics g)
        {
            // 测量文本大小
            var font = StatusFont;
            var textSize = TextRenderer.MeasureText(statusText, font);

            // 计算状态文本位置
            Rectangle statusRect = CalculateStatusRectangle(textSize);

            // 创建圆角矩形路径
            using (GraphicsPath path = CreateRoundedRectangle(statusRect, 4))
            {
                // 绘制半透明背景
                using (var bgBrush = new SolidBrush(Color.FromArgb(200, statusBackColor)))
                {
                    g.FillPath(bgBrush, path);
                }

                // 绘制边框
                //using (var pen = new Pen(Color.FromArgb(100, statusForeColor), 1))
                //{
                //    g.DrawPath(pen, path);
                //}
            }

            // 绘制文本
            TextRenderer.DrawText(g, statusText, font, statusRect, statusForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private Rectangle CalculateStatusRectangle(Size textSize)
        {
            int padding = 8;
            int width = textSize.Width + padding * 2;
            int height = textSize.Height + padding;

            int x = 0, y = 0;

            // 根据对齐方式计算位置
            switch (statusAlignment)
            {
                case ContentAlignment.TopLeft:
                    x = statusOffset.X;
                    y = statusOffset.Y;
                    break;
                case ContentAlignment.TopCenter:
                    x = (Width - width) / 2;
                    y = statusOffset.Y;
                    break;
                case ContentAlignment.TopRight:
                    x = Width - width - statusOffset.X;
                    y = statusOffset.Y;
                    break;
                case ContentAlignment.MiddleLeft:
                    x = statusOffset.X;
                    y = (Height - height) / 2;
                    break;
                case ContentAlignment.MiddleCenter:
                    x = (Width - width) / 2;
                    y = (Height - height) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    x = Width - width - statusOffset.X;
                    y = (Height - height) / 2;
                    break;
                case ContentAlignment.BottomLeft:
                    x = statusOffset.X;
                    y = Height - height - statusOffset.Y;
                    break;
                case ContentAlignment.BottomCenter:
                    x = (Width - width) / 2;
                    y = Height - height - statusOffset.Y;
                    break;
                case ContentAlignment.BottomRight:
                    x = Width - width - statusOffset.X;
                    y = Height - height - statusOffset.Y;
                    break;
            }

            return new Rectangle(x, y, width, height);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            int diameter = radius * 2;

            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上角
            path.AddArc(arc, 180, 90);

            // 右上角
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // 右下角
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // 左下角
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                statusTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class TabPageEventArgs : EventArgs
    {
        public TabPageEventArgs(FluentTabPage tabPage, int index)
        {
            TabPage = tabPage;
            Index = index;
        }

        public FluentTabPage TabPage { get; }

        public int Index { get; }

        public bool Cancel { get; set; }

    }

    #endregion

    #region 列表项集合

    public class FluentTabPageCollection : IList<FluentTabPage>, IList
    {
        private readonly FluentTabControl owner;
        private readonly List<FluentTabPage> pages = new List<FluentTabPage>();

        public FluentTabPageCollection(FluentTabControl owner)
        {
            this.owner = owner;
        }

        public FluentTabPage this[int index]
        {
            get => pages[index];
            set
            {
                if (pages[index] != value)
                {
                    var oldPage = pages[index];
                    pages[index] = value;
                    owner.OnTabPageReplaced(oldPage, value, index);
                }
            }
        }

        public int Count => pages.Count;
        public bool IsReadOnly => false;

        public void Add(FluentTabPage item)
        {
            if (item == null)
            {
                return;
            }

            pages.Add(item);
            owner.OnTabPageAdded(item);
        }

        public void AddRange(IEnumerable<FluentTabPage> collection)
        {
            foreach (var page in collection)
            {
                Add(page);
            }
        }

        public void Clear()
        {
            var oldPages = pages.ToArray();
            pages.Clear();
            owner.OnTabPagesCleared(oldPages);
        }

        public bool Contains(FluentTabPage item) => pages.Contains(item);
        public void CopyTo(FluentTabPage[] array, int arrayIndex) => pages.CopyTo(array, arrayIndex);
        public IEnumerator<FluentTabPage> GetEnumerator() => pages.GetEnumerator();
        public int IndexOf(FluentTabPage item) => pages.IndexOf(item);

        public void Insert(int index, FluentTabPage item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            pages.Insert(index, item);
            owner.OnTabPageInserted(item, index);
        }

        public bool Remove(FluentTabPage item)
        {
            int index = pages.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < pages.Count)
            {
                var page = pages[index];
                pages.RemoveAt(index);
                owner.OnTabPageRemoved(page, index);
            }
        }

        #region IList 实现

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value)
        {
            if (value is FluentTabPage page)
            {
                Add(page);
                return pages.Count - 1;
            }
            throw new ArgumentException("Value must be FluentTabPage", nameof(value));
        }

        bool IList.Contains(object value) => value is FluentTabPage page && Contains(page);
        int IList.IndexOf(object value) => value is FluentTabPage page ? IndexOf(page) : -1;

        void IList.Insert(int index, object value)
        {
            if (value is FluentTabPage page)
            {
                Insert(index, page);
            }
            else
            {
                throw new ArgumentException("Value must be FluentTabPage", nameof(value));
            }
        }

        void IList.Remove(object value)
        {
            if (value is FluentTabPage page)
            {
                Remove(page);
            }
        }

        bool IList.IsFixedSize => false;

        public object SyncRoot => pages;

        public bool IsSynchronized => false;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)pages).CopyTo(array, index);
        }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is FluentTabPage page)
                {
                    this[index] = page;
                }
                else
                {
                    throw new ArgumentException("Value must be FluentTabPage");
                }
            }
        }

        #endregion
    }

    #endregion

    #region 设计器

    /// <summary>
    /// 标签页集合编辑器
    /// </summary>
    public class TabPageCollectionEditor : CollectionEditor
    {
        public TabPageCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentTabPage);
        }

        protected override object CreateInstance(Type itemType)
        {
            FluentTabPage page = (FluentTabPage)base.CreateInstance(itemType);
            page.TabText = "TabPage" + (Context.Instance as FluentTabPageCollection)?.Count;
            return page;
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentTabPage page)
            {
                return $"{page.TabText} [{page.State}]";
            }
            return base.GetDisplayText(value);
        }
    }

    /// <summary>
    /// 标签页设计器
    /// </summary>
    public class FluentTabPageDesigner : ScrollableControlDesigner
    {
        public override SelectionRules SelectionRules
        {
            get
            {
                // 禁用调整大小和移动
                return SelectionRules.Visible;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 移除 Dock 属性
            string[] removeProperties = { "Dock", "Anchor", "Location", "Size" };
            foreach (string prop in removeProperties)
            {
                if (properties.Contains(prop))
                {
                    properties.Remove(prop);
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                // 不提供默认的设计器操作
                return new DesignerActionListCollection();
            }
        }
    }

    /// <summary>
    /// FluentTabControl 设计器
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class FluentTabControlDesigner : ParentControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private ISelectionService selectionService;
        private FluentTabControl tabControl;
        private DesignerVerbCollection verbs;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            tabControl = component as FluentTabControl;
            if (tabControl == null)
            {
                return;
            }

            selectionService = GetService(typeof(ISelectionService)) as ISelectionService;

            // 启用设计时选择
            //EnableDesignMode(tabControl, "TabControl");

            // 为每个现有的标签页启用设计模式
            foreach (FluentTabPage page in tabControl.TabPages)
            {
                EnableDesignMode(page, page.Name);
            }

            // 订阅选择变化事件
            if (selectionService != null)
            {
                selectionService.SelectionChanged += OnSelectionChanged;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (selectionService != null)
                {
                    selectionService.SelectionChanged -= OnSelectionChanged;
                }
            }
            base.Dispose(disposing);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (selectionService != null && tabControl != null)
            {
                var selected = selectionService.GetSelectedComponents();
                foreach (object obj in selected)
                {
                    if (obj is FluentTabPage page)
                    {
                        int index = tabControl.TabPages.IndexOf(page);
                        if (index >= 0 && tabControl.SelectedIndex != index)
                        {
                            tabControl.SelectedIndex = index;
                        }
                    }
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentTabControlActionList(Component));
                }
                return actionLists;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (verbs == null)
                {
                    verbs = new DesignerVerbCollection();
                    verbs.Add(new DesignerVerb("Add Tab", OnAddTab));
                    verbs.Add(new DesignerVerb("Remove Tab", OnRemoveTab));
                }
                return verbs;
            }
        }

        private void OnAddTab(object sender, EventArgs e)
        {
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null && tabControl != null)
            {
                using (DesignerTransaction transaction = host.CreateTransaction("Add Tab"))
                {
                    try
                    {
                        // 触发组件更改事件
                        IComponentChangeService changeService = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                        changeService?.OnComponentChanging(tabControl,
                            TypeDescriptor.GetProperties(tabControl)["TabPages"]);

                        FluentTabPage page = host.CreateComponent(typeof(FluentTabPage)) as FluentTabPage;
                        if (page != null)
                        {
                            page.TabText = "TabPage" + (tabControl.TabPages.Count + 1);
                            page.Name = "tabPage" + (tabControl.TabPages.Count + 1);
                            tabControl.TabPages.Add(page);
                            EnableDesignMode(page, page.Name);
                            tabControl.SelectedTab = page;

                            changeService?.OnComponentChanged(tabControl,
                                TypeDescriptor.GetProperties(tabControl)["TabPages"],
                                null, null);
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

        private void OnRemoveTab(object sender, EventArgs e)
        {
            if (tabControl != null && tabControl.SelectedTab != null)
            {
                IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    using (DesignerTransaction transaction = host.CreateTransaction("Remove Tab"))
                    {
                        try
                        {
                            IComponentChangeService changeService = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            changeService?.OnComponentChanging(tabControl,
                                TypeDescriptor.GetProperties(tabControl)["TabPages"]);

                            FluentTabPage page = tabControl.SelectedTab;
                            tabControl.TabPages.Remove(page);
                            host.DestroyComponent(page);

                            changeService?.OnComponentChanged(tabControl,
                                TypeDescriptor.GetProperties(tabControl)["TabPages"],
                                null, null);

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
        }

        protected override bool GetHitTest(Point point)
        {
            if (tabControl == null)
            {
                return false;
            }

            // 转换为客户端坐标
            Point clientPoint = tabControl.PointToClient(point);

            // 允许点击标签进行切换
            Rectangle tabStripBounds = GetTabStripBounds();
            if (tabStripBounds.Contains(clientPoint))
            {
                return true;
            }

            return false;
        }

        private Rectangle GetTabStripBounds()
        {
            if (tabControl == null)
            {
                return Rectangle.Empty;
            }

            switch (tabControl.TabAlignment)
            {
                case TabAlignment.Top:
                    return new Rectangle(0, 0, tabControl.Width, tabControl.TabHeight);
                case TabAlignment.Bottom:
                    return new Rectangle(0, tabControl.Height - tabControl.TabHeight, tabControl.Width, tabControl.TabHeight);
                case TabAlignment.Left:
                    return new Rectangle(0, 0, tabControl.TabHeight, tabControl.Height);
                case TabAlignment.Right:
                    return new Rectangle(tabControl.Width - tabControl.TabHeight, 0, tabControl.TabHeight, tabControl.Height);
                default:
                    return Rectangle.Empty;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 隐藏一些不需要的属性
            string[] hiddenProperties = { "BackgroundImage", "BackgroundImageLayout" };
            foreach (string prop in hiddenProperties)
            {
                if (properties.Contains(prop))
                {
                    properties[prop] = TypeDescriptor.CreateProperty(
                        typeof(FluentTabControlDesigner),
                        (PropertyDescriptor)properties[prop],
                        new BrowsableAttribute(false));
                }
            }
        }
    }

    /// <summary>
    /// FluentTabControl 设计器操作列表
    /// </summary>
    public class FluentTabControlActionList : DesignerActionList
    {
        private FluentTabControl tabControl;
        private DesignerActionUIService designerActionUIService;

        public FluentTabControlActionList(IComponent component) : base(component)
        {
            tabControl = component as FluentTabControl;
            designerActionUIService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public void EditTabPages()
        {
            // 获取属性描述符
            PropertyDescriptor pd = TypeDescriptor.GetProperties(tabControl)["TabPages"];
            if (pd != null)
            {
                // 获取编辑器
                UITypeEditor editor = pd.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (editor != null)
                {
                    // 获取设计器宿主
                    IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (host != null)
                    {
                        // 创建类型描述符上下文
                        TypeDescriptorContext context = new TypeDescriptorContext(tabControl, pd, host);

                        // 编辑值
                        object currentValue = pd.GetValue(tabControl);
                        object newValue = editor.EditValue(context, context, currentValue);

                        // 如果值有变化, 应用更改
                        if (newValue != currentValue)
                        {
                            pd.SetValue(tabControl, newValue);
                        }

                        // 刷新设计器
                        if (designerActionUIService != null)
                        {
                            designerActionUIService.Refresh(Component);
                        }
                    }
                }
            }
        }

        public void AddTab()
        {
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                using (DesignerTransaction transaction = host.CreateTransaction("Add Tab"))
                {
                    try
                    {
                        FluentTabPage page = host.CreateComponent(typeof(FluentTabPage)) as FluentTabPage;
                        if (page != null)
                        {
                            page.TabText = "TabPage" + (tabControl.TabPages.Count + 1);
                            page.Name = "tabPage" + (tabControl.TabPages.Count + 1);
                            tabControl.TabPages.Add(page);
                            tabControl.SelectedTab = page;

                            // 触发组件更改事件
                            IComponentChangeService changeService = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            changeService?.OnComponentChanged(tabControl,
                                TypeDescriptor.GetProperties(tabControl)["TabPages"],
                                null, null);
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

        public void RemoveTab()
        {
            if (tabControl.SelectedTab != null)
            {
                IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    using (DesignerTransaction transaction = host.CreateTransaction("Remove Tab"))
                    {
                        try
                        {
                            FluentTabPage page = tabControl.SelectedTab;

                            // 触发组件更改事件
                            IComponentChangeService changeService = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            changeService?.OnComponentChanging(tabControl,
                                TypeDescriptor.GetProperties(tabControl)["TabPages"]);

                            tabControl.TabPages.Remove(page);
                            host.DestroyComponent(page);

                            changeService?.OnComponentChanged(tabControl,
                                TypeDescriptor.GetProperties(tabControl)["TabPages"],
                                null, null);

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
        }

        public DockStyle Dock
        {
            get => tabControl.Dock;
            set => SetProperty("Dock", value);
        }

        public BorderStyle BorderStyle
        {
            get => tabControl.BorderStyle;
            set => SetProperty("BorderStyle", value);
        }

        public TabAlignment TabAlignment
        {
            get => tabControl.TabAlignment;
            set => SetProperty("TabAlignment", value);
        }

        public bool ShowCloseButton
        {
            get => tabControl.ShowCloseButton;
            set => SetProperty("ShowCloseButton", value);
        }

        public bool AnimateTabSwitch
        {
            get => tabControl.AnimateTabSwitch;
            set => SetProperty("AnimateTabSwitch", value);
        }

        public Color TabStripBackColor
        {
            get => tabControl.TabStripBackColor;
            set => SetProperty("TabStripBackColor", value);
        }

        public Color DefaultTabBackColor
        {
            get => tabControl.DefaultTabBackColor;
            set => SetProperty("DefaultTabBackColor", value);
        }

        public Color SelectedTabBackColor
        {
            get => tabControl.SelectedTabBackColor;
            set => SetProperty("SelectedTabBackColor", value);
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(tabControl)[propertyName];
            property.SetValue(tabControl, value);
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();

            // 标签页管理
            items.Add(new DesignerActionHeaderItem("标签页管理"));
            items.Add(new DesignerActionMethodItem(this, "AddTab", "添加标签页", "标签页管理", "Add a new tab page", true));
            items.Add(new DesignerActionMethodItem(this, "RemoveTab", "移除选中标签页", "标签页管理", "Remove the selected tab page", true));
            items.Add(new DesignerActionMethodItem(this, "EditTabPages", "编辑标签页...", "标签页管理", "Open collection editor", true));

            // 布局
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Dock", "Dock", "布局", "设置控件Docking模式"));
            items.Add(new DesignerActionPropertyItem("TabAlignment", "标签页对齐方式", "布局", "设置标签页对齐方式"));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("BorderStyle", "Border样式", "Common Properties", "设置Border样式"));
            items.Add(new DesignerActionPropertyItem("ShowCloseButton", "显示关闭按钮", "Appearance", "是否在标签页上显示关闭按钮"));
            items.Add(new DesignerActionPropertyItem("AnimateTabSwitch", "显示动画", "Appearance", "是否显示切换动画"));

            // 颜色
            items.Add(new DesignerActionHeaderItem("颜色"));
            items.Add(new DesignerActionPropertyItem("TabStripBackColor", "标签条背景色", "颜色", "设置标签条背景色"));
            items.Add(new DesignerActionPropertyItem("DefaultTabBackColor", "默认标签背景色", "颜色", "设置默认标签背景色"));
            items.Add(new DesignerActionPropertyItem("SelectedTabBackColor", "选中标签背景色", "颜色", "设置选中标签背景色"));

            return items;
        }
    }

    #endregion

    /// <summary>
    /// 标签页状态
    /// </summary>
    public enum TabPageState
    {
        Normal,     // 正常
        Hidden,     // 隐藏
        Locked,     // 锁定
        NoCloseButton
    }

    /// <summary>
    /// 标签页位置
    /// </summary>
    public enum TabAlignment
    {
        Top,
        Bottom,
        Left,
        Right
    }

}
