using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Animation;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentPanelListDesigner))]
    [DefaultProperty("Panels")]
    public class FluentPanelList : FluentControlBase
    {

        private FluentPanelListItemCollection panels;
        private VScrollBar scrollBar;
        private int scrollOffset = 0;
        private int panelSpacing = 5;

        private Dictionary<FluentPanelListItem, AnimationState> expandAnimations;
        private Timer animationTimer;

        private bool isUpdatingLayout = false;
        private bool isInitializing = true;

        // 记录面板原始索引
        private Dictionary<FluentPanelListItem, int> originalIndices = new Dictionary<FluentPanelListItem, int>();
        private FluentPanelListItem highlightedPanel = null;

        #region 构造函数

        public FluentPanelList()
        {
            isInitializing = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);

            panels = new FluentPanelListItemCollection(this);
            expandAnimations = new Dictionary<FluentPanelListItem, AnimationState>();

            InitializeScrollBar();
            InitializeAnimationTimer();
            AnimationDuration = 200;

            base.Size = new Size(250, 500);

            // 最后订阅事件
            panels.ItemChanged += OnPanelsCollectionChanged;

            isInitializing = false;
        }

        private void InitializeScrollBar()
        {
            scrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            scrollBar.Scroll += OnScrollBarScroll;
            Controls.Add(scrollBar);
        }

        private void InitializeAnimationTimer()
        {
            animationTimer = new Timer
            {
                Interval = 16    // 60 FPS
            };
            animationTimer.Tick += OnAnimationTimerTick;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 面板集合
        /// </summary>
        [Category("Fluent")]
        [Description("面板列表中的面板集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentPanelListItemCollectionEditor), typeof(UITypeEditor))]
        public FluentPanelListItemCollection Panels => panels;

        /// <summary>
        /// 面板间距
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(5)]
        [Description("面板之间的间距")]
        public int PanelSpacing
        {
            get => panelSpacing;
            set
            {
                if (panelSpacing != value && value >= 0)
                {
                    panelSpacing = value;
                    UpdatePanelLayout();
                }
            }
        }



        #endregion

        #region 集合变更处理

        private void OnPanelsCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Element is FluentPanelListItem panel)
            {
                if (e.Action == CollectionChangeAction.Add)
                {
                    // 订阅面板的展开事件
                    panel.ExpandedChanged += OnPanelExpandedChanged;
                    panel.VisibilityChanged += OnPanelVisibilityChanged;
                    panel.ParentList = this;

                    // 记录原始索引
                    UpdateOriginalIndices();

                    // 确保面板被添加到 Controls 集合
                    if (!Controls.Contains(panel))
                    {
                        panel.SuspendLayout();

                        // 设置面板的基本属性
                        panel.Visible = panel.IsItemVisible;
                        panel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                        // 应用主题
                        if (UseTheme && Theme != null)
                        {
                            panel.TitleBackColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.ControlLight);
                            panel.TitleForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                            panel.BackColor = GetThemeColor(c => c.Surface, SystemColors.Window);
                        }

                        panel.ResumeLayout(false);

                        // 添加到控件集合
                        Controls.Add(panel);
                    }

                    // 立即更新布局
                    if (IsHandleCreated && !isInitializing)
                    {
                        // 使用 BeginInvoke 确保控件已完全添加
                        BeginInvoke(new Action(() =>
                        {
                            if (!IsDisposed)
                            {
                                ForceUpdateLayout();
                                Invalidate();
                                Update(); // 强制立即绘制
                            }
                        }));
                    }
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    panel.ExpandedChanged -= OnPanelExpandedChanged;
                    panel.VisibilityChanged -= OnPanelVisibilityChanged;
                    panel.ParentList = null;

                    // 移除原始索引记录
                    originalIndices.Remove(panel);

                    if (Controls.Contains(panel))
                    {
                        Controls.Remove(panel);
                    }

                    if (IsHandleCreated && !isInitializing)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            if (!IsDisposed)
                            {
                                ForceUpdateLayout();
                                Invalidate();
                            }
                        }));
                    }
                }
            }
        }

        private void OnPanelExpandedChanged(object sender, EventArgs e)
        {
            if (sender is FluentPanelListItem panel)
            {
                StartPanelExpandAnimation(panel);
            }
        }

        private void OnPanelVisibilityChanged(object sender, EventArgs e)
        {
            if (!isUpdatingLayout)
            {
                UpdatePanelLayout();
            }
        }

        /// <summary>
        /// 更新原始索引映射
        /// </summary>
        private void UpdateOriginalIndices()
        {
            originalIndices.Clear();
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i] != null)
                {
                    originalIndices[panels[i]] = i;
                }
            }
        }

        #endregion

        #region 布局管理

        /// <summary>
        /// 强制更新布局
        /// </summary>
        private void ForceUpdateLayout()
        {
            if (panels == null || IsDisposed)
            {
                return;
            }

            // 临时允许在初始化时更新
            bool wasInitializing = isInitializing;
            isInitializing = false;

            try
            {
                UpdatePanelLayout();
            }
            finally
            {
                isInitializing = wasInitializing;
            }
        }

        public void UpdatePanelLayout()
        {
            if (isUpdatingLayout || panels == null || IsDisposed)
            {
                return;
            }

            // 在设计时或初始化完成后才更新
            if (isInitializing && !DesignMode || !IsHandleCreated)
            {
                return;
            }

            try
            {
                isUpdatingLayout = true;

                if (panels.Count == 0)
                {
                    UpdateScrollBar();
                    return;
                }

                SuspendLayout();

                int top = panelSpacing;
                var visibleRect = GetVisibleRectangle();
                int availableWidth = visibleRect.Width - panelSpacing * 2;

                // 确保有足够的宽度
                if (availableWidth < 50)
                {
                    availableWidth = Math.Max(50, Width - panelSpacing * 2 - (scrollBar?.Visible == true ? scrollBar.Width : 0));
                }

                foreach (var panel in panels)
                {
                    if (panel == null || panel.IsDisposed)
                    {
                        continue;
                    }

                    // 跳过不可见的面板
                    if (!panel.IsItemVisible)
                    {
                        // 确保不可见的面板真的被隐藏
                        panel.Visible = false;
                        continue;
                    }

                    panel.SuspendLayout();

                    int panelTop = top - scrollOffset;
                    panel.Location = new Point(panelSpacing, panelTop);
                    panel.Width = availableWidth;

                    // 只在非动画状态下更新高度
                    if (!panel.IsAnimating && !expandAnimations.ContainsKey(panel))
                    {
                        panel.Height = panel.IsExpanded ? panel.FullHeight : panel.CollapsedHeight;
                    }

                    panel.Visible = true;
                    panel.BringToFront();

                    panel.ResumeLayout(false);

                    top += panel.Height + panelSpacing;
                }

                ResumeLayout(false);
                UpdateScrollBar();
            }
            finally
            {
                isUpdatingLayout = false;
            }

            Invalidate();
        }

        private Rectangle GetVisibleRectangle()
        {
            var rect = ClientRectangle;
            if (scrollBar != null && scrollBar.Visible)
            {
                rect.Width -= scrollBar.Width;
            }
            return rect;
        }

        private int GetTotalHeight()
        {
            int total = panelSpacing;

            foreach (var panel in panels)
            {
                // 只计算可见面板的高度
                if (panel != null && !panel.IsDisposed && panel.IsItemVisible)
                {
                    total += panel.Height + panelSpacing;
                }
            }

            return total;
        }

        private void UpdateScrollBar()
        {
            if (scrollBar == null || isInitializing)
            {
                return;
            }

            var visibleRect = GetVisibleRectangle();
            int totalHeight = GetTotalHeight();

            if (totalHeight > visibleRect.Height)
            {
                scrollBar.Visible = true;
                scrollBar.Maximum = totalHeight - visibleRect.Height + scrollBar.LargeChange - 1;
                scrollBar.LargeChange = Math.Max(1, visibleRect.Height / 2);
                scrollBar.SmallChange = 20;

                int maxScroll = Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
                if (scrollOffset > maxScroll)
                {
                    scrollOffset = maxScroll;
                    scrollBar.Value = scrollOffset;
                }
            }
            else
            {
                scrollBar.Visible = false;
                scrollOffset = 0;
            }
        }

        #endregion

        #region 高亮面板

        /// <summary>
        /// 高亮指定面板
        /// </summary>
        /// <param name="panel">要高亮的面板</param>
        /// <param name="duration">持续时间(毫秒), 0表示持续高亮</param>
        /// <param name="scrollToView">是否滚动到可见区域</param>
        public void HighlightPanel(FluentPanelListItem panel, int duration = 0, bool scrollToView = true)
        {
            if (panel == null || !panels.Contains(panel))
            {
                return;
            }

            // 清除之前的高亮
            ClearHighlight();

            // 设置新的高亮
            panel.Highlight(duration);
            highlightedPanel = panel;

            // 滚动到可见区域
            if (scrollToView)
            {
                ScrollToPanel(panel);
            }
        }

        /// <summary>
        /// 根据索引高亮面板
        /// </summary>
        public void HighlightPanelAt(int index, int duration = 0, bool scrollToView = true)
        {
            if (index >= 0 && index < panels.Count)
            {
                HighlightPanel(panels[index], duration, scrollToView);
            }
        }

        /// <summary>
        /// 根据标题高亮面板
        /// </summary>
        public void HighlightPanelByTitle(string title, int duration = 0, bool scrollToView = true)
        {
            var panel = panels.FirstOrDefault(p => p.Title == title);
            if (panel != null)
            {
                HighlightPanel(panel, duration, scrollToView);
            }
        }

        /// <summary>
        /// 清除所有高亮
        /// </summary>
        public void ClearHighlight()
        {
            if (highlightedPanel != null)
            {
                highlightedPanel.ClearHighlight();
                highlightedPanel = null;
            }

            // 清除所有面板的高亮
            foreach (var panel in panels)
            {
                if (panel != null && panel.IsHighlighted)
                {
                    panel.ClearHighlight();
                }
            }
        }

        /// <summary>
        /// 获取当前高亮的面板
        /// </summary>
        public FluentPanelListItem GetHighlightedPanel()
        {
            return highlightedPanel;
        }

        /// <summary>
        /// 高亮并滚动到面板
        /// </summary>
        public void HighlightAndScrollTo(FluentPanelListItem panel, int highlightDuration = 2000, ScrollPosition position = ScrollPosition.Center)
        {
            if (panel == null || !panels.Contains(panel))
            {
                return;
            }

            // 确保面板可见
            if (!panel.IsItemVisible)
            {
                panel.IsItemVisible = true;
            }

            // 确保面板展开
            if (!panel.IsExpanded)
            {
                panel.IsExpanded = true;
            }

            // 延迟一点再滚动, 确保展开动画完成
            BeginInvoke(new Action(() =>
            {
                ScrollToPanel(panel, position);

                // 滚动完成后再高亮
                BeginInvoke(new Action(() =>
                {
                    HighlightPanel(panel, highlightDuration, false);
                }));
            }));
        }

        /// <summary>
        /// 根据索引高亮并滚动
        /// </summary>
        public void HighlightAndScrollToIndex(int index, int highlightDuration = 2000, ScrollPosition position = ScrollPosition.Center)
        {
            if (index >= 0 && index < panels.Count)
            {
                HighlightAndScrollTo(panels[index], highlightDuration, position);
            }
        }

        /// <summary>
        /// 根据标题高亮并滚动
        /// </summary>
        public void HighlightAndScrollToTitle(string title, int highlightDuration = 2000, ScrollPosition position = ScrollPosition.Center)
        {
            var panel = panels.FirstOrDefault(p => p.Title == title);
            if (panel != null)
            {
                HighlightAndScrollTo(panel, highlightDuration, position);
            }
        }

        /// <summary>
        /// 闪烁面板
        /// </summary>
        public void FlashPanel(FluentPanelListItem panel, int flashCount = 3, int flashInterval = 300)
        {
            if (panel == null || !panels.Contains(panel))
            {
                return;
            }

            ScrollToPanel(panel, ScrollPosition.Center);

            int count = 0;
            var flashTimer = new Timer { Interval = flashInterval };

            flashTimer.Tick += (s, e) =>
            {
                count++;
                panel.IsHighlighted = !panel.IsHighlighted;

                if (count >= flashCount * 2)
                {
                    flashTimer.Stop();
                    flashTimer.Dispose();
                    panel.IsHighlighted = false;
                }
            };

            flashTimer.Start();
        }

        #endregion

        #region 面板可见性管理

        /// <summary>
        /// 隐藏指定的面板
        /// </summary>
        public void HidePanel(FluentPanelListItem panel)
        {
            if (panel != null && panels.Contains(panel))
            {
                panel.IsItemVisible = false;
            }
        }

        /// <summary>
        /// 显示指定的面板
        /// </summary>
        public void ShowPanel(FluentPanelListItem panel)
        {
            if (panel != null && panels.Contains(panel))
            {
                panel.IsItemVisible = true;
            }
        }

        /// <summary>
        /// 切换面板的可见性
        /// </summary>
        public void TogglePanelVisibility(FluentPanelListItem panel)
        {
            if (panel != null && panels.Contains(panel))
            {
                panel.ToggleVisibility();
            }
        }

        /// <summary>
        /// 设置面板的可见性
        /// </summary>
        public void SetPanelVisibility(FluentPanelListItem panel, bool visible)
        {
            if (visible)
            {
                ShowPanel(panel);
            }
            else
            {
                HidePanel(panel);
            }
        }

        /// <summary>
        /// 根据标题隐藏面板
        /// </summary>
        public void HidePanelByTitle(string title)
        {
            var panel = panels.FirstOrDefault(p => p.Title == title);
            if (panel != null)
            {
                HidePanel(panel);
            }
        }

        /// <summary>
        /// 根据标题显示面板
        /// </summary>
        public void ShowPanelByTitle(string title)
        {
            var panel = panels.FirstOrDefault(p => p.Title == title);
            if (panel != null)
            {
                ShowPanel(panel);
            }
        }

        /// <summary>
        /// 根据索引隐藏面板
        /// </summary>
        public void HidePanelAt(int index)
        {
            if (index >= 0 && index < panels.Count)
            {
                HidePanel(panels[index]);
            }
        }

        /// <summary>
        /// 根据索引显示面板
        /// </summary>
        public void ShowPanelAt(int index)
        {
            if (index >= 0 && index < panels.Count)
            {
                ShowPanel(panels[index]);
            }
        }

        /// <summary>
        /// 批量隐藏面板
        /// </summary>
        public void HidePanels(IEnumerable<FluentPanelListItem> panelsToHide)
        {
            if (panelsToHide == null)
            {
                return;
            }

            foreach (var panel in panelsToHide)
            {
                if (panel != null && panels.Contains(panel))
                {
                    panel.IsItemVisible = false;
                }
            }

            UpdatePanelLayout();
        }

        /// <summary>
        /// 批量显示面板
        /// </summary>
        public void ShowPanels(IEnumerable<FluentPanelListItem> panelsToShow)
        {
            if (panelsToShow == null)
            {
                return;
            }

            foreach (var panel in panelsToShow)
            {
                if (panel != null && panels.Contains(panel))
                {
                    panel.IsItemVisible = true;
                }
            }

            UpdatePanelLayout();
        }

        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    panel.IsItemVisible = false;
                }
            }

            UpdatePanelLayout();
        }

        /// <summary>
        /// 显示所有面板
        /// </summary>
        public void ShowAllPanels()
        {
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    panel.IsItemVisible = true;
                }
            }

            UpdatePanelLayout();
        }

        /// <summary>
        /// 根据条件隐藏面板
        /// </summary>
        public void HidePanelsWhere(Func<FluentPanelListItem, bool> predicate)
        {
            if (predicate == null)
            {
                return;
            }

            foreach (var panel in panels.Where(predicate))
            {
                panel.IsItemVisible = false;
            }

            UpdatePanelLayout();
        }

        /// <summary>
        /// 根据条件显示面板
        /// </summary>
        public void ShowPanelsWhere(Func<FluentPanelListItem, bool> predicate)
        {
            if (predicate == null)
            {
                return;
            }

            foreach (var panel in panels.Where(predicate))
            {
                panel.IsItemVisible = true;
            }

            UpdatePanelLayout();
        }

        /// <summary>
        /// 获取所有可见的面板
        /// </summary>
        public IEnumerable<FluentPanelListItem> GetVisiblePanels()
        {
            return panels.Where(p => p != null && p.IsItemVisible);
        }

        /// <summary>
        /// 获取所有隐藏的面板
        /// </summary>
        public IEnumerable<FluentPanelListItem> GetHiddenPanels()
        {
            return panels.Where(p => p != null && !p.IsItemVisible);
        }

        /// <summary>
        /// 获取可见面板的数量
        /// </summary>
        public int GetVisiblePanelCount()
        {
            return panels.Count(p => p != null && p.IsItemVisible);
        }

        /// <summary>
        /// 保存当前面板的可见性状态
        /// </summary>
        public Dictionary<string, bool> SaveVisibilityState()
        {
            var state = new Dictionary<string, bool>();

            for (int i = 0; i < panels.Count; i++)
            {
                var panel = panels[i];
                if (panel != null)
                {
                    // 使用索引+标题作为键, 确保唯一性
                    string key = $"{i}_{panel.Title}";
                    state[key] = panel.IsItemVisible;
                }
            }

            return state;
        }

        /// <summary>
        /// 恢复面板的可见性状态
        /// </summary>
        public void RestoreVisibilityState(Dictionary<string, bool> state)
        {
            if (state == null)
            {
                return;
            }

            for (int i = 0; i < panels.Count; i++)
            {
                var panel = panels[i];
                if (panel != null)
                {
                    string key = $"{i}_{panel.Title}";
                    if (state.TryGetValue(key, out bool visible))
                    {
                        panel.IsItemVisible = visible;
                    }
                }
            }

            UpdatePanelLayout();
        }

        #endregion

        #region 滚动处理

        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            scrollOffset = e.NewValue;
            UpdatePanelLayout();
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
                    UpdatePanelLayout();
                }
            }
        }

        /// <summary>
        /// 滚动到指定面板
        /// </summary>
        /// <param name="panel">目标面板</param>
        /// <param name="position">滚动位置</param>
        public void ScrollToPanel(FluentPanelListItem panel, ScrollPosition position = ScrollPosition.Center)
        {
            if (panel == null || !panels.Contains(panel) || !panel.IsItemVisible)
            {
                return;
            }

            if (!scrollBar.Visible)
            {
                return; // 没有滚动条, 无需滚动
            }

            // 计算面板的实际位置
            int panelTop = CalculatePanelTop(panel);
            int panelHeight = panel.Height;

            var visibleRect = GetVisibleRectangle();
            int targetScroll = scrollOffset;

            switch (position)
            {
                case ScrollPosition.Top:
                    // 滚动到顶部
                    targetScroll = panelTop - panelSpacing;
                    break;

                case ScrollPosition.Center:
                    // 滚动到中间
                    targetScroll = panelTop - (visibleRect.Height - panelHeight) / 2;
                    break;

                case ScrollPosition.Bottom:
                    // 滚动到底部
                    targetScroll = panelTop + panelHeight - visibleRect.Height + panelSpacing;
                    break;

                case ScrollPosition.Nearest:
                    // 滚动到最近的可见位置
                    int panelBottom = panelTop + panelHeight;
                    int currentTop = scrollOffset;
                    int currentBottom = scrollOffset + visibleRect.Height;

                    if (panelTop < currentTop)
                    {
                        // 面板在可见区域上方
                        targetScroll = panelTop - panelSpacing;
                    }
                    else if (panelBottom > currentBottom)
                    {
                        // 面板在可见区域下方
                        targetScroll = panelBottom - visibleRect.Height + panelSpacing;
                    }
                    else
                    {
                        // 面板已经可见, 不需要滚动
                        return;
                    }
                    break;
            }

            // 限制滚动范围
            targetScroll = Math.Max(0, Math.Min(scrollBar.Maximum - scrollBar.LargeChange + 1, targetScroll));

            // 执行滚动
            if (EnableAnimation && !DesignMode)
            {
                AnimateScroll(targetScroll);
            }
            else
            {
                scrollOffset = targetScroll;
                scrollBar.Value = targetScroll;
                UpdatePanelLayout();
            }
        }

        /// <summary>
        /// 根据索引滚动到面板
        /// </summary>
        public void ScrollToPanelAt(int index, ScrollPosition position = ScrollPosition.Center)
        {
            if (index >= 0 && index < panels.Count)
            {
                ScrollToPanel(panels[index], position);
            }
        }

        /// <summary>
        /// 根据标题滚动到面板
        /// </summary>
        public void ScrollToPanelByTitle(string title, ScrollPosition position = ScrollPosition.Center)
        {
            var panel = panels.FirstOrDefault(p => p.Title == title);
            if (panel != null)
            {
                ScrollToPanel(panel, position);
            }
        }

        /// <summary>
        /// 确保面板可见
        /// </summary>
        public void EnsurePanelVisible(FluentPanelListItem panel)
        {
            ScrollToPanel(panel, ScrollPosition.Nearest);
        }

        /// <summary>
        /// 滚动到顶部
        /// </summary>
        public void ScrollToTop()
        {
            if (EnableAnimation && !DesignMode)
            {
                AnimateScroll(0);
            }
            else
            {
                scrollOffset = 0;
                scrollBar.Value = 0;
                UpdatePanelLayout();
            }
        }

        /// <summary>
        /// 滚动到底部
        /// </summary>
        public void ScrollToBottom()
        {
            int maxScroll = Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
            if (EnableAnimation && !DesignMode)
            {
                AnimateScroll(maxScroll);
            }
            else
            {
                scrollOffset = maxScroll;
                scrollBar.Value = maxScroll;
                UpdatePanelLayout();
            }
        }

        /// <summary>
        /// 计算面板的顶部位置
        /// </summary>
        private int CalculatePanelTop(FluentPanelListItem targetPanel)
        {
            int top = panelSpacing;

            foreach (var panel in panels)
            {
                if (panel == null || panel.IsDisposed || !panel.IsItemVisible)
                {
                    continue;
                }

                if (panel == targetPanel)
                {
                    return top;
                }

                top += panel.Height + panelSpacing;
            }

            return top;
        }

        /// <summary>
        /// 平滑滚动动画
        /// </summary>
        private void AnimateScroll(int targetScroll)
        {
            var startScroll = scrollOffset;
            var distance = targetScroll - startScroll;

            if (Math.Abs(distance) < 1)
            {
                return;
            }

            var scrollTimer = new Timer { Interval = 16 }; // 60 FPS
            var startTime = DateTime.Now;
            var duration = 300; // 300ms

            scrollTimer.Tick += (s, e) =>
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / duration);

                // 使用缓动函数
                var easedProgress = Easing.CubicOut(progress);
                var currentScroll = startScroll + (int)(distance * easedProgress);

                scrollOffset = currentScroll;
                scrollBar.Value = Math.Max(0, Math.Min(scrollBar.Maximum - scrollBar.LargeChange + 1, currentScroll));
                UpdatePanelLayoutDuringAnimation();

                if (progress >= 1.0)
                {
                    scrollTimer.Stop();
                    scrollTimer.Dispose();

                    // 确保最终位置准确
                    scrollOffset = targetScroll;
                    scrollBar.Value = targetScroll;
                    UpdatePanelLayout();
                }
            };

            scrollTimer.Start();
        }

        #endregion

        #region 动画处理

        private void StartPanelExpandAnimation(FluentPanelListItem panel)
        {
            if (!EnableAnimation || DesignMode)
            {
                UpdatePanelLayout();
                return;
            }

            // 停止该面板的现有动画
            if (expandAnimations.ContainsKey(panel))
            {
                expandAnimations.Remove(panel);
            }

            // 设置动画标志
            panel.IsAnimating = true;

            // 确保内容面板在动画期间可见(用于展开动画)
            if (panel.IsExpanded && panel.ContentPanel != null)
            {
                panel.ContentPanel.Visible = true;
            }

            var state = new AnimationState
            {
                Control = panel,
                CurrentStep = 0,
                TotalSteps = Math.Max(1, AnimationDuration / 16),
                Easing = Easing.CubicOut,

                // 记录起始和目标高度
                StartSize = new Size(panel.Width, panel.Height),
                TargetSize = new Size(panel.Width, panel.IsExpanded ? panel.FullHeight : panel.CollapsedHeight),
                AnimateHeight = true
            };

            expandAnimations[panel] = state;

            if (!animationTimer.Enabled)
            {
                animationTimer.Start();
            }
        }

        private void OnAnimationTimerTick(object sender, EventArgs e)
        {
            if (expandAnimations.Count == 0)
            {
                animationTimer.Stop();
                return;
            }

            bool needsUpdate = false;
            var completedPanels = new List<FluentPanelListItem>();

            foreach (var kvp in expandAnimations.ToArray())
            {
                var panel = kvp.Key;
                var state = kvp.Value;

                if (panel == null || panel.IsDisposed)
                {
                    completedPanels.Add(panel);
                    continue;
                }

                state.CurrentStep++;
                double progress = Math.Min(1.0, (double)state.CurrentStep / state.TotalSteps);
                double easedProgress = state.Easing(progress);

                // 计算当前高度
                int startHeight = state.StartSize.Height;
                int targetHeight = state.TargetSize.Height;
                int currentHeight = startHeight + (int)((targetHeight - startHeight) * easedProgress);

                // 直接设置高度, 不触发布局事件
                panel.Height = currentHeight;

                needsUpdate = true;

                // 检查是否完成
                if (state.CurrentStep >= state.TotalSteps)
                {
                    completedPanels.Add(panel);
                }
            }

            // 移除已完成的动画
            foreach (var panel in completedPanels)
            {
                expandAnimations.Remove(panel);

                if (panel != null && !panel.IsDisposed)
                {
                    // 确保最终高度准确
                    panel.Height = panel.IsExpanded ? panel.FullHeight : panel.CollapsedHeight;

                    // 更新内容面板可见性
                    if (panel.ContentPanel != null)
                    {
                        panel.ContentPanel.Visible = panel.IsExpanded;
                    }

                    // 清除动画标志
                    panel.IsAnimating = false;
                }
            }

            // 动画过程中更新布局(但不重新计算面板高度)
            if (needsUpdate)
            {
                UpdatePanelLayoutDuringAnimation();
            }

            if (expandAnimations.Count == 0)
            {
                animationTimer.Stop();
                // 动画完成后完整更新布局
                UpdatePanelLayout();
            }
        }

        /// <summary>
        /// 动画期间的布局更新
        /// </summary>
        private void UpdatePanelLayoutDuringAnimation()
        {
            if (isUpdatingLayout || panels == null || IsDisposed)
            {
                return;
            }

            try
            {
                isUpdatingLayout = true;

                int top = panelSpacing;
                var visibleRect = GetVisibleRectangle();
                int availableWidth = visibleRect.Width - panelSpacing * 2;

                if (availableWidth < 50)
                {
                    availableWidth = Math.Max(50, Width - panelSpacing * 2 - (scrollBar?.Visible == true ? scrollBar.Width : 0));
                }

                // 只更新位置, 不改变高度
                foreach (var panel in panels)
                {
                    if (panel == null || panel.IsDisposed)
                    {
                        continue;
                    }

                    int panelTop = top - scrollOffset;

                    // 平滑移动位置
                    if (panel.Top != panelTop)
                    {
                        panel.Top = panelTop;
                    }

                    // 确保宽度正确
                    if (panel.Width != availableWidth)
                    {
                        panel.Width = availableWidth;
                    }

                    top += panel.Height + panelSpacing;
                }

                UpdateScrollBar();
            }
            finally
            {
                isUpdatingLayout = false;
            }

            // 只使一次, 减少闪烁
            Invalidate();
        }


        #endregion

        #region 公共方法

        /// <summary>
        /// 添加面板
        /// </summary>
        public FluentPanelListItem AddPanel(string title)
        {
            var panel = new FluentPanelListItem(title);

            return AddPanel(panel);
        }

        public FluentPanelListItem AddPanel(FluentPanelListItem panel)
        {
            // 应用主题
            if (UseTheme && Theme != null)
            {
                panel.TitleBackColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.ControlLight);
                panel.TitleForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                panel.BackColor = GetThemeColor(c => c.Surface, SystemColors.Window);
            }

            panels.Add(panel);

            // 在设计时立即显示
            if (DesignMode)
            {
                Refresh();
            }

            return panel;
        }

        /// <summary>
        /// 移除面板
        /// </summary>
        public void RemovePanel(FluentPanelListItem panel)
        {
            if (panels.Contains(panel))
            {
                panels.Remove(panel);
            }
        }

        public void Clear()
        {
            panels.Clear();
        }

        /// <summary>
        /// 展开所有面板
        /// </summary>
        public void ExpandAll()
        {
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    panel.IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// 折叠所有面板
        /// </summary>
        public void CollapseAll()
        {
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    panel.IsExpanded = false;
                }
            }
        }

        #endregion

        #region 重写方法

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!isInitializing)
            {
                UpdatePanelLayout();
            }
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!isInitializing)
            {
                UpdatePanelLayout();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 在设计时显示提示信息
            if (DesignMode && panels.Count == 0)
            {
                var g = e.Graphics;
                string hint = "使用智能标签或属性编辑器添加面板";
                using (var font = new Font(Font.FontFamily, 9f))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(hint, font, brush, ClientRectangle, sf);
                }
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            var bgColor = GetThemeColor(c => c.Background, SystemColors.Control);
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 内容由子面板绘制
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                BackColor = GetThemeColor(c => c.Background, SystemColors.Control);

                // 应用主题到所有面板
                foreach (var panel in panels)
                {
                    if (panel != null)
                    {
                        panel.TitleBackColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.ControlLight);
                        panel.TitleForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
                        panel.BackColor = GetThemeColor(c => c.Surface, SystemColors.Window);
                    }
                }
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
                scrollBar?.Dispose();

                if (panels != null)
                {
                    panels.ItemChanged -= OnPanelsCollectionChanged;
                    foreach (var panel in panels)
                    {
                        if (panel != null)
                        {
                            panel.ExpandedChanged -= OnPanelExpandedChanged;
                            panel.VisibilityChanged -= OnPanelVisibilityChanged;
                        }
                    }
                }
                originalIndices?.Clear();
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    #region 面板项

    [Designer(typeof(FluentPanelListItemDesigner))]
    [ToolboxItem(false)]
    [DefaultProperty("Title")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FluentPanelListItem : Panel
    {
        private Label titleLabel;
        private Panel titlePanel;
        private Panel contentPanel;
        private PictureBox expandButton;

        private bool isExpanded = true;
        private int titleHeight = 40;
        private int contentPadding = 8;
        private int minPanelHeight = 50;
        private bool isItemVisible = true;

        private Color titleBackColor = SystemColors.ControlLight;
        private Color titleForeColor = SystemColors.ControlText;
        private Color contentBackColor = Color.White;
        private Font contentFont = new Font("Microsoft Yahei", 9F);

        // 高亮突出显示
        private bool isHighlighted = false;
        private Color highlightColor = Color.FromArgb(255, 165, 0); // 默认橙色
        private int highlightBorderWidth = 2;
        private Timer highlightTimer;
        private int highlightDuration = 0; // 0 表示持续高亮

        private FluentPanelList parentList;

        // 添加递归保护和初始化标志
        private bool isUpdatingLayout = false;
        private bool isInitializing = true;
        private bool isCalculatingHeight = false;
        private bool isAnimating = false;

        // 事件
        public event EventHandler VisibilityChanged;
        public event EventHandler HighlightChanged;

        public FluentPanelListItem()
        {
            isInitializing = true;

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            DoubleBuffered = true;

            InitializeComponents();

            isInitializing = false;
        }

        public FluentPanelListItem(string title) : this()
        {
            Title = title;
        }

        #region 属性

        /// <summary>
        /// 标题
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        public string Title
        {
            get => titleLabel?.Text ?? string.Empty;
            set
            {
                if (titleLabel != null)
                {
                    titleLabel.Text = value;
                    titlePanel?.Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否展开
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;

                    // 布局更新交给动画系统处理
                    if (!isInitializing && !isUpdatingLayout)
                    {
                        OnExpandedChanged();
                    }
                }
            }
        }

        /// <summary>
        /// 标题高度
        /// </summary>
        [Category("Layout")]
        [DefaultValue(40)]
        public int TitleHeight
        {
            get => titleHeight;
            set
            {
                if (titleHeight != value && value > 0)
                {
                    titleHeight = value;
                    if (titlePanel != null)
                    {
                        titlePanel.Height = titleHeight;
                    }
                    UpdateLayout();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(50)]
        [Description("面板内容区域的最小高度")]
        public int MinPanelHeight
        {
            get => minPanelHeight;
            set
            {
                if (minPanelHeight != value && value >= 0)
                {
                    minPanelHeight = value;
                    if (isExpanded)
                    {
                        RecalculateHeight();
                    }
                }
            }
        }

        /// <summary>
        /// 内容区域内边距
        /// </summary>
        [Category("Layout")]
        [DefaultValue(8)]
        public int ContentPadding
        {
            get => contentPadding;
            set
            {
                if (contentPadding != value && value >= 0)
                {
                    contentPadding = value;
                    if (contentPanel != null)
                    {
                        contentPanel.Padding = new Padding(contentPadding);
                    }
                }
            }
        }

        /// <summary>
        /// 标题背景色
        /// </summary>
        [Category("Appearance")]
        public Color TitleBackColor
        {
            get => titleBackColor;
            set
            {
                titleBackColor = value;
                if (titlePanel != null)
                {
                    titlePanel.BackColor = value;
                }
            }
        }

        /// <summary>
        /// 标题前景色
        /// </summary>
        [Category("Appearance")]
        public Color TitleForeColor
        {
            get => titleForeColor;
            set
            {
                titleForeColor = value;
                if (titleLabel != null)
                {
                    titleLabel.ForeColor = value;
                }
            }
        }

        /// <summary>
        /// 内容背景色
        /// </summary>
        [Category("Appearance")]
        public Color ContentBackColor
        {
            get => contentBackColor;
            set
            {
                contentBackColor = value;
                if (contentPanel != null)
                {
                    contentPanel.BackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        public Font ContentFont
        {
            get => contentFont;
            set
            {
                contentFont = value;
                if (contentPanel != null)
                {
                    contentPanel.Font = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否处于高亮状态
        /// </summary>
        [Browsable(false)]
        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted != value)
                {
                    isHighlighted = value;
                    Invalidate();
                    OnHighlightChanged();
                }
            }
        }

        /// <summary>
        /// 高亮颜色
        /// </summary>
        [Category("Appearance")]
        [Description("面板高亮时的颜色")]
        public Color HighlightColor
        {
            get => highlightColor;
            set
            {
                if (highlightColor != value)
                {
                    highlightColor = value;
                    if (isHighlighted)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 高亮边框宽度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(2)]
        [Description("高亮边框的宽度")]
        public int HighlightBorderWidth
        {
            get => highlightBorderWidth;
            set
            {
                if (highlightBorderWidth != value && value > 0)
                {
                    highlightBorderWidth = value;
                    if (isHighlighted)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 项目是否可见
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("控制面板项是否可见, 隐藏后不占用空间")]
        public bool IsItemVisible
        {
            get => isItemVisible;
            set
            {
                if (isItemVisible != value)
                {
                    isItemVisible = value;

                    // 同时更新基类的 Visible 属性
                    base.Visible = value;

                    // 通知父列表更新布局
                    parentList?.UpdatePanelLayout();

                    OnVisibilityChanged();
                }
            }
        }

        /// <summary>
        /// 是否正在执行动画
        /// </summary>
        [Browsable(false)]
        public bool IsAnimating
        {
            get => isAnimating;
            internal set => isAnimating = value;
        }

        /// <summary>
        /// 内容面板
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Panel ContentPanel => contentPanel;

        /// <summary>
        /// 父列表控件
        /// </summary>
        [Browsable(false)]
        public FluentPanelList ParentList
        {
            get => parentList;
            internal set => parentList = value;
        }

        /// <summary>
        /// 获取完整高度(展开时)
        /// </summary>
        [Browsable(false)]
        public int FullHeight
        {
            get
            {
                if (isCalculatingHeight)
                {
                    return titleHeight + 100; // 防止递归, 返回默认值
                }

                try
                {
                    isCalculatingHeight = true;
                    return titleHeight + GetContentHeight();
                }
                finally
                {
                    isCalculatingHeight = false;
                }
            }
        }

        /// <summary>
        /// 获取折叠高度
        /// </summary>
        [Browsable(false)]
        public int CollapsedHeight => titleHeight;

        #endregion

        #region 事件

        public event EventHandler ExpandedChanged;

        protected virtual void OnExpandedChanged()
        {
            ExpandedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 初始化

        private void InitializeComponents()
        {
            SuspendLayout();

            // 标题面板
            titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = titleHeight,
                BackColor = titleBackColor,
                Cursor = Cursors.Hand
            };

            // 启用双缓冲
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, titlePanel, new object[] { true });

            titlePanel.Paint += TitlePanel_Paint;
            titlePanel.MouseClick += TitlePanel_MouseClick;
            titlePanel.MouseEnter += TitlePanel_MouseEnter;
            titlePanel.MouseLeave += TitlePanel_MouseLeave;

            // 展开按钮图标
            expandButton = new PictureBox
            {
                Width = 16,
                Height = 16,
                Location = new Point(8, (titleHeight - 16) / 2),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            expandButton.MouseClick += TitlePanel_MouseClick;

            // 标题标签
            titleLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = titleForeColor,
                Padding = new Padding(32, 0, 8, 0),
                Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            titlePanel.Controls.Add(expandButton);
            titlePanel.Controls.Add(titleLabel);

            // 内容面板
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(contentPadding),
                BackColor = contentBackColor,
                Font = contentFont
            };

            // 启用双缓冲
            typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, contentPanel, new object[] { true });

            Controls.Add(contentPanel);
            Controls.Add(titlePanel);

            AutoSize = false;
            Height = titleHeight;

            ResumeLayout(false);
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制高亮边框
            if (isHighlighted)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(highlightColor, highlightBorderWidth))
                {
                    var rect = new Rectangle(
                        highlightBorderWidth / 2,
                        highlightBorderWidth / 2,
                        Width - highlightBorderWidth,
                        Height - highlightBorderWidth);

                    g.DrawRectangle(pen, rect);
                }
            }
        }

        private void TitlePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 如果高亮, 绘制高亮背景
            if (isHighlighted && titlePanel != null)
            {
                var highlightBackColor = Color.FromArgb(50, highlightColor);
                using (var brush = new SolidBrush(highlightBackColor))
                {
                    g.FillRectangle(brush, titlePanel.ClientRectangle);
                }
            }

            // 绘制展开/折叠箭头
            DrawExpandIcon(g);

            // 绘制底部分隔线
            var lineColor = isHighlighted ? highlightColor : Color.FromArgb(200, 200, 200);
            using (var pen = new Pen(lineColor))
            {
                g.DrawLine(pen, 0, titlePanel.Height - 1, titlePanel.Width, titlePanel.Height - 1);
            }
        }

        private void DrawExpandIcon(Graphics g)
        {
            var iconRect = new Rectangle(8, (titleHeight - 16) / 2, 16, 16);
            var centerX = iconRect.X + iconRect.Width / 2f;
            var centerY = iconRect.Y + iconRect.Height / 2f;

            using (var pen = new Pen(titleForeColor, 2))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                if (isExpanded)
                {
                    // 向下箭头
                    var points = new PointF[]
                    {
                    new PointF(centerX - 4, centerY - 2),
                    new PointF(centerX, centerY + 2),
                    new PointF(centerX + 4, centerY - 2)
                    };
                    g.DrawLines(pen, points);
                }
                else
                {
                    // 向右箭头
                    var points = new PointF[]
                    {
                    new PointF(centerX - 2, centerY - 4),
                    new PointF(centerX + 2, centerY),
                    new PointF(centerX - 2, centerY + 4)
                    };
                    g.DrawLines(pen, points);
                }
            }
        }

        #endregion

        #region 交互

        private void TitlePanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsExpanded = !IsExpanded;
            }
        }

        private void TitlePanel_MouseEnter(object sender, EventArgs e)
        {
            if (titlePanel != null)
            {
                titlePanel.BackColor = ControlPaint.Light(titleBackColor, 0.1f);
            }
        }

        private void TitlePanel_MouseLeave(object sender, EventArgs e)
        {
            if (titlePanel != null)
            {
                titlePanel.BackColor = titleBackColor;
            }
        }

        public void Expand()
        {
            IsExpanded = true;
        }

        public void Collapse()
        {
            IsExpanded = false;
        }

        #endregion

        #region 可见性

        protected virtual void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 隐藏面板项
        /// </summary>
        public new void Hide()
        {
            IsItemVisible = false;
        }

        /// <summary>
        /// 显示面板项
        /// </summary>
        public new void Show()
        {
            IsItemVisible = true;
        }

        /// <summary>
        /// 切换可见性
        /// </summary>
        public void ToggleVisibility()
        {
            IsItemVisible = !IsItemVisible;
        }

        #endregion

        #region 高亮效果

        protected virtual void OnHighlightChanged()
        {
            HighlightChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 设置高亮
        /// </summary>
        /// <param name="duration">高亮持续时间(毫秒), 0表示持续高亮</param>
        public void Highlight(int duration = 0)
        {
            IsHighlighted = true;
            highlightDuration = duration;

            if (duration > 0)
            {
                // 停止之前的计时器
                highlightTimer?.Stop();
                highlightTimer?.Dispose();

                // 创建新的计时器
                highlightTimer = new Timer { Interval = duration };
                highlightTimer.Tick += (s, e) =>
                {
                    IsHighlighted = false;
                    highlightTimer.Stop();
                    highlightTimer.Dispose();
                    highlightTimer = null;
                };
                highlightTimer.Start();
            }
        }

        /// <summary>
        /// 取消高亮
        /// </summary>
        public void ClearHighlight()
        {
            highlightTimer?.Stop();
            highlightTimer?.Dispose();
            highlightTimer = null;
            IsHighlighted = false;
        }


        #endregion

        #region 布局更新

        private void UpdateExpandState()
        {
            if (isInitializing || isUpdatingLayout || contentPanel == null)
            {
                return;
            }

            contentPanel.Visible = isExpanded;
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (isInitializing || isUpdatingLayout || isAnimating)
            {
                return;
            }

            try
            {
                isUpdatingLayout = true;

                if (isExpanded)
                {
                    Height = FullHeight;
                }
                else
                {
                    Height = CollapsedHeight;
                }

                // 通知父列表更新布局
                parentList?.UpdatePanelLayout();
            }
            finally
            {
                isUpdatingLayout = false;
            }
        }

        private int GetContentHeight()
        {
            if (contentPanel == null)
            {
                return Math.Max(minPanelHeight, contentPadding * 2 + 50);
            }

            if (contentPanel.Controls.Count == 0)
            {
                return Math.Max(minPanelHeight, contentPadding * 2 + 50);
            }

            int maxBottom = 0;
            foreach (Control control in contentPanel.Controls)
            {
                if (control == null || !control.Visible)
                {
                    continue;
                }

                int bottom = control.Top + control.Height + control.Margin.Bottom;
                if (bottom > maxBottom)
                {
                    maxBottom = bottom;
                }
            }

            // 加上内边距, 并确保不小于最小高度
            int calculatedHeight = maxBottom + contentPadding * 2;
            return Math.Max(minPanelHeight, calculatedHeight);
        }

        public void RecalculateHeight()
        {
            if (isInitializing || isUpdatingLayout || isCalculatingHeight)
            {
                return;
            }

            if (isExpanded)
            {
                try
                {
                    isUpdatingLayout = true;
                    int newHeight = FullHeight;

                    if (Height != newHeight)
                    {
                        Height = newHeight;
                        parentList?.UpdatePanelLayout();
                    }
                }
                finally
                {
                    isUpdatingLayout = false;
                }
            }
        }

        #endregion

        #region 控件变更监听

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (isInitializing || e.Control == titlePanel || e.Control == contentPanel)
            {
                return;
            }

            if (contentPanel != null && IsHandleCreated)
            {
                // 延迟检查, 确保控件已完全添加
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed && contentPanel != null && contentPanel.Controls.Contains(e.Control))
                    {
                        e.Control.SizeChanged += ContentControl_Changed;
                        e.Control.LocationChanged += ContentControl_Changed;
                        e.Control.VisibleChanged += ContentControl_Changed;

                        RecalculateHeight();
                    }
                }));
            }
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            if (isInitializing || e.Control == titlePanel || e.Control == contentPanel)
            {
                return;
            }

            e.Control.SizeChanged -= ContentControl_Changed;
            e.Control.LocationChanged -= ContentControl_Changed;
            e.Control.VisibleChanged -= ContentControl_Changed;

            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(new Action(() => RecalculateHeight()));
            }
        }

        private void ContentControl_Changed(object sender, EventArgs e)
        {
            if (!isInitializing && !isUpdatingLayout && !IsDisposed)
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                    {
                        RecalculateHeight();
                    }
                }));
            }
        }

        #endregion

        #region 重写方法

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 初始化或更新期间不处理, 防止递归
            if (isInitializing || isUpdatingLayout)
            {
                return;
            }

            Invalidate();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                highlightTimer?.Stop();
                highlightTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
    /// <summary>
    /// FluentPanelList 面板集合
    /// </summary>
    public class FluentPanelListItemCollection : Collection<FluentPanelListItem>
    {
        private FluentPanelList owner;

        public FluentPanelListItemCollection(FluentPanelList owner)
        {
            this.owner = owner;
        }

        public event CollectionChangeEventHandler ItemChanged;

        protected override void InsertItem(int index, FluentPanelListItem item)
        {
            base.InsertItem(index, item);
            item.ParentList = owner;
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }

        protected override void RemoveItem(int index)
        {
            FluentPanelListItem item = base[index];
            item.ParentList = null;
            base.RemoveItem(index);
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));
        }

        protected override void SetItem(int index, FluentPanelListItem item)
        {
            base.SetItem(index, item);
            item.ParentList = owner;
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, item));
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.ParentList = null;
            }
            for (int i = Count - 1; i >= 0; i--)
            {
                RemoveItem(i);
            }
            base.ClearItems();
            ItemChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 滚动位置枚举
    /// </summary>
    public enum ScrollPosition
    {
        Top,        // 滚动到顶部
        Center,     // 滚动到中间
        Bottom,     // 滚动到底部
        Nearest     // 滚动到最近的可见位置
    }

    #endregion


    #region 设计时支持

    /// <summary>
    /// FluentPanelList 设计器
    /// </summary>
    public class FluentPanelListDesigner : ParentControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private DesignerVerbCollection verbs;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentPanelListActionList(Component));
                }
                return actionLists;
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // 启用设计时控件添加
            if (component is FluentPanelList panelList)
            {
                // 只为 FluentPanelListItem 启用设计模式
                foreach (Control control in panelList.Controls)
                {
                    if (control is FluentPanelListItem || control is VScrollBar)
                    {
                        EnableDesignMode(control, control.Name);
                    }
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            properties.Remove("AutoScroll");
            properties.Remove("AutoScrollMargin");
            properties.Remove("AutoScrollMinSize");
        }

        /// <summary>
        /// 允许在设计时调整大小
        /// </summary>
        public override SelectionRules SelectionRules
        {
            get
            {
                return SelectionRules.Visible | SelectionRules.AllSizeable | SelectionRules.Moveable;
            }
        }

        public override bool CanParent(Control control)
        {
            // 只允许 FluentPanelListItem 作为直接子控件
            return control is FluentPanelListItem;
        }

        protected override bool GetHitTest(Point point)
        {
            // 检查点击位置是否在某个 FluentPanelListItem 上
            if (Component is FluentPanelList panelList)
            {
                Point clientPoint = panelList.PointToClient(Control.MousePosition);

                foreach (var panel in panelList.Panels)
                {
                    if (panel != null && panel.Bounds.Contains(clientPoint))
                    {
                        return false;
                    }
                }
            }

            // 其他位置返回 true, 阻止添加控件
            return true;
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            // 检查是否是 FluentPanelListItem
            var data = de.Data.GetData(typeof(FluentPanelListItem));
            if (data is FluentPanelListItem)
            {
                base.OnDragDrop(de);
            }
            else
            {
                // 阻止其他控件的拖放
                de.Effect = DragDropEffects.None;
            }
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            var data = de.Data.GetData(typeof(FluentPanelListItem));
            if (data is FluentPanelListItem)
            {
                base.OnDragEnter(de);
            }
            else
            {
                de.Effect = DragDropEffects.None;
            }
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            var data = de.Data.GetData(typeof(FluentPanelListItem));
            if (data is FluentPanelListItem)
            {
                base.OnDragOver(de);
            }
            else
            {
                de.Effect = DragDropEffects.None;
            }
        }

        #region 设计器右键菜单支持

        /// <summary>
        /// 设计器右键菜单
        /// </summary>
        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (verbs == null)
                {
                    verbs = new DesignerVerbCollection();

                    verbs.Add(new DesignerVerb("添加面板", (s, e) => AddPanelVerb()));
                    verbs.Add(new DesignerVerb("刷新布局", (s, e) => RefreshLayoutVerb()));
                    verbs.Add(new DesignerVerb("展开所有面板", (s, e) => ExpandAllVerb()));
                    verbs.Add(new DesignerVerb("折叠所有面板", (s, e) => CollapseAllVerb()));
                }
                return verbs;
            }
        }

        private void AddPanelVerb()
        {
            if (Component is FluentPanelList list)
            {
                var actionList = new FluentPanelListActionList(Component);
                var addPanelMethod = actionList.GetType().GetMethod("AddPanel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                addPanelMethod?.Invoke(actionList, null);
            }
        }

        private void RefreshLayoutVerb()
        {
            if (Component is FluentPanelList list)
            {
                var actionList = new FluentPanelListActionList(Component);
                var refreshMethod = actionList.GetType().GetMethod("RefreshLayout",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                refreshMethod?.Invoke(actionList, null);
            }
        }

        private void ExpandAllVerb()
        {
            if (Component is FluentPanelList list)
            {
                list.ExpandAll();
                RefreshLayoutVerb();
            }
        }

        private void CollapseAllVerb()
        {
            if (Component is FluentPanelList list)
            {
                list.CollapseAll();
                RefreshLayoutVerb();
            }
        }

        #endregion
    }

    public class FluentPanelListItemDesigner : ParentControlDesigner
    {
        private IComponentChangeService changeService;
        private ISelectionService selectionService;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (component is FluentPanelListItem item && item.ContentPanel != null)
            {
                // 允许将控件拖放到 ContentPanel 中
                EnableDesignMode(item.ContentPanel, "ContentPanel");

                // 获取服务
                changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                selectionService = GetService(typeof(ISelectionService)) as ISelectionService;

                if (changeService != null)
                {
                    changeService.ComponentAdded += OnComponentAdded;
                    changeService.ComponentRemoved += OnComponentRemoved;
                    changeService.ComponentChanged += OnComponentChanged;
                }
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            if (e.Component is Control control && Component is FluentPanelListItem item)
            {
                // 检查控件是否被添加到 ContentPanel
                if (item.ContentPanel != null && item.ContentPanel.Controls.Contains(control))
                {
                    // 延迟重新计算高度
                    System.Windows.Forms.Application.DoEvents();
                    item.RecalculateHeight();
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            if (e.Component is Control && Component is FluentPanelListItem item)
            {
                System.Windows.Forms.Application.DoEvents();
                item.RecalculateHeight();
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.Component is Control control && Component is FluentPanelListItem item)
            {
                if (item.ContentPanel != null && item.ContentPanel.Controls.Contains(control))
                {
                    // 如果是大小或位置变化, 重新计算高度
                    if (e.Member != null &&
                        (e.Member.Name == "Size" || e.Member.Name == "Location" ||
                         e.Member.Name == "Width" || e.Member.Name == "Height" ||
                         e.Member.Name == "Visible"))
                    {
                        item.RecalculateHeight();
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && changeService != null)
            {
                changeService.ComponentAdded -= OnComponentAdded;
                changeService.ComponentRemoved -= OnComponentRemoved;
                changeService.ComponentChanged -= OnComponentChanged;
            }
            base.Dispose(disposing);
        }

        public override SelectionRules SelectionRules
        {
            get
            {
                // 在设计时锁定位置, 但允许调整高度
                return SelectionRules.Visible | SelectionRules.BottomSizeable;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            properties.Remove("Location");
            properties.Remove("Dock");
            properties.Remove("Anchor");
            properties.Remove("AutoScroll");
        }

        // 改进的命中测试 - 允许点击内容面板
        protected override bool GetHitTest(Point point)
        {
            if (Component is FluentPanelListItem item && item.ContentPanel != null)
            {
                // 将点转换为屏幕坐标
                var screenPoint = item.PointToScreen(point);

                // 检查是否在内容面板区域内
                var contentRect = item.ContentPanel.RectangleToScreen(item.ContentPanel.ClientRectangle);

                if (contentRect.Contains(screenPoint))
                {
                    // 在内容面板内, 允许选择和拖放
                    return false;
                }
            }

            // 在标题栏区域, 不允许添加控件
            return true;
        }

        public override bool CanParent(Control control)
        {
            // 允许除了 FluentPanelListItem 之外的所有控件
            return !(control is FluentPanelListItem);
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            if (Component is FluentPanelListItem item && item.ContentPanel != null)
            {
                // 获取拖放的位置
                Point clientPoint = item.PointToClient(new Point(de.X, de.Y));
                var contentRect = item.ContentPanel.Bounds;

                if (contentRect.Contains(clientPoint))
                {
                    // 在内容面板内, 允许拖放
                    base.OnDragDrop(de);
                    return;
                }
            }

            de.Effect = DragDropEffects.None;
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            if (Component is FluentPanelListItem item && item.ContentPanel != null)
            {
                Point clientPoint = item.PointToClient(new Point(de.X, de.Y));
                var contentRect = item.ContentPanel.Bounds;

                if (contentRect.Contains(clientPoint))
                {
                    base.OnDragEnter(de);
                    return;
                }
            }

            de.Effect = DragDropEffects.None;
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            if (Component is FluentPanelListItem item && item.ContentPanel != null)
            {
                Point clientPoint = item.PointToClient(new Point(de.X, de.Y));
                var contentRect = item.ContentPanel.Bounds;

                if (contentRect.Contains(clientPoint))
                {
                    base.OnDragOver(de);
                    return;
                }
            }

            de.Effect = DragDropEffects.None;
        }
    }

    public class FluentPanelListActionList : DesignerActionList
    {
        private FluentPanelList control;
        private DesignerActionUIService designerService;

        public FluentPanelListActionList(IComponent component) : base(component)
        {
            control = component as FluentPanelList;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 布局分组
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Dock", "Dock", "布局", "设置控件Docking模式"));
            items.Add(new DesignerActionPropertyItem("PanelSpacing", "面板间距", "布局"));
            items.Add(new DesignerActionMethodItem(this, "RefreshLayout", "刷新布局", "布局", "强制刷新控件布局和显示"));

            // 操作分组
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "EditPanels", "编辑面板...", "操作", true));
            items.Add(new DesignerActionMethodItem(this, "AddPanel", "添加面板", "操作", false));
            items.Add(new DesignerActionMethodItem(this, "ExpandAll", "展开所有", "操作", false));
            items.Add(new DesignerActionMethodItem(this, "CollapseAll", "折叠所有", "操作", false));

            return items;
        }

        #region 属性

        public int PanelSpacing
        {
            get => control.PanelSpacing;
            set
            {
                SetProperty("PanelSpacing", value);
            }
        }

        public DockStyle Dock
        {
            get => control.Dock;
            set => SetProperty("Dock", value);
        }

        #endregion

        #region 方法

        private void EditPanels()
        {
            // 直接获取属性编辑器
            var propertyDescriptor = TypeDescriptor.GetProperties(control)["Panels"];
            if (propertyDescriptor != null)
            {
                var editor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (editor != null)
                {
                    var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                    var context = new TypeDescriptorContext(control, propertyDescriptor, host);

                    editor.EditValue(context, context, control.Panels);

                    // 刷新布局和显示
                    control.UpdatePanelLayout();
                    control.Refresh();

                    // 刷新设计器
                    designerService?.Refresh(control);
                }
            }
        }

        private void AddPanel()
        {
            var panel = new FluentPanelListItem
            {
                Title = "面板" + (control.Panels.Count + 1),
                IsExpanded = true
            };

            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            var propertyDescriptor = TypeDescriptor.GetProperties(control)["Panels"];
            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;

            changeService?.OnComponentChanging(control, propertyDescriptor);

            // 添加面板
            control.Panels.Add(panel);

            // 确保面板在控件树中
            if (!control.Controls.Contains(panel))
            {
                control.Controls.Add(panel);
            }

            changeService?.OnComponentChanged(control, propertyDescriptor, null, null);

            // 通知设计器服务
            designerService?.Refresh(control);

            // 选中新添加的面板
            var selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null && host != null)
            {
                // 将面板添加到设计器
                if (panel.Site == null)
                {
                    host.Container.Add(panel, null);
                }

                // 选中面板的内容区域, 方便添加控件
                selectionService.SetSelectedComponents(new[] { panel.ContentPanel }, SelectionTypes.Replace);

                // 更新布局
                control.UpdatePanelLayout();
            }
            // 刷新显示
            control.Refresh();
        }

        public void RefreshLayout()
        {
            try
            {
                var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;

                // 1. 触发尺寸变更(这是最可靠的方式)
                var sizeProperty = TypeDescriptor.GetProperties(control)["Size"];
                if (changeService != null && sizeProperty != null)
                {
                    var originalSize = control.Size;

                    changeService.OnComponentChanging(control, sizeProperty);

                    // 临时增加1像素
                    control.Size = new Size(originalSize.Width + 1, originalSize.Height + 1);
                    System.Windows.Forms.Application.DoEvents();

                    // 恢复原始尺寸
                    control.Size = originalSize;
                    System.Windows.Forms.Application.DoEvents();

                    changeService.OnComponentChanged(control, sizeProperty, originalSize, originalSize);
                }

                // 2. 强制重新布局
                control.SuspendLayout();
                control.PerformLayout();
                control.ResumeLayout(true);

                // 3. 递归刷新所有子控件
                RefreshControlRecursive(control);

                // 4. 强制设计器表面重绘
                if (host != null)
                {
                    var view = host.GetDesigner(host.RootComponent) as IDesigner;
                    if (view != null)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }

                // 5. 刷新设计器服务
                designerService?.Refresh(control);

                // 6. 强制父容器刷新
                control.Parent?.Refresh();

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 递归刷新控件
        /// </summary>
        private void RefreshControlRecursive(Control ctrl)
        {
            if (ctrl == null || ctrl.IsDisposed)
                return;

            ctrl.Invalidate(true);
            ctrl.Update();

            foreach (Control child in ctrl.Controls)
            {
                RefreshControlRecursive(child);
            }
        }

        /// <summary>
        /// 生成唯一的组件名称
        /// </summary>
        private string GenerateUniqueName(FluentPanelListItem panel)
        {
            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host?.Container == null)
            {
                return null;
            }

            string baseName = "fluentPanelListItem";
            int index = 1;
            string name;

            do
            {
                name = baseName + index;
                index++;
            }
            while (host.Container.Components[name] != null);

            return name;
        }


        private void ExpandAll()
        {
            control.ExpandAll();
            control.Invalidate();
        }

        private void CollapseAll()
        {
            control.CollapseAll();
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
