using System;
using System.Collections;
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

namespace FluentControls.Controls
{
    [DefaultEvent("ImageSelected")]
    [Designer(typeof(FluentImageViewerDesigner))]
    [ToolboxBitmap(typeof(PictureBox))]
    public class FluentImageViewer : FluentControlBase
    {
        #region 字段

        private readonly int DefaultRowHeight = 32;

        private ImageList imageSource;
        private FluentImageList fluentImageSource;
        private List<ImageItem> imageItems = new List<ImageItem>();
        private ImageDisplayMode displayMode = ImageDisplayMode.Single;
        //private ImageSizeMode sizeMode = ImageSizeMode.Zoom;
        private ImageEditMode editMode = ImageEditMode.None;
        private ImageSizeMode singleSizeMode = ImageSizeMode.StretchImage;
        private ImageSizeMode gridSizeMode = ImageSizeMode.StretchImage;
        private ImageSizeMode marqueeSizeMode = ImageSizeMode.StretchImage;

        // 滚动条
        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;
        private int scrollOffsetX = 0;
        private int scrollOffsetY = 0;

        // 跑马灯
        private Timer marqueeTimer;
        private int marqueeOffset = 0;
        private int marqueeSpeed = 2;
        private int marqueeImageSpacing = 20;

        // 选择和交互
        private int selectedIndex = -1;
        private int hoveredIndex = -1;
        private Point lastMousePosition;

        // 布局
        private int gridSpacing = 10;
        private int borderPadding = 10;

        // 编辑相关
        private ImageItem clipboardImage;
        private List<ImageItem> editSlots = new List<ImageItem>(4); // 最多4格

        // 导航箭头(Single模式下)
        private bool showLeftArrow = false;
        private bool showRightArrow = false;
        private bool isLeftArrowHovered = false;
        private bool isRightArrowHovered = false;
        private Rectangle leftArrowBounds;
        private Rectangle rightArrowBounds;
        private int arrowWidth = 60;
        private int arrowOpacity = 128; // 半透明度

        // 图片信息
        private ImageInfoDisplayMode imageInfoDisplay = ImageInfoDisplayMode.NameOnly;
        private ImageInfoPosition imageInfoPosition = ImageInfoPosition.Bottom;
        private Color imageInfoBackColor = Color.FromArgb(180, 0, 0, 0);
        private Color imageInfoForeColor = Color.White;
        private Font imageInfoFont;

        // 右键菜单
        private ContextMenuStrip normalContextMenu;
        private ContextMenuStrip editContextMenu;
        private int rightClickedSlotIndex = -1;

        #endregion

        #region 构造函数

        public FluentImageViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            InitializeScrollBars();
            InitializeMarqueeTimer();
            InitializeContextMenus();

            Size = new Size(400, 300);
        }

        private void InitializeScrollBars()
        {
            // 垂直滚动条
            vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            vScrollBar.Scroll += VScrollBar_Scroll;

            // 水平滚动条
            hScrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                Visible = false
            };
            hScrollBar.Scroll += HScrollBar_Scroll;

            Controls.Add(vScrollBar);
            Controls.Add(hScrollBar);
        }

        private void InitializeMarqueeTimer()
        {
            marqueeTimer = new Timer
            {
                Interval = 30 // ~33 FPS
            };
            marqueeTimer.Tick += MarqueeTimer_Tick;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 图片源
        /// </summary>
        [Category("Data")]
        [DefaultValue(null)]
        [Description("Fluent图片列表源")]
        [TypeConverter(typeof(ReferenceConverter))]
        [RefreshProperties(RefreshProperties.Repaint)]
        public FluentImageList FluentImageSource
        {
            get => fluentImageSource;
            set
            {
                if (fluentImageSource != value)
                {
                    // 取消旧的事件订阅
                    if (fluentImageSource != null)
                    {
                        fluentImageSource.ImagesChanged -= FluentImageSource_ImagesChanged;
                    }

                    fluentImageSource = value;

                    // 订阅新的事件
                    if (fluentImageSource != null)
                    {
                        fluentImageSource.ImagesChanged += FluentImageSource_ImagesChanged;
                    }

                    LoadImagesFromFluentSource();
                    OnImageSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 图片源
        /// </summary>
        [Category("Data")]
        [DefaultValue(null)]
        [Description("图片列表源")]
        [TypeConverter(typeof(ReferenceConverter))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Obsolete("建议使用 FluentImageSource 属性代替")]
        [Browsable(false)]
        public ImageList ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    LoadImagesFromSource();
                    OnImageSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 显示模式
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageDisplayMode.Single)]
        [Description("图片显示模式")]
        [RefreshProperties(RefreshProperties.Repaint)] // 改变时刷新
        public ImageDisplayMode DisplayMode
        {
            get => displayMode;
            set
            {
                if (displayMode != value)
                {
                    displayMode = value;

                    // 停止跑马灯
                    if (displayMode == ImageDisplayMode.Marquee)
                    {
                        marqueeTimer.Start();
                    }
                    else
                    {
                        marqueeTimer.Stop();
                        marqueeOffset = 0;
                    }

                    // 重置滚动
                    scrollOffsetX = 0;
                    scrollOffsetY = 0;
                    if (vScrollBar != null)
                    {
                        vScrollBar.Value = 0;
                    }

                    if (hScrollBar != null)
                    {
                        hScrollBar.Value = 0;
                    }

                    UpdateLayout();

                    // 完全重绘
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 单图模式的大小模式
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageSizeMode.Zoom)]
        [Description("单图显示模式下的图片大小模式")]
        public ImageSizeMode SingleSizeMode
        {
            get => singleSizeMode;
            set
            {
                if (singleSizeMode != value)
                {
                    singleSizeMode = value;
                    if (displayMode == ImageDisplayMode.Single)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 多格模式的大小模式
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageSizeMode.Zoom)]
        [Description("多格显示模式下的图片大小模式")]
        public ImageSizeMode GridSizeMode
        {
            get => gridSizeMode;
            set
            {
                if (gridSizeMode != value)
                {
                    gridSizeMode = value;
                    if (displayMode != ImageDisplayMode.Single &&
                        displayMode != ImageDisplayMode.Marquee)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 跑马灯模式的大小模式
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageSizeMode.Zoom)]
        [Description("跑马灯显示模式下的图片大小模式")]
        public ImageSizeMode MarqueeSizeMode
        {
            get => marqueeSizeMode;
            set
            {
                if (marqueeSizeMode != value)
                {
                    marqueeSizeMode = value;
                    if (displayMode == ImageDisplayMode.Marquee)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 保留原有的 SizeMode 属性作为通用设置
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageSizeMode.Zoom)]
        [Description("图片大小显示模式（会同时设置所有模式）")]
        [Browsable(false)] // 隐藏，使用独立的模式设置
        public ImageSizeMode SizeMode
        {
            get => GetCurrentSizeMode();
            set
            {
                singleSizeMode = value;
                gridSizeMode = value;
                marqueeSizeMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 图片大小模式
        /// </summary>
        //[Category("Appearance")]
        //[DefaultValue(ImageSizeMode.Zoom)]
        //[Description("图片大小显示模式")]
        //[RefreshProperties(RefreshProperties.Repaint)]
        //public ImageSizeMode SizeMode
        //{
        //    get => sizeMode;
        //    set
        //    {
        //        if (sizeMode != value)
        //        {
        //            sizeMode = value;
        //            Invalidate();
        //        }
        //    }
        //}

        /// <summary>
        /// 编辑模式
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(ImageEditMode.None)]
        [Description("图片编辑模式")]
        public ImageEditMode EditMode
        {
            get => editMode;
            set
            {
                if (editMode != value)
                {
                    editMode = value;

                    if (value != ImageEditMode.None)
                    {
                        EnterEditMode();
                    }
                    else
                    {
                        ExitEditMode();
                    }

                    UpdateContextMenu(); // 根据模式设置菜单
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 网格间距
        /// </summary>
        [Category("Layout")]
        [DefaultValue(10)]
        [Description("多格显示时的间距")]
        public int GridSpacing
        {
            get => gridSpacing;
            set
            {
                if (gridSpacing != value && value >= 0)
                {
                    gridSpacing = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 跑马灯速度
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(2)]
        [Description("跑马灯滚动速度（像素/帧）")]
        public int MarqueeSpeed
        {
            get => marqueeSpeed;
            set
            {
                if (marqueeSpeed != value && value > 0)
                {
                    marqueeSpeed = value;
                }
            }
        }

        /// <summary>
        /// 跑马灯图片间距
        /// </summary>
        [Category("Layout")]
        [DefaultValue(20)]
        [Description("跑马灯模式下图片间距")]
        public int MarqueeImageSpacing
        {
            get => marqueeImageSpacing;
            set
            {
                if (marqueeImageSpacing != value && value >= 0)
                {
                    marqueeImageSpacing = value;
                }
            }
        }

        /// <summary>
        /// 是否启用箭头导航
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("在单图模式下是否显示左右箭头导航")]
        public bool EnableArrowNavigation { get; set; } = true;

        /// <summary>
        /// 箭头宽度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(60)]
        [Description("箭头导航区域的宽度")]
        public int ArrowWidth
        {
            get => arrowWidth;
            set
            {
                if (arrowWidth != value && value > 20)
                {
                    arrowWidth = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片信息显示模式
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageInfoDisplayMode.NameOnly)]
        [Description("图片信息显示模式")]
        public ImageInfoDisplayMode ImageInfoDisplay
        {
            get => imageInfoDisplay;
            set
            {
                if (imageInfoDisplay != value)
                {
                    imageInfoDisplay = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片信息显示位置
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ImageInfoPosition.Bottom)]
        [Description("图片信息显示位置")]
        public ImageInfoPosition ImageInfoPosition
        {
            get => imageInfoPosition;
            set
            {
                if (imageInfoPosition != value)
                {
                    imageInfoPosition = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片信息背景色
        /// </summary>
        [Category("Appearance")]
        [Description("图片信息背景色")]
        public Color ImageInfoBackColor
        {
            get => imageInfoBackColor;
            set
            {
                if (imageInfoBackColor != value)
                {
                    imageInfoBackColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片信息前景色
        /// </summary>
        [Category("Appearance")]
        [Description("图片信息文字颜色")]
        public Color ImageInfoForeColor
        {
            get => imageInfoForeColor;
            set
            {
                if (imageInfoForeColor != value)
                {
                    imageInfoForeColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 图片信息字体
        /// </summary>
        [Category("Appearance")]
        [Description("图片信息字体")]
        public Font ImageInfoFont
        {
            get => imageInfoFont ?? Font;
            set
            {
                if (imageInfoFont != value)
                {
                    imageInfoFont = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否启用右键菜单
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否启用右键菜单")]
        public bool EnableContextMenu { get; set; } = true;

        /// <summary>
        /// 选中的图片索引
        /// </summary>
        [Browsable(false)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                System.Diagnostics.Debug.WriteLine($"SelectedIndex changing: {selectedIndex} -> {value}");

                if (value < 0 || value >= imageItems.Count)
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedIndex out of range: {value}");
                    return;
                }

                if (selectedIndex != value)
                {
                    // 取消之前的选中
                    if (selectedIndex >= 0 && selectedIndex < imageItems.Count)
                    {
                        imageItems[selectedIndex].IsSelected = false;
                    }

                    selectedIndex = value;

                    // 设置新的选中
                    if (selectedIndex >= 0 && selectedIndex < imageItems.Count)
                    {
                        imageItems[selectedIndex].IsSelected = true;
                    }

                    System.Diagnostics.Debug.WriteLine($"SelectedIndex changed to: {selectedIndex}");

                    OnImageSelected(new ImageEventArgs(selectedIndex));

                    // 单图模式需要更新布局
                    if (displayMode == ImageDisplayMode.Single)
                    {
                        UpdateSingleLayout();
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 选中的图片
        /// </summary>
        [Browsable(false)]
        public ImageItem SelectedImage
        {
            get => selectedIndex >= 0 && selectedIndex < imageItems.Count
                ? imageItems[selectedIndex]
                : null;
        }

        /// <summary>
        /// 图片数量
        /// </summary>
        [Browsable(false)]
        public int ImageCount => imageItems.Count;

        #endregion

        #region 事件

        /// <summary>
        /// 图片选中事件
        /// </summary>
        [Category("Action")]
        [Description("图片被选中时触发")]
        public event EventHandler<ImageEventArgs> ImageSelected;

        /// <summary>
        /// 图片双击事件
        /// </summary>
        [Category("Action")]
        [Description("图片被双击时触发")]
        public event EventHandler<ImageEventArgs> ImageDoubleClicked;

        /// <summary>
        /// 图片源改变事件
        /// </summary>
        [Category("Property Changed")]
        [Description("图片源改变时触发")]
        public event EventHandler ImageSourceChanged;

        protected virtual void OnImageSelected(ImageEventArgs e)
        {
            ImageSelected?.Invoke(this, e);
        }

        protected virtual void OnImageDoubleClicked(ImageEventArgs e)
        {
            ImageDoubleClicked?.Invoke(this, e);
        }

        protected virtual void OnImageSourceChanged(EventArgs e)
        {
            ImageSourceChanged?.Invoke(this, e);
        }

        private void FluentImageSource_ImagesChanged(object sender, EventArgs e)
        {
            LoadImagesFromFluentSource();
        }

        #endregion

        #region 数据加载

        private void LoadImagesFromFluentSource()
        {
            imageItems.Clear();
            selectedIndex = -1;
            hoveredIndex = -1;

            if (fluentImageSource == null || fluentImageSource.Images.Count == 0)
            {
                UpdateLayout();
                Invalidate();
                return;
            }

            for (int i = 0; i < fluentImageSource.Images.Count; i++)
            {
                var item = fluentImageSource.Images.GetItem(i);
                imageItems.Add(new ImageItem(
                    item.Image,
                    i,
                    item.Name ?? item.Key,
                    item.Description));
            }

            UpdateLayout();
            Invalidate();
        }

        private void LoadImagesFromSource()
        {
            imageItems.Clear();
            selectedIndex = -1;
            hoveredIndex = -1;

            if (imageSource == null || imageSource.Images.Count == 0)
            {
                UpdateLayout();
                Invalidate();
                return;
            }

            for (int i = 0; i < imageSource.Images.Count; i++)
            {
                var image = imageSource.Images[i];
                var key = imageSource.Images.Keys[i];
                imageItems.Add(new ImageItem(image, i, key));
            }

            UpdateLayout();
            Invalidate();
        }

        #endregion

        #region 布局计算

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (imageItems.Count == 0)
            {
                vScrollBar.Visible = false;
                hScrollBar.Visible = false;
                return;
            }

            if (editMode != ImageEditMode.None)
            {
                UpdateEditLayout();
            }
            else
            {
                switch (displayMode)
                {
                    case ImageDisplayMode.Single:
                        UpdateSingleLayout();
                        break;
                    case ImageDisplayMode.Marquee:
                        UpdateMarqueeLayout();
                        break;
                    default:
                        UpdateGridLayout();
                        break;
                }
            }
        }

        private void UpdateSingleLayout()
        {
            var contentArea = GetContentArea();

            System.Diagnostics.Debug.WriteLine($"UpdateSingleLayout - ContentArea: {contentArea}, Selected: {selectedIndex}, Total: {imageItems.Count}");

            // 确保有选中的图片
            if (selectedIndex < 0 && imageItems.Count > 0)
            {
                selectedIndex = 0;
                imageItems[0].IsSelected = true;
            }

            // 为所有图片设置相同的边界
            for (int i = 0; i < imageItems.Count; i++)
            {
                imageItems[i].Bounds = contentArea;
            }

            vScrollBar.Visible = false;
            hScrollBar.Visible = false;

            System.Diagnostics.Debug.WriteLine($"UpdateSingleLayout completed - Bounds set to: {contentArea}");
        }

        private void UpdateGridLayout()
        {
            var contentArea = GetContentArea();
            int cols, rows;
            GetGridDimensions(out cols, out rows);

            int cellWidth = (contentArea.Width - (cols - 1) * gridSpacing) / cols;
            int cellHeight = (contentArea.Height - (rows - 1) * gridSpacing) / rows;

            // 计算所有图片的位置（不考虑可见性）
            int index = 0;
            for (int r = 0; r < int.MaxValue && index < imageItems.Count; r++)
            {
                for (int c = 0; c < cols && index < imageItems.Count; c++)
                {
                    int x = contentArea.X + c * (cellWidth + gridSpacing);
                    int y = contentArea.Y + r * (cellHeight + gridSpacing);

                    imageItems[index].Bounds = new Rectangle(x, y, cellWidth, cellHeight);
                    index++;
                }
            }

            // 计算总高度
            int totalRows = (int)Math.Ceiling((double)imageItems.Count / cols);
            int totalHeight = totalRows * (cellHeight + gridSpacing) - gridSpacing;

            // 更新滚动条
            bool needVScroll = totalHeight > contentArea.Height;

            if (needVScroll)
            {
                vScrollBar.Minimum = 0;
                vScrollBar.Maximum = totalHeight;
                vScrollBar.LargeChange = contentArea.Height;
                vScrollBar.SmallChange = cellHeight + gridSpacing;

                // 限制滚动值
                if (scrollOffsetY > totalHeight - contentArea.Height)
                {
                    scrollOffsetY = Math.Max(0, totalHeight - contentArea.Height);
                }

                vScrollBar.Value = scrollOffsetY;
                vScrollBar.Visible = true;
            }
            else
            {
                vScrollBar.Visible = false;
                scrollOffsetY = 0;
            }

            hScrollBar.Visible = false;
            scrollOffsetX = 0;
        }

        private void UpdateMarqueeLayout()
        {
            // 跑马灯布局在绘制时动态计算
            vScrollBar.Visible = false;
            hScrollBar.Visible = false;
        }

        private void UpdateEditLayout()
        {
            var contentArea = GetContentArea();
            int slots = GetEditSlotCount();

            if (slots == 1)
            {
                if (editSlots.Count > 0 && editSlots[0] != null)
                {
                    editSlots[0].Bounds = contentArea;
                }
            }
            else if (slots == 2)
            {
                int cellWidth = (contentArea.Width - gridSpacing) / 2;

                for (int i = 0; i < Math.Min(2, editSlots.Count); i++)
                {
                    if (editSlots[i] != null)
                    {
                        int x = contentArea.X + i * (cellWidth + gridSpacing);
                        editSlots[i].Bounds = new Rectangle(x, contentArea.Y, cellWidth, contentArea.Height);
                    }
                }
            }
            else // 4
            {
                int cellWidth = (contentArea.Width - gridSpacing) / 2;
                int cellHeight = (contentArea.Height - gridSpacing) / 2;

                for (int i = 0; i < Math.Min(4, editSlots.Count); i++)
                {
                    if (editSlots[i] != null)
                    {
                        int row = i / 2;
                        int col = i % 2;
                        int x = contentArea.X + col * (cellWidth + gridSpacing);
                        int y = contentArea.Y + row * (cellHeight + gridSpacing);
                        editSlots[i].Bounds = new Rectangle(x, y, cellWidth, cellHeight);
                    }
                }
            }

            vScrollBar.Visible = false;
            hScrollBar.Visible = false;
        }

        private void GetGridDimensions(out int cols, out int rows)
        {
            switch (displayMode)
            {
                case ImageDisplayMode.Grid2:
                    cols = 2; rows = 1;
                    break;
                case ImageDisplayMode.Grid4:
                    cols = 2; rows = 2;
                    break;
                case ImageDisplayMode.Grid6:
                    cols = 3; rows = 2;
                    break;
                case ImageDisplayMode.Grid8:
                    cols = 4; rows = 2;
                    break;
                case ImageDisplayMode.Grid9:
                    cols = 3; rows = 3;
                    break;
                case ImageDisplayMode.Grid16:
                    cols = 4; rows = 4;
                    break;
                default:
                    cols = 1; rows = 1;
                    break;
            }
        }

        private int GetEditSlotCount()
        {
            switch (editMode)
            {
                case ImageEditMode.Single:
                    return 1;
                case ImageEditMode.Grid2:
                    return 2;
                case ImageEditMode.Grid4:
                    return 4;
                default:
                    return 0;
            }
        }

        private Rectangle GetContentArea()
        {
            int width = ClientSize.Width - borderPadding * 2;
            int height = ClientSize.Height - borderPadding * 2;

            if (vScrollBar.Visible)
            {
                width -= vScrollBar.Width;
            }

            if (hScrollBar.Visible)
            {
                height -= hScrollBar.Height;
            }

            return new Rectangle(borderPadding, borderPadding,
                Math.Max(0, width), Math.Max(0, height));
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            // 先清除背景
            e.Graphics.Clear(UseTheme && Theme != null
                ? GetThemeColor(c => c.Background, Color.FromArgb(240, 240, 240))
                : Color.FromArgb(240, 240, 240));

            base.OnPaint(e);
        }

        protected override void DrawBackground(Graphics g)
        {
            var backColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.Background, Color.FromArgb(240, 240, 240))
                : Color.FromArgb(240, 240, 240);

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (imageItems.Count == 0)
            {
                DrawEmptyMessage(g);
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (editMode != ImageEditMode.None)
            {
                DrawEditMode(g);
            }
            else
            {
                switch (displayMode)
                {
                    case ImageDisplayMode.Single:
                        DrawSingleImage(g);
                        break;
                    case ImageDisplayMode.Marquee:
                        DrawMarquee(g);
                        break;
                    default:
                        DrawGrid(g);
                        break;
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.Border, Color.FromArgb(204, 204, 204))
                : Color.FromArgb(204, 204, 204);

            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawEmptyMessage(Graphics g)
        {
            var message = "暂无图片";
            var font = UseTheme && Theme != null
                ? GetThemeFont(t => t.Body, Font)
                : Font;

            var textColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.TextSecondary, Color.Gray)
                : Color.Gray;

            using (var brush = new SolidBrush(textColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(message, font, brush, ClientRectangle, format);
            }
        }

        private void DrawSingleImage(Graphics g)
        {
            System.Diagnostics.Debug.WriteLine($"DrawSingleImage - Selected: {selectedIndex}, Count: {imageItems.Count}");

            // 确保有选中的图片
            if (selectedIndex < 0 && imageItems.Count > 0)
            {
                selectedIndex = 0;
                imageItems[0].IsSelected = true;
            }

            if (selectedIndex < 0 || selectedIndex >= imageItems.Count)
            {
                System.Diagnostics.Debug.WriteLine("DrawSingleImage - No valid selection");
                DrawEmptyMessage(g);
                return;
            }

            var item = imageItems[selectedIndex];
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine("DrawSingleImage - Item is null");
                DrawEmptyMessage(g);
                return;
            }

            if (item.CurrentImage == null)
            {
                System.Diagnostics.Debug.WriteLine("DrawSingleImage - CurrentImage is null");
                DrawEmptyMessage(g);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"DrawSingleImage - Drawing image {selectedIndex}: {item.Name}, Bounds: {item.Bounds}");

            // 使用单图模式的大小模式
            var contentArea = GetContentArea();
            DrawImageItemWithMode(g, item, contentArea, false, singleSizeMode);

            // 绘制箭头导航
            if (EnableArrowNavigation && editMode == ImageEditMode.None && imageItems.Count > 1)
            {
                if (showLeftArrow && selectedIndex > 0)
                {
                    DrawNavigationArrow(g, leftArrowBounds, true, isLeftArrowHovered);
                }

                if (showRightArrow && selectedIndex < imageItems.Count - 1)
                {
                    DrawNavigationArrow(g, rightArrowBounds, false, isRightArrowHovered);
                }
            }
        }

        /// <summary>
        /// 使用指定大小模式绘制图片项
        /// </summary>
        private void DrawImageItemWithMode(Graphics g, ImageItem item, Rectangle bounds, bool isHovered, ImageSizeMode sizeMode)
        {
            if (item.CurrentImage == null)
            {
                DrawPlaceholder(g, bounds);
                return;
            }

            // 绘制背景
            var bgColor = item.IsSelected
                ? (UseTheme && Theme != null ? GetThemeColor(c => c.Primary, Color.FromArgb(0, 120, 215)) : Color.FromArgb(0, 120, 215))
                : (isHovered ? Color.FromArgb(230, 230, 230) : Color.White);

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 计算图片信息区域高度
            int infoHeight = CalculateInfoHeight();

            // 调整图片显示区域
            Rectangle imageBounds = bounds;
            if (imageInfoDisplay != ImageInfoDisplayMode.None)
            {
                if (imageInfoPosition == ImageInfoPosition.Top)
                {
                    imageBounds = new Rectangle(bounds.X, bounds.Y + infoHeight,
                        bounds.Width, bounds.Height - infoHeight);
                }
                else
                {
                    imageBounds = new Rectangle(bounds.X, bounds.Y,
                        bounds.Width, bounds.Height - infoHeight);
                }
            }

            // 绘制图片（使用指定的大小模式）
            var actualImageBounds = CalculateImageBounds(item.CurrentImage, imageBounds);
            DrawImageInBounds(g, item.CurrentImage, actualImageBounds, sizeMode);

            // 绘制图片信息
            if (imageInfoDisplay != ImageInfoDisplayMode.None)
            {
                DrawImageInfo(g, item, bounds, infoHeight);
            }

            // 绘制边框
            var borderColor = item.IsSelected
                ? (UseTheme && Theme != null ? GetThemeColor(c => c.Primary, Color.FromArgb(0, 120, 215)) : Color.FromArgb(0, 120, 215))
                : (UseTheme && Theme != null ? GetThemeColor(c => c.Border, Color.FromArgb(204, 204, 204)) : Color.FromArgb(204, 204, 204));

            using (var pen = new Pen(borderColor, item.IsSelected ? 2 : 1))
            {
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
        }

        /// <summary>
        /// 绘制导航箭头
        /// </summary>
        private void DrawNavigationArrow(Graphics g, Rectangle bounds, bool isLeft, bool isHovered)
        {
            // 设置抗锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 背景色（半透明黑色）
            int bgOpacity = isHovered ? 180 : 100;
            var bgColor = Color.FromArgb(bgOpacity, 0, 0, 0);

            // 绘制半透明背景（使用渐变效果更美观）
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddRectangle(bounds);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    bounds,
                    isLeft ? Color.FromArgb(bgOpacity, 0, 0, 0) : Color.FromArgb(0, 0, 0, 0),
                    isLeft ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(bgOpacity, 0, 0, 0),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    g.FillPath(brush, path);
                }
            }

            // 计算箭头位置和大小
            int arrowSize = 40;
            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;

            // 创建箭头路径
            using (var arrowPath = new System.Drawing.Drawing2D.GraphicsPath())
            {
                if (isLeft)
                {
                    // 左箭头 <
                    Point[] points = new Point[]
                    {
                new Point(centerX + arrowSize / 4, centerY - arrowSize / 2),
                new Point(centerX - arrowSize / 4, centerY),
                new Point(centerX + arrowSize / 4, centerY + arrowSize / 2)
                    };
                    arrowPath.AddLines(points);
                }
                else
                {
                    // 右箭头 >
                    Point[] points = new Point[]
                    {
                        new Point(centerX - arrowSize / 4, centerY - arrowSize / 2),
                        new Point(centerX + arrowSize / 4, centerY),
                        new Point(centerX - arrowSize / 4, centerY + arrowSize / 2)
                    };
                    arrowPath.AddLines(points);
                }

                // 绘制箭头（白色，带阴影效果）
                var arrowColor = Color.White;

                // 先绘制阴影
                using (var shadowPen = new Pen(Color.FromArgb(100, 0, 0, 0), 5))
                {
                    shadowPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    shadowPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    shadowPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                    var shadowMatrix = new System.Drawing.Drawing2D.Matrix();
                    shadowMatrix.Translate(2, 2);
                    arrowPath.Transform(shadowMatrix);
                    g.DrawPath(shadowPen, arrowPath);

                    shadowMatrix.Translate(-2, -2);
                    arrowPath.Transform(shadowMatrix);
                }

                // 绘制箭头主体
                using (var arrowPen = new Pen(arrowColor, 4))
                {
                    arrowPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    arrowPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    arrowPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                    g.DrawPath(arrowPen, arrowPath);
                }
            }

            // 如果悬停，绘制提示文本和圆形背景
            if (isHovered)
            {
                string text = isLeft ? "上一张" : "下一张";

                // 绘制圆形背景
                int circleDiameter = 50;
                var circleRect = new Rectangle(
                    centerX - circleDiameter / 2,
                    centerY - circleDiameter / 2,
                    circleDiameter,
                    circleDiameter);

                using (var circleBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                {
                    g.FillEllipse(circleBrush, circleRect);
                }

                // 重新绘制箭头在圆形上
                using (var arrowPath = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int smallArrowSize = 20;
                    if (isLeft)
                    {
                        Point[] points = new Point[]
                        {
                            new Point(centerX + smallArrowSize / 4, centerY - smallArrowSize / 2),
                            new Point(centerX - smallArrowSize / 4, centerY),
                            new Point(centerX + smallArrowSize / 4, centerY + smallArrowSize / 2)
                        };
                        arrowPath.AddLines(points);
                    }
                    else
                    {
                        Point[] points = new Point[]
                        {
                            new Point(centerX - smallArrowSize / 4, centerY - smallArrowSize / 2),
                            new Point(centerX + smallArrowSize / 4, centerY),
                            new Point(centerX - smallArrowSize / 4, centerY + smallArrowSize / 2)
                        };
                        arrowPath.AddLines(points);
                    }

                    using (var arrowPen = new Pen(Color.White, 3))
                    {
                        arrowPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        arrowPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        arrowPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                        g.DrawPath(arrowPen, arrowPath);
                    }
                }

                // 绘制提示文本
                var font = new Font(Font.FontFamily, 9, FontStyle.Bold);
                using (var brush = new SolidBrush(Color.White))
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    var textBounds = new Rectangle(bounds.X, bounds.Bottom - 40, bounds.Width, 25);
                    g.DrawString(text, font, brush, textBounds, format);
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            var contentArea = GetContentArea();

            var oldTransform = g.Transform;

            try
            {
                var oldClip = g.Clip;
                g.SetClip(contentArea);

                g.TranslateTransform(0, -scrollOffsetY);

                foreach (var item in imageItems)
                {
                    var actualBounds = item.Bounds;
                    actualBounds.Offset(0, -scrollOffsetY);

                    if (actualBounds.Bottom < contentArea.Top - 50 ||
                        actualBounds.Top > contentArea.Bottom + 50)
                    {
                        continue;
                    }

                    bool isHovered = item.Index == hoveredIndex;
                    DrawImageItemWithMode(g, item, item.Bounds, isHovered, gridSizeMode);
                }

                g.Clip = oldClip;
            }
            finally
            {
                g.Transform = oldTransform;
            }
        }

        private void DrawMarquee(Graphics g)
        {
            if (imageItems.Count == 0)
            {
                return;
            }

            var contentArea = GetContentArea();

            int infoHeight = CalculateInfoHeight();
            int imageHeight = contentArea.Height - infoHeight - 20;

            int currentX = contentArea.X - marqueeOffset;

            var oldClip = g.Clip;
            g.SetClip(contentArea);

            foreach (var item in imageItems)
            {
                if (item.CurrentImage == null)
                {
                    continue;
                }

                float aspectRatio = (float)item.CurrentImage.Width / item.CurrentImage.Height;
                int imageWidth = (int)(imageHeight * aspectRatio);

                var totalBounds = new Rectangle(currentX, contentArea.Y + 10, imageWidth, contentArea.Height - 20);
                var imageBounds = totalBounds;

                if (imageInfoDisplay != ImageInfoDisplayMode.None)
                {
                    if (imageInfoPosition == ImageInfoPosition.Top)
                    {
                        imageBounds = new Rectangle(totalBounds.X, totalBounds.Y + infoHeight,
                            totalBounds.Width, totalBounds.Height - infoHeight);
                    }
                    else
                    {
                        imageBounds = new Rectangle(totalBounds.X, totalBounds.Y,
                            totalBounds.Width, totalBounds.Height - infoHeight);
                    }
                }

                if (totalBounds.Right > contentArea.Left && totalBounds.Left < contentArea.Right)
                {
                    DrawImageInBounds(g, item.CurrentImage, imageBounds, marqueeSizeMode);

                    if (imageInfoDisplay != ImageInfoDisplayMode.None)
                    {
                        DrawImageInfo(g, item, totalBounds, infoHeight);
                    }
                }

                currentX += imageWidth + marqueeImageSpacing;
            }

            g.Clip = oldClip;
        }

        private void DrawEditMode(Graphics g)
        {
            for (int i = 0; i < editSlots.Count; i++)
            {
                var slot = editSlots[i];
                if (slot == null)
                {
                    continue;
                }

                bool isHovered = i == hoveredIndex;
                DrawImageItem(g, slot, slot.Bounds, isHovered);

                // 绘制槽位标签
                DrawSlotLabel(g, i, slot.Bounds);
            }
        }

        private void DrawImageItem(Graphics g, ImageItem item, Rectangle bounds, bool isHovered)
        {
            DrawImageItemWithMode(g, item, bounds, isHovered, gridSizeMode);
        }

        /// <summary>
        /// 计算信息区域高度
        /// </summary>
        private int CalculateInfoHeight()
        {
            if (imageInfoDisplay == ImageInfoDisplayMode.None)
            {
                return 0;
            }

            int baseHeight = 25;
            if (imageInfoDisplay == ImageInfoDisplayMode.NameAndDescription)
            {
                return baseHeight * 2;
            }

            return baseHeight;
        }

        /// <summary>
        /// 绘制图片信息
        /// </summary>
        private void DrawImageInfo(Graphics g, ImageItem item, Rectangle bounds, int infoHeight)
        {
            if (imageInfoDisplay == ImageInfoDisplayMode.None || infoHeight == 0)
            {
                return;
            }

            Rectangle infoBounds;
            if (imageInfoPosition == ImageInfoPosition.Top)
            {
                infoBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, infoHeight);
            }
            else
            {
                infoBounds = new Rectangle(bounds.X, bounds.Bottom - infoHeight, bounds.Width, infoHeight);
            }

            // 绘制背景
            using (var brush = new SolidBrush(imageInfoBackColor))
            {
                g.FillRectangle(brush, infoBounds);
            }

            // 绘制文本
            var font = imageInfoFont ?? Font;
            using (var textBrush = new SolidBrush(imageInfoForeColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                string displayText = GetImageInfoText(item);

                if (imageInfoDisplay == ImageInfoDisplayMode.NameAndDescription)
                {
                    // 分两行显示
                    int halfHeight = infoHeight / 2;

                    // 名称
                    var nameRect = new Rectangle(infoBounds.X, infoBounds.Y, infoBounds.Width, halfHeight);
                    g.DrawString(item.Name, font, textBrush, nameRect, format);

                    // 描述
                    var descRect = new Rectangle(infoBounds.X, infoBounds.Y + halfHeight, infoBounds.Width, halfHeight);
                    g.DrawString(item.Description ?? "", font, textBrush, descRect, format);
                }
                else
                {
                    // 单行显示
                    g.DrawString(displayText, font, textBrush, infoBounds, format);
                }
            }
        }

        /// <summary>
        /// 获取显示文本
        /// </summary>
        private string GetImageInfoText(ImageItem item)
        {
            switch (imageInfoDisplay)
            {
                case ImageInfoDisplayMode.NameOnly:
                    return item.Name ?? "";
                case ImageInfoDisplayMode.DescriptionOnly:
                    return item.Description ?? "";
                case ImageInfoDisplayMode.NameAndDescription:
                    return $"{item.Name}\n{item.Description}";
                default:
                    return "";
            }
        }


        private void DrawImageInBounds(Graphics g, Image image, Rectangle bounds, ImageSizeMode mode)
        {
            if (image == null || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            switch (mode)
            {
                case ImageSizeMode.Normal:
                    g.DrawImage(image, bounds.Location);
                    break;

                case ImageSizeMode.StretchImage:
                    g.DrawImage(image, bounds);
                    break;

                case ImageSizeMode.CenterImage:
                    {
                        int x = bounds.X + (bounds.Width - image.Width) / 2;
                        int y = bounds.Y + (bounds.Height - image.Height) / 2;
                        g.DrawImage(image, x, y, image.Width, image.Height);
                    }
                    break;

                case ImageSizeMode.Zoom:
                    {
                        float imageAspect = (float)image.Width / image.Height;
                        float boundsAspect = (float)bounds.Width / bounds.Height;

                        int drawWidth, drawHeight;
                        if (imageAspect > boundsAspect)
                        {
                            drawWidth = bounds.Width;
                            drawHeight = (int)(bounds.Width / imageAspect);
                        }
                        else
                        {
                            drawHeight = bounds.Height;
                            drawWidth = (int)(bounds.Height * imageAspect);
                        }

                        int x = bounds.X + (bounds.Width - drawWidth) / 2;
                        int y = bounds.Y + (bounds.Height - drawHeight) / 2;

                        g.DrawImage(image, x, y, drawWidth, drawHeight);
                    }
                    break;
            }
        }

        private Rectangle CalculateImageBounds(Image image, Rectangle containerBounds)
        {
            int padding = 5;
            return new Rectangle(
                containerBounds.X + padding,
                containerBounds.Y + padding,
                containerBounds.Width - padding * 2,
                containerBounds.Height - padding * 2);
        }

        private void DrawPlaceholder(Graphics g, Rectangle bounds)
        {
            using (var brush = new SolidBrush(Color.FromArgb(245, 245, 245)))
            {
                g.FillRectangle(brush, bounds);
            }

            using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
                g.DrawLine(pen, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
            }
        }

        private void DrawImageName(Graphics g, string name, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var textBounds = new Rectangle(bounds.X, bounds.Bottom - 25, bounds.Width, 20);

            using (var brush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            {
                g.FillRectangle(brush, textBounds);
            }

            using (var textBrush = new SolidBrush(Color.White))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                g.DrawString(name, Font, textBrush, textBounds, format);
            }
        }

        private void DrawSlotLabel(Graphics g, int slotIndex, Rectangle bounds)
        {
            var label = $"槽位 {slotIndex + 1}";
            var labelBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, 25);

            using (var brush = new SolidBrush(Color.FromArgb(200, 100, 100, 100)))
            {
                g.FillRectangle(brush, labelBounds);
            }

            using (var textBrush = new SolidBrush(Color.White))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(label, Font, textBrush, labelBounds, format);
            }
        }

        /// <summary>
        /// 获取当前显示模式对应的大小模式
        /// </summary>
        private ImageSizeMode GetCurrentSizeMode()
        {
            switch (displayMode)
            {
                case ImageDisplayMode.Single:
                    return singleSizeMode;
                case ImageDisplayMode.Marquee:
                    return marqueeSizeMode;
                default:
                    return gridSizeMode;
            }
        }

        #endregion

        #region 编辑功能

        /// <summary>
        /// 进入编辑模式
        /// </summary>
        private void EnterEditMode()
        {
            if (selectedIndex < 0 || selectedIndex >= imageItems.Count)
            {
                MessageBox.Show("请先选择要编辑的图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                editMode = ImageEditMode.None;
                return;
            }

            // 初始化编辑槽位
            int slotCount = GetEditSlotCount();
            editSlots.Clear();

            // 第一个槽位放置选中的图片
            var selectedImage = imageItems[selectedIndex].Clone();
            editSlots.Add(selectedImage);

            // 填充其他空槽位
            for (int i = 1; i < slotCount; i++)
            {
                editSlots.Add(new ImageItem(null, i));
            }

            UpdateLayout();
            Invalidate();
        }

        /// <summary>
        /// 退出编辑模式
        /// </summary>
        private void ExitEditMode()
        {
            editSlots.Clear();
            clipboardImage = null;
            UpdateLayout();
            Invalidate();
        }

        /// <summary>
        /// 剪切当前槽位的图片
        /// </summary>
        public void CutImage(int slotIndex = 0)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            if (slot.CurrentImage == null)
            {
                MessageBox.Show("该槽位没有图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 复制到剪贴板
            clipboardImage = slot.Clone();

            // 清空当前槽位
            slot.OriginalImage = null;
            slot.EditedImage = null;

            Invalidate();
        }

        /// <summary>
        /// 复制当前槽位的图片
        /// </summary>
        public void CopyImage(int slotIndex = 0)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            if (slot.CurrentImage == null)
            {
                MessageBox.Show("该槽位没有图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            clipboardImage = slot.Clone();
        }

        /// <summary>
        /// 粘贴到指定槽位
        /// </summary>
        public void PasteImage(int slotIndex)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            if (clipboardImage == null || clipboardImage.CurrentImage == null)
            {
                MessageBox.Show("剪贴板中没有图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 将剪贴板图片粘贴到目标槽位
            editSlots[slotIndex].EditedImage = clipboardImage.CurrentImage.Clone() as Image;
            editSlots[slotIndex].OriginalImage = clipboardImage.OriginalImage?.Clone() as Image;

            Invalidate();
        }

        /// <summary>
        /// 旋转图片
        /// </summary>
        /// <param name="slotIndex">槽位索引</param>
        /// <param name="angle">旋转角度（90, 180, 270）</param>
        public void RotateImage(int slotIndex, int angle)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            if (slot.CurrentImage == null)
            {
                MessageBox.Show("该槽位没有图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 创建编辑后的图片
            if (slot.EditedImage == null)
            {
                slot.EditedImage = slot.OriginalImage.Clone() as Image;
            }

            // 旋转
            RotateFlipType rotateType;
            switch (angle)
            {
                case 90:
                    rotateType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 180:
                    rotateType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 270:
                    rotateType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    MessageBox.Show("旋转角度只支持 90、180、270 度", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            slot.EditedImage.RotateFlip(rotateType);
            Invalidate();
        }

        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        public void RotateClockwise(int slotIndex = 0)
        {
            RotateImage(slotIndex, 90);
        }

        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        public void RotateCounterClockwise(int slotIndex = 0)
        {
            RotateImage(slotIndex, 270);
        }

        /// <summary>
        /// 旋转180度
        /// </summary>
        public void Rotate180(int slotIndex = 0)
        {
            RotateImage(slotIndex, 180);
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        public void CropImage(int slotIndex, Rectangle cropRect)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            if (slot.CurrentImage == null)
            {
                MessageBox.Show("该槽位没有图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 确保裁剪区域在图片范围内
                cropRect.Intersect(new Rectangle(0, 0,
                    slot.CurrentImage.Width, slot.CurrentImage.Height));

                if (cropRect.Width <= 0 || cropRect.Height <= 0)
                {
                    MessageBox.Show("裁剪区域无效", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 创建裁剪后的图片
                var croppedImage = new Bitmap(cropRect.Width, cropRect.Height);
                using (var g = Graphics.FromImage(croppedImage))
                {
                    g.DrawImage(slot.CurrentImage,
                        new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                        cropRect, GraphicsUnit.Pixel);
                }

                slot.EditedImage = croppedImage;
                Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"裁剪失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 保存指定槽位的图片
        /// </summary>
        public void SaveImage(int slotIndex, string filePath = null, System.Drawing.Imaging.ImageFormat format = null)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            if (slot.CurrentImage == null)
            {
                MessageBox.Show("该槽位没有图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 如果没有指定路径，显示保存对话框
                if (string.IsNullOrEmpty(filePath))
                {
                    using (var dialog = new SaveFileDialog())
                    {
                        dialog.Filter = "PNG 图片 (*.png)|*.png|JPEG 图片 (*.jpg)|*.jpg|BMP 图片 (*.bmp)|*.bmp|所有文件 (*.*)|*.*";
                        dialog.DefaultExt = "png";
                        dialog.FileName = $"{slot.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            filePath = dialog.FileName;

                            // 根据扩展名确定格式
                            var ext = System.IO.Path.GetExtension(filePath).ToLower();
                            switch (ext)
                            {
                                case ".png":
                                    format = System.Drawing.Imaging.ImageFormat.Png;
                                    break;
                                case ".jpg":
                                case ".jpeg":
                                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;
                                case ".bmp":
                                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                                    break;
                                case ".gif":
                                    format = System.Drawing.Imaging.ImageFormat.Gif;
                                    break;
                                default:
                                    format = System.Drawing.Imaging.ImageFormat.Png;
                                    break;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                // 保存图片
                if (format == null)
                {
                    format = System.Drawing.Imaging.ImageFormat.Png;
                }

                slot.CurrentImage.Save(filePath, format);

                MessageBox.Show("保存成功！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 应用编辑（将编辑后的图片应用回原图片列表）
        /// </summary>
        public void ApplyEdit(int slotIndex = 0)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            if (slot.EditedImage == null)
            {
                MessageBox.Show("没有编辑操作需要应用", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 更新原始图片
            if (selectedIndex >= 0 && selectedIndex < imageItems.Count)
            {
                imageItems[selectedIndex].EditedImage = slot.EditedImage.Clone() as Image;
                imageItems[selectedIndex].OriginalImage = slot.EditedImage.Clone() as Image;
            }

            MessageBox.Show("编辑已应用", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 重置编辑（恢复到原始图片）
        /// </summary>
        public void ResetEdit(int slotIndex = 0)
        {
            if (!ValidateEditMode() || !ValidateSlotIndex(slotIndex))
            {
                return;
            }

            var slot = editSlots[slotIndex];
            slot.EditedImage = null;
            Invalidate();
        }

        private bool ValidateEditMode()
        {
            if (editMode == ImageEditMode.None)
            {
                MessageBox.Show("请先进入编辑模式", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= editSlots.Count)
            {
                MessageBox.Show($"槽位索引无效（0-{editSlots.Count - 1}）", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        #endregion

        #region 右键菜单

        /// <summary>
        /// 初始化右键菜单
        /// </summary>
        private void InitializeContextMenus()
        {
            // 普通模式右键菜单
            normalContextMenu = new ContextMenuStrip();
            CreateNormalContextMenu();

            // 编辑模式右键菜单
            editContextMenu = new ContextMenuStrip();
            CreateEditContextMenu();

            // 根据模式设置菜单
            UpdateContextMenu();
        }

        /// <summary>
        /// 创建普通模式右键菜单
        /// </summary>
        private void CreateNormalContextMenu()
        {
            // 显示模式子菜单
            var menuDisplayMode = new ToolStripMenuItem("显示模式");

            var menuSingle = new ToolStripMenuItem("单图显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Single);
            var menuGrid2 = new ToolStripMenuItem("2格显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Grid2);
            var menuGrid4 = new ToolStripMenuItem("4格显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Grid4);
            var menuGrid6 = new ToolStripMenuItem("6格显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Grid6);
            var menuGrid8 = new ToolStripMenuItem("8格显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Grid8);
            var menuGrid9 = new ToolStripMenuItem("9格显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Grid9);
            var menuGrid16 = new ToolStripMenuItem("16格显示", null,
                (s, e) => DisplayMode = ImageDisplayMode.Grid16);
            var menuMarquee = new ToolStripMenuItem("跑马灯", null,
                (s, e) => DisplayMode = ImageDisplayMode.Marquee);

            menuDisplayMode.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuSingle, menuGrid2, menuGrid4, menuGrid6,
                menuGrid8, menuGrid9, menuGrid16,
                new ToolStripSeparator(),
                menuMarquee
            });

            // 大小模式子菜单 - 根据当前显示模式动态调整
            var menuSizeMode = new ToolStripMenuItem("大小模式");
            menuSizeMode.DropDownOpening += (s, e) =>
            {
                menuSizeMode.DropDownItems.Clear();

                ImageSizeMode currentMode = GetCurrentSizeMode();

                var menuNormal = new ToolStripMenuItem("原始大小", null,
                    (sender, args) => SetCurrentSizeMode(ImageSizeMode.Normal));
                menuNormal.Checked = (currentMode == ImageSizeMode.Normal);

                var menuStretch = new ToolStripMenuItem("拉伸填充", null,
                    (sender, args) => SetCurrentSizeMode(ImageSizeMode.StretchImage));
                menuStretch.Checked = (currentMode == ImageSizeMode.StretchImage);

                var menuCenter = new ToolStripMenuItem("居中显示", null,
                    (sender, args) => SetCurrentSizeMode(ImageSizeMode.CenterImage));
                menuCenter.Checked = (currentMode == ImageSizeMode.CenterImage);

                var menuZoom = new ToolStripMenuItem("等比缩放", null,
                    (sender, args) => SetCurrentSizeMode(ImageSizeMode.Zoom));
                menuZoom.Checked = (currentMode == ImageSizeMode.Zoom);

                menuSizeMode.DropDownItems.AddRange(new ToolStripItem[]
                {
                    menuNormal, menuStretch, menuCenter, menuZoom
                });
            };

            // 图片信息显示子菜单
            var menuImageInfo = new ToolStripMenuItem("图片信息");

            var menuInfoNone = new ToolStripMenuItem("不显示", null,
                (s, e) => ImageInfoDisplay = ImageInfoDisplayMode.None);
            var menuInfoName = new ToolStripMenuItem("显示名称", null,
                (s, e) => ImageInfoDisplay = ImageInfoDisplayMode.NameOnly);
            var menuInfoDesc = new ToolStripMenuItem("显示描述", null,
                (s, e) => ImageInfoDisplay = ImageInfoDisplayMode.DescriptionOnly);
            var menuInfoBoth = new ToolStripMenuItem("名称和描述", null,
                (s, e) => ImageInfoDisplay = ImageInfoDisplayMode.NameAndDescription);

            var menuInfoPosTop = new ToolStripMenuItem("顶部显示", null,
                (s, e) => ImageInfoPosition = ImageInfoPosition.Top);
            var menuInfoPosBottom = new ToolStripMenuItem("底部显示", null,
                (s, e) => ImageInfoPosition = ImageInfoPosition.Bottom);

            menuImageInfo.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuInfoNone, menuInfoName, menuInfoDesc, menuInfoBoth,
                new ToolStripSeparator(),
                menuInfoPosTop, menuInfoPosBottom
            });

            // 编辑菜单
            var menuEnterEdit = new ToolStripMenuItem("编辑图片", null, MenuEnterEdit_Click);
            menuEnterEdit.ShortcutKeys = Keys.Control | Keys.E;

            // 保存菜单
            var menuSave = new ToolStripMenuItem("保存图片...", null, MenuSaveNormal_Click);
            menuSave.ShortcutKeys = Keys.Control | Keys.S;

            // 刷新菜单
            var menuRefresh = new ToolStripMenuItem("刷新 (F5)", null,
                (s, e) => Invalidate());

            // 添加所有菜单项（删除了上一张/下一张/删除）
            normalContextMenu.Items.AddRange(new ToolStripItem[]
            {
                menuDisplayMode,
                menuSizeMode,
                menuImageInfo,
                new ToolStripSeparator(),
                menuEnterEdit,
                menuSave,
                new ToolStripSeparator(),
                menuRefresh
            });

            // 菜单打开前更新状态
            normalContextMenu.Opening += NormalContextMenu_Opening;
        }

        private void SetCurrentSizeMode(ImageSizeMode mode)
        {
            switch (displayMode)
            {
                case ImageDisplayMode.Single:
                    SingleSizeMode = mode;
                    break;
                case ImageDisplayMode.Marquee:
                    MarqueeSizeMode = mode;
                    break;
                default:
                    GridSizeMode = mode;
                    break;
            }
        }

        /// <summary>
        /// 创建编辑模式右键菜单
        /// </summary>
        private void CreateEditContextMenu()
        {
            // 旋转菜单
            var menuRotate = new ToolStripMenuItem("旋转");

            var menuRotate90 = new ToolStripMenuItem("顺时针90°", null,
                (s, e) => RotateClockwise(rightClickedSlotIndex));
            var menuRotate180 = new ToolStripMenuItem("180°", null,
                (s, e) => Rotate180(rightClickedSlotIndex));
            var menuRotate270 = new ToolStripMenuItem("逆时针90°", null,
                (s, e) => RotateCounterClockwise(rightClickedSlotIndex));

            menuRotate.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuRotate90, menuRotate180, menuRotate270
            });

            // 剪切
            var menuCut = new ToolStripMenuItem("剪切", null, MenuCut_Click);
            menuCut.ShortcutKeys = Keys.Control | Keys.X;

            // 复制
            var menuCopy = new ToolStripMenuItem("复制", null, MenuCopy_Click);
            menuCopy.ShortcutKeys = Keys.Control | Keys.C;

            // 粘贴
            var menuPaste = new ToolStripMenuItem("粘贴", null, MenuPaste_Click);
            menuPaste.ShortcutKeys = Keys.Control | Keys.V;

            // 保存
            var menuSave = new ToolStripMenuItem("保存图片...", null, MenuSaveEdit_Click);
            menuSave.ShortcutKeys = Keys.Control | Keys.S;

            // 重置
            var menuReset = new ToolStripMenuItem("重置编辑", null, MenuReset_Click);
            menuReset.ShortcutKeys = Keys.Control | Keys.R;

            // 应用编辑（Ctrl+A 可能与全选冲突，改为 Ctrl+Enter，但 Enter 也不支持，改为在文本显示）
            var menuApply = new ToolStripMenuItem("应用编辑 (Ctrl+A)", null, MenuApply_Click);

            menuApply.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;

            // 退出编辑
            var menuExitEdit = new ToolStripMenuItem("退出编辑 (Esc)", null, MenuExitEdit_Click);

            // 添加所有菜单项
            editContextMenu.Items.AddRange(new ToolStripItem[]
            {
                menuRotate,
                new ToolStripSeparator(),
                menuCut,
                menuCopy,
                menuPaste,
                new ToolStripSeparator(),
                menuSave,
                menuReset,
                menuApply,
                new ToolStripSeparator(),
                menuExitEdit
            });

            // 菜单打开前更新状态
            editContextMenu.Opening += EditContextMenu_Opening;
        }

        /// <summary>
        /// 更新右键菜单
        /// </summary>
        private void UpdateContextMenu()
        {
            if (!EnableContextMenu)
            {
                ContextMenuStrip = null;
                return;
            }

            ContextMenuStrip = editMode != ImageEditMode.None
                ? editContextMenu
                : normalContextMenu;
        }

        #region 普通模式菜单事件

        private void NormalContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasImages = imageItems.Count > 0;
            bool hasSelection = selectedIndex >= 0 && selectedIndex < imageItems.Count;

            // 更新菜单项状态
            foreach (ToolStripItem item in normalContextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    switch (menuItem.Text)
                    {
                        case "编辑图片":
                        case "保存图片...":
                            menuItem.Enabled = hasSelection;
                            break;
                    }

                    // 更新显示模式选中状态
                    if (menuItem.Text == "显示模式")
                    {
                        UpdateDisplayModeChecks(menuItem);
                    }

                    // 更新大小模式选中状态
                    if (menuItem.Text == "大小模式")
                    {
                        UpdateSizeModeChecks(menuItem);
                    }

                    // 更新图片信息选中状态
                    if (menuItem.Text == "图片信息")
                    {
                        UpdateImageInfoChecks(menuItem);
                    }
                }
            }
        }

        private void MenuEnterEdit_Click(object sender, EventArgs e)
        {
            if (selectedIndex < 0 || selectedIndex >= imageItems.Count)
            {
                MessageBox.Show("请先选择要编辑的图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 显示编辑模式选择对话框
            using (var dialog = new Form())
            {
                dialog.Text = "选择编辑模式";
                dialog.Size = new Size(300, 180);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var label = new Label
                {
                    Text = "请选择编辑模式:",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                var radioSingle = new RadioButton
                {
                    Text = "单格编辑",
                    Location = new Point(40, 50),
                    Checked = true,
                    AutoSize = true
                };

                var radioGrid2 = new RadioButton
                {
                    Text = "2格编辑",
                    Location = new Point(40, 75),
                    AutoSize = true
                };

                var radioGrid4 = new RadioButton
                {
                    Text = "4格编辑",
                    Location = new Point(40, 100),
                    AutoSize = true
                };

                var btnOK = new Button
                {
                    Text = "确定",
                    DialogResult = DialogResult.OK,
                    Location = new Point(100, 130),
                    Size = new Size(75, 25)
                };

                var btnCancel = new Button
                {
                    Text = "取消",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(185, 130),
                    Size = new Size(75, 25)
                };

                dialog.Controls.AddRange(new Control[]
                {
            label, radioSingle, radioGrid2, radioGrid4, btnOK, btnCancel
                });

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (radioSingle.Checked)
                    {
                        EditMode = ImageEditMode.Single;
                    }
                    else if (radioGrid2.Checked)
                    {
                        EditMode = ImageEditMode.Grid2;
                    }
                    else if (radioGrid4.Checked)
                    {
                        EditMode = ImageEditMode.Grid4;
                    }
                }
            }
        }

        private void MenuSaveNormal_Click(object sender, EventArgs e)
        {
            if (selectedIndex >= 0 && selectedIndex < imageItems.Count)
            {
                var item = imageItems[selectedIndex];
                if (item.CurrentImage != null)
                {
                    SaveImageToFile(item.CurrentImage, item.Name);
                }
            }
        }

        private void MenuDelete_Click(object sender, EventArgs e)
        {
            if (selectedIndex >= 0 && selectedIndex < imageItems.Count)
            {
                var result = MessageBox.Show("确定要删除选中的图片吗？", "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    RemoveSelectedImage();
                }
            }
        }

        private void UpdateDisplayModeChecks(ToolStripMenuItem menuItem)
        {
            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                if (subItem is ToolStripMenuItem subMenu)
                {
                    subMenu.Checked = false;

                    switch (subMenu.Text)
                    {
                        case "单图显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Single;
                            break;
                        case "2格显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Grid2;
                            break;
                        case "4格显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Grid4;
                            break;
                        case "6格显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Grid6;
                            break;
                        case "8格显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Grid8;
                            break;
                        case "9格显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Grid9;
                            break;
                        case "16格显示":
                            subMenu.Checked = displayMode == ImageDisplayMode.Grid16;
                            break;
                        case "跑马灯":
                            subMenu.Checked = displayMode == ImageDisplayMode.Marquee;
                            break;
                    }
                }
            }
        }

        private void UpdateSizeModeChecks(ToolStripMenuItem menuItem)
        {
            ImageSizeMode sizeMode = GetCurrentSizeMode();

            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                if (subItem is ToolStripMenuItem subMenu)
                {
                    subMenu.Checked = false;

                    switch (subMenu.Text)
                    {
                        case "原始大小":
                            subMenu.Checked = sizeMode == ImageSizeMode.Normal;
                            break;
                        case "拉伸填充":
                            subMenu.Checked = sizeMode == ImageSizeMode.StretchImage;
                            break;
                        case "居中显示":
                            subMenu.Checked = sizeMode == ImageSizeMode.CenterImage;
                            break;
                        case "等比缩放":
                            subMenu.Checked = sizeMode == ImageSizeMode.Zoom;
                            break;
                    }
                }
            }
        }

        private void UpdateImageInfoChecks(ToolStripMenuItem menuItem)
        {
            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                if (subItem is ToolStripMenuItem subMenu)
                {
                    subMenu.Checked = false;

                    switch (subMenu.Text)
                    {
                        case "不显示":
                            subMenu.Checked = imageInfoDisplay == ImageInfoDisplayMode.None;
                            break;
                        case "显示名称":
                            subMenu.Checked = imageInfoDisplay == ImageInfoDisplayMode.NameOnly;
                            break;
                        case "显示描述":
                            subMenu.Checked = imageInfoDisplay == ImageInfoDisplayMode.DescriptionOnly;
                            break;
                        case "名称和描述":
                            subMenu.Checked = imageInfoDisplay == ImageInfoDisplayMode.NameAndDescription;
                            break;
                        case "顶部显示":
                            subMenu.Checked = imageInfoPosition == ImageInfoPosition.Top;
                            break;
                        case "底部显示":
                            subMenu.Checked = imageInfoPosition == ImageInfoPosition.Bottom;
                            break;
                    }
                }
            }
        }

        #endregion

        #region 编辑模式菜单事件

        private void EditContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasClipboard = clipboardImage != null && clipboardImage.CurrentImage != null;
            bool hasImage = rightClickedSlotIndex >= 0 &&
                            rightClickedSlotIndex < editSlots.Count &&
                            editSlots[rightClickedSlotIndex].CurrentImage != null;

            // 更新菜单项状态
            foreach (ToolStripItem item in editContextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    switch (menuItem.Text)
                    {
                        case "旋转":
                        case "剪切":
                        case "复制":
                        case "保存图片...":
                        case "重置编辑":
                            menuItem.Enabled = hasImage;
                            break;
                        case "粘贴":
                            menuItem.Enabled = hasClipboard;
                            break;
                        case "应用编辑":
                            menuItem.Enabled = editSlots.Count > 0 &&
                                             editSlots[0]?.EditedImage != null;
                            break;
                    }
                }
            }
        }

        private void MenuCut_Click(object sender, EventArgs e)
        {
            CutImage(rightClickedSlotIndex);
        }

        private void MenuCopy_Click(object sender, EventArgs e)
        {
            CopyImage(rightClickedSlotIndex);
        }

        private void MenuPaste_Click(object sender, EventArgs e)
        {
            PasteImage(rightClickedSlotIndex);
        }

        private void MenuSaveEdit_Click(object sender, EventArgs e)
        {
            SaveImage(rightClickedSlotIndex);
        }

        private void MenuReset_Click(object sender, EventArgs e)
        {
            ResetEdit(rightClickedSlotIndex);
        }

        private void MenuApply_Click(object sender, EventArgs e)
        {
            ApplyEdit(0);
        }

        private void MenuExitEdit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("退出编辑模式？未保存的更改将丢失。", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                EditMode = ImageEditMode.None;
            }
        }

        #endregion

        #endregion

        #region 鼠标交互

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            lastMousePosition = e.Location;

            // Single 模式下的箭头检测
            if (displayMode == ImageDisplayMode.Single && EnableArrowNavigation && editMode == ImageEditMode.None && imageItems.Count > 1)
            {
                UpdateArrowState(e.Location);
            }
            else
            {
                // 其他模式清除箭头状态
                if (showLeftArrow || showRightArrow)
                {
                    showLeftArrow = false;
                    showRightArrow = false;
                    isLeftArrowHovered = false;
                    isRightArrowHovered = false;
                    Invalidate();
                }

                // 网格模式的悬停检测
                int newHoveredIndex = GetItemIndexAtPoint(e.Location);
                if (newHoveredIndex != hoveredIndex)
                {
                    hoveredIndex = newHoveredIndex;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 更新箭头状态
        /// </summary>
        private void UpdateArrowState(Point mouseLocation)
        {
            var contentArea = GetContentArea();
            bool needsUpdate = false;

            // 计算箭头区域
            leftArrowBounds = new Rectangle(contentArea.X, contentArea.Y, arrowWidth, contentArea.Height);
            rightArrowBounds = new Rectangle(contentArea.Right - arrowWidth, contentArea.Y, arrowWidth, contentArea.Height);

            // 检测左箭头
            bool canShowLeft = selectedIndex > 0;
            bool mouseOnLeft = canShowLeft && leftArrowBounds.Contains(mouseLocation);

            if (showLeftArrow != mouseOnLeft || isLeftArrowHovered != mouseOnLeft)
            {
                showLeftArrow = mouseOnLeft;
                isLeftArrowHovered = mouseOnLeft;
                needsUpdate = true;
            }

            // 检测右箭头
            bool canShowRight = selectedIndex < imageItems.Count - 1;
            bool mouseOnRight = canShowRight && rightArrowBounds.Contains(mouseLocation);

            if (showRightArrow != mouseOnRight || isRightArrowHovered != mouseOnRight)
            {
                showRightArrow = mouseOnRight;
                isRightArrowHovered = mouseOnRight;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredIndex >= 0)
            {
                hoveredIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                rightClickedSlotIndex = GetItemIndexAtPoint(e.Location);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // Single 模式箭头点击
            if (displayMode == ImageDisplayMode.Single && EnableArrowNavigation && editMode == ImageEditMode.None)
            {
                if (showLeftArrow && leftArrowBounds.Contains(e.Location))
                {
                    SelectPrevious();
                    return;
                }

                if (showRightArrow && rightArrowBounds.Contains(e.Location))
                {
                    SelectNext();
                    return;
                }
            }

            // 原有的点击处理
            int clickedIndex = GetItemIndexAtPoint(e.Location);

            if (editMode != ImageEditMode.None)
            {
                if (clickedIndex >= 0 && clickedIndex < editSlots.Count)
                {
                    hoveredIndex = clickedIndex;
                    Invalidate();
                }
            }
            else
            {
                if (clickedIndex >= 0 && clickedIndex < imageItems.Count)
                {
                    SelectedIndex = clickedIndex;
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int clickedIndex = GetItemIndexAtPoint(e.Location);

            if (editMode == ImageEditMode.None &&
                clickedIndex >= 0 && clickedIndex < imageItems.Count)
            {
                OnImageDoubleClicked(new ImageEventArgs(clickedIndex));
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (vScrollBar.Visible)
            {
                int delta = e.Delta > 0 ? -DefaultRowHeight : DefaultRowHeight;
                int newValue = Math.Max(vScrollBar.Minimum,
                    Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange + 1,
                        vScrollBar.Value + delta));

                if (newValue != vScrollBar.Value)
                {
                    vScrollBar.Value = newValue;
                    scrollOffsetY = newValue;
                    Invalidate();
                }
            }
        }

        private int GetItemIndexAtPoint(Point location)
        {
            if (editMode != ImageEditMode.None)
            {
                // 编辑模式：检查槽位
                for (int i = 0; i < editSlots.Count; i++)
                {
                    if (editSlots[i].Bounds.Contains(location))
                    {
                        return i;
                    }
                }
            }
            else if (displayMode == ImageDisplayMode.Single)
            {
                // 单图模式
                return selectedIndex;
            }
            else if (displayMode != ImageDisplayMode.Marquee)
            {
                // 网格模式：考虑滚动偏移
                var testPoint = new Point(location.X, location.Y + scrollOffsetY);

                for (int i = 0; i < imageItems.Count; i++)
                {
                    if (imageItems[i].Bounds.Contains(testPoint))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        #endregion

        #region 滚动条处理

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            scrollOffsetY = e.NewValue;
            Invalidate();
        }

        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            scrollOffsetX = e.NewValue;
            Invalidate();
        }

        #endregion

        #region 跑马灯动画

        private void MarqueeTimer_Tick(object sender, EventArgs e)
        {
            if (displayMode != ImageDisplayMode.Marquee || imageItems.Count == 0)
            {
                marqueeTimer.Stop();
                return;
            }

            marqueeOffset += marqueeSpeed;

            // 计算总宽度
            var contentArea = GetContentArea();
            int imageHeight = contentArea.Height - 20;
            int totalWidth = 0;

            foreach (var item in imageItems)
            {
                if (item.CurrentImage != null)
                {
                    float aspectRatio = (float)item.CurrentImage.Width / item.CurrentImage.Height;
                    int imageWidth = (int)(imageHeight * aspectRatio);
                    totalWidth += imageWidth + marqueeImageSpacing;
                }
            }

            // 循环滚动
            if (marqueeOffset >= totalWidth)
            {
                marqueeOffset = 0;
            }

            Invalidate();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置图片描述
        /// </summary>
        public void SetImageDescription(int index, string description)
        {
            if (index >= 0 && index < imageItems.Count)
            {
                imageItems[index].Description = description;
                Invalidate();
            }
        }

        /// <summary>
        /// 设置图片名称
        /// </summary>
        public void SetImageName(int index, string name)
        {
            if (index >= 0 && index < imageItems.Count)
            {
                imageItems[index].Name = name;
                Invalidate();
            }
        }

        /// <summary>
        /// 批量设置图片信息
        /// </summary>
        public void SetImageInfo(int index, string name, string description)
        {
            if (index >= 0 && index < imageItems.Count)
            {
                imageItems[index].Name = name;
                imageItems[index].Description = description;
                Invalidate();
            }
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        public void AddImage(Image image, string name = null)
        {
            if (image == null)
            {
                return;
            }

            int index = imageItems.Count;
            imageItems.Add(new ImageItem(image, index, name ?? $"Image {index}"));

            UpdateLayout();
            Invalidate();
        }

        /// <summary>
        /// 移除图片
        /// </summary>
        public void RemoveImage(int index)
        {
            if (index >= 0 && index < imageItems.Count)
            {
                imageItems.RemoveAt(index);

                // 重新索引
                for (int i = 0; i < imageItems.Count; i++)
                {
                    imageItems[i].Index = i;
                }

                if (selectedIndex >= imageItems.Count)
                {
                    selectedIndex = imageItems.Count - 1;
                }

                UpdateLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// 移除选中的图片
        /// </summary>
        public void RemoveSelectedImage()
        {
            if (selectedIndex >= 0)
            {
                RemoveImage(selectedIndex);
            }
        }

        /// <summary>
        /// 清除所有图片
        /// </summary>
        public void ClearImages()
        {
            imageItems.Clear();
            selectedIndex = -1;
            hoveredIndex = -1;

            UpdateLayout();
            Invalidate();
        }

        /// <summary>
        /// 获取指定索引的图片
        /// </summary>
        public Image GetImage(int index)
        {
            if (index >= 0 && index < imageItems.Count)
            {
                return imageItems[index].CurrentImage;
            }
            return null;
        }

        /// <summary>
        /// 选择上一张图片
        /// </summary>
        public void SelectPrevious()
        {
            if (imageItems.Count == 0)
            {
                return;
            }

            int newIndex;
            if (selectedIndex <= 0)
            {
                newIndex = imageItems.Count - 1;
            }
            else
            {
                newIndex = selectedIndex - 1;
            }

            System.Diagnostics.Debug.WriteLine($"SelectPrevious: {selectedIndex} -> {newIndex}");

            SelectedIndex = newIndex;

            // 强制更新单图模式布局
            if (displayMode == ImageDisplayMode.Single)
            {
                UpdateSingleLayout();
                Invalidate();
                Update();
            }
        }

        /// <summary>
        /// 选择下一张图片
        /// </summary>
        public void SelectNext()
        {
            if (imageItems.Count == 0)
            {
                return;
            }

            int newIndex;
            if (selectedIndex >= imageItems.Count - 1)
            {
                newIndex = 0;
            }
            else
            {
                newIndex = selectedIndex + 1;
            }

            System.Diagnostics.Debug.WriteLine($"SelectNext: {selectedIndex} -> {newIndex}");

            SelectedIndex = newIndex;

            // 强制更新单图模式布局
            if (displayMode == ImageDisplayMode.Single)
            {
                UpdateSingleLayout();
                Invalidate();
                Update();
            }
        }

        /// <summary>
        /// 开始跑马灯
        /// </summary>
        public void StartMarquee()
        {
            DisplayMode = ImageDisplayMode.Marquee;
        }

        /// <summary>
        /// 停止跑马灯
        /// </summary>
        public void StopMarquee()
        {
            if (displayMode == ImageDisplayMode.Marquee)
            {
                DisplayMode = ImageDisplayMode.Single;
            }
        }

        /// <summary>
        /// 保存图片到文件
        /// </summary>
        public void SaveImageToFile(Image image, string defaultName)
        {
            if (image == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG 图片 (*.png)|*.png|JPEG 图片 (*.jpg)|*.jpg|BMP 图片 (*.bmp)|*.bmp|所有文件 (*.*)|*.*";
                dialog.DefaultExt = "png";
                dialog.FileName = $"{defaultName}_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var ext = System.IO.Path.GetExtension(dialog.FileName).ToLower();
                        System.Drawing.Imaging.ImageFormat format;

                        switch (ext)
                        {
                            case ".jpg":
                            case ".jpeg":
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                format = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                            case ".gif":
                                format = System.Drawing.Imaging.ImageFormat.Gif;
                                break;
                            default:
                                format = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                        }

                        image.Save(dialog.FileName, format);
                        MessageBox.Show("保存成功！", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion

        #region 键盘支持

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // 编辑模式快捷键
            if (editMode != ImageEditMode.None)
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            CopyImage(hoveredIndex >= 0 ? hoveredIndex : 0);
                            e.Handled = true;
                            break;
                        case Keys.X:
                            CutImage(hoveredIndex >= 0 ? hoveredIndex : 0);
                            e.Handled = true;
                            break;
                        case Keys.V:
                            PasteImage(hoveredIndex >= 0 ? hoveredIndex : 0);
                            e.Handled = true;
                            break;
                        case Keys.S:
                            SaveImage(hoveredIndex >= 0 ? hoveredIndex : 0);
                            e.Handled = true;
                            break;
                        case Keys.R:
                            ResetEdit(hoveredIndex >= 0 ? hoveredIndex : 0);
                            e.Handled = true;
                            break;
                        case Keys.A:
                            ApplyEdit(0);
                            e.Handled = true;
                            break;
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    EditMode = ImageEditMode.None;
                    e.Handled = true;
                }
            }
            else // 普通模式快捷键
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.E:
                            MenuEnterEdit_Click(this, EventArgs.Empty);
                            e.Handled = true;
                            break;
                        case Keys.S:
                            MenuSaveNormal_Click(this, EventArgs.Empty);
                            e.Handled = true;
                            break;
                    }
                }
                else if (e.KeyCode == Keys.F5)
                {
                    Invalidate();
                    e.Handled = true;
                }
            }

            // 通用快捷键
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Up:
                    SelectPrevious();
                    e.Handled = true;
                    break;

                case Keys.Right:
                case Keys.Down:
                    SelectNext();
                    e.Handled = true;
                    break;

                case Keys.Delete:
                    if (editMode == ImageEditMode.None && selectedIndex >= 0)
                    {
                        var result = MessageBox.Show("确定要删除选中的图片吗？", "确认",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            RemoveSelectedImage();
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.Home:
                    if (imageItems.Count > 0)
                    {
                        SelectedIndex = 0;
                    }
                    e.Handled = true;
                    break;

                case Keys.End:
                    if (imageItems.Count > 0)
                    {
                        SelectedIndex = imageItems.Count - 1;
                    }
                    e.Handled = true;
                    break;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                marqueeTimer?.Stop();
                marqueeTimer?.Dispose();

                vScrollBar?.Dispose();
                hScrollBar?.Dispose();

                // 清理图片
                foreach (var item in imageItems)
                {
                    item.OriginalImage?.Dispose();
                    item.EditedImage?.Dispose();
                }
                imageItems.Clear();

                foreach (var slot in editSlots)
                {
                    slot?.OriginalImage?.Dispose();
                    slot?.EditedImage?.Dispose();
                }
                editSlots.Clear();

                clipboardImage?.OriginalImage?.Dispose();
                clipboardImage?.EditedImage?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }


    #region 枚举和辅助类

    /// <summary>
    /// 图片显示模式
    /// </summary>
    public enum ImageDisplayMode
    {
        Single,      // 单幅显示
        Grid2,       // 2格 (1x2)
        Grid4,       // 4格 (2x2)
        Grid6,       // 6格 (2x3)
        Grid8,       // 8格 (2x4)
        Grid9,       // 9格 (3x3)
        Grid16,      // 16格 (4x4)
        Marquee      // 跑马灯
    }

    /// <summary>
    /// 图片大小模式
    /// </summary>
    public enum ImageSizeMode
    {
        Normal,          // 原始大小
        StretchImage,    // 拉伸填充
        AutoSize,        // 自动调整控件大小
        CenterImage,     // 居中显示
        Zoom             // 等比缩放适应
    }

    /// <summary>
    /// 编辑模式
    /// </summary>
    public enum ImageEditMode
    {
        None,      // 非编辑模式
        Single,    // 单格编辑
        Grid2,     // 2格编辑
        Grid4      // 4格编辑
    }

    /// <summary>
    /// 图片信息显示模式
    /// </summary>
    public enum ImageInfoDisplayMode
    {
        None,           // 不显示
        NameOnly,       // 仅显示名称
        DescriptionOnly, // 仅显示描述
        NameAndDescription // 显示名称和描述
    }

    /// <summary>
    /// 图片信息显示位置
    /// </summary>
    public enum ImageInfoPosition
    {
        Top,    // 顶部
        Bottom  // 底部
    }

    public class ImageEventArgs : EventArgs
    {
        public ImageEventArgs(int imageIndex)
        {
            ImageIndex = imageIndex;
        }

        public int ImageIndex { get; set; }
    }

    /// <summary>
    /// 图片项
    /// </summary>
    public class ImageItem
    {
        public ImageItem(Image image, int index, string name = null, string description = null)
        {
            OriginalImage = image;
            Index = index;
            Name = name ?? $"Image {index}";
            Description = description ?? "";
        }

        public Image OriginalImage { get; set; }
        public Image EditedImage { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } // 新增：图片描述
        public int Index { get; set; }
        public bool IsSelected { get; set; }
        public Rectangle Bounds { get; set; }

        public Image CurrentImage => EditedImage ?? OriginalImage;


        public ImageItem Clone()
        {
            return new ImageItem(OriginalImage?.Clone() as Image, Index, Name, Description)
            {
                EditedImage = EditedImage?.Clone() as Image,
                IsSelected = IsSelected
            };
        }
    }

    #endregion

    #region 设计时支持

    public class FluentImageViewerDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentImageViewerActionList(this.Component));
                }
                return actionLists;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
        }
    }

    public class FluentImageViewerActionList : DesignerActionList
    {
        private FluentImageViewer imageViewer;
        private DesignerActionUIService designerService;

        public FluentImageViewerActionList(IComponent component) : base(component)
        {
            imageViewer = component as FluentImageViewer;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            items.Add(new DesignerActionHeaderItem("操作"));

            if (imageViewer.Dock == DockStyle.Fill)
            {
                items.Add(new DesignerActionMethodItem(this, "UndockControl", "取消停靠", "操作", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "DockFill", "在父容器中停靠", "操作", true));
            }

            // 添加属性项
            items.Add(new DesignerActionHeaderItem("图片源"));
            items.Add(new DesignerActionPropertyItem("FluentImageSource", "Fluent图片列表", "图片源", "设置Fluent图片来源"));
            //items.Add(new DesignerActionPropertyItem("ImageSource", "图片列表", "图片源", "设置图片来源"));

            items.Add(new DesignerActionHeaderItem("显示设置"));
            items.Add(new DesignerActionPropertyItem("DisplayMode", "显示模式", "显示设置", "设置图片显示模式"));
            items.Add(new DesignerActionPropertyItem("SizeMode", "大小模式", "显示设置", "设置图片大小模式"));

            return items;
        }

        // 属性包装器
        public ImageList ImageSource
        {
            get => imageViewer.ImageSource;
            set => SetProperty("ImageSource", value);
        }

        public FluentImageList FluentImageSource
        {
            get => imageViewer.FluentImageSource;
            set => SetProperty("FluentImageSource", value);
        }

        public ImageDisplayMode DisplayMode
        {
            get => imageViewer.DisplayMode;
            set => SetProperty("DisplayMode", value);
        }

        public ImageSizeMode SizeMode
        {
            get => imageViewer.SizeMode;
            set => SetProperty("SizeMode", value);
        }

        [DisplayName("在父容器中停靠")]
        [Description("将控件停靠填充父容器")]
        public void DockFill()
        {
            if (imageViewer.Parent != null)
            {
                PropertyDescriptor prop = GetPropertyByName("Dock");
                prop.SetValue(imageViewer, DockStyle.Fill);
            }
        }

        [DisplayName("取消停靠")]
        [Description("取消控件的停靠")]
        public void UndockControl()
        {
            PropertyDescriptor prop = GetPropertyByName("Dock");
            prop.SetValue(imageViewer, DockStyle.None);
        }

        private PropertyDescriptor GetPropertyByName(string propName)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(imageViewer)[propName];
            if (prop == null)
            {
                throw new ArgumentException("未找到属性", propName);
            }

            return prop;
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(imageViewer)[propertyName];
            if (property != null)
            {
                property.SetValue(imageViewer, value);
                designerService?.Refresh(imageViewer);
            }
        }
    }

    #endregion

}
