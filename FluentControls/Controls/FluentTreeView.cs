using System;
using System.Collections.Generic;
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
using Infrastructure;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FluentControls.Controls
{

    [Designer(typeof(FluentTreeViewDesigner))]
    [DefaultProperty("Nodes")]
    [DefaultEvent("AfterSelect")]
    [Docking(DockingBehavior.Ask)]
    public class FluentTreeView : FluentControlBase
    {
        private FluentTreeNodeCollection nodes;
        private FluentTreeNode selectedNode;
        private FluentTreeNode hoveredNode;
        private FluentTreeNode dragNode;
        private FluentTreeNode dropNode;
        private DragDropEffects dragEffect;

        // 搜索相关
        private FluentTextBox searchBox;
        private bool showSearchBox = false;
        private string searchText = "";
        private Timer searchTimer;
        private List<FluentTreeNode> searchResults;

        // 布局相关
        private VScrollBar vScrollBar;
        private int scrollOffset = 0;
        private int totalHeight = 0;
        private Rectangle treeArea;
        private Dictionary<FluentTreeNode, Rectangle> nodeBounds;

        // 样式设置
        private int nodeHeight = 24;
        private int nodeIndent = 20;
        private int iconSize = 16;
        private bool showLines = true;
        private bool showPlusMinus = true;
        private bool showRootLines = true;
        private bool checkBoxes = false;
        private bool fullRowSelect = true;
        private bool hotTracking = true;
        private bool allowDrop = false;

        // 颜色设置
        private Color nodeBackColor = Color.Transparent;
        private Color nodeHoverColor = Color.FromArgb(30, 0, 120, 215);
        private Color nodeSelectedColor = Color.FromArgb(50, 0, 120, 215);
        private Color nodeFocusedColor = Color.FromArgb(70, 0, 120, 215);
        private Color lineColor = Color.FromArgb(200, 200, 200);

        // 事件
        public delegate void TreeViewEventHandler(object sender, TreeViewEventArgs e);
        public delegate void TreeNodeMouseClickEventHandler(object sender, TreeNodeMouseClickEventArgs e);
        public delegate void TreeNodeDragEventHandler(object sender, TreeNodeDragEventArgs e);

        #region 构造函数

        public FluentTreeView()
        {
            SetStyle(ControlStyles.Selectable, true);
            nodes = new FluentTreeNodeCollection(null,this);
            nodeBounds = new Dictionary<FluentTreeNode, Rectangle>();

            InitializeScrollBar();
            InitializeSearchBox();
            InitializeSearchTimer();
        }

        private void InitializeScrollBar()
        {
            vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            vScrollBar.Scroll += OnScroll;
            Controls.Add(vScrollBar);
        }

        private void InitializeSearchBox()
        {
            searchBox = new FluentTextBox
            {
                Placeholder = "搜索...",
                Height = 30,
                Visible = false
            };
            searchBox.TextChanged += OnSearchTextChanged;
            searchBox.KeyDown += OnSearchBoxKeyDown;
            Controls.Add(searchBox);
        }

        private void InitializeSearchTimer()
        {
            searchTimer = new Timer { Interval = 300 };
            searchTimer.Tick += (s, e) =>
            {
                searchTimer.Stop();
                PerformSearch();
            };
        }

        #endregion

        #region 属性

        [Category("数据")]
        [Description("树的根节点集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentTreeNodeCollectionEditor), typeof(UITypeEditor))]
        public FluentTreeNodeCollection Nodes
        {
            get => nodes;
            set
            {
                if (nodes != value)
                {
                    nodes = value;
                    if (nodes != null)
                    {
                        // 设置集合的 TreeView 引用
                        nodes.SetTreeView(this);
                        // 初始化所有节点的引用
                        InitializeNodesTreeViewReference();
                    }
                    UpdateLayout();
                }
            }
        }

        [Browsable(false)]
        public FluentTreeNode SelectedNode
        {
            get => selectedNode;
            set
            {
                if (selectedNode != value)
                {
                    var oldNode = selectedNode;
                    selectedNode = value;

                    if (oldNode != null)
                    {
                        oldNode.isSelected = false;
                        InvalidateNode(oldNode);
                    }

                    if (selectedNode != null)
                    {
                        selectedNode.isSelected = true;
                        InvalidateNode(selectedNode);
                        EnsureNodeVisible(selectedNode);
                    }

                    OnAfterSelect(new TreeViewEventArgs(selectedNode));
                }
            }
        }

        [Category("外观")]
        [DefaultValue(DockStyle.None)]
        public new DockStyle Dock
        {
            get => base.Dock;
            set => base.Dock = value;
        }

        [Category("Fluent TreeView")]
        [DefaultValue(24)]
        [Description("节点高度")]
        public int NodeHeight
        {
            get => nodeHeight;
            set
            {
                if (nodeHeight != value && value > 0)
                {
                    nodeHeight = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(20)]
        [Description("节点缩进")]
        public int NodeIndent
        {
            get => nodeIndent;
            set
            {
                if (nodeIndent != value && value >= 0)
                {
                    nodeIndent = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(true)]
        [Description("显示连接线")]
        public bool ShowLines
        {
            get => showLines;
            set
            {
                if (showLines != value)
                {
                    showLines = value;
                    Invalidate();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(true)]
        [Description("显示展开/折叠按钮")]
        public bool ShowPlusMinus
        {
            get => showPlusMinus;
            set
            {
                if (showPlusMinus != value)
                {
                    showPlusMinus = value;
                    Invalidate();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(false)]
        [Description("显示复选框")]
        public bool CheckBoxes
        {
            get => checkBoxes;
            set
            {
                if (checkBoxes != value)
                {
                    checkBoxes = value;
                    Invalidate();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(true)]
        [Description("整行选择")]
        public bool FullRowSelect
        {
            get => fullRowSelect;
            set
            {
                if (fullRowSelect != value)
                {
                    fullRowSelect = value;
                    Invalidate();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(false)]
        [Description("显示搜索框")]
        public bool ShowSearchBox
        {
            get => showSearchBox;
            set
            {
                if (showSearchBox != value)
                {
                    showSearchBox = value;
                    searchBox.Visible = value;
                    UpdateLayout();
                }
            }
        }

        [Category("Fluent TreeView")]
        [DefaultValue(false)]
        [Description("允许拖放")]
        public new bool AllowDrop
        {
            get => allowDrop;
            set
            {
                allowDrop = value;
                base.AllowDrop = value;
            }
        }

        #endregion

        #region 数据

        /// <summary>
        /// 绑定层次结构数据
        /// </summary>
        public void BindHierarchicalData<T>(IEnumerable<IHierarchicalItem<T>> items)
        {
            Nodes.Clear();

            foreach (var item in items)
            {
                Nodes.Add(item.ToTreeNode());
            }

            UpdateLayout();
        }

        /// <summary>
        /// 绑定数据源
        /// </summary>
        public void BindData<T>(
            IEnumerable<T> dataSource,
            Func<T, IEnumerable<T>> childSelector,
            Func<T, string> displayTextSelector = null,
            Func<T, Image> iconSelector = null,
            Func<T, bool> isEnabledSelector = null,
            Func<T, bool> isExpandedSelector = null,
            Func<T, object> tagSelector = null)
        {
            Nodes.Clear();

            foreach (var item in dataSource)
            {
                var hierarchicalItem = item.AsHierarchical(
                    childSelector,
                    displayTextSelector,
                    iconSelector,
                    isEnabledSelector,
                    isExpandedSelector,
                    tagSelector);

                Nodes.Add(hierarchicalItem.ToTreeNode());
            }

            UpdateLayout();
        }

        /// <summary>
        /// 查找节点通过Tag
        /// </summary>
        public FluentTreeNode FindNodeByTag(object tag)
        {
            return FindNodeByTagRecursive(Nodes, tag);
        }

        private FluentTreeNode FindNodeByTagRecursive(FluentTreeNodeCollection nodes, object tag)
        {
            foreach (var node in nodes)
            {
                if (Equals(node.Tag, tag))
                {
                    return node;
                }

                var found = FindNodeByTagRecursive(node.Nodes, tag);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有选中的数据项
        /// </summary>
        public IEnumerable<T> GetCheckedItems<T>() where T : class
        {
            var nodes = GetCheckedNodesRecursive(Nodes);
            return nodes.Select(n => n.Tag as T).Where(t => t != null);
        }

        private IEnumerable<FluentTreeNode> GetCheckedNodesRecursive(FluentTreeNodeCollection nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Checked)
                {
                    yield return node;
                }

                foreach (var child in GetCheckedNodesRecursive(node.Nodes))
                {
                    yield return child;
                }
            }
        }

        #endregion

        #region 布局和绘制

        public void ExpandAll()
        {
            foreach (FluentTreeNode node in Nodes)
            {
                node.ExpandAll();
            }
        }

        public void CollapseAll()
        {
            foreach (FluentTreeNode node in Nodes)
            {
                node.CollapseAll();
            }
        }

        public void UpdateAll()
        {
            UpdateLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            InitializeNodesTreeViewReference();

            // 确保初始布局计算
            UpdateLayout();
        }

        private void InitializeNodesTreeViewReference()
        {
            InitializeNodeTreeViewRecursive(nodes);
        }

        private void InitializeNodeTreeViewRecursive(FluentTreeNodeCollection nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                // 设置 TreeView 引用
                node.TreeView = this;

                // 递归处理子节点
                if (node.Nodes != null && node.Nodes.Count > 0)
                {
                    InitializeNodeTreeViewRecursive(node.Nodes);
                }
            }
        }

        internal void UpdateLayout()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            // 更新搜索框位置
            if (searchBox != null && searchBox.Visible)
            {
                searchBox.Location = new Point(0, 0);
                searchBox.Width = Width - (vScrollBar.Visible ? vScrollBar.Width : 0);
            }

            // 更新树区域
            int top = showSearchBox && searchBox != null && searchBox.Visible ? searchBox.Bottom + 2 : 0;
            treeArea = new Rectangle(0, top, Width - (vScrollBar.Visible ? vScrollBar.Width : 0), Height - top);

            // 计算总高度
            totalHeight = CalculateTotalHeight();

            // 更新滚动条
            UpdateScrollBar();

            Invalidate();
        }

        private int CalculateTotalHeight()
        {
            int height = 0;
            nodeBounds.Clear();
            CalculateNodeBounds(nodes, ref height, 0);
            return height;
        }

        private void CalculateNodeBounds(FluentTreeNodeCollection nodes, ref int yOffset, int level)
        {
            foreach (var node in nodes)
            {
                if (!node.IsVisible)
                {
                    continue;
                }

                int x = level * nodeIndent + 5;
                var bounds = new Rectangle(x, yOffset - scrollOffset + treeArea.Top, treeArea.Width - x, nodeHeight);
                nodeBounds[node] = bounds;

                yOffset += nodeHeight;

                if (node.IsExpanded && node.Nodes.Count > 0)
                {
                    CalculateNodeBounds(node.Nodes, ref yOffset, level + 1);
                }
            }
        }

        private void UpdateScrollBar()
        {
            if (totalHeight > treeArea.Height)
            {
                vScrollBar.Visible = true;
                vScrollBar.Maximum = totalHeight - treeArea.Height;
                vScrollBar.LargeChange = treeArea.Height;
                vScrollBar.SmallChange = nodeHeight;
            }
            else
            {
                vScrollBar.Visible = false;
                scrollOffset = 0;
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            using (var brush = new SolidBrush(GetThemeColor(c => c.Surface, BackColor)))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (nodes.Count == 0)
            {
                return;
            }

            g.SetClip(treeArea);

            // 绘制节点
            DrawNodes(g, nodes, 0);

            g.ResetClip();
        }

        private void DrawNodes(Graphics g, FluentTreeNodeCollection nodes, int level)
        {
            foreach (var node in nodes)
            {
                if (!node.IsVisible || !nodeBounds.ContainsKey(node))
                {
                    continue;
                }

                var bounds = nodeBounds[node];

                // 检查是否在可见区域
                if (bounds.Bottom < treeArea.Top || bounds.Top > treeArea.Bottom)
                {
                    if (node.IsExpanded && node.Nodes.Count > 0)
                    {
                        DrawNodes(g, node.Nodes, level + 1);
                    }
                    continue;
                }

                DrawNode(g, node, bounds, level);

                if (node.IsExpanded && node.Nodes.Count > 0)
                {
                    DrawNodes(g, node.Nodes, level + 1);
                }
            }
        }

        private void DrawNode(Graphics g, FluentTreeNode node, Rectangle bounds, int level)
        {
            // 绘制背景
            DrawNodeBackground(g, node, bounds);

            int x = bounds.X;

            // 绘制连接线
            if (showLines)
            {
                DrawNodeLines(g, node, bounds, level);
            }

            // 绘制展开/折叠按钮
            if (showPlusMinus && node.Nodes.Count > 0)
            {
                var plusMinusBounds = new Rectangle(x + 2, bounds.Y + (bounds.Height - 12) / 2, 12, 12);
                DrawPlusMinus(g, plusMinusBounds, node.IsExpanded);
                x += 20;
            }
            else
            {
                x += showPlusMinus ? 20 : 5;
            }

            // 绘制复选框
            if (checkBoxes)
            {
                var checkBounds = new Rectangle(x, bounds.Y + (bounds.Height - 16) / 2, 16, 16);
                DrawCheckBox(g, checkBounds, node.CheckState, node.IsEnabled);
                x += 20;
            }

            // 绘制图标
            if (node.Icon != null)
            {
                var iconBounds = new Rectangle(x, bounds.Y + (bounds.Height - iconSize) / 2, iconSize, iconSize);
                g.DrawImage(node.Icon, iconBounds);
                x += iconSize + 4;
            }

            // 绘制文本
            DrawNodeText(g, node, new Rectangle(x, bounds.Y, bounds.Right - x, bounds.Height));
        }

        private void DrawNodeBackground(Graphics g, FluentTreeNode node, Rectangle bounds)
        {
            Color backColor = nodeBackColor;

            if (!node.IsEnabled)
            {
                // 禁用状态不绘制背景
                return;
            }

            if (node == selectedNode)
            {
                backColor = Focused ? nodeFocusedColor : nodeSelectedColor;
            }
            else if (node == hoveredNode)
            {
                backColor = nodeHoverColor;
            }
            else if (node.IsHighlighted)
            {
                backColor = Color.FromArgb(40, Color.Yellow);
            }

            if (backColor != Color.Transparent)
            {
                if (fullRowSelect)
                {
                    bounds = new Rectangle(treeArea.X, bounds.Y, treeArea.Width, bounds.Height);
                }

                using (var brush = new SolidBrush(backColor))
                {
                    if (UseTheme && Theme?.Elevation != null)
                    {
                        int radius = Theme.Elevation.CornerRadiusSmall;
                        using (var path = GetRoundedRectangle(bounds, radius))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        g.FillRectangle(brush, bounds);
                    }
                }
            }
        }

        private void DrawNodeLines(Graphics g, FluentTreeNode node, Rectangle bounds, int level)
        {
            using (var pen = new Pen(lineColor))
            {
                pen.DashStyle = DashStyle.Dot;

                int x = bounds.X - nodeIndent + 10;
                int y = bounds.Y + bounds.Height / 2;

                // 水平线
                g.DrawLine(pen, x + 10, y, bounds.X, y);

                // 垂直线
                if (node.Parent != null || showRootLines)
                {
                    bool isLast = IsLastNode(node);
                    int top = bounds.Y;
                    int bottom = isLast ? y : bounds.Bottom;

                    g.DrawLine(pen, x, top, x, bottom);

                    // 绘制父节点的延续线
                    var parent = node.Parent;
                    int parentLevel = level - 1;
                    while (parent != null)
                    {
                        if (!IsLastNode(parent))
                        {
                            int px = bounds.X - (level - parentLevel) * nodeIndent + 10;
                            g.DrawLine(pen, px, bounds.Y, px, bounds.Bottom);
                        }
                        parent = parent.Parent;
                        parentLevel--;
                    }
                }
            }
        }

        private bool IsLastNode(FluentTreeNode node)
        {
            var parent = node.Parent;
            var collection = parent?.Nodes ?? nodes;
            return collection.Count > 0 && collection[collection.Count - 1] == node;
        }

        private void DrawPlusMinus(Graphics g, Rectangle bounds, bool expanded)
        {
            var color = GetThemeColor(c => c.Border, Color.Gray);

            using (var pen = new Pen(color))
            using (var brush = new SolidBrush(GetThemeColor(c => c.Surface, Color.White)))
            {
                g.FillRectangle(brush, bounds);
                g.DrawRectangle(pen, bounds);

                // 绘制横线
                int centerY = bounds.Y + bounds.Height / 2;
                g.DrawLine(pen, bounds.X + 3, centerY, bounds.Right - 3, centerY);

                // 绘制竖线（仅在折叠时）
                if (!expanded)
                {
                    int centerX = bounds.X + bounds.Width / 2;
                    g.DrawLine(pen, centerX, bounds.Y + 3, centerX, bounds.Bottom - 3);
                }
            }
        }

        private void DrawCheckBox(Graphics g, Rectangle bounds, TreeNodeState state, bool enabled)
        {
            var borderColor = enabled ? GetThemeColor(c => c.Border, Color.Gray) : Color.LightGray;
            var backColor = GetThemeColor(c => c.Surface, Color.White);

            using (var brush = new SolidBrush(backColor))
            using (var pen = new Pen(borderColor, 1))
            {
                g.FillRectangle(brush, bounds);
                g.DrawRectangle(pen, bounds);

                if (state == TreeNodeState.Checked)
                {
                    // 绘制勾选标记
                    using (var checkPen = new Pen(enabled ? GetThemeColor(c => c.Primary, Color.Black) : Color.Gray, 2))
                    {
                        g.DrawLine(checkPen, bounds.X + 3, bounds.Y + 8, bounds.X + 6, bounds.Y + 11);
                        g.DrawLine(checkPen, bounds.X + 6, bounds.Y + 11, bounds.X + 13, bounds.Y + 4);
                    }
                }
                else if (state == TreeNodeState.Indeterminate)
                {
                    // 绘制不确定状态
                    using (var indBrush = new SolidBrush(enabled ? GetThemeColor(c => c.Primary, Color.Gray) : Color.LightGray))
                    {
                        g.FillRectangle(indBrush, bounds.X + 4, bounds.Y + 7, 8, 2);
                    }
                }
            }
        }

        private void DrawNodeText(Graphics g, FluentTreeNode node, Rectangle bounds)
        {
            var foreColor = node.ForeColor ?? GetThemeColor(c => c.TextPrimary, ForeColor);
            if (!node.IsEnabled)
            {
                foreColor = GetThemeColor(c => c.TextDisabled, Color.Gray);
            }

            var font = node.NodeFont ?? GetThemeFont(t => t.Body, Font);

            TextRenderer.DrawText(g, node.Text, font, bounds, foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void DrawBorder(Graphics g)
        {
            if (UseTheme && Theme != null)
            {
                var borderColor = Focused
                    ? Theme.Colors.BorderFocused
                    : Theme.Colors.Border;

                using (var pen = new Pen(borderColor, 1))
                {
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragNode != null && allowDrop)
            {
                // 处理拖拽
                HandleDragOver(e.Location);
                return;
            }

            var node = GetNodeAt(e.Location);
            if (node != hoveredNode)
            {
                var oldNode = hoveredNode;
                hoveredNode = node;

                if (oldNode != null)
                {
                    InvalidateNode(oldNode);
                }

                if (hoveredNode != null)
                {
                    InvalidateNode(hoveredNode);
                }

                if (hotTracking)
                {
                    Cursor = hoveredNode != null ? Cursors.Hand : Cursors.Default;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!treeArea.Contains(e.Location))
            {
                return;
            }

            var node = GetNodeAt(e.Location);
            if (node == null)
            {
                return;
            }

            var nodeBound = nodeBounds[node];
            int x = nodeBound.X;

            // 检查点击展开/折叠按钮
            if (showPlusMinus && node.Nodes.Count > 0)
            {
                var plusMinusBounds = new Rectangle(x + 2, nodeBound.Y + (nodeBound.Height - 12) / 2, 12, 12);
                if (plusMinusBounds.Contains(e.Location))
                {
                    node.Toggle();
                    return;
                }
                x += 20;
            }
            else
            {
                x += showPlusMinus ? 20 : 5;
            }

            // 检查点击复选框
            if (checkBoxes)
            {
                var checkBounds = new Rectangle(x, nodeBound.Y + (nodeBound.Height - 16) / 2, 16, 16);
                if (checkBounds.Contains(e.Location))
                {
                    if (node.IsEnabled)
                    {
                        ToggleNodeCheck(node);
                    }
                    return;
                }
            }

            // 选择节点
            if (node.IsEnabled)
            {
                SelectedNode = node;

                // 开始拖拽
                if (allowDrop && e.Button == MouseButtons.Left)
                {
                    dragNode = node;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (dragNode != null && dropNode != null && dragNode != dropNode)
            {
                PerformDragDrop(dragNode, dropNode);
            }

            dragNode = null;
            dropNode = null;
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var node = GetNodeAt(e.Location);
            if (node != null && node.IsEnabled)
            {
                if (node.Nodes.Count > 0)
                {
                    node.Toggle();
                }
                OnNodeMouseDoubleClick(new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredNode != null)
            {
                var oldNode = hoveredNode;
                hoveredNode = null;
                InvalidateNode(oldNode);
            }
        }

        #endregion

        #region 键盘事件

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Ctrl+F 打开搜索
            if (e.Control && e.KeyCode == Keys.F)
            {
                ShowSearchBox = true;
                searchBox.Focus();
                e.Handled = true;
                return;
            }

            if (selectedNode == null)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Up:
                    SelectPreviousNode();
                    e.Handled = true;
                    break;

                case Keys.Down:
                    SelectNextNode();
                    e.Handled = true;
                    break;

                case Keys.Left:
                    if (selectedNode.IsExpanded && selectedNode.Nodes.Count > 0)
                    {
                        selectedNode.Collapse();
                    }
                    else if (selectedNode.Parent != null)
                    {
                        SelectedNode = selectedNode.Parent;
                    }

                    e.Handled = true;
                    break;

                case Keys.Right:
                    if (!selectedNode.IsExpanded && selectedNode.Nodes.Count > 0)
                    {
                        selectedNode.Expand();
                    }
                    else if (selectedNode.Nodes.Count > 0)
                    {
                        SelectedNode = selectedNode.Nodes[0];
                    }

                    e.Handled = true;
                    break;

                case Keys.Space:
                    if (checkBoxes && selectedNode.IsEnabled)
                    {
                        ToggleNodeCheck(selectedNode);
                        e.Handled = true;
                    }
                    break;

                case Keys.Enter:
                    if (selectedNode.Nodes.Count > 0)
                    {
                        selectedNode.Toggle();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ShowSearchBox = false;
                Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                NavigateToNextSearchResult();
            }
        }

        #endregion

        #region 节点操作

        private FluentTreeNode GetNodeAt(Point point)
        {
            if (!treeArea.Contains(point))
            {
                return null;
            }

            foreach (var kvp in nodeBounds)
            {
                if (kvp.Value.Contains(point))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        private void SelectPreviousNode()
        {
            var visibleNodes = GetVisibleNodes().ToList();
            int index = visibleNodes.IndexOf(selectedNode);
            if (index > 0)
            {
                SelectedNode = visibleNodes[index - 1];
            }
        }

        private void SelectNextNode()
        {
            var visibleNodes = GetVisibleNodes().ToList();
            int index = visibleNodes.IndexOf(selectedNode);
            if (index >= 0 && index < visibleNodes.Count - 1)
            {
                SelectedNode = visibleNodes[index + 1];
            }
        }

        private IEnumerable<FluentTreeNode> GetVisibleNodes()
        {
            return GetVisibleNodesRecursive(nodes);
        }

        private IEnumerable<FluentTreeNode> GetVisibleNodesRecursive(FluentTreeNodeCollection nodes)
        {
            foreach (var node in nodes)
            {
                if (node.IsVisible)
                {
                    yield return node;
                    if (node.IsExpanded)
                    {
                        foreach (var child in GetVisibleNodesRecursive(node.Nodes))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        private void ToggleNodeCheck(FluentTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            TreeNodeState newState;
            switch (node.CheckState)
            {
                case TreeNodeState.Unchecked:
                    newState = TreeNodeState.Checked;
                    break;
                case TreeNodeState.Checked:
                    newState = TreeNodeState.Unchecked;
                    break;
                case TreeNodeState.Indeterminate:
                    // 不确定状态切换为勾选
                    newState = TreeNodeState.Checked;
                    break;
                default:
                    return;
            }

            node.CheckState = newState;
            // 递归更新所有子节点
            if (node.Nodes.Count > 0)
            {
                node.UpdateChildrenCheckStateRecursive(newState);
            }
            // 向上更新父节点状态
            node.Parent?.UpdateParentCheckState();
            OnAfterCheck(new TreeViewEventArgs(node));
            Invalidate();
        }

        internal void InvalidateNode(FluentTreeNode node)
        {
            if (node != null && nodeBounds.ContainsKey(node))
            {
                Invalidate(nodeBounds[node]);
            }
        }

        internal void EnsureNodeVisible(FluentTreeNode node)
        {
            if (node == null || !nodeBounds.ContainsKey(node))
            {
                return;
            }

            var bounds = nodeBounds[node];

            if (bounds.Top < treeArea.Top)
            {
                scrollOffset -= (treeArea.Top - bounds.Top);
                vScrollBar.Value = scrollOffset;
                UpdateLayout();
            }
            else if (bounds.Bottom > treeArea.Bottom)
            {
                scrollOffset += (bounds.Bottom - treeArea.Bottom);
                vScrollBar.Value = scrollOffset;
                UpdateLayout();
            }
        }

        internal void OnNodesChanged()
        {
            UpdateLayout();
        }

        internal void OnNodeExpandedChanged(FluentTreeNode node)
        {
            UpdateLayout();
            Invalidate();
            OnAfterExpand(new TreeViewEventArgs(node));
        }

        internal void OnNodeCheckStateChanged(FluentTreeNode node)
        {
            OnAfterCheck(new TreeViewEventArgs(node));
            InvalidateNode(node);
        }

        #endregion

        #region 搜索功能

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            searchText = searchBox.Text;
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void PerformSearch()
        {
            // 清除之前的高亮
            if (searchResults != null)
            {
                foreach (var node in searchResults)
                {
                    node.IsHighlighted = false;
                }
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                searchResults = null;
                Invalidate();
                return;
            }

            // 执行搜索
            searchResults = new List<FluentTreeNode>();
            SearchNodes(nodes, searchText.ToLower());

            // 高亮搜索结果
            foreach (var node in searchResults)
            {
                node.IsHighlighted = true;
                node.EnsureVisible();
            }

            // 选择第一个结果
            if (searchResults.Count > 0)
            {
                SelectedNode = searchResults[0];
            }

            Invalidate();
        }

        private void SearchNodes(FluentTreeNodeCollection nodes, string searchText)
        {
            foreach (var node in nodes)
            {
                if (node.Text.ToLower().Contains(searchText))
                {
                    searchResults.Add(node);
                }

                if (node.Nodes.Count > 0)
                {
                    SearchNodes(node.Nodes, searchText);
                }
            }
        }

        private void NavigateToNextSearchResult()
        {
            if (searchResults == null || searchResults.Count == 0)
            {
                return;
            }

            int currentIndex = searchResults.IndexOf(selectedNode);
            int nextIndex = (currentIndex + 1) % searchResults.Count;
            SelectedNode = searchResults[nextIndex];
        }

        public void ClearSearch()
        {
            searchText = "";
            searchBox.Text = "";
            if (searchResults != null)
            {
                foreach (var node in searchResults)
                {
                    node.IsHighlighted = false;
                }
                searchResults = null;
            }
            Invalidate();
        }

        #endregion

        #region 拖放功能

        private void HandleDragOver(Point location)
        {
            var targetNode = GetNodeAt(location);

            if (targetNode != dropNode)
            {
                dropNode = targetNode;
                Invalidate();
            }

            // 确定拖放效果
            if (targetNode == null || targetNode == dragNode || IsChildNode(dragNode, targetNode))
            {
                dragEffect = DragDropEffects.None;
            }
            else
            {
                dragEffect = DragDropEffects.Move;
            }
        }

        private bool IsChildNode(FluentTreeNode parent, FluentTreeNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current == parent)
                {
                    return true;
                }

                current = current.Parent;
            }
            return false;
        }

        private void PerformDragDrop(FluentTreeNode source, FluentTreeNode target)
        {
            if (source == null || target == null || source == target)
            {
                return;
            }

            if (IsChildNode(source, target))
            {
                return;
            }

            // 触发拖放前事件
            var args = new TreeNodeDragEventArgs(source, target);
            OnBeforeNodeDrop(args);

            if (args.Cancel)
            {
                return;
            }

            // 执行拖放
            source.Remove();
            target.Nodes.Add(source);
            target.Expand();

            // 触发拖放后事件
            OnAfterNodeDrop(new TreeNodeDragEventArgs(source, target));

            SelectedNode = source;
        }

        #endregion

        #region 滚动

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            scrollOffset = e.NewValue;
            UpdateLayout();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!vScrollBar.Visible)
            {
                return;
            }

            int newValue = scrollOffset - (e.Delta / 120 * nodeHeight * 3);
            newValue = Math.Max(0, Math.Min(vScrollBar.Maximum, newValue));

            if (newValue != scrollOffset)
            {
                scrollOffset = newValue;
                vScrollBar.Value = scrollOffset;
                UpdateLayout();
            }
        }

        #endregion

        #region 事件

        public event TreeViewEventHandler AfterSelect;
        public event TreeViewEventHandler AfterCheck;
        public event TreeViewEventHandler AfterExpand;
        public event TreeNodeMouseClickEventHandler NodeMouseDoubleClick;
        public event TreeNodeDragEventHandler BeforeNodeDrop;
        public event TreeNodeDragEventHandler AfterNodeDrop;

        protected virtual void OnAfterSelect(TreeViewEventArgs e)
        {
            AfterSelect?.Invoke(this, e);
        }

        protected virtual void OnAfterCheck(TreeViewEventArgs e)
        {
            AfterCheck?.Invoke(this, e);
        }

        protected virtual void OnAfterExpand(TreeViewEventArgs e)
        {
            AfterExpand?.Invoke(this, e);
        }

        protected virtual void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            NodeMouseDoubleClick?.Invoke(this, e);
        }

        protected virtual void OnBeforeNodeDrop(TreeNodeDragEventArgs e)
        {
            BeforeNodeDrop?.Invoke(this, e);
        }

        protected virtual void OnAfterNodeDrop(TreeNodeDragEventArgs e)
        {
            AfterNodeDrop?.Invoke(this, e);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                searchTimer?.Dispose();
                searchBox?.Dispose();
                vScrollBar?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }


    #region 树节点

    /// <summary>
    /// Fluent树节点
    /// </summary>
    public class FluentTreeNode : ICloneable
    {
        private FluentTreeNodeCollection nodes;
        private FluentTreeNode parent;
        internal FluentTreeView treeView;
        private bool isExpanded = false;
        private bool isEnabled = true;
        internal bool isSelected = false;

        private TreeNodeState checkState = TreeNodeState.Unchecked;
        private object tag;
        private string text = "";
        private string name = "";
        private Image icon;
        private Color? foreColor;
        private Color? backColor;
        private Font nodeFont;
        private bool isVisible = true;
        private bool isHighlighted = false;

        public FluentTreeNode()
        {
            nodes = new FluentTreeNodeCollection(this, null);
        }

        public FluentTreeNode(string text) : this()
        {
            this.text = text;
        }

        public FluentTreeNode(string text, Image icon) : this(text)
        {
            this.icon = icon;
        }

        #region 属性

        public string Name
        {
            get => name;
            set => name = value;
        }

        [Category("外观")]
        [Description("节点显示的文本")]
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    InvalidateNode();
                }
            }
        }

        [Category("外观")]
        [Description("节点的图标")]
        [DefaultValue(null)]
        public Image Icon
        {
            get => icon;
            set
            {
                if (icon != value)
                {
                    icon = value;
                    InvalidateNode();
                }
            }
        }

        [Category("数据")]
        [Description("子节点集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentTreeNodeCollectionEditor), typeof(UITypeEditor))]
        public FluentTreeNodeCollection Nodes => nodes;

        [Browsable(false)]
        public FluentTreeNode Parent
        {
            get => parent;
            internal set => parent = value;
        }

        [Browsable(false)]
        public FluentTreeView TreeView
        {
            get
            {
                if (treeView != null)
                {
                    return treeView;
                }

                return parent?.TreeView;
            }
            internal set
            {
                treeView = value;

                if (nodes != null)
                {
                    nodes.SetTreeView(value);
                }
            }
        }

        [Category("行为")]
        [Description("节点是否展开")]
        [DefaultValue(false)]
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;

                    // 通知 TreeView
                    var treeView = TreeView;
                    if (treeView != null)
                    {
                        treeView.OnNodeExpandedChanged(this);
                    }


                    InvalidateNode();
                }
            }
        }

        [Browsable(false)]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    if (value && TreeView != null)
                    {
                        TreeView.SelectedNode = this;
                    }
                    InvalidateNode();
                }
            }
        }

        [Category("行为")]
        [Description("节点是否启用")]
        [DefaultValue(true)]
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    InvalidateNode();
                }
            }
        }

        [Category("行为")]
        [Description("节点的勾选状态")]
        public TreeNodeState CheckState
        {
            get => checkState;
            set
            {
                if (checkState != value)
                {
                    checkState = value;
                    TreeView?.OnNodeCheckStateChanged(this);
                    InvalidateNode();
                }
            }
        }

        [Category("行为")]
        [Description("节点是否勾选")]
        public bool Checked
        {
            get => checkState == TreeNodeState.Checked;
            set
            {
                //CheckState = value ? TreeNodeState.Checked : TreeNodeState.Unchecked;
                var newState = value ? TreeNodeState.Checked : TreeNodeState.Unchecked;
                if (checkState != newState)
                {
                    CheckState = newState;

                    // 同步更新子节点
                    if (TreeView?.CheckBoxes == true)
                    {
                        UpdateCheckState();
                    }
                }
            }
        }

        [Browsable(false)]
        public object Tag
        {
            get => tag;
            set => tag = value;
        }

        [Category("外观")]
        [Description("节点的前景色")]
        public Color? ForeColor
        {
            get => foreColor;
            set
            {
                if (foreColor != value)
                {
                    foreColor = value;
                    InvalidateNode();
                }
            }
        }

        [Category("外观")]
        [Description("节点的背景色")]
        public Color? BackColor
        {
            get => backColor;
            set
            {
                if (backColor != value)
                {
                    backColor = value;
                    InvalidateNode();
                }
            }
        }

        [Category("外观")]
        [Description("节点的字体")]
        public Font NodeFont
        {
            get => nodeFont;
            set
            {
                if (nodeFont != value)
                {
                    nodeFont = value;
                    InvalidateNode();
                }
            }
        }

        [Browsable(false)]
        public int Level
        {
            get
            {
                int level = 0;
                var node = parent;
                while (node != null)
                {
                    level++;
                    node = node.parent;
                }
                return level;
            }
        }

        [Browsable(false)]
        public bool IsVisible
        {
            get => isVisible;
            internal set => isVisible = value;
        }

        [Browsable(false)]
        public bool IsHighlighted
        {
            get => isHighlighted;
            internal set
            {
                if (isHighlighted != value)
                {
                    isHighlighted = value;
                    InvalidateNode();
                }
            }
        }

        [Browsable(false)]
        public string FullPath
        {
            get
            {
                var path = Text;
                var node = parent;
                while (node != null)
                {
                    path = node.Text + "\\" + path;
                    node = node.parent;
                }
                return path;
            }
        }

        #endregion

        #region 方法

        public void Expand()
        {
            if (nodes.Count > 0 && !isExpanded)
            {
                IsExpanded = true;
            }
        }

        public void Collapse()
        {
            if (isExpanded)
            {
                IsExpanded = false;
            }
        }

        public void Toggle()
        {
            if (nodes.Count > 0)
            {
                IsExpanded = !IsExpanded;
            }
        }

        public void ExpandAll()
        {
            IsExpanded = true;
            foreach (FluentTreeNode node in nodes)
            {
                node.ExpandAll();
            }
            InvalidateNode();
        }

        public void CollapseAll()
        {
            foreach (FluentTreeNode node in nodes)
            {
                node.CollapseAll();
            }
            IsExpanded = false;
            InvalidateNode();
        }

        public void Remove()
        {
            parent?.Nodes.Remove(this);
        }

        public void EnsureVisible()
        {
            var node = parent;
            while (node != null)
            {
                node.IsExpanded = true;
                node = node.parent;
            }
            TreeView?.EnsureNodeVisible(this);
        }

        public FluentTreeNode[] GetNodePath()
        {
            var path = new List<FluentTreeNode>();
            var node = this;
            while (node != null)
            {
                path.Insert(0, node);
                node = node.parent;
            }
            return path.ToArray();
        }

        public IEnumerable<FluentTreeNode> GetAllNodes()
        {
            yield return this;
            foreach (var child in nodes)
            {
                foreach (var descendant in child.GetAllNodes())
                {
                    yield return descendant;
                }
            }
        }

        internal void InvalidateNode()
        {
            TreeView?.InvalidateNode(this);
        }

        internal void UpdateCheckState()
        {
            if (TreeView?.CheckBoxes != true)
            {
                return;
            }

            if (checkState != TreeNodeState.Indeterminate)
            {
                UpdateChildrenCheckStateRecursive(checkState);
            }

            // 更新父节点状态
            parent?.UpdateParentCheckState();

            // 刷新整个树
            TreeView?.Invalidate();
        }

        /// <summary>
        /// 递归更新所有子节点的勾选状态
        /// </summary>
        internal void UpdateChildrenCheckStateRecursive(TreeNodeState state)
        {
            foreach (FluentTreeNode child in nodes)
            {
                // 设置子节点状态
                if (child.checkState != state)
                {
                    child.checkState = state;

                    TreeView?.OnNodeCheckStateChanged(child);
                    child.InvalidateNode();
                }

                // 递归更新子节点的子节点
                if (child.Nodes.Count > 0)
                {
                    child.UpdateChildrenCheckStateRecursive(state);
                }
            }
        }

        internal void UpdateParentCheckState()
        {
            if (nodes.Count == 0)
            {
                return;
            }

            int checkedCount = 0;
            int uncheckedCount = 0;
            int indeterminateCount = 0;

            // 统计子节点状态
            foreach (FluentTreeNode child in nodes)
            {
                switch (child.checkState)
                {
                    case TreeNodeState.Checked:
                        checkedCount++;
                        break;
                    case TreeNodeState.Unchecked:
                        uncheckedCount++;
                        break;
                    case TreeNodeState.Indeterminate:
                        indeterminateCount++;
                        break;
                }
            }

            // 确定新状态
            TreeNodeState newState;

            if (checkedCount == nodes.Count)
            {
                // 所有子节点都勾选
                newState = TreeNodeState.Checked;
            }
            else if (uncheckedCount == nodes.Count)
            {
                // 所有子节点都未勾选
                newState = TreeNodeState.Unchecked;
            }
            else
            {
                // 部分勾选
                newState = TreeNodeState.Indeterminate;
            }

            // 更新状态
            if (checkState != newState)
            {
                checkState = newState;
                TreeView?.OnNodeCheckStateChanged(this);
                InvalidateNode();

                parent?.UpdateParentCheckState();
            }
        }

        /// <summary>
        /// 勾选此节点及其所有子节点
        /// </summary>
        public void CheckAllChildren()
        {
            checkState = TreeNodeState.Checked;
            UpdateChildrenCheckStateRecursive(TreeNodeState.Checked);
            TreeView?.Invalidate();
        }

        /// <summary>
        /// 取消勾选此节点及其所有子节点
        /// </summary>
        public void UncheckAllChildren()
        {
            checkState = TreeNodeState.Unchecked;
            UpdateChildrenCheckStateRecursive(TreeNodeState.Unchecked);
            TreeView?.Invalidate();
        }

        /// <summary>
        /// 获取所有勾选的子节点
        /// </summary>
        public IEnumerable<FluentTreeNode> GetCheckedNodes()
        {
            return GetCheckedNodesRecursive();
        }

        private IEnumerable<FluentTreeNode> GetCheckedNodesRecursive()
        {
            if (checkState == TreeNodeState.Checked)
            {
                yield return this;
            }

            foreach (FluentTreeNode child in nodes)
            {
                foreach (var checkedNode in child.GetCheckedNodesRecursive())
                {
                    yield return checkedNode;
                }
            }
        }

        public object Clone()
        {
            var clone = new FluentTreeNode(text, icon)
            {
                name = name,
                checkState = checkState,
                isExpanded = isExpanded,
                isEnabled = isEnabled,
                tag = tag,
                foreColor = foreColor,
                backColor = backColor,
                nodeFont = nodeFont
            };

            foreach (FluentTreeNode child in nodes)
            {
                clone.Nodes.Add((FluentTreeNode)child.Clone());
            }

            return clone;
        }

        #endregion
    }

    #endregion

    #region 树节点集合

    /// <summary>
    /// 树节点集合
    /// </summary>
    [Serializable]
    [Editor(typeof(FluentTreeNodeCollectionEditor), typeof(UITypeEditor))]
    public class FluentTreeNodeCollection : List<FluentTreeNode>
    {
        private readonly FluentTreeNode owner;
        private FluentTreeView treeView;

        internal FluentTreeNodeCollection(FluentTreeNode owner, FluentTreeView treeView = null)
        {
            this.owner = owner;
            this.treeView = treeView;
        }

        internal void SetTreeView(FluentTreeView treeView)
        {
            this.treeView = treeView;
        }

        private FluentTreeView GetTreeView()
        {
            if (treeView != null)
            {
                return treeView;
            }

            return owner?.TreeView;
        }

        public new void Add(FluentTreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            node.Parent = owner;

            var tv = GetTreeView();
            if (tv != null)
            {
                node.TreeView = tv;
                // 递归设置所有子节点的 TreeView
                SetTreeViewRecursive(node, tv);
            }

            base.Add(node);

            tv?.OnNodesChanged();
        }

        /// <summary>
        /// 递归设置节点及其子节点的 TreeView 引用
        /// </summary>
        private void SetTreeViewRecursive(FluentTreeNode node, FluentTreeView treeView)
        {
            if (node == null)
            {
                return;
            }

            node.TreeView = treeView;

            if (node.Nodes != null && node.Nodes.Count > 0)
            {
                // 设置子集合的 TreeView 引用
                node.Nodes.SetTreeView(treeView);

                // 递归设置每个子节点
                foreach (var child in node.Nodes)
                {
                    SetTreeViewRecursive(child, treeView);
                }
            }
        }

        public void AddRange(FluentTreeNode[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            foreach (var node in nodes)
            {
                Add(node);
            }
        }

        public new void Remove(FluentTreeNode node)
        {
            if (node != null)
            {
                node.Parent = null;
                node.TreeView = null;
                base.Remove(node);
                GetTreeView()?.OnNodesChanged();
            }
        }

        public new void Clear()
        {
            foreach (var node in this)
            {
                node.Parent = null;
                node.TreeView = null;
            }
            base.Clear();
            GetTreeView()?.OnNodesChanged();
        }

        public new void Insert(int index, FluentTreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            node.Parent = owner;

            var tv = GetTreeView();
            if (tv != null)
            {
                node.TreeView = tv;
                SetTreeViewRecursive(node, tv);
            }

            base.Insert(index, node);
            tv?.OnNodesChanged();
        }

        public FluentTreeNode Find(string key, bool searchAllChildren)
        {
            foreach (var node in this)
            {
                if (node.Name == key)
                {
                    return node;
                }

                if (searchAllChildren)
                {
                    var found = node.Nodes.Find(key, true);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 树节点状态
    /// </summary>
    public enum TreeNodeState
    {
        Unchecked,
        Checked,
        Indeterminate
    }

    public class TreeViewEventArgs : EventArgs
    {
        public FluentTreeNode Node { get; }

        public TreeViewEventArgs(FluentTreeNode node)
        {
            Node = node;
        }
    }

    public class TreeNodeMouseClickEventArgs : MouseEventArgs
    {
        public FluentTreeNode Node { get; }

        public TreeNodeMouseClickEventArgs(FluentTreeNode node, MouseButtons button, int clicks, int x, int y)
            : base(button, clicks, x, y, 0)
        {
            Node = node;
        }
    }

    public class TreeNodeDragEventArgs : EventArgs
    {
        public FluentTreeNode SourceNode { get; }
        public FluentTreeNode TargetNode { get; }
        public bool Cancel { get; set; }

        public TreeNodeDragEventArgs(FluentTreeNode source, FluentTreeNode target)
        {
            SourceNode = source;
            TargetNode = target;
        }
    }

    #endregion

    #region 设计时支持

    /// <summary>
    /// FluentTreeView设计器
    /// </summary>
    public class FluentTreeViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentTreeViewActionList(Component));
                }
                return actionLists;
            }
        }
    }

    /// <summary>
    /// 智能标记
    /// </summary>
    public class FluentTreeViewActionList : DesignerActionList
    {
        private FluentTreeView treeView;
        private IDesignerHost designerHost;
        private DesignerActionUIService designerService;


        public FluentTreeViewActionList(IComponent component) : base(component)
        {
            treeView = component as FluentTreeView;
            designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public bool CheckBoxes
        {
            get => treeView.CheckBoxes;
            set => SetProperty("CheckBoxes", value);
        }

        public bool ShowSearchBox
        {
            get => treeView.ShowSearchBox;
            set => SetProperty("ShowSearchBox", value);
        }

        public bool ShowLines
        {
            get => treeView.ShowLines;
            set => SetProperty("ShowLines", value);
        }

        public bool ShowPlusMinus
        {
            get => treeView.ShowPlusMinus;
            set => SetProperty("ShowPlusMinus", value);
        }

        public bool FullRowSelect
        {
            get => treeView.FullRowSelect;
            set => SetProperty("FullRowSelect", value);
        }

        public int NodeHeight
        {
            get => treeView.NodeHeight;
            set => SetProperty("NodeHeight", value);
        }

        public int NodeIndent
        {
            get => treeView.NodeIndent;
            set => SetProperty("NodeIndent", value);
        }

        public DockStyle Dock
        {
            get => treeView.Dock;
            set => SetProperty("Dock", value);
        }

        public void EditNodes()
        {
            if (designerHost != null)
            {
                // 创建设计器事务
                DesignerTransaction transaction = designerHost.CreateTransaction("编辑节点集合");
                try
                {
                    var propertyDescriptor = TypeDescriptor.GetProperties(treeView)["Nodes"];
                    if (propertyDescriptor != null)
                    {
                        // 使用您已有的TypeDescriptorContext
                        var context = new TypeDescriptorContext(treeView, propertyDescriptor, designerHost);

                        // 获取集合编辑器
                        var editor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                        if (editor == null)
                        {
                            // 如果没有找到编辑器，创建一个新的
                            editor = new FluentTreeNodeCollectionEditor(typeof(FluentTreeNodeCollection));
                        }

                        // 编辑节点集合
                        var oldValue = treeView.Nodes;
                        var newValue = editor.EditValue(context, context, oldValue);

                        context.OnComponentChanging();
                        propertyDescriptor.SetValue(treeView, newValue);
                        context.OnComponentChanged();

                        treeView.UpdateLayout();
                        treeView.Invalidate();
                    }

                    transaction.Commit();

                    if (designerService != null)
                    {
                        designerService.Refresh(treeView);
                    }

                }
                catch
                {
                    transaction.Cancel();
                    throw;
                }

            }

        }

        public void AddSampleNodes()
        {
            // 清空现有节点
            treeView.Nodes.Clear();

            // 添加示例节点
            var root = new FluentTreeNode("根节点");
            treeView.Nodes.Add(root);

            // 然后添加子节点
            root.Nodes.Add(new FluentTreeNode("子节点 1"));
            root.Nodes.Add(new FluentTreeNode("子节点 2"));

            var child3 = new FluentTreeNode("子节点 3");
            root.Nodes.Add(child3);

            // 添加孙节点
            child3.Nodes.Add(new FluentTreeNode("子节点 3.1"));
            child3.Nodes.Add(new FluentTreeNode("子节点 3.2"));

            // 通知设计器
            var changeService = designerHost?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(treeView)["Nodes"];
                changeService.OnComponentChanged(treeView, propertyDescriptor, null, treeView.Nodes);
            }

            treeView.UpdateLayout();
            treeView.Invalidate();
        }

        public void ClearNodes()
        {
            var changeService = designerHost?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            var propertyDescriptor = TypeDescriptor.GetProperties(treeView)["Nodes"];

            if (changeService != null && propertyDescriptor != null)
            {
                changeService.OnComponentChanging(treeView, propertyDescriptor);
            }

            treeView.Nodes.Clear();

            if (changeService != null && propertyDescriptor != null)
            {
                changeService.OnComponentChanged(treeView, propertyDescriptor, null, treeView.Nodes);
            }

            treeView.UpdateLayout();
            treeView.Invalidate();
        }

        public void ExpandAll()
        {
            foreach (FluentTreeNode node in treeView.Nodes)
            {
                node.ExpandAll();
            }
            treeView.Invalidate();
        }

        public void CollapseAll()
        {
            foreach (FluentTreeNode node in treeView.Nodes)
            {
                node.CollapseAll();
            }
            treeView.Invalidate();
        }

        private PropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return TypeDescriptor.GetProperties(treeView)[propertyName];
        }

        private void SetProperty(string propertyName, object value)
        {
            var propertyDescriptor = GetPropertyDescriptor(propertyName);
            if (propertyDescriptor != null)
            {
                // 使用您的TypeDescriptorContext
                var context = new TypeDescriptorContext(treeView, propertyDescriptor, designerHost);

                context.OnComponentChanging();
                propertyDescriptor.SetValue(treeView, value);
                context.OnComponentChanged();
            }
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowLines", "显示连接线", "外观", "是否显示节点之间的连接线"));
            items.Add(new DesignerActionPropertyItem("ShowPlusMinus", "显示展开按钮", "外观", "是否显示展开/折叠按钮"));
            items.Add(new DesignerActionPropertyItem("CheckBoxes", "显示复选框", "外观", "是否在节点前显示复选框"));
            items.Add(new DesignerActionPropertyItem("FullRowSelect", "整行选择", "外观", "是否启用整行选择"));
            items.Add(new DesignerActionPropertyItem("ShowSearchBox", "显示搜索框", "外观", "是否显示搜索框"));

            // 布局
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Dock", "停靠", "布局", "控件的停靠方式"));
            items.Add(new DesignerActionPropertyItem("NodeHeight", "节点高度", "布局", "每个节点的高度"));
            items.Add(new DesignerActionPropertyItem("NodeIndent", "节点缩进", "布局", "子节点的缩进距离"));

            // 数据
            items.Add(new DesignerActionHeaderItem("数据"));
            items.Add(new DesignerActionMethodItem(this, "EditNodes", "编辑节点...", "数据", "打开节点编辑器", true));
            items.Add(new DesignerActionMethodItem(this, "AddSampleNodes", "添加示例节点", "数据", "添加一些示例节点用于测试", false));
            items.Add(new DesignerActionMethodItem(this, "ClearNodes", "清空所有节点", "数据", "删除所有节点", false));

            // 操作
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "ExpandAll", "展开所有", "操作", "展开所有节点", false));
            items.Add(new DesignerActionMethodItem(this, "CollapseAll", "折叠所有", "操作", "折叠所有节点", false));

            return items;
        }
    }

    /// <summary>
    /// 树节点集合编辑器
    /// </summary>
    public class FluentTreeNodeCollectionEditor : CollectionEditor
    {
        public FluentTreeNodeCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentTreeNode);
        }

        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(FluentTreeNode))
            {
                return new FluentTreeNode("新节点");
            }
            return base.CreateInstance(itemType);
        }

        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            form.Text = "树节点集合编辑器";
            form.Width = 600;
            form.Height = 400;
            return form;
        }
    }

    /// <summary>
    /// 节点类型转换器
    /// </summary>
    public class FluentTreeNodeConverter : ExpandableObjectConverter
    {
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
            if (destinationType == typeof(string) && value is FluentTreeNode node)
            {
                return $"{node.Text} [{node.Nodes.Count} 子节点]";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion
}
