using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    [DefaultEvent("ColorChanged")]
    [DefaultProperty("SelectedColor")]
    public class FluentColorComboBox : FluentControlBase
    {
        private Color selectedColor = Color.FromArgb(0, 120, 212);

        // 外观
        private ColorBlockShape blockShape = ColorBlockShape.Square;
        private ColorTextFormat textFormat = ColorTextFormat.Hex;
        private int blockSize = 18;
        private int spacing = 8;
        private bool showColorText = true;

        // 边框相关
        private bool showBorder = true;
        private int borderSize = 1;
        private Color borderColor = Color.Gray;
        private bool useBorderThemeColor = true;

        // 文本显示
        private Rectangle colorBlockRect;
        private Rectangle textRect;

        // 下拉面板
        private ColorListPanel listPanel;
        private ToolStripDropDown dropDown;
        private ToolStripControlHost dropDownHost;
        private bool isDroppedDown = false;

        // 预置颜色列表
        private List<ColorItem> colorItems;

        // 交互状态
        private bool isHovering = false;
        private bool isPressed = false;
        private bool autoResizeWidth = true;

        public event EventHandler ColorChanged;
        public event EventHandler<ColorChangingEventArgs> ColorChanging;

        #region 构造函数

        public FluentColorComboBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.Selectable, true);

            InitializeDefaultColors();
            if (colorItems != null && colorItems.Count > 0)
            {
                selectedColor = colorItems[0].Color;
            }

            Cursor = Cursors.Hand;
            int initialWidth = CalculatePreferredWidth();
            Size = new Size(initialWidth, 28);

            InitializeComponents();
            UpdateLayout();
        }

        private void InitializeDefaultColors()
        {
            colorItems = new List<ColorItem>
            {
                new ColorItem(Color.FromArgb(0, 120, 212), "蓝色"),
                new ColorItem(Color.FromArgb(32, 32, 32), "黑色"),
                new ColorItem(Color.FromArgb(76, 175, 80), "绿色"),
                new ColorItem(Color.FromArgb(255, 255, 0), "黄色"),
                new ColorItem(Color.FromArgb(156, 39, 176), "紫色")
            };
        }

        private void InitializeComponents()
        {
            // 创建颜色列表面板
            listPanel = new ColorListPanel(colorItems);
            listPanel.ShowColorText = showColorText;
            listPanel.BlockShape = blockShape;
            listPanel.ColorSelected += (s, item) =>
            {
                SetColor(item.Color, true);
                CloseDropDown();
            };
        }

        #endregion

        #region 属性

        [Category("Color")]
        [Description("选中的颜色")]
        public Color SelectedColor
        {
            get => selectedColor;
            set => SetColor(value, true);
        }

        [Category("Appearance")]
        [DefaultValue(ColorBlockShape.Square)]
        [Description("颜色块形状")]
        public ColorBlockShape BlockShape
        {
            get => blockShape;
            set
            {
                if (blockShape != value)
                {
                    blockShape = value;
                    if (listPanel != null)
                    {
                        listPanel.BlockShape = value;
                    }
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(ColorTextFormat.Hex)]
        [Description("颜色文本格式")]
        public ColorTextFormat TextFormat
        {
            get => textFormat;
            set
            {
                if (textFormat != value)
                {
                    textFormat = value;
                    if (listPanel != null)
                    {
                        listPanel.TextFormat = value;
                    }

                    if (showColorText)
                    {
                        AdjustWidthToFit();
                    }

                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(18)]
        [Description("颜色块大小")]
        public int BlockSize
        {
            get => blockSize;
            set
            {
                if (blockSize != value && value > 0)
                {
                    blockSize = value;

                    if (showColorText)
                    {
                        AdjustWidthToFit();
                    }

                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否显示颜色文本")]
        public bool ShowColorText
        {
            get => showColorText;
            set
            {
                if (showColorText != value)
                {
                    showColorText = value;
                    if (listPanel != null)
                    {
                        listPanel.ShowColorText = value;
                    }

                    // 自动调整控件宽度
                    AdjustWidthToFit();

                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [DefaultValue(true)]
        [Description("是否显示边框")]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                if (showBorder != value)
                {
                    showBorder = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [DefaultValue(1)]
        [Description("边框粗细")]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                if (borderSize != value && value > 0 && value <= 5)
                {
                    borderSize = value;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Border")]
        [DefaultValue(typeof(Color), "Gray")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    useBorderThemeColor = false;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Border")]
        [DefaultValue(true)]
        [Description("是否使用主题边框颜色")]
        public bool UseBorderThemeColor
        {
            get => useBorderThemeColor;
            set
            {
                if (useBorderThemeColor != value)
                {
                    useBorderThemeColor = value;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(8)]
        [Description("颜色块与文本的间距")]
        public int Spacing
        {
            get => spacing;
            set
            {
                if (spacing != value && value >= 0)
                {
                    spacing = value;

                    if (showColorText)
                    {
                        AdjustWidthToFit();
                    }

                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否在内容改变时自动调整宽度")]
        public bool AutoResizeWidth
        {
            get => autoResizeWidth;
            set
            {
                if (autoResizeWidth != value)
                {
                    autoResizeWidth = value;
                    if (value && showColorText)
                    {
                        AdjustWidthToFit();
                    }
                }
            }
        }

        /// <summary>
        /// 获取颜色项集合（只读）
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<ColorItem> ColorItems => colorItems.AsReadOnly();

        #endregion

        #region 颜色列表管理

        /// <summary>
        /// 添加颜色项
        /// </summary>
        public void AddColor(Color color, string name = null)
        {
            var item = new ColorItem(color, name);
            colorItems.Add(item);
            listPanel?.RefreshColors(colorItems);
        }

        /// <summary>
        /// 添加多个颜色项
        /// </summary>
        public void AddColors(params ColorItem[] items)
        {
            if (items != null && items.Length > 0)
            {
                colorItems.AddRange(items);
                listPanel?.RefreshColors(colorItems);
            }
        }

        /// <summary>
        /// 移除颜色项
        /// </summary>
        public void RemoveColor(Color color)
        {
            colorItems.RemoveAll(item => item.Color == color);
            listPanel?.RefreshColors(colorItems);
        }

        /// <summary>
        /// 清空所有颜色项
        /// </summary>
        public void ClearColors()
        {
            colorItems.Clear();
            listPanel?.RefreshColors(colorItems);
        }

        /// <summary>
        /// 替换所有颜色项
        /// </summary>
        public void SetColors(params ColorItem[] items)
        {
            colorItems.Clear();
            if (items != null && items.Length > 0)
            {
                colorItems.AddRange(items);
            }
            listPanel?.RefreshColors(colorItems);
        }

        /// <summary>
        /// 重置为默认颜色
        /// </summary>
        public void ResetToDefaultColors()
        {
            InitializeDefaultColors();
            listPanel?.RefreshColors(colorItems);
        }

        #endregion

        #region 事件

        protected virtual void OnColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnColorChanging(ColorChangingEventArgs e)
        {
            ColorChanging?.Invoke(this, e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (showColorText)
            {
                AdjustWidthToFit();
            }

            UpdateLayout();
        }

        #endregion

        #region 颜色管理

        private void SetColor(Color color, bool raiseEvent)
        {
            if (selectedColor != color)
            {
                if (raiseEvent)
                {
                    var args = new ColorChangingEventArgs(selectedColor, color);
                    OnColorChanging(args);
                    if (args.Cancel)
                    {
                        return;
                    }
                }

                selectedColor = color;

                // 根据文本内容调整宽度
                if (showColorText)
                {
                    AdjustWidthToFit();
                }

                UpdateLayout();
                Invalidate();

                if (raiseEvent)
                {
                    OnColorChanged();
                }
            }
        }

        private string GetColorText()
        {
            // 优先查找命名颜色 - 使用颜色值精确匹配
            var item = colorItems.FirstOrDefault(i =>
                i.Color.R == selectedColor.R &&
                i.Color.G == selectedColor.G &&
                i.Color.B == selectedColor.B &&
                i.Color.A == selectedColor.A);

            if (item != null && !string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }

            // 否则根据格式显示
            switch (textFormat)
            {
                case ColorTextFormat.Hex:
                    return $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";

                case ColorTextFormat.RGB:
                    return $"RGB({selectedColor.R}, {selectedColor.G}, {selectedColor.B})";

                case ColorTextFormat.ARGB:
                    return $"R:{selectedColor.R} G:{selectedColor.G} B:{selectedColor.B}";

                default:
                    return selectedColor.Name;
            }
        }

        #endregion

        #region 布局

        private void UpdateLayout()
        {
            // 计算颜色块位置
            int yPos = (Height - blockSize) / 2;
            colorBlockRect = new Rectangle(8, yPos, blockSize, blockSize);

            // 计算文本位置
            if (showColorText)
            {
                string colorText = GetColorText();
                Size textSize = TextRenderer.MeasureText(colorText, Font);

                textRect = new Rectangle(
                    colorBlockRect.Right + spacing,
                    (Height - textSize.Height) / 2,
                    Width - colorBlockRect.Right - spacing - 30, // 留空间给箭头
                    textSize.Height);
            }
            else
            {
                textRect = Rectangle.Empty;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        /// <summary>
        /// 计算控件的最适宽度
        /// </summary>
        private int CalculatePreferredWidth()
        {
            if (!showColorText)
            {
                // 只显示色块：左边距 + 色块 + 间距 + 箭头区域 + 右边距
                return 8 + blockSize + spacing + 20 + 8;
            }
            else
            {
                // 显示文本：需要测量当前颜色文本的宽度
                string text = GetColorText();
                Size textSize;

                using (var g = CreateGraphics())
                {
                    textSize = TextRenderer.MeasureText(g, text, Font);
                }

                // 左边距 + 色块 + 间距 + 文本宽度 + 间距 + 箭头区域 + 右边距
                int width = 8 + blockSize + spacing + textSize.Width + spacing + 20 + 8;

                // 设置最小宽度
                return Math.Max(width, 100);
            }
        }

        /// <summary>
        /// 调整控件宽度以适应内容
        /// </summary>
        private void AdjustWidthToFit()
        {
            if (!autoResizeWidth)
            {
                return;
            }

            int preferredWidth = CalculatePreferredWidth();

            // 只修改宽度，保持高度不变
            if (Width != preferredWidth)
            {
                Width = preferredWidth;
            }
        }

        /// <summary>
        /// 手动触发宽度自动调整
        /// </summary>
        public void AdjustWidth()
        {
            AdjustWidthToFit();
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovering = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                ShowDropDown();
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isPressed = false;
            Invalidate();
        }

        #endregion

        #region 下拉面板

        private void ShowDropDown()
        {
            if (isDroppedDown)
            {
                return;
            }

            if (dropDown == null)
            {
                dropDown = new ToolStripDropDown();
                dropDown.AutoSize = false;
                dropDown.Margin = Padding.Empty;
                dropDown.Padding = new Padding(1);

                dropDownHost = new ToolStripControlHost(listPanel);
                dropDownHost.Margin = Padding.Empty;
                dropDownHost.Padding = Padding.Empty;
                dropDownHost.AutoSize = false;

                dropDown.Items.Add(dropDownHost);
            }

            // 设置选中项
            listPanel.SelectColor(selectedColor);

            // 设置面板大小
            Size panelSize = listPanel.GetPreferredSize();

            int dropDownWidth = Width - 2;
            dropDownHost.Size = new Size(dropDownWidth, panelSize.Height);
            dropDown.Size = new Size(Width, panelSize.Height + 2);

            // 显示下拉面板
            dropDown.Show(this, new Point(0, Height));
            isDroppedDown = true;

            dropDown.Closed += (s, e) =>
            {
                isDroppedDown = false;
                isPressed = false;
                Invalidate();
            };
        }

        private void CloseDropDown()
        {
            if (dropDown != null && isDroppedDown)
            {
                dropDown.Close();
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var rect = ClientRectangle;
            Color bgColor = BackColor;

            if (UseTheme && Theme != null)
            {
                if (isPressed || isDroppedDown)
                {
                    bgColor = GetThemeColor(c => c.SurfacePressed, bgColor);
                }
                else if (isHovering)
                {
                    bgColor = GetThemeColor(c => c.SurfaceHover, bgColor);
                }
                else
                {
                    bgColor = GetThemeColor(c => c.Surface, bgColor);
                }
            }

            using (var brush = new SolidBrush(bgColor))
            {
                if (UseTheme && Theme?.Elevation?.CornerRadius > 0)
                {
                    using (var path = GetRoundedRectangle(rect, Theme.Elevation.CornerRadius))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    g.FillRectangle(brush, rect);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制颜色块
            DrawColorBlock(g);

            // 绘制文本
            if (showColorText && !textRect.IsEmpty)
            {
                DrawColorText(g);
            }

            // 绘制下拉箭头
            DrawDropDownArrow(g);
        }

        private void DrawColorBlock(Graphics g)
        {
            // 先绘制棋盘格背景
            DrawCheckerboard(g, colorBlockRect);

            // 绘制颜色
            using (var brush = new SolidBrush(selectedColor))
            {
                switch (blockShape)
                {
                    case ColorBlockShape.Circle:
                        g.FillEllipse(brush, colorBlockRect);
                        break;

                    case ColorBlockShape.Square:
                        g.FillRectangle(brush, colorBlockRect);
                        break;

                    case ColorBlockShape.RoundedSquare:
                        using (var path = GetRoundedRectangle(colorBlockRect, 4))
                        {
                            g.FillPath(brush, path);
                        }
                        break;
                }
            }

            // 绘制色块边框
            using (var pen = new Pen(Color.FromArgb(200, Color.Gray), 1))
            {
                switch (blockShape)
                {
                    case ColorBlockShape.Circle:
                        g.DrawEllipse(pen, colorBlockRect);
                        break;

                    case ColorBlockShape.Square:
                        g.DrawRectangle(pen, colorBlockRect);
                        break;

                    case ColorBlockShape.RoundedSquare:
                        using (var path = GetRoundedRectangle(colorBlockRect, 4))
                        {
                            g.DrawPath(pen, path);
                        }
                        break;
                }
            }
        }

        private void DrawCheckerboard(Graphics g, Rectangle rect)
        {
            const int checkSize = 4;
            using (var lightBrush = new SolidBrush(Color.White))
            using (var darkBrush = new SolidBrush(Color.LightGray))
            {
                g.FillRectangle(lightBrush, rect);

                for (int y = rect.Top; y < rect.Bottom; y += checkSize)
                {
                    for (int x = rect.Left; x < rect.Right; x += checkSize)
                    {
                        if ((x - rect.Left) / checkSize % 2 == (y - rect.Top) / checkSize % 2)
                        {
                            var checkRect = new Rectangle(x, y,
                                Math.Min(checkSize, rect.Right - x),
                                Math.Min(checkSize, rect.Bottom - y));
                            g.FillRectangle(darkBrush, checkRect);
                        }
                    }
                }
            }
        }

        private void DrawColorText(Graphics g)
        {
            string text = GetColorText();
            Color textColor = ForeColor;

            if (UseTheme && Theme != null)
            {
                textColor = GetThemeColor(c => c.TextPrimary, textColor);
            }

            TextRenderer.DrawText(g, text, Font, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private void DrawDropDownArrow(Graphics g)
        {
            int arrowSize = 4; 
            int arrowX = Width - arrowSize - 10;
            int arrowY = Height / 2;

            Color arrowColor = ForeColor;
            if (UseTheme && Theme != null)
            {
                arrowColor = GetThemeColor(c => c.TextSecondary, arrowColor);
            }

            Point[] arrow = new Point[]
            {
                new Point(arrowX - arrowSize, arrowY - 2),
                new Point(arrowX + arrowSize, arrowY - 2),
                new Point(arrowX, arrowY + 2) 
            };

            using (var brush = new SolidBrush(arrowColor))
            {
                g.FillPolygon(brush, arrow);
            }
        }
        protected override void DrawBorder(Graphics g)
        {
            if (!showBorder)
            {
                return;
            }

            int offset = borderSize / 2;
            var rect = new Rectangle(offset, offset, Width - borderSize, Height - borderSize);

            Color actualBorderColor = borderColor;

            if (useBorderThemeColor && UseTheme && Theme != null)
            {
                if (isDroppedDown)
                {
                    actualBorderColor = GetThemeColor(c => c.Primary, Color.Blue);
                }
                else if (isHovering)
                {
                    actualBorderColor = GetThemeColor(c => c.BorderFocused, Color.DarkGray);
                }
                else
                {
                    actualBorderColor = GetThemeColor(c => c.Border, Color.Gray);
                }
            }

            int actualBorderSize = (isDroppedDown && useBorderThemeColor) ? Math.Min(borderSize + 1, 3) : borderSize;

            using (var pen = new Pen(actualBorderColor, actualBorderSize))
            {
                if (UseTheme && Theme?.Elevation?.CornerRadius > 0)
                {
                    using (var path = GetRoundedRectangle(rect, Theme.Elevation.CornerRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        public void ResetBorderColor()
        {
            borderColor = Color.Gray;
            useBorderThemeColor = true;
            if (showBorder)
            {
                Invalidate();
            }
        }

        #endregion

        #region 主题

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
        }

        protected override void ApplyThemeStyles()
        {
            if (Theme == null)
            {
                return;
            }

            BackColor = GetThemeColor(c => c.Surface, BackColor);
            ForeColor = GetThemeColor(c => c.TextPrimary, ForeColor);
            Font = GetThemeFont(t => t.Body, Font);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dropDown?.Dispose();
                listPanel?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 颜色列表面板

    /// <summary>
    /// 颜色列表下拉面板
    /// </summary>
    internal class ColorListPanel : UserControl
    {
        private ListBox colorListBox;
        private List<ColorItem> items;
        private bool showColorText = true;
        private ColorBlockShape blockShape = ColorBlockShape.Square;
        private ColorTextFormat textFormat = ColorTextFormat.Hex;
        private const int ItemHeight = 26;
        private const int BlockSize = 18;
        private const int Spacing = 8;

        public event EventHandler<ColorItem> ColorSelected;

        public bool ShowColorText
        {
            get => showColorText;
            set
            {
                if (showColorText != value)
                {
                    showColorText = value;
                    colorListBox?.Invalidate();
                }
            }
        }

        public ColorBlockShape BlockShape
        {
            get => blockShape;
            set
            {
                if (blockShape != value)
                {
                    blockShape = value;
                    colorListBox?.Invalidate();
                }
            }
        }

        public ColorTextFormat TextFormat
        {
            get => textFormat;
            set
            {
                if (textFormat != value)
                {
                    textFormat = value;
                    colorListBox?.Invalidate();
                }
            }
        }

        public ColorListPanel(List<ColorItem> colorItems)
        {
            items = colorItems;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;

            colorListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = ItemHeight,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false
            };

            colorListBox.DrawItem += OnDrawItem;
            colorListBox.SelectedIndexChanged += OnSelectedIndexChanged;
            colorListBox.MouseClick += OnMouseClick;

            RefreshColors(items);

            Controls.Add(colorListBox);
        }

        public void RefreshColors(List<ColorItem> colorItems)
        {
            items = colorItems;
            colorListBox.Items.Clear();
            foreach (var item in items)
            {
                colorListBox.Items.Add(item);
            }
        }

        public void SelectColor(Color color)
        {
            for (int i = 0; i < items.Count; i++)
            {
                // 精确比较ARGB值
                if (items[i].Color.R == color.R &&
                    items[i].Color.G == color.G &&
                    items[i].Color.B == color.B &&
                    items[i].Color.A == color.A)
                {
                    colorListBox.SelectedIndex = i;
                    return;
                }
            }

            // 如果没找到精确匹配，取消选中
            colorListBox.SelectedIndex = -1;
        }

        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= items.Count)
            {
                return;
            }

            var item = items[e.Index];
            e.DrawBackground();

            // 计算颜色块位置
            var blockRect = new Rectangle(
                e.Bounds.X + 4,
                e.Bounds.Y + (ItemHeight - BlockSize) / 2,
                BlockSize,
                BlockSize);

            // 绘制棋盘格背景
            DrawCheckerboard(e.Graphics, blockRect);

            // 绘制颜色块
            using (var brush = new SolidBrush(item.Color))
            {
                switch (blockShape)
                {
                    case ColorBlockShape.Circle:
                        e.Graphics.FillEllipse(brush, blockRect);
                        break;

                    case ColorBlockShape.Square:
                        e.Graphics.FillRectangle(brush, blockRect);
                        break;

                    case ColorBlockShape.RoundedSquare:
                        using (var path = GetRoundedRectangle(blockRect, 3))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                        break;
                }
            }

            // 绘制色块边框
            using (var pen = new Pen(Color.FromArgb(150, Color.Gray), 1))
            {
                switch (blockShape)
                {
                    case ColorBlockShape.Circle:
                        e.Graphics.DrawEllipse(pen, blockRect);
                        break;

                    case ColorBlockShape.Square:
                        e.Graphics.DrawRectangle(pen, blockRect);
                        break;

                    case ColorBlockShape.RoundedSquare:
                        using (var path = GetRoundedRectangle(blockRect, 3))
                        {
                            e.Graphics.DrawPath(pen, path);
                        }
                        break;
                }
            }

            // 绘制文本
            if (showColorText)
            {
                var textRect = new Rectangle(
                    blockRect.Right + Spacing,
                    e.Bounds.Y,
                    e.Bounds.Width - blockRect.Right - Spacing - 4,
                    e.Bounds.Height);

                string text = GetColorText(item);
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? SystemColors.HighlightText
                    : SystemColors.ControlText;

                TextRenderer.DrawText(e.Graphics, text, e.Font, textRect, textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            e.DrawFocusRectangle();
        }

        private void DrawCheckerboard(Graphics g, Rectangle rect)
        {
            const int checkSize = 4;
            using (var lightBrush = new SolidBrush(Color.White))
            using (var darkBrush = new SolidBrush(Color.LightGray))
            {
                g.FillRectangle(lightBrush, rect);

                for (int y = rect.Top; y < rect.Bottom; y += checkSize)
                {
                    for (int x = rect.Left; x < rect.Right; x += checkSize)
                    {
                        if ((x - rect.Left) / checkSize % 2 == (y - rect.Top) / checkSize % 2)
                        {
                            var checkRect = new Rectangle(x, y,
                                Math.Min(checkSize, rect.Right - x),
                                Math.Min(checkSize, rect.Bottom - y));
                            g.FillRectangle(darkBrush, checkRect);
                        }
                    }
                }
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
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

        private string GetColorText(ColorItem item)
        {
            // 优先使用自定义名称
            if (!string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }

            // 否则根据格式显示
            var color = item.Color;
            switch (textFormat)
            {
                case ColorTextFormat.Hex:
                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}";

                case ColorTextFormat.RGB:
                    return $"RGB({color.R}, {color.G}, {color.B})";

                case ColorTextFormat.ARGB:
                    return $"R:{color.R} G:{color.G} B:{color.B}";

                default:
                    return color.Name;
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            // 可以在这里添加预览效果
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && colorListBox.SelectedIndex >= 0)
            {
                var item = items[colorListBox.SelectedIndex];
                OnColorSelected(item);
            }
        }

        protected virtual void OnColorSelected(ColorItem item)
        {
            ColorSelected?.Invoke(this, item);
        }

        public Size GetPreferredSize()
        {
            int itemCount = Math.Min(items.Count, 8);
            if (itemCount == 0)
            {
                itemCount = 1;
            }

            int height = itemCount * ItemHeight + 2; 
            int width = this.Width > 0 ? this.Width : 200;

            return new Size(width, height);
        }
    }

    #endregion

    #region 颜色项类

    /// <summary>
    /// 颜色项
    /// </summary>
    public class ColorItem
    {
        public Color Color { get; set; }
        public string Name { get; set; }

        public ColorItem(Color color, string name = null)
        {
            Color = color;
            Name = name ?? GetDefaultName(color);
        }

        private static string GetDefaultName(Color color)
        {
            if (color.IsNamedColor)
            {
                return color.Name;
            }
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public override string ToString()
        {
            return Name ?? Color.ToString();
        }
    }

    #endregion
}
