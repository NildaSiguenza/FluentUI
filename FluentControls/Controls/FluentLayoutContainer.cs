using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Xml.Linq;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent布局容器
    /// </summary>
    [Designer(typeof(FluentLayoutContainerDesigner))]
    [ToolboxItem(true)]
    [DefaultProperty("LayoutMode")]
    [DefaultEvent("LayoutModeChanged")]
    public class FluentLayoutContainer : FluentContainerBase
    {
        private FluentLayoutMode layoutMode = FluentLayoutMode.Vertical;
        private readonly FluentGridCell[] gridCells = new FluentGridCell[4];

        private float splitterSize = 2;
        private Color splitterColor = Color.FromArgb(200, 200, 200);
        private Color splitterHoverColor = Color.FromArgb(100, 150, 200);

        private float horizontalSplitterDistance = 0.25f;
        private float verticalSplitterDistance = 0.25f;
        private float secondHorizontalSplitterDistance = 0.75f;
        private float secondVerticalSplitterDistance = 0.75f;

        private bool isDragging = false;
        private int activeSplitter = -1;
        private int hoverSplitter = -1;
        private bool showSplitter = true;
        private bool allowSplitterDrag = true;

        // 格子可见性控制
        private bool showTopPanel = true;
        private bool showBottomPanel = true;
        private bool showLeftPanel = true;
        private bool showRightPanel = true;

        // 边框
        private bool showContainerBorder = true;
        private int containerBorderSize = 1;
        private Color containerBorderColor = Color.DarkGray;

        // 栅格系统
        private const int GRID_COLUMNS = 24;
        private const int GRID_ROWS = 24;


        #region 构造函数

        public FluentLayoutContainer()
        {
            // 初始化格子容器
            for (int i = 0; i < 4; i++)
            {
                gridCells[i] = new FluentGridCell(i + 1);
                gridCells[i].Name = $"panel{i + 1}";
                gridCells[i].Tag = $"GridCell{i + 1}";
                gridCells[i].Visible = false;
            }

            Controls.AddRange(gridCells);

            // 默认垂直布局
            UpdateLayoutPositions();
            Dock = DockStyle.Fill;
            MinimumSize = new Size(200, 160);
            BackColor = Color.Transparent;
        }

        #endregion

        #region 公共属性

        [Category("Layout")]
        [Description("布局模式")]
        [DefaultValue(FluentLayoutMode.Vertical)]
        [RefreshProperties(RefreshProperties.All)]
        public FluentLayoutMode LayoutMode
        {
            get => layoutMode;
            set
            {
                if (layoutMode != value)
                {
                    layoutMode = value;
                    UpdateLayoutPositions();
                    PerformLayout();
                    RefreshControl();
                    OnLayoutModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 获取第一个格子
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentGridCell Panel1 => gridCells[0];

        /// <summary>
        /// 获取第二个格子
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentGridCell Panel2 => gridCells[1];

        /// <summary>
        /// 获取第三个格子
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentGridCell Panel3 => gridCells[2];

        /// <summary>
        /// 获取第四个格子
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentGridCell Panel4 => gridCells[3];

        [Category("Layout")]
        [Description("水平分隔条位置(0-1)")]
        [DefaultValue(0.25f)]
        public float HorizontalSplitterDistance
        {
            get => horizontalSplitterDistance;
            set
            {
                value = Math.Max(0.05f, Math.Min(0.95f, value));
                if (Math.Abs(horizontalSplitterDistance - value) > 0.001f)
                {
                    horizontalSplitterDistance = value;
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("垂直分隔条位置(0-1)")]
        [DefaultValue(0.25f)]
        public float VerticalSplitterDistance
        {
            get => verticalSplitterDistance;
            set
            {
                value = Math.Max(0.05f, Math.Min(0.95f, value));
                if (Math.Abs(verticalSplitterDistance - value) > 0.001f)
                {
                    verticalSplitterDistance = value;
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("第二个水平分隔条位置(0-1)")]
        [DefaultValue(0.75f)]
        public float SecondHorizontalSplitterDistance
        {
            get => secondHorizontalSplitterDistance;
            set
            {
                value = Math.Max(horizontalSplitterDistance + 0.05f, Math.Min(0.95f, value));
                if (Math.Abs(secondHorizontalSplitterDistance - value) > 0.001f)
                {
                    secondHorizontalSplitterDistance = value;
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("第二个垂直分隔条位置(0-1)")]
        [DefaultValue(0.75f)]
        public float SecondVerticalSplitterDistance
        {
            get => secondVerticalSplitterDistance;
            set
            {
                value = Math.Max(verticalSplitterDistance + 0.05f, Math.Min(0.95f, value));
                if (Math.Abs(secondVerticalSplitterDistance - value) > 0.001f)
                {
                    secondVerticalSplitterDistance = value;
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("显示顶部面板")]
        [DefaultValue(true)]
        public bool ShowTopPanel
        {
            get => showTopPanel;
            set
            {
                if (showTopPanel != value)
                {
                    showTopPanel = value;
                    UpdateLayoutPositions();
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("显示底部面板")]
        [DefaultValue(true)]
        public bool ShowBottomPanel
        {
            get => showBottomPanel;
            set
            {
                if (showBottomPanel != value)
                {
                    showBottomPanel = value;
                    UpdateLayoutPositions();
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("显示左侧面板")]
        [DefaultValue(true)]
        public bool ShowLeftPanel
        {
            get => showLeftPanel;
            set
            {
                if (showLeftPanel != value)
                {
                    showLeftPanel = value;
                    UpdateLayoutPositions();
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("显示右侧面板")]
        [DefaultValue(true)]
        public bool ShowRightPanel
        {
            get => showRightPanel;
            set
            {
                if (showRightPanel != value)
                {
                    showRightPanel = value;
                    UpdateLayoutPositions();
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("显示容器边框")]
        [DefaultValue(true)]
        public bool ShowContainerBorder
        {
            get => showContainerBorder;
            set
            {
                if (showContainerBorder != value)
                {
                    showContainerBorder = value;
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("容器边框大小")]
        [DefaultValue(1)]
        public int ContainerBorderSize
        {
            get => containerBorderSize;
            set
            {
                if (containerBorderSize != value && value >= 0)
                {
                    containerBorderSize = value;
                    RefreshControl();
                }
            }
        }

        [Category("Layout")]
        [Description("容器边框颜色")]
        public Color ContainerBorderColor
        {
            get => containerBorderColor;
            set
            {
                if (containerBorderColor != value)
                {
                    containerBorderColor = value;
                    RefreshControl();
                }
            }
        }

        [Category("Splitter")]
        [Description("分隔条大小")]
        [DefaultValue(2f)]
        public float SplitterSize
        {
            get => splitterSize;
            set
            {
                if (splitterSize != value && value >= 1)
                {
                    splitterSize = value;
                    PerformLayout();
                    RefreshControl();
                }
            }
        }

        [Category("Splitter")]
        [Description("分隔条颜色")]
        public Color SplitterColor
        {
            get => splitterColor;
            set
            {
                if (splitterColor != value)
                {
                    splitterColor = value;
                    RefreshControl();
                }
            }
        }

        [Category("Splitter")]
        [Description("分隔条悬停颜色")]
        public Color SplitterHoverColor
        {
            get => splitterHoverColor;
            set
            {
                if (splitterHoverColor != value)
                {
                    splitterHoverColor = value;
                    RefreshControl();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示分隔条
        /// </summary>
        [Category("Splitter")]
        [Description("是否显示分隔条")]
        [DefaultValue(true)]
        public bool ShowSplitter
        {
            get => showSplitter;
            set
            {
                if (showSplitter != value)
                {
                    showSplitter = value;

                    // 如果不显示分隔条, 自动禁用拖动
                    if (!value && allowSplitterDrag)
                    {
                        allowSplitterDrag = false;
                    }

                    RefreshControl();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否允许在运行时拖动分隔条
        /// </summary>
        [Category("Splitter")]
        [Description("是否允许在运行时拖动分隔条")]
        [DefaultValue(true)]
        public bool AllowSplitterDrag
        {
            get => allowSplitterDrag;
            set
            {
                if (allowSplitterDrag != value)
                {
                    allowSplitterDrag = value;

                    // 如果启用拖动, 必须显示分隔条
                    if (value && !showSplitter)
                    {
                        showSplitter = true;
                    }

                    // 重置拖动状态
                    if (!value)
                    {
                        isDragging = false;
                        activeSplitter = -1;
                        hoverSplitter = -1;
                    }

                    RefreshControl();
                }
            }
        }

        [Browsable(false)]
        internal FluentGridCell[] GridCells => gridCells;

        #endregion

        #region 事件

        public event EventHandler LayoutModeChanged;

        protected virtual void OnLayoutModeChanged(EventArgs e)
        {
            LayoutModeChanged?.Invoke(this, e);
        }

        #endregion

        #region 布局逻辑

        private void UpdateLayoutPositions()
        {
            switch (layoutMode)
            {
                case FluentLayoutMode.Vertical:
                    UpdateVerticalLayout();
                    break;

                case FluentLayoutMode.Horizontal:
                    UpdateHorizontalLayout();
                    break;

                case FluentLayoutMode.Grid:
                    gridCells[0].Position = FluentLayoutPosition.TopLeft;
                    gridCells[1].Position = FluentLayoutPosition.TopRight;
                    gridCells[2].Position = FluentLayoutPosition.BottomLeft;
                    gridCells[3].Position = FluentLayoutPosition.BottomRight;
                    if (Math.Abs(horizontalSplitterDistance - 0.25f) < 0.01f &&
                        Math.Abs(verticalSplitterDistance - 0.25f) < 0.01f)
                    {
                        horizontalSplitterDistance = 0.5f;
                        verticalSplitterDistance = 0.5f;
                    }
                    break;

                case FluentLayoutMode.TopSpan:
                    gridCells[0].Position = FluentLayoutPosition.Top;
                    gridCells[1].Position = FluentLayoutPosition.BottomLeft;
                    gridCells[2].Position = FluentLayoutPosition.BottomRight;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    break;

                case FluentLayoutMode.BottomSpan:
                    gridCells[0].Position = FluentLayoutPosition.TopLeft;
                    gridCells[1].Position = FluentLayoutPosition.TopRight;
                    gridCells[2].Position = FluentLayoutPosition.Bottom;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    break;

                case FluentLayoutMode.LeftSpan:
                    gridCells[0].Position = FluentLayoutPosition.Left;
                    gridCells[1].Position = FluentLayoutPosition.TopRight;
                    gridCells[2].Position = FluentLayoutPosition.BottomRight;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    break;

                case FluentLayoutMode.RightSpan:
                    gridCells[0].Position = FluentLayoutPosition.TopLeft;
                    gridCells[1].Position = FluentLayoutPosition.BottomLeft;
                    gridCells[2].Position = FluentLayoutPosition.Right;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    break;

                case FluentLayoutMode.ThreeColumns:
                    gridCells[0].Position = FluentLayoutPosition.Left;
                    gridCells[1].Position = FluentLayoutPosition.Center;
                    gridCells[2].Position = FluentLayoutPosition.Right;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    horizontalSplitterDistance = 0.33f;
                    secondHorizontalSplitterDistance = 0.67f;
                    break;

                case FluentLayoutMode.ThreeRows:
                    gridCells[0].Position = FluentLayoutPosition.Top;
                    gridCells[1].Position = FluentLayoutPosition.Center;
                    gridCells[2].Position = FluentLayoutPosition.Bottom;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    verticalSplitterDistance = 0.33f;
                    secondVerticalSplitterDistance = 0.67f;
                    break;

                case FluentLayoutMode.Sidebar:
                    gridCells[0].Position = FluentLayoutPosition.Sidebar;
                    gridCells[1].Position = FluentLayoutPosition.Content;
                    gridCells[2].Position = FluentLayoutPosition.None;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    horizontalSplitterDistance = 0.25f;
                    break;

                case FluentLayoutMode.ReverseSidebar:
                    gridCells[0].Position = FluentLayoutPosition.Content;
                    gridCells[1].Position = FluentLayoutPosition.Sidebar;
                    gridCells[2].Position = FluentLayoutPosition.None;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    horizontalSplitterDistance = 0.75f;
                    break;

                case FluentLayoutMode.HeaderFooter:
                    gridCells[0].Position = FluentLayoutPosition.Header;
                    gridCells[1].Position = FluentLayoutPosition.Content;
                    gridCells[2].Position = FluentLayoutPosition.Footer;
                    gridCells[3].Position = FluentLayoutPosition.None;
                    verticalSplitterDistance = 0.15f;
                    secondVerticalSplitterDistance = 0.85f;
                    break;

                case FluentLayoutMode.GridCustom:
                    // 使用栅格定义
                    for (int i = 0; i < gridCells.Length; i++)
                    {
                        gridCells[i].Position = FluentLayoutPosition.GridCustom;
                    }
                    break;
            }

            // 更新格子可见性
            for (int i = 0; i < gridCells.Length; i++)
            {
                gridCells[i].Visible = gridCells[i].Position != FluentLayoutPosition.None;
            }
        }

        private void UpdateVerticalLayout()
        {
            if (showTopPanel && showBottomPanel)
            {
                gridCells[0].Position = FluentLayoutPosition.Top;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.Bottom;
            }
            else if (showTopPanel)
            {
                gridCells[0].Position = FluentLayoutPosition.Top;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.None;
            }
            else if (showBottomPanel)
            {
                gridCells[0].Position = FluentLayoutPosition.None;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.Bottom;
            }
            else
            {
                gridCells[0].Position = FluentLayoutPosition.None;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.None;
            }
            gridCells[3].Position = FluentLayoutPosition.None;
        }

        private void UpdateHorizontalLayout()
        {
            if (showLeftPanel && showRightPanel)
            {
                gridCells[0].Position = FluentLayoutPosition.Left;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.Right;
            }
            else if (showLeftPanel)
            {
                gridCells[0].Position = FluentLayoutPosition.Left;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.None;
            }
            else if (showRightPanel)
            {
                gridCells[0].Position = FluentLayoutPosition.None;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.Right;
            }
            else
            {
                gridCells[0].Position = FluentLayoutPosition.None;
                gridCells[1].Position = FluentLayoutPosition.Center;
                gridCells[2].Position = FluentLayoutPosition.None;
            }
            gridCells[3].Position = FluentLayoutPosition.None;
        }

        private bool ShouldSerializeAllowSplitterDrag()
        {
            return showSplitter;
        }

        private void RefreshControl()
        {
            Invalidate();
            Update();

            // 通知设计器刷新
            if (Site != null && Site.DesignMode)
            {
                var service = GetService(typeof(System.ComponentModel.Design.IComponentChangeService))
                    as System.ComponentModel.Design.IComponentChangeService;

                service?.OnComponentChanged(this, null, null, null);
            }
        }

        #endregion

        #region 布局计算

        private Dictionary<FluentLayoutPosition, Rectangle> CalculateLayoutRectangles()
        {
            var result = new Dictionary<FluentLayoutPosition, Rectangle>();
            int splitterSize = (int)this.splitterSize;

            int borderOffset = showContainerBorder ? containerBorderSize : 0;
            int availableWidth = Math.Max(0, Width - 2 * borderOffset);
            int availableHeight = Math.Max(0, Height - 2 * borderOffset);

            int hSplit = borderOffset + (int)(availableWidth * horizontalSplitterDistance);
            int vSplit = borderOffset + (int)(availableHeight * verticalSplitterDistance);
            int hSplit2 = borderOffset + (int)(availableWidth * secondHorizontalSplitterDistance);
            int vSplit2 = borderOffset + (int)(availableHeight * secondVerticalSplitterDistance);

            switch (layoutMode)
            {
                case FluentLayoutMode.Vertical:
                    CalculateVerticalLayout(result, borderOffset, availableWidth, availableHeight,
                        vSplit, vSplit2, splitterSize);
                    break;

                case FluentLayoutMode.Horizontal:
                    CalculateHorizontalLayout(result, borderOffset, availableWidth, availableHeight,
                        hSplit, hSplit2, splitterSize);
                    break;

                case FluentLayoutMode.Grid:
                    CalculateGridLayout(result, borderOffset, availableWidth, availableHeight,
                        hSplit, vSplit, splitterSize);
                    break;

                case FluentLayoutMode.TopSpan:
                    result[FluentLayoutPosition.Top] = new Rectangle(
                        borderOffset, borderOffset, availableWidth, vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.BottomLeft] = new Rectangle(
                        borderOffset, vSplit + splitterSize / 2, hSplit - borderOffset - splitterSize / 2,
                        availableHeight - vSplit - splitterSize / 2 + borderOffset);
                    result[FluentLayoutPosition.BottomRight] = new Rectangle(
                        hSplit + splitterSize / 2, vSplit + splitterSize / 2,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset,
                        availableHeight - vSplit - splitterSize / 2 + borderOffset);
                    break;

                case FluentLayoutMode.BottomSpan:
                    result[FluentLayoutPosition.TopLeft] = new Rectangle(
                        borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2,
                        vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.TopRight] = new Rectangle(
                        hSplit + splitterSize / 2, borderOffset,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset,
                        vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.Bottom] = new Rectangle(
                        borderOffset, vSplit + splitterSize / 2, availableWidth,
                        availableHeight - vSplit - splitterSize / 2 + borderOffset);
                    break;

                case FluentLayoutMode.LeftSpan:
                    result[FluentLayoutPosition.Left] = new Rectangle(
                        borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2, availableHeight);
                    result[FluentLayoutPosition.TopRight] = new Rectangle(
                        hSplit + splitterSize / 2, borderOffset,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset,
                        vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.BottomRight] = new Rectangle(
                        hSplit + splitterSize / 2, vSplit + splitterSize / 2,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset,
                        availableHeight - vSplit - splitterSize / 2 + borderOffset);
                    break;

                case FluentLayoutMode.RightSpan:
                    result[FluentLayoutPosition.TopLeft] = new Rectangle(
                        borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2,
                        vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.BottomLeft] = new Rectangle(
                        borderOffset, vSplit + splitterSize / 2, hSplit - borderOffset - splitterSize / 2,
                        availableHeight - vSplit - splitterSize / 2 + borderOffset);
                    result[FluentLayoutPosition.Right] = new Rectangle(
                        hSplit + splitterSize / 2, borderOffset,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset, availableHeight);
                    break;

                case FluentLayoutMode.ThreeColumns:
                    result[FluentLayoutPosition.Left] = new Rectangle(
                        borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2, availableHeight);
                    result[FluentLayoutPosition.Center] = new Rectangle(
                        hSplit + splitterSize / 2, borderOffset, hSplit2 - hSplit - splitterSize, availableHeight);
                    result[FluentLayoutPosition.Right] = new Rectangle(
                        hSplit2 + splitterSize / 2, borderOffset,
                        availableWidth - hSplit2 - splitterSize / 2 + borderOffset, availableHeight);
                    break;

                case FluentLayoutMode.ThreeRows:
                    result[FluentLayoutPosition.Top] = new Rectangle(
                        borderOffset, borderOffset, availableWidth, vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.Center] = new Rectangle(
                        borderOffset, vSplit + splitterSize / 2, availableWidth, vSplit2 - vSplit - splitterSize);
                    result[FluentLayoutPosition.Bottom] = new Rectangle(
                        borderOffset, vSplit2 + splitterSize / 2, availableWidth,
                        availableHeight - vSplit2 - splitterSize / 2 + borderOffset);
                    break;

                case FluentLayoutMode.Sidebar:
                    result[FluentLayoutPosition.Sidebar] = new Rectangle(
                        borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2, availableHeight);
                    result[FluentLayoutPosition.Content] = new Rectangle(
                        hSplit + splitterSize / 2, borderOffset,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset, availableHeight);
                    break;

                case FluentLayoutMode.ReverseSidebar:
                    result[FluentLayoutPosition.Content] = new Rectangle(
                        borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2, availableHeight);
                    result[FluentLayoutPosition.Sidebar] = new Rectangle(
                        hSplit + splitterSize / 2, borderOffset,
                        availableWidth - hSplit - splitterSize / 2 + borderOffset, availableHeight);
                    break;

                case FluentLayoutMode.HeaderFooter:
                    result[FluentLayoutPosition.Header] = new Rectangle(
                        borderOffset, borderOffset, availableWidth, vSplit - borderOffset - splitterSize / 2);
                    result[FluentLayoutPosition.Content] = new Rectangle(
                        borderOffset, vSplit + splitterSize / 2, availableWidth, vSplit2 - vSplit - splitterSize);
                    result[FluentLayoutPosition.Footer] = new Rectangle(
                        borderOffset, vSplit2 + splitterSize / 2, availableWidth,
                        availableHeight - vSplit2 - splitterSize / 2 + borderOffset);
                    break;

                case FluentLayoutMode.GridCustom:
                    CalculateGridCustomLayout(result, borderOffset, availableWidth, availableHeight);
                    break;
            }

            return result;
        }

        private void CalculateVerticalLayout(Dictionary<FluentLayoutPosition, Rectangle> result,
            int borderOffset, int availableWidth, int availableHeight, int vSplit, int vSplit2, int splitterSize)
        {
            bool hasTop = showTopPanel;
            bool hasBottom = showBottomPanel;

            if (hasTop && hasBottom)
            {
                result[FluentLayoutPosition.Top] = new Rectangle(
                    borderOffset, borderOffset, availableWidth, vSplit - borderOffset - splitterSize / 2);
                result[FluentLayoutPosition.Center] = new Rectangle(
                    borderOffset, vSplit + splitterSize / 2, availableWidth, vSplit2 - vSplit - splitterSize);
                result[FluentLayoutPosition.Bottom] = new Rectangle(
                    borderOffset, vSplit2 + splitterSize / 2, availableWidth,
                    availableHeight - vSplit2 - splitterSize / 2 + borderOffset);
            }
            else if (hasTop)
            {
                result[FluentLayoutPosition.Top] = new Rectangle(
                    borderOffset, borderOffset, availableWidth, vSplit - borderOffset - splitterSize / 2);
                result[FluentLayoutPosition.Center] = new Rectangle(
                    borderOffset, vSplit + splitterSize / 2, availableWidth,
                    availableHeight - vSplit - splitterSize / 2 + borderOffset);
            }
            else if (hasBottom)
            {
                result[FluentLayoutPosition.Center] = new Rectangle(
                    borderOffset, borderOffset, availableWidth, vSplit2 - splitterSize / 2);
                result[FluentLayoutPosition.Bottom] = new Rectangle(
                    borderOffset, vSplit2 + splitterSize / 2, availableWidth,
                    availableHeight - vSplit2 - splitterSize / 2 + borderOffset);
            }
            else
            {
                result[FluentLayoutPosition.Center] = new Rectangle(
                    borderOffset, borderOffset, availableWidth, availableHeight);
            }
        }

        private void CalculateHorizontalLayout(Dictionary<FluentLayoutPosition, Rectangle> result,
            int borderOffset, int availableWidth, int availableHeight, int hSplit, int hSplit2, int splitterSize)
        {
            bool hasLeft = showLeftPanel;
            bool hasRight = showRightPanel;

            if (hasLeft && hasRight)
            {
                result[FluentLayoutPosition.Left] = new Rectangle(
                    borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2, availableHeight);
                result[FluentLayoutPosition.Center] = new Rectangle(
                    hSplit + splitterSize / 2, borderOffset, hSplit2 - hSplit - splitterSize, availableHeight);
                result[FluentLayoutPosition.Right] = new Rectangle(
                    hSplit2 + splitterSize / 2, borderOffset,
                    availableWidth - hSplit2 - splitterSize / 2 + borderOffset, availableHeight);
            }
            else if (hasLeft)
            {
                result[FluentLayoutPosition.Left] = new Rectangle(
                    borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2, availableHeight);
                result[FluentLayoutPosition.Center] = new Rectangle(
                    hSplit + splitterSize / 2, borderOffset,
                    availableWidth - hSplit - splitterSize / 2 + borderOffset, availableHeight);
            }
            else if (hasRight)
            {
                result[FluentLayoutPosition.Center] = new Rectangle(
                    borderOffset, borderOffset, hSplit2 - splitterSize / 2, availableHeight);
                result[FluentLayoutPosition.Right] = new Rectangle(
                    hSplit2 + splitterSize / 2, borderOffset,
                    availableWidth - hSplit2 - splitterSize / 2 + borderOffset, availableHeight);
            }
            else
            {
                result[FluentLayoutPosition.Center] = new Rectangle(
                    borderOffset, borderOffset, availableWidth, availableHeight);
            }
        }

        private void CalculateGridLayout(Dictionary<FluentLayoutPosition, Rectangle> result,
            int borderOffset, int availableWidth, int availableHeight, int hSplit, int vSplit, int splitterSize)
        {
            result[FluentLayoutPosition.TopLeft] = new Rectangle(
                borderOffset, borderOffset, hSplit - borderOffset - splitterSize / 2,
                vSplit - borderOffset - splitterSize / 2);
            result[FluentLayoutPosition.TopRight] = new Rectangle(
                hSplit + splitterSize / 2, borderOffset,
                availableWidth - hSplit - splitterSize / 2 + borderOffset,
                vSplit - borderOffset - splitterSize / 2);
            result[FluentLayoutPosition.BottomLeft] = new Rectangle(
                borderOffset, vSplit + splitterSize / 2, hSplit - borderOffset - splitterSize / 2,
                availableHeight - vSplit - splitterSize / 2 + borderOffset);
            result[FluentLayoutPosition.BottomRight] = new Rectangle(
                hSplit + splitterSize / 2, vSplit + splitterSize / 2,
                availableWidth - hSplit - splitterSize / 2 + borderOffset,
                availableHeight - vSplit - splitterSize / 2 + borderOffset);
        }

        private void CalculateGridCustomLayout(Dictionary<FluentLayoutPosition, Rectangle> result,
            int borderOffset, int availableWidth, int availableHeight)
        {
            float columnWidth = (float)availableWidth / GRID_COLUMNS;
            float rowHeight = (float)availableHeight / GRID_ROWS;

            foreach (var cell in gridCells.Where(c => c.Position == FluentLayoutPosition.GridCustom))
            {
                var def = cell.GridDefinition;
                if (def.IsValid())
                {
                    int x = borderOffset + (int)(def.Column * columnWidth);
                    int y = borderOffset + (int)(def.Row * rowHeight);
                    int width = (int)(def.ColumnSpan * columnWidth);
                    int height = (int)(def.RowSpan * rowHeight);

                    // 为每个格子创建一个唯一的键
                    var key = (FluentLayoutPosition)((int)FluentLayoutPosition.GridCustom + cell.GridIndex);
                    result[key] = new Rectangle(x, y, width, height);
                }
            }
        }

        #endregion

        #region 分隔条

        /// <summary>
        /// 获取分隔条的矩形区域
        /// </summary>
        private List<Rectangle> GetSplitterRectangles()
        {
            var result = new List<Rectangle>();
            int splitterSize = (int)this.splitterSize;

            int borderOffset = showContainerBorder ? containerBorderSize : 0;
            int availableWidth = Math.Max(0, Width - 2 * borderOffset);
            int availableHeight = Math.Max(0, Height - 2 * borderOffset);

            int hSplit = borderOffset + (int)(availableWidth * horizontalSplitterDistance);
            int vSplit = borderOffset + (int)(availableHeight * verticalSplitterDistance);
            int hSplit2 = borderOffset + (int)(availableWidth * secondHorizontalSplitterDistance);
            int vSplit2 = borderOffset + (int)(availableHeight * secondVerticalSplitterDistance);

            hSplit = Math.Max(borderOffset + 10, Math.Min(Width - borderOffset - 10, hSplit));
            vSplit = Math.Max(borderOffset + 10, Math.Min(Height - borderOffset - 10, vSplit));
            hSplit2 = Math.Max(hSplit + 20, Math.Min(Width - borderOffset - 10, hSplit2));
            vSplit2 = Math.Max(vSplit + 20, Math.Min(Height - borderOffset - 10, vSplit2));

            switch (layoutMode)
            {
                case FluentLayoutMode.Vertical:
                    {
                        bool hasTop = showTopPanel;
                        bool hasBottom = showBottomPanel;

                        if (hasTop)
                        {
                            result.Add(new Rectangle(borderOffset, vSplit - splitterSize / 2,
                                availableWidth, splitterSize));
                        }

                        if (hasBottom)
                        {
                            result.Add(new Rectangle(borderOffset, vSplit2 - splitterSize / 2,
                                availableWidth, splitterSize));
                        }
                    }
                    break;

                case FluentLayoutMode.Horizontal:
                    {
                        bool hasLeft = showLeftPanel;
                        bool hasRight = showRightPanel;

                        if (hasLeft)
                        {
                            result.Add(new Rectangle(hSplit - splitterSize / 2, borderOffset,
                                splitterSize, availableHeight));
                        }

                        if (hasRight)
                        {
                            result.Add(new Rectangle(hSplit2 - splitterSize / 2, borderOffset,
                                splitterSize, availableHeight));
                        }
                    }
                    break;

                case FluentLayoutMode.Grid:
                case FluentLayoutMode.TopSpan:
                case FluentLayoutMode.BottomSpan:
                case FluentLayoutMode.LeftSpan:
                case FluentLayoutMode.RightSpan:
                    // 横向分隔条（水平线, 控制Y方向）
                    result.Add(new Rectangle(0, vSplit - splitterSize / 2, Width, splitterSize));
                    // 竖向分隔条（垂直线, 控制X方向）
                    result.Add(new Rectangle(hSplit - splitterSize / 2, 0, splitterSize, Height));
                    break;

                case FluentLayoutMode.ThreeColumns:
                    result.Add(new Rectangle(hSplit - splitterSize / 2, borderOffset,
                        splitterSize, availableHeight));
                    result.Add(new Rectangle(hSplit2 - splitterSize / 2, borderOffset,
                        splitterSize, availableHeight));
                    break;

                case FluentLayoutMode.ThreeRows:
                case FluentLayoutMode.HeaderFooter:
                    result.Add(new Rectangle(borderOffset, vSplit - splitterSize / 2,
                        availableWidth, splitterSize));
                    result.Add(new Rectangle(borderOffset, vSplit2 - splitterSize / 2,
                        availableWidth, splitterSize));
                    break;

                case FluentLayoutMode.Sidebar:
                case FluentLayoutMode.ReverseSidebar:
                    result.Add(new Rectangle(hSplit - splitterSize / 2, borderOffset,
                        splitterSize, availableHeight));
                    break;
            }

            return result;
        }

        private int GetSplitterAtPoint(Point point)
        {
            var splitters = GetSplitterRectangles();
            for (int i = splitters.Count - 1; i >= 0; i--)
            {
                if (splitters[i].Contains(point))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region 重写方法

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            var layoutRects = CalculateLayoutRectangles();

            foreach (var cell in gridCells)
            {
                if (cell.Position == FluentLayoutPosition.GridCustom)
                {
                    // GridCustom 模式使用特殊键
                    var key = (FluentLayoutPosition)((int)FluentLayoutPosition.GridCustom + cell.GridIndex);
                    if (layoutRects.ContainsKey(key))
                    {
                        cell.Bounds = layoutRects[key];
                        cell.Visible = true;
                    }
                    else
                    {
                        cell.Visible = false;
                    }
                }
                else if (cell.Position != FluentLayoutPosition.None && layoutRects.ContainsKey(cell.Position))
                {
                    cell.Bounds = layoutRects[cell.Position];
                    cell.Visible = true;
                }
                else
                {
                    cell.Visible = false;
                }
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            // 绘制背景
            if (BackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (showSplitter)
            {
                var splitters = GetSplitterRectangles();
                for (int i = 0; i < splitters.Count; i++)
                {
                    var rect = splitters[i];
                    var color = (i == hoverSplitter && allowSplitterDrag) ? splitterHoverColor : splitterColor;

                    using (var brush = new SolidBrush(color))
                    {
                        g.FillRectangle(brush, rect);
                    }

                    // 绘制拖动手柄（仅在允许拖动时）
                    if (allowSplitterDrag)
                    {
                        DrawSplitterGrip(g, rect);
                    }
                }
            }

            // 在设计模式下绘制网格线
            if (DesignMode && layoutMode == FluentLayoutMode.GridCustom)
            {
                DrawGridLines(g);
            }
        }

        /// <summary>
        /// 绘制分隔条拖动手柄
        /// </summary>
        private void DrawSplitterGrip(Graphics g, Rectangle rect)
        {
            bool isHorizontal = rect.Width > rect.Height;
            Color gripColor = Color.FromArgb(100, Color.White);

            using (var pen = new Pen(gripColor, 2))
            {
                if (isHorizontal)
                {
                    // 横向分隔条：绘制垂直的抓手
                    int centerX = rect.Left + rect.Width / 2;
                    int centerY = rect.Top + rect.Height / 2;

                    for (int i = -8; i <= 8; i += 4)
                    {
                        g.DrawLine(pen,
                            centerX + i, centerY - 1,
                            centerX + i, centerY + 1);
                    }
                }
                else
                {
                    // 竖向分隔条：绘制水平的抓手
                    int centerX = rect.Left + rect.Width / 2;
                    int centerY = rect.Top + rect.Height / 2;

                    for (int i = -8; i <= 8; i += 4)
                    {
                        g.DrawLine(pen,
                            centerX - 1, centerY + i,
                            centerX + 1, centerY + i);
                    }
                }
            }
        }

        private void DrawGridLines(Graphics g)
        {
            int borderOffset = showContainerBorder ? containerBorderSize : 0;
            int availableWidth = Width - 2 * borderOffset;
            int availableHeight = Height - 2 * borderOffset;

            float columnWidth = (float)availableWidth / GRID_COLUMNS;
            float rowHeight = (float)availableHeight / GRID_ROWS;

            using (var pen = new Pen(Color.FromArgb(50, Color.Gray)))
            {
                pen.DashStyle = DashStyle.Dot;

                // 绘制列线
                for (int i = 1; i < GRID_COLUMNS; i++)
                {
                    int x = borderOffset + (int)(i * columnWidth);
                    g.DrawLine(pen, x, borderOffset, x, Height - borderOffset);
                }

                // 绘制行线
                for (int i = 1; i < GRID_ROWS; i++)
                {
                    int y = borderOffset + (int)(i * rowHeight);
                    g.DrawLine(pen, borderOffset, y, Width - borderOffset, y);
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (showContainerBorder && containerBorderSize > 0)
            {
                var borderColor = GetThemeColor(c => c.Border, containerBorderColor);
                using (var pen = new Pen(borderColor, containerBorderSize))
                {
                    int halfWidth = containerBorderSize / 2;
                    g.DrawRectangle(pen, halfWidth, halfWidth,
                        Width - containerBorderSize, Height - containerBorderSize);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!allowSplitterDrag || !showSplitter)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                activeSplitter = GetSplitterAtPoint(e.Location);
                if (activeSplitter >= 0)
                {
                    isDragging = true;
                    Capture = true;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!allowSplitterDrag || !showSplitter)
            {
                Cursor = Cursors.Default;
                return;
            }

            int splitterIndex = GetSplitterAtPoint(e.Location);

            if (splitterIndex != hoverSplitter)
            {
                hoverSplitter = splitterIndex;
                Invalidate();
            }

            if (splitterIndex >= 0)
            {
                var splitterRect = GetSplitterRectangles()[splitterIndex];
                Cursor = splitterRect.Width > splitterRect.Height ? Cursors.HSplit : Cursors.VSplit;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (isDragging && activeSplitter >= 0)
            {
                UpdateSplitterPosition(e.Location);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (isDragging)
            {
                isDragging = false;
                activeSplitter = -1;
                Capture = false;
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoverSplitter >= 0)
            {
                hoverSplitter = -1;
                Invalidate();
            }

            Cursor = Cursors.Default;
        }

        /// <summary>
        /// 更新分隔条位置
        /// </summary>
        private void UpdateSplitterPosition(Point location)
        {
            var splitters = GetSplitterRectangles();
            if (activeSplitter < 0 || activeSplitter >= splitters.Count)
            {
                return;
            }

            Rectangle splitterRect = splitters[activeSplitter];
            bool isHorizontalBar = splitterRect.Width > splitterRect.Height; // 横向分隔条（水平线）
            int borderOffset = showContainerBorder ? containerBorderSize : 0;

            switch (layoutMode)
            {
                case FluentLayoutMode.Vertical:
                    {
                        // 垂直布局只有横向分隔条
                        float newDistance = (float)(location.Y - borderOffset) / (Height - 2 * borderOffset);
                        newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));

                        if (activeSplitter == 0)
                        {
                            // 第一个横向分隔条
                            VerticalSplitterDistance = Math.Min(newDistance, secondVerticalSplitterDistance - 0.05f);
                        }
                        else if (activeSplitter == 1)
                        {
                            // 第二个横向分隔条
                            SecondVerticalSplitterDistance = Math.Max(newDistance, verticalSplitterDistance + 0.05f);
                        }
                    }
                    break;

                case FluentLayoutMode.Horizontal:
                    {
                        // 水平布局只有竖向分隔条
                        float newDistance = (float)(location.X - borderOffset) / (Width - 2 * borderOffset);
                        newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));

                        if (activeSplitter == 0)
                        {
                            // 第一个竖向分隔条
                            HorizontalSplitterDistance = Math.Min(newDistance, secondHorizontalSplitterDistance - 0.05f);
                        }
                        else if (activeSplitter == 1)
                        {
                            // 第二个竖向分隔条
                            SecondHorizontalSplitterDistance = Math.Max(newDistance, horizontalSplitterDistance + 0.05f);
                        }
                    }
                    break;

                case FluentLayoutMode.Grid:
                case FluentLayoutMode.TopSpan:
                case FluentLayoutMode.BottomSpan:
                case FluentLayoutMode.LeftSpan:
                case FluentLayoutMode.RightSpan:
                    {
                        // Grid类布局：索引0是横向分隔条, 索引1是竖向分隔条
                        if (activeSplitter == 0)
                        {
                            // 横向分隔条（水平线）, 调整Y方向
                            float newDistance = (float)(location.Y - borderOffset) / (Height - 2 * borderOffset);
                            newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));
                            VerticalSplitterDistance = newDistance;
                        }
                        else if (activeSplitter == 1)
                        {
                            // 竖向分隔条（垂直线）, 调整X方向
                            float newDistance = (float)(location.X - borderOffset) / (Width - 2 * borderOffset);
                            newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));
                            HorizontalSplitterDistance = newDistance;
                        }
                    }
                    break;

                case FluentLayoutMode.ThreeColumns:
                    {
                        // 两个竖向分隔条
                        float newDistance = (float)(location.X - borderOffset) / (Width - 2 * borderOffset);
                        newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));

                        if (activeSplitter == 0)
                        {
                            HorizontalSplitterDistance = Math.Min(newDistance, secondHorizontalSplitterDistance - 0.05f);
                        }
                        else if (activeSplitter == 1)
                        {
                            SecondHorizontalSplitterDistance = Math.Max(newDistance, horizontalSplitterDistance + 0.05f);
                        }
                    }
                    break;

                case FluentLayoutMode.ThreeRows:
                case FluentLayoutMode.HeaderFooter:
                    {
                        // 两个横向分隔条
                        float newDistance = (float)(location.Y - borderOffset) / (Height - 2 * borderOffset);
                        newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));

                        if (activeSplitter == 0)
                        {
                            VerticalSplitterDistance = Math.Min(newDistance, secondVerticalSplitterDistance - 0.05f);
                        }
                        else if (activeSplitter == 1)
                        {
                            SecondVerticalSplitterDistance = Math.Max(newDistance, verticalSplitterDistance + 0.05f);
                        }
                    }
                    break;

                case FluentLayoutMode.Sidebar:
                case FluentLayoutMode.ReverseSidebar:
                    {
                        // 一个竖向分隔条
                        float newDistance = (float)(location.X - borderOffset) / (Width - 2 * borderOffset);
                        newDistance = Math.Max(0.05f, Math.Min(0.95f, newDistance));
                        HorizontalSplitterDistance = newDistance;
                    }
                    break;
            }

            PerformLayout();
            RefreshControl();
        }

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                splitterColor = GetThemeColor(c => c.Border, Color.FromArgb(200, 200, 200));
                splitterHoverColor = GetThemeColor(c => c.Primary, Color.FromArgb(100, 150, 200));
                containerBorderColor = GetThemeColor(c => c.Border, Color.DarkGray);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置栅格单元格定义
        /// </summary>
        public void SetGridCellDefinition(int cellIndex, int column, int row, int columnSpan, int rowSpan)
        {
            if (cellIndex < 1 || cellIndex > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            }

            if (layoutMode != FluentLayoutMode.GridCustom)
            {
                throw new InvalidOperationException("仅在 GridCustom 模式下可以设置栅格定义");
            }

            var cell = gridCells[cellIndex - 1];
            cell.GridDefinition.Column = column;
            cell.GridDefinition.Row = row;
            cell.GridDefinition.ColumnSpan = columnSpan;
            cell.GridDefinition.RowSpan = rowSpan;

            PerformLayout();
            RefreshControl();
        }

        /// <summary>
        /// 获取栅格单元格定义
        /// </summary>
        public GridCellDefinition GetGridCellDefinition(int cellIndex)
        {
            if (cellIndex < 1 || cellIndex > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            }

            return gridCells[cellIndex - 1].GridDefinition.Clone();
        }

        #endregion
    }

    #region 布局单元格

    //[Designer("System.Windows.Forms.Design.ScrollableControlDesigner, System.Design")]
    [Designer(typeof(FluentGridCellDesigner))]
    [ToolboxItem(false)]
    public class FluentGridCell : Panel
    {
        private int gridIndex;
        private FluentLayoutPosition position = FluentLayoutPosition.None;
        private GridCellDefinition gridDefinition;
        private Color cellBackColor = Color.Transparent;
        private bool showCellBorder = false;
        private Color cellBorderColor = Color.LightGray;

        public FluentGridCell(int gridIndex)
        {
            this.gridIndex = gridIndex;
            this.gridDefinition = new GridCellDefinition();

            // 设置默认属性
            BorderStyle = BorderStyle.None;
            Dock = DockStyle.None;
            Padding = new Padding(0);
            Margin = new Padding(0);
            DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        #region 属性

        /// <summary>
        /// 获取网格索引（1-4）
        /// </summary>
        [Browsable(false)]
        public int GridIndex => gridIndex;

        /// <summary>
        /// 获取或设置布局位置
        /// </summary>
        [Browsable(false)]
        public FluentLayoutPosition Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 栅格定义（仅在GridCustom模式下使用）
        /// </summary>
        [Category("Grid Layout")]
        [Description("栅格布局定义")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GridCellDefinition GridDefinition
        {
            get => gridDefinition;
            set
            {
                gridDefinition = value ?? new GridCellDefinition();
                Invalidate();
            }
        }

        /// <summary>
        /// 单元格背景色
        /// </summary>
        [Category("Appearance")]
        [Description("单元格背景颜色")]
        public Color CellBackColor
        {
            get => cellBackColor;
            set
            {
                if (cellBackColor != value)
                {
                    cellBackColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示单元格边框
        /// </summary>
        [Category("Appearance")]
        [Description("是否显示单元格边框")]
        [DefaultValue(false)]
        public bool ShowCellBorder
        {
            get => showCellBorder;
            set
            {
                if (showCellBorder != value)
                {
                    showCellBorder = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 单元格边框颜色
        /// </summary>
        [Category("Appearance")]
        [Description("单元格边框颜色")]
        public Color CellBorderColor
        {
            get => cellBorderColor;
            set
            {
                if (cellBorderColor != value)
                {
                    cellBorderColor = value;
                    Invalidate();
                }
            }
        }

        #endregion

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            // 设置默认停靠模式
            if (e.Control.Dock == DockStyle.None)
            {
                e.Control.Dock = DockStyle.Fill;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制背景
            if (cellBackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(cellBackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }

            base.OnPaint(e);

            // 绘制边框
            if (showCellBorder && Width > 0 && Height > 0)
            {
                using (var pen = new Pen(cellBorderColor, 1))
                {
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }

            // 在设计模式下显示位置提示
            if (DesignMode && Controls.Count == 0)
            {
                using (Font font = new Font("Segoe UI", 9f))
                using (StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    string text = position == FluentLayoutPosition.GridCustom
                        ? $"Cell {GridIndex}\n{gridDefinition}"
                        : $"Cell {GridIndex}\n({position})";

                    g.DrawString(text, font, SystemBrushes.GrayText,
                        new RectangleF(0, 0, Width, Height), format);
                }
            }
        }
    }

    #endregion

    #region 布局模板

    public static class FluentLayoutTemplates
    {
        /// <summary>
        /// 经典的IDE布局(顶部工具栏+左侧树+中心编辑区+底部输出)
        /// </summary>
        public static void ApplyIDELayout(FluentLayoutContainer container)
        {
            if (container.LayoutMode != FluentLayoutMode.GridCustom)
            {
                container.LayoutMode = FluentLayoutMode.GridCustom;
            }

            // Panel1: 顶部工具栏
            container.SetGridCellDefinition(1, 0, 0, 24, 2);

            // Panel2: 左侧树
            container.SetGridCellDefinition(2, 0, 2, 6, 20);

            // Panel3: 中心编辑区
            container.SetGridCellDefinition(3, 6, 2, 18, 15);

            // Panel4: 底部输出
            container.SetGridCellDefinition(4, 6, 17, 18, 5);
        }

        /// <summary>
        /// 响应式仪表板(2x2在大屏, 1x4在小屏)
        /// </summary>
        public static void ApplyDashboardLayout(FluentLayoutContainer container, bool isLargeScreen = true)
        {
            if (container.LayoutMode != FluentLayoutMode.GridCustom)
            {
                container.LayoutMode = FluentLayoutMode.GridCustom;
            }

            if (isLargeScreen)
            {
                // 2x2 布局
                container.SetGridCellDefinition(1, 0, 0, 12, 12);
                container.SetGridCellDefinition(2, 12, 0, 12, 12);
                container.SetGridCellDefinition(3, 0, 12, 12, 12);
                container.SetGridCellDefinition(4, 12, 12, 12, 12);
            }
            else
            {
                // 1x4 布局
                container.SetGridCellDefinition(1, 0, 0, 24, 6);
                container.SetGridCellDefinition(2, 0, 6, 24, 6);
                container.SetGridCellDefinition(3, 0, 12, 24, 6);
                container.SetGridCellDefinition(4, 0, 18, 24, 6);
            }
        }

        /// <summary>
        /// 邮件客户端布局
        /// </summary>
        public static void ApplyEmailLayout(FluentLayoutContainer container)
        {
            if (container.LayoutMode != FluentLayoutMode.GridCustom)
            {
                container.LayoutMode = FluentLayoutMode.GridCustom;
            }

            // Panel1: 左侧文件夹列表
            container.SetGridCellDefinition(1, 0, 0, 5, 24);

            // Panel2: 邮件列表
            container.SetGridCellDefinition(2, 5, 0, 7, 24);

            // Panel3: 邮件详情
            container.SetGridCellDefinition(3, 12, 0, 12, 24);

            // Panel4: 隐藏
            container.Panel4.Visible = false;
        }

        /// <summary>
        /// 属性编辑器布局
        /// </summary>
        public static void ApplyPropertyEditorLayout(FluentLayoutContainer container)
        {
            if (container.LayoutMode != FluentLayoutMode.GridCustom)
            {
                container.LayoutMode = FluentLayoutMode.GridCustom;
            }

            // Panel1: 左侧对象树
            container.SetGridCellDefinition(1, 0, 0, 6, 24);

            // Panel2: 中心预览
            container.SetGridCellDefinition(2, 6, 0, 12, 24);

            // Panel3: 右侧属性
            container.SetGridCellDefinition(3, 18, 0, 6, 24);

            // Panel4: 隐藏
            container.Panel4.Visible = false;
        }

        /// <summary>
        /// 黄金分割布局
        /// </summary>
        public static void ApplyGoldenRatioLayout(FluentLayoutContainer container, bool verticalSplit = false)
        {
            const float goldenRatio = 0.618f;

            if (verticalSplit)
            {
                container.LayoutMode = FluentLayoutMode.Horizontal;
                container.HorizontalSplitterDistance = goldenRatio;
                container.ShowLeftPanel = true;
                container.ShowRightPanel = false;
            }
            else
            {
                container.LayoutMode = FluentLayoutMode.Vertical;
                container.VerticalSplitterDistance = goldenRatio;
                container.ShowTopPanel = true;
                container.ShowBottomPanel = false;
            }
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 布局模式枚举
    /// </summary>
    public enum FluentLayoutMode
    {
        Vertical,       // 垂直布局(上中下)
        Horizontal,     // 水平布局(左中右)
        Grid,           // 网格布局(2x2)
        TopSpan,        // 顶部跨越
        BottomSpan,     // 底部跨越
        LeftSpan,       // 左侧跨越
        RightSpan,      // 右侧跨越
        ThreeColumns,   // 三列等宽
        ThreeRows,      // 三行等高
        Sidebar,        // 侧边栏布局(左侧边栏+右内容区)
        ReverseSidebar, // 反向侧边栏(左内容区+右侧边栏)
        HeaderFooter,   // 页眉页脚布局
        GridCustom      // 自定义栅格布局(24格系统)
    }

    /// <summary>
    /// 布局位置枚举
    /// </summary>
    public enum FluentLayoutPosition
    {
        None = 0,

        // 基础位置
        Center,
        Top,
        Bottom,
        Left,
        Right,

        // 四角位置
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,

        // 中间位置
        TopCenter,
        BottomCenter,
        LeftCenter,
        RightCenter,

        // 扩展位置
        Header,
        Footer,
        Sidebar,
        Content,

        // 自定义栅格位置
        GridCustom
    }

    /// <summary>
    /// 栅格单元格定义
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GridCellDefinition
    {
        [Category("Grid Layout")]
        [Description("起始列位置(0-23)")]
        [DefaultValue(0)]
        public int Column { get; set; } = 0;

        [Category("Grid Layout")]
        [Description("起始行位置(0-23)")]
        [DefaultValue(0)]
        public int Row { get; set; } = 0;

        [Category("Grid Layout")]
        [Description("跨越的列数(1-24)")]
        [DefaultValue(6)]
        public int ColumnSpan { get; set; } = 6;

        [Category("Grid Layout")]
        [Description("跨越的行数(1-24)")]
        [DefaultValue(6)]
        public int RowSpan { get; set; } = 6;

        /// <summary>
        /// 验证栅格定义
        /// </summary>
        public bool IsValid()
        {
            return Column >= 0 && Column < 24 &&
                   Row >= 0 && Row < 24 &&
                   ColumnSpan >= 1 && ColumnSpan <= 24 &&
                   RowSpan >= 1 && RowSpan <= 24 &&
                   Column + ColumnSpan <= 24 &&
                   Row + RowSpan <= 24;
        }

        public override string ToString()
        {
            return $"Col:{Column}, Row:{Row}, Span:{ColumnSpan}x{RowSpan}";
        }

        public GridCellDefinition Clone()
        {
            return new GridCellDefinition
            {
                Column = this.Column,
                Row = this.Row,
                ColumnSpan = this.ColumnSpan,
                RowSpan = this.RowSpan
            };
        }
    }

    #endregion

    #region 设计器

    public class FluentGridCellDesigner : ScrollableControlDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 移除 Dock 属性
            string[] removeProperties = { "Dock" };
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

    public class FluentLayoutContainerDesigner : ParentControlDesigner
    {
        private FluentLayoutContainer Container => (FluentLayoutContainer)Component;
        private ISelectionService selectionService;
        private IComponentChangeService changeService;
        private IDesignerHost designerHost;
        private bool draggingSplitter = false;
        private int activeSplitterIndex = -1;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;

            // 启用格子的设计模式
            if (Container.Panel1 != null)
            {
                EnableDesignMode(Container.Panel1, "panel1");
            }
            if (Container.Panel2 != null)
            {
                EnableDesignMode(Container.Panel2, "panel2");
            }
            if (Container.Panel3 != null)
            {
                EnableDesignMode(Container.Panel3, "panel3");
            }
            if (Container.Panel4 != null)
            {
                EnableDesignMode(Container.Panel4, "panel4");
            }

            // 订阅事件
            if (changeService != null)
            {
                changeService.ComponentAdded += OnComponentAdded;
                changeService.ComponentRemoved += OnComponentRemoved;
                changeService.ComponentChanged += OnComponentChanged;
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            if (Container == null || e == null || e.Component == null)
            {
                return;
            }

            // 如果添加的是普通控件（不是 FluentGridCell）
            if (e.Component is Control control && !(e.Component is FluentGridCell))
            {
                // 检查控件是否被添加到某个 Panel
                bool foundInPanel = false;

                foreach (var panel in Container.GridCells)
                {
                    if (panel != null && panel.Controls.Contains(control))
                    {
                        foundInPanel = true;

                        if (control.Dock == DockStyle.None)
                        {
                            // 通过 PropertyDescriptor 设置以序列化
                            PropertyDescriptor dockProp = TypeDescriptor.GetProperties(control)["Dock"];
                            if (dockProp != null)
                            {
                                dockProp.SetValue(control, DockStyle.Fill);
                            }
                        }

                        break;
                    }
                }

                // 将不再任何Panel中的控件移动至默认的Panel
                if (!foundInPanel && control.Parent == Container)
                {
                    var targetPanel = GetDefaultPanel();
                    if (targetPanel != null)
                    {
                        // 使用 IDesignerHost 来正确地重新父级化控件
                        if (designerHost != null)
                        {
                            using (DesignerTransaction transaction = designerHost.CreateTransaction("Reparent control"))
                            {
                                try
                                {
                                    // 从容器中移除
                                    PropertyDescriptor controlsProp = TypeDescriptor.GetProperties(Container)["Controls"];
                                    if (controlsProp != null)
                                    {
                                        Container.Controls.Remove(control);
                                    }

                                    // 添加到目标 Panel
                                    PropertyDescriptor panelControlsProp = TypeDescriptor.GetProperties(targetPanel)["Controls"];
                                    if (panelControlsProp != null)
                                    {
                                        targetPanel.Controls.Add(control);
                                    }

                                    // 设置 Dock
                                    PropertyDescriptor dockProp = TypeDescriptor.GetProperties(control)["Dock"];
                                    if (dockProp != null && control.Dock == DockStyle.None)
                                    {
                                        dockProp.SetValue(control, DockStyle.Fill);
                                    }

                                    transaction.Commit();
                                }
                                catch
                                {
                                    transaction.Cancel();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            // 清理工作
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            // 可以在这里处理属性变化
        }

        private FluentGridCell GetDefaultPanel()
        {
            // 如果有选中的 Panel, 返回它
            if (selectionService != null)
            {
                var selected = selectionService.PrimarySelection as Control;
                if (selected is FluentGridCell cell && Container.GridCells.Contains(cell))
                {
                    return cell;
                }

                // 如果选中的是 Panel 中的控件, 返回该 Panel
                if (selected?.Parent is FluentGridCell parentCell)
                {
                    return parentCell;
                }
            }

            // 返回第一个可见的 Panel
            if (Container.GridCells != null)
            {
                foreach (var panel in Container.GridCells)
                {
                    if (panel != null && panel.Visible)
                    {
                        return panel;
                    }
                }
            }

            return Container.Panel1;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            //properties.Remove("Controls");
            properties.Remove("AutoScroll");
            properties.Remove("AutoScrollMargin");
            properties.Remove("AutoScrollMinSize");
        }

        protected override bool GetHitTest(Point point)
        {
            if (Container == null)
            {
                return base.GetHitTest(point);
            }

            Point controlPoint = Container.PointToClient(point);

            // 检查是否启用了运行时拖动
            if (!Container.AllowSplitterDrag)
            {
                return base.GetHitTest(point);
            }

            // 检查点是否在分隔条上
            int splitterIndex = GetSplitterIndexAtPoint(controlPoint);
            return splitterIndex >= 0 || base.GetHitTest(point);
        }

        private int GetSplitterIndexAtPoint(Point point)
        {
            // 使用反射获取分隔条矩形
            var method = typeof(FluentLayoutContainer).GetMethod("GetSplitterRectangles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var splitters = method.Invoke(Container, null) as List<Rectangle>;
                if (splitters != null)
                {
                    for (int i = splitters.Count - 1; i >= 0; i--)
                    {
                        if (splitters[i].Contains(point))
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        public override bool CanParent(Control control)
        {
            // 只允许FluentGridCell作为直接子控件
            return control is FluentGridCell;
        }

        protected override Control GetParentForComponent(IComponent component)
        {
            if (Container == null || component == null)
            {
                return base.GetParentForComponent(component);
            }

            // 如果是 FluentGridCell, 返回容器本身
            if (component is FluentGridCell)
            {
                return Container;
            }

            // 其他控件：尝试找到当前选中的 Panel
            var targetPanel = GetDefaultPanel();
            if (targetPanel != null)
            {
                return targetPanel;
            }

            // 如果都失败了, 返回基类的实现
            return base.GetParentForComponent(component);
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);

            if (Container == null || pe == null)
            {
                return;
            }

            // 在设计时绘制分隔条（如果启用显示）
            if (Container.ShowSplitter)
            {
                DrawSplitters(pe);
            }

            // 绘制单元格边框提示
            if (Container.LayoutMode != FluentLayoutMode.GridCustom)
            {
                using (var pen = new Pen(Color.FromArgb(100, Color.Blue)))
                {
                    pen.DashStyle = DashStyle.Dot;

                    if (Container.GridCells != null)
                    {
                        foreach (var cell in Container.GridCells)
                        {
                            if (cell != null && cell.Visible && cell.Width > 0 && cell.Height > 0)
                            {
                                pe.Graphics.DrawRectangle(pen, cell.Bounds);

                                // 绘制 Panel 标签
                                if (cell.Controls.Count == 0)
                                {
                                    using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                                    using (var brush = new SolidBrush(Color.FromArgb(150, Color.Blue)))
                                    {
                                        var sf = new StringFormat
                                        {
                                            Alignment = StringAlignment.Center,
                                            LineAlignment = StringAlignment.Center
                                        };

                                        pe.Graphics.DrawString($"Panel{cell.GridIndex}",
                                            font, brush, cell.Bounds, sf);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawSplitters(PaintEventArgs pe)
        {
            var method = typeof(FluentLayoutContainer).GetMethod("GetSplitterRectangles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var splitters = method.Invoke(Container, null) as List<Rectangle>;
                if (splitters != null)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(150, Container.SplitterColor)))
                    {
                        foreach (var rect in splitters)
                        {
                            pe.Graphics.FillRectangle(brush, rect);
                        }
                    }
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                var actionLists = new DesignerActionListCollection();
                actionLists.Add(new FluentLayoutContainerActionList(Container));
                return actionLists;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (changeService != null)
                {
                    changeService.ComponentAdded -= OnComponentAdded;
                    changeService.ComponentRemoved -= OnComponentRemoved;
                    changeService.ComponentChanged -= OnComponentChanged;
                }
            }

            selectionService = null;
            changeService = null;
            designerHost = null;

            base.Dispose(disposing);
        }
        protected override InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if (Control == null)
                {
                    return base.InheritanceAttribute;
                }

                return base.InheritanceAttribute;
            }
        }

    }

    public class FluentLayoutContainerActionList : DesignerActionList
    {
        private FluentLayoutContainer container;
        private IDesignerHost host;

        public FluentLayoutContainerActionList(IComponent component) : base(component)
        {
            container = component as FluentLayoutContainer;
            host = GetService(typeof(IDesignerHost)) as IDesignerHost;
        }

        #region 属性

        [DisplayName("布局模式")]
        [Description("选择容器的布局模式")]
        public FluentLayoutMode LayoutMode
        {
            get => container.LayoutMode;
            set
            {
                GetPropertyByName("LayoutMode").SetValue(container, value);

                // 刷新智能标记面板
                var service = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
                service?.Refresh(container);
            }
        }

        [DisplayName("显示分隔条")]
        [Description("是否显示分隔条")]
        public bool ShowSplitter
        {
            get => container.ShowSplitter;
            set => GetPropertyByName("ShowSplitter").SetValue(container, value);
        }

        [DisplayName("允许拖动分隔条")]
        [Description("是否允许在运行时拖动分隔条")]
        public bool AllowSplitterDrag
        {
            get => container.AllowSplitterDrag;
            set => GetPropertyByName("AllowSplitterDrag").SetValue(container, value);
        }

        [DisplayName("分隔条大小")]
        [Description("分隔条的宽度")]
        public float SplitterSize
        {
            get => container.SplitterSize;
            set => GetPropertyByName("SplitterSize").SetValue(container, value);
        }

        #endregion

        #region 方法

        [DisplayName("在父容器中停靠")]
        [Description("将控件停靠填充父容器")]
        public void DockFill()
        {
            if (container.Parent != null)
            {
                PropertyDescriptor prop = GetPropertyByName("Dock");
                prop.SetValue(container, DockStyle.Fill);
            }
        }

        [DisplayName("取消停靠")]
        [Description("取消控件的停靠")]
        public void UndockControl()
        {
            PropertyDescriptor prop = GetPropertyByName("Dock");
            prop.SetValue(container, DockStyle.None);
        }

        [DisplayName("编辑栅格布局")]
        [Description("打开栅格布局编辑器")]
        public void EditGridLayout()
        {
            if (container.LayoutMode == FluentLayoutMode.GridCustom)
            {
                using (var editor = new GridLayoutEditorForm(container))
                {
                    editor.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("请先将布局模式设置为 GridCustom", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 标题
            items.Add(new DesignerActionHeaderItem("布局设置"));

            // 属性
            items.Add(new DesignerActionPropertyItem("LayoutMode", "布局模式", "布局设置"));
            items.Add(new DesignerActionPropertyItem("SplitterSize", "分隔条大小", "布局设置"));

            // 操作
            items.Add(new DesignerActionHeaderItem("操作"));

            if (container.Dock == DockStyle.Fill)
            {
                items.Add(new DesignerActionMethodItem(this, "UndockControl", "取消停靠", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "DockFill", "在父容器中停靠", "操作", true));
            }

            if (container.LayoutMode == FluentLayoutMode.GridCustom)
            {
                items.Add(new DesignerActionMethodItem(this, "EditGridLayout", "编辑栅格布局...", "操作", true));
            }

            items.Add(new DesignerActionHeaderItem("快速布局"));
            items.Add(new DesignerActionMethodItem(this, "ApplyGridLayout", "应用网格布局 (2x2)", "快速布局", false));
            items.Add(new DesignerActionMethodItem(this, "ApplySidebarLayout", "应用侧边栏布局", "快速布局", false));
            items.Add(new DesignerActionMethodItem(this, "ApplyHeaderFooterLayout", "应用页眉页脚布局", "快速布局", false));

            return items;
        }

        public void ApplyGridLayout()
        {
            GetPropertyByName("LayoutMode").SetValue(container, FluentLayoutMode.Grid);
        }

        public void ApplySidebarLayout()
        {
            GetPropertyByName("LayoutMode").SetValue(container, FluentLayoutMode.Sidebar);
        }

        public void ApplyHeaderFooterLayout()
        {
            GetPropertyByName("LayoutMode").SetValue(container, FluentLayoutMode.HeaderFooter);
        }

        private PropertyDescriptor GetPropertyByName(string propName)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(container)[propName];
            if (prop == null)
            {
                throw new ArgumentException("未找到属性", propName);
            }

            return prop;
        }
    }

    public class GridLayoutEditorForm : Form
    {
        private FluentLayoutContainer container;
        private Panel previewPanel;
        private GroupBox[] cellGroups;
        private NumericUpDown[] columnInputs;
        private NumericUpDown[] rowInputs;
        private NumericUpDown[] columnSpanInputs;
        private NumericUpDown[] rowSpanInputs;
        private CheckBox[] visibleCheckBoxes;
        private Button btnOK;
        private Button btnCancel;
        private Button btnReset;

        private const int GRID_SIZE = 24;

        public GridLayoutEditorForm(FluentLayoutContainer container)
        {
            this.container = container;
            InitializeComponents();
            LoadCurrentSettings();
        }

        private void InitializeComponents()
        {
            this.Text = "栅格布局编辑器";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 预览面板
            var lblPreview = new Label
            {
                Text = "布局预览 (24x24 栅格)",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };

            previewPanel = new Panel
            {
                Location = new Point(10, 35),
                Size = new Size(480, 480),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            previewPanel.Paint += PreviewPanel_Paint;

            // 设置面板
            var settingsPanel = new Panel
            {
                Location = new Point(500, 35),
                Size = new Size(280, 480),
                AutoScroll = true
            };

            cellGroups = new GroupBox[4];
            columnInputs = new NumericUpDown[4];
            rowInputs = new NumericUpDown[4];
            columnSpanInputs = new NumericUpDown[4];
            rowSpanInputs = new NumericUpDown[4];
            visibleCheckBoxes = new CheckBox[4];

            for (int i = 0; i < 4; i++)
            {
                int cellIndex = i;
                var group = new GroupBox
                {
                    Text = $"单元格 {i + 1}",
                    Location = new Point(5, i * 120),
                    Size = new Size(255, 115)
                };

                visibleCheckBoxes[i] = new CheckBox
                {
                    Text = "可见",
                    Location = new Point(10, 20),
                    Checked = true
                };
                visibleCheckBoxes[i].CheckedChanged += (s, e) => previewPanel.Invalidate();

                // 列
                var lblCol = new Label { Text = "起始列:", Location = new Point(10, 45), AutoSize = true };
                columnInputs[i] = new NumericUpDown
                {
                    Location = new Point(65, 43),
                    Size = new Size(60, 23),
                    Minimum = 0,
                    Maximum = GRID_SIZE - 1,
                    Value = 0
                };
                columnInputs[i].ValueChanged += (s, e) => previewPanel.Invalidate();

                var lblColSpan = new Label { Text = "列跨度:", Location = new Point(135, 45), AutoSize = true };
                columnSpanInputs[i] = new NumericUpDown
                {
                    Location = new Point(190, 43),
                    Size = new Size(55, 23),
                    Minimum = 1,
                    Maximum = GRID_SIZE,
                    Value = 6
                };
                columnSpanInputs[i].ValueChanged += (s, e) => previewPanel.Invalidate();

                // 行
                var lblRow = new Label { Text = "起始行:", Location = new Point(10, 75), AutoSize = true };
                rowInputs[i] = new NumericUpDown
                {
                    Location = new Point(65, 73),
                    Size = new Size(60, 23),
                    Minimum = 0,
                    Maximum = GRID_SIZE - 1,
                    Value = 0
                };
                rowInputs[i].ValueChanged += (s, e) => previewPanel.Invalidate();

                var lblRowSpan = new Label { Text = "行跨度:", Location = new Point(135, 75), AutoSize = true };
                rowSpanInputs[i] = new NumericUpDown
                {
                    Location = new Point(190, 73),
                    Size = new Size(55, 23),
                    Minimum = 1,
                    Maximum = GRID_SIZE,
                    Value = 6
                };
                rowSpanInputs[i].ValueChanged += (s, e) => previewPanel.Invalidate();

                group.Controls.AddRange(new Control[]
                {
                    visibleCheckBoxes[i],
                    lblCol, columnInputs[i], lblColSpan, columnSpanInputs[i],
                    lblRow, rowInputs[i], lblRowSpan, rowSpanInputs[i]
                });

                cellGroups[i] = group;
                settingsPanel.Controls.Add(group);
            }

            // 按钮
            btnReset = new Button
            {
                Text = "重置为默认",
                Location = new Point(10, 525),
                Size = new Size(100, 30)
            };
            btnReset.Click += BtnReset_Click;

            btnOK = new Button
            {
                Text = "确定",
                Location = new Point(580, 525),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(690, 525),
                Size = new Size(100, 30),
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // 添加控件
            this.Controls.AddRange(new Control[]
            {
                lblPreview, previewPanel, settingsPanel,
                btnReset, btnOK, btnCancel
            });
        }

        private void LoadCurrentSettings()
        {
            for (int i = 0; i < 4; i++)
            {
                var cell = container.GridCells[i];
                var def = cell.GridDefinition;

                visibleCheckBoxes[i].Checked = cell.Visible;
                columnInputs[i].Value = def.Column;
                rowInputs[i].Value = def.Row;
                columnSpanInputs[i].Value = def.ColumnSpan;
                rowSpanInputs[i].Value = def.RowSpan;
            }

            previewPanel.Invalidate();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            // 默认布局：四个6x6的区域
            var defaults = new[]
            {
                new { Col = 0, Row = 0, ColSpan = 12, RowSpan = 12 },
                new { Col = 12, Row = 0, ColSpan = 12, RowSpan = 12 },
                new { Col = 0, Row = 12, ColSpan = 12, RowSpan = 12 },
                new { Col = 12, Row = 12, ColSpan = 12, RowSpan = 12 }
            };

            for (int i = 0; i < 4; i++)
            {
                visibleCheckBoxes[i].Checked = true;
                columnInputs[i].Value = defaults[i].Col;
                rowInputs[i].Value = defaults[i].Row;
                columnSpanInputs[i].Value = defaults[i].ColSpan;
                rowSpanInputs[i].Value = defaults[i].RowSpan;
            }

            previewPanel.Invalidate();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // 验证设置
            for (int i = 0; i < 4; i++)
            {
                if (!visibleCheckBoxes[i].Checked)
                {
                    continue;
                }

                int col = (int)columnInputs[i].Value;
                int row = (int)rowInputs[i].Value;
                int colSpan = (int)columnSpanInputs[i].Value;
                int rowSpan = (int)rowSpanInputs[i].Value;

                if (col + colSpan > GRID_SIZE || row + rowSpan > GRID_SIZE)
                {
                    MessageBox.Show($"单元格 {i + 1} 的设置超出了栅格范围！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }

            // 应用设置
            for (int i = 0; i < 4; i++)
            {
                var cell = container.GridCells[i];

                if (visibleCheckBoxes[i].Checked)
                {
                    container.SetGridCellDefinition(
                        i + 1,
                        (int)columnInputs[i].Value,
                        (int)rowInputs[i].Value,
                        (int)columnSpanInputs[i].Value,
                        (int)rowSpanInputs[i].Value);
                    cell.Visible = true;
                }
                else
                {
                    cell.Visible = false;
                }
            }
        }

        private void PreviewPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cellWidth = (float)previewPanel.Width / GRID_SIZE;
            float cellHeight = (float)previewPanel.Height / GRID_SIZE;

            // 绘制网格线
            using (var gridPen = new Pen(Color.LightGray))
            {
                for (int i = 1; i < GRID_SIZE; i++)
                {
                    float x = i * cellWidth;
                    float y = i * cellHeight;
                    g.DrawLine(gridPen, x, 0, x, previewPanel.Height);
                    g.DrawLine(gridPen, 0, y, previewPanel.Width, y);
                }
            }

            // 绘制每5格的粗线
            using (var majorPen = new Pen(Color.Gray, 1.5f))
            {
                for (int i = 5; i < GRID_SIZE; i += 5)
                {
                    float x = i * cellWidth;
                    float y = i * cellHeight;
                    g.DrawLine(majorPen, x, 0, x, previewPanel.Height);
                    g.DrawLine(majorPen, 0, y, previewPanel.Width, y);
                }
            }

            // 绘制单元格区域
            var colors = new[]
            {
                Color.FromArgb(100, Color.Red),
                Color.FromArgb(100, Color.Blue),
                Color.FromArgb(100, Color.Green),
                Color.FromArgb(100, Color.Orange)
            };

            for (int i = 0; i < 4; i++)
            {
                if (!visibleCheckBoxes[i].Checked)
                {
                    continue;
                }

                int col = (int)columnInputs[i].Value;
                int row = (int)rowInputs[i].Value;
                int colSpan = (int)columnSpanInputs[i].Value;
                int rowSpan = (int)rowSpanInputs[i].Value;

                float x = col * cellWidth;
                float y = row * cellHeight;
                float width = colSpan * cellWidth;
                float height = rowSpan * cellHeight;

                var rect = new RectangleF(x, y, width, height);

                // 填充
                using (var brush = new SolidBrush(colors[i]))
                {
                    g.FillRectangle(brush, rect);
                }

                // 边框
                using (var pen = new Pen(Color.FromArgb(200, colors[i].R, colors[i].G, colors[i].B), 2))
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                // 标签
                using (var font = new Font("Arial", 12, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(150, Color.Black)))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    string label = $"Cell {i + 1}\n({col},{row})\n{colSpan}x{rowSpan}";
                    g.DrawString(label, font, brush, rect, sf);
                }
            }

            // 外边框
            using (var pen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(pen, 0, 0, previewPanel.Width - 1, previewPanel.Height - 1);
            }
        }
    }

    #endregion
}
