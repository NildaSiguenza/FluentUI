using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;

namespace FluentControls.Controls
{
    public class FluentButton : FluentControlBase, IButtonControl
    {
        private string text = "Button";
        private Image image;
        private ButtonStyle buttonStyle = ButtonStyle.Primary;
        private ContentLayout contentLayout = ContentLayout.ImageBeforeText;
        private ContentAlignment textAlign = ContentAlignment.MiddleCenter;
        private ContentAlignment imageAlign = ContentAlignment.MiddleCenter;
        private int imageTextSpacing = 8;
        private Keys shortcutKeys = Keys.None;
        private bool showKeyTips = true;

        private bool isDefault;
        private DialogResult dialogResult;

        // 状态相关的颜色
        private Color? normalBackColor;
        private Color? normalForeColor;
        private Color? hoverBackColor;
        private Color? hoverForeColor;
        private Color? pressedBackColor;
        private Color? pressedForeColor;
        private Color? disabledBackColor;
        private Color? disabledForeColor;

        #region 构造函数

        public FluentButton()
        {
            Size = new Size(100, 36);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.StandardClick, true);

            ButtonStyle = ButtonStyle.Primary;
        }

        #endregion

        #region 属性

        [Category("Fluent")]
        [Description("按钮显示的文本")]
        public override string Text
        {
            get => text;
            set
            {
                text = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("按钮显示的图像")]
        [Editor(typeof(ImageEditor), typeof(UITypeEditor))]
        public Image Image
        {
            get => image;
            set
            {
                image = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("按钮样式")]
        [DefaultValue(ButtonStyle.Primary)]
        public ButtonStyle ButtonStyle
        {
            get => buttonStyle;
            set
            {
                buttonStyle = value;
                OnThemeChanged();
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("内容布局方式")]
        [DefaultValue(ContentLayout.ImageBeforeText)]
        public ContentLayout ContentLayout
        {
            get => contentLayout;
            set
            {
                contentLayout = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("文本对齐方式")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment TextAlign
        {
            get => textAlign;
            set
            {
                textAlign = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("图像对齐方式")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment ImageAlign
        {
            get => imageAlign;
            set
            {
                imageAlign = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("图像和文本之间的间距")]
        [DefaultValue(8)]
        public int ImageTextSpacing
        {
            get => imageTextSpacing;
            set
            {
                imageTextSpacing = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("快捷键")]
        [DefaultValue(Keys.None)]
        public Keys ShortcutKeys
        {
            get => shortcutKeys;
            set => shortcutKeys = value;
        }

        [Category("Fluent")]
        [Description("是否显示快捷键提示")]
        [DefaultValue(true)]
        public bool ShowKeyTips
        {
            get => showKeyTips;
            set
            {
                showKeyTips = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public DialogResult DialogResult
        {
            get => dialogResult;
            set => dialogResult = value;
        }

        #region 状态颜色

        [Category("Colors")]
        [Description("正常状态背景色")]
        public Color? NormalBackColor
        {
            get => normalBackColor;
            set
            {
                normalBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("正常状态前景色")]
        public Color? NormalForeColor
        {
            get => normalForeColor;
            set
            {
                normalForeColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("悬停状态背景色")]
        public Color? HoverBackColor
        {
            get => hoverBackColor;
            set
            {
                hoverBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("悬停状态前景色")]
        public Color? HoverForeColor
        {
            get => hoverForeColor;
            set
            {
                hoverForeColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("按下状态背景色")]
        public Color? PressedBackColor
        {
            get => pressedBackColor;
            set
            {
                pressedBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("按下状态前景色")]
        public Color? PressedForeColor
        {
            get => pressedForeColor;
            set
            {
                pressedForeColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("禁用状态背景色")]
        public Color? DisabledBackColor
        {
            get => disabledBackColor;
            set
            {
                disabledBackColor = value;
                Invalidate();
            }
        }

        [Category("Colors")]
        [Description("禁用状态前景色")]
        public Color? DisabledForeColor
        {
            get => disabledForeColor;
            set
            {
                disabledForeColor = value;
                Invalidate();
            }
        }

        [Category("Fluent")]
        [Description("按钮圆角半径")]
        [DefaultValue(0)]
        public int CornerRadius { get; set; } = 0;

        DialogResult IButtonControl.DialogResult
        {
            get => dialogResult;
            set => dialogResult = value;
        }

        #endregion

        #endregion

        #region 事件

        [Category("Events")]
        [Description("按钮被按下时触发")]
        public event EventHandler ButtonPressed;

        [Category("Events")]
        [Description("按钮被释放时触发")]
        public event EventHandler ButtonReleased;

        #endregion

        #region IButtonControl接口

        public void PerformClick()
        {
            if (this.CanSelect)
            {
                this.OnClick(EventArgs.Empty);
            }
        }

        void IButtonControl.NotifyDefault(bool value)
        {
            if (isDefault != value)
            {
                isDefault = value;
                this.Invalidate();
            }
        }

        #endregion

        #region 重写方法

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Space)
            {
                OnClick(EventArgs.Empty);
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

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
            if (Theme != null)
            {
                BackColor = Theme.Colors.Primary;
                ForeColor = Theme.Colors.TextOnPrimary;
                Font = Theme.Typography.Button;
            }

            switch (ButtonStyle)
            {
                case ButtonStyle.Primary:
                    BackColor = normalBackColor ?? Theme.Colors.Primary;
                    ForeColor = normalForeColor ?? Theme.Colors.TextOnPrimary;
                    break;
                case ButtonStyle.Secondary:
                    BackColor = normalBackColor ?? Theme.Colors.Surface;
                    ForeColor = normalForeColor ?? Theme.Colors.Primary;
                    break;
                case ButtonStyle.Text:
                    BackColor = normalBackColor ?? Color.Transparent;
                    ForeColor = normalForeColor ?? Theme.Colors.Primary;
                    break;
                case ButtonStyle.Danger:
                    BackColor = normalBackColor ?? Theme.Colors.Error;
                    ForeColor = normalForeColor ?? Theme.Colors.TextOnPrimary;
                    break;
                case ButtonStyle.Success:
                    BackColor = normalBackColor ?? Theme.Colors.Success;
                    ForeColor = normalForeColor ?? Theme.Colors.TextOnPrimary;
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                ButtonPressed?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                ButtonReleased?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ShortcutKeys != Keys.None && keyData == ShortcutKeys)
            {
                PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void DrawBackground(Graphics g)
        {
            var rect = ClientRectangle;
            Color bgColor = GetBackgroundColor();

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (CornerRadius > 0)
            {
                using (var path = GetRoundedRectangle(rect, CornerRadius))
                {
                    using (var brush = new SolidBrush(bgColor))
                    {
                        g.FillPath(brush, path);
                    }

                    // 设置控件区域以支持圆角点击  
                    this.Region = new Region(path);
                }
            }
            else
            {
                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRectangle(brush, rect);
                }
                this.Region = null;
            }
        }

        protected override void DrawContent(Graphics g)
        {
            var rect = ClientRectangle;
            rect.Inflate(-Theme.Spacing.Small, -Theme.Spacing.Small);

            bool hasImage = Image != null;
            bool hasText = !string.IsNullOrEmpty(Text);

            if (!hasImage && !hasText)
            {
                return;
            }

            // 只有图像
            if (hasImage && !hasText)
            {
                DrawImageOnly(g, rect);
            }
            // 只有文本
            else if (!hasImage && hasText)
            {
                DrawTextOnly(g, rect);
            }
            // 图像和文本都有
            else
            {
                DrawImageAndText(g, rect);
            }

            // 绘制快捷键提示
            if (ShowKeyTips && ShortcutKeys != Keys.None && hasText)
            {
                DrawShortcutHint(g, rect);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void DrawBorder(Graphics g)
        {
            var rect = ClientRectangle;
            rect.Width--;
            rect.Height--;

            // 如果是默认按钮，绘制加粗边框
            if (isDefault && Enabled)
            {
                rect.Inflate(-1, -1);
                using (var path = GetRoundedRectangle(rect, CornerRadius))
                {
                    using (var pen = new Pen(Theme.Colors.PrimaryLight, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                rect.Inflate(-1, -1);
            }

            if (ButtonStyle == ButtonStyle.Secondary)
            {
                using (var path = GetRoundedRectangle(rect, CornerRadius))
                {
                    using (var pen = new Pen(GetBorderColor(), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // 绘制焦点框  
            if (Focused && State == ControlState.Focused)
            {
                rect.Inflate(-2, -2);
                using (var path = GetRoundedRectangle(rect, Math.Max(0, CornerRadius - 2)))
                {
                    using (var pen = new Pen(Theme.Colors.BorderFocused, 1))
                    {
                        pen.DashStyle = DashStyle.Dot;
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        #endregion

        #region 辅助方法

        private void DrawImageOnly(Graphics g, Rectangle rect)
        {
            var imageRect = GetAlignedRectangle(rect, Image.Size, ImageAlign);

            if (!Enabled && DisabledForeColor.HasValue)
            {
                // 绘制禁用状态的图像  
                System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
                try
                {
                    var matrix = new System.Drawing.Imaging.ColorMatrix();
                    matrix.Matrix33 = 0.5f; // 透明度  
                    attributes.SetColorMatrix(matrix);

                    g.DrawImage(Image, imageRect, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, attributes);
                }
                finally
                {
                    attributes.Dispose();
                }
            }
            else
            {
                g.DrawImage(Image, imageRect);
            }
        }

        private void DrawTextOnly(Graphics g, Rectangle rect)
        {
            using (var brush = new SolidBrush(GetForegroundColor()))
            {
                var format = GetStringFormat(TextAlign);
                g.DrawString(Text, Font, brush, rect, format);
            }
        }

        private void DrawImageAndText(Graphics g, Rectangle rect)
        {
            switch (ContentLayout)
            {
                case ContentLayout.ImageBeforeText:
                    DrawHorizontalLayout(g, rect, true);
                    break;
                case ContentLayout.TextBeforeImage:
                    DrawHorizontalLayout(g, rect, false);
                    break;
                case ContentLayout.ImageAboveText:
                    DrawVerticalLayout(g, rect, true);
                    break;
                case ContentLayout.TextAboveImage:
                    DrawVerticalLayout(g, rect, false);
                    break;
            }
        }

        private void DrawHorizontalLayout(Graphics g, Rectangle rect, bool imageFirst)
        {
            var textSize = TextRenderer.MeasureText(Text, Font);
            var imageSize = Image.Size;
            var totalWidth = imageSize.Width + ImageTextSpacing + textSize.Width;

            int startX = rect.X + (rect.Width - totalWidth) / 2;

            Rectangle imageRect, textRect;

            if (imageFirst)
            {
                imageRect = new Rectangle(startX,
                    rect.Y + (rect.Height - imageSize.Height) / 2,
                    imageSize.Width, imageSize.Height);
                textRect = new Rectangle(startX + imageSize.Width + ImageTextSpacing,
                    rect.Y, textSize.Width, rect.Height);
            }
            else
            {
                textRect = new Rectangle(startX, rect.Y, textSize.Width, rect.Height);
                imageRect = new Rectangle(startX + textSize.Width + ImageTextSpacing,
                    rect.Y + (rect.Height - imageSize.Height) / 2,
                    imageSize.Width, imageSize.Height);
            }

            g.DrawImage(Image, imageRect);

            using (var brush = new SolidBrush(GetForegroundColor()))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(Text, Font, brush, textRect, format);
            }
        }

        private void DrawVerticalLayout(Graphics g, Rectangle rect, bool imageFirst)
        {
            var textSize = TextRenderer.MeasureText(Text, Font);
            var imageSize = Image.Size;
            var totalHeight = imageSize.Height + ImageTextSpacing + textSize.Height;

            int startY = rect.Y + (rect.Height - totalHeight) / 2;

            Rectangle imageRect, textRect;

            if (imageFirst)
            {
                imageRect = new Rectangle(
                    rect.X + (rect.Width - imageSize.Width) / 2,
                    startY, imageSize.Width, imageSize.Height);
                textRect = new Rectangle(rect.X,
                    startY + imageSize.Height + ImageTextSpacing,
                    rect.Width, textSize.Height);
            }
            else
            {
                textRect = new Rectangle(rect.X, startY, rect.Width, textSize.Height);
                imageRect = new Rectangle(
                    rect.X + (rect.Width - imageSize.Width) / 2,
                    startY + textSize.Height + ImageTextSpacing,
                    imageSize.Width, imageSize.Height);
            }

            g.DrawImage(Image, imageRect);

            using (var brush = new SolidBrush(GetForegroundColor()))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(Text, Font, brush, textRect, format);
            }
        }

        private void DrawShortcutHint(Graphics g, Rectangle rect)
        {
            var shortcutText = GetShortcutText(ShortcutKeys);
            if (string.IsNullOrEmpty(shortcutText))
            {
                return;
            }

            using (var font = new Font(Font.FontFamily, Font.Size * 0.8f))
            {
                var textSize = TextRenderer.MeasureText(shortcutText, font);

                var hintRect = new Rectangle(
                    rect.Right - textSize.Width - 5,
                    rect.Bottom - textSize.Height - 2,
                    textSize.Width,
                    textSize.Height);

                using (var brush = new SolidBrush(
                    Theme.Colors.GetColorWithOpacity(GetForegroundColor(), 0.7f)))
                {
                    g.DrawString(shortcutText, font, brush, hintRect);
                }
            }
        }

        private Color GetBackgroundColor()
        {
            if (!Enabled)
            {
                return disabledBackColor ?? Theme.Colors.GetColorWithOpacity(BackColor, 0.5f);
            }

            switch (State)
            {
                case ControlState.Hover:
                    return hoverBackColor ??
                           (ButtonStyle == ButtonStyle.Text
                               ? Theme.Colors.GetColorWithOpacity(Theme.Colors.Primary, 0.1f)
                               : ControlPaint.Light(BackColor, 0.1f));
                case ControlState.Pressed:
                    return pressedBackColor ??
                           (ButtonStyle == ButtonStyle.Text
                               ? Theme.Colors.GetColorWithOpacity(Theme.Colors.Primary, 0.2f)
                               : ControlPaint.Dark(BackColor, 0.1f));
                default:
                    return normalBackColor ?? BackColor;
            }
        }

        private Color GetForegroundColor()
        {
            if (!Enabled)
            {
                return disabledForeColor ?? Theme.Colors.TextDisabled;
            }

            switch (State)
            {
                case ControlState.Hover:
                    return hoverForeColor ?? ForeColor;
                case ControlState.Pressed:
                    return pressedForeColor ?? ForeColor;
                default:
                    return normalForeColor ?? ForeColor;
            }
        }

        private Color GetBorderColor()
        {
            if (!Enabled)
            {
                return Theme.Colors.BorderLight;
            }

            switch (State)
            {
                case ControlState.Hover:
                    return Theme.Colors.Primary;
                case ControlState.Pressed:
                    return Theme.Colors.PrimaryDark;
                case ControlState.Focused:
                    return Theme.Colors.BorderFocused;
                default:
                    return Theme.Colors.Border;
            }
        }

        private Rectangle GetAlignedRectangle(Rectangle container, Size size, ContentAlignment alignment)
        {
            int x = container.X;
            int y = container.Y;

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    break;
                case ContentAlignment.TopCenter:
                    x = container.X + (container.Width - size.Width) / 2;
                    break;
                case ContentAlignment.TopRight:
                    x = container.Right - size.Width;
                    break;
                case ContentAlignment.MiddleLeft:
                    y = container.Y + (container.Height - size.Height) / 2;
                    break;
                case ContentAlignment.MiddleCenter:
                    x = container.X + (container.Width - size.Width) / 2;
                    y = container.Y + (container.Height - size.Height) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    x = container.Right - size.Width;
                    y = container.Y + (container.Height - size.Height) / 2;
                    break;
                case ContentAlignment.BottomLeft:
                    y = container.Bottom - size.Height;
                    break;
                case ContentAlignment.BottomCenter:
                    x = container.X + (container.Width - size.Width) / 2;
                    y = container.Bottom - size.Height;
                    break;
                case ContentAlignment.BottomRight:
                    x = container.Right - size.Width;
                    y = container.Bottom - size.Height;
                    break;
            }

            return new Rectangle(x, y, size.Width, size.Height);
        }

        private StringFormat GetStringFormat(ContentAlignment alignment)
        {
            var format = new StringFormat();

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Far;
                    break;
            }

            return format;
        }

        private string GetShortcutText(Keys keys)
        {
            if (keys == Keys.None)
            {
                return "";
            }

            var converter = new KeysConverter();
            return converter.ConvertToString(keys);
        }

        #endregion
    }

    /// <summary>
    /// 按钮样式枚举
    /// </summary>
    public enum ButtonStyle
    {
        Primary,    // 主要按钮
        Secondary,  // 次要按钮
        Text,       // 文本按钮
        Danger,     // 危险按钮
        Success     // 成功按钮
    }

    /// <summary>
    /// 内容布局方式
    /// </summary>
    public enum ContentLayout
    {
        ImageBeforeText,  // 图像在文本前(水平)
        TextBeforeImage,  // 文本在图像前(水平)
        ImageAboveText,   // 图像在文本上(垂直)
        TextAboveImage    // 文本在图像上(垂直)
    }
}


