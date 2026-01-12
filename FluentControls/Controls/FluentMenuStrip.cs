using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using FluentControls.Themes;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.Security.Permissions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FluentControls.Controls
{

    [ToolboxBitmap(typeof(FluentMenuStrip))]
    [Designer(typeof(FluentMenuStripDesigner))]
    public class FluentMenuStrip : FluentControlBase, IFluentItemContainer
    {
        private FluentMenuItemCollection items;
        private FluentMenuItem hoveredItem;
        private FluentMenuItem activeDropDownItem; // 当前活动的下拉菜单项

        private int itemHeight = 28;
        private Color menuBackColor = Color.Empty;
        private Color menuForeColor = Color.Empty;
        private Font menuFont;

        // 下拉菜单
        private Color dropDownBorderColor = Color.Empty;
        private int dropDownBorderWidth = 1;
        private bool showDropDownBorder = true;


        public FluentMenuStrip()
        {
            items = new FluentMenuItemCollection(null);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            // 订阅集合变更
            items.CollectionChanged += Items_CollectionChanged;


            int defaultHeight = itemHeight + Padding.Vertical;
            Height = defaultHeight;
            MinimumSize = new Size(0, defaultHeight);
            Dock = DockStyle.Top;
        }

        #region 属性

        /// <summary>
        /// 菜单项集合
        /// </summary>
        [Category("Data")]
        [Description("菜单项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentMenuItemCollectionEditor), typeof(UITypeEditor))]
        public FluentMenuItemCollection Items => items;

        /// <summary>
        /// 菜单背景色
        /// </summary>
        [Category("Appearance")]
        [Description("菜单栏背景颜色")]
        public Color MenuBackColor
        {
            get => menuBackColor;
            set
            {
                if (menuBackColor != value)
                {
                    menuBackColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 菜单前景色
        /// </summary>
        [Category("Appearance")]
        [Description("菜单栏前景颜色")]
        public Color MenuForeColor
        {
            get => menuForeColor;
            set
            {
                if (menuForeColor != value)
                {
                    menuForeColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 菜单字体
        /// </summary>
        [Category("Appearance")]
        [Description("菜单项字体")]
        public Font MenuFont
        {
            get => menuFont ?? Font;
            set
            {
                if (menuFont != value)
                {
                    menuFont = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否有任何下拉菜单打开
        /// </summary>
        [Browsable(false)]
        public bool IsAnyDropDownOpen
        {
            get
            {
                foreach (var item in items)
                {
                    if (item.IsDropDownOpen)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 菜单项高度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(28)]
        [Description("菜单项高度")]
        public int ItemHeight
        {
            get => itemHeight;
            set
            {
                if (itemHeight != value && value > 0)
                {
                    itemHeight = value;
                    Height = itemHeight + Padding.Vertical;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 下拉菜单边框颜色
        /// </summary>
        [Category("下拉菜单")]
        [Description("下拉菜单的边框颜色")]
        public Color DropDownBorderColor
        {
            get => dropDownBorderColor;
            set
            {
                if (dropDownBorderColor != value)
                {
                    dropDownBorderColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 下拉菜单边框宽度
        /// </summary>
        [Category("下拉菜单")]
        [DefaultValue(1)]
        [Description("下拉菜单的边框宽度")]
        public int DropDownBorderWidth
        {
            get => dropDownBorderWidth;
            set
            {
                if (dropDownBorderWidth != value && value >= 0)
                {
                    dropDownBorderWidth = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示下拉菜单边框
        /// </summary>
        [Category("下拉菜单")]
        [DefaultValue(true)]
        [Description("是否显示下拉菜单的边框")]
        public bool ShowDropDownBorder
        {
            get => showDropDownBorder;
            set
            {
                if (showDropDownBorder != value)
                {
                    showDropDownBorder = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 关闭所有下拉菜单
        /// </summary>
        public void CloseAllDropDowns()
        {
            foreach (var item in items)
            {
                item.HideDropDown();
            }

            // 重置活动项
            if (activeDropDownItem != null)
            {
                activeDropDownItem = null;
            }
        }

        /// <summary>
        /// 添加菜单项
        /// </summary>
        public FluentMenuItem AddMenuItem(string text)
        {
            var item = new FluentMenuItem(text);
            items.Add(item);
            return item;
        }

        /// <summary>
        /// 添加菜单项
        /// </summary>
        public FluentMenuItem AddMenuItem(string text, EventHandler onClick)
        {
            var item = new FluentMenuItem(text, onClick);
            items.Add(item);
            return item;
        }

        private void Items_CollectionChanged(object sender, EventArgs e)
        {
            foreach (var item in items)
            {
                if (item.Owner != this)
                {
                    item.Owner = this;
                }
            }

            // 集合变更时重新布局
            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// 设置活动的下拉菜单项
        /// </summary>
        internal void SetActiveDropDownItem(FluentMenuItem item)
        {
            if (activeDropDownItem != item)
            {
                // 关闭之前的下拉菜单并重置其状态
                if (activeDropDownItem != null)
                {
                    activeDropDownItem.HideDropDown();
                    activeDropDownItem.ResetMouseOverState();
                }

                activeDropDownItem = item;
            }
        }

        /// <summary>
        /// 获取当前活动的下拉菜单项
        /// </summary>
        internal FluentMenuItem GetActiveDropDownItem()
        {
            return activeDropDownItem;
        }

        private bool ShouldSerializeDropDownBorderColor() => dropDownBorderColor != Color.Empty;
        private void ResetDropDownBorderColor() => DropDownBorderColor = Color.Empty;

        #endregion

        #region 布局

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            PerformMenuLayout();
        }

        private void PerformMenuLayout()
        {
            int x = Padding.Left;
            int y = Padding.Top;

            foreach (var item in items)
            {
                if (!item.Visible)
                {
                    item.Bounds = Rectangle.Empty;
                    continue;
                }

                item.IsTopLevel = true;
                item.Owner = this;

                var itemSize = item.GetPreferredSize();
                item.Bounds = new Rectangle(x, y, itemSize.Width, itemHeight);

                x += itemSize.Width;
            }
        }

        public new void PerformLayout()
        {
            PerformMenuLayout();
            Invalidate();
        }

        #endregion

        #region 绘制

        private bool ShouldSerializeMenuBackColor()
        {
            return menuBackColor != Color.Empty;
        }

        private void ResetMenuBackColor()
        {
            MenuBackColor = Color.Empty;
        }

        private bool ShouldSerializeMenuForeColor()
        {
            return menuForeColor != Color.Empty;
        }

        private void ResetMenuForeColor()
        {
            MenuForeColor = Color.Empty;
        }

        private bool ShouldSerializeMenuFont()
        {
            return menuFont != null;
        }

        private void ResetMenuFont()
        {
            MenuFont = null;
        }

        protected override void DrawBackground(Graphics g)
        {
            Color backColor = menuBackColor;

            if (backColor == Color.Empty)
            {
                if (UseTheme && Theme != null)
                {
                    backColor = GetThemeColor(c => c.Surface, SystemColors.MenuBar);
                }
                else
                {
                    backColor = BackColor != Color.Transparent ? BackColor : SystemColors.MenuBar;
                }
            }

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            foreach (var item in items)
            {
                if (!item.Visible || item.Bounds.IsEmpty)
                {
                    continue;
                }

                var paintArgs = new PaintEventArgs(g, item.Bounds);
                item.OnPaint(paintArgs);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            Color borderColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.Border, SystemColors.ControlDark)
                : SystemColors.ControlDark;

            using (var pen = new Pen(borderColor))
            {
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }
        }

        #endregion

        #region 鼠标处理

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            FluentMenuItem newHoveredItem = null;

            foreach (var item in items)
            {
                if (item.Visible && item.Bounds.Contains(e.Location))
                {
                    newHoveredItem = item;
                    break;
                }
            }

            if (hoveredItem != newHoveredItem)
            {
                if (hoveredItem != null)
                {
                    hoveredItem.OnMouseLeave(EventArgs.Empty);
                }

                hoveredItem = newHoveredItem;

                if (hoveredItem != null)
                {
                    hoveredItem.OnMouseEnter(EventArgs.Empty);
                }

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            // 只有在没有打开的下拉菜单时才重置悬停项
            if (hoveredItem != null && !IsAnyDropDownOpen)
            {
                hoveredItem.OnMouseLeave(EventArgs.Empty);
                hoveredItem = null;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            foreach (var item in items)
            {
                if (item.Visible && item.Enabled && item.Bounds.Contains(e.Location))
                {
                    item.OnClick(EventArgs.Empty);
                    break;
                }
            }
        }

        #endregion

        #region 接口实现

        public void ItemStateChanged(FluentToolStripItem item)
        {
            Invalidate();
        }

        #endregion

        #region 主题

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                if (menuBackColor == Color.Empty)
                {
                    BackColor = GetThemeColor(c => c.Surface, SystemColors.MenuBar);
                }

                if (menuForeColor == Color.Empty)
                {
                    ForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.MenuText);
                }

                if (menuFont == null)
                {
                    Font = GetThemeFont(t => t.Body, SystemFonts.MenuFont);
                }
            }
        }

        public new Color GetThemeColor(Func<IColorPalette, Color> selector, Color defaultColor)
        {
            return base.GetThemeColor(selector, defaultColor);
        }

        public new Font GetThemeFont(Func<ITypography, Font> selector, Font defaultFont)
        {
            return base.GetThemeFont(selector, defaultFont);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseAllDropDowns();
                items?.Clear();
            }
            base.Dispose(disposing);
        }
    }

    #region 菜单项

    public class FluentMenuItem : FluentToolStripItem
    {
        private Keys shortcutKeys = Keys.None;
        private bool showShortcutKeys = true;
        private bool checkOnClick = false;
        private bool isChecked = false;
        private CheckStyle checkStyle = CheckStyle.CheckMark;
        private FluentMenuItemCollection dropDownItems;
        private FluentMenuDropDown dropDown;

        private bool isTopLevel = false;
        private bool isDropDownOpen = false;
        private int itemWidth = 0; // 0表示自动计算

        // 新增颜色属性
        private Color defaultForeColor = Color.Empty;
        private Color defaultBackColor = Color.Empty;
        private Color hoverForeColor = Color.Empty;
        private Color hoverBackColor = Color.Empty;
        private Color checkedBackColor = Color.Empty;
        private Font itemFont;

        // 状态
        private bool isMouseOver = false;

        // 事件
        public event EventHandler CheckedChanged;
        public event EventHandler DropDownOpening;
        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;

        public FluentMenuItem()
        {
            dropDownItems = new FluentMenuItemCollection(this);
            Padding = new Padding(8, 4, 8, 4);

            dropDownItems.CollectionChanged += DropDownItems_CollectionChanged;
        }

        public FluentMenuItem(string text) : this()
        {
            Text = text;
        }

        public FluentMenuItem(string text, Image image) : this(text)
        {
            Image = image;
        }

        public FluentMenuItem(string text, EventHandler onClick) : this(text)
        {
            if (onClick != null)
            {
                Click += onClick;
            }
        }

        #region 属性

        /// <summary>
        /// 默认前景色
        /// </summary>
        [Category("Appearance")]
        [Description("菜单项默认前景颜色")]
        public Color DefaultForeColor
        {
            get => defaultForeColor;
            set
            {
                if (defaultForeColor != value)
                {
                    defaultForeColor = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 默认背景色
        /// </summary>
        [Category("Appearance")]
        [Description("菜单项默认背景颜色")]
        public Color DefaultBackColor
        {
            get => defaultBackColor;
            set
            {
                if (defaultBackColor != value)
                {
                    defaultBackColor = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 悬停前景色
        /// </summary>
        [Category("Appearance")]
        [Description("鼠标悬停时的前景颜色")]
        public Color HoverForeColor
        {
            get => hoverForeColor;
            set
            {
                if (hoverForeColor != value)
                {
                    hoverForeColor = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 悬停背景色
        /// </summary>
        [Category("Appearance")]
        [Description("鼠标悬停时的背景颜色")]
        public Color HoverBackColor
        {
            get => hoverBackColor;
            set
            {
                if (hoverBackColor != value)
                {
                    hoverBackColor = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 勾选时的背景色
        /// </summary>
        [Category("Appearance")]
        [Description("勾选时的背景颜色")]
        public Color CheckedBackColor
        {
            get => checkedBackColor;
            set
            {
                if (checkedBackColor != value)
                {
                    checkedBackColor = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 菜单项字体
        /// </summary>
        [Category("Appearance")]
        [Description("菜单项字体")]
        public Font ItemFont
        {
            get => itemFont;
            set
            {
                if (itemFont != value)
                {
                    itemFont = value;
                    InvalidateItem();
                }
            }
        }

        [Browsable(false)]
        public bool IsSeparator => Text == "-";

        [Category("Appearance")]
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;

                // 如果是分隔符, 调整外观
                if (IsSeparator)
                {
                    Enabled = false;
                }
            }
        }

        /// <summary>
        /// 自定义宽度(0为自动)
        /// </summary>
        [Category("Layout")]
        [DefaultValue(0)]
        [Description("菜单项的自定义宽度(0为自动计算) ")]
        public int ItemWidth
        {
            get => itemWidth;
            set
            {
                if (itemWidth != value && value >= 0)
                {
                    itemWidth = value;
                    InvalidateItem();
                }
            }
        }

        private bool ShouldSerializeDefaultForeColor() => defaultForeColor != Color.Empty;
        private void ResetDefaultForeColor() => DefaultForeColor = Color.Empty;

        private bool ShouldSerializeDefaultBackColor() => defaultBackColor != Color.Empty;
        private void ResetDefaultBackColor() => DefaultBackColor = Color.Empty;

        private bool ShouldSerializeHoverForeColor() => hoverForeColor != Color.Empty;
        private void ResetHoverForeColor() => HoverForeColor = Color.Empty;

        private bool ShouldSerializeHoverBackColor() => hoverBackColor != Color.Empty;
        private void ResetHoverBackColor() => HoverBackColor = Color.Empty;

        private bool ShouldSerializeCheckedBackColor() => checkedBackColor != Color.Empty;
        private void ResetCheckedBackColor() => CheckedBackColor = Color.Empty;

        private bool ShouldSerializeItemFont() => itemFont != null;
        private void ResetItemFont() => ItemFont = null;

        #endregion

        #region 基类属性

        [Category("Behavior")]
        [DefaultValue(Keys.None)]
        [Description("菜单项快捷键")]
        public Keys ShortcutKeys
        {
            get => shortcutKeys;
            set
            {
                if (shortcutKeys != value)
                {
                    shortcutKeys = value;
                    InvalidateItem();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否显示快捷键文本")]
        public bool ShowShortcutKeys
        {
            get => showShortcutKeys;
            set
            {
                if (showShortcutKeys != value)
                {
                    showShortcutKeys = value;
                    InvalidateItem();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("点击时是否切换勾选状态")]
        public bool CheckOnClick
        {
            get => checkOnClick;
            set => checkOnClick = value;
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("是否勾选")]
        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnCheckedChanged(EventArgs.Empty);
                    InvalidateItem();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(CheckStyle.CheckMark)]
        [Description("勾选标记样式")]
        public CheckStyle CheckStyle
        {
            get => checkStyle;
            set
            {
                if (checkStyle != value)
                {
                    checkStyle = value;
                    InvalidateItem();
                }
            }
        }

        [Category("Data")]
        [Description("下拉菜单项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentMenuItemCollection DropDownItems => dropDownItems;

        [Browsable(false)]
        public bool HasDropDownItems => dropDownItems != null && dropDownItems.Count > 0;

        [Browsable(false)]
        public bool IsTopLevel
        {
            get => isTopLevel;
            internal set => isTopLevel = value;
        }

        [Browsable(false)]
        public bool IsDropDownOpen => isDropDownOpen;

        #endregion

        #region 事件

        private void DropDownItems_CollectionChanged(object sender, EventArgs e)
        {
            // 确保所有子项的Owner正确设置
            if (Owner != null)
            {
                foreach (var item in dropDownItems)
                {
                    if (item.Owner != Owner)
                    {
                        item.Owner = Owner;
                    }
                }
            }

            InvalidateItem();
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        protected virtual void OnDropDownOpening(EventArgs e)
        {
            DropDownOpening?.Invoke(this, e);
        }

        protected virtual void OnDropDownOpened(EventArgs e)
        {
            DropDownOpened?.Invoke(this, e);
        }

        protected virtual void OnDropDownClosed(EventArgs e)
        {
            DropDownClosed?.Invoke(this, e);
        }

        #endregion

        #region 方法

        public void ShowDropDown()
        {
            if (!HasDropDownItems || isDropDownOpen)
            {
                return;
            }

            OnDropDownOpening(EventArgs.Empty);

            // 通知MenuStrip设置当前活动项
            if (Owner is FluentMenuStrip menuStrip)
            {
                menuStrip.SetActiveDropDownItem(this);
            }

            if (dropDown == null)
            {
                dropDown = new FluentMenuDropDown(this);
                dropDown.Closed += DropDown_Closed;
            }

            // 边框设置
            if (Owner is FluentMenuStrip menuStrip2)
            {
                dropDown.BorderColor = menuStrip2.DropDownBorderColor;
                dropDown.BorderWidth = menuStrip2.DropDownBorderWidth;
                dropDown.ShowBorder = menuStrip2.ShowDropDownBorder;
            }

            Point location;
            if (IsTopLevel)
            {
                location = Owner.PointToScreen(new Point(Bounds.Left, Bounds.Bottom));
            }
            else
            {
                var parent = Owner as FluentMenuDropDown;
                if (parent != null)
                {
                    location = parent.PointToScreen(new Point(Bounds.Right, Bounds.Top));
                }
                else
                {
                    location = Owner.PointToScreen(new Point(Bounds.Right, Bounds.Top));
                }
            }

            dropDown.Show(location);
            isDropDownOpen = true;
            OnDropDownOpened(EventArgs.Empty);
        }

        public void HideDropDown()
        {
            if (dropDown != null && isDropDownOpen)
            {
                dropDown.Close();
            }
        }

        public string GetShortcutKeyText()
        {
            if (shortcutKeys == Keys.None)
            {
                return string.Empty;
            }

            var converter = new KeysConverter();
            return converter.ConvertToString(shortcutKeys);
        }

        /// <summary>
        /// 重置鼠标悬停状态
        /// </summary>
        internal void ResetMouseOverState()
        {
            if (isMouseOver)
            {
                isMouseOver = false;
                InvalidateItem();
            }
        }

        #endregion

        #region 绘制

        protected override void DrawItem(Graphics g)
        {
            if (Bounds.IsEmpty)
            {
                return;
            }

            // 如果是分隔符, 绘制分隔线
            if (IsSeparator)
            {
                DrawSeparator(g);
                return;
            }

            // 正常绘制
            var bounds = Bounds;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawBackground(g, bounds);
            DrawCheckMark(g, bounds);

            if (Image != null)
            {
                DrawImage(g, bounds);
            }

            DrawText(g, bounds);

            if (!IsTopLevel && ShowShortcutKeys && ShortcutKeys != Keys.None)
            {
                DrawShortcutKey(g, bounds);
            }

            if (!IsTopLevel && HasDropDownItems)
            {
                DrawArrow(g, bounds);
            }
        }

        private void DrawSeparator(Graphics g)
        {
            Color lineColor = Owner?.UseTheme == true && Owner.Theme != null
                ? (Owner as FluentMenuStrip).GetThemeColor(c => c.Border, SystemColors.ControlDark)
                : SystemColors.ControlDark;

            using (var pen = new Pen(lineColor, 1))
            {
                int y = Bounds.Y + Bounds.Height / 2;
                g.DrawLine(pen, Bounds.X + 4, y, Bounds.Right - 4, y);
            }
        }

        private void DrawBackground(Graphics g, Rectangle bounds)
        {
            Color backColor = Color.Transparent;

            if (!Enabled)
            {
                backColor = defaultBackColor != Color.Empty ?
                    Color.FromArgb(100, defaultBackColor) : Color.Transparent;
            }
            else if (isDropDownOpen)
            {
                // 下拉菜单打开时, 使用更明显的高亮
                if (hoverBackColor != Color.Empty)
                {
                    backColor = hoverBackColor;
                }
                else if (Owner?.UseTheme == true && Owner.Theme != null)
                {
                    var primaryColor = (Owner as FluentMenuStrip).GetThemeColor(c => c.Primary, SystemColors.Highlight);
                    backColor = Color.FromArgb(50, primaryColor);
                }
                else
                {
                    backColor = Color.FromArgb(50, SystemColors.Highlight);
                }
            }
            else if (isMouseOver)
            {
                // 仅悬停时, 使用较浅的高亮
                if (hoverBackColor != Color.Empty)
                {
                    backColor = hoverBackColor;
                }
                else if (Owner?.UseTheme == true && Owner.Theme != null)
                {
                    var primaryColor = (Owner as FluentMenuStrip).GetThemeColor(c => c.Primary, SystemColors.Highlight);
                    backColor = Color.FromArgb(40, primaryColor);
                }
                else
                {
                    backColor = Color.FromArgb(40, SystemColors.Highlight);
                }
            }
            else if (Checked)
            {
                if (checkedBackColor != Color.Empty)
                {
                    backColor = checkedBackColor;
                }
                else if (Owner?.UseTheme == true && Owner.Theme != null)
                {
                    var accentColor = (Owner as FluentMenuStrip).GetThemeColor(c => c.Accent, SystemColors.Highlight);
                    backColor = Color.FromArgb(60, accentColor);
                }
                else
                {
                    backColor = Color.FromArgb(60, SystemColors.Highlight);
                }
            }
            else if (defaultBackColor != Color.Empty)
            {
                backColor = defaultBackColor;
            }

            if (backColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
        }

        private void DrawCheckMark(Graphics g, Rectangle bounds)
        {
            if (!Checked && IsTopLevel)
            {
                return;
            }

            int checkSize = 16;
            Rectangle checkBounds = new Rectangle(
                bounds.X + 4,
                bounds.Y + (bounds.Height - checkSize) / 2,
                checkSize,
                checkSize);

            if (Checked)
            {
                Color checkColor = GetForeColor();

                using (var pen = new Pen(checkColor, 2))
                {
                    if (checkStyle == CheckStyle.CheckMark)
                    {
                        Point[] checkPoints = new Point[]
                        {
                        new Point(checkBounds.X + 3, checkBounds.Y + checkBounds.Height / 2),
                        new Point(checkBounds.X + checkBounds.Width / 2 - 1, checkBounds.Bottom - 4),
                        new Point(checkBounds.Right - 3, checkBounds.Y + 3)
                        };
                        g.DrawLines(pen, checkPoints);
                    }
                    else
                    {
                        using (var brush = new SolidBrush(checkColor))
                        {
                            g.FillEllipse(brush,
                                checkBounds.X + 4,
                                checkBounds.Y + 4,
                                checkBounds.Width - 8,
                                checkBounds.Height - 8);
                        }
                    }
                }
            }
        }

        private void DrawImage(Graphics g, Rectangle bounds)
        {
            if (Image == null)
            {
                return;
            }

            int imageSize = 16;
            int imageX = bounds.X + (IsTopLevel ? Padding.Left : 24);
            int imageY = bounds.Y + (bounds.Height - imageSize) / 2;

            Rectangle imageRect = new Rectangle(imageX, imageY, imageSize, imageSize);

            if (Enabled)
            {
                g.DrawImage(Image, imageRect);
            }
            else
            {
                ControlPaint.DrawImageDisabled(g, Image, imageRect.X, imageRect.Y, Color.Transparent);
            }
        }

        private void DrawText(Graphics g, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            Color textColor = GetForeColor();
            Font font = itemFont ?? Owner?.Font ?? SystemFonts.MenuFont;

            int textX = bounds.X + Padding.Left;
            if (!IsTopLevel)
            {
                textX = bounds.X + (Image != null ? 44 : 24);
            }

            int shortcutWidth = 0;
            if (!IsTopLevel && ShowShortcutKeys && ShortcutKeys != Keys.None)
            {
                shortcutWidth = 60;
            }

            Rectangle textBounds = new Rectangle(
                textX,
                bounds.Y + Padding.Top,
                bounds.Width - (textX - bounds.X) - Padding.Right - shortcutWidth - (HasDropDownItems ? 20 : 0),
                bounds.Height - Padding.Vertical);

            using (var brush = new SolidBrush(textColor))
            using (var format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Near;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;

                g.DrawString(Text, font, brush, textBounds, format);
            }
        }

        private void DrawShortcutKey(Graphics g, Rectangle bounds)
        {
            string shortcutText = GetShortcutKeyText();
            if (string.IsNullOrEmpty(shortcutText))
            {
                return;
            }

            Color textColor = Enabled ? SystemColors.GrayText : SystemColors.InactiveCaptionText;
            if (Owner?.UseTheme == true && Owner.Theme != null)
            {
                textColor = Enabled
                    ? (Owner as FluentMenuStrip).GetThemeColor(c => c.TextSecondary, SystemColors.GrayText)
                    : (Owner as FluentMenuStrip).GetThemeColor(c => c.TextDisabled, SystemColors.InactiveCaptionText);
            }

            Font font = itemFont ?? Owner?.Font ?? SystemFonts.MenuFont;

            Rectangle shortcutBounds = new Rectangle(
                bounds.Right - 80,
                bounds.Y,
                60,
                bounds.Height);

            using (var brush = new SolidBrush(textColor))
            using (var format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Far;

                g.DrawString(shortcutText, font, brush, shortcutBounds, format);
            }
        }

        private void DrawArrow(Graphics g, Rectangle bounds)
        {
            Color arrowColor = GetForeColor();

            int arrowSize = 4;
            int arrowX = bounds.Right - 12;
            int arrowY = bounds.Y + bounds.Height / 2;

            Point[] arrowPoints = new Point[]
            {
            new Point(arrowX, arrowY - arrowSize),
            new Point(arrowX + arrowSize, arrowY),
            new Point(arrowX, arrowY + arrowSize)
            };

            using (var brush = new SolidBrush(arrowColor))
            {
                g.FillPolygon(brush, arrowPoints);
            }
        }

        private Color GetForeColor()
        {
            if (!Enabled)
            {
                if (Owner?.UseTheme == true && Owner.Theme != null)
                {
                    return (Owner as FluentMenuStrip).GetThemeColor(c => c.TextDisabled, SystemColors.GrayText);
                }
                return SystemColors.GrayText;
            }

            if (isMouseOver || isDropDownOpen)
            {
                if (hoverForeColor != Color.Empty)
                {
                    return hoverForeColor;
                }
            }

            if (defaultForeColor != Color.Empty)
            {
                return defaultForeColor;
            }

            if (Owner?.UseTheme == true && Owner.Theme != null)
            {
                return (Owner as FluentMenuStrip).GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
            }

            return SystemColors.ControlText;
        }

        #endregion

        #region 事件处理

        internal override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isMouseOver = true;
            InvalidateItem();

            var menuStrip = Owner as FluentMenuStrip;

            // 如果是顶级菜单项且有其他菜单打开, 自动切换
            if (IsTopLevel && menuStrip != null && menuStrip.IsAnyDropDownOpen && HasDropDownItems)
            {
                // 获取当前活动的菜单项
                var activeItem = menuStrip.GetActiveDropDownItem();

                // 如果当前活动项不是自己, 先关闭它
                if (activeItem != null && activeItem != this)
                {
                    activeItem.HideDropDown();
                    activeItem.ResetMouseOverState();
                }

                // 显示自己的下拉菜单
                ShowDropDown();
            }
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            // 只有在下拉菜单未打开时才重置悬停状态
            if (!isDropDownOpen)
            {
                isMouseOver = false;
                InvalidateItem();
            }
        }

        internal override void OnClick(EventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            if (CheckOnClick)
            {
                Checked = !Checked;
            }

            if (HasDropDownItems)
            {
                if (isDropDownOpen)
                {
                    HideDropDown();
                }
                else
                {
                    // 如果是顶级菜单项, 关闭其他菜单
                    if (IsTopLevel && Owner is FluentMenuStrip menuStrip)
                    {
                        var activeItem = menuStrip.GetActiveDropDownItem();
                        if (activeItem != null && activeItem != this)
                        {
                            activeItem.HideDropDown();
                            activeItem.ResetMouseOverState();
                        }
                    }

                    ShowDropDown();
                }
            }
            else
            {
                base.OnClick(e);
                CloseAllDropDowns();
            }
        }

        private void CloseAllDropDowns()
        {
            var current = Owner;
            while (current != null)
            {
                if (current is FluentMenuStrip menuStrip)
                {
                    menuStrip.CloseAllDropDowns();
                    break;
                }
                else if (current is FluentMenuDropDown dropDown)
                {
                    dropDown.OwnerItem?.HideDropDown();
                    current = dropDown.OwnerItem?.Owner;
                }
                else
                {
                    break;
                }
            }
        }

        private void DropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            isDropDownOpen = false;

            if (!IsTopLevel)
            {
                isMouseOver = false;
            }

            OnDropDownClosed(EventArgs.Empty);
            InvalidateItem();

            // 清除MenuStrip中的活动项引用
            if (Owner is FluentMenuStrip menuStrip)
            {
                if (menuStrip.GetActiveDropDownItem() == this)
                {
                    menuStrip.SetActiveDropDownItem(null);
                }
            }
        }

        #endregion

        #region 大小计算

        internal override Size GetPreferredSize()
        {
            // 如果设置了自定义宽度, 使用自定义宽度
            if (itemWidth > 0)
            {
                int h = Padding.Vertical + 22;
                Font f = itemFont ?? Owner?.Font ?? SystemFonts.MenuFont;

                if (!string.IsNullOrEmpty(Text))
                {
                    using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        var textSize = TextRenderer.MeasureText(g, Text, f,
                            new Size(int.MaxValue, int.MaxValue),
                            TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                        h = Math.Max(h, textSize.Height + Padding.Vertical);
                    }
                }

                return new Size(itemWidth, h);
            }

            // 分隔符特殊处理
            if (IsSeparator)
            {
                return new Size(100, 6);
            }

            // 自动计算
            int width = Padding.Horizontal;
            int height = Padding.Vertical + 22;

            Font font = itemFont ?? Owner?.Font ?? SystemFonts.MenuFont;

            if (!string.IsNullOrEmpty(Text))
            {
                Size textSize;
                if (Owner is Control control && control.IsHandleCreated)
                {
                    using (var g = control.CreateGraphics())
                    {
                        textSize = TextRenderer.MeasureText(g, Text, font,
                            new Size(int.MaxValue, int.MaxValue),
                            TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                    }
                }
                else
                {
                    textSize = TextRenderer.MeasureText(Text, font,
                        new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                }

                width += textSize.Width + 8;
                height = Math.Max(height, textSize.Height + Padding.Vertical);
            }

            if (IsTopLevel)
            {
                if (Image != null)
                {
                    width += 20;
                }
                width += 16;
            }
            else
            {
                width += 60;

                if (ShowShortcutKeys && ShortcutKeys != Keys.None)
                {
                    width += 80;
                }

                if (HasDropDownItems)
                {
                    width += 20;
                }

                width = Math.Max(width, 150);
            }

            return new Size(width, height);
        }


        //internal override Size GetPreferredSize()
        //{
        //    // 分隔符特殊处理
        //    if (IsSeparator)
        //    {
        //        return new Size(100, 6);
        //    }

        //    // 正常计算
        //    int width = Padding.Horizontal;
        //    int height = Padding.Vertical + 22;

        //    Font font = itemFont ?? Owner?.Font ?? SystemFonts.MenuFont;

        //    if (!string.IsNullOrEmpty(Text))
        //    {
        //        Size textSize;
        //        if (Owner is Control control && control.IsHandleCreated)
        //        {
        //            using (var g = control.CreateGraphics())
        //            {
        //                textSize = TextRenderer.MeasureText(g, Text, font,
        //                    new Size(int.MaxValue, int.MaxValue),
        //                    TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        //            }
        //        }
        //        else
        //        {
        //            textSize = TextRenderer.MeasureText(Text, font,
        //                new Size(int.MaxValue, int.MaxValue),
        //                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        //        }

        //        width += textSize.Width + 8;
        //        height = Math.Max(height, textSize.Height + Padding.Vertical);
        //    }

        //    if (IsTopLevel)
        //    {
        //        if (Image != null)
        //        {
        //            width += 20;
        //        }
        //        width += 16;
        //    }
        //    else
        //    {
        //        width += 60;

        //        if (ShowShortcutKeys && ShortcutKeys != Keys.None)
        //        {
        //            width += 80;
        //        }

        //        if (HasDropDownItems)
        //        {
        //            width += 20;
        //        }

        //        width = Math.Max(width, 150);
        //    }

        //    return new Size(width, height);
        //}

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (dropDown != null)
                {
                    dropDown.Closed -= DropDown_Closed;
                    dropDown.Dispose();
                }
                dropDownItems?.Clear();
            }
            base.Dispose(disposing);
        }
    }
    #endregion

    #region 菜单项集合

    /// <summary>
    /// 菜单项集合
    /// </summary>
    [Editor(typeof(FluentMenuItemCollectionEditor), typeof(UITypeEditor))]
    [ListBindable(false)]
    public class FluentMenuItemCollection : IList<FluentMenuItem>, IList
    {
        private List<FluentMenuItem> items = new List<FluentMenuItem>();
        private object owner; // 改为object类型, 可以是FluentMenuItem或FluentMenuStrip

        // 添加集合变更事件
        public event EventHandler CollectionChanged;

        internal FluentMenuItemCollection(object owner)
        {
            this.owner = owner;
        }

        protected virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);

            // 通知Owner更新
            if (owner is FluentMenuStrip menuStrip)
            {
                menuStrip.PerformLayout();
                menuStrip.Invalidate();
            }
            else if (owner is FluentMenuItem menuItem)
            {
                menuItem.Invalidate();
            }
        }

        public FluentMenuItem this[int index]
        {
            get => items[index];
            set
            {
                if (items[index] != value)
                {
                    var oldItem = items[index];
                    items[index] = value;

                    // 更新Owner
                    if (value != null && owner is FluentMenuItem ownerItem)
                    {
                        value.Owner = ownerItem.Owner;
                    }

                    OnCollectionChanged();
                }
            }
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;

        public void Add(FluentMenuItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            items.Add(item);

            // 设置Owner
            if (owner is FluentMenuItem ownerItem && ownerItem.Owner != null)
            {
                item.Owner = ownerItem.Owner;
            }

            OnCollectionChanged();
        }

        public void Add(string text)
        {
            Add(new FluentMenuItem(text));
        }

        public void Add(string text, EventHandler onClick)
        {
            Add(new FluentMenuItem(text, onClick));
        }

        public void AddRange(params FluentMenuItem[] items)
        {
            if (items != null && items.Length > 0)
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        this.items.Add(item);

                        // 设置Owner
                        if (owner is FluentMenuItem ownerItem && ownerItem.Owner != null)
                        {
                            item.Owner = ownerItem.Owner;
                        }
                    }
                }
                OnCollectionChanged();
            }
        }

        public void Insert(int index, FluentMenuItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            items.Insert(index, item);

            // 设置Owner
            if (owner is FluentMenuItem ownerItem && ownerItem.Owner != null)
            {
                item.Owner = ownerItem.Owner;
            }

            OnCollectionChanged();
        }

        public bool Remove(FluentMenuItem item)
        {
            bool result = items.Remove(item);
            if (result)
            {
                OnCollectionChanged();
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
            OnCollectionChanged();
        }

        public void Clear()
        {
            items.Clear();
            OnCollectionChanged();
        }

        public bool Contains(FluentMenuItem item) => items.Contains(item);
        public int IndexOf(FluentMenuItem item) => items.IndexOf(item);
        public void CopyTo(FluentMenuItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public IEnumerator<FluentMenuItem> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region IList实现
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (FluentMenuItem)value;
        }

        int IList.Add(object value)
        {
            Add((FluentMenuItem)value);
            return items.Count - 1;
        }

        bool IList.Contains(object value) => Contains((FluentMenuItem)value);
        int IList.IndexOf(object value) => IndexOf((FluentMenuItem)value);
        void IList.Insert(int index, object value) => Insert(index, (FluentMenuItem)value);
        void IList.Remove(object value) => Remove((FluentMenuItem)value);
        bool IList.IsFixedSize => false;

        void ICollection.CopyTo(Array array, int index) => ((ICollection)items).CopyTo(array, index);
        object ICollection.SyncRoot => ((ICollection)items).SyncRoot;
        bool ICollection.IsSynchronized => false;
        #endregion

        // 用于调试和显示
        public override string ToString()
        {
            return $"Count = {Count}";
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 勾选样式
    /// </summary>
    public enum CheckStyle
    {
        /// <summary>
        /// 勾选标记
        /// </summary>
        CheckMark,

        /// <summary>
        /// 圆点
        /// </summary>
        RadioButton
    }

    /// <summary>
    /// Fluent下拉菜单
    /// </summary>
    [ToolboxItem(false)]
    public class FluentMenuDropDown : ToolStripDropDown
    {
        private FluentMenuItem ownerItem;
        private FluentMenuItemHost itemHost;
        private bool useTheme = false;
        private IFluentTheme theme;

        // 边框属性
        private Color borderColor = Color.Empty;
        private int borderWidth = 1;
        private bool showBorder = false;

        public FluentMenuDropDown(FluentMenuItem ownerItem)
        {
            this.ownerItem = ownerItem;

            // 设置样式
            AutoSize = true;
            DropShadowEnabled = true;
            Padding = new Padding(2);

            // 创建宿主
            itemHost = new FluentMenuItemHost(ownerItem.DropDownItems);
            Items.Add(itemHost);

            // 继承主题
            if (ownerItem.Owner is FluentMenuStrip menuStrip)
            {
                useTheme = menuStrip.UseTheme;
                theme = menuStrip.Theme;
                itemHost.UseTheme = useTheme;
                itemHost.Theme = theme;
            }
        }

        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                itemHost?.SetBorderColor(value);
            }
        }

        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                itemHost?.SetBorderWidth(value);
            }
        }

        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                showBorder = value;
                itemHost?.SetShowBorder(value);
            }
        }

        public FluentMenuItem OwnerItem => ownerItem;

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            base.OnClosed(e);

            // 通知所有子菜单项关闭
            foreach (var item in ownerItem.DropDownItems)
            {
                item.HideDropDown();
            }
        }
    }

    /// <summary>
    /// 菜单项宿主
    /// </summary>
    public class FluentMenuItemHost : ToolStripControlHost
    {
        private MenuItemPanel panel;

        public FluentMenuItemHost(FluentMenuItemCollection items)
            : base(new MenuItemPanel(items))
        {
            panel = (MenuItemPanel)Control;
            AutoSize = true;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
        }

        public bool UseTheme
        {
            get => panel.UseTheme;
            set => panel.UseTheme = value;
        }

        public IFluentTheme Theme
        {
            get => panel.Theme;
            set => panel.Theme = value;
        }

        public void SetBorderColor(Color color)
        {
            panel.BorderColor = color;
        }

        public void SetBorderWidth(int width)
        {
            panel.BorderWidth = width;
        }

        public void SetShowBorder(bool show)
        {
            panel.ShowBorder = show;
        }
    }

    /// <summary>
    /// 菜单项面板
    /// </summary>
    [ToolboxItem(false)]
    public class MenuItemPanel : Control
    {
        private FluentMenuItemCollection items;
        private FluentMenuItem hoveredItem;
        private bool useTheme = false;
        private IFluentTheme theme;
        private int itemHeight = 24;

        // 边框属性
        private Color borderColor = Color.Empty;
        private int borderWidth = 1;
        private bool showBorder = false;

        public MenuItemPanel(FluentMenuItemCollection items)
        {
            this.items = items;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            DoubleBuffered = true;

            CalculateSize();
        }

        public bool UseTheme
        {
            get => useTheme;
            set
            {
                useTheme = value;
                Invalidate();
            }
        }

        public IFluentTheme Theme
        {
            get => theme;
            set
            {
                theme = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = Math.Max(0, value);
                Invalidate();
            }
        }

        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                showBorder = value;
                Invalidate();
            }
        }

        private void CalculateSize()
        {
            int width = 150;
            int height = 0;

            using (var g = CreateGraphics())
            {
                foreach (var item in items)
                {
                    // 设置Owner为this以便正确应用主题
                    var size = item.Size;
                    width = Math.Max(width, size.Width + 4);
                    height += size.Height;

                    // 设置项目边界
                    item.Bounds = new Rectangle(2, height - size.Height, width - 4, size.Height);
                }
            }

            Size = new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制背景
            Color backColor = useTheme && theme != null
                ? GetThemeColor(c => c.Surface, SystemColors.Menu)
                : SystemColors.Menu;

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 绘制边框
            if (showBorder)
            {
                Color border = borderColor;
                if (border == Color.Empty)
                {
                    border = useTheme && theme != null
                        ? GetThemeColor(c => c.Border, SystemColors.ControlDark)
                        : SystemColors.ControlDark;
                }

                using (var pen = new Pen(border, borderWidth))
                {
                    Rectangle borderRect = new Rectangle(
                        borderWidth / 2,
                        borderWidth / 2,
                        Width - borderWidth,
                        Height - borderWidth);
                    g.DrawRectangle(pen, borderRect);
                }
            }

            // 绘制所有菜单项
            foreach (var item in items)
            {
                var itemBounds = item.Bounds;
                if (!itemBounds.IsEmpty)
                {
                    var paintArgs = new PaintEventArgs(g, itemBounds);
                    item.OnPaint(paintArgs);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            FluentMenuItem newHoveredItem = null;

            foreach (var item in items)
            {
                if (item.Bounds.Contains(e.Location))
                {
                    newHoveredItem = item;
                    break;
                }
            }

            if (hoveredItem != newHoveredItem)
            {
                if (hoveredItem != null)
                {
                    hoveredItem.OnMouseLeave(EventArgs.Empty);
                }

                hoveredItem = newHoveredItem;

                if (hoveredItem != null)
                {
                    hoveredItem.OnMouseEnter(EventArgs.Empty);
                }

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredItem != null)
            {
                hoveredItem.OnMouseLeave(EventArgs.Empty);
                hoveredItem = null;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            foreach (var item in items)
            {
                if (item.Bounds.Contains(e.Location) && item.Enabled)
                {
                    item.OnClick(EventArgs.Empty);
                    break;
                }
            }
        }

        private Color GetThemeColor(Func<IColorPalette, Color> selector, Color defaultColor)
        {
            if (useTheme && theme?.Colors != null)
            {
                try
                {
                    return selector(theme.Colors);
                }
                catch
                {
                }
            }
            return defaultColor;
        }
    }

    #endregion

    #region 设计器

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class FluentMenuStripDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private ISelectionService selectionService;
        private IDesignerHost designerHost;
        private IServiceProvider serviceProvider;
        private DesignerActionUIService designerService;

        public FluentMenuStrip MenuStrip => (FluentMenuStrip)Control;


        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (component is FluentMenuStrip toolStrip)
            {
                // 获取设计时服务
                serviceProvider = component.Site;
                selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
                MenuStrip.Height = MenuStrip.ItemHeight + MenuStrip.Padding.Vertical;

                var actionService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
                actionService?.ShowUI(component);
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            if (Control is FluentMenuStrip menuStrip)
            {
                menuStrip.Height = menuStrip.ItemHeight + menuStrip.Padding.Vertical;
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentMenuStripActionList(this));
                }
                return actionLists;
            }
        }

        protected override bool GetHitTest(Point point)
        {
            if (Control != null && Control.IsHandleCreated)
            {
                var menuStrip = Control as FluentMenuStrip;
                if (menuStrip != null)
                {
                    Point clientPoint = menuStrip.PointToClient(point);

                    // 只在菜单项范围内处理点击
                    foreach (var item in menuStrip.Items)
                    {
                        if (item.Visible && item.Bounds.Contains(clientPoint))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                actionLists = null;
            }
            base.Dispose(disposing);
        }

    }

    public class FluentMenuStripActionList : DesignerActionList
    {
        private FluentMenuStripDesigner designer;
        private FluentMenuStrip menuStrip;
        private DesignerActionUIService designerService;
        private IDesignerHost designerHost;
        private IComponentChangeService changeService;

        public FluentMenuStripActionList(FluentMenuStripDesigner designer) : base(designer.Component)
        {
            this.designer = designer;
            this.menuStrip = designer.MenuStrip;

            // 获取设计时服务
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
            designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        }

        #region 属性

        [Category("布局")]
        [Description("停靠样式")]
        public DockStyle Dock
        {
            get => menuStrip?.Dock ?? DockStyle.Top;
            set => SetProperty("Dock", value);
        }

        [Category("外观")]
        [Description("菜单项高度")]
        public int ItemHeight
        {
            get => menuStrip?.ItemHeight ?? 28;
            set
            {
                if (menuStrip != null)
                {
                    SetProperty("ItemHeight", value);
                }
            }
        }

        [Category("外观")]
        [Description("菜单背景色")]
        public Color MenuBackColor
        {
            get => menuStrip?.MenuBackColor ?? Color.Empty;
            set
            {
                if (menuStrip != null)
                {
                    SetProperty("MenuBackColor", value);
                }
            }
        }

        [Category("外观")]
        [Description("菜单前景色")]
        public Color MenuForeColor
        {
            get => menuStrip?.MenuForeColor ?? Color.Empty;
            set
            {
                if (menuStrip != null)
                {
                    SetProperty("MenuForeColor", value);
                }
            }
        }

        [Category("主题")]
        [Description("使用主题")]
        public bool UseTheme
        {
            get => menuStrip?.UseTheme ?? false;
            set
            {
                if (menuStrip != null)
                {
                    SetProperty("UseTheme", value);
                }
            }
        }

        [Category("主题")]
        [Description("主题名称")]
        [TypeConverter(typeof(ThemeNameConverter))]
        public string ThemeName
        {
            get => menuStrip?.ThemeName ?? "";
            set
            {
                if (menuStrip != null)
                {
                    SetProperty("ThemeName", value);
                }
            }
        }

        #endregion

        #region 方法

        [Description("编辑菜单项")]
        public void EditItems()
        {
            if (menuStrip == null)
            {
                return;
            }

            try
            {
                PropertyDescriptor itemsProperty = TypeDescriptor.GetProperties(menuStrip)["Items"];
                if (itemsProperty == null)
                {
                    return;
                }

                var editor = itemsProperty.GetEditor(typeof(System.Drawing.Design.UITypeEditor))
                    as System.Drawing.Design.UITypeEditor;

                if (editor != null)
                {
                    var context = new TypeDescriptorContext(
                        menuStrip,
                        itemsProperty,
                        designerHost);

                    object value = editor.EditValue(context, context, menuStrip.Items);

                    // 强制刷新
                    ForceRefresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑菜单项时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Description("添加标准文件菜单")]
        public void AddFileMenu()
        {
            if (menuStrip == null)
            {
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = designerHost?.CreateTransaction("添加文件菜单");

                var fileMenu = new FluentMenuItem("文件(&F)");

                designerHost?.Container.Add(fileMenu);

                // 创建并添加所有子菜单项到容器
                var newItem = new FluentMenuItem("新建(&N)") { ShortcutKeys = Keys.Control | Keys.N };
                designerHost?.Container.Add(newItem);
                fileMenu.DropDownItems.Add(newItem);

                var openItem = new FluentMenuItem("打开(&O)") { ShortcutKeys = Keys.Control | Keys.O };
                designerHost?.Container.Add(openItem);
                fileMenu.DropDownItems.Add(openItem);

                var separator1 = new FluentMenuItem("-");
                designerHost?.Container.Add(separator1);
                fileMenu.DropDownItems.Add(separator1);

                var saveItem = new FluentMenuItem("保存(&S)") { ShortcutKeys = Keys.Control | Keys.S };
                designerHost?.Container.Add(saveItem);
                fileMenu.DropDownItems.Add(saveItem);

                var saveAsItem = new FluentMenuItem("另存为(&A)");
                designerHost?.Container.Add(saveAsItem);
                fileMenu.DropDownItems.Add(saveAsItem);

                var separator2 = new FluentMenuItem("-");
                designerHost?.Container.Add(separator2);
                fileMenu.DropDownItems.Add(separator2);

                var exitItem = new FluentMenuItem("退出(&X)") { ShortcutKeys = Keys.Alt | Keys.F4 };
                designerHost?.Container.Add(exitItem);
                fileMenu.DropDownItems.Add(exitItem);

                // 通知属性更改
                PropertyDescriptor itemsProp = TypeDescriptor.GetProperties(menuStrip)["Items"];
                changeService?.OnComponentChanging(menuStrip, itemsProp);

                menuStrip.Items.Add(fileMenu);

                changeService?.OnComponentChanged(menuStrip, itemsProp, null, null);

                transaction?.Commit();

                ForceRefresh();
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"添加文件菜单时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Description("添加标准编辑菜单")]
        public void AddEditMenu()
        {
            if (menuStrip == null)
            {
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = designerHost?.CreateTransaction("添加编辑菜单");

                var editMenu = new FluentMenuItem("编辑(&E)");
                designerHost?.Container.Add(editMenu);

                // 添加所有子项到容器
                var undoItem = new FluentMenuItem("撤销(&U)") { ShortcutKeys = Keys.Control | Keys.Z };
                designerHost?.Container.Add(undoItem);
                editMenu.DropDownItems.Add(undoItem);

                var redoItem = new FluentMenuItem("重做(&R)") { ShortcutKeys = Keys.Control | Keys.Y };
                designerHost?.Container.Add(redoItem);
                editMenu.DropDownItems.Add(redoItem);

                var separator1 = new FluentMenuItem("-");
                designerHost?.Container.Add(separator1);
                editMenu.DropDownItems.Add(separator1);

                var cutItem = new FluentMenuItem("剪切(&T)") { ShortcutKeys = Keys.Control | Keys.X };
                designerHost?.Container.Add(cutItem);
                editMenu.DropDownItems.Add(cutItem);

                var copyItem = new FluentMenuItem("复制(&C)") { ShortcutKeys = Keys.Control | Keys.C };
                designerHost?.Container.Add(copyItem);
                editMenu.DropDownItems.Add(copyItem);

                var pasteItem = new FluentMenuItem("粘贴(&P)") { ShortcutKeys = Keys.Control | Keys.V };
                designerHost?.Container.Add(pasteItem);
                editMenu.DropDownItems.Add(pasteItem);

                PropertyDescriptor itemsProp = TypeDescriptor.GetProperties(menuStrip)["Items"];
                changeService?.OnComponentChanging(menuStrip, itemsProp);

                menuStrip.Items.Add(editMenu);

                changeService?.OnComponentChanged(menuStrip, itemsProp, null, null);
                transaction?.Commit();

                ForceRefresh();
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"添加编辑菜单时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Description("添加标准视图菜单")]
        public void AddViewMenu()
        {
            if (menuStrip == null)
            {
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = designerHost?.CreateTransaction("添加视图菜单");

                var viewMenu = new FluentMenuItem("视图(&V)");
                designerHost?.Container.Add(viewMenu);

                // 添加所有子项到容器
                var toolbarItem = new FluentMenuItem("工具栏(&T)") { CheckOnClick = true, Checked = true };
                designerHost?.Container.Add(toolbarItem);
                viewMenu.DropDownItems.Add(toolbarItem);

                var statusBarItem = new FluentMenuItem("状态栏(&S)") { CheckOnClick = true, Checked = true };
                designerHost?.Container.Add(statusBarItem);
                viewMenu.DropDownItems.Add(statusBarItem);

                var sidebarItem = new FluentMenuItem("侧边栏(&B)") { CheckOnClick = true };
                designerHost?.Container.Add(sidebarItem);
                viewMenu.DropDownItems.Add(sidebarItem);

                PropertyDescriptor itemsProp = TypeDescriptor.GetProperties(menuStrip)["Items"];
                changeService?.OnComponentChanging(menuStrip, itemsProp);

                menuStrip.Items.Add(viewMenu);

                changeService?.OnComponentChanged(menuStrip, itemsProp, null, null);
                transaction?.Commit();

                ForceRefresh();
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"添加视图菜单时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Description("添加标准帮助菜单")]
        public void AddHelpMenu()
        {
            if (menuStrip == null)
            {
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = designerHost?.CreateTransaction("添加帮助菜单");

                var helpMenu = new FluentMenuItem("帮助(&H)");
                designerHost?.Container.Add(helpMenu);

                // 添加所有子项到容器
                var helpItem = new FluentMenuItem("查看帮助(&H)") { ShortcutKeys = Keys.F1 };
                designerHost?.Container.Add(helpItem);
                helpMenu.DropDownItems.Add(helpItem);

                var separator = new FluentMenuItem("-");
                designerHost?.Container.Add(separator);
                helpMenu.DropDownItems.Add(separator);

                var aboutItem = new FluentMenuItem("关于(&A)");
                designerHost?.Container.Add(aboutItem);
                helpMenu.DropDownItems.Add(aboutItem);

                PropertyDescriptor itemsProp = TypeDescriptor.GetProperties(menuStrip)["Items"];
                changeService?.OnComponentChanging(menuStrip, itemsProp);

                menuStrip.Items.Add(helpMenu);

                changeService?.OnComponentChanged(menuStrip, itemsProp, null, null);
                transaction?.Commit();

                ForceRefresh();
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"添加帮助菜单时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Description("添加标准菜单组")]
        public void AddStandardMenus()
        {
            if (menuStrip == null)
            {
                return;
            }

            if (menuStrip.Items.Count > 0)
            {
                var result = MessageBox.Show(
                    "菜单栏已有菜单项, 是否清除后继续？",
                    "确认",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                {
                    return;
                }

                if (result == DialogResult.Yes)
                {
                    ClearAllMenus();
                }
            }

            AddFileMenu();
            AddEditMenu();
            AddViewMenu();
            AddHelpMenu();
        }

        [Description("清除所有菜单")]
        public void ClearAllMenus()
        {
            if (menuStrip == null)
            {
                return;
            }

            var result = MessageBox.Show(
                "确定要清除所有菜单项吗？",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DesignerTransaction transaction = null;
                try
                {
                    transaction = designerHost?.CreateTransaction("清除所有菜单");

                    PropertyDescriptor itemsProp = TypeDescriptor.GetProperties(menuStrip)["Items"];
                    changeService?.OnComponentChanging(menuStrip, itemsProp);

                    // 递归删除所有项目及其子项
                    RemoveAllItemsRecursive(menuStrip.Items);

                    changeService?.OnComponentChanged(menuStrip, itemsProp, null, null);
                    transaction?.Commit();

                    ForceRefresh();
                }
                catch
                {
                    transaction?.Cancel();
                    throw;
                }
            }
        }

        [Description("停靠到顶部")]
        public void DockTop()
        {
            Dock = DockStyle.Top;
        }

        [Description("停靠到底部")]
        public void DockBottom()
        {
            Dock = DockStyle.Bottom;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 递归删除所有项目
        /// </summary>
        private void RemoveAllItemsRecursive(FluentMenuItemCollection items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            // 从后往前删除, 避免索引问题
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];

                // 先递归删除子项
                if (item.HasDropDownItems)
                {
                    RemoveAllItemsRecursive(item.DropDownItems);
                }

                // 从设计器容器中移除
                if (designerHost?.Container != null && item.Site != null)
                {
                    designerHost.Container.Remove(item);
                }

                // 从集合中移除
                items.RemoveAt(i);

                // 释放资源
                item.Dispose();
            }
        }

        private void SetProperty(string propertyName, object value)
        {
            if (menuStrip == null)
            {
                return;
            }

            try
            {
                PropertyDescriptor property = TypeDescriptor.GetProperties(menuStrip)[propertyName];
                if (property != null && !property.IsReadOnly)
                {
                    changeService?.OnComponentChanging(menuStrip, property);

                    property.SetValue(menuStrip, value);

                    changeService?.OnComponentChanged(menuStrip, property, null, value);

                    ForceRefresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置属性 {propertyName} 时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ForceRefresh()
        {
            if (menuStrip == null)
            {
                return;
            }

            try
            {
                // 强制重新布局
                menuStrip.PerformLayout();

                // 强制重绘
                menuStrip.Invalidate();
                menuStrip.Update();

                // 刷新父容器
                if (menuStrip.Parent != null)
                {
                    menuStrip.Parent.PerformLayout();
                    menuStrip.Parent.Invalidate();
                }

                // 刷新设计器
                designerService?.Refresh(Component);
            }
            catch
            {
                // 忽略刷新错误
            }
        }

        #endregion

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 快速操作
            items.Add(new DesignerActionHeaderItem("快速操作"));
            items.Add(new DesignerActionMethodItem(this, "EditItems", "编辑菜单项...", "快速操作",
                "打开集合编辑器编辑菜单项", true));
            items.Add(new DesignerActionMethodItem(this, "AddStandardMenus", "添加标准菜单组", "快速操作",
                "添加文件、编辑、视图、帮助菜单", true));

            // 添加菜单
            items.Add(new DesignerActionHeaderItem("添加菜单"));
            items.Add(new DesignerActionMethodItem(this, "AddFileMenu", "文件菜单", "添加菜单",
                "添加包含新建、打开、保存等的文件菜单", false));
            items.Add(new DesignerActionMethodItem(this, "AddEditMenu", "编辑菜单", "添加菜单",
                "添加包含撤销、复制、粘贴等的编辑菜单", false));
            items.Add(new DesignerActionMethodItem(this, "AddViewMenu", "视图菜单", "添加菜单",
                "添加包含工具栏、状态栏等的视图菜单", false));
            items.Add(new DesignerActionMethodItem(this, "AddHelpMenu", "帮助菜单", "添加菜单",
                "添加包含查看帮助、关于等的帮助菜单", false));
            items.Add(new DesignerActionMethodItem(this, "ClearAllMenus", "清除所有", "添加菜单",
                "清除所有菜单项", false));

            // 布局
            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Dock", "停靠位置", "布局",
                "设置菜单栏的停靠位置"));
            items.Add(new DesignerActionPropertyItem("ItemHeight", "菜单项高度", "布局",
                "设置菜单项的高度"));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("MenuBackColor", "背景色", "外观",
                "设置菜单栏背景颜色"));
            items.Add(new DesignerActionPropertyItem("MenuForeColor", "前景色", "外观",
                "设置菜单栏前景颜色"));

            // 主题
            items.Add(new DesignerActionHeaderItem("主题"));
            items.Add(new DesignerActionPropertyItem("UseTheme", "使用主题", "主题",
                "是否使用Fluent主题"));
            items.Add(new DesignerActionPropertyItem("ThemeName", "主题名称", "主题",
                "选择要应用的主题"));

            return items;
        }
    }

    public class FluentMenuItemCollectionEditor : CollectionEditor
    {
        public FluentMenuItemCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentMenuItem);
        }

        protected override object CreateInstance(Type itemType)
        {
            var item = base.CreateInstance(itemType) as FluentMenuItem;
            if (item != null)
            {
                item.Text = "菜单项";
            }
            return item;
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentMenuItem item)
            {
                if (item.Text == "-")
                {
                    return "(分隔符)";
                }

                string displayText = item.Text;

                if (!string.IsNullOrEmpty(item.Name))
                {
                    displayText = string.IsNullOrEmpty(item.Text)
                        ? item.Name
                        : $"{item.Name} - {item.Text}";
                }
                else if (string.IsNullOrEmpty(item.Text))
                {
                    displayText = "(菜单项)";
                }

                // 如果有子项, 显示子项数量
                if (item.HasDropDownItems)
                {
                    displayText += $" [{item.DropDownItems.Count} 项]";
                }

                return displayText;
            }
            return base.GetDisplayText(value);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[]
            {
                typeof(FluentMenuItem)
            };
        }

        protected override void DestroyInstance(object instance)
        {
            if (instance is FluentMenuItem item)
            {
                // 递归删除所有子项
                DestroyMenuItemRecursive(item);
            }

            base.DestroyInstance(instance);
        }

        /// <summary>
        /// 递归销毁菜单项及其子项
        /// </summary>
        private void DestroyMenuItemRecursive(FluentMenuItem item)
        {
            if (item == null)
            {
                return;
            }

            // 如果有子项, 先递归删除
            if (item.HasDropDownItems)
            {
                // 从后往前删除
                for (int i = item.DropDownItems.Count - 1; i >= 0; i--)
                {
                    var subItem = item.DropDownItems[i];
                    DestroyMenuItemRecursive(subItem);
                    item.DropDownItems.RemoveAt(i);
                }
            }

            // 从设计器容器中移除
            if (item.Site?.Container != null)
            {
                item.Site.Container.Remove(item);
            }

            // 释放资源
            item.Dispose();
        }


        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            try
            {
                // 保存原始集合的引用
                object result = base.EditValue(context, provider, value);

                // 确保所有项目的Owner正确设置
                if (result is FluentMenuItemCollection collection && context?.Instance != null)
                {
                    IFluentItemContainer owner = null;

                    if (context.Instance is FluentMenuStrip menuStrip)
                    {
                        owner = menuStrip;
                    }
                    else if (context.Instance is FluentMenuItem menuItem)
                    {
                        owner = menuItem.Owner;
                    }

                    if (owner != null)
                    {
                        foreach (var item in collection)
                        {
                            item.Owner = owner;

                            // 递归设置子项的Owner
                            SetOwnerRecursive(item, owner);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑菜单项时出错: {ex.Message}\n\n{ex.StackTrace}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return value;
            }
        }

        private void SetOwnerRecursive(FluentMenuItem item, IFluentItemContainer owner)
        {
            if (item.HasDropDownItems)
            {
                foreach (var subItem in item.DropDownItems)
                {
                    subItem.Owner = owner;
                    SetOwnerRecursive(subItem, owner);
                }
            }
        }

        protected override object SetItems(object editValue, object[] value)
        {
            // 调用基类方法设置项目
            var result = base.SetItems(editValue, value);

            // 强制刷新
            if (Context?.Instance is FluentMenuStrip menuStrip)
            {
                menuStrip.PerformLayout();
                menuStrip.Invalidate();
                menuStrip.Update();
            }

            return result;
        }

        protected override bool CanSelectMultipleInstances()
        {
            return true;
        }
    }

    #endregion
}
