using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using static FluentControls.Controls.FluentComboBox;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{

    [DefaultEvent("SelectedIndexChanged")]
    [Designer(typeof(FluentListBoxDesigner))]
    public class FluentListBox : FluentControlBase
    {
        private FluentListItemCollection items;
        private List<int> selectedIndices = new List<int>();
        private int hoveredIndex = -1;
        private int focusedIndex = -1;

        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;
        private Panel scrollCorner;

        private int scrollOffsetY = 0;
        private int scrollOffsetX = 0;
        private int maxScrollY = 0;
        private int maxScrollX = 0;

        private bool showCheckBoxes = false;
        private bool multiSelect = false;
        private ScrollBarMode scrollBarMode = ScrollBarMode.Auto;

        private Rectangle contentBounds;
        private List<Rectangle> itemBounds = new List<Rectangle>();
        private List<Rectangle> checkBoxBounds = new List<Rectangle>();

        private Timer doubleClickTimer;
        private int lastClickIndex = -1;
        private DateTime lastClickTime = DateTime.MinValue;

        private bool isInitialized = false;

        public event EventHandler SelectedIndexChanged;
        public event EventHandler<ItemCheckEventArgs> ItemCheck;
        public event EventHandler<int> ItemDoubleClick;

        #region 构造函数

        public FluentListBox()
        {
            Size = new Size(200, 150);

            items = new FluentListItemCollection(this);

            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.StandardClick, true);
            SetStyle(ControlStyles.StandardDoubleClick, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!isInitialized)
            {
                InitializeComponents();
                isInitialized = true;
                RecalculateLayout();
            }
        }

        private void InitializeComponents()
        {
            // 初始化滚动条
            if (!DesignMode)
            {
                InitializeScrollBars();
                InitializeDoubleClick();
            }

            // 初始化内容边界
            contentBounds = new Rectangle(0, 0, Width, Height);
        }

        private void InitializeScrollBars()
        {
            if (vScrollBar == null)
            {
                vScrollBar = new VScrollBar
                {
                    Visible = false,
                    Dock = DockStyle.Right,
                    Width = SystemInformation.VerticalScrollBarWidth
                };
                vScrollBar.Scroll += OnVerticalScroll;
                vScrollBar.ValueChanged += (s, e) =>
                {
                    scrollOffsetY = vScrollBar.Value;
                    Invalidate();
                };
                Controls.Add(vScrollBar);
            }

            if (hScrollBar == null)
            {
                hScrollBar = new HScrollBar
                {
                    Visible = false,
                    Dock = DockStyle.Bottom,
                    Height = SystemInformation.HorizontalScrollBarHeight
                };
                hScrollBar.Scroll += OnHorizontalScroll;
                hScrollBar.ValueChanged += (s, e) =>
                {
                    scrollOffsetX = hScrollBar.Value;
                    Invalidate();
                };
                Controls.Add(hScrollBar);
            }

            if (scrollCorner == null)
            {
                scrollCorner = new Panel
                {
                    Visible = false,
                    Size = new Size(SystemInformation.VerticalScrollBarWidth,
                                   SystemInformation.HorizontalScrollBarHeight),
                    BackColor = ContainerBackColor
                };
                Controls.Add(scrollCorner);
            }
        }

        private void InitializeDoubleClick()
        {
            if (doubleClickTimer == null)
            {
                doubleClickTimer = new Timer();
                doubleClickTimer.Interval = SystemInformation.DoubleClickTime;
                doubleClickTimer.Tick += (s, e) =>
                {
                    doubleClickTimer.Stop();
                    lastClickIndex = -1;
                };
            }
        }

        #endregion

        #region 属性


        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        [Description("列表项集合")]
        public FluentListItemCollection Items => items;

        private int itemHeight = 30;
        [Category("Appearance")]
        [DefaultValue(30)]
        [Description("列表项高度")]
        public int ItemHeight
        {
            get => itemHeight;
            set
            {
                if (itemHeight != value && value > 0)
                {
                    itemHeight = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private int itemSpacing = 2;
        [Category("Appearance")]
        [DefaultValue(2)]
        [Description("列表项间距")]
        public int ItemSpacing
        {
            get => itemSpacing;
            set
            {
                if (itemSpacing != value && value >= 0)
                {
                    itemSpacing = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private Padding itemPadding = new Padding(8, 4, 8, 4);
        [Category("Appearance")]
        [Description("列表项内边距")]
        public Padding ItemPadding
        {
            get => itemPadding;
            set
            {
                if (itemPadding != value)
                {
                    itemPadding = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private int iconTextSpacing = 8;
        [Category("Appearance")]
        [DefaultValue(8)]
        [Description("图标与文本间距")]
        public int IconTextSpacing
        {
            get => iconTextSpacing;
            set
            {
                if (iconTextSpacing != value && value >= 0)
                {
                    iconTextSpacing = value;
                    if (isInitialized)
                    {
                        Invalidate();
                    }
                }
            }
        }

        private IconPosition iconPosition = IconPosition.Left;
        [Category("Appearance")]
        [DefaultValue(IconPosition.Left)]
        [Description("图标位置")]
        public IconPosition IconPosition
        {
            get => iconPosition;
            set
            {
                if (iconPosition != value)
                {
                    iconPosition = value;
                    if (isInitialized)
                    {
                        Invalidate();
                    }
                }
            }
        }

        private IconSizeMode iconSizeMode = IconSizeMode.AutoSize;
        [Category("Appearance")]
        [DefaultValue(IconSizeMode.AutoSize)]
        [Description("图标缩放模式")]
        public IconSizeMode IconSizeMode
        {
            get => iconSizeMode;
            set
            {
                if (iconSizeMode != value)
                {
                    iconSizeMode = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private Size? customIconSize;
        [Category("Appearance")]
        [Description("自定义图标尺寸")]
        public Size? CustomIconSize
        {
            get => customIconSize;
            set
            {
                if (customIconSize != value)
                {
                    customIconSize = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private int iconMargin = 4;
        [Category("Appearance")]
        [DefaultValue(4)]
        [Description("图标边距")]
        public int IconMargin
        {
            get => iconMargin;
            set
            {
                if (iconMargin != value && value >= 0)
                {
                    iconMargin = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private FluentBorderStyle borderStyle = FluentBorderStyle.FocusOnly;
        [Category("Appearance")]
        [DefaultValue(FluentBorderStyle.FocusOnly)]
        [Description("边框显示样式")]
        public FluentBorderStyle BorderStyle
        {
            get => borderStyle;
            set
            {
                if (borderStyle != value)
                {
                    borderStyle = value;
                    if (isInitialized)
                    {
                        Invalidate();
                    }
                }
            }
        }

        private bool showFocusBorder = true;
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否在聚焦时显示边框")]
        public bool ShowFocusBorder
        {
            get => showFocusBorder;
            set
            {
                if (showFocusBorder != value)
                {
                    showFocusBorder = value;
                    if (isInitialized)
                    {
                        Invalidate();
                    }
                }
            }
        }

        private Color? borderColor;
        [Category("Colors")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor ?? (Theme?.Colors?.Border ?? SystemColors.ControlDark);
            set
            {
                borderColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? focusedBorderColor;
        [Category("Colors")]
        [Description("聚焦时边框颜色")]
        public Color FocusedBorderColor
        {
            get => focusedBorderColor ?? (Theme?.Colors?.BorderFocused ?? SystemColors.Highlight);
            set
            {
                focusedBorderColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private int borderWidth = 1;
        [Category("Appearance")]
        [DefaultValue(1)]
        [Description("边框宽度")]
        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth != value && value >= 0 && value <= 5)
                {
                    borderWidth = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private Color? itemBackColor;
        [Category("Colors")]
        [Description("列表项背景色")]
        public Color ItemBackColor
        {
            get => GetThemeColor(c => c.Surface, itemBackColor ?? Color.White);
            set
            {
                itemBackColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? itemForeColor;
        [Category("Colors")]
        [Description("列表项前景色")]
        public Color ItemForeColor
        {
            get => GetThemeColor(c => c.TextPrimary, itemForeColor ?? Color.Black);
            set
            {
                itemForeColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? selectedBackColor;
        [Category("Colors")]
        [Description("选中项背景色")]
        public Color SelectedBackColor
        {
            get => GetThemeColor(c => c.Primary, selectedBackColor ?? SystemColors.Highlight);
            set
            {
                selectedBackColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? selectedForeColor;
        [Category("Colors")]
        [Description("选中项前景色")]
        public Color SelectedForeColor
        {
            get => GetThemeColor(c => c.TextOnPrimary, selectedForeColor ?? Color.White);
            set
            {
                selectedForeColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? hoverBackColor;
        [Category("Colors")]
        [Description("悬停项背景色")]
        public Color HoverBackColor
        {
            get => GetThemeColor(c => c.SurfaceHover, hoverBackColor ?? Color.LightGray); 
            set
            {
                hoverBackColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? hoverForeColor;
        [Category("Colors")]
        [Description("悬停项前景色")]
        public Color HoverForeColor
        {
            get => GetThemeColor(c => c.TextPrimary, hoverForeColor ?? Color.Black); 
            set
            {
                hoverForeColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        private Color? containerBackColor;
        [Category("Colors")]
        [Description("容器背景色")]
        public Color ContainerBackColor
        {
            get => GetThemeColor(c => c.Background, containerBackColor ?? SystemColors.Window);
            set
            {
                containerBackColor = value;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("是否显示复选框")]
        public bool ShowCheckBoxes
        {
            get => showCheckBoxes;
            set
            {
                if (showCheckBoxes != value)
                {
                    showCheckBoxes = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        private int checkBoxMargin = 4;
        [Category("Behavior")]
        [DefaultValue(4)]
        [Description("复选框边距")]
        public int CheckBoxMargin
        {
            get => checkBoxMargin;
            set
            {
                if (checkBoxMargin != value && value >= 0)
                {
                    checkBoxMargin = value;
                    if (isInitialized)
                    {
                        RecalculateLayout();
                        Invalidate();
                    }
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("是否支持多选")]
        public bool MultiSelect
        {
            get => multiSelect;
            set
            {
                if (multiSelect != value)
                {
                    multiSelect = value;
                    if (!multiSelect && selectedIndices.Count > 1)
                    {
                        var first = selectedIndices.FirstOrDefault();
                        selectedIndices.Clear();
                        if (first >= 0)
                        {
                            selectedIndices.Add(first);
                        }
                    }
                    if (isInitialized)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(ScrollBarMode.Auto)]
        [Description("滚动条模式")]
        public ScrollBarMode ScrollBarMode
        {
            get => scrollBarMode;
            set
            {
                if (scrollBarMode != value)
                {
                    scrollBarMode = value;
                    if (isInitialized)
                    {
                        UpdateScrollBars();
                        Invalidate();
                    }
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => selectedIndices?.FirstOrDefault() ?? -1;
            set
            {
                if (value < -1 || value >= items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                selectedIndices.Clear();
                if (value >= 0)
                {
                    selectedIndices.Add(value);
                    if (isInitialized)
                    {
                        EnsureVisible(value);
                    }
                }
                OnSelectedIndexChanged(EventArgs.Empty);
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<int> SelectedIndices => selectedIndices.AsReadOnly();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                var index = SelectedIndex;
                return index >= 0 ? items[index].Text : null;
            }
            set
            {
                var index = items.ToList().FindIndex(i => i.Text == value);
                SelectedIndex = index;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get
            {
                var index = SelectedIndex;
                return index >= 0 ? items[index].Value : null;
            }
            set
            {
                var index = items.ToList().FindIndex(i => Equals(i.Value, value));
                SelectedIndex = index;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FluentListItem SelectedItem
        {
            get
            {
                var index = SelectedIndex;
                return index >= 0 ? items[index] : null;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<object> SelectedValues
        {
            get
            {
                return selectedIndices.Select(i => items[i].Value).ToList();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> SelectedTexts
        {
            get
            {
                return selectedIndices.Select(i => items[i].Text).ToList();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<FluentListItem> SelectedItems
        {
            get
            {
                return selectedIndices.Select(i => items[i]).ToList();
            }
        }

        #endregion

        #region 事件

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        protected virtual void OnItemCheck(ItemCheckEventArgs e)
        {
            ItemCheck?.Invoke(this, e);
        }

        protected virtual void OnItemDoubleClick(int index)
        {
            ItemDoubleClick?.Invoke(this, index);
        }

        internal void OnItemsChanged()
        {
            if (isInitialized)
            {
                RecalculateLayout();
                Invalidate();
            }
        }

        #endregion

        #region 项管理

        public void AddItem(object value, Image icon = null, int index = -1)
        {
            var item = new FluentListItem(value, index == -1 ? items.Count : index)
            {
                Icon = icon
            };

            items.Add(item);
        }

        public void AddItem(object value, string text, Image icon = null, int index = -1)
        {
            var item = new FluentListItem(value, text, index == -1 ? items.Count : index)
            {
                Icon = icon
            };

            items.Add(item);
        }

        public void AddItem(string text, Image icon = null, int index = -1)
        {
            var item = new FluentListItem(text, text, index == -1 ? items.Count : index)
            {
                Icon = icon
            };

            items.Add(item);
        }

        public FluentListItem FindItemByValue(object value)
        {
            return items.FirstOrDefault(i => Equals(i.Value, value));
        }

        public FluentListItem FindItemByText(string text)
        {
            return items.FirstOrDefault(i => i.Text == text);
        }

        public int FindIndexByValue(object value)
        {
            return items.ToList().FindIndex(i => Equals(i.Value, value));
        }

        public int FindIndexByText(string text)
        {
            return items.ToList().FindIndex(i => i.Text == text);
        }

        public void Clear()
        {
            items.Clear();
            selectedIndices.Clear();
            hoveredIndex = -1;
            focusedIndex = -1;
            scrollOffsetY = 0;
            scrollOffsetX = 0;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                return;
            }

            items.RemoveAt(index);

            // 更新选中索引
            selectedIndices.RemoveAll(i => i == index);
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                if (selectedIndices[i] > index)
                {
                    selectedIndices[i]--;
                }
            }
        }

        public void RemoveByItemIndex(int itemIndex)
        {
            var index = items.ToList().FindIndex(i => i.Index == itemIndex);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        public void SortByIndex()
        {
            var sortedList = items.OrderBy(i => i.Index).ToList();
            items.Clear();
            items.AddRange(sortedList);
        }

        public void SetItemIcon(int index, Image icon)
        {
            if (index >= 0 && index < items.Count)
            {
                items[index].Icon = icon;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        public void SetItemChecked(int index, bool isChecked)
        {
            if (index >= 0 && index < items.Count && showCheckBoxes)
            {
                items[index].Checked = isChecked;
                OnItemCheck(new ItemCheckEventArgs(index, isChecked, items[index]));
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        public bool GetItemChecked(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                return items[index].Checked;
            }
            return false;
        }

        public List<FluentListItem> GetCheckedItems()
        {
            return items.Where(i => i.Checked).ToList();
        }

        #endregion

        #region 布局计算

        private void RecalculateLayout()
        {
            if (!isInitialized)
            {
                return;
            }

            // 计算实际的内容区域(考虑边框)
            int borderOffset = 0;
            if (borderStyle != FluentBorderStyle.None)
            {
                borderOffset = borderStyle == FluentBorderStyle.Fixed3D ? 2 : borderWidth;
            }

            if (items.Count == 0)
            {
                maxScrollY = 0;
                maxScrollX = 0;
                UpdateScrollBars();
                return;
            }

            itemBounds.Clear();
            checkBoxBounds.Clear();

            // 获取图标尺寸
            Size iconSize = GetIconSize();

            int y = ItemPadding.Top + borderOffset;
            int maxWidth = 0;

            using (var g = CreateGraphics())
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var text = item.ToString();
                    var textSize = TextRenderer.MeasureText(g, text, Font);

                    int itemWidth = ItemPadding.Left + ItemPadding.Right + textSize.Width + (borderOffset * 2);
                    int checkBoxWidth = 0;

                    if (showCheckBoxes)
                    {
                        checkBoxWidth = 16 + checkBoxMargin * 2;
                        itemWidth += checkBoxWidth;

                        var checkRect = new Rectangle(
                            ItemPadding.Left + checkBoxMargin + borderOffset,
                            y + (itemHeight - 16) / 2,
                            16, 16);
                        checkBoxBounds.Add(checkRect);
                    }

                    if (item.Icon != null)
                    {
                        itemWidth += iconSize.Width + iconTextSpacing;
                    }

                    var itemRect = new Rectangle(borderOffset, y, Width - (borderOffset * 2), itemHeight);
                    itemBounds.Add(itemRect);

                    maxWidth = Math.Max(maxWidth, itemWidth);
                    y += itemHeight + itemSpacing;
                }
            }

            // 计算总高度(最后一项不需要间距)
            int totalHeight = y - itemSpacing + ItemPadding.Bottom + borderOffset;

            // 计算可视区域高度
            int visibleHeight = Height - (borderOffset * 2);
            if (hScrollBar != null && hScrollBar.Visible)
            {
                visibleHeight -= hScrollBar.Height;
            }

            // 计算可视区域宽度
            int visibleWidth = Width - (borderOffset * 2);
            if (vScrollBar != null && vScrollBar.Visible)
            {
                visibleWidth -= vScrollBar.Width;
            }

            // 计算最大滚动值
            maxScrollY = Math.Max(0, totalHeight - visibleHeight);
            maxScrollX = Math.Max(0, maxWidth - visibleWidth);

            // 确保滚动偏移在有效范围内
            scrollOffsetY = Math.Max(0, Math.Min(scrollOffsetY, maxScrollY));
            scrollOffsetX = Math.Max(0, Math.Min(scrollOffsetX, maxScrollX));

            UpdateScrollBars();
        }

        private void UpdateScrollBars()
        {
            if (!isInitialized || DesignMode)
            {
                return;
            }

            // 计算边框偏移
            int borderOffset = 0;
            if (borderStyle != FluentBorderStyle.None)
            {
                borderOffset = borderStyle == FluentBorderStyle.Fixed3D ? 2 : borderWidth;
            }

            // 计算可视区域
            int visibleHeight = Height - (borderOffset * 2);
            int visibleWidth = Width - (borderOffset * 2);

            bool needVScroll = false;
            bool needHScroll = false;

            // 首先判断是否需要滚动条
            switch (scrollBarMode)
            {
                case ScrollBarMode.Auto:
                    // 先检查垂直滚动条
                    if (maxScrollY > 0)
                    {
                        needVScroll = true;
                        visibleWidth -= SystemInformation.VerticalScrollBarWidth;
                        // 重新计算水平滚动需求
                        maxScrollX = Math.Max(0, GetMaxItemWidth() - visibleWidth);
                    }
                    // 再检查水平滚动条
                    if (maxScrollX > 0)
                    {
                        needHScroll = true;
                        if (!needVScroll)
                        {
                            visibleHeight -= SystemInformation.HorizontalScrollBarHeight;
                            // 重新计算垂直滚动需求
                            maxScrollY = Math.Max(0, GetTotalHeight() - visibleHeight);
                            needVScroll = maxScrollY > 0;
                        }
                    }
                    break;

                case ScrollBarMode.Vertical:
                    needVScroll = maxScrollY > 0;
                    if (needVScroll)
                    {
                        visibleWidth -= SystemInformation.VerticalScrollBarWidth;
                    }

                    break;

                case ScrollBarMode.Horizontal:
                    needHScroll = maxScrollX > 0;
                    if (needHScroll)
                    {
                        visibleHeight -= SystemInformation.HorizontalScrollBarHeight;
                    }

                    break;

                case ScrollBarMode.Both:
                    needVScroll = maxScrollY > 0;
                    needHScroll = maxScrollX > 0;
                    if (needVScroll)
                    {
                        visibleWidth -= SystemInformation.VerticalScrollBarWidth;
                    }

                    if (needHScroll)
                    {
                        visibleHeight -= SystemInformation.HorizontalScrollBarHeight;
                    }

                    break;
            }

            // 更新实际可视区域
            if (needHScroll)
            {
                visibleHeight = Height - (borderOffset * 2) - SystemInformation.HorizontalScrollBarHeight;
            }

            if (needVScroll)
            {
                visibleWidth = Width - (borderOffset * 2) - SystemInformation.VerticalScrollBarWidth;
            }

            // 重新计算最大滚动值
            maxScrollY = Math.Max(0, GetTotalHeight() - visibleHeight);
            maxScrollX = Math.Max(0, GetMaxItemWidth() - visibleWidth);

            // 设置垂直滚动条
            if (vScrollBar != null)
            {
                vScrollBar.Visible = needVScroll;
                if (needVScroll)
                {
                    vScrollBar.Minimum = 0;
                    vScrollBar.SmallChange = 5;// itemHeight;
                    vScrollBar.LargeChange = Math.Max(1, visibleHeight);
                    vScrollBar.Maximum = Math.Max(0, maxScrollY + vScrollBar.LargeChange - 1);

                    // 确保当前值在有效范围内
                    int maxValue = Math.Max(0, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
                    vScrollBar.Value = Math.Min(scrollOffsetY, maxValue);
                    scrollOffsetY = vScrollBar.Value;
                }
            }

            // 设置水平滚动条
            if (hScrollBar != null)
            {
                hScrollBar.Visible = needHScroll;
                if (needHScroll)
                {
                    hScrollBar.Minimum = 0;
                    hScrollBar.SmallChange = 20;
                    hScrollBar.LargeChange = Math.Max(1, visibleWidth);
                    hScrollBar.Maximum = Math.Max(0, maxScrollX + hScrollBar.LargeChange - 1);

                    // 确保当前值在有效范围内
                    int maxValue = Math.Max(0, hScrollBar.Maximum - hScrollBar.LargeChange + 1);
                    hScrollBar.Value = Math.Min(scrollOffsetX, maxValue);
                    scrollOffsetX = hScrollBar.Value;
                }
            }

            // 设置滚动条角落
            if (scrollCorner != null)
            {
                scrollCorner.Visible = needVScroll && needHScroll;
                if (scrollCorner.Visible)
                {
                    scrollCorner.Location = new Point(
                        Width - SystemInformation.VerticalScrollBarWidth,
                        Height - SystemInformation.HorizontalScrollBarHeight);
                    scrollCorner.BackColor = ContainerBackColor;
                }
            }

            // 更新内容边界
            contentBounds = new Rectangle(
                borderOffset,
                borderOffset,
                Width - (needVScroll ? SystemInformation.VerticalScrollBarWidth : 0) - (borderOffset * 2),
                Height - (needHScroll ? SystemInformation.HorizontalScrollBarHeight : 0) - (borderOffset * 2));
        }

        private Size GetIconSize()
        {
            if (customIconSize.HasValue)
            {
                return customIconSize.Value;
            }

            // 自动计算图标区域大小
            int iconHeight = itemHeight - (ItemPadding.Top + ItemPadding.Bottom + iconMargin * 2);
            int iconWidth = iconHeight; // 默认正方形

            return new Size(Math.Max(16, iconWidth), Math.Max(16, iconHeight));
        }

        private int GetTotalHeight()
        {
            if (items.Count == 0)
            {
                return 0;
            }

            int borderOffset = 0;
            if (borderStyle != FluentBorderStyle.None)
            {
                borderOffset = borderStyle == FluentBorderStyle.Fixed3D ? 2 : borderWidth;
            }

            int totalHeight = ItemPadding.Top + borderOffset;
            totalHeight += items.Count * itemHeight;
            totalHeight += Math.Max(0, items.Count - 1) * itemSpacing;
            totalHeight += ItemPadding.Bottom + borderOffset;

            // Fix: 用于补充缺失高度(但是本不应该添加)
            //totalHeight += SystemInformation.HorizontalScrollBarHeight;

            return totalHeight;
        }

        private int GetMaxItemWidth()
        {
            if (items.Count == 0)
            {
                return 0;
            }

            int borderOffset = 0;
            if (borderStyle != FluentBorderStyle.None)
            {
                borderOffset = borderStyle == FluentBorderStyle.Fixed3D ? 2 : borderWidth;
            }

            Size iconSize = GetIconSize();
            int maxWidth = 0;

            using (var g = CreateGraphics())
            {
                foreach (var item in items)
                {
                    var text = item.ToString();
                    var textSize = TextRenderer.MeasureText(g, text, Font);

                    int itemWidth = ItemPadding.Left + ItemPadding.Right + textSize.Width + (borderOffset * 2);

                    if (showCheckBoxes)
                    {
                        itemWidth += 16 + checkBoxMargin * 2;
                    }

                    if (item.Icon != null)
                    {
                        itemWidth += iconSize.Width + iconTextSpacing;
                    }

                    maxWidth = Math.Max(maxWidth, itemWidth);
                }
            }

            return maxWidth;
        }


        private void EnsureVisible(int index)
        {
            if (!isInitialized || index < 0 || index >= itemBounds.Count)
            {
                return;
            }

            var itemRect = itemBounds[index];

            // 垂直滚动
            if (vScrollBar != null && vScrollBar.Visible)
            {
                if (itemRect.Top < scrollOffsetY)
                {
                    scrollOffsetY = itemRect.Top - ItemPadding.Top;
                }
                else if (itemRect.Bottom > scrollOffsetY + contentBounds.Height)
                {
                    scrollOffsetY = itemRect.Bottom - contentBounds.Height + ItemPadding.Bottom;
                }

                // 确保在有效范围内
                int maxScroll = Math.Max(0, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
                scrollOffsetY = Math.Max(0, Math.Min(maxScroll, scrollOffsetY));
                vScrollBar.Value = scrollOffsetY;
            }

            // 水平滚动
            if (hScrollBar != null && hScrollBar.Visible)
            {
                //Todo: 水平滚动的逻辑
            }

            Invalidate();
        }

        #endregion

        #region 事件处理

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!isInitialized)
            {
                return;
            }

            int newHoveredIndex = GetItemIndexAt(e.Location);
            if (newHoveredIndex != hoveredIndex)
            {
                hoveredIndex = newHoveredIndex;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredIndex != -1)
            {
                hoveredIndex = -1;
                if (isInitialized)
                {
                    Invalidate();
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!isInitialized || e.Button != MouseButtons.Left)
            {
                return;
            }

            Focus();

            int clickedIndex = GetItemIndexAt(e.Location);
            if (clickedIndex == -1)
            {
                return;
            }

            // 检查是否点击了复选框
            if (showCheckBoxes && clickedIndex < checkBoxBounds.Count)
            {
                var checkRect = checkBoxBounds[clickedIndex];
                checkRect.Offset(0, -scrollOffsetY);

                if (checkRect.Contains(e.Location))
                {
                    items[clickedIndex].Checked = !items[clickedIndex].Checked;
                    OnItemCheck(new ItemCheckEventArgs(clickedIndex, items[clickedIndex].Checked, items[clickedIndex]));
                    Invalidate();
                    return;
                }
            }

            // 处理双击
            if (clickedIndex == lastClickIndex &&
                (DateTime.Now - lastClickTime).TotalMilliseconds <= SystemInformation.DoubleClickTime)
            {
                doubleClickTimer?.Stop();
                OnItemDoubleClick(clickedIndex);
                lastClickIndex = -1;
            }
            else
            {
                lastClickIndex = clickedIndex;
                lastClickTime = DateTime.Now;
                doubleClickTimer?.Start();
            }

            // 处理选择
            if (multiSelect && (Control.ModifierKeys & Keys.Control) != 0)
            {
                // Ctrl+Click 多选
                if (selectedIndices.Contains(clickedIndex))
                {
                    selectedIndices.Remove(clickedIndex);
                }
                else
                {
                    selectedIndices.Add(clickedIndex);
                }
            }
            else if (multiSelect && (Control.ModifierKeys & Keys.Shift) != 0 && selectedIndices.Count > 0)
            {
                // Shift+Click 范围选择
                int start = selectedIndices.Last();
                int end = clickedIndex;

                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                }

                selectedIndices.Clear();
                for (int i = start; i <= end; i++)
                {
                    selectedIndices.Add(i);
                }
            }
            else
            {
                // 单选
                selectedIndices.Clear();
                selectedIndices.Add(clickedIndex);
            }

            focusedIndex = clickedIndex;
            OnSelectedIndexChanged(EventArgs.Empty);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!isInitialized)
            {
                return;
            }

            if (vScrollBar != null && vScrollBar.Visible)
            {
                int delta = (e.Delta / 120) * itemHeight;
                int newValue = scrollOffsetY - delta;

                // 计算实际的最大可滚动值
                int maxScroll = Math.Max(0, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
                newValue = Math.Max(0, Math.Min(maxScroll, newValue));

                if (newValue != scrollOffsetY)
                {
                    scrollOffsetY = newValue;
                    vScrollBar.Value = scrollOffsetY;
                    Invalidate();
                }
            }
            else if (hScrollBar != null && hScrollBar.Visible)
            {
                int delta = (e.Delta / 120) * 20;
                int newValue = scrollOffsetX - delta;

                // 计算实际的最大可滚动值
                int maxScroll = Math.Max(0, hScrollBar.Maximum - hScrollBar.LargeChange + 1);
                newValue = Math.Max(0, Math.Min(maxScroll, newValue));

                if (newValue != scrollOffsetX)
                {
                    scrollOffsetX = newValue;
                    hScrollBar.Value = scrollOffsetX;
                    Invalidate();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!isInitialized || items.Count == 0)
            {
                return;
            }

            int newIndex = focusedIndex < 0 ? 0 : focusedIndex;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newIndex = Math.Max(0, newIndex - 1);
                    break;
                case Keys.Down:
                    newIndex = Math.Min(items.Count - 1, newIndex + 1);
                    break;
                case Keys.Home:
                    newIndex = 0;
                    break;
                case Keys.End:
                    newIndex = items.Count - 1;
                    break;
                case Keys.PageUp:
                    newIndex = Math.Max(0, newIndex - (contentBounds.Height / (itemHeight + itemSpacing)));
                    break;
                case Keys.PageDown:
                    newIndex = Math.Min(items.Count - 1, newIndex + (contentBounds.Height / (itemHeight + itemSpacing)));
                    break;
                case Keys.Space:
                    if (showCheckBoxes && focusedIndex >= 0)
                    {
                        items[focusedIndex].Checked = !items[focusedIndex].Checked;
                        OnItemCheck(new ItemCheckEventArgs(focusedIndex, items[focusedIndex].Checked, items[focusedIndex]));
                        Invalidate();
                    }
                    return;
            }

            if (newIndex != focusedIndex)
            {
                focusedIndex = newIndex;

                if (!multiSelect || (Control.ModifierKeys & Keys.Shift) == 0)
                {
                    selectedIndices.Clear();
                    selectedIndices.Add(focusedIndex);
                    OnSelectedIndexChanged(EventArgs.Empty);
                }

                EnsureVisible(focusedIndex);
                Invalidate();
            }
        }

        private void OnVerticalScroll(object sender, ScrollEventArgs e)
        {
            if (e.Type != ScrollEventType.EndScroll)
            {
                scrollOffsetY = e.NewValue;
                Invalidate();
            }
        }

        private void OnHorizontalScroll(object sender, ScrollEventArgs e)
        {
            if (e.Type != ScrollEventType.EndScroll)
            {
                scrollOffsetX = e.NewValue;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (isInitialized)
            {
                RecalculateLayout();
            }
        }

        private int GetItemIndexAt(Point location)
        {
            if (!isInitialized)
            {
                return -1;
            }

            for (int i = 0; i < itemBounds.Count; i++)
            {
                var rect = itemBounds[i];
                rect.Offset(-scrollOffsetX, -scrollOffsetY);

                if (rect.Contains(location))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region 样式

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
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


            // 更新边框颜色
            if (!borderColor.HasValue)
            {
                Invalidate();
            }

        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            using (var brush = new SolidBrush(ContainerBackColor))
            {
                g.FillRectangle(brush, contentBounds);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (!isInitialized || items.Count == 0)
            {
                return;
            }

            // 设置裁剪区域
            g.SetClip(contentBounds);

            for (int i = 0; i < items.Count && i < itemBounds.Count; i++)
            {
                var itemRect = itemBounds[i];
                itemRect.Offset(-scrollOffsetX, -scrollOffsetY);

                // 只绘制可见项
                if (itemRect.Bottom < 0 || itemRect.Top > contentBounds.Height)
                {
                    continue;
                }

                DrawItem(g, i, itemRect);
            }

            g.ResetClip();
        }

        private void DrawItem(Graphics g, int index, Rectangle bounds)
        {
            if (index >= items.Count)
            {
                return;
            }

            var item = items[index];
            bool isSelected = selectedIndices.Contains(index);
            bool isHovered = hoveredIndex == index;
            bool isFocused = focusedIndex == index;

            // 绘制背景
            Color backColor = ItemBackColor;
            if (isSelected)
            {
                backColor = SelectedBackColor;
            }
            else if (isHovered)
            {
                backColor = HoverBackColor;
            }

            using (var brush = new SolidBrush(backColor))
            {
                var rect = bounds;
                rect.Inflate(-1, 0);

                int cornerRadius = Theme?.Elevation?.CornerRadiusSmall ?? 4;
                if (cornerRadius > 0)
                {
                    using (var path = GetRoundedRectangle(rect, cornerRadius))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    g.FillRectangle(brush, rect);
                }
            }

            // 绘制焦点框
            if (isFocused && Focused)
            {
                Color borderColor = Theme?.Colors?.BorderFocused ?? SystemColors.Highlight;
                using (var pen = new Pen(borderColor, 2))
                {
                    pen.DashStyle = DashStyle.Dot;
                    var focusRect = bounds;
                    focusRect.Inflate(-2, -1);
                    g.DrawRectangle(pen, focusRect);
                }
            }

            int x = bounds.X + ItemPadding.Left;
            int y = bounds.Y + (bounds.Height - Font.Height) / 2;

            // 绘制复选框
            if (showCheckBoxes && index < checkBoxBounds.Count)
            {
                var checkRect = checkBoxBounds[index];
                checkRect.Offset(-scrollOffsetX, -scrollOffsetY);
                DrawCheckBox(g, checkRect, item.Checked);
                x = checkRect.Right + checkBoxMargin;
            }

            // 绘制图标和文本
            Color foreColor = isSelected ? SelectedForeColor : (isHovered ? HoverForeColor : ItemForeColor);

            // 绘制前置图标
            if (iconPosition == IconPosition.Left && item.Icon != null)
            {
                Size iconSize = GetIconSize();
                var iconRect = new Rectangle(
                    x,
                    bounds.Y + (bounds.Height - iconSize.Height) / 2,
                    iconSize.Width,
                    iconSize.Height);

                DrawIcon(g, item.Icon, iconRect);
                x = iconRect.Right + iconTextSpacing;
            }

            // 绘制文本
            using (var brush = new SolidBrush(foreColor))
            {
                var textRect = new Rectangle(x, bounds.Y,
                    bounds.Right - x - ItemPadding.Right, bounds.Height);

                if (iconPosition == IconPosition.Right && item.Icon != null)
                {
                    Size iconSize = GetIconSize();
                    textRect.Width -= (iconSize.Width + iconTextSpacing);
                }

                TextRenderer.DrawText(g, item.ToString(), Font, textRect, foreColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                // 绘制后置图标
                if (iconPosition == IconPosition.Right && item.Icon != null)
                {
                    Size iconSize = GetIconSize();
                    var iconRect = new Rectangle(
                        bounds.Right - ItemPadding.Right - iconSize.Width,
                        bounds.Y + (bounds.Height - iconSize.Height) / 2,
                        iconSize.Width,
                        iconSize.Height);

                    DrawIcon(g, item.Icon, iconRect);
                }
            }
        }

        private void DrawIcon(Graphics g, Image icon, Rectangle targetRect)
        {
            if (icon == null || targetRect.Width <= 0 || targetRect.Height <= 0)
            {
                return;
            }

            var oldInterpolation = g.InterpolationMode;
            var oldSmoothingMode = g.SmoothingMode;

            // 设置高质量绘制模式
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            try
            {
                switch (iconSizeMode)
                {
                    case IconSizeMode.None:
                        // 原始大小, 左上角对齐
                        g.DrawImage(icon, targetRect.X, targetRect.Y, icon.Width, icon.Height);
                        break;

                    case IconSizeMode.AutoSize:
                    case IconSizeMode.StretchImage:
                        // 拉伸填充整个区域
                        g.DrawImage(icon, targetRect);
                        break;

                    case IconSizeMode.CenterImage:
                        // 居中显示原始大小
                        int centerX = targetRect.X + (targetRect.Width - icon.Width) / 2;
                        int centerY = targetRect.Y + (targetRect.Height - icon.Height) / 2;
                        g.DrawImage(icon, centerX, centerY, icon.Width, icon.Height);
                        break;

                    case IconSizeMode.Zoom:
                        // 保持比例缩放
                        Rectangle zoomRect = CalculateZoomRect(icon, targetRect);
                        g.DrawImage(icon, zoomRect);
                        break;
                }
            }
            finally
            {
                g.InterpolationMode = oldInterpolation;
                g.SmoothingMode = oldSmoothingMode;
            }
        }

        /// <summary>
        /// 计算保持比例的缩放矩形
        /// </summary>
        private Rectangle CalculateZoomRect(Image image, Rectangle targetRect)
        {
            float targetAspect = (float)targetRect.Width / targetRect.Height;
            float imageAspect = (float)image.Width / image.Height;

            int width, height;

            if (imageAspect > targetAspect)
            {
                // 图像更宽, 以宽度为准
                width = targetRect.Width;
                height = (int)(targetRect.Width / imageAspect);
            }
            else
            {
                // 图像更高, 以高度为准
                height = targetRect.Height;
                width = (int)(targetRect.Height * imageAspect);
            }

            int x = targetRect.X + (targetRect.Width - width) / 2;
            int y = targetRect.Y + (targetRect.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }


        private void DrawCheckBox(Graphics g, Rectangle bounds, bool isChecked)
        {
            Color borderColor = Theme?.Colors?.Border ?? SystemColors.ControlDark;
            Color primaryColor = Theme?.Colors?.Primary ?? SystemColors.Highlight;
            Color checkColor = Theme?.Colors?.TextOnPrimary ?? Color.White;

            // 绘制复选框边框
            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, bounds);
            }

            // 如果选中, 绘制勾选标记
            if (isChecked)
            {
                using (var brush = new SolidBrush(primaryColor))
                {
                    g.FillRectangle(brush, bounds.X + 2, bounds.Y + 2,
                        bounds.Width - 4, bounds.Height - 4);
                }

                // 绘制勾选符号
                using (var pen = new Pen(checkColor, 2))
                {
                    g.DrawLine(pen,
                        bounds.X + 3, bounds.Y + bounds.Height / 2,
                        bounds.X + bounds.Width / 2 - 1, bounds.Y + bounds.Height - 4);
                    g.DrawLine(pen,
                        bounds.X + bounds.Width / 2 - 1, bounds.Y + bounds.Height - 4,
                        bounds.X + bounds.Width - 3, bounds.Y + 3);
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (DesignMode || !isInitialized)
            {
                return;
            }

            bool shouldDrawBorder = false;
            Color currentBorderColor = BorderColor;
            int currentBorderWidth = borderWidth;

            switch (borderStyle)
            {
                case FluentBorderStyle.None:
                    // 不绘制边框
                    return;

                case FluentBorderStyle.FixedSingle:
                    // 始终绘制单线边框
                    shouldDrawBorder = true;
                    break;

                case FluentBorderStyle.Fixed3D:
                    // 绘制3D边框
                    Draw3DBorder(g);
                    return;

                case FluentBorderStyle.FocusOnly:
                    // 仅在聚焦时绘制
                    if (Focused && showFocusBorder)
                    {
                        shouldDrawBorder = true;
                        currentBorderColor = FocusedBorderColor;
                        currentBorderWidth = Math.Max(2, borderWidth); // 聚焦时至少2像素宽
                    }
                    break;
            }

            // 如果需要绘制边框
            if (shouldDrawBorder)
            {
                using (var pen = new Pen(currentBorderColor, currentBorderWidth))
                {
                    int offset = currentBorderWidth / 2;
                    g.DrawRectangle(pen, offset, offset, Width - currentBorderWidth, Height - currentBorderWidth);
                }
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            // 绘制3D效果边框
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // 外边框 - 深色
            using (var darkPen = new Pen(SystemColors.ControlDark))
            {
                g.DrawLine(darkPen, rect.Left, rect.Bottom, rect.Left, rect.Top);
                g.DrawLine(darkPen, rect.Left, rect.Top, rect.Right, rect.Top);
            }

            // 内边框 - 更深色
            using (var darkerPen = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(darkerPen, rect.Left + 1, rect.Bottom - 1, rect.Left + 1, rect.Top + 1);
                g.DrawLine(darkerPen, rect.Left + 1, rect.Top + 1, rect.Right - 1, rect.Top + 1);
            }

            // 外边框 - 亮色
            using (var lightPen = new Pen(SystemColors.ControlLightLight))
            {
                g.DrawLine(lightPen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                g.DrawLine(lightPen, rect.Right, rect.Bottom, rect.Left, rect.Bottom);
            }

            // 内边框 - 较亮色
            using (var lighterPen = new Pen(SystemColors.ControlLight))
            {
                g.DrawLine(lighterPen, rect.Right - 1, rect.Top + 1, rect.Right - 1, rect.Bottom - 1);
                g.DrawLine(lighterPen, rect.Right - 1, rect.Bottom - 1, rect.Left + 1, rect.Bottom - 1);
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                doubleClickTimer?.Dispose();
                vScrollBar?.Dispose();
                hScrollBar?.Dispose();
                scrollCorner?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 列表项

    /// <summary>
    /// 列表项数据
    /// </summary>
    [TypeConverter(typeof(FluentListItemConverter))]
    public class FluentListItem : ICloneable
    {
        private object value;
        private string text;
        private Image icon;
        private int index;
        private bool isChecked;
        private object tag;

        public FluentListItem() : this(null, "Item", 0)
        {
        }

        public FluentListItem(object value, int index = 0) : this(value, value?.ToString() ?? "", index)
        {
        }

        public FluentListItem(object value, string text, int index = 0)
        {
            Value = value;
            Text = text ?? "";
            Index = index;
        }

        [Description("项的显示文本")]
        public string Text
        {
            get => text;
            set
            {
                this.text = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public object Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        [Description("项的图标")]
        public Image Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        [Description("项的索引")]
        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged();
            }
        }

        [Description("是否选中")]
        public bool Checked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public object Tag { get; set; }

        public event EventHandler PropertyChanged;



        protected virtual void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Text) ? Text : (Value?.ToString() ?? string.Empty);
        }

        public object Clone()
        {
            return new FluentListItem(Value, Text, Index)
            {
                Icon = Icon,
                Checked = Checked,
                Tag = Tag
            };
        }
    }

    public class FluentListItemConverter : ExpandableObjectConverter
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
            if (destinationType == typeof(string) && value is FluentListItem item)
            {
                return item.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ItemCheckEventArgs : EventArgs
    {
        public ItemCheckEventArgs(int index, bool isChecked, FluentListItem item)
        {
            Index = index;
            Checked = isChecked;
            Item = item;
        }

        public int Index { get; }

        public bool Checked { get; }

        public FluentListItem Item { get; }
    }

    #endregion

    #region 列表项集合

    /// <summary>
    /// 列表项集合
    /// </summary>
    public class FluentListItemCollection : IList<FluentListItem>, IList
    {
        private readonly List<FluentListItem> items = new List<FluentListItem>();
        private readonly FluentListBox owner;

        public event EventHandler<ListChangedEventArgs> ListChanged;

        public FluentListItemCollection(FluentListBox owner)
        {
            this.owner = owner;
        }

        public FluentListItem this[int index]
        {
            get => items[index];
            set
            {
                if (items[index] != value)
                {
                    if (items[index] != null)
                    {
                        items[index].PropertyChanged -= Item_PropertyChanged;
                    }

                    items[index] = value;

                    if (value != null)
                    {
                        value.PropertyChanged += Item_PropertyChanged;
                    }

                    OnListChanged(ListChangedType.ItemChanged, index);
                }
            }
        }

        private void Item_PropertyChanged(object sender, EventArgs e)
        {
            var index = items.IndexOf(sender as FluentListItem);
            if (index >= 0)
            {
                OnListChanged(ListChangedType.ItemChanged, index);
            }
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;

        public void Add(FluentListItem item)
        {
            if (item != null)
            {
                item.PropertyChanged += Item_PropertyChanged;
                items.Add(item);
                OnListChanged(ListChangedType.ItemAdded, items.Count - 1);
            }
        }

        public void AddRange(IEnumerable<FluentListItem> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public void Clear()
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            items.Clear();
            OnListChanged(ListChangedType.Reset, -1);
        }

        public bool Contains(FluentListItem item) => items.Contains(item);
        public void CopyTo(FluentListItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public IEnumerator<FluentListItem> GetEnumerator() => items.GetEnumerator();
        public int IndexOf(FluentListItem item) => items.IndexOf(item);

        public void Insert(int index, FluentListItem item)
        {
            if (item != null)
            {
                item.PropertyChanged += Item_PropertyChanged;
                items.Insert(index, item);
                OnListChanged(ListChangedType.ItemAdded, index);
            }
        }

        public bool Remove(FluentListItem item)
        {
            int index = items.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                var item = items[index];
                if (item != null)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }

                items.RemoveAt(index);
                OnListChanged(ListChangedType.ItemDeleted, index);
            }
        }

        protected virtual void OnListChanged(ListChangedType type, int index)
        {
            ListChanged?.Invoke(this, new ListChangedEventArgs(type, index));
            owner?.OnItemsChanged();
        }

        #region IList 实现

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value)
        {
            if (value is FluentListItem item)
            {
                Add(item);
                return items.Count - 1;
            }
            else if (value != null)
            {
                var newItem = new FluentListItem(value, items.Count);
                Add(newItem);
                return items.Count - 1;
            }
            return -1;
        }

        bool IList.Contains(object value) => value is FluentListItem item && Contains(item);
        int IList.IndexOf(object value) => value is FluentListItem item ? IndexOf(item) : -1;
        void IList.Insert(int index, object value)
        {
            if (value is FluentListItem item)
            {
                Insert(index, item);
            }
            else if (value != null)
            {
                Insert(index, new FluentListItem(value, index));
            }
        }

        void IList.Remove(object value)
        {
            if (value is FluentListItem item)
            {
                Remove(item);
            }
        }

        bool IList.IsFixedSize => false;

        public object SyncRoot => items;

        public bool IsSynchronized => false;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)items).CopyTo(array, index);
        }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is FluentListItem item)
                {
                    this[index] = item;
                }
                else if (value != null)
                {
                    this[index] = new FluentListItem(value, index);
                }
            }
        }

        #endregion
    }

    public enum ListChangedType
    {
        Reset,
        ItemAdded,
        ItemDeleted,
        ItemChanged
    }

    public class ListChangedEventArgs : EventArgs
    {
        public ListChangedType ListChangedType { get; }
        public int Index { get; }

        public ListChangedEventArgs(ListChangedType type, int index)
        {
            ListChangedType = type;
            Index = index;
        }
    }

    #endregion

    #region 设计器

    public class FluentListBoxDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentListBoxActionList(Component));
                }
                return actionLists;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 隐藏一些不需要的属性
            string[] hiddenProperties = { "BackgroundImage", "BackgroundImageLayout", "ForeColor", "BackColor", "Font" };
            foreach (string prop in hiddenProperties)
            {
                if (properties.Contains(prop))
                {
                    properties[prop] = TypeDescriptor.CreateProperty(
                        typeof(FluentListBoxDesigner),
                        (PropertyDescriptor)properties[prop],
                        new BrowsableAttribute(false));
                }
            }
        }
    }

    /// <summary>
    /// FluentListBox 设计器操作列表
    /// </summary>
    public class FluentListBoxActionList : DesignerActionList
    {
        private FluentListBox listBox;
        private DesignerActionUIService designerService;

        public FluentListBoxActionList(IComponent component) : base(component)
        {
            listBox = component as FluentListBox;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }


        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();

            // 项管理
            items.Add(new DesignerActionHeaderItem("项管理"));
            items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑项...", "项管理", "打开项编辑器", true));
            items.Add(new DesignerActionMethodItem(this, "AddSampleItems", "添加样本数据", "项管理", "添加样本数据", true));
            items.Add(new DesignerActionMethodItem(this, "ClearItems", "清空项", "项管理", "清除所有项", true));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ItemHeight", "项高度", "外观", "设置项高度"));
            items.Add(new DesignerActionPropertyItem("ItemSpacing", "项间距", "外观", "设置项间距"));
            items.Add(new DesignerActionPropertyItem("BorderStyle", "Border样式", "外观", "设置边框样式"));

            // 行为
            items.Add(new DesignerActionHeaderItem("行为"));
            items.Add(new DesignerActionPropertyItem("ShowCheckBoxes", "复选框", "行为", "显示复选框"));
            items.Add(new DesignerActionPropertyItem("MultiSelect", "多选", "行为", "允许多选"));
            items.Add(new DesignerActionPropertyItem("ScrollBarMode", "滚动条模式", "行为", "滚动条显示模式"));

            return items;
        }

        public void EditItems()
        {
            // 获取设计器宿主
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                // 创建设计器事务
                DesignerTransaction transaction = host.CreateTransaction("编辑项目集合");
                try
                {
                    // 获取属性描述符
                    PropertyDescriptor pd = TypeDescriptor.GetProperties(listBox)["Items"];
                    if (pd != null)
                    {
                        // 获取 UITypeEditor
                        UITypeEditor editor = pd.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                        if (editor != null)
                        {
                            // 创建类型描述符上下文
                            TypeDescriptorContext context = new TypeDescriptorContext(listBox, pd, host);

                            // 编辑值
                            object newValue = editor.EditValue(context, context, listBox.Items);

                            if (newValue != null)
                            {
                                pd.SetValue(listBox, newValue);
                            }
                        }
                    }

                    transaction.Commit();

                    if (designerService != null)
                    {
                        designerService.Refresh(listBox);
                    }
                }
                catch
                {
                    transaction.Cancel();
                    throw;
                }
            }
        }

        public void AddSampleItems()
        {
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                using (DesignerTransaction transaction = host.CreateTransaction("添加样本数据"))
                {
                    try
                    {
                        listBox.Items.Clear();
                        listBox.Items.Add(new FluentListItem(1, "Item1", 0));
                        listBox.Items.Add(new FluentListItem(2, "Item2", 1));
                        listBox.Items.Add(new FluentListItem(3, "Item3", 2));

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

        public void ClearItems()
        {
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                using (DesignerTransaction transaction = host.CreateTransaction("清空项"))
                {
                    try
                    {
                        listBox.Items.Clear();
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

        public bool ShowCheckBoxes
        {
            get => listBox.ShowCheckBoxes;
            set => SetProperty("ShowCheckBoxes", value);
        }

        public bool MultiSelect
        {
            get => listBox.MultiSelect;
            set => SetProperty("MultiSelect", value);
        }

        public FluentBorderStyle BorderStyle
        {
            get => listBox.BorderStyle;
            set => SetProperty("BorderStyle", value);
        }

        public ScrollBarMode ScrollBarMode
        {
            get => listBox.ScrollBarMode;
            set => SetProperty("ScrollBarMode", value);
        }

        public int ItemHeight
        {
            get => listBox.ItemHeight;
            set => SetProperty("ItemHeight", value);
        }

        public int ItemSpacing
        {
            get => listBox.ItemSpacing;
            set => SetProperty("ItemSpacing", value);
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(listBox)[propertyName];
            property.SetValue(listBox, value);
        }
    }


    internal class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
    {
        private IComponent component;
        private IDesignerHost host;

        private PropertyDescriptor propertyDescriptor;


        public TypeDescriptorContext(IComponent component, PropertyDescriptor propertyDescriptor, IDesignerHost host)
        {
            this.component = component;
            this.propertyDescriptor = propertyDescriptor;
            this.host = host;
        }

        public IContainer Container
        {
            get
            {
                if (component != null)
                {
                    return component.Site.Container;
                }

                return null;
            }
        }

        public object Instance => component;

        public PropertyDescriptor PropertyDescriptor => propertyDescriptor;

        public void OnComponentChanged()
        {
            if (host != null)
            {
                IComponentChangeService changeService = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                changeService?.OnComponentChanged(component, propertyDescriptor, null, null);
            }
        }

        public bool OnComponentChanging()
        {
            if (host != null)
            {
                IComponentChangeService changeService = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                try
                {
                    changeService?.OnComponentChanging(component, propertyDescriptor);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IWindowsFormsEditorService))
            {
                return new WindowsFormsEditorService();
            }

            if (host != null)
            {
                return host.GetService(serviceType);
            }
            return null;
        }
    }

    internal class WindowsFormsEditorService : IWindowsFormsEditorService
    {
        private Form dropDownForm;

        public void CloseDropDown()
        {
            if (dropDownForm != null)
            {
                dropDownForm.Close();
                dropDownForm = null;
            }
        }

        public void DropDownControl(Control control)
        {
            dropDownForm = new Form();
            dropDownForm.FormBorderStyle = FormBorderStyle.None;
            dropDownForm.StartPosition = FormStartPosition.Manual;
            dropDownForm.ShowInTaskbar = false;
            dropDownForm.Controls.Add(control);
            dropDownForm.Size = control.Size;

            Point location = Cursor.Position;
            Rectangle screen = Screen.FromPoint(location).WorkingArea;

            if (location.X + dropDownForm.Width > screen.Right)
            {
                location.X = screen.Right - dropDownForm.Width;
            }

            if (location.Y + dropDownForm.Height > screen.Bottom)
            {
                location.Y = location.Y - dropDownForm.Height;
            }

            dropDownForm.Location = location;
            dropDownForm.ShowDialog();
        }

        public DialogResult ShowDialog(Form dialog)
        {
            return dialog.ShowDialog();
        }
    }


    #endregion

    /// <summary>
    /// 滚动条模式
    /// </summary>
    public enum ScrollBarMode
    {
        Auto,
        Vertical,
        Horizontal,
        Both,
        None
    }


    /// <summary>
    /// 边框样式枚举
    /// </summary>
    public enum FluentBorderStyle
    {
        None,           // 无边框
        FixedSingle,    // 单线边框
        Fixed3D,        // 3D边框
        FocusOnly       // 仅聚焦时显示
    }


    /// <summary>
    /// 图标缩放模式
    /// </summary>
    public enum IconSizeMode
    {
        None,           // 不缩放, 原始大小
        AutoSize,       // 自动适应(保持比例)
        CenterImage,    // 居中显示原始大小
        StretchImage,   // 拉伸填充(可能变形)
        Zoom           // 缩放填充(保持比例, 可能有空白)
    }

}
