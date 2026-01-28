using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{
    [DefaultEvent("ActionClick")]
    [Designer(typeof(FluentCardDesigner))]
    public class FluentCard : FluentContainerBase
    {
        #region 字段

        private CardLayout layout = CardLayout.LeftImageRightContent;
        private Image cardImage;
        private CardImageSizeMode imageSizeMode = CardImageSizeMode.Auto;
        private Size imageSize = new Size(120, 120);

        private string title = "Card Title";
        private string subtitle = "";
        private Font titleFont;
        private Font subtitleFont;
        private Color titleColor = Color.Empty;
        private Color subtitleColor = Color.Empty;

        private CardActionCollection actions;
        private int actionButtonHeight = 32;
        private int actionButtonMinWidth = 80;
        private int actionSpacing = 8;

        private bool showBorder = true;
        private int borderWidth = 1;
        private Color borderColor = Color.Empty;
        private int cornerRadius = 4;

        private bool showShadow = true;
        private int shadowLevel = 2;

        private Padding contentPadding = new Padding(16);
        private int elementSpacing = 12;

        // 内部控件缓存
        private Dictionary<CardActionItem, Control> actionControls = new Dictionary<CardActionItem, Control>();

        #endregion

        #region 构造函数

        public FluentCard()
        {
            actions = new CardActionCollection(this);

            Size = new Size(300, 200);
            BackColor = Color.White;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        /// <summary>
        /// 卡片布局
        /// </summary>
        [Category("Layout")]
        [DefaultValue(CardLayout.LeftImageRightContent)]
        [Description("卡片的布局模式")]
        public new CardLayout Layout
        {
            get => layout;
            set
            {
                if (layout != value)
                {
                    layout = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 内容边距
        /// </summary>
        [Category("Layout")]
        [Description("卡片内容的内边距")]
        [DefaultValue(typeof(Padding), "16, 16, 16, 16")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Padding Padding
        {
            get => contentPadding;
            set
            {
                if (contentPadding != value)
                {
                    contentPadding = value;
                    OnPaddingChanged(EventArgs.Empty);
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        private void ResetPadding()
        {
            Padding = new Padding(16);
        }

        private bool ShouldSerializePadding()
        {
            return !contentPadding.Equals(new Padding(16, 16, 16, 16));
        }

        /// <summary>
        /// 元素间距
        /// </summary>
        [Category("Layout")]
        [DefaultValue(12)]
        [Description("卡片内元素之间的间距")]
        public int ElementSpacing
        {
            get => elementSpacing;
            set
            {
                if (elementSpacing != value && value >= 0)
                {
                    elementSpacing = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 卡片图片
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [Description("卡片显示的图片")]
        public Image CardImage
        {
            get => cardImage;
            set
            {
                if (cardImage != value)
                {
                    cardImage = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片大小模式
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(CardImageSizeMode.Auto)]
        [Description("图片的大小调整模式")]
        public CardImageSizeMode ImageSizeMode
        {
            get => imageSizeMode;
            set
            {
                if (imageSizeMode != value)
                {
                    imageSizeMode = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片大小(固定模式下使用)
        /// </summary>
        [Category("Appearance")]
        [Description("图片的固定大小")]
        public Size ImageSize
        {
            get => imageSize;
            set
            {
                if (imageSize != value)
                {
                    imageSize = value;
                    if (imageSizeMode == CardImageSizeMode.Fixed)
                    {
                        PerformLayout();
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 卡片标题
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("Card Title")]
        [Description("卡片的标题文本")]
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 卡片副标题
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("卡片的副标题文本")]
        public string Subtitle
        {
            get => subtitle;
            set
            {
                if (subtitle != value)
                {
                    subtitle = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 标题字体
        /// </summary>
        [Category("Appearance")]
        [Description("标题的字体")]
        public Font TitleFont
        {
            get => titleFont ?? GetDefaultTitleFont();
            set
            {
                titleFont = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 副标题字体
        /// </summary>
        [Category("Appearance")]
        [Description("副标题的字体")]
        public Font SubtitleFont
        {
            get => subtitleFont ?? GetDefaultSubtitleFont();
            set
            {
                subtitleFont = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 标题颜色
        /// </summary>
        [Category("Appearance")]
        [Description("标题的颜色")]
        public Color TitleColor
        {
            get => titleColor.IsEmpty ? GetDefaultTitleColor() : titleColor;
            set
            {
                titleColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 副标题颜色
        /// </summary>
        [Category("Appearance")]
        [Description("副标题的颜色")]
        public Color SubtitleColor
        {
            get => subtitleColor.IsEmpty ? GetDefaultSubtitleColor() : subtitleColor;
            set
            {
                subtitleColor = value;
                Invalidate();
            }
        }

        private Font GetDefaultTitleFont()
        {
            if (UseTheme && Theme != null)
            {
                return GetThemeFont(t => t.Title, new Font(Font.FontFamily, Font.Size + 2, FontStyle.Bold));
            }
            return new Font(Font.FontFamily, Font.Size + 2, FontStyle.Bold);
        }

        private Font GetDefaultSubtitleFont()
        {
            if (UseTheme && Theme != null)
            {
                return GetThemeFont(t => t.Body, new Font(Font.FontFamily, Font.Size - 1));
            }
            return new Font(Font.FontFamily, Font.Size - 1);
        }

        private Color GetDefaultTitleColor()
        {
            if (UseTheme && Theme != null)
            {
                return GetThemeColor(c => c.TextPrimary, Color.Black);
            }
            return Color.Black;
        }

        private Color GetDefaultSubtitleColor()
        {
            if (UseTheme && Theme != null)
            {
                return GetThemeColor(c => c.TextSecondary, Color.FromArgb(96, 96, 96));
            }
            return Color.FromArgb(96, 96, 96);
        }

        /// <summary>
        /// 操作按钮集合
        /// </summary>
        [Category("Data")]
        [Description("卡片的操作按钮集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(CardActionCollectionEditor), typeof(UITypeEditor))]
        public CardActionCollection Actions => actions;

        /// <summary>
        /// 操作按钮高度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(32)]
        [Description("操作按钮的高度")]
        public int ActionButtonHeight
        {
            get => actionButtonHeight;
            set
            {
                if (actionButtonHeight != value && value > 0)
                {
                    actionButtonHeight = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 操作按钮最小宽度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(80)]
        [Description("操作按钮的最小宽度")]
        public int ActionButtonMinWidth
        {
            get => actionButtonMinWidth;
            set
            {
                if (actionButtonMinWidth != value && value > 0)
                {
                    actionButtonMinWidth = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 操作按钮间距
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(8)]
        [Description("操作按钮之间的间距")]
        public int ActionSpacing
        {
            get => actionSpacing;
            set
            {
                if (actionSpacing != value && value >= 0)
                {
                    actionSpacing = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示边框
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否显示卡片边框")]
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

        /// <summary>
        /// 边框宽度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(1)]
        [Description("卡片边框的宽度")]
        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth != value && value >= 0)
                {
                    borderWidth = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        [Category("Appearance")]
        [Description("卡片边框的颜色")]
        public Color BorderColor
        {
            get => borderColor.IsEmpty ? GetDefaultBorderColor() : borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(4)]
        [Description("卡片的圆角半径")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                if (cornerRadius != value && value >= 0)
                {
                    cornerRadius = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示阴影
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否显示卡片阴影")]
        public bool ShowShadow
        {
            get => showShadow;
            set
            {
                if (showShadow != value)
                {
                    showShadow = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 阴影级别
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(2)]
        [Description("卡片阴影的级别(0-24)")]
        public int ShadowLevel
        {
            get => shadowLevel;
            set
            {
                if (shadowLevel != value && value >= 0 && value <= 24)
                {
                    shadowLevel = value;
                    Invalidate();
                }
            }
        }

        private Color GetDefaultBorderColor()
        {
            if (UseTheme && Theme != null)
            {
                return GetThemeColor(c => c.Border, Color.FromArgb(229, 229, 229));
            }
            return Color.FromArgb(229, 229, 229);
        }

        #endregion

        #region 事件

        /// <summary>
        /// 操作按钮点击事件
        /// </summary>
        [Category("Action")]
        [Description("操作按钮被点击时触发")]
        public event EventHandler<CardActionEventArgs> ActionClick;

        protected virtual void OnActionClick(CardActionEventArgs e)
        {
            ActionClick?.Invoke(this, e);
        }

        internal void OnActionsChanged()
        {
            // 延迟执行, 避免在枚举时修改
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    RecreateActionControls();
                    PerformLayout();
                    Invalidate();
                }));
            }
            else
            {
                RecreateActionControls();
                PerformLayout();
                Invalidate();
            }
        }

        #endregion


        #region 布局计算

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            if (DesignMode || IsHandleCreated)
            {
                PerformCardLayout();
            }
        }

        private void PerformCardLayout()
        {
            if (actions.Count == 0)
            {
                // 没有按钮时也要确保控件被移除
                foreach (var control in actionControls.Values.ToArray())
                {
                    control.Visible = false;
                }
                return;
            }

            // 确保操作控件已创建
            RecreateActionControls();

            // 根据布局模式进行布局
            switch (layout)
            {
                case CardLayout.VerticalFlow:
                    LayoutVerticalFlow();
                    break;
                case CardLayout.HorizontalFlow:
                    LayoutHorizontalFlow();
                    break;
                case CardLayout.LeftImageRightContent:
                    LayoutLeftImageRightContent();
                    break;
                case CardLayout.RightImageLeftContent:
                    LayoutRightImageLeftContent();
                    break;
                case CardLayout.TopImageBottomSplit:
                    LayoutTopImageBottomSplit();
                    break;
                case CardLayout.BottomImageTopSplit:
                    LayoutBottomImageTopSplit();
                    break;
            }
        }

        private List<Control> GetVisibleActionControls()
        {
            var visibleControls = new List<Control>();
            foreach (var kvp in actionControls)
            {
                if (kvp.Key.Visible && kvp.Value != null)
                {
                    visibleControls.Add(kvp.Value);
                }
            }
            return visibleControls;
        }

        /// <summary>
        /// 布局操作按钮
        /// </summary>
        private int LayoutActionButtons(List<Control> controls, Rectangle bounds, bool horizontal)
        {
            if (controls.Count == 0)
            {
                return 0;
            }

            int currentX = bounds.X;
            int currentY = bounds.Y;
            int rowHeight = actionButtonHeight;
            int maxX = bounds.X;
            int maxY = bounds.Y;

            foreach (var control in controls)
            {
                control.Height = actionButtonHeight;

                // 检查是否需要换行
                if (horizontal && currentX > bounds.X && currentX + control.Width > bounds.Right)
                {
                    // 换行
                    currentX = bounds.X;
                    currentY += rowHeight + actionSpacing;
                }

                control.Location = new Point(currentX, currentY);
                control.Visible = true;

                if (horizontal)
                {
                    currentX += control.Width + actionSpacing;
                    maxX = Math.Max(maxX, control.Right);
                    maxY = Math.Max(maxY, control.Bottom);
                }
                else
                {
                    currentY += control.Height + actionSpacing;
                    maxX = Math.Max(maxX, control.Right);
                    maxY = Math.Max(maxY, currentY);
                }
            }

            // 返回实际占用的尺寸
            return horizontal ? (maxY - bounds.Y + rowHeight) : (maxX - bounds.X);
        }

        private Rectangle GetContentBounds()
        {
            int left = contentPadding.Left;
            int top = contentPadding.Top;
            int width = Width - contentPadding.Horizontal;
            int height = Height - contentPadding.Vertical;

            // 确保宽高不为负
            width = Math.Max(0, width);
            height = Math.Max(0, height);

            return new Rectangle(left, top, width, height);
        }

        private Size GetImageDisplaySize()
        {
            if (cardImage == null)
            {
                return Size.Empty;
            }

            switch (imageSizeMode)
            {
                case CardImageSizeMode.Fixed:
                    return imageSize;

                case CardImageSizeMode.Auto:
                    return cardImage.Size;

                case CardImageSizeMode.Stretch:
                case CardImageSizeMode.Zoom:
                    // 将在绘制时计算
                    return imageSize;

                default:
                    return cardImage.Size;
            }
        }

        #region 各种布局模式

        /// <summary>
        /// 上下流式布局
        /// </summary>
        private void LayoutVerticalFlow()
        {
            var bounds = GetContentBounds();
            int currentY = bounds.Y;

            // 1. 图片
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                currentY += imgSize.Height + elementSpacing;
            }

            // 2. 标题
            if (!string.IsNullOrEmpty(title))
            {
                using (var g = CreateGraphics())
                {
                    var titleSize = g.MeasureString(title, TitleFont, bounds.Width);
                    currentY += (int)titleSize.Height + elementSpacing;
                }
            }

            // 3. 副标题
            if (!string.IsNullOrEmpty(subtitle))
            {
                using (var g = CreateGraphics())
                {
                    var subtitleSize = g.MeasureString(subtitle, SubtitleFont, bounds.Width);
                    currentY += (int)subtitleSize.Height + elementSpacing;
                }
            }

            // 4. 操作按钮(横向排列, 支持换行)
            var visibleControls = GetVisibleActionControls();
            if (visibleControls.Count > 0)
            {
                var buttonBounds = new Rectangle(bounds.X, currentY, bounds.Width, bounds.Bottom - currentY);
                int buttonHeight = LayoutActionButtons(visibleControls, buttonBounds, true);

                // 检查是否需要调整控件高度
                int requiredHeight = currentY + buttonHeight + contentPadding.Bottom;
                if (requiredHeight > Height)
                {
                    Height = requiredHeight;
                }
            }
        }

        /// <summary>
        /// 左右流式布局
        /// </summary>
        private void LayoutHorizontalFlow()
        {
            var bounds = GetContentBounds();
            int currentX = bounds.X;

            // 1. 图片
            int imageWidth = 0;
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                imageWidth = imgSize.Width;
                currentX += imageWidth + elementSpacing;
            }

            // 计算文本和按钮区域
            int contentWidth = bounds.Width - imageWidth - (imageWidth > 0 ? elementSpacing : 0);
            int currentY = bounds.Y;

            // 2. 标题
            if (!string.IsNullOrEmpty(title))
            {
                using (var g = CreateGraphics())
                {
                    var titleSize = g.MeasureString(title, TitleFont, contentWidth);
                    currentY += (int)titleSize.Height + elementSpacing / 2;
                }
            }

            // 3. 副标题
            if (!string.IsNullOrEmpty(subtitle))
            {
                using (var g = CreateGraphics())
                {
                    var subtitleSize = g.MeasureString(subtitle, SubtitleFont, contentWidth);
                    currentY += (int)subtitleSize.Height + elementSpacing;
                }
            }

            // 4. 操作按钮(横向排列, 支持换行)
            var visibleControls = GetVisibleActionControls();
            if (visibleControls.Count > 0)
            {
                var buttonBounds = new Rectangle(currentX, currentY, contentWidth, bounds.Bottom - currentY);
                int buttonHeight = LayoutActionButtons(visibleControls, buttonBounds, true);

                // 检查是否需要调整控件高度
                int requiredHeight = currentY + buttonHeight + contentPadding.Bottom;
                if (requiredHeight > Height)
                {
                    Height = requiredHeight;
                }
            }
        }

        /// <summary>
        /// 左图右内容
        /// </summary>
        private void LayoutLeftImageRightContent()
        {
            var bounds = GetContentBounds();
            int leftWidth = 0;

            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                leftWidth = imgSize.Width + elementSpacing;
            }

            int contentLeft = bounds.X + leftWidth;
            int contentWidth = bounds.Width - leftWidth;
            int currentY = bounds.Y;

            // 计算标题和副标题占用的高度
            if (!string.IsNullOrEmpty(title))
            {
                using (var g = CreateGraphics())
                {
                    var titleSize = g.MeasureString(title, TitleFont, contentWidth);
                    currentY += (int)titleSize.Height + elementSpacing / 2;
                }
            }

            if (!string.IsNullOrEmpty(subtitle))
            {
                using (var g = CreateGraphics())
                {
                    var subtitleSize = g.MeasureString(subtitle, SubtitleFont, contentWidth);
                    currentY += (int)subtitleSize.Height + elementSpacing;
                }
            }

            // 布局操作按钮(横向，支持换行)
            var visibleControls = GetVisibleActionControls();
            if (visibleControls.Count > 0)
            {
                var buttonBounds = new Rectangle(contentLeft, currentY, contentWidth, bounds.Bottom - currentY);
                int buttonHeight = LayoutActionButtons(visibleControls, buttonBounds, true);

                // 检查是否需要调整控件高度
                int requiredHeight = currentY + buttonHeight + contentPadding.Bottom;
                if (requiredHeight > Height)
                {
                    Height = requiredHeight;
                }
            }
        }

        /// <summary>
        /// 右图左内容
        /// </summary>
        private void LayoutRightImageLeftContent()
        {
            var bounds = GetContentBounds();
            int rightWidth = 0;

            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                rightWidth = imgSize.Width + elementSpacing;
            }

            int contentWidth = bounds.Width - rightWidth;
            int currentY = bounds.Y;

            // 计算标题和副标题占用的高度
            if (!string.IsNullOrEmpty(title))
            {
                using (var g = CreateGraphics())
                {
                    var titleSize = g.MeasureString(title, TitleFont, contentWidth);
                    currentY += (int)titleSize.Height + elementSpacing / 2;
                }
            }

            if (!string.IsNullOrEmpty(subtitle))
            {
                using (var g = CreateGraphics())
                {
                    var subtitleSize = g.MeasureString(subtitle, SubtitleFont, contentWidth);
                    currentY += (int)subtitleSize.Height + elementSpacing;
                }
            }

            // 布局操作按钮
            var visibleControls = GetVisibleActionControls();
            if (visibleControls.Count > 0)
            {
                var buttonBounds = new Rectangle(bounds.X, currentY, contentWidth, bounds.Bottom - currentY);
                int buttonHeight = LayoutActionButtons(visibleControls, buttonBounds, true);

                int requiredHeight = currentY + buttonHeight + contentPadding.Bottom;
                if (requiredHeight > Height)
                {
                    Height = requiredHeight;
                }
            }
        }

        /// <summary>
        /// 上图下分栏
        /// </summary>
        private void LayoutTopImageBottomSplit()
        {
            var bounds = GetContentBounds();
            int topHeight = 0;

            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                topHeight = imgSize.Height + elementSpacing;
            }

            int contentTop = bounds.Y + topHeight;
            int contentHeight = bounds.Height - topHeight;

            // 计算文本所需宽度
            int textWidth = 0;
            int textHeight = 0;

            using (var g = CreateGraphics())
            {
                if (!string.IsNullOrEmpty(title))
                {
                    var titleSize = g.MeasureString(title, TitleFont, bounds.Width / 2);
                    textWidth = Math.Max(textWidth, (int)titleSize.Width);
                    textHeight += (int)titleSize.Height + elementSpacing / 2;
                }

                if (!string.IsNullOrEmpty(subtitle))
                {
                    var subtitleSize = g.MeasureString(subtitle, SubtitleFont, bounds.Width / 2);
                    textWidth = Math.Max(textWidth, (int)subtitleSize.Width);
                    textHeight += (int)subtitleSize.Height;
                }
            }

            // 计算按钮可用宽度
            int buttonAreaWidth = bounds.Width - textWidth - elementSpacing;

            // 确保按钮区域至少有最小宽度
            if (buttonAreaWidth < actionButtonMinWidth)
            {
                // 如果宽度不够，让文本区域收缩
                textWidth = bounds.Width - actionButtonMinWidth - elementSpacing;
                buttonAreaWidth = actionButtonMinWidth;
            }

            // 布局按钮(右侧，横向排列，支持换行)
            var visibleControls = GetVisibleActionControls();
            if (visibleControls.Count > 0)
            {
                int buttonX = bounds.Right - buttonAreaWidth;

                // 如果按钮总宽度超过可用宽度，需要换行
                int totalButtonWidth = 0;
                foreach (var control in visibleControls)
                {
                    totalButtonWidth += control.Width + actionSpacing;
                }

                if (totalButtonWidth > buttonAreaWidth)
                {
                    // 需要换行，使用全宽
                    buttonX = bounds.X + textWidth + elementSpacing;
                    buttonAreaWidth = bounds.Width - textWidth - elementSpacing;
                }

                var buttonBounds = new Rectangle(buttonX, contentTop, buttonAreaWidth, contentHeight);
                int buttonHeight = LayoutActionButtons(visibleControls, buttonBounds, true);

                // 检查是否需要调整控件高度
                int requiredHeight = contentTop + Math.Max(textHeight, buttonHeight) + contentPadding.Bottom;
                if (requiredHeight > Height)
                {
                    Height = requiredHeight;
                }
            }
        }

        /// <summary>
        /// 下图上分栏
        /// </summary>
        private void LayoutBottomImageTopSplit()
        {
            var bounds = GetContentBounds();
            int bottomHeight = 0;

            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                bottomHeight = imgSize.Height + elementSpacing;
            }

            int contentHeight = bounds.Height - bottomHeight;

            // 计算文本所需宽度
            int textWidth = 0;
            int textHeight = 0;

            using (var g = CreateGraphics())
            {
                if (!string.IsNullOrEmpty(title))
                {
                    var titleSize = g.MeasureString(title, TitleFont, bounds.Width / 2);
                    textWidth = Math.Max(textWidth, (int)titleSize.Width);
                    textHeight += (int)titleSize.Height + elementSpacing / 2;
                }

                if (!string.IsNullOrEmpty(subtitle))
                {
                    var subtitleSize = g.MeasureString(subtitle, SubtitleFont, bounds.Width / 2);
                    textWidth = Math.Max(textWidth, (int)subtitleSize.Width);
                    textHeight += (int)subtitleSize.Height;
                }
            }

            // 计算按钮可用宽度
            int buttonAreaWidth = bounds.Width - textWidth - elementSpacing;

            if (buttonAreaWidth < actionButtonMinWidth)
            {
                textWidth = bounds.Width - actionButtonMinWidth - elementSpacing;
                buttonAreaWidth = actionButtonMinWidth;
            }

            // 布局按钮
            var visibleControls = GetVisibleActionControls();
            if (visibleControls.Count > 0)
            {
                int buttonX = bounds.Right - buttonAreaWidth;

                // 检查是否需要换行
                int totalButtonWidth = 0;
                foreach (var control in visibleControls)
                {
                    totalButtonWidth += control.Width + actionSpacing;
                }

                if (totalButtonWidth > buttonAreaWidth)
                {
                    buttonX = bounds.X + textWidth + elementSpacing;
                    buttonAreaWidth = bounds.Width - textWidth - elementSpacing;
                }

                var buttonBounds = new Rectangle(buttonX, bounds.Y, buttonAreaWidth, contentHeight);
                int buttonHeight = LayoutActionButtons(visibleControls, buttonBounds, true);

                // 检查是否需要调整控件高度
                int requiredHeight = Math.Max(textHeight, buttonHeight) + bottomHeight + contentPadding.Vertical;
                if (requiredHeight > Height)
                {
                    Height = requiredHeight;
                }
            }
        }

        #endregion

        #endregion

        #region 操作控件管理

        private void RecreateActionControls()
        {
            if (actions == null)
            {
                return;
            }

            // 暂停布局
            SuspendLayout();

            try
            {
                // 清理旧控件
                var oldControls = actionControls.Values.ToArray(); // 使用副本避免枚举问题
                foreach (var control in oldControls)
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
                actionControls.Clear();

                // 创建新控件(使用副本)
                var actionsCopy = actions.GetItemsCopy();
                foreach (var action in actionsCopy)
                {
                    if (action != null)
                    {
                        var control = CreateActionControl(action);
                        if (control != null)
                        {
                            actionControls[action] = control;
                            Controls.Add(control);
                        }
                    }
                }
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private Control CreateActionControl(CardActionItem action)
        {
            Control control;

            if (action.ActionType == CardActionType.Button)
            {
                var button = new FluentButton
                {
                    Text = action.Text,
                    ButtonStyle = action.ButtonStyle,
                    Height = actionButtonHeight,
                    MinimumSize = new Size(actionButtonMinWidth, actionButtonHeight),
                    AutoSize = true,
                    Enabled = action.Enabled
                };

                // 继承主题
                if (UseTheme)
                {
                    button.InheritThemeFrom(this);
                }

                button.Click += (s, e) =>
                {
                    action.OnClick(e);
                    OnActionClick(new CardActionEventArgs(action));
                };

                control = button;
            }
            else // Link
            {
                var label = new FluentLabel
                {
                    Text = action.Text,
                    AutoSize = true,
                    Cursor = Cursors.Hand,
                    Enabled = action.Enabled,
                    TextDecoration = TextDecoration.Underline
                };

                // 继承主题
                if (UseTheme)
                {
                    label.InheritThemeFrom(this);
                }

                // 设置链接颜色
                if (UseTheme && Theme != null)
                {
                    label.ForeColor = GetThemeColor(c => c.Primary, Color.Blue);
                }
                else
                {
                    label.ForeColor = Color.Blue;
                }

                label.Click += (s, e) =>
                {
                    action.OnClick(e);
                    OnActionClick(new CardActionEventArgs(action));
                };

                control = label;
            }

            return control;
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 绘制阴影
            if (showShadow && shadowLevel > 0)
            {
                DrawCardShadow(g);
            }

            // 绘制卡片背景 - 填充整个卡片区域
            var cardRect = new Rectangle(0, 0, Width, Height);

            using (var path = GetRoundedRectangle(cardRect, cornerRadius))
            {
                var bgColor = UseTheme && Theme != null
                    ? GetThemeColor(c => c.Surface, BackColor)
                    : BackColor;

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = GetContentBounds();

            // 根据布局绘制内容
            switch (layout)
            {
                case CardLayout.VerticalFlow:
                    DrawVerticalFlow(g, bounds);
                    break;
                case CardLayout.HorizontalFlow:
                    DrawHorizontalFlow(g, bounds);
                    break;
                case CardLayout.LeftImageRightContent:
                    DrawLeftImageRightContent(g, bounds);
                    break;
                case CardLayout.RightImageLeftContent:
                    DrawRightImageLeftContent(g, bounds);
                    break;
                case CardLayout.TopImageBottomSplit:
                    DrawTopImageBottomSplit(g, bounds);
                    break;
                case CardLayout.BottomImageTopSplit:
                    DrawBottomImageTopSplit(g, bounds);
                    break;
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            if (!showBorder || borderWidth <= 0)
            {
                return;
            }

            // 边框绘制在卡片边缘
            var borderRect = new Rectangle(
                borderWidth / 2,
                borderWidth / 2,
                Width - borderWidth,
                Height - borderWidth);

            using (var path = GetRoundedRectangle(borderRect, cornerRadius))
            using (var pen = new Pen(BorderColor, borderWidth))
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawCardShadow(Graphics g)
        {
            if (UseTheme && Theme?.Elevation != null)
            {
                var shadow = Theme.Elevation.GetShadow(shadowLevel);
                if (shadow != null)
                {
                    DrawShadowEffect(g, shadow);
                    return;
                }
            }

            // 默认阴影
            int offset = shadowLevel * 2;
            int blur = shadowLevel * 4;
            var shadowColor = Color.FromArgb(Math.Min(40 + shadowLevel * 5, 80), 0, 0, 0);

            using (var shadowBrush = new SolidBrush(shadowColor))
            {
                // 阴影绘制在卡片外部, 不受 Padding 影响
                var shadowRect = new Rectangle(offset, offset, Width - offset, Height - offset);
                using (var path = GetRoundedRectangle(shadowRect, cornerRadius))
                {
                    g.FillPath(shadowBrush, path);
                }
            }
        }

        private void DrawShadowEffect(Graphics g, Shadow shadow)
        {
            var shadowColor = Color.FromArgb((int)(255 * shadow.Opacity), shadow.Color);
            using (var shadowBrush = new SolidBrush(shadowColor))
            {
                var shadowRect = new Rectangle(
                    shadow.OffsetX,
                    shadow.OffsetY,
                    Width,
                    Height);

                using (var path = GetRoundedRectangle(shadowRect, cornerRadius))
                {
                    g.FillPath(shadowBrush, path);
                }
            }
        }

        #region 各布局的绘制方法

        private void DrawVerticalFlow(Graphics g, Rectangle bounds)
        {
            int currentY = bounds.Y;

            // 绘制图片
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                var imgRect = new Rectangle(
                    bounds.X + (bounds.Width - imgSize.Width) / 2,
                    currentY,
                    imgSize.Width,
                    imgSize.Height);

                DrawImage(g, imgRect);
                currentY += imgSize.Height + elementSpacing;
            }

            // 绘制标题
            if (!string.IsNullOrEmpty(title))
            {
                var titleSize = g.MeasureString(title, TitleFont, bounds.Width);
                var titleRect = new RectangleF(bounds.X, currentY, bounds.Width, titleSize.Height);

                using (var brush = new SolidBrush(TitleColor))
                using (var format = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(title, TitleFont, brush, titleRect, format);
                }

                currentY += (int)titleSize.Height + elementSpacing;
            }

            // 绘制副标题
            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleSize = g.MeasureString(subtitle, SubtitleFont, bounds.Width);
                var subtitleRect = new RectangleF(bounds.X, currentY, bounds.Width, subtitleSize.Height);

                using (var brush = new SolidBrush(SubtitleColor))
                using (var format = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(subtitle, SubtitleFont, brush, subtitleRect, format);
                }

                currentY += (int)subtitleSize.Height + elementSpacing;
            }

            // 操作按钮在底部绘制(使用实际控件)
        }

        private void DrawHorizontalFlow(Graphics g, Rectangle bounds)
        {
            int currentX = bounds.X;

            // 绘制图片
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                var imgRect = new Rectangle(currentX, bounds.Y, imgSize.Width, imgSize.Height);
                DrawImage(g, imgRect);
                currentX += imgSize.Width + elementSpacing;
            }

            // 计算文本区域
            int textWidth = bounds.Width - (currentX - bounds.X);
            int currentY = bounds.Y;

            // 绘制标题
            if (!string.IsNullOrEmpty(title))
            {
                var titleSize = g.MeasureString(title, TitleFont, textWidth);
                var titleRect = new RectangleF(currentX, currentY, textWidth, titleSize.Height);

                using (var brush = new SolidBrush(TitleColor))
                {
                    g.DrawString(title, TitleFont, brush, titleRect);
                }

                currentY += (int)titleSize.Height + elementSpacing / 2;
            }

            // 绘制副标题
            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleSize = g.MeasureString(subtitle, SubtitleFont, textWidth);
                var subtitleRect = new RectangleF(currentX, currentY, textWidth, subtitleSize.Height);

                using (var brush = new SolidBrush(SubtitleColor))
                {
                    g.DrawString(subtitle, SubtitleFont, brush, subtitleRect);
                }

                currentY += (int)subtitleSize.Height + elementSpacing;
            }
        }

        private void DrawLeftImageRightContent(Graphics g, Rectangle bounds)
        {
            int currentX = bounds.X;
            int leftWidth = 0;

            // 绘制图片(左侧)
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                var imgRect = new Rectangle(
                    currentX,
                    bounds.Y + (bounds.Height - imgSize.Height) / 2,
                    imgSize.Width,
                    imgSize.Height);

                DrawImage(g, imgRect);
                leftWidth = imgSize.Width + elementSpacing;
                currentX += leftWidth;
            }

            // 右侧内容区域
            int contentWidth = bounds.Width - leftWidth;
            int currentY = bounds.Y;

            // 绘制标题
            if (!string.IsNullOrEmpty(title))
            {
                var titleSize = g.MeasureString(title, TitleFont, contentWidth);
                var titleRect = new RectangleF(currentX, currentY, contentWidth, titleSize.Height);

                using (var brush = new SolidBrush(TitleColor))
                {
                    g.DrawString(title, TitleFont, brush, titleRect);
                }

                currentY += (int)titleSize.Height + elementSpacing / 2;
            }

            // 绘制副标题
            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleSize = g.MeasureString(subtitle, SubtitleFont, contentWidth);
                var subtitleRect = new RectangleF(currentX, currentY, contentWidth, subtitleSize.Height);

                using (var brush = new SolidBrush(SubtitleColor))
                {
                    g.DrawString(subtitle, SubtitleFont, brush, subtitleRect);
                }
            }
        }

        private void DrawRightImageLeftContent(Graphics g, Rectangle bounds)
        {
            int rightWidth = 0;

            // 计算图片位置(右侧)
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                rightWidth = imgSize.Width + elementSpacing;

                var imgRect = new Rectangle(
                    bounds.Right - imgSize.Width,
                    bounds.Y + (bounds.Height - imgSize.Height) / 2,
                    imgSize.Width,
                    imgSize.Height);

                DrawImage(g, imgRect);
            }

            // 左侧内容区域
            int contentWidth = bounds.Width - rightWidth;
            int currentY = bounds.Y;

            // 绘制标题
            if (!string.IsNullOrEmpty(title))
            {
                var titleSize = g.MeasureString(title, TitleFont, contentWidth);
                var titleRect = new RectangleF(bounds.X, currentY, contentWidth, titleSize.Height);

                using (var brush = new SolidBrush(TitleColor))
                {
                    g.DrawString(title, TitleFont, brush, titleRect);
                }

                currentY += (int)titleSize.Height + elementSpacing / 2;
            }

            // 绘制副标题
            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleSize = g.MeasureString(subtitle, SubtitleFont, contentWidth);
                var subtitleRect = new RectangleF(bounds.X, currentY, contentWidth, subtitleSize.Height);

                using (var brush = new SolidBrush(SubtitleColor))
                {
                    g.DrawString(subtitle, SubtitleFont, brush, subtitleRect);
                }
            }
        }

        private void DrawTopImageBottomSplit(Graphics g, Rectangle bounds)
        {
            int currentY = bounds.Y;

            // 绘制图片(顶部)
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                var imgRect = new Rectangle(
                    bounds.X + (bounds.Width - imgSize.Width) / 2,
                    currentY,
                    imgSize.Width,
                    imgSize.Height);

                DrawImage(g, imgRect);
                currentY += imgSize.Height + elementSpacing;
            }

            // 下方分为左右两栏
            int textWidth = bounds.Width / 2;

            // 左侧：标题和副标题
            int textY = currentY;

            if (!string.IsNullOrEmpty(title))
            {
                var titleSize = g.MeasureString(title, TitleFont, textWidth);
                var titleRect = new RectangleF(bounds.X, textY, textWidth, titleSize.Height);

                using (var brush = new SolidBrush(TitleColor))
                {
                    g.DrawString(title, TitleFont, brush, titleRect);
                }

                textY += (int)titleSize.Height + elementSpacing / 2;
            }

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleSize = g.MeasureString(subtitle, SubtitleFont, textWidth);
                var subtitleRect = new RectangleF(bounds.X, textY, textWidth, subtitleSize.Height);

                using (var brush = new SolidBrush(SubtitleColor))
                {
                    g.DrawString(subtitle, SubtitleFont, brush, subtitleRect);
                }
            }

            // 右侧：操作按钮(使用实际控件)
        }

        private void DrawBottomImageTopSplit(Graphics g, Rectangle bounds)
        {
            // 上方分为左右两栏
            int textWidth = bounds.Width / 2;
            int textY = bounds.Y;

            // 左侧：标题和副标题
            if (!string.IsNullOrEmpty(title))
            {
                var titleSize = g.MeasureString(title, TitleFont, textWidth);
                var titleRect = new RectangleF(bounds.X, textY, textWidth, titleSize.Height);

                using (var brush = new SolidBrush(TitleColor))
                {
                    g.DrawString(title, TitleFont, brush, titleRect);
                }

                textY += (int)titleSize.Height + elementSpacing / 2;
            }

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleSize = g.MeasureString(subtitle, SubtitleFont, textWidth);
                var subtitleRect = new RectangleF(bounds.X, textY, textWidth, subtitleSize.Height);

                using (var brush = new SolidBrush(SubtitleColor))
                {
                    g.DrawString(subtitle, SubtitleFont, brush, subtitleRect);
                }
            }

            // 下方：图片
            if (cardImage != null)
            {
                var imgSize = GetImageDisplaySize();
                var imgRect = new Rectangle(
                    bounds.X + (bounds.Width - imgSize.Width) / 2,
                    bounds.Bottom - imgSize.Height,
                    imgSize.Width,
                    imgSize.Height);

                DrawImage(g, imgRect);
            }
        }

        private void DrawImage(Graphics g, Rectangle rect)
        {
            if (cardImage == null)
            {
                return;
            }

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            switch (imageSizeMode)
            {
                case CardImageSizeMode.Fixed:
                case CardImageSizeMode.Stretch:
                    g.DrawImage(cardImage, rect);
                    break;

                case CardImageSizeMode.Auto:
                    g.DrawImage(cardImage, rect.Location);
                    break;

                case CardImageSizeMode.Zoom:
                    var destRect = GetZoomedImageRect(cardImage, rect);
                    g.DrawImage(cardImage, destRect);
                    break;
            }
        }

        private Rectangle GetZoomedImageRect(Image image, Rectangle containerRect)
        {
            float imageAspect = (float)image.Width / image.Height;
            float containerAspect = (float)containerRect.Width / containerRect.Height;

            int width, height;

            if (imageAspect > containerAspect)
            {
                width = containerRect.Width;
                height = (int)(containerRect.Width / imageAspect);
            }
            else
            {
                height = containerRect.Height;
                width = (int)(containerRect.Height * imageAspect);
            }

            int x = containerRect.X + (containerRect.Width - width) / 2;
            int y = containerRect.Y + (containerRect.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }

        #endregion

        #endregion

        #region 主题支持

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (!UseTheme || Theme == null)
            {
                return;
            }

            // 应用到操作按钮
            for (int i = 0; i < actionControls.Values.Count; i++)
            {
                var control = actionControls.Values.ElementAt(i);
                if (control is FluentControlBase fluentControl)
                {
                    fluentControl.InheritThemeFrom(this);
                }
            }

            Invalidate();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var control in actionControls.Values)
                {
                    control.Dispose();
                }
                actionControls.Clear();

                cardImage?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 卡片操作

    /// <summary>
    /// 卡片操作项
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CardActionItem
    {
        private string text = "Action";
        private CardActionType actionType = CardActionType.Button;
        private ButtonStyle buttonStyle = ButtonStyle.Secondary;
        private object tag;

        // 事件
        public event EventHandler Click;
        public event EventHandler PropertyChanged;


        /// <summary>
        /// 文本
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("Action")]
        [Description("操作项显示的文本")]
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(CardActionType.Button)]
        [Description("操作项的类型(按钮或链接)")]
        public CardActionType ActionType
        {
            get => actionType;
            set
            {
                actionType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 按钮样式(仅当类型为Button时有效)
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ButtonStyle.Secondary)]
        [Description("按钮样式")]
        public ButtonStyle ButtonStyle
        {
            get => buttonStyle;
            set
            {
                buttonStyle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否启用该操作项")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 是否可见
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否显示该操作项")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 自定义数据
        /// </summary>
        [Browsable(false)]
        public object Tag
        {
            get => tag;
            set => tag = value;
        }


        internal void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }


        private void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return $"{Text} ({ActionType})";
        }

        public CardActionItem Clone()
        {
            return new CardActionItem
            {
                Text = this.Text,
                ActionType = this.ActionType,
                ButtonStyle = this.ButtonStyle,
                Enabled = this.Enabled,
                Visible = this.Visible,
                Tag = this.Tag
            };
        }
    }

    /// <summary>
    /// 卡片操作项集合
    /// </summary>
    public class CardActionCollection : CollectionBase
    {
        private readonly FluentCard owner;
        private bool suppressEvents = false; // 抑制事件触发

        internal CardActionCollection(FluentCard owner)
        {
            this.owner = owner;
        }

        public CardActionItem this[int index]
        {
            get => (CardActionItem)List[index];
            set
            {
                if (List[index] is CardActionItem oldItem)
                {
                    oldItem.PropertyChanged -= Item_PropertyChanged;
                }

                List[index] = value;

                if (value != null)
                {
                    value.PropertyChanged += Item_PropertyChanged;
                }

                if (!suppressEvents)
                {
                    owner?.OnActionsChanged();
                }
            }
        }

        public int Add(CardActionItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.PropertyChanged += Item_PropertyChanged;
            int index = List.Add(item);

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }

            return index;
        }

        public void AddRange(CardActionItem[] items)
        {
            if (items == null)
            {
                return;
            }

            suppressEvents = true;
            try
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        Add(item);
                    }
                }
            }
            finally
            {
                suppressEvents = false;
                owner?.OnActionsChanged();
            }
        }

        public void Insert(int index, CardActionItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.PropertyChanged += Item_PropertyChanged;
            List.Insert(index, item);

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        public void Remove(CardActionItem item)
        {
            if (item != null)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                List.Remove(item);

                if (!suppressEvents)
                {
                    owner?.OnActionsChanged();
                }
            }
        }

        public bool Contains(CardActionItem item)
        {
            return List.Contains(item);
        }

        public int IndexOf(CardActionItem item)
        {
            return List.IndexOf(item);
        }

        public void CopyTo(CardActionItem[] array, int index)
        {
            List.CopyTo(array, index);
        }

        protected override void OnClear()
        {
            // 取消所有事件订阅
            foreach (CardActionItem item in List)
            {
                if (item != null)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }

            base.OnClear();

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        protected override void OnClearComplete()
        {
            base.OnClearComplete();

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);

            if (value is CardActionItem item)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);

            if (value is CardActionItem item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            base.OnSetComplete(index, oldValue, newValue);

            if (oldValue is CardActionItem oldItem)
            {
                oldItem.PropertyChanged -= Item_PropertyChanged;
            }

            if (newValue is CardActionItem newItem)
            {
                newItem.PropertyChanged += Item_PropertyChanged;
            }

            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        private void Item_PropertyChanged(object sender, EventArgs e)
        {
            if (!suppressEvents)
            {
                owner?.OnActionsChanged();
            }
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        internal void BeginUpdate()
        {
            suppressEvents = true;
        }

        /// <summary>
        /// 结束批量更新
        /// </summary>
        internal void EndUpdate()
        {
            suppressEvents = false;
            owner?.OnActionsChanged();
        }

        /// <summary>
        /// 获取所有项的副本(避免枚举时修改)
        /// </summary>
        internal CardActionItem[] GetItemsCopy()
        {
            var items = new CardActionItem[Count];
            for (int i = 0; i < Count; i++)
            {
                items[i] = this[i];
            }
            return items;
        }
    }
    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 卡片布局模式
    /// </summary>
    public enum CardLayout
    {
        VerticalFlow,               // 上下流式布局：图片-标题-副标题-按钮组
        HorizontalFlow,             // 左右流式布局：图片-标题-副标题-按钮组
        LeftImageRightContent,      // 左上下布局：左图片, 右(标题-副标题-按钮组)
        RightImageLeftContent,      // 上下右布局：右图片, 左(标题-副标题-按钮组)
        TopImageBottomSplit,        // 上左右布局：上图片, 下(左:标题-副标题, 右:按钮组)
        BottomImageTopSplit         // 左右下布局：下图片, 上(左:标题-副标题, 右:按钮组)
    }

    /// <summary>
    /// 卡片操作项类型
    /// </summary>
    public enum CardActionType
    {
        Button,     // 按钮
        Link        // 链接
    }

    /// <summary>
    /// 图片大小模式
    /// </summary>
    public enum CardImageSizeMode
    {
        Fixed,      // 固定大小
        Auto,       // 自动调整(保持宽高比)
        Stretch,    // 拉伸填充
        Zoom        // 等比缩放
    }

    // <summary>
    /// 卡片操作事件参数
    /// </summary>
    public class CardActionEventArgs : EventArgs
    {
        public CardActionEventArgs(CardActionItem action)
        {
            Action = action;
        }

        public CardActionItem Action { get; }
    }

    #endregion

    #region 设计时支持

    public class FluentCardDesigner : ControlDesigner
    {
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                var actionLists = new DesignerActionListCollection();
                actionLists.Add(new FluentCardActionList(Component));
                return actionLists;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 移除基类的 Padding, 使用我们自己的
            if (properties.Contains("Padding"))
            {
                properties.Remove("Padding");
            }

            // 添加新的 Padding 属性
            var paddingProp = TypeDescriptor.GetProperties(typeof(FluentCard))["Padding"];
            if (paddingProp != null)
            {
                properties["Padding"] = TypeDescriptor.CreateProperty(
                    typeof(FluentCard),
                    paddingProp,
                    new Attribute[]
                    {
                        new CategoryAttribute("Layout"),
                        new DescriptionAttribute("卡片内容的内边距"),
                        new BrowsableAttribute(true),
                        new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)
                    });
            }
        }
    }

    public class FluentCardActionList : DesignerActionList
    {
        private FluentCard card;
        private DesignerActionUIService designerService;

        public FluentCardActionList(IComponent component) : base(component)
        {
            card = component as FluentCard;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            items.Add(new DesignerActionHeaderItem("内容"));
            items.Add(new DesignerActionPropertyItem("Title", "标题", "内容"));
            items.Add(new DesignerActionPropertyItem("Subtitle", "副标题", "内容"));
            items.Add(new DesignerActionPropertyItem("CardImage", "图片", "内容"));

            items.Add(new DesignerActionHeaderItem("布局"));
            items.Add(new DesignerActionPropertyItem("Layout", "布局模式", "布局"));
            items.Add(new DesignerActionPropertyItem("ImageSizeMode", "图片尺寸模式", "布局"));
            items.Add(new DesignerActionPropertyItem("ElementSpacing", "元素间距", "布局"));

            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowBorder", "显示边框", "外观"));
            items.Add(new DesignerActionPropertyItem("ShowShadow", "显示阴影", "外观"));

            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "EditActions", "编辑操作按钮...", "操作", true));

            return items;
        }

        public string Title
        {
            get => card.Title;
            set => SetProperty("Title", value);
        }

        public string Subtitle
        {
            get => card.Subtitle;
            set => SetProperty("Subtitle", value);
        }

        public Image CardImage
        {
            get => card.CardImage;
            set => SetProperty("CardImage", value);
        }

        public CardLayout Layout
        {
            get => card.Layout;
            set => SetProperty("Layout", value);
        }

        public CardImageSizeMode ImageSizeMode
        {
            get => card.ImageSizeMode;
            set => SetProperty("ImageSizeMode", value);
        }

        public bool ShowBorder
        {
            get => card.ShowBorder;
            set => SetProperty("ShowBorder", value);
        }

        public bool ShowShadow
        {
            get => card.ShowShadow;
            set => SetProperty("ShowShadow", value);
        }

        public int ElementSpacing
        {
            get => card.ElementSpacing;
            set => SetProperty("ElementSpacing", value);
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(card)[propertyName];
            if (property != null)
            {
                property.SetValue(card, value);
                designerService?.Refresh(card);
            }
        }

        public void EditActions()
        {
            var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                return;
            }

            var property = TypeDescriptor.GetProperties(card)["Actions"];
            if (property == null)
            {
                return;
            }

            var editor = property.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
            if (editor == null)
            {
                return;
            }

            var context = new TypeDescriptorContext(card, property, host);

            // 创建事务
            DesignerTransaction transaction = null;
            try
            {
                transaction = host.CreateTransaction("编辑操作按钮");

                var result = editor.EditValue(context, context, card.Actions);

                // 提交更改
                if (transaction != null)
                {
                    transaction.Commit();
                }

                // 刷新设计器
                designerService?.Refresh(card);

                // 触发组件改变通知
                context.OnComponentChanged();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                MessageBox.Show($"编辑操作失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 卡片操作集合编辑器
    /// </summary>
    internal class CardActionCollectionEditor : CollectionEditor
    {
        public CardActionCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(CardActionItem);
        }

        protected override string GetDisplayText(object value)
        {
            if (value is CardActionItem item)
            {
                return item.ToString();
            }
            return base.GetDisplayText(value);
        }

        protected override object CreateInstance(Type itemType)
        {
            var newItem = new CardActionItem
            {
                Text = "新建操作"
            };
            return newItem;
        }

        protected override object SetItems(object editValue, object[] value)
        {
            if (editValue is CardActionCollection collection)
            {
                // 批量更新, 避免频繁触发事件
                collection.BeginUpdate();
                try
                {
                    collection.Clear();

                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is CardActionItem actionItem)
                            {
                                collection.Add(actionItem);
                            }
                        }
                    }

                    return collection;
                }
                finally
                {
                    collection.EndUpdate();
                }
            }

            return base.SetItems(editValue, value);
        }

        protected override string HelpTopic
        {
            get
            {
                return "添加、删除或编辑卡片的操作按钮。\n\n" +
                        "操作类型：\n" +
                        "- Button: 显示为按钮样式\n" +
                        "- Link: 显示为链接文本\n\n" +
                        "按钮样式(仅Button类型有效)：\n" +
                        "- Primary: 主要按钮\n" +
                        "- Secondary: 次要按钮\n" +
                        "- Danger: 危险操作按钮\n" +
                        "- Success: 成功/确认按钮";
            }

        }


        //protected override string GetHelpText()
        //{
        //    return "添加、删除或编辑卡片的操作按钮。\n\n" +
        //           "操作类型：\n" +
        //           "- Button: 显示为按钮样式\n" +
        //           "- Link: 显示为链接文本\n\n" +
        //           "按钮样式(仅Button类型有效)：\n" +
        //           "- Primary: 主要按钮\n" +
        //           "- Secondary: 次要按钮\n" +
        //           "- Danger: 危险操作按钮\n" +
        //           "- Success: 成功/确认按钮";
        //}
    }

    #endregion
}
