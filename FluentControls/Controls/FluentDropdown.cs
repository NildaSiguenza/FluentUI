using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Collections;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{
    [DefaultEvent("SelectedItemChanged")]
    [Designer(typeof(FluentDropdownDesigner))]
    public class FluentDropdown : FluentControlBase
    {
        #region 字段

        private bool isDroppedDown = false;
        private FluentDropdownList dropdownList;
        private DropdownItemCollection items;
        private DropdownItem selectedItem;
        private List<DropdownItem> selectedItems = new List<DropdownItem>();
        private ImageList dropdownIconList;
        private Rectangle dropdownButtonBounds;
        private bool mouseOverButton = false;
        private SelectionMode selectionMode = SelectionMode.Single;

        // 外观设置
        private Image displayIcon;
        private string displayText = "请选择";
        private DropdownIconPosition iconPosition = DropdownIconPosition.Left;
        private bool showIcon = true;
        private bool showText = true;

        // Fluent 样式
        private int cornerRadius = 4;
        private Color borderColor = Color.FromArgb(200, 200, 200);
        private Color borderHoverColor = Color.FromArgb(100, 100, 100);
        private Color borderFocusColor = Color.FromArgb(0, 120, 212);
        private Color dropdownButtonColor = Color.FromArgb(150, 150, 150);
        private Color dropdownButtonHoverColor = Color.FromArgb(0, 120, 212);

        // 动画
        private Timer expandTimer;
        private float expandProgress = 0f;
        private bool isExpanding = false;

        #endregion

        #region 构造函数

        public FluentDropdown()
        {
            this.Size = new Size(200, 36);
            this.Cursor = Cursors.Hand;

            items = new DropdownItemCollection();
            items.CollectionChanged += (s, e) =>
            {
                if (dropdownList != null)
                {
                    dropdownList.UpdateItems(items.ToList());
                }
            };

            InitializeDropdownList();
            InitializeAnimation();
        }

        private void InitializeDropdownList()
        {
            dropdownList = new FluentDropdownList(this)
            {
                Visible = false,
                SelectionMode = selectionMode
            };

            dropdownList.ItemClicked += OnDropdownItemClicked;
        }

        private void InitializeAnimation()
        {
            expandTimer = new Timer { Interval = 16 };
            expandTimer.Tick += ExpandTimer_Tick;
        }

        #endregion

        #region 属性

        [Category("Fluent")]
        [Description("圆角半径")]
        [DefaultValue(4)]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("边框悬停颜色")]
        public Color BorderHoverColor
        {
            get => borderHoverColor;
            set
            {
                borderHoverColor = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("边框焦点颜色")]
        public Color BorderFocusColor
        {
            get => borderFocusColor;
            set
            {
                borderFocusColor = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("选择模式(单选或多选)")]
        [DefaultValue(SelectionMode.Single)]
        public SelectionMode SelectionMode
        {
            get => selectionMode;
            set
            {
                if (selectionMode != value)
                {
                    selectionMode = value;

                    if (value == SelectionMode.Single)
                    {
                        ClearMultipleSelection();
                    }

                    if (dropdownList != null)
                    {
                        dropdownList.SelectionMode = value;
                    }

                    OnSelectionModeChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [Description("显示的图标")]
        public Image DisplayIcon
        {
            get => displayIcon;
            set
            {
                displayIcon = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("显示的文本")]
        public string DisplayText
        {
            get => displayText;
            set
            {
                displayText = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("图标位置")]
        [DefaultValue(DropdownIconPosition.Left)]
        public DropdownIconPosition IconPosition
        {
            get => iconPosition;
            set
            {
                iconPosition = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("是否显示图标")]
        [DefaultValue(true)]
        public bool ShowIcon
        {
            get => showIcon;
            set
            {
                showIcon = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("是否显示文本")]
        [DefaultValue(true)]
        public bool ShowText
        {
            get => showText;
            set
            {
                showText = value;
                Invalidate();
            }
        }

        [Category("Dropdown")]
        [Description("下拉列表项高度")]
        [DefaultValue(36)]
        public int ItemHeight
        {
            get => dropdownList.ItemHeight;
            set => dropdownList.ItemHeight = value;
        }

        [Category("Dropdown")]
        [Description("下拉列表项间距")]
        [DefaultValue(2)]
        public int ItemSpacing
        {
            get => dropdownList.ItemSpacing;
            set => dropdownList.ItemSpacing = value;
        }

        [Category("Dropdown")]
        [Description("下拉列表最大高度")]
        [DefaultValue(300)]
        public int DropdownMaxHeight
        {
            get => dropdownList.MaxHeight;
            set => dropdownList.MaxHeight = value;
        }

        [Browsable(false)]
        public DropdownItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;

                    if (selectionMode == SelectionMode.Single)
                    {
                        UpdateDisplay();
                        OnSelectedItemChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [Category("Dropdown")]
        [Description("下拉列表图标列表")]
        public ImageList IconList
        {
            get => dropdownIconList;
            set
            {
                dropdownIconList = value;
                if (dropdownList != null)
                {
                    dropdownList.UpdateItems(items.ToList());
                }
                Invalidate();
            }
        }

        [Browsable(false)]
        public List<DropdownItem> SelectedItems => new List<DropdownItem>(selectedItems);

        [Category("Appearance")]
        [Description("多选模式下显示的文本格式")]
        [DefaultValue("(已选{0}项)")]
        public string MultiSelectTextFormat { get; set; } = "(已选{0}项)";

        [Category("Data")]
        [Description("下拉列表项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DropdownItemCollection Items => items;

        #endregion

        #region 事件

        public event EventHandler SelectedItemChanged;
        public event EventHandler SelectedItemsChanged;
        public event EventHandler SelectionModeChanged;
        public event EventHandler DropdownOpened;
        public event EventHandler DropdownClosed;

        protected virtual void OnSelectedItemChanged(EventArgs e)
        {
            SelectedItemChanged?.Invoke(this, e);
        }

        protected virtual void OnSelectedItemsChanged(EventArgs e)
        {
            SelectedItemsChanged?.Invoke(this, e);
        }

        protected virtual void OnSelectionModeChanged(EventArgs e)
        {
            SelectionModeChanged?.Invoke(this, e);
        }

        protected virtual void OnDropdownOpened(EventArgs e)
        {
            DropdownOpened?.Invoke(this, e);
        }

        protected virtual void OnDropdownClosed(EventArgs e)
        {
            DropdownClosed?.Invoke(this, e);
        }

        #endregion

        #region 公共方法

        public void SelectItem(DropdownItem item, bool selected)
        {
            if (item == null || item.ItemType != DropdownItemType.Normal)
            {
                return;
            }

            if (selectionMode == SelectionMode.Single)
            {
                if (selected)
                {
                    foreach (var i in items.Where(x => x.ItemType == DropdownItemType.Normal))
                    {
                        i.Selected = false;
                    }
                    item.Selected = true;
                    SelectedItem = item;
                }
            }
            else
            {
                item.Selected = selected;

                if (selected && !selectedItems.Contains(item))
                {
                    selectedItems.Add(item);
                }
                else if (!selected && selectedItems.Contains(item))
                {
                    selectedItems.Remove(item);
                }

                OnSelectedItemsChanged(EventArgs.Empty);
            }
        }

        public void ClearSelection()
        {
            if (selectionMode == SelectionMode.Single)
            {
                selectedItem = null;
                foreach (var item in items)
                {
                    item.Selected = false;
                }
                UpdateDisplay();
            }
            else
            {
                foreach (var item in selectedItems)
                {
                    item.Selected = false;
                }
                selectedItems.Clear();
                OnSelectedItemsChanged(EventArgs.Empty);
            }
            Invalidate();
        }

        public int GetSelectedCount()
        {
            if (selectionMode == SelectionMode.Single)
            {
                return selectedItem != null ? 1 : 0;
            }
            else
            {
                return items?.Count(t => t.Selected) ?? 0;
            }
        }

        public void AddItem(DropdownItem item)
        {
            items.Add(item);
        }

        public void AddItem(string text, Image icon = null)
        {
            items.Add(new DropdownItem(text, icon));
        }

        public void AddSeparator()
        {
            items.Add(new DropdownItem("") { ItemType = DropdownItemType.Separator });
        }

        public void AddButton(string text, Image icon, Action<DropdownItem> onClick)
        {
            items.Add(new DropdownItem(text, icon)
            {
                ItemType = DropdownItemType.Button,
                OnClick = onClick
            });
        }

        public void RemoveItem(DropdownItem item)
        {
            items.Remove(item);
        }

        public void ClearItems()
        {
            items.Clear();
            selectedItem = null;
            UpdateDisplay();
        }

        #endregion

        #region 内部方法

        internal Image GetItemIcon(DropdownItem item)
        {
            if (item.Icon != null)
            {
                return item.Icon;
            }

            if (dropdownIconList != null && item.IconIndex >= 0 && item.IconIndex < dropdownIconList.Images.Count)
            {
                return dropdownIconList.Images[item.IconIndex];
            }

            return null;
        }

        private void ClearMultipleSelection()
        {
            foreach (var item in selectedItems)
            {
                item.Selected = false;
            }
            selectedItems.Clear();

            if (selectedItem != null)
            {
                selectedItem.Selected = true;
            }
        }

        private void UpdateDisplay()
        {
            if (selectionMode == SelectionMode.Single && selectedItem != null)
            {
                displayText = selectedItem.Text;
                displayIcon = GetItemIcon(selectedItem);
            }
            Invalidate();
        }

        private void ShowDropdown()
        {
            if (isDroppedDown || items.Count == 0)
            {
                return;
            }

            isDroppedDown = true;
            isExpanding = true;
            expandProgress = 0f;

            Point location = this.Parent.PointToScreen(new Point(this.Left, this.Bottom + 2));
            dropdownList.Location = location;
            dropdownList.Width = this.Width;

            // 应用主题到下拉列表
            if (UseTheme && Theme != null)
            {
                dropdownList.ApplyTheme(Theme);
            }

            dropdownList.Show();
            dropdownList.BringToFront();

            if (EnableAnimation)
            {
                expandTimer.Start();
            }

            OnDropdownOpened(EventArgs.Empty);
            Invalidate();
        }

        private void HideDropdown()
        {
            if (!isDroppedDown)
            {
                return;
            }

            isDroppedDown = false;
            isExpanding = false;
            expandTimer.Stop();
            dropdownList.Hide();

            OnDropdownClosed(EventArgs.Empty);
            Invalidate();
        }

        private void OnDropdownItemClicked(object sender, DropdownItem item)
        {
            if (item.ItemType == DropdownItemType.Normal)
            {
                if (selectionMode == SelectionMode.Single)
                {
                    foreach (var i in items.Where(x => x.ItemType == DropdownItemType.Normal))
                    {
                        i.Selected = false;
                    }
                    item.Selected = true;
                    SelectedItem = item;
                    HideDropdown();
                }
                else
                {
                    SelectItem(item, !item.Selected);
                }

                item.OnClick?.Invoke(item);
            }
            else if (item.ItemType == DropdownItemType.Button)
            {
                item.OnClick?.Invoke(item);

                if (selectionMode == SelectionMode.Single)
                {
                    HideDropdown();
                }
            }
            Invalidate();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var rect = this.ClientRectangle;

            Color bgColor = this.BackColor;
            if (UseTheme && Theme != null)
            {
                bgColor = Theme.Colors.Surface;

                if (State == ControlState.Hover)
                {
                    bgColor = Theme.Colors.SurfaceHover;
                }
                else if (State == ControlState.Pressed || isDroppedDown)
                {
                    bgColor = Theme.Colors.SurfacePressed;
                }
            }

            using (var brush = new SolidBrush(bgColor))
            using (var path = GetRoundedRectangle(rect, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            int buttonWidth = 32;
            dropdownButtonBounds = new Rectangle(
                this.Width - buttonWidth,
                0,
                buttonWidth,
                this.Height
            );

            // 绘制下拉按钮背景
            if (mouseOverButton)
            {
                Color hoverColor = UseTheme && Theme != null
                    ? Theme.Colors.GetColorWithOpacity(Theme.Colors.Primary, 0.1f)
                    : Color.FromArgb(20, dropdownButtonHoverColor);

                using (var brush = new SolidBrush(hoverColor))
                {
                    var buttonRect = dropdownButtonBounds;
                    buttonRect.Inflate(-2, -2);
                    using (var path = GetRoundedRectangle(buttonRect, cornerRadius - 1))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            // 绘制下拉箭头
            Color arrowColor = mouseOverButton ? dropdownButtonHoverColor : dropdownButtonColor;
            if (UseTheme && Theme != null)
            {
                arrowColor = mouseOverButton ? Theme.Colors.Primary : Theme.Colors.TextSecondary;
            }
            DrawDropdownArrow(g, dropdownButtonBounds, arrowColor, isDroppedDown);

            // 绘制内容区域
            Rectangle contentBounds = new Rectangle(
                cornerRadius + 4,
                0,
                this.Width - buttonWidth - cornerRadius - 8,
                this.Height
            );

            Color textColor = this.ForeColor;
            if (UseTheme && Theme != null)
            {
                textColor = Theme.Colors.TextPrimary;
            }

            // 多选模式显示
            if (selectionMode == SelectionMode.Multiple)
            {
                int selectedCount = GetSelectedCount();
                string text = selectedCount > 0
                    ? $"{DisplayText} {string.Format(MultiSelectTextFormat, selectedCount)}"
                    : DisplayText;

                TextRenderer.DrawText(g, text, this.Font, contentBounds, textColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            else
            {
                // 单选模式显示
                if (showIcon && displayIcon != null && showText)
                {
                    if (iconPosition == DropdownIconPosition.Left)
                    {
                        Rectangle iconBounds = new Rectangle(
                            contentBounds.Left,
                            contentBounds.Top + (contentBounds.Height - 20) / 2,
                            20, 20
                        );
                        g.DrawImage(displayIcon, iconBounds);

                        Rectangle textBounds = new Rectangle(
                            iconBounds.Right + 8,
                            contentBounds.Top,
                            contentBounds.Width - 28,
                            contentBounds.Height
                        );
                        TextRenderer.DrawText(g, displayText, this.Font, textBounds, textColor,
                            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    }
                    else
                    {
                        // 图标在上
                        int iconSize = 20;
                        Rectangle iconBounds = new Rectangle(
                            contentBounds.Left + (contentBounds.Width - iconSize) / 2,
                            contentBounds.Top + 4,
                            iconSize, iconSize
                        );
                        g.DrawImage(displayIcon, iconBounds);

                        Rectangle textBounds = new Rectangle(
                            contentBounds.Left,
                            iconBounds.Bottom + 2,
                            contentBounds.Width,
                            contentBounds.Height - iconSize - 6
                        );
                        TextRenderer.DrawText(g, displayText, this.Font, textBounds, textColor,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
                    }
                }
                else if (showIcon && displayIcon != null)
                {
                    Rectangle iconBounds = new Rectangle(
                        contentBounds.Left + (contentBounds.Width - 20) / 2,
                        contentBounds.Top + (contentBounds.Height - 20) / 2,
                        20, 20
                    );
                    g.DrawImage(displayIcon, iconBounds);
                }
                else if (showText)
                {
                    TextRenderer.DrawText(g, displayText, this.Font, contentBounds, textColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            var rect = this.ClientRectangle;
            rect.Width--;
            rect.Height--;

            Color currentBorderColor = borderColor;

            if (UseTheme && Theme != null)
            {
                if (State == ControlState.Focused || isDroppedDown)
                {
                    currentBorderColor = Theme.Colors.BorderFocused;
                }
                else if (State == ControlState.Hover)
                {
                    currentBorderColor = Theme.Colors.Primary;
                }
                else
                {
                    currentBorderColor = Theme.Colors.Border;
                }
            }
            else
            {
                if (State == ControlState.Focused || isDroppedDown)
                {
                    currentBorderColor = borderFocusColor;
                }
                else if (State == ControlState.Hover)
                {
                    currentBorderColor = borderHoverColor;
                }
            }

            using (var pen = new Pen(currentBorderColor, 1))
            using (var path = GetRoundedRectangle(rect, cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawDropdownArrow(Graphics g, Rectangle bounds, Color color, bool isUp)
        {
            int centerX = bounds.Left + bounds.Width / 2;
            int centerY = bounds.Top + bounds.Height / 2;

            int arrowWidth = 10;
            int arrowHeight = 5;

            using (var path = new GraphicsPath())
            {
                if (isUp)
                {
                    path.AddLine(
                        centerX - arrowWidth / 2, centerY + arrowHeight / 2,
                        centerX, centerY - arrowHeight / 2
                    );
                    path.AddLine(
                        centerX, centerY - arrowHeight / 2,
                        centerX + arrowWidth / 2, centerY + arrowHeight / 2
                    );
                }
                else
                {
                    path.AddLine(
                        centerX - arrowWidth / 2, centerY - arrowHeight / 2,
                        centerX, centerY + arrowHeight / 2
                    );
                    path.AddLine(
                        centerX, centerY + arrowHeight / 2,
                        centerX + arrowWidth / 2, centerY - arrowHeight / 2
                    );
                }

                using (var pen = new Pen(color, 2))
                {
                    pen.LineJoin = LineJoin.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawPath(pen, path);
                }
            }
        }

        #endregion

        #region 主题

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                this.BackColor = Theme.Colors.Surface;
                this.ForeColor = Theme.Colors.TextPrimary;
                this.Font = Theme.Typography.Body;

                borderColor = Theme.Colors.Border;
                borderHoverColor = Theme.Colors.Primary;
                borderFocusColor = Theme.Colors.BorderFocused;

                Invalidate();
            }
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool wasOverButton = mouseOverButton;
            mouseOverButton = dropdownButtonBounds.Contains(e.Location);

            if (wasOverButton != mouseOverButton)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (mouseOverButton)
            {
                mouseOverButton = false;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                if (isDroppedDown)
                {
                    HideDropdown();
                }
                else
                {
                    ShowDropdown();
                }
            }
        }

        #endregion

        #region 动画

        private void ExpandTimer_Tick(object sender, EventArgs e)
        {
            if (isExpanding)
            {
                expandProgress += 0.1f;
                if (expandProgress >= 1f)
                {
                    expandProgress = 1f;
                    expandTimer.Stop();
                }
            }
            else
            {
                expandProgress -= 0.1f;
                if (expandProgress <= 0f)
                {
                    expandProgress = 0f;
                    expandTimer.Stop();
                }
            }

            if (dropdownList != null)
            {
                dropdownList.SetExpandProgress(expandProgress);
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                expandTimer?.Dispose();
                dropdownList?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 内部类 - FluentDropdownList

        private class FluentDropdownList : Form
        {
            private FluentDropdown owner;
            private List<DropdownItem> items = new List<DropdownItem>();
            private VScrollBar scrollBar;

            private int itemHeight = 36;
            private int itemSpacing = 2;
            private int maxHeight = 300;
            private int hoveredIndex = -1;
            private int cornerRadius = 4;

            private SelectionMode selectionMode = SelectionMode.Single;
            private ToolTip toolTip;

            private float expandProgress = 1f;

            // 主题颜色
            private Color surfaceColor = Color.White;
            private Color selectedBackColor = Color.FromArgb(230, 240, 255);
            private Color selectedForeColor = Color.Black;
            private Color hoverBackColor = Color.FromArgb(245, 245, 245);
            private Color borderColor = Color.FromArgb(200, 200, 200);
            private Color textColor = Color.Black;

            public event EventHandler<DropdownItem> ItemClicked;

            public FluentDropdownList(FluentDropdown owner)
            {
                this.owner = owner;

                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.BackColor = surfaceColor;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);

                scrollBar = new VScrollBar
                {
                    Dock = DockStyle.Right,
                    Visible = false
                };
                scrollBar.Scroll += (s, e) => Invalidate();
                this.Controls.Add(scrollBar);

                toolTip = new ToolTip
                {
                    AutoPopDelay = 5000,
                    InitialDelay = 500,
                    ReshowDelay = 100,
                    ShowAlways = true
                };

                this.Deactivate += (s, e) => owner.HideDropdown();
            }

            public SelectionMode SelectionMode
            {
                get => selectionMode;
                set => selectionMode = value;
            }

            public int ItemHeight
            {
                get => itemHeight;
                set
                {
                    itemHeight = value;
                    UpdateSize();
                }
            }

            public int ItemSpacing
            {
                get => itemSpacing;
                set
                {
                    itemSpacing = value;
                    UpdateSize();
                }
            }

            public int MaxHeight
            {
                get => maxHeight;
                set
                {
                    maxHeight = value;
                    UpdateSize();
                }
            }

            public void ApplyTheme(IFluentTheme theme)
            {
                if (theme == null) return;

                surfaceColor = theme.Colors.Surface;
                selectedBackColor = theme.Colors.GetColorWithOpacity(theme.Colors.Primary, 0.2f);
                selectedForeColor = theme.Colors.TextPrimary;
                hoverBackColor = theme.Colors.SurfaceHover;
                borderColor = theme.Colors.Border;
                textColor = theme.Colors.TextPrimary;
                cornerRadius = theme.Elevation.CornerRadiusSmall;

                this.BackColor = surfaceColor;
                Invalidate();
            }

            public void SetExpandProgress(float progress)
            {
                expandProgress = Math.Max(0f, Math.Min(1f, progress));
                Invalidate();
            }

            public void UpdateItems(List<DropdownItem> newItems)
            {
                this.items = newItems;
                UpdateSize();
                Invalidate();
            }

            private void UpdateSize()
            {
                if (items.Count == 0)
                {
                    this.Height = itemHeight;
                    return;
                }

                int totalHeight = 0;
                foreach (var item in items)
                {
                    if (item.ItemType == DropdownItemType.Separator)
                    {
                        totalHeight += 9 + itemSpacing;
                    }
                    else
                    {
                        totalHeight += itemHeight + itemSpacing;
                    }
                }
                totalHeight -= itemSpacing;

                this.Height = Math.Min(totalHeight + 4, maxHeight);

                scrollBar.Visible = totalHeight > maxHeight - 4;
                if (scrollBar.Visible)
                {
                    scrollBar.Maximum = totalHeight - this.ClientSize.Height + scrollBar.LargeChange;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // 绘制背景
                var bgRect = this.ClientRectangle;
                using (var brush = new SolidBrush(surfaceColor))
                using (var path = GetRoundedRectPath(bgRect, cornerRadius))
                {
                    g.FillPath(brush, path);
                }

                // 绘制边框
                using (var pen = new Pen(borderColor, 1))
                using (var path = GetRoundedRectPath(bgRect, cornerRadius))
                {
                    var borderRect = bgRect;
                    borderRect.Width--;
                    borderRect.Height--;
                    g.DrawPath(pen, path);
                }

                // 绘制阴影
                DrawShadow(g);

                // 设置裁剪区域
                Rectangle clientRect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);
                if (scrollBar.Visible)
                {
                    clientRect.Width -= scrollBar.Width;
                }

                g.SetClip(clientRect);

                // 绘制项目
                int y = 2 - (scrollBar.Visible ? scrollBar.Value : 0);
                int index = 0;

                foreach (var item in items)
                {
                    if (item.ItemType == DropdownItemType.Separator)
                    {
                        DrawSeparator(g, y, clientRect.Width);
                        y += 9 + itemSpacing;
                    }
                    else
                    {
                        Rectangle itemBounds = new Rectangle(2, y, clientRect.Width - 4, itemHeight);

                        if (itemBounds.Bottom > 0 && itemBounds.Top < this.Height)
                        {
                            DrawItem(g, item, itemBounds, index == hoveredIndex);
                        }

                        y += itemHeight + itemSpacing;
                        index++;
                    }
                }
            }

            private void DrawSeparator(Graphics g, int y, int width)
            {
                int sepY = y + 4;
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    g.DrawLine(pen, 10, sepY, width - 10, sepY);
                }
            }

            private void DrawItem(Graphics g, DropdownItem item, Rectangle bounds, bool isHovered)
            {
                // 绘制背景
                if (item.Selected && item.ItemType == DropdownItemType.Normal)
                {
                    using (var brush = new SolidBrush(selectedBackColor))
                    using (var path = GetRoundedRectPath(bounds, cornerRadius - 1))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else if (isHovered)
                {
                    using (var brush = new SolidBrush(hoverBackColor))
                    using (var path = GetRoundedRectPath(bounds, cornerRadius - 1))
                    {
                        g.FillPath(brush, path);
                    }
                }

                int contentX = bounds.Left + 12;
                int checkMarkWidth = 24;

                // 绘制图标
                Image itemIcon = owner.GetItemIcon(item);
                if (itemIcon != null)
                {
                    Rectangle iconBounds = new Rectangle(
                        contentX,
                        bounds.Top + (itemHeight - 20) / 2,
                        20, 20
                    );
                    g.DrawImage(itemIcon, iconBounds);
                    contentX += 24;
                }

                // 计算文本区域
                int textWidth = bounds.Width - contentX - 8;
                if (item.ItemType == DropdownItemType.Normal)
                {
                    textWidth -= checkMarkWidth;
                }

                Rectangle textBounds = new Rectangle(contentX, bounds.Top, textWidth, itemHeight);
                Color itemTextColor = item.Selected && item.ItemType == DropdownItemType.Normal
                    ? selectedForeColor
                    : textColor;

                if (item.ItemType == DropdownItemType.Button)
                {
                    // 按钮样式
                    using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                    {
                        var buttonRect = bounds;
                        buttonRect.Inflate(-4, -4);
                        using (var path = GetRoundedRectPath(buttonRect, cornerRadius - 1))
                        {
                            g.DrawPath(pen, path);
                        }
                    }

                    TextRenderer.DrawText(g, item.Text, owner.Font, textBounds, itemTextColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                else
                {
                    TextRenderer.DrawText(g, item.Text, owner.Font, textBounds, itemTextColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                // 绘制选中标记
                if (item.Selected && item.ItemType == DropdownItemType.Normal)
                {
                    using (var font = new Font("Segoe UI Symbol", 12, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.FromArgb(0, 150, 136)))
                    {
                        int checkX = bounds.Right - checkMarkWidth;
                        int checkY = bounds.Top + (itemHeight - 16) / 2;
                        g.DrawString("✓", font, brush, checkX, checkY);
                    }
                }
            }

            private void DrawShadow(Graphics g)
            {
                // 简单的阴影效果
                var shadowRect = this.ClientRectangle;
                shadowRect.Inflate(1, 1);

                using (var path = GetRoundedRectPath(shadowRect, cornerRadius + 1))
                using (var brush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillPath(brush, path);
                }
            }

            private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                if (radius <= 0)
                {
                    path.AddRectangle(rect);
                    return path;
                }

                int diameter = radius * 2;
                var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();

                return path;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                int y = 2 - (scrollBar.Visible ? scrollBar.Value : 0);
                int newHoveredIndex = -1;
                int index = 0;
                DropdownItem hoveredItem = null;

                foreach (var item in items)
                {
                    if (item.ItemType == DropdownItemType.Separator)
                    {
                        y += 9 + itemSpacing;
                    }
                    else
                    {
                        Rectangle itemBounds = new Rectangle(2, y, this.Width - 4, itemHeight);
                        if (itemBounds.Contains(e.Location))
                        {
                            newHoveredIndex = index;
                            hoveredItem = item;
                            break;
                        }
                        y += itemHeight + itemSpacing;
                        index++;
                    }
                }

                if (newHoveredIndex != hoveredIndex)
                {
                    hoveredIndex = newHoveredIndex;
                    Invalidate();

                    if (hoveredItem != null && hoveredItem.ItemType != DropdownItemType.Separator)
                    {
                        string tooltipText = string.IsNullOrEmpty(hoveredItem.ToolTipText)
                            ? hoveredItem.Text
                            : hoveredItem.ToolTipText;
                        toolTip.SetToolTip(this, tooltipText);
                    }
                    else
                    {
                        toolTip.SetToolTip(this, string.Empty);
                    }
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

                toolTip.SetToolTip(this, string.Empty);
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);

                if (e.Button == MouseButtons.Left)
                {
                    int y = 2 - (scrollBar.Visible ? scrollBar.Value : 0);

                    foreach (var item in items)
                    {
                        if (item.ItemType == DropdownItemType.Separator)
                        {
                            y += 9 + itemSpacing;
                        }
                        else
                        {
                            Rectangle itemBounds = new Rectangle(2, y, this.Width - 4, itemHeight);
                            if (itemBounds.Contains(e.Location))
                            {
                                ItemClicked?.Invoke(this, item);
                                break;
                            }
                            y += itemHeight + itemSpacing;
                        }
                    }
                }
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);

                if (scrollBar.Visible)
                {
                    int newValue = scrollBar.Value - (e.Delta / 120) * itemHeight;
                    scrollBar.Value = Math.Max(0, Math.Min(scrollBar.Maximum - scrollBar.LargeChange + 1, newValue));
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    toolTip?.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        #endregion
    }

    #region 下拉列表项

    /// <summary>
    /// 下拉列表项
    /// </summary>
    [TypeConverter(typeof(DropdownItemTypeConverter))]
    public class DropdownItem
    {
        private int index = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        public DropdownItem()
        {
            Text = "新项目";
            IconIndex = -1;
        }

        public DropdownItem(string text, Image icon = null)
        {
            Text = text;
            Icon = icon;
            IconIndex = -1;
        }

        public DropdownItem(string text, int iconIndex)
        {
            Text = text;
            Icon = null;
            IconIndex = iconIndex;
        }

        [Category("外观")]
        [Description("显示的文本")]
        public string Text { get; set; }

        [Category("外观")]
        [Description("显示的图标")]
        public Image Icon { get; set; }

        [Category("外观")]
        [Description("图标索引")]
        public int IconIndex { get; set; } = -1;

        [Category("行为")]
        [Description("项目类型")]
        [DefaultValue(DropdownItemType.Normal)]
        public DropdownItemType ItemType { get; set; } = DropdownItemType.Normal;

        [Category("行为")]
        [Description("是否选中")]
        [DefaultValue(false)]
        public bool Selected { get; set; }

        [Category("外观")]
        [Description("自定义的Tooltip文本, 如果为空则自动使用Text")]
        public string ToolTipText { get; set; }

        [Category("数据")]
        [Bindable(true)]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        public object Tag { get; set; }

        [Browsable(false)]
        public Action<DropdownItem> OnClick { get; set; }

        /// <summary>
        /// 项索引
        /// </summary>
        [Browsable(false)]
        public int Index
        {
            get => index;
            internal set
            {
                if (index != value)
                {
                    index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            if (ItemType == DropdownItemType.Separator)
            {
                return "[分隔符]";
            }

            return string.IsNullOrEmpty(Text) ? "[未命名]" : Text;
        }
    }

    /// <summary>
    /// 下拉列表项集合
    /// </summary>
    [Editor(typeof(DropdownItemCollectionEditor), typeof(UITypeEditor))]
    [Serializable]
    public class DropdownItemCollection : IList<DropdownItem>, IList, INotifyCollectionChanged
    {
        private List<DropdownItem> items = new List<DropdownItem>();

        public event EventHandler CollectionChanged;
        public event NotifyCollectionChangedEventHandler CollectionChangedDetailed;

        public DropdownItem this[int index]
        {
            get { return items[index]; }
            set
            {
                var oldItem = items[index];
                items[index] = value;

                if (value != null)
                {
                    value.Index = index;
                }

                OnCollectionChanged(NotifyCollectionChangedAction.Replace, value, oldItem, index);
            }
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;


        public void Add(DropdownItem item)
        {
            if (item == null)
            {
                return;
            }

            items.Add(item);
            item.Index = items.Count - 1;

            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, null, item.Index);
        }

        public void Insert(int index, DropdownItem item)
        {
            if (item == null)
            {
                return;
            }

            items.Insert(index, item);
            UpdateIndicesFrom(index);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, null, index);
        }

        public bool Remove(DropdownItem item)
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
            var item = items[index];
            items.RemoveAt(index);

            UpdateIndicesFrom(index);
            item.Index = -1;

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, null, item, index);
        }

        public void Clear()
        {
            foreach (var item in items)
            {
                item.Index = -1;
            }

            var oldItems = items.ToList();
            items.Clear();

            OnCollectionChanged(NotifyCollectionChangedAction.Reset, null, oldItems, -1);
        }

        public int IndexOf(DropdownItem item)
        {
            return items.IndexOf(item);
        }

        public bool Contains(DropdownItem item)
        {
            return items.Contains(item);
        }

        public void CopyTo(DropdownItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DropdownItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IList 非泛型实现

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)items).SyncRoot;

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                if (value is DropdownItem item)
                {
                    this[index] = item;
                }
                else
                {
                    throw new ArgumentException("Value must be of type DropdownItem");
                }
            }
        }

        int IList.Add(object value)
        {
            if (value is DropdownItem item)
            {
                Add(item);
                return Count - 1;
            }
            throw new ArgumentException("Value must be of type DropdownItem");
        }

        void IList.Clear()
        {
            Clear();
        }

        bool IList.Contains(object value)
        {
            return value is DropdownItem item && Contains(item);
        }

        int IList.IndexOf(object value)
        {
            if (value is DropdownItem item)
            {
                return IndexOf(item);
            }

            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is DropdownItem item)
            {
                Insert(index, item);
            }
            else
            {
                throw new ArgumentException("Value must be of type DropdownItem");
            }
        }

        void IList.Remove(object value)
        {
            if (value is DropdownItem item)
            {
                Remove(item);
            }
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)items).CopyTo(array, index);
        }

        #endregion

        #region 辅助方法
        public void AddRange(IEnumerable<DropdownItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var itemsList = items.ToList();
            int startIndex = this.items.Count;

            foreach (var item in itemsList)
            {
                this.items.Add(item);
                item.Index = this.items.Count - 1;
            }

            OnCollectionChanged(NotifyCollectionChangedAction.Add, itemsList, null, startIndex);
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            }

            if (newIndex < 0 || newIndex >= items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            if (oldIndex == newIndex)
            {
                return;
            }

            var item = items[oldIndex];
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, item);

            int start = Math.Min(oldIndex, newIndex);
            int end = Math.Max(oldIndex, newIndex);
            UpdateIndicesFrom(start, end + 1);

            OnCollectionChanged(NotifyCollectionChangedAction.Move, item, item, newIndex, oldIndex);
        }

        private void UpdateIndicesFrom(int startIndex, int? endIndex = null)
        {
            int end = endIndex ?? items.Count;
            for (int i = startIndex; i < end; i++)
            {
                if (items[i] != null)
                {
                    items[i].Index = i;
                }
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object newItem, object oldItem, int index, int oldIndex = -1)
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);

            if (CollectionChangedDetailed != null)
            {
                NotifyCollectionChangedEventArgs args = null;

                switch (action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (newItem is IList)
                        {
                            args = new NotifyCollectionChangedEventArgs(action, newItem as IList, index);
                        }
                        else
                        {
                            args = new NotifyCollectionChangedEventArgs(action, newItem, index);
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        args = new NotifyCollectionChangedEventArgs(action, oldItem, index);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        args = new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index);
                        break;

                    case NotifyCollectionChangedAction.Move:
                        args = new NotifyCollectionChangedEventArgs(action, newItem, index, oldIndex);
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        args = new NotifyCollectionChangedEventArgs(action);
                        break;
                }

                CollectionChangedDetailed?.Invoke(this, args);
            }
        }
        #endregion
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 下拉列表项类型
    /// </summary>
    public enum DropdownItemType
    {
        Normal,     // 常规选择项
        Separator,  // 分隔符
        Button      // 按钮项
    }

    /// <summary>
    /// 图标位置
    /// </summary>
    public enum DropdownIconPosition
    {
        Left,
        Top
    }

    /// <summary>
    /// 选择模式
    /// </summary>
    public enum SelectionMode
    {
        Single,     // 单选模式
        Multiple    // 多选模式
    }

    #endregion

    #region 设计时支持

    public class DropdownItemTypeConverter : ExpandableObjectConverter
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
            if (destinationType == typeof(string) && value is DropdownItem item)
            {
                return item.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class DropdownItemCollectionEditor : CollectionEditor
    {
        public DropdownItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(DropdownItem);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(DropdownItem) };
        }

        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(DropdownItem))
            {
                return new DropdownItem
                {
                    Text = "新项目",
                    ItemType = DropdownItemType.Normal,
                    IconIndex = -1
                };
            }
            return base.CreateInstance(itemType);
        }

        protected override string GetDisplayText(object value)
        {
            if (value is DropdownItem item)
            {
                return item.ToString();
            }
            return base.GetDisplayText(value);
        }

        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            form.Text = "下拉列表项集合编辑器";
            form.HelpButton = false;
            form.MinimumSize = new Size(600, 400);
            return form;
        }

        // 可以设置项目是否可以多选
        protected override bool CanSelectMultipleInstances()
        {
            return true;
        }

        // 设置是否可以移除项目
        protected override bool CanRemoveInstance(object value)
        {
            return true;
        }
    }

    /// <summary>
    /// 下拉列表设计器
    /// </summary>
    public class FluentDropdownDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentDropdownActionList(this.Component));
                }
                return actionLists;
            }
        }

        // 添加设计时绘制辅助
        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);

            var dropdown = this.Control as FluentDropdown;
            if (dropdown != null && dropdown.Items.Count == 0)
            {
                // 在设计时显示提示文本
                using (var brush = new SolidBrush(Color.Gray))
                {
                    pe.Graphics.DrawString("(空列表)", dropdown.Font, brush, 5, 5);
                }
            }
        }
    }

    /// <summary>
    /// 设计时智能标记
    /// </summary>
    public class FluentDropdownActionList : DesignerActionList
    {
        private FluentDropdown dropdown;
        private DesignerActionUIService designerActionUIService;

        public FluentDropdownActionList(IComponent component) : base(component)
        {
            dropdown = component as FluentDropdown;
            designerActionUIService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public void AddItem()
        {
            dropdown.AddItem($"项目 {dropdown.Items.Count + 1}");
            RefreshDesigner();
        }

        public void AddSeparator()
        {
            dropdown.AddSeparator();
            RefreshDesigner();
        }

        public void ClearItems()
        {
            dropdown.ClearItems();
            RefreshDesigner();
        }

        // 属性访问器
        public bool ShowIcon
        {
            get { return dropdown.ShowIcon; }
            set
            {
                dropdown.ShowIcon = value;
                RefreshDesigner();
            }
        }

        public bool ShowText
        {
            get { return dropdown.ShowText; }
            set
            {
                dropdown.ShowText = value;
                RefreshDesigner();
            }
        }

        public DropdownIconPosition IconPosition
        {
            get { return dropdown.IconPosition; }
            set
            {
                dropdown.IconPosition = value;
                RefreshDesigner();
            }
        }

        public string DisplayText
        {
            get { return dropdown.DisplayText; }
            set
            {
                dropdown.DisplayText = value;
                RefreshDesigner();
            }
        }

        private void RefreshDesigner()
        {
            if (designerActionUIService != null)
            {
                designerActionUIService.Refresh(Component);
            }
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 快速操作
            items.Add(new DesignerActionHeaderItem("快速操作"));
            items.Add(new DesignerActionMethodItem(this, "AddItem", "添加项目", "快速操作", true));
            items.Add(new DesignerActionMethodItem(this, "AddSeparator", "添加分隔符", "快速操作", true));
            items.Add(new DesignerActionMethodItem(this, "ClearItems", "清除所有项目", "快速操作", true));

            // 外观设置
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowIcon", "显示图标", "外观"));
            items.Add(new DesignerActionPropertyItem("ShowText", "显示文本", "外观"));
            items.Add(new DesignerActionPropertyItem("IconPosition", "图标位置", "外观"));
            items.Add(new DesignerActionPropertyItem("DisplayText", "显示文本", "外观"));

            return items;
        }
    }

    #endregion
}
