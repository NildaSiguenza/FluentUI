using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Themes;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent标题栏控件
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FluentTitleBar
    {
        private FluentForm form;
        private IFluentTheme theme;
        private List<FluentTitleBarButton> buttons;
        private Rectangle bounds;

        #region 属性

        [Category("标题栏")]
        [Description("标题栏高度")]
        [DefaultValue(32)]
        public int Height { get; set; } = 32;

        [Category("标题栏")]
        [Description("标题文本")]
        public string Title { get; set; }

        [Category("标题栏")]
        [Description("标题对齐方式")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TitleAlignment { get; set; } = ContentAlignment.MiddleLeft;

        [Category("标题栏")]
        [Description("显示图标")]
        [DefaultValue(true)]
        public bool ShowIcon { get; set; } = true;

        [Category("标题栏")]
        [Description("图标")]
        public Image Icon { get; set; }

        [Category("标题栏")]
        [Description("背景色")]
        public Color BackColor { get; set; }

        [Category("标题栏")]
        [Description("前景色")]
        public Color ForeColor { get; set; }

        [Category("标题栏")]
        [Description("标题字体")]
        public Font TitleFont { get; set; }

        [Category("标题栏按钮")]
        [Description("显示关闭按钮")]
        [DefaultValue(true)]
        public bool ShowCloseButton { get; set; } = true;

        [Category("标题栏按钮")]
        [Description("显示最大化按钮")]
        [DefaultValue(true)]
        public bool ShowMaximizeButton { get; set; } = true;

        [Category("标题栏按钮")]
        [Description("显示最小化按钮")]
        [DefaultValue(true)]
        public bool ShowMinimizeButton { get; set; } = true;

        [Browsable(false)]
        public Rectangle Bounds => bounds;

        [Browsable(false)]
        public FluentTitleBarButton CloseButton { get; private set; }

        [Browsable(false)]
        public FluentTitleBarButton MaximizeButton { get; private set; }

        [Browsable(false)]
        public FluentTitleBarButton MinimizeButton { get; private set; }

        [Browsable(false)]
        public int Width { get; set; }

        #endregion

        #region 构造函数

        public FluentTitleBar(FluentForm form)
        {
            this.form = form;
            buttons = new List<FluentTitleBarButton>();
            InitializeSystemButtons();
        }

        private void InitializeSystemButtons()
        {
            // 关闭按钮
            CloseButton = new FluentTitleBarButton
            {
                Name = "CloseButton",
                Type = TitleBarButtonType.Close,
                Text = "", // 不使用文本, 使用自绘
                Width = 45,
                ToolTip = "关闭",
                form = this.form // 添加form引用
            };

            // 最大化按钮
            MaximizeButton = new FluentTitleBarButton
            {
                Name = "MaximizeButton",
                Type = TitleBarButtonType.Maximize,
                Text = "", // 不使用文本, 使用自绘
                Width = 45,
                ToolTip = form?.WindowState == FormWindowState.Maximized ? "还原" : "最大化",
                form = this.form
            };

            // 最小化按钮
            MinimizeButton = new FluentTitleBarButton
            {
                Name = "MinimizeButton",
                Type = TitleBarButtonType.Minimize,
                Text = "", // 不使用文本, 使用自绘
                Width = 45,
                ToolTip = "最小化",
                form = this.form
            };
        }

        #endregion

        #region 方法

        public void ApplyTheme(IFluentTheme theme)
        {
            this.theme = theme;

            if (theme != null)
            {
                BackColor = theme.Colors.Primary;
                ForeColor = theme.Colors.TextOnPrimary;
                TitleFont = theme.Typography.Title;

                foreach (var button in GetAllButtons())
                {
                    button.ApplyTheme(theme);
                }
            }
        }

        public void AddButton(FluentTitleBarButton button)
        {
            if (!buttons.Contains(button))
            {
                buttons.Add(button);
                button.ApplyTheme(theme);
                UpdateButtonPositions();
            }
        }

        public void RemoveButton(FluentTitleBarButton button)
        {
            buttons.Remove(button);
            UpdateButtonPositions();
        }

        public FluentTitleBarButton GetButton(string name)
        {
            return GetAllButtons().FirstOrDefault(b => b.Name == name);
        }

        public void UpdateButtonPositions()
        {
            var allButtons = GetAllButtons();
            int x = Width;

            foreach (var button in allButtons)
            {
                x -= button.Width;
                button.Bounds = new Rectangle(x, 0, button.Width, Height);
            }
        }

        public bool IsPointOnButton(Point pt)
        {
            return GetAllButtons().Any(b => b.Bounds.Contains(pt));
        }

        public bool HasHoveredButton()
        {
            return GetAllButtons().Any(b => b.IsHovered);
        }

        public void ClearHoverState()
        {
            foreach (var button in GetAllButtons())
            {
                button.IsHovered = false;
            }
        }

        internal FluentForm GetForm()
        {
            return form;
        }

        #endregion

        #region 绘制

        public void Draw(Graphics g)
        {
            bounds = new Rectangle(0, 0, Width, Height);

            bool hasRoundCorners = form.CornerRadius > 0 && form.WindowState != FormWindowState.Maximized;
            if (hasRoundCorners)
            {
                // 使用圆角路径填充背景
                using (var path = CreateRoundedTopPath(bounds, form.CornerRadius))
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillPath(brush, path);
                }
            }
            else
            {
                // 普通矩形填充
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // 绘制背景
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 绘制图标
            int iconOffset = 0;
            if (ShowIcon && Icon != null)
            {
                var iconRect = new Rectangle(8, (Height - 20) / 2, 20, 20);
                g.DrawImage(Icon, iconRect);
                iconOffset = 32;
            }

            // 绘制标题
            if (!string.IsNullOrEmpty(Title))
            {
                using (var brush = new SolidBrush(ForeColor))
                {
                    Rectangle titleRect;
                    StringFormat format = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    // 根据对齐方式计算标题区域
                    if (TitleAlignment == ContentAlignment.MiddleCenter)
                    {
                        // 居中时使用整个标题栏宽度
                        titleRect = new Rectangle(iconOffset + 8, 0, Width - iconOffset - 16, Height);
                        format.Alignment = StringAlignment.Center;

                        // 检查是否会与按钮重叠
                        var textSize = g.MeasureString(Title, TitleFont ?? form.Font);
                        var centerX = Width / 2;
                        var textLeft = centerX - textSize.Width / 2;
                        var textRight = centerX + textSize.Width / 2;
                        var buttonsLeft = Width - GetButtonsWidth();

                        // 如果文本会与按钮重叠, 调整显示
                        if (textRight > buttonsLeft - 10)
                        {
                            // 限制在按钮区域之前
                            titleRect.Width = buttonsLeft - iconOffset - 20;
                        }
                    }
                    else
                    {
                        // 其他对齐方式保持原样
                        titleRect = new Rectangle(
                            8 + iconOffset,
                            0,
                            Width - iconOffset - GetButtonsWidth() - 16,
                            Height);
                        format.Alignment = GetHorizontalAlignment(TitleAlignment);
                    }

                    g.DrawString(Title, TitleFont ?? form.Font, brush, titleRect, format);
                }
            }

            // 绘制按钮
            foreach (var button in GetAllButtons())
            {
                button.Draw(g, theme);
            }
        }

        /// <summary>
        /// 创建顶部圆角路径
        /// </summary>
        private GraphicsPath CreateRoundedTopPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            // 左上角圆弧
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);

            // 右上角圆弧
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);

            // 右边直线
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom);

            // 底部直线
            path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);

            // 左边直线
            path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Y + radius);

            path.CloseFigure();
            return path;
        }

        #endregion

        #region 鼠标事件处理

        public bool OnMouseMove(MouseEventArgs e)
        {
            bool needInvalidate = false;

            foreach (var button in GetAllButtons())
            {
                bool wasHovered = button.IsHovered;
                button.IsHovered = button.Bounds.Contains(e.Location);
                if (wasHovered != button.IsHovered)
                {
                    needInvalidate = true;
                }
            }

            return needInvalidate;
        }

        public bool OnMouseDown(MouseEventArgs e)
        {
            var button = GetAllButtons().FirstOrDefault(b => b.Bounds.Contains(e.Location));
            if (button != null)
            {
                button.IsPressed = true;
                button.OnClick(form);
                return true;
            }
            return false;
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            foreach (var button in GetAllButtons())
            {
                button.IsPressed = false;
            }
        }

        #endregion

        #region 私有方法

        private List<FluentTitleBarButton> GetAllButtons()
        {
            var allButtons = new List<FluentTitleBarButton>();

            if (ShowCloseButton)
            {
                allButtons.Add(CloseButton);
            }

            if (ShowMaximizeButton)
            {
                allButtons.Add(MaximizeButton);
            }

            if (ShowMinimizeButton)
            {
                allButtons.Add(MinimizeButton);
            }

            allButtons.AddRange(buttons);

            return allButtons;
        }

        private int GetButtonsWidth()
        {
            return GetAllButtons().Sum(b => b.Width);
        }

        private StringAlignment GetHorizontalAlignment(ContentAlignment alignment)
        {
            switch (alignment)
            {
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    return StringAlignment.Center;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Near;
            }
        }

        #endregion
    }

    /// <summary>
    /// Fluent标题栏按钮
    /// </summary>
    public class FluentTitleBarButton
    {
        internal FluentForm form;
        private ContextMenuStrip dropDownMenu;
        private List<DropDownItem> dropDownItems;

        public FluentTitleBarButton()
        {
            dropDownItems = new List<DropDownItem>();
        }

        #region 属性

        public string Name { get; set; }
        public TitleBarButtonType Type { get; set; }
        public string Text { get; set; }
        public Image Icon { get; set; }
        public int Width { get; set; } = 46;
        public string ToolTip { get; set; }
        public bool EnableDropDown { get; set; }
        public Rectangle Bounds { get; set; }
        public bool IsHovered { get; set; }
        public bool IsPressed { get; set; }
        public bool IsEnabled { get; set; } = true;

        public event EventHandler Click;

        #endregion

        #region 方法

        public void ApplyTheme(IFluentTheme theme)
        {
            // 可以根据主题调整按钮样式
        }

        public void AddDropDownItem(string text, EventHandler handler)
        {
            dropDownItems.Add(new DropDownItem { Text = text, Handler = handler });
            EnableDropDown = true;
        }

        public void OnClick(Control parent)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (EnableDropDown)
            {
                ShowDropDown(parent);
            }
            else
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ShowDropDown(Control parent)
        {
            if (dropDownMenu == null)
            {
                dropDownMenu = new ContextMenuStrip();

                foreach (var item in dropDownItems)
                {
                    var menuItem = new ToolStripMenuItem(item.Text);
                    menuItem.Click += item.Handler;
                    dropDownMenu.Items.Add(menuItem);
                }
            }

            var showPoint = parent.PointToScreen(new Point(Bounds.Left, Bounds.Bottom));
            dropDownMenu.Show(showPoint);
        }

        public void Draw(Graphics g, IFluentTheme theme)
        {
            if (theme == null)
            {
                return;
            }

            // 绘制背景
            Color bgColor = Color.Transparent;

            if (!IsEnabled)
            {
                bgColor = Color.Transparent;
            }
            else if (IsPressed)
            {
                bgColor = Type == TitleBarButtonType.Close
                    ? Color.FromArgb(200, theme.Colors.Error)
                    : theme.Colors.GetColorWithOpacity(theme.Colors.TextOnPrimary, 0.2f);
            }
            else if (IsHovered)
            {
                bgColor = Type == TitleBarButtonType.Close
                    ? theme.Colors.Error
                    : theme.Colors.GetColorWithOpacity(theme.Colors.TextOnPrimary, 0.1f);
            }

            if (bgColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRectangle(brush, Bounds);
                }
            }

            // 根据按钮类型绘制图标
            if (Icon != null)
            {
                var iconRect = new Rectangle(
                    Bounds.X + (Bounds.Width - 16) / 2,
                    Bounds.Y + (Bounds.Height - 16) / 2,
                    16, 16);

                if (!IsEnabled)
                {
                    ControlPaint.DrawImageDisabled(g, Icon, iconRect.X, iconRect.Y, Color.Transparent);
                }
                else
                {
                    g.DrawImage(Icon, iconRect);
                }
            }
            else
            {
                // 使用自绘图形代替字符
                var color = IsEnabled
                    ? (IsHovered && Type == TitleBarButtonType.Close ? Color.White : theme.Colors.TextOnPrimary)
                    : theme.Colors.GetColorWithOpacity(theme.Colors.TextOnPrimary, 0.5f);

                DrawButtonIcon(g, Type, Bounds, color);
            }

            // 绘制下拉箭头
            if (EnableDropDown)
            {
                var arrowRect = new Rectangle(
                    Bounds.Right - 12,
                    Bounds.Y + (Bounds.Height - 8) / 2,
                    8, 8);

                using (var brush = new SolidBrush(theme.Colors.TextOnPrimary))
                {
                    var points = new Point[]
                    {
                new Point(arrowRect.Left, arrowRect.Top),
                new Point(arrowRect.Right, arrowRect.Top),
                new Point(arrowRect.Left + arrowRect.Width / 2, arrowRect.Bottom)
                    };
                    g.FillPolygon(brush, points);
                }
            }
        }

        private void DrawButtonIcon(Graphics g, TitleBarButtonType type, Rectangle bounds, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(color, 1.2f))
            using (var brush = new SolidBrush(color))
            {
                int centerX = bounds.X + bounds.Width / 2;
                int centerY = bounds.Y + bounds.Height / 2;

                switch (type)
                {
                    case TitleBarButtonType.Close:
                        // 绘制 X
                        int size = 10;
                        pen.Width = 1.5f;
                        g.DrawLine(pen,
                            centerX - size / 2, centerY - size / 2,
                            centerX + size / 2, centerY + size / 2);
                        g.DrawLine(pen,
                            centerX + size / 2, centerY - size / 2,
                            centerX - size / 2, centerY + size / 2);
                        break;

                    case TitleBarButtonType.Maximize:
                        // 根据窗口状态绘制不同图标
                        if (form != null && form.WindowState == FormWindowState.Maximized)
                        {
                            // 绘制还原图标(两个重叠的方框)
                            var rect1 = new Rectangle(centerX - 5, centerY - 5, 8, 8);
                            var rect2 = new Rectangle(centerX - 3, centerY - 3, 8, 8);

                            g.DrawRectangle(pen, rect2);

                            // 绘制后面的方框(只绘制可见部分)
                            g.DrawLine(pen, rect1.Left, rect1.Top, rect1.Right, rect1.Top);
                            g.DrawLine(pen, rect1.Right, rect1.Top, rect1.Right, rect2.Top);
                        }
                        else
                        {
                            // 绘制最大化图标(方框)
                            var rect = new Rectangle(centerX - 5, centerY - 5, 10, 10);
                            g.DrawRectangle(pen, rect);

                            // 绘制标题栏线
                            g.DrawLine(pen, rect.Left, rect.Top + 2, rect.Right, rect.Top + 2);
                        }
                        break;

                    case TitleBarButtonType.Minimize:
                        // 绘制最小化线
                        pen.Width = 1.5f;
                        g.DrawLine(pen,
                            centerX - 5, centerY,
                            centerX + 5, centerY);
                        break;

                    case TitleBarButtonType.Pin:
                        // 绘制图钉图标或使用文本
                        if (!string.IsNullOrEmpty(Text))
                        {
                            using (var font = new Font("Segoe UI Emoji", 12))
                            {
                                var stringFormat = new StringFormat
                                {
                                    Alignment = StringAlignment.Center,
                                    LineAlignment = StringAlignment.Center
                                };
                                g.DrawString(Text, font, brush, bounds, stringFormat);
                            }
                        }
                        break;

                    case TitleBarButtonType.Custom:
                        // 自定义按钮使用文本
                        if (!string.IsNullOrEmpty(Text))
                        {
                            Font font = IsEmoji(Text)
                                ? new Font("Segoe UI Emoji", 11)
                                : new Font("Segoe UI", 10);

                            using (font)
                            {
                                var stringFormat = new StringFormat
                                {
                                    Alignment = StringAlignment.Center,
                                    LineAlignment = StringAlignment.Center
                                };
                                g.DrawString(Text, font, brush, bounds, stringFormat);
                            }
                        }
                        break;
                }
            }

            g.SmoothingMode = SmoothingMode.Default;
        }


        private bool IsEmoji(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            // 简单判断：包含emoji常用的Unicode范围
            foreach (char c in text)
            {
                int code = (int)c;
                if (code >= 0x1F300 && code <= 0x1F9FF)
                {
                    return true; // Emoji范围
                }

                if (code >= 0x2600 && code <= 0x26FF)
                {
                    return true;   // 杂项符号
                }

                if (code >= 0x2700 && code <= 0x27BF)
                {
                    return true;   // 装饰符号
                }
            }

            // 检查特定的emoji字符
            return text.Contains("🎨") || text.Contains("📍") || text.Contains("📌") ||
                   text.Contains("☰") || text.Contains("⚙");
        }

        #endregion

        #region 内部类

        private class DropDownItem
        {
            public string Text { get; set; }
            public EventHandler Handler { get; set; }
        }

        #endregion
    }

    public enum TitleBarButtonType
    {
        Close,
        Maximize,
        Minimize,
        Pin,
        Settings,
        Custom
    }

}
