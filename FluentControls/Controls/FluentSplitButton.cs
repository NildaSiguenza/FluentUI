using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.Design.Behavior;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent分割按钮
    /// </summary>
    [DefaultEvent("Click")]
    [DefaultProperty("Text")]
    [Designer(typeof(FluentSplitButtonDesigner))]
    public class FluentSplitButton : FluentControlBase
    {
        private Image image;
        private string text = string.Empty;
        private FluentSplitButtonOrientation orientation = FluentSplitButtonOrientation.Horizontal;
        private FluentSplitButtonOrientation? dropDownOrientation = null;
        private FluentSplitButtonItemCollection items;
        private bool isPressed = false;
        private bool enablePressedState = true;
        private FluentSplitButtonItem pressedItem; // 当前按下的项

        private Size iconSize = new Size(16, 16);
        private bool showIcon = true;
        private bool showText = true;
        private int itemSpacing = 4;
        private int dropDownSize = 20;
        private FluentSplitButtonItemPosition itemPosition = FluentSplitButtonItemPosition.Center;
        private bool autoSizeToContent = false;
        private bool showDropDownBorder = true;
        private int padding = 4;

        private Rectangle contentRect;  // 图标文本区域
        private Rectangle iconRect;
        private Rectangle textRect;
        private Rectangle dropDownRect;  // 下拉箭头区域
        private bool isDropDownHot = false;
        private bool isContentHot = false;

        [NonSerialized]
        private FluentSplitButtonDropDown dropDown;

        [NonSerialized]
        private List<FluentSplitButtonSubItemDropDown> openSubDropDowns = new List<FluentSplitButtonSubItemDropDown>();
        public FluentSplitButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            items = new FluentSplitButtonItemCollection(this, true);

            this.Size = new Size(100, 32);
            this.Cursor = Cursors.Hand;
            this.BackColor = SystemColors.Control;
            this.ForeColor = SystemColors.ControlText;
            this.Font = SystemFonts.DefaultFont;
            this.Visible = true;

            CalculateLayout();
        }

        #region 属性

        [Category("Appearance")]
        [Description("按钮图标")]
        [DefaultValue(null)]
        public Image Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("按钮文本")]
        [DefaultValue("")]
        public override string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value ?? string.Empty;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("排列方向")]
        [DefaultValue(FluentSplitButtonOrientation.Horizontal)]
        [RefreshProperties(RefreshProperties.All)]
        public FluentSplitButtonOrientation Orientation
        {
            get => orientation;
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("下拉列表排列方向(null表示自动反向)")]
        [DefaultValue(null)]
        public FluentSplitButtonOrientation? DropDownOrientation
        {
            get => dropDownOrientation;
            set => dropDownOrientation = value;
        }

        [Category("Fluent")]
        [Description("实际使用的下拉列表排列方向")]
        [Browsable(false)]
        public FluentSplitButtonOrientation ActualDropDownOrientation
        {
            get
            {
                if (dropDownOrientation.HasValue)
                {
                    return dropDownOrientation.Value;
                }

                return orientation == FluentSplitButtonOrientation.Horizontal
                    ? FluentSplitButtonOrientation.Vertical
                    : FluentSplitButtonOrientation.Horizontal;
            }
        }

        [Category("Fluent")]
        [Description("按钮项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentSplitButtonItemCollectionEditor), typeof(UITypeEditor))]
        [MergableProperty(false)]
        public FluentSplitButtonItemCollection Items
        {
            get
            {
                if (items == null)
                {
                    items = new FluentSplitButtonItemCollection(this, true);
                }

                return items;
            }
        }

        [Category("Fluent")]
        [Description("是否处于按压状态")]
        [DefaultValue(false)]
        [Browsable(true)]
        public bool IsPressed
        {
            get => isPressed;
            set
            {
                if (isPressed != value)
                {
                    isPressed = value;
                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("是否启用按压状态功能")]
        [DefaultValue(true)]
        public bool EnablePressedState
        {
            get => enablePressedState;
            set => enablePressedState = value;
        }

        /// <summary>
        /// 当前按下的项(null 表示主按钮被按下)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FluentSplitButtonItem PressedItem
        {
            get => pressedItem;
            set
            {
                if (pressedItem != value)
                {
                    pressedItem = value;
                    Invalidate();

                    // 刷新下拉面板中的项
                    if (IsDropDownOpen && dropDown != null)
                    {
                        dropDown.UpdateItems();
                    }
                }
            }
        }

        /// <summary>
        /// 判断主按钮是否被按下
        /// </summary>
        [Browsable(false)]
        public bool IsMainButtonPressed => IsPressed && PressedItem == null;

        [Category("Fluent")]
        [Description("图标大小")]
        [DefaultValue(typeof(Size), "16, 16")]
        public Size IconSize
        {
            get => iconSize;
            set
            {
                if (iconSize != value)
                {
                    iconSize = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("是否显示图标")]
        [DefaultValue(true)]
        public bool ShowIcon
        {
            get => showIcon;
            set
            {
                if (showIcon != value)
                {
                    showIcon = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("是否显示文本")]
        [DefaultValue(true)]
        public bool ShowText
        {
            get => showText;
            set
            {
                if (showText != value)
                {
                    showText = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("图标和文本之间的间距")]
        [DefaultValue(4)]
        public int ItemSpacing
        {
            get => itemSpacing;
            set
            {
                if (itemSpacing != value)
                {
                    itemSpacing = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("下拉箭头区域的宽度(横向)或高度(纵向)")]
        [DefaultValue(20)]
        public int DropDownSize
        {
            get => dropDownSize;
            set
            {
                if (dropDownSize != value && value > 0)
                {
                    dropDownSize = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Category("Fluent")]
        [Description("图标和文本在内容区域中的位置")]
        [DefaultValue(FluentSplitButtonItemPosition.Center)]
        public FluentSplitButtonItemPosition ItemPosition
        {
            get => itemPosition;
            set
            {
                if (itemPosition != value)
                {
                    itemPosition = value;
                    CalculateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [Description("是否根据内容自动调整大小")]
        [DefaultValue(false)]
        public bool AutoSizeToContent
        {
            get => autoSizeToContent;
            set
            {
                if (autoSizeToContent != value)
                {
                    autoSizeToContent = value;
                    if (value)
                    {
                        PerformAutoSize();
                    }
                }
            }
        }

        [Category("Fluent")]
        [Description("是否显示下拉面板边框")]
        [DefaultValue(true)]
        public bool ShowDropDownBorder
        {
            get => showDropDownBorder;
            set => showDropDownBorder = value;
        }

        [Category("Fluent")]
        [Description("内容区域的内边距")]
        [DefaultValue(4)]
        public int ContentPadding
        {
            get => padding;
            set
            {
                if (padding != value && value >= 0)
                {
                    padding = value;
                    if (autoSizeToContent)
                    {
                        PerformAutoSize();
                    }
                    else
                    {
                        CalculateLayout();
                    }

                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasDropDown => items != null && items.Count > 0;

        [Browsable(false)]
        public bool IsDropDownOpen => dropDown != null && dropDown.Visible;

        #endregion

        #region 布局计算

        private void CalculateLayout()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            bool hasDropDown = HasDropDown;

            // 第一步：划分两个主要区域
            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                // 横向：下拉区在右侧
                if (hasDropDown)
                {
                    dropDownRect = new Rectangle(Width - dropDownSize, 0, dropDownSize, Height);
                    contentRect = new Rectangle(0, 0, Width - dropDownSize, Height);
                }
                else
                {
                    dropDownRect = Rectangle.Empty;
                    contentRect = new Rectangle(0, 0, Width, Height);
                }
            }
            else
            {
                // 纵向：下拉区在底部
                if (hasDropDown)
                {
                    dropDownRect = new Rectangle(0, Height - dropDownSize, Width, dropDownSize);
                    contentRect = new Rectangle(0, 0, Width, Height - dropDownSize);
                }
                else
                {
                    dropDownRect = Rectangle.Empty;
                    contentRect = new Rectangle(0, 0, Width, Height);
                }
            }

            // 第二步：在内容区域中布局图标和文本
            CalculateContentLayout();
        }

        private void CalculateContentLayout()
        {
            if (contentRect.IsEmpty)
            {
                return;
            }

            iconRect = Rectangle.Empty;
            textRect = Rectangle.Empty;

            bool hasIcon = showIcon && image != null;
            bool hasText = showText && !string.IsNullOrEmpty(text);

            if (!hasIcon && !hasText)
            {
                return;
            }

            // 计算内容实际占用的大小
            Size contentSize = CalculateContentSize();

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                // 横向布局
                int totalWidth = contentSize.Width;
                int totalHeight = contentSize.Height;

                // 根据 ItemPosition 确定起始位置
                int startX = contentRect.X;
                switch (itemPosition)
                {
                    case FluentSplitButtonItemPosition.Start:
                        startX = contentRect.X + padding;
                        break;
                    case FluentSplitButtonItemPosition.Center:
                        startX = contentRect.X + (contentRect.Width - totalWidth) / 2;
                        break;
                    case FluentSplitButtonItemPosition.End:
                        startX = contentRect.Right - totalWidth - padding;
                        break;
                }

                int y = contentRect.Y + (contentRect.Height - totalHeight) / 2;

                // 布局图标
                if (hasIcon)
                {
                    iconRect = new Rectangle(startX, y + (totalHeight - iconSize.Height) / 2,
                        iconSize.Width, iconSize.Height);
                    startX += iconSize.Width;
                    if (hasText)
                    {
                        startX += itemSpacing;
                    }
                }

                // 布局文本
                if (hasText)
                {
                    Size textSize = MeasureText();
                    textRect = new Rectangle(startX, y + (totalHeight - textSize.Height) / 2,
                        textSize.Width, textSize.Height);
                }
            }
            else
            {
                // 纵向布局
                int totalWidth = contentSize.Width;
                int totalHeight = contentSize.Height;

                // 根据 ItemPosition 确定起始位置
                int startY = contentRect.Y;
                switch (itemPosition)
                {
                    case FluentSplitButtonItemPosition.Start:
                        startY = contentRect.Y + padding;
                        break;
                    case FluentSplitButtonItemPosition.Center:
                        startY = contentRect.Y + (contentRect.Height - totalHeight) / 2;
                        break;
                    case FluentSplitButtonItemPosition.End:
                        startY = contentRect.Bottom - totalHeight - padding;
                        break;
                }

                int x = contentRect.X + (contentRect.Width - totalWidth) / 2;

                // 布局图标
                if (hasIcon)
                {
                    iconRect = new Rectangle(x + (totalWidth - iconSize.Width) / 2, startY,
                        iconSize.Width, iconSize.Height);
                    startY += iconSize.Height;
                    if (hasText)
                    {
                        startY += itemSpacing;
                    }
                }

                // 布局文本
                if (hasText)
                {
                    Size textSize = MeasureText();
                    textRect = new Rectangle(x + (totalWidth - textSize.Width) / 2, startY,
                        textSize.Width, textSize.Height);
                }
            }
        }

        private Size CalculateContentSize()
        {
            bool hasIcon = showIcon && image != null;
            bool hasText = showText && !string.IsNullOrEmpty(text);

            if (!hasIcon && !hasText)
            {
                return Size.Empty;
            }

            int width = 0;
            int height = 0;

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                // 横向：宽度累加, 高度取最大
                if (hasIcon)
                {
                    width += iconSize.Width;
                    height = Math.Max(height, iconSize.Height);
                }

                if (hasText)
                {
                    Size textSize = MeasureText();
                    if (hasIcon)
                    {
                        width += itemSpacing;
                    }

                    width += textSize.Width;
                    height = Math.Max(height, textSize.Height);
                }
            }
            else
            {
                // 纵向：高度累加, 宽度取最大
                if (hasIcon)
                {
                    width = Math.Max(width, iconSize.Width);
                    height += iconSize.Height;
                }

                if (hasText)
                {
                    Size textSize = MeasureText();
                    if (hasIcon)
                    {
                        height += itemSpacing;
                    }

                    width = Math.Max(width, textSize.Width);
                    height += textSize.Height;
                }
            }

            return new Size(width, height);
        }

        private Size MeasureText()
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }

            try
            {
                using (var g = CreateGraphics())
                {
                    var size = g.MeasureString(text, this.Font);
                    return new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
                }
            }
            catch
            {
                return new Size(50, 20);
            }
        }

        private void PerformAutoSize()
        {
            Size contentSize = CalculateContentSize();

            int width, height;

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                width = contentSize.Width + padding * 2;
                height = contentSize.Height + padding * 2;

                if (HasDropDown)
                {
                    width += dropDownSize;
                }

                height = Math.Max(height, 32); // 最小高度
            }
            else
            {
                width = contentSize.Width + padding * 2;
                height = contentSize.Height + padding * 2;

                if (HasDropDown)
                {
                    height += dropDownSize;
                }

                width = Math.Max(width, 60); // 最小宽度
            }

            this.Size = new Size(width, height);
            CalculateLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!autoSizeToContent)
            {
                CalculateLayout();
            }

            Invalidate();
        }

        protected override void OnBoundsChanged()
        {
            base.OnBoundsChanged();
            if (!autoSizeToContent)
            {
                CalculateLayout();
            }
        }

        internal void OnItemsChanged()
        {
            if (autoSizeToContent)
            {
                PerformAutoSize();
            }
            else
            {
                CalculateLayout();
            }

            Invalidate();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            Color bgColor;

            if (!Enabled)
            {
                bgColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.Control);
            }
            else if (IsMainButtonPressed && EnablePressedState)
            {
                bgColor = GetThemeColor(c => c.SurfacePressed, SystemColors.ControlDark);
            }
            else if (isContentHot || State == ControlState.Hover || State == ControlState.Focused)
            {
                bgColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
            }
            else
            {
                bgColor = UseTheme
                    ? GetThemeColor(c => c.Surface, SystemColors.Control)
                    : this.BackColor;
            }

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 绘制下拉区域高亮
            if (isDropDownHot && !dropDownRect.IsEmpty)
            {
                var hoverColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
                var dropDownBgColor = AdjustBrightness(hoverColor, -0.05f);

                using (var brush = new SolidBrush(dropDownBgColor))
                {
                    g.FillRectangle(brush, dropDownRect);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            var fgColor = Enabled
                ? (UseTheme ? GetThemeColor(c => c.TextPrimary, this.ForeColor) : this.ForeColor)
                : GetThemeColor(c => c.TextDisabled, SystemColors.GrayText);

            // 绘制图标
            if (!iconRect.IsEmpty && image != null)
            {
                try
                {
                    if (Enabled)
                    {
                        g.DrawImage(image, iconRect);
                    }
                    else
                    {
                        ControlPaint.DrawImageDisabled(g, image, iconRect.X, iconRect.Y, Color.Transparent);
                    }
                }
                catch { }
            }

            // 绘制文本
            if (!textRect.IsEmpty && !string.IsNullOrEmpty(text))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                using (var brush = new SolidBrush(fgColor))
                {
                    g.DrawString(text, this.Font, brush, textRect, sf);
                }
            }

            // 绘制下拉分割线和箭头
            if (!dropDownRect.IsEmpty)
            {
                // 绘制分割线
                var separatorColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
                using (var pen = new Pen(separatorColor, 1))
                {
                    if (orientation == FluentSplitButtonOrientation.Horizontal)
                    {
                        int lineX = dropDownRect.X;
                        g.DrawLine(pen, lineX, 0, lineX, Height);
                    }
                    else
                    {
                        int lineY = dropDownRect.Y;
                        g.DrawLine(pen, 0, lineY, Width, lineY);
                    }
                }

                // 绘制箭头(居中)
                DrawDropDownArrow(g, dropDownRect, fgColor);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            Color borderColor;

            if (IsMainButtonPressed && EnablePressedState)
            {
                borderColor = GetThemeColor(c => c.BorderFocused, SystemColors.Highlight);
            }
            else if (State == ControlState.Hover || State == ControlState.Focused)
            {
                borderColor = GetThemeColor(c => c.BorderLight, SystemColors.ControlDark);
            }
            else
            {
                borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
            }

            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawDropDownArrow(Graphics g, Rectangle rect, Color color)
        {
            var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            var arrowSize = 4;

            Point[] arrow = new Point[]
            {
            new Point(center.X - arrowSize, center.Y - arrowSize / 2),
            new Point(center.X + arrowSize, center.Y - arrowSize / 2),
            new Point(center.X, center.Y + arrowSize / 2)
            };

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        #endregion

        #region 鼠标交互

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool newIsDropDownHot = !dropDownRect.IsEmpty && dropDownRect.Contains(e.Location);
            bool newIsContentHot = contentRect.Contains(e.Location);

            if (newIsDropDownHot != isDropDownHot || newIsContentHot != isContentHot)
            {
                isDropDownHot = newIsDropDownHot;
                isContentHot = newIsContentHot;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (isDropDownHot || isContentHot)
            {
                isDropDownHot = false;
                isContentHot = false;
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            var mousePos = PointToClient(MousePosition);

            // 检查是否点击下拉区域
            if (!dropDownRect.IsEmpty && dropDownRect.Contains(mousePos))
            {
                // 点击下拉箭头不改变按压状态
                ToggleDropDown();
            }
            else
            {
                // 点击内容区域
                if (!IsDropDownOpen)
                {
                    if (EnablePressedState)
                    {
                        // 如果已经按下主按钮, 取消按下; 否则按下主按钮
                        if (IsPressed && PressedItem == null)
                        {
                            IsPressed = false;
                            PressedItem = null;
                        }
                        else
                        {
                            IsPressed = true;
                            PressedItem = null;  // 主按钮被按下
                        }
                    }

                    OnButtonClick();
                    base.OnClick(e);
                }
            }
        }

        protected virtual void OnButtonClick()
        {
            // 子类可重写或订阅Click事件
        }

        #endregion

        #region 下拉菜单

        public event EventHandler DropDownOpening;
        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;

        public void ShowDropDown()
        {
            if (!HasDropDown)
            {
                return;
            }

            DropDownOpening?.Invoke(this, EventArgs.Empty);

            if (dropDown == null)
            {
                dropDown = new FluentSplitButtonDropDown(this);
            }

            dropDown.UpdateItems();

            Point location = PointToScreen(new Point(0, Height));

            //Point location;
            //if (orientation == FluentSplitButtonOrientation.Horizontal)
            //{
            //    location = PointToScreen(new Point(0, Height));
            //}
            //else
            //{
            //    location = PointToScreen(new Point(Width, 0));
            //}

            dropDown.Show(location);

            DropDownOpened?.Invoke(this, EventArgs.Empty);
        }

        public void HideDropDown(bool immediate = true)
        {
            if (immediate)
            {
                // 立即关闭
                CloseAllSubDropDownsInternal(except: null);

                if (dropDown != null && !dropDown.IsDisposed && dropDown.Visible)
                {
                    dropDown.Close();
                }
            }
            else
            {
                // 延迟关闭
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!IsDisposed)
                        {
                            CloseAllSubDropDownsInternal(except: null);

                            if (dropDown != null && !dropDown.IsDisposed && dropDown.Visible)
                            {
                                dropDown.Close();
                            }
                        }
                    }));
                }
            }
        }

        public void HideDropDown()
        {
            HideDropDown(immediate: true);
        }

        public void ToggleDropDown()
        {
            if (IsDropDownOpen)
            {
                HideDropDown();
            }
            else
            {
                ShowDropDown();
            }
        }

        internal void OnDropDownClosed()
        {
            DropDownClosed?.Invoke(this, EventArgs.Empty);
        }

        internal void OnItemClicked(FluentSplitButtonItem item)
        {
            // 先关闭所有子项下拉面板
            CloseAllSubDropDowns(immediate: false, except: null);

            // 项被点击时, 设置按压状态(如果启用)
            if (EnablePressedState)
            {
                // 如果已经按下该项, 取消按下; 否则按下该项
                if (IsPressed && PressedItem == item)
                {
                    IsPressed = false;
                    PressedItem = null;
                }
                else
                {
                    IsPressed = true;
                    PressedItem = item;
                }
            }

            // 关闭下拉面板
            HideDropDown(immediate: false);
        }

        #endregion

        #region 子面板管理

        /// <summary>
        /// 关闭指定项的子面板(除了排除的)
        /// </summary>
        internal void CloseOtherSubDropDowns(FluentSplitButtonItem forItem)
        {
            // 找到该项对应的子面板
            var itemSubDropDown = openSubDropDowns
                .FirstOrDefault(sd => sd != null && !sd.IsDisposed && sd.ParentItem == forItem);

            // 关闭其他所有子面板
            //CloseAllSubDropDowns(immediate: false, except: itemSubDropDown);
        }

        /// <summary>
        /// 注册打开的子项下拉面板
        /// </summary>
        internal void RegisterSubDropDown(FluentSplitButtonSubItemDropDown subDropDown)
        {
            if (subDropDown != null && !openSubDropDowns.Contains(subDropDown))
            {
                openSubDropDowns.Add(subDropDown);

                // 订阅关闭事件，自动从列表中移除
                subDropDown.Closed += (s, e) =>
                {
                    openSubDropDowns.Remove(subDropDown);
                };
            }
        }

        /// <summary>
        /// 关闭所有子项下拉面板
        /// </summary>
        internal void CloseAllSubDropDowns(bool immediate = false, FluentSplitButtonSubItemDropDown except = null)
        {
            if (immediate)
            {
                CloseAllSubDropDownsInternal(except);
            }
            else
            {
                // 延迟执行，确保当前事件处理完成
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!IsDisposed)
                        {
                            CloseAllSubDropDownsInternal(except);
                        }
                    }));
                }
            }
        }

        private void CloseAllSubDropDownsInternal(FluentSplitButtonSubItemDropDown except = null)
        {
            // 创建副本以避免在遍历时修改集合
            var subDropDownsToClose = openSubDropDowns
                                       .Where(sd => sd != except && sd != null && !sd.IsDisposed)
                                       .OrderByDescending(sd => GetNestingLevel(sd.ParentItem))
                                       .ToArray();

            foreach (var subDropDown in subDropDownsToClose)
            {
                try
                {
                    if (subDropDown != null && !subDropDown.IsDisposed && subDropDown.Visible)
                    {
                        subDropDown.Close();
                        System.Threading.Thread.Sleep(1);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 对象已释放，忽略
                }
                catch (InvalidOperationException)
                {
                    // 可能在关闭过程中，忽略
                }
            }

            // 只清理已关闭的面板
            openSubDropDowns.RemoveAll(sd =>
                sd == null ||
                sd.IsDisposed ||
                !sd.Visible ||
                subDropDownsToClose.Contains(sd));
        }

        /// <summary>
        /// 计算项的嵌套层级
        /// </summary>
        private int GetNestingLevel(FluentSplitButtonItem item)
        {
            int level = 0;
            var current = item;
            while (current != null)
            {
                level++;
                current = current.Parent;
            }
            return level;
        }

        /// <summary>
        /// 关闭指定项的子项下拉面板
        /// </summary>
        internal void CloseSubDropDownForItem(FluentSplitButtonItem item)
        {
            var subDropDownsToClose = openSubDropDowns
                .Where(sd => sd != null && !sd.IsDisposed && sd.ParentItem == item)
                .ToArray();

            foreach (var subDropDown in subDropDownsToClose)
            {
                subDropDown.Close();
                openSubDropDowns.Remove(subDropDown);
            }
        }

        #endregion

        #region 序列化支持

        private bool ShouldSerializeItems()
        {
            return items != null && items.Count > 0;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (items != null)
            {
                items.SetOwner(this);
                foreach (FluentSplitButtonItem item in items)
                {
                    item.Owner = this;
                }
            }

            if (autoSizeToContent)
            {
                PerformAutoSize();
            }
            else
            {
                CalculateLayout();
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseAllSubDropDownsInternal(except: null);
                dropDown?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    #region 按钮项和集合

    /// <summary>
    /// 分割按钮项
    /// </summary>
    [ToolboxItem(false)]
    [TypeConverter(typeof(FluentSplitButtonItemConverter))]
    public class FluentSplitButtonItem : ICloneable
    {
        private Image image;
        private string text = string.Empty;
        private FluentSplitButtonItemType itemType = FluentSplitButtonItemType.Button;
        private FluentSplitButtonItemCollection items;
        private bool enabled = true;

        // 不序列化这些引用
        [NonSerialized]
        private FluentSplitButtonItem parent;

        [NonSerialized]
        private FluentSplitButton owner;

        public FluentSplitButtonItem()
        {
            items = new FluentSplitButtonItemCollection(this);
        }

        public FluentSplitButtonItem(string text) : this()
        {
            this.text = text ?? string.Empty;
        }

        public FluentSplitButtonItem(string text, Image image) : this(text)
        {
            this.image = image;
        }

        public FluentSplitButtonItem(string text, FluentSplitButtonItemType itemType) : this(text)
        {
            this.itemType = itemType;
        }

        #region 可序列化属性

        [Category("Appearance")]
        [Description("按钮图标")]
        [DefaultValue(null)]
        public Image Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Appearance")]
        [Description("按钮文本")]
        [DefaultValue("")]
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Behavior")]
        [Description("项类型")]
        [DefaultValue(FluentSplitButtonItemType.Button)]
        public FluentSplitButtonItemType ItemType
        {
            get => itemType;
            set
            {
                if (itemType != value)
                {
                    itemType = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Behavior")]
        [Description("是否启用")]
        [DefaultValue(true)]
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Behavior")]
        [Description("子项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentSplitButtonItemCollection Items
        {
            get => items;
        }

        #endregion

        #region 不序列化的属性

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasItems => items != null && items.Count > 0;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal FluentSplitButtonItem Parent
        {
            get => parent;
            set => parent = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal FluentSplitButton Owner
        {
            get => owner;
            set => owner = value;
        }

        #endregion

        #region 事件(不序列化)

        [field: NonSerialized]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler Click;

        [field: NonSerialized]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler PropertyChanged;

        internal void OnClick()
        {
            if (Enabled && ItemType == FluentSplitButtonItemType.Button)
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            owner?.Invalidate();
        }

        #endregion

        #region 序列化支持

        private bool ShouldSerializeImage()
        {
            return image != null;
        }

        private bool ShouldSerializeText()
        {
            return !string.IsNullOrEmpty(text);
        }

        private bool ShouldSerializeItemType()
        {
            return itemType != FluentSplitButtonItemType.Button;
        }

        private bool ShouldSerializeEnabled()
        {
            return !enabled;
        }

        private bool ShouldSerializeItems()
        {
            return items != null && items.Count > 0;
        }

        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("Text", text);
        //    info.AddValue("ItemType", itemType);
        //    info.AddValue("Enabled", enabled);

        //    if (image != null)
        //    {
        //        try
        //        {
        //            info.AddValue("Image", image);
        //        }
        //        catch { }
        //    }

        //    if (items != null && items.Count > 0)
        //    {
        //        var itemsArray = new FluentSplitButtonItem[items.Count];
        //        items.CopyTo(itemsArray, 0);
        //        info.AddValue("Items", itemsArray);
        //    }
        //}

        #endregion

        public object Clone()
        {
            var item = new FluentSplitButtonItem
            {
                Image = this.Image,
                Text = this.Text,
                ItemType = this.ItemType,
                Enabled = this.Enabled
            };

            foreach (FluentSplitButtonItem child in this.Items)
            {
                item.Items.Add((FluentSplitButtonItem)child.Clone());
            }

            return item;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Text) ? "(未命名)" : Text;
        }
    }

    internal class FluentSplitButtonItemConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is FluentSplitButtonItem item)
            {
                return item.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context,
            object value, Attribute[] attributes)
        {
            if (value is FluentSplitButtonItem)
            {
                return TypeDescriptor.GetProperties(typeof(FluentSplitButtonItem), attributes);
            }
            return base.GetProperties(context, value, attributes);
        }
    }

    /// <summary>
    /// 分割按钮项集合
    /// </summary>
    [Editor(typeof(FluentSplitButtonItemCollectionEditor), typeof(UITypeEditor))]
    public class FluentSplitButtonItemCollection : IList, IList<FluentSplitButtonItem>
    {
        private readonly List<FluentSplitButtonItem> items = new List<FluentSplitButtonItem>();

        [NonSerialized]
        private FluentSplitButtonItem ownerItem;

        [NonSerialized]
        private FluentSplitButton ownerButton;

        public FluentSplitButtonItemCollection()
        {
            this.ownerItem = null;
            this.ownerButton = null;
        }

        internal FluentSplitButtonItemCollection(FluentSplitButtonItem owner) : this()
        {
            this.ownerItem = owner;
        }

        internal FluentSplitButtonItemCollection(FluentSplitButton owner, bool isButton) : this()
        {
            this.ownerButton = owner;
        }

        // 设置owner的内部方法
        internal void SetOwner(FluentSplitButton owner)
        {
            this.ownerButton = owner;
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.Owner = owner;
                }
            }
        }

        internal void SetOwner(FluentSplitButtonItem owner)
        {
            this.ownerItem = owner;
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.Parent = owner;
                    item.Owner = GetRootOwner();
                }
            }
        }

        #region IList<T> 实现

        public FluentSplitButtonItem this[int index]
        {
            get => items[index];
            set
            {
                var oldItem = items[index];
                if (oldItem != null)
                {
                    oldItem.Parent = null;
                    oldItem.Owner = null;
                }

                items[index] = value;

                if (value != null)
                {
                    value.Parent = ownerItem;
                    value.Owner = GetRootOwner();
                }

                OnCollectionChanged();
            }
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public object SyncRoot => ((ICollection)items).SyncRoot;
        public bool IsSynchronized => false;

        public void Add(FluentSplitButtonItem item)
        {
            if (item == null)
            {
                return;
            }

            items.Add(item);
            item.Parent = ownerItem;
            item.Owner = GetRootOwner();
            OnCollectionChanged();
        }

        public void AddRange(IEnumerable<FluentSplitButtonItem> collection)
        {
            if (collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                if (item != null)
                {
                    items.Add(item);
                    item.Parent = ownerItem;
                    item.Owner = GetRootOwner();
                }
            }
            OnCollectionChanged();
        }

        public void Insert(int index, FluentSplitButtonItem item)
        {
            if (item == null)
            {
                return;
            }

            items.Insert(index, item);
            item.Parent = ownerItem;
            item.Owner = GetRootOwner();
            OnCollectionChanged();
        }

        public bool Remove(FluentSplitButtonItem item)
        {
            if (items.Remove(item))
            {
                if (item != null)
                {
                    item.Parent = null;
                    item.Owner = null;
                }
                OnCollectionChanged();
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var item = items[index];
            items.RemoveAt(index);

            if (item != null)
            {
                item.Parent = null;
                item.Owner = null;
            }

            OnCollectionChanged();
        }

        public void Clear()
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.Parent = null;
                    item.Owner = null;
                }
            }
            items.Clear();
            OnCollectionChanged();
        }

        public bool Contains(FluentSplitButtonItem item) => items.Contains(item);
        public int IndexOf(FluentSplitButtonItem item) => items.IndexOf(item);
        public void CopyTo(FluentSplitButtonItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public void CopyTo(Array array, int index) => ((ICollection)items).CopyTo(array, index);

        public IEnumerator<FluentSplitButtonItem> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IList 实现(非泛型)

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = value as FluentSplitButtonItem;
        }

        int IList.Add(object value)
        {
            if (value is FluentSplitButtonItem item)
            {
                Add(item);
                return Count - 1;
            }
            return -1;
        }

        bool IList.Contains(object value)
        {
            return value is FluentSplitButtonItem item && Contains(item);
        }

        int IList.IndexOf(object value)
        {
            return value is FluentSplitButtonItem item ? IndexOf(item) : -1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is FluentSplitButtonItem item)
            {
                Insert(index, item);
            }
        }

        void IList.Remove(object value)
        {
            if (value is FluentSplitButtonItem item)
            {
                Remove(item);
            }
        }

        #endregion

        #region ISerializable 实现

        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    if (items.Count > 0)
        //    {
        //        var itemsArray = new FluentSplitButtonItem[items.Count];
        //        items.CopyTo(itemsArray, 0);
        //        info.AddValue("Items", itemsArray);
        //    }
        //}

        #endregion

        private FluentSplitButton GetRootOwner()
        {
            if (ownerButton != null)
            {
                return ownerButton;
            }

            var current = ownerItem;
            while (current?.Parent != null)
            {
                current = current.Parent;
            }
            return current?.Owner;
        }

        private void OnCollectionChanged()
        {
            GetRootOwner()?.OnItemsChanged();
        }
    }

    #endregion

    #region  下拉面板

    [ToolboxItem(false)]
    public class FluentSplitButtonDropDown : ToolStripDropDown
    {
        private FluentSplitButton owner;
        private FluentSplitButtonDropDownPanel panel;

        // Windows 消息常量
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int WM_NCACTIVATE = 0x0086;
        private const int MA_NOACTIVATE = 0x0003;

        public FluentSplitButtonDropDown(FluentSplitButton owner)
        {
            this.owner = owner;
            this.AutoSize = true;
            this.Margin = Padding.Empty;
            this.Padding = new Padding(0); // 移除默认padding
            this.DropShadowEnabled = true;
            this.AutoClose = false;

            panel = new FluentSplitButtonDropDownPanel(owner);
            var host = new ToolStripControlHost(panel)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = true
            };

            Items.Add(host);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE - 防止窗口激活时改变焦点
                cp.ExStyle |= 0x08000000;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEACTIVATE:
                    // 阻止鼠标激活改变焦点
                    m.Result = (IntPtr)MA_NOACTIVATE;
                    return;

                case WM_NCACTIVATE:
                    // 阻止非客户区激活改变焦点
                    // 但仍然绘制激活状态
                    m.WParam = (IntPtr)1;
                    break;
            }

            base.WndProc(ref m);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // 处理键盘输入但不触发焦点变化
            return base.ProcessDialogKey(keyData);
        }

        public void UpdateItems()
        {
            panel.UpdateItems();
        }

        public void RequestClose(FluentSplitButtonItem clickedItem, bool isDropDownArrowClicked)
        {
            // 如果点击的是下拉箭头, 不关闭
            if (isDropDownArrowClicked)
            {
                return;
            }

            // 如果点击的项是标签, 不关闭
            if (clickedItem.ItemType == FluentSplitButtonItemType.Label)
            {
                return;
            }

            // 如果点击的项有子项, 不关闭(用户可能想打开子项)
            if (clickedItem.HasItems)
            {
                return;
            }

            // 其他情况, 关闭面板
            this.Close();
        }


        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            panel.UpdateItems();
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            base.OnClosed(e);
            owner.OnDropDownClosed();
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            // 只允许通过代码或点击项来关闭
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                // 阻止默认的 ItemClicked 关闭
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }

    /// <summary>
    /// 下拉面板内容控件
    /// </summary>
    [ToolboxItem(false)]
    public class FluentSplitButtonDropDownPanel : FluentContainerBase
    {
        private FluentSplitButton owner;
        private FluentSplitButtonDropDown parentDropDown;
        private List<FluentSplitButtonItemControl> itemControls = new List<FluentSplitButtonItemControl>();
        private int itemSpacing = 2;

        public FluentSplitButtonDropDownPanel(FluentSplitButton owner, FluentSplitButtonDropDown parentDropDown = null)
        {
            this.owner = owner;
            this.AutoSize = true;
            this.parentDropDown = parentDropDown;
            this.Padding = new Padding(0);
        }

        public void UpdateItems()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            itemControls.Clear();

            if (owner.Items.Count == 0)
            {
                this.Size = new Size(100, 50);
                this.ResumeLayout();
                return;
            }

            // 获取排列方向
            var listOrientation = owner.ActualDropDownOrientation;

            int x = owner.ShowDropDownBorder ? 1 : 0; // 如果有边框, 留1px
            int y = owner.ShowDropDownBorder ? 1 : 0;
            int maxWidth = 0;
            int maxHeight = 0;

            // 创建项控件
            foreach (FluentSplitButtonItem item in owner.Items)
            {
                var itemControl = new FluentSplitButtonItemControl(item, owner);

                itemControl.ItemContentClicked += (s, e) =>
                {
                    // 通知父下拉面板处理关闭逻辑
                    //parentDropDown?.RequestClose(item, false);

                    // 通知主控件项被点击
                    owner.OnItemClicked(item);
                };

                itemControl.ItemDropDownClicked += (s, e) =>
                {
                    owner.CloseOtherSubDropDowns(item);
                };

                // 订阅点击事件
                //itemControl.ItemClicked += (s, e) =>
                //{
                //    // 通知主控件项被点击
                //    owner.OnItemClicked(item);
                //};

                itemControls.Add(itemControl);
                this.Controls.Add(itemControl);

                if (listOrientation == FluentSplitButtonOrientation.Vertical)
                {
                    // 纵向排列：每行一个
                    itemControl.Location = new Point(x, y);
                    y += itemControl.Height + itemSpacing;
                    maxWidth = Math.Max(maxWidth, itemControl.Width);
                }
                else
                {
                    // 横向排列：每列一个
                    itemControl.Location = new Point(x, y);
                    x += itemControl.Width + itemSpacing;
                    maxHeight = Math.Max(maxHeight, itemControl.Height);
                }
            }

            // 调整控件大小
            int borderSize = owner.ShowDropDownBorder ? 2 : 0;

            if (listOrientation == FluentSplitButtonOrientation.Vertical)
            {
                // 统一宽度
                foreach (var ctrl in itemControls)
                {
                    ctrl.Width = maxWidth;
                }

                // 移除最后一个间距
                y -= itemSpacing;
                this.Size = new Size(maxWidth + borderSize, y + borderSize);
            }
            else
            {
                // 统一高度
                foreach (var ctrl in itemControls)
                {
                    ctrl.Height = maxHeight;
                }

                // 移除最后一个间距
                x -= itemSpacing;
                this.Size = new Size(x + borderSize, maxHeight + borderSize);
            }

            this.ResumeLayout(true);
        }

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

        }

        protected override void DrawBorder(Graphics g)
        {
            // 只在启用边框时绘制
            if (owner.ShowDropDownBorder)
            {
                var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
                using (var pen = new Pen(borderColor, 1))
                {
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }
        }
    }

    [ToolboxItem(false)]
    public class FluentSplitButtonItemControl : FluentControlBase
    {
        private FluentSplitButtonItem item;
        private FluentSplitButton rootOwner;

        private Rectangle contentRect;  // 图标文本区域
        private Rectangle iconRect;
        private Rectangle textRect;
        private Rectangle dropDownRect;  // 下拉箭头区域

        private bool isDropDownHot = false;
        private bool isContentHot = false;

        private FluentSplitButtonSubItemDropDown subDropDown;

        public event EventHandler ItemClicked;
        public event EventHandler ItemContentClicked;  // 点击内容区域
        public event EventHandler ItemDropDownClicked; // 点击下拉箭头

        public FluentSplitButtonItemControl(FluentSplitButtonItem item, FluentSplitButton rootOwner)
        {
            this.item = item;
            this.rootOwner = rootOwner;

            // 继承主题
            if (rootOwner.UseTheme)
            {
                this.InheritThemeFrom(rootOwner);
            }

            CalculateSize();
        }

        /// <summary>
        /// 判断当前项是否被按下
        /// </summary>
        private bool IsItemPressed
        {
            get
            {
                return rootOwner.EnablePressedState &&
                       rootOwner.IsPressed &&
                       rootOwner.PressedItem == item;
            }
        }

        /// <summary>
        /// 计算并设置控件大小
        /// </summary>
        private void CalculateSize()
        {
            var orientation = rootOwner.Orientation;
            var dropDownSize = rootOwner.DropDownSize;
            var padding = rootOwner.ContentPadding;

            // 计算内容大小
            Size contentSize = CalculateContentSize();

            int width, height;

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                width = contentSize.Width + padding * 2;
                height = contentSize.Height + padding * 2;

                if (item.HasItems && item.ItemType == FluentSplitButtonItemType.Button)
                {
                    width += dropDownSize;
                }

                height = Math.Max(height, 28); // 最小高度
            }
            else
            {
                width = contentSize.Width + padding * 2;
                height = contentSize.Height + padding * 2;

                if (item.HasItems && item.ItemType == FluentSplitButtonItemType.Button)
                {
                    height += dropDownSize;
                }

                width = Math.Max(width, 60); // 最小宽度
            }

            this.Size = new Size(width, height);

            // 计算布局
            CalculateLayout();
        }

        private Size CalculateContentSize()
        {
            bool hasIcon = item.Image != null && item.ItemType == FluentSplitButtonItemType.Button;
            bool hasText = !string.IsNullOrEmpty(item.Text);

            if (!hasIcon && !hasText)
            {
                return new Size(50, 20); // 默认大小
            }

            var iconSize = rootOwner.IconSize;
            var itemSpacing = rootOwner.ItemSpacing;
            var orientation = rootOwner.Orientation;

            int width = 0;
            int height = 0;

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                // 横向：宽度累加
                if (hasIcon)
                {
                    width += iconSize.Width;
                    height = Math.Max(height, iconSize.Height);
                }

                if (hasText)
                {
                    Size textSize = MeasureText();
                    if (hasIcon)
                    {
                        width += itemSpacing;
                    }

                    width += textSize.Width;
                    height = Math.Max(height, textSize.Height);
                }
            }
            else
            {
                // 纵向：高度累加
                if (hasIcon)
                {
                    width = Math.Max(width, iconSize.Width);
                    height += iconSize.Height;
                }

                if (hasText)
                {
                    Size textSize = MeasureText();
                    if (hasIcon)
                    {
                        height += itemSpacing;
                    }

                    width = Math.Max(width, textSize.Width);
                    height += textSize.Height;
                }
            }

            return new Size(width, height);
        }

        private Size MeasureText()
        {
            if (string.IsNullOrEmpty(item.Text))
            {
                return Size.Empty;
            }

            try
            {
                using (var g = CreateGraphics())
                {
                    var size = g.MeasureString(item.Text, this.Font);
                    return new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
                }
            }
            catch
            {
                return new Size(50, 20);
            }
        }

        private void CalculateLayout()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            var orientation = rootOwner.Orientation;
            var dropDownSize = rootOwner.DropDownSize;
            var hasDropDown = item.HasItems && item.ItemType == FluentSplitButtonItemType.Button;

            // 划分两个区域
            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                if (hasDropDown)
                {
                    dropDownRect = new Rectangle(Width - dropDownSize, 0, dropDownSize, Height);
                    contentRect = new Rectangle(0, 0, Width - dropDownSize, Height);
                }
                else
                {
                    dropDownRect = Rectangle.Empty;
                    contentRect = new Rectangle(0, 0, Width, Height);
                }
            }
            else
            {
                if (hasDropDown)
                {
                    dropDownRect = new Rectangle(0, Height - dropDownSize, Width, dropDownSize);
                    contentRect = new Rectangle(0, 0, Width, Height - dropDownSize);
                }
                else
                {
                    dropDownRect = Rectangle.Empty;
                    contentRect = new Rectangle(0, 0, Width, Height);
                }
            }

            // 在内容区域中布局图标和文本
            CalculateContentLayout();
        }

        private void CalculateContentLayout()
        {
            if (contentRect.IsEmpty)
            {
                return;
            }

            iconRect = Rectangle.Empty;
            textRect = Rectangle.Empty;

            bool hasIcon = item.Image != null && item.ItemType == FluentSplitButtonItemType.Button;
            bool hasText = !string.IsNullOrEmpty(item.Text);

            if (!hasIcon && !hasText)
            {
                return;
            }

            var iconSize = rootOwner.IconSize;
            var itemSpacing = rootOwner.ItemSpacing;
            var itemPosition = rootOwner.ItemPosition;
            var padding = rootOwner.ContentPadding;
            var orientation = rootOwner.Orientation;

            // 计算内容实际占用的大小
            Size contentSize = CalculateContentSize();

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                // 横向布局
                int totalWidth = contentSize.Width;
                int totalHeight = contentSize.Height;

                // 根据 ItemPosition 确定起始位置
                int startX = contentRect.X;
                switch (itemPosition)
                {
                    case FluentSplitButtonItemPosition.Start:
                        startX = contentRect.X + padding;
                        break;
                    case FluentSplitButtonItemPosition.Center:
                        startX = contentRect.X + (contentRect.Width - totalWidth) / 2;
                        break;
                    case FluentSplitButtonItemPosition.End:
                        startX = contentRect.Right - totalWidth - padding;
                        break;
                }

                int y = contentRect.Y + (contentRect.Height - totalHeight) / 2;

                // 布局图标
                if (hasIcon)
                {
                    iconRect = new Rectangle(startX, y + (totalHeight - iconSize.Height) / 2,
                        iconSize.Width, iconSize.Height);
                    startX += iconSize.Width;
                    if (hasText)
                    {
                        startX += itemSpacing;
                    }
                }

                // 布局文本
                if (hasText)
                {
                    Size textSize = MeasureText();
                    textRect = new Rectangle(startX, y + (totalHeight - textSize.Height) / 2,
                        textSize.Width, textSize.Height);
                }
            }
            else
            {
                // 纵向布局
                int totalWidth = contentSize.Width;
                int totalHeight = contentSize.Height;

                // 根据 ItemPosition 确定起始位置
                int startY = contentRect.Y;
                switch (itemPosition)
                {
                    case FluentSplitButtonItemPosition.Start:
                        startY = contentRect.Y + padding;
                        break;
                    case FluentSplitButtonItemPosition.Center:
                        startY = contentRect.Y + (contentRect.Height - totalHeight) / 2;
                        break;
                    case FluentSplitButtonItemPosition.End:
                        startY = contentRect.Bottom - totalHeight - padding;
                        break;
                }

                int x = contentRect.X + (contentRect.Width - totalWidth) / 2;

                // 布局图标
                if (hasIcon)
                {
                    iconRect = new Rectangle(x + (totalWidth - iconSize.Width) / 2, startY,
                        iconSize.Width, iconSize.Height);
                    startY += iconSize.Height;
                    if (hasText)
                    {
                        startY += itemSpacing;
                    }
                }

                // 布局文本
                if (hasText)
                {
                    Size textSize = MeasureText();
                    textRect = new Rectangle(x + (totalWidth - textSize.Width) / 2, startY,
                        textSize.Width, textSize.Height);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool newIsDropDownHot = !dropDownRect.IsEmpty && dropDownRect.Contains(e.Location);
            bool newIsContentHot = contentRect.Contains(e.Location);

            if (newIsDropDownHot != isDropDownHot || newIsContentHot != isContentHot)
            {
                isDropDownHot = newIsDropDownHot;
                isContentHot = newIsContentHot;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (isDropDownHot || isContentHot)
            {
                isDropDownHot = false;
                isContentHot = false;
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (!item.Enabled)
            {
                return;
            }

            var mousePos = PointToClient(MousePosition);

            // 检查是否点击下拉区域
            if (!dropDownRect.IsEmpty && dropDownRect.Contains(mousePos) && item.HasItems)
            {
                // 先显示子项面板
                ShowSubItemDropDown();

                // 然后触发事件(这时子面板已经在 openSubDropDowns 列表中了)
                ItemDropDownClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (item.ItemType == FluentSplitButtonItemType.Button)
            {
                // 触发按钮点击
                item.OnClick();

                // 触发内容点击事件
                ItemContentClicked?.Invoke(this, EventArgs.Empty);

                // 保持兼容性
                ItemClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (item.ItemType == FluentSplitButtonItemType.Label)
            {
                // 点击标签，不做任何操作
            }
        }

        private void ShowSubItemDropDown()
        {
            if (!item.HasItems)
            {
                return;
            }

            // 如果该项的子面板已经打开，关闭它
            if (subDropDown != null && !subDropDown.IsDisposed && subDropDown.Visible)
            {
                try
                {
                    subDropDown.Close();
                    subDropDown.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // 已释放，忽略
                }
                finally
                {
                    subDropDown = null;
                }
                return;
            }

            // 关闭该项之前打开的子面板(如果存在)
            if (subDropDown != null)
            {
                try
                {
                    if (!subDropDown.IsDisposed)
                    {
                        subDropDown.Close();
                        subDropDown.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 已释放，忽略
                }
                finally
                {
                    subDropDown = null;
                }
            }

            // 创建新的子项下拉面板
            subDropDown = new FluentSplitButtonSubItemDropDown(item, rootOwner);

            // 计算下拉位置
            var orientation = rootOwner.Orientation;
            Point location;

            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                location = this.PointToScreen(new Point(this.Width, 0));
            }
            else
            {
                location = this.PointToScreen(new Point(0, this.Height));
            }

            // 订阅关闭事件，清理引用
            subDropDown.Closed += (s, e) =>
            {
                try
                {
                    if (subDropDown != null && !subDropDown.IsDisposed)
                    {
                        subDropDown.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 已释放，忽略
                }
                finally
                {
                    subDropDown = null;
                }
            };

            try
            {
                subDropDown.Show(location);
            }
            catch (ObjectDisposedException)
            {
                // 对象在显示前被释放
                subDropDown = null;
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            Color bgColor;

            if (!item.Enabled)
            {
                bgColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.Control);
            }
            else if (IsItemPressed)  // 检查该项是否被按下
            {
                bgColor = GetThemeColor(c => c.SurfacePressed, SystemColors.ControlDark);
            }
            else if (State == ControlState.Pressed)
            {
                // 按下时稍微变暗
                bgColor = GetThemeColor(c => c.SurfacePressed, SystemColors.ControlDark);
            }
            else if (State == ControlState.Hover || isContentHot)
            {
                // 悬停时改变背景色(不显示边框)
                bgColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
            }
            else
            {
                bgColor = GetThemeColor(c => c.Surface, SystemColors.Window);
            }

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 绘制下拉区域高亮
            if (isDropDownHot && !dropDownRect.IsEmpty)
            {
                var hoverColor = GetThemeColor(c => c.SurfaceHover, SystemColors.ControlLight);
                var dropDownBgColor = AdjustBrightness(hoverColor, -0.05f);

                using (var brush = new SolidBrush(dropDownBgColor))
                {
                    g.FillRectangle(brush, dropDownRect);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            var orientation = rootOwner.Orientation;

            var fgColor = item.Enabled
                ? GetThemeColor(c => c.TextPrimary, SystemColors.ControlText)
                : GetThemeColor(c => c.TextDisabled, SystemColors.GrayText);

            // 绘制图标
            if (!iconRect.IsEmpty && item.Image != null)
            {
                try
                {
                    if (item.Enabled)
                    {
                        g.DrawImage(item.Image, iconRect);
                    }
                    else
                    {
                        ControlPaint.DrawImageDisabled(g, item.Image, iconRect.X, iconRect.Y, Color.Transparent);
                    }
                }
                catch { }
            }

            // 绘制文本
            if (!textRect.IsEmpty && !string.IsNullOrEmpty(item.Text))
            {
                var sf = new StringFormat
                {
                    Alignment = item.ItemType == FluentSplitButtonItemType.Label
                        ? StringAlignment.Center
                        : StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                using (var brush = new SolidBrush(fgColor))
                {
                    g.DrawString(item.Text, this.Font, brush, textRect, sf);
                }
            }

            // 绘制下拉分割线和箭头
            if (!dropDownRect.IsEmpty)
            {
                // 绘制分割线
                var separatorColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
                using (var pen = new Pen(separatorColor, 1))
                {
                    if (orientation == FluentSplitButtonOrientation.Horizontal)
                    {
                        int lineX = dropDownRect.X;
                        g.DrawLine(pen, lineX, 0, lineX, Height);
                    }
                    else
                    {
                        int lineY = dropDownRect.Y;
                        g.DrawLine(pen, 0, lineY, Width, lineY);
                    }
                }

                // 绘制箭头
                DrawDropDownArrow(g, dropDownRect, fgColor, orientation);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (IsItemPressed)
            {
                var borderColor = GetThemeColor(c => c.BorderFocused, SystemColors.Highlight);
                using (var pen = new Pen(borderColor, 1))
                {
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }
        }

        private void DrawDropDownArrow(Graphics g, Rectangle rect, Color color, FluentSplitButtonOrientation orientation)
        {
            var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            var arrowSize = 4;

            Point[] arrow;
            if (orientation == FluentSplitButtonOrientation.Horizontal)
            {
                // 向右箭头
                arrow = new Point[]
                {
                    new Point(center.X - arrowSize / 2, center.Y - arrowSize),
                    new Point(center.X + arrowSize / 2, center.Y),
                    new Point(center.X - arrowSize / 2, center.Y + arrowSize)
                };
            }
            else
            {
                // 向下箭头
                arrow = new Point[]
                {
                    new Point(center.X - arrowSize, center.Y - arrowSize / 2),
                    new Point(center.X + arrowSize, center.Y - arrowSize / 2),
                    new Point(center.X, center.Y + arrowSize / 2)
                };
            }

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (subDropDown != null)
                {
                    try
                    {
                        if (!subDropDown.IsDisposed)
                        {
                            subDropDown.Close();
                            subDropDown.Dispose();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // 已释放，忽略
                    }
                    finally
                    {
                        subDropDown = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 子项下拉面板
    /// </summary>
    [ToolboxItem(false)]
    public class FluentSplitButtonSubItemDropDown : ToolStripDropDown
    {
        private FluentSplitButtonItem parentItem;
        private FluentSplitButton rootOwner;
        private FluentSplitButtonSubItemPanel panel;

        // 嵌套层级
        private int nestingLevel = 0;

        // Windows 消息常量
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int WM_NCACTIVATE = 0x0086;
        private const int WM_ACTIVATE = 0x0006;
        private const int MA_NOACTIVATE = 0x0003;
        private const int WA_INACTIVE = 0;

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        public FluentSplitButtonSubItemDropDown(FluentSplitButtonItem parentItem, FluentSplitButton rootOwner)
        {
            this.parentItem = parentItem;
            this.rootOwner = rootOwner;

            this.AutoSize = true;
            this.Margin = Padding.Empty;
            this.Padding = new Padding(0);
            this.DropShadowEnabled = true;
            this.AutoClose = false;

            // 计算嵌套层级
            nestingLevel = CalculateNestingLevel(parentItem);

            panel = new FluentSplitButtonSubItemPanel(parentItem, rootOwner, this);
            var host = new ToolStripControlHost(panel)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = true
            };

            Items.Add(host);

            // 注册到主控件
            if (rootOwner != null && !rootOwner.IsDisposed)
            {
                rootOwner.RegisterSubDropDown(this);
            }
        }

        public FluentSplitButtonItem ParentItem => parentItem;


        /// <summary>
        /// 计算当前项的嵌套层级
        /// </summary>
        private int CalculateNestingLevel(FluentSplitButtonItem item)
        {
            int level = 1; // 从1开始，因为是子项面板
            var current = item.Parent;
            while (current != null)
            {
                level++;
                current = current.Parent;
            }
            return level;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE - 防止窗口激活时改变焦点
                cp.ExStyle |= 0x08000000;

                // 对于深层嵌套，添加额外的样式
                if (nestingLevel >= 3)
                {
                    // WS_EX_TOOLWINDOW - 防止在任务栏显示，减少焦点影响
                    cp.ExStyle |= 0x00000080;
                }

                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEACTIVATE:
                    // 阻止鼠标激活改变焦点
                    m.Result = (IntPtr)MA_NOACTIVATE;
                    return;

                case WM_NCACTIVATE:
                    // 阻止非客户区激活，但保持视觉效果
                    m.WParam = (IntPtr)1;
                    break;

                case WM_ACTIVATE:
                    // 对于深层嵌套，完全阻止激活消息
                    if (nestingLevel >= 3)
                    {
                        m.WParam = (IntPtr)WA_INACTIVE;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        public new void Show(Point screenLocation)
        {
            // 保存当前活动窗口
            IntPtr currentActiveWindow = GetActiveWindow();

            // 显示下拉面板
            base.Show(screenLocation);

            // 如果是深层嵌套，立即恢复焦点
            if (nestingLevel >= 3 && currentActiveWindow != IntPtr.Zero)
            {
                SetActiveWindow(currentActiveWindow);
            }
        }

        // 重写 Close 方法，确保不影响焦点
        public new void Close()
        {
            // 保存当前活动窗口
            IntPtr currentActiveWindow = GetActiveWindow();

            // 关闭面板
            base.Close();

            // 如果是深层嵌套，恢复焦点
            if (nestingLevel >= 3 && currentActiveWindow != IntPtr.Zero)
            {
                // 延迟恢复，确保关闭完成
                if (rootOwner != null && !rootOwner.IsDisposed)
                {
                    rootOwner.BeginInvoke(new Action(() =>
                    {
                        SetActiveWindow(currentActiveWindow);
                    }));
                }
            }
        }

        public void RequestClose(FluentSplitButtonItem clickedItem, bool isDropDownArrowClicked)
        {
            if (isDropDownArrowClicked)
            {
                return;
            }

            if (clickedItem.ItemType == FluentSplitButtonItemType.Label)
            {
                return;
            }

            if (clickedItem.HasItems)
            {
                return;
            }

            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed && Visible)
                    {
                        Close();
                    }
                }));
            }
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }

    /// <summary>
    /// 子项面板
    /// </summary>
    [ToolboxItem(false)]
    public class FluentSplitButtonSubItemPanel : FluentContainerBase
    {
        private FluentSplitButtonItem parentItem;
        private FluentSplitButton rootOwner;
        private FluentSplitButtonSubItemDropDown parentDropDown;
        private List<FluentSplitButtonItemControl> itemControls = new List<FluentSplitButtonItemControl>();
        private int itemSpacing = 2;

        public FluentSplitButtonSubItemPanel(FluentSplitButtonItem parentItem, FluentSplitButton rootOwner, FluentSplitButtonSubItemDropDown parentDropDown = null)
        {
            this.parentItem = parentItem;
            this.rootOwner = rootOwner;
            this.parentDropDown = parentDropDown;
            this.AutoSize = true;
            this.Padding = new Padding(0);

            UpdateItems();
        }

        private void UpdateItems()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            itemControls.Clear();

            if (parentItem.Items.Count == 0)
            {
                this.Size = new Size(100, 50);
                this.ResumeLayout();
                return;
            }

            var listOrientation = rootOwner.ActualDropDownOrientation;

            int x = rootOwner.ShowDropDownBorder ? 1 : 0;
            int y = rootOwner.ShowDropDownBorder ? 1 : 0;
            int maxWidth = 0;
            int maxHeight = 0;

            foreach (FluentSplitButtonItem item in parentItem.Items)
            {
                var itemControl = new FluentSplitButtonItemControl(item, rootOwner);

                itemControl.ItemContentClicked += (s, e) =>
                {
                    // 通知父下拉面板
                    //parentDropDown?.RequestClose(item, false);

                    // 关闭其他子面板
                    //rootOwner.CloseAllSubDropDowns();

                    // 通知主控件
                    rootOwner.OnItemClicked(item);
                };

                itemControl.ItemDropDownClicked += (s, e) =>
                {
                    rootOwner.CloseOtherSubDropDowns(item);
                };

                //itemControl.ItemClicked += (s, e) =>
                //{
                //    // 关闭所有下拉面板
                //    var parent = this.Parent;
                //    while (parent != null)
                //    {
                //        if (parent is ToolStripDropDown dropdown)
                //        {
                //            dropdown.Close();
                //        }
                //        parent = parent.Parent;
                //    }

                //    rootOwner.OnItemClicked(item);
                //};

                itemControls.Add(itemControl);
                this.Controls.Add(itemControl);

                if (listOrientation == FluentSplitButtonOrientation.Vertical)
                {
                    itemControl.Location = new Point(x, y);
                    y += itemControl.Height + itemSpacing;
                    maxWidth = Math.Max(maxWidth, itemControl.Width);
                }
                else
                {
                    itemControl.Location = new Point(x, y);
                    x += itemControl.Width + itemSpacing;
                    maxHeight = Math.Max(maxHeight, itemControl.Height);
                }
            }

            int borderSize = rootOwner.ShowDropDownBorder ? 2 : 0;

            if (listOrientation == FluentSplitButtonOrientation.Vertical)
            {
                foreach (var ctrl in itemControls)
                {
                    ctrl.Width = maxWidth;
                }

                y -= itemSpacing;
                this.Size = new Size(maxWidth + borderSize, y + borderSize);
            }
            else
            {
                foreach (var ctrl in itemControls)
                {
                    ctrl.Height = maxHeight;
                }

                x -= itemSpacing;
                this.Size = new Size(x + borderSize, maxHeight + borderSize);
            }

            this.ResumeLayout(true);
        }

        /// <summary>
        /// 关闭所有父级下拉面板
        /// </summary>
        private void CloseAllParentDropDowns()
        {
            var parent = this.Parent;
            while (parent != null)
            {
                if (parent is ToolStripDropDown dropdown)
                {
                    dropdown.Close();
                }
                parent = parent.Parent;
            }
        }

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
        }

        protected override void DrawBorder(Graphics g)
        {
            if (rootOwner.ShowDropDownBorder)
            {
                var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
                using (var pen = new Pen(borderColor, 1))
                {
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 按钮项图标文本区布局模式
    /// </summary>
    public enum FluentSplitButtonItemPosition
    {
        Start,      // 靠左/靠上
        Center,
        End         // 靠右/靠下
    }

    /// <summary>
    /// 分割按钮方向
    /// </summary>
    public enum FluentSplitButtonOrientation
    {
        Horizontal,  // 横向排列
        Vertical     // 纵向排列
    }

    /// <summary>
    /// 分割按钮项类型
    /// </summary>
    public enum FluentSplitButtonItemType
    {
        Button,  // 按钮项
        Label    // 标签项
    }

    #endregion

    #region 设计时支持

    public class FluentSplitButtonDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private ISelectionService selectionService;
        private IComponentChangeService changeService;
        private Adorner adorner;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // 获取设计时服务
            selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;

            // 监听属性更改
            if (changeService != null)
            {
                changeService.ComponentChanged += OnComponentChanged;
                changeService.ComponentAdded += OnComponentChanged;
                changeService.ComponentRemoved += OnComponentChanged;
            }

            // 确保Items集合初始化
            var button = component as FluentSplitButton;
            if (button != null && button.Items == null)
            {
                // 强制初始化Items
                var items = button.Items;
            }

            // 启用拖放
            EnableDragDrop(true);

            // 自动刷新
            AutoResizeHandles = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (changeService != null)
                {
                    changeService.ComponentChanged -= OnComponentChanged;
                    changeService.ComponentAdded -= OnComponentChanged;
                    changeService.ComponentRemoved -= OnComponentChanged;
                }
            }
            base.Dispose(disposing);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentSplitButtonActionList(this.Component as FluentSplitButton));
                }
                return actionLists;
            }
        }

        public override SelectionRules SelectionRules
        {
            get
            {
                return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.AllSizeable;
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            var button = Control as FluentSplitButton;
            if (button != null)
            {
                // 设置初始文本
                button.Text = button.Name;
                button.Size = new Size(120, 32);

                // 通知设计器
                var propertyDescriptor = TypeDescriptor.GetProperties(button)["Text"];
                changeService?.OnComponentChanged(button, propertyDescriptor, null, button.Text);
            }
        }

        private void OnComponentChanged(object sender, EventArgs e)
        {
            // 强制刷新设计器
            this.Control?.Invalidate();
            this.Control?.Update();
        }


        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);

            // 绘制设计时边框
            var button = Control as FluentSplitButton;
            if (button != null && selectionService != null &&
                selectionService.GetComponentSelected(button))
            {
                using (var pen = new Pen(SystemColors.Highlight, 1))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    var rect = new Rectangle(0, 0, button.Width - 1, button.Height - 1);
                    pe.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        // 过滤属性
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 确保Items属性可见且可编辑
            if (properties.Contains("Items"))
            {
                var prop = properties["Items"] as PropertyDescriptor;
                if (prop != null)
                {
                    properties["Items"] = TypeDescriptor.CreateProperty(
                        typeof(FluentSplitButton),
                        prop,
                        new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content),
                        new CategoryAttribute("Fluent"),
                        new DescriptionAttribute("按钮项集合"));
                }
            }
        }
    }

    /// <summary>
    /// 设计器快捷操作列表
    /// </summary>
    public class FluentSplitButtonActionList : DesignerActionList
    {
        private FluentSplitButton splitButton;
        private DesignerActionUIService designerActionService;
        private IDesignerHost designerHost;

        public FluentSplitButtonActionList(FluentSplitButton component) : base(component)
        {
            this.splitButton = component;
            this.designerActionService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
            this.designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 添加标题
            items.Add(new DesignerActionHeaderItem("常用操作"));
            items.Add(new DesignerActionPropertyItem("Text", "文本:", "常用操作", "设置按钮显示的文本"));
            items.Add(new DesignerActionPropertyItem("Orientation", "排列方向:", "常用操作", "设置按钮的排列方向"));
            items.Add(new DesignerActionPropertyItem("ShowIcon", "显示图标", "常用操作", "是否显示图标"));
            items.Add(new DesignerActionPropertyItem("ShowText", "显示文本", "常用操作", "是否显示文本"));

            // 添加方法项
            items.Add(new DesignerActionHeaderItem("编辑"));
            items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑项集合...", "编辑", "编辑按钮项集合", true));

            // 添加主题相关
            items.Add(new DesignerActionHeaderItem("主题"));
            items.Add(new DesignerActionPropertyItem("UseTheme", "使用主题", "主题", "是否使用Fluent主题"));
            items.Add(new DesignerActionPropertyItem("ThemeName", "主题名称:", "主题", "选择要使用的主题"));

            return items;
        }

        #region 属性

        public string Text
        {
            get => splitButton.Text;
            set
            {
                SetProperty("Text", value);
            }
        }

        public FluentSplitButtonOrientation Orientation
        {
            get => splitButton.Orientation;
            set
            {
                SetProperty("Orientation", value);
            }
        }

        public bool ShowIcon
        {
            get => splitButton.ShowIcon;
            set
            {
                SetProperty("ShowIcon", value);
            }
        }

        public bool ShowText
        {
            get => splitButton.ShowText;
            set
            {
                SetProperty("ShowText", value);
            }
        }

        public bool UseTheme
        {
            get => splitButton.UseTheme;
            set
            {
                SetProperty("UseTheme", value);
            }
        }

        public string ThemeName
        {
            get => splitButton.ThemeName;
            set
            {
                SetProperty("ThemeName", value);
            }
        }

        #endregion

        #region 方法

        public void EditItems()
        {
            // 获取Items属性的编辑器
            var propertyDescriptor = TypeDescriptor.GetProperties(splitButton)["Items"];
            if (propertyDescriptor != null)
            {
                var editor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;

                if (editor != null)
                {
                    var context = new TypeDescriptorContext(splitButton, propertyDescriptor, designerHost);
                    editor.EditValue(context, context, splitButton.Items);
                }
            }
        }

        #endregion

        #region 辅助方法

        private void SetProperty(string propertyName, object value)
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(splitButton)[propertyName];
            if (propertyDescriptor != null)
            {
                var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;

                try
                {
                    changeService?.OnComponentChanging(splitButton, propertyDescriptor);
                    propertyDescriptor.SetValue(splitButton, value);
                    changeService?.OnComponentChanged(splitButton, propertyDescriptor, null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"设置属性 {propertyName} 时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }

    internal class FluentSplitButtonItemCollectionEditor : CollectionEditor
    {
        public FluentSplitButtonItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentSplitButtonItem);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(FluentSplitButtonItem)
            };
        }

        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(FluentSplitButtonItem))
            {
                return new FluentSplitButtonItem("Item" + (Context?.Instance is FluentSplitButton btn ? btn.Items.Count + 1 : 1));
            }
            return base.CreateInstance(itemType);
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentSplitButtonItem item)
            {
                string typePrefix = item.ItemType == FluentSplitButtonItemType.Label ? "[标签] " : "";
                string text = string.IsNullOrEmpty(item.Text) ? "(未命名)" : item.Text;
                string childrenInfo = item.HasItems ? $" ({item.Items.Count}项)" : "";
                return $"{typePrefix}{text}{childrenInfo}";
            }
            return base.GetDisplayText(value);
        }

        protected override bool CanRemoveInstance(object value)
        {
            return true;
        }

        protected override bool CanSelectMultipleInstances()
        {
            return true;
        }
    }

    #endregion


}
