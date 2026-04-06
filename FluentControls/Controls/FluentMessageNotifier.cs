using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Policy;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using FluentControls.IconFonts;
using System.Drawing.Design;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentMessageNotifierDesigner))]
    [ToolboxItem(true)]
    [DefaultEvent("NotificationClicked")]
    public class FluentMessageNotifier : FluentControlBase
    {
        #region 字段

        private List<NotificationItem> notifications;
        private NotificationPanel notificationPanel;

        private Image icon;
        private int badgeCount = 0;
        private bool showBadge = true;
        private BadgePosition badgePosition = BadgePosition.TopRight;
        private int maxBadgeCount = 99;

        private Color badgeBackColor = Color.FromArgb(244, 67, 54); // 红色
        private Color badgeForeColor = Color.White;
        private int iconSize = 24; // 图标大小
        private int badgeExtraSpace = 8; // 角标额外空间

        private bool isPanelVisible = false;
        private NotificationFilter filter;
        private NotificationFilterMode filterMode = NotificationFilterMode.WarningAndError;

        // 防止循环更新的标志
        private bool IsInFilterModeChange = false;

        #endregion

        #region 构造函数

        public FluentMessageNotifier()
        {
            notifications = new List<NotificationItem>();

            // 使用模式初始化过滤器
            filterMode = NotificationFilterMode.WarningAndError;
            filter = NotificationFilter.FromMode(filterMode);

            Size = CalculateRequiredSize(); // 计算需要的尺寸
            Cursor = Cursors.Hand;

            // 默认图标
            icon = CreateDefaultBellIcon();

            UpdateBadgeCount();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 通知图标
        /// </summary>
        [Category("Fluent")]
        [Description("通知图标")]
        [Editor(typeof(IconFontImageEditor), typeof(UITypeEditor))]
        public Image Icon
        {
            get => icon;
            set
            {
                icon = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 图标大小
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(24)]
        [Description("通知图标大小")]
        public int IconSize
        {
            get => iconSize;
            set
            {
                if (iconSize != value && value > 0)
                {
                    iconSize = value;
                    Size = CalculateRequiredSize();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示角标
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("是否显示角标")]
        public bool ShowBadge
        {
            get => showBadge;
            set
            {
                showBadge = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 角标位置
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(BadgePosition.TopRight)]
        [Description("角标显示位置")]
        public new BadgePosition BadgePosition
        {
            get => badgePosition;
            set
            {
                if (badgePosition != value)
                {
                    badgePosition = value;
                    Size = CalculateRequiredSize(); // 重新计算尺寸
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 角标最大显示数量
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(99)]
        [Description("角标最大显示数量, 超过显示+")]
        public int MaxBadgeCount
        {
            get => maxBadgeCount;
            set
            {
                maxBadgeCount = Math.Max(1, value);
                UpdateTooltip();
                Invalidate();
            }
        }

        /// <summary>
        /// 角标背景色
        /// </summary>
        [Category("Fluent")]
        [Description("角标背景色")]
        public Color BadgeBackColor
        {
            get => badgeBackColor;
            set
            {
                badgeBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 角标前景色
        /// </summary>
        [Category("Fluent")]
        [Description("角标文字颜色")]
        public Color BadgeForeColor
        {
            get => badgeForeColor;
            set
            {
                badgeForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 通知过滤模式
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(NotificationFilterMode.WarningAndError)]
        [Description("通知过滤模式")]
        public NotificationFilterMode FilterMode
        {
            get => filterMode;
            set
            {
                if (filterMode != value)
                {
                    filterMode = value;

                    // 只有非自定义模式才自动更新过滤器
                    if (filterMode != NotificationFilterMode.Custom)
                    {
                        filter = NotificationFilter.FromMode(filterMode);
                    }

                    OnFilterModeChanged();
                }
            }
        }

        /// <summary>
        /// 通知过滤器(设计时隐藏)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NotificationFilter Filter
        {
            get => filter;
            set
            {
                filter = value ?? NotificationFilter.Default;

                // 设置自定义过滤器时, 自动切换模式
                if (!IsInFilterModeChange)
                {
                    filterMode = NotificationFilterMode.Custom;
                }
            }
        }

        /// <summary>
        /// 未读通知数量
        /// </summary>
        [Browsable(false)]
        public int UnreadCount => notifications.Count(n => !n.IsRead);

        /// <summary>
        /// 未读重要通知数量
        /// </summary>
        [Browsable(false)]
        public int UnreadImportantCount => notifications.Count(n => !n.IsRead && n.IsImportant);

        /// <summary>
        /// 所有通知
        /// </summary>
        [Browsable(false)]
        public IEnumerable<NotificationItem> Notifications => notifications.AsReadOnly();

        #endregion

        #region 事件

        public event EventHandler<NotificationEventArgs> NotificationAdded;
        public event EventHandler<NotificationEventArgs> NotificationClicked;
        public event EventHandler<NotificationEventArgs> NotificationRead;
        public event EventHandler<NotificationEventArgs> NotificationDeleted;

        public event EventHandler FilterModeChanged;
        public event EventHandler AllNotificationsRead;
        public event EventHandler PanelOpened;
        public event EventHandler PanelClosed;

        protected virtual void OnNotificationAdded(NotificationEventArgs e)
        {
            NotificationAdded?.Invoke(this, e);
        }

        protected virtual void OnNotificationClicked(NotificationEventArgs e)
        {
            NotificationClicked?.Invoke(this, e);
        }

        protected virtual void OnNotificationRead(NotificationEventArgs e)
        {
            NotificationRead?.Invoke(this, e);
        }

        protected virtual void OnNotificationDeleted(NotificationEventArgs e)
        {
            NotificationDeleted?.Invoke(this, e);
        }

        protected virtual void OnAllNotificationsRead()
        {
            AllNotificationsRead?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPanelOpened()
        {
            PanelOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPanelClosed()
        {
            PanelClosed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFilterModeChanged()
        {
            IsInFilterModeChange = true;
            try
            {
                FilterModeChanged?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsInFilterModeChange = false;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加通知
        /// </summary>
        public void AddNotification(NotificationItem notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            notifications.Insert(0, notification); // 插入到开头(最新的在前面)
            UpdateBadgeCount();
            UpdateTooltip();

            OnNotificationAdded(new NotificationEventArgs(notification));

            // 如果面板已打开, 刷新显示
            if (isPanelVisible && notificationPanel != null)
            {
                notificationPanel.RefreshNotifications();
            }
        }

        /// <summary>
        /// 从FluentMessage添加通知
        /// </summary>
        public void AddNotificationFromMessage(MessageOptions messageOptions)
        {
            if (messageOptions == null)
            {
                return;
            }

            // 每次都重新获取当前的过滤器
            var currentFilter = this.Filter;

            if (currentFilter == null || currentFilter.ShouldNotify == null)
            {
                return;
            }

            // 检查是否应该通知
            if (!currentFilter.ShouldNotify(messageOptions))
            {
                return;
            }

            var notification = new NotificationItem
            {
                Title = messageOptions.Title,
                Content = messageOptions.Content,
                Type = messageOptions.Type,
                Icon = messageOptions.CustomIcon,
                IsImportant = currentFilter.IsImportant != null && currentFilter.IsImportant(messageOptions),
                SourceMessageOptions = messageOptions
            };

            AddNotification(notification);
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public void MarkAsRead(Guid notificationId)
        {
            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                UpdateBadgeCount();
                UpdateTooltip();
                OnNotificationRead(new NotificationEventArgs(notification));

                if (isPanelVisible && notificationPanel != null)
                {
                    notificationPanel.RefreshNotifications();
                }
            }
        }

        /// <summary>
        /// 全部标记为已读
        /// </summary>
        public void MarkAllAsRead()
        {
            bool hasChanges = false;
            foreach (var notification in notifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
                hasChanges = true;
            }

            if (hasChanges)
            {
                UpdateBadgeCount();
                UpdateTooltip();
                OnAllNotificationsRead();

                if (isPanelVisible && notificationPanel != null)
                {
                    notificationPanel.RefreshNotifications();
                }
            }
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public void DeleteNotification(Guid notificationId)
        {
            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notifications.Remove(notification);
                UpdateBadgeCount();
                UpdateTooltip();
                OnNotificationDeleted(new NotificationEventArgs(notification));

                if (isPanelVisible && notificationPanel != null)
                {
                    notificationPanel.RefreshNotifications();
                }
            }
        }

        /// <summary>
        /// 清除所有已读的普通通知
        /// </summary>
        public void ClearReadNotifications()
        {
            var toRemove = notifications.Where(n => n.IsRead && !n.IsImportant).ToList();
            foreach (var notification in toRemove)
            {
                notifications.Remove(notification);
            }

            if (toRemove.Any())
            {
                UpdateBadgeCount();
                UpdateTooltip();

                if (isPanelVisible && notificationPanel != null)
                {
                    notificationPanel.RefreshNotifications();
                }
            }
        }

        /// <summary>
        /// 清除所有通知
        /// </summary>
        public void ClearAll()
        {
            notifications.Clear();
            UpdateBadgeCount();
            UpdateTooltip();

            if (isPanelVisible && notificationPanel != null)
            {
                notificationPanel.RefreshNotifications();
            }
        }

        /// <summary>
        /// 显示/隐藏通知面板
        /// </summary>
        public void TogglePanel()
        {
            if (isPanelVisible)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        /// <summary>
        /// 显示通知面板
        /// </summary>
        public void ShowPanel()
        {
            if (isPanelVisible)
            {
                return;
            }

            // 创建通知面板
            if (notificationPanel == null || notificationPanel.IsDisposed)
            {
                notificationPanel = new NotificationPanel(this);
                notificationPanel.NotificationClicked += (s, e) =>
                {
                    MarkAsRead(e.Notification.Id);
                    OnNotificationClicked(e);
                };
                notificationPanel.NotificationDeleted += (s, e) =>
                {
                    DeleteNotification(e.Notification.Id);
                };
                notificationPanel.MarkAllAsReadClicked += (s, e) =>
                {
                    MarkAllAsRead();
                };
                notificationPanel.PanelClosed += (s, e) =>
                {
                    HidePanel();
                };
            }

            // 计算显示位置
            Point location = CalculatePanelLocation();
            notificationPanel.Location = location;

            // 显示面板
            notificationPanel.Show();
            notificationPanel.BringToFront();

            isPanelVisible = true;
            OnPanelOpened();
        }

        /// <summary>
        /// 隐藏通知面板
        /// </summary>
        public void HidePanel()
        {
            if (!isPanelVisible)
            {
                return;
            }

            if (notificationPanel != null && !notificationPanel.IsDisposed)
            {
                notificationPanel.Hide();
            }

            isPanelVisible = false;
            OnPanelClosed();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 计算控件所需的尺寸(包含角标)
        /// </summary>
        private Size CalculateRequiredSize()
        {
            int width = iconSize;
            int height = iconSize;

            if (showBadge)
            {
                switch (badgePosition)
                {
                    case BadgePosition.TopRight:
                    case BadgePosition.BottomRight:
                        width = iconSize + badgeExtraSpace;
                        break;
                    case BadgePosition.TopLeft:
                    case BadgePosition.BottomLeft:
                        width = iconSize + badgeExtraSpace;
                        break;
                }

                switch (badgePosition)
                {
                    case BadgePosition.TopRight:
                    case BadgePosition.TopLeft:
                        height = iconSize + badgeExtraSpace;
                        break;
                    case BadgePosition.BottomRight:
                    case BadgePosition.BottomLeft:
                        height = iconSize + badgeExtraSpace;
                        break;
                }
            }

            return new Size(width, height);
        }

        /// <summary>
        /// 获取图标绘制区域
        /// </summary>
        private Rectangle GetIconRectangle()
        {
            int offsetX = 0;
            int offsetY = 0;

            if (showBadge)
            {
                switch (badgePosition)
                {
                    case BadgePosition.TopLeft:
                        offsetX = badgeExtraSpace;
                        offsetY = badgeExtraSpace;
                        break;
                    case BadgePosition.TopRight:
                        offsetY = badgeExtraSpace;
                        break;
                    case BadgePosition.BottomLeft:
                        offsetX = badgeExtraSpace;
                        break;
                    case BadgePosition.BottomRight:
                        // 不需要偏移
                        break;
                }
            }

            return new Rectangle(offsetX, offsetY, iconSize, iconSize);
        }

        private void UpdateBadgeCount()
        {
            badgeCount = UnreadCount;
            Invalidate();
        }

        private void UpdateTooltip()
        {
            int unreadCount = UnreadCount;
            int importantCount = UnreadImportantCount;

            //if (unreadCount == 0)
            //{
            //    ToolTipText = "无未读通知";
            //}
            //else if (importantCount > 0)
            //{
            //    ToolTipText = $"未读通知: {unreadCount} (重要: {importantCount})";
            //}
            //else
            //{
            //    ToolTipText = $"未读通知: {unreadCount}";
            //}
        }

        private Point CalculatePanelLocation()
        {
            // 获取控件在屏幕上的位置
            Point screenLocation = PointToScreen(Point.Empty);

            // 默认显示在控件下方
            int x = screenLocation.X - (notificationPanel.Width - Width) / 2;
            int y = screenLocation.Y + Height + 5;

            // 检查是否超出屏幕
            Rectangle screenBounds = Screen.FromControl(this).WorkingArea;

            // 调整X坐标
            if (x + notificationPanel.Width > screenBounds.Right)
            {
                x = screenBounds.Right - notificationPanel.Width - 10;
            }
            if (x < screenBounds.Left)
            {
                x = screenBounds.Left + 10;
            }

            // 调整Y坐标(如果下方空间不够, 显示在上方)
            if (y + notificationPanel.Height > screenBounds.Bottom)
            {
                y = screenLocation.Y - notificationPanel.Height - 5;
            }

            return new Point(x, y);
        }

        private Image CreateDefaultBellIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var iconColor = GetThemeColor(c => c.TextPrimary, Color.FromArgb(100, 100, 100));

                using (var brush = new SolidBrush(iconColor))
                using (var pen = new Pen(iconColor, 1.5f))
                {
                    // 绘制铃铛
                    g.DrawArc(pen, 6, 5, 12, 12, 200, 140);
                    g.DrawLine(pen, 8, 16, 16, 16);

                    // 铃铛顶部
                    g.DrawLine(pen, 12, 5, 12, 2);
                    g.FillEllipse(brush, 10, 1, 4, 3);

                    // 铃铛底部
                    g.DrawArc(pen, 10, 16, 4, 4, 0, 180);
                }
            }
            return bitmap;
        }

        #endregion

        #region 主题

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
        }

        protected override void InitializeDefaultStyles()
        {
            base.InitializeDefaultStyles();
            BackColor = Color.Transparent;
            ForeColor = Color.Black;
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

            BadgeBackColor = GetThemeColor(c => c.Error, BadgeBackColor);
            BadgeForeColor = GetThemeColor(c => c.TextOnPrimary, BadgeForeColor);
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                TogglePanel();
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 背景透明或使用主题色
        }

        protected override void DrawContent(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制图标(使用计算的区域)
            if (icon != null)
            {
                var iconRect = GetIconRectangle();
                g.DrawImage(Icon, iconRect);
            }

            // 绘制角标
            if (showBadge && badgeCount > 0)
            {
                DrawBadge(g);
            }
        }

        private void DrawBadge(Graphics g)
        {
            string badgeText = badgeCount > maxBadgeCount ? $"{maxBadgeCount}+" : badgeCount.ToString();

            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                var textSize = g.MeasureString(badgeText, font);
                int badgeWidth = Math.Max(16, (int)textSize.Width + 6);
                int badgeHeight = 16;

                // 计算角标位置(相对于控件边界)
                Point badgeLocation = new Point(Width - badgeWidth, 0);
                switch (badgePosition)
                {
                    case BadgePosition.TopRight:
                        badgeLocation = new Point(Width - badgeWidth, 0);
                        break;
                    case BadgePosition.TopLeft:
                        badgeLocation = new Point(0, 0);
                        break;
                    case BadgePosition.BottomRight:
                        badgeLocation = new Point(Width - badgeWidth, Height - badgeHeight);
                        break;
                    case BadgePosition.BottomLeft:
                        badgeLocation = new Point(0, Height - badgeHeight);
                        break;
                }

                var badgeRect = new Rectangle(badgeLocation, new Size(badgeWidth, badgeHeight));

                // 绘制角标背景
                using (var path = GetRoundedRectangle(badgeRect, 8))
                using (var brush = new SolidBrush(badgeBackColor))
                {
                    g.FillPath(brush, path);
                }

                // 绘制角标文字
                using (var brush = new SolidBrush(badgeForeColor))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(badgeText, font, brush, badgeRect, sf);
                }
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            // 无边框或根据需要绘制
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notificationPanel?.Dispose();
                //icon?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 通知项

    /// <summary>
    /// 通知项
    /// </summary>
    public class NotificationItem
    {
        public NotificationItem()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now;
            IsRead = false;
            IsImportant = false;
        }

        public NotificationItem(string title, string content, MessageType type = MessageType.Info)
            : this()
        {
            Title = title;
            Content = content;
            Type = type;
        }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 是否重要
        /// </summary>
        public bool IsImportant { get; set; }

        /// <summary>
        /// 自定义图标
        /// </summary>
        public Image Icon { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 相关的消息选项(如果来自FluentMessage)
        /// </summary>
        public MessageOptions SourceMessageOptions { get; set; }
    }

    /// <summary>
    /// 通知过滤器
    /// </summary>
    [Serializable]
    public class NotificationFilter
    {
        /// <summary>
        /// 是否应该添加到通知列表
        /// </summary>
        [field: NonSerialized]
        public Func<MessageOptions, bool> ShouldNotify { get; set; }

        /// <summary>
        /// 是否应该标记为重要通知
        /// </summary>
        [field: NonSerialized]
        public Func<MessageOptions, bool> IsImportant { get; set; }

        /// <summary>
        /// 从模式创建过滤器
        /// </summary>
        public static NotificationFilter FromMode(NotificationFilterMode mode)
        {
            switch (mode)
            {
                case NotificationFilterMode.All:
                    return All;

                case NotificationFilterMode.WarningAndError:
                    return WarningAndError;

                case NotificationFilterMode.ErrorOnly:
                    return ErrorOnly;

                case NotificationFilterMode.None:
                    return None;

                case NotificationFilterMode.Custom:
                    return new NotificationFilter(); // 空过滤器, 需要手动设置

                default:
                    return WarningAndError;
            }
        }

        /// <summary>
        /// 默认过滤器
        /// </summary>
        public static NotificationFilter Default => WarningAndError;

        /// <summary>
        /// 警告和错误
        /// </summary>
        public static NotificationFilter WarningAndError => new NotificationFilter
        {
            ShouldNotify = options => options.Type == MessageType.Warning || options.Type == MessageType.Error,
            IsImportant = options => options.Type == MessageType.Error
        };

        /// <summary>
        /// 全部通知
        /// </summary>
        public static NotificationFilter All => new NotificationFilter
        {
            ShouldNotify = options => true,
            IsImportant = options => options.Type == MessageType.Error
        };

        /// <summary>
        /// 仅错误
        /// </summary>
        public static NotificationFilter ErrorOnly => new NotificationFilter
        {
            ShouldNotify = options => options.Type == MessageType.Error,
            IsImportant = options => true
        };

        /// <summary>
        /// 不通知(禁用)
        /// </summary>
        public static NotificationFilter None => new NotificationFilter
        {
            ShouldNotify = options => false,
            IsImportant = options => false
        };
    }

    #endregion

    #region 浮动通知面板

    internal class NotificationPanel : FluentForm
    {
        #region 字段

        private FluentMessageNotifier owner;
        private Panel titlePanel;
        private Label titleLabel;
        private PictureBox closeButton;
        private PictureBox markAllReadButton;
        private FluentTabControl tabControl;
        private FluentTabPage allTab;
        private FluentTabPage importantTab;
        private DoubleBufferedPanel allNotificationsPanel;
        private DoubleBufferedPanel importantNotificationsPanel;

        private bool isDraggable = true;
        private bool isDragging = false;
        private Point dragStartPoint;

        // 事件
        public event EventHandler<NotificationEventArgs> NotificationClicked;
        public event EventHandler<NotificationEventArgs> NotificationDeleted;
        public event EventHandler MarkAllAsReadClicked;
        public event EventHandler PanelClosed;

        #endregion

        #region 构造函数

        public NotificationPanel(FluentMessageNotifier owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));

            InitializeComponent();
            ApplyTheme();
            RefreshNotifications();
        }

        private void InitializeComponent()
        {
            // 窗体设置
            ShowTitleBar = false;
            base.CanResize = false;
            CornerRadius = 0;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;

            Size = new Size(380, 500);
            BackColor = Color.White;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            DoubleBuffered = true;

            // 标题栏
            titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            titleLabel = new Label
            {
                Text = "通知",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 13),
                AutoSize = true,
                ForeColor = Color.FromArgb(33, 33, 33)
            };

            // 全部已读按钮
            markAllReadButton = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(Width - 80, 13),
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = CreateMarkAllReadIcon()
            };
            markAllReadButton.Click += (s, e) => MarkAllAsReadClicked?.Invoke(this, EventArgs.Empty);

            var markAllReadTooltip = new ToolTip();
            markAllReadTooltip.SetToolTip(markAllReadButton, "全部标记为已读");

            // 关闭按钮
            closeButton = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(Width - 40, 13),
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = CreateCloseIcon()
            };
            closeButton.Click += (s, e) => Close();

            var closeTooltip = new ToolTip();
            closeTooltip.SetToolTip(closeButton, "关闭");

            titlePanel.Controls.AddRange(new Control[] { titleLabel, markAllReadButton, closeButton });

            // 拖动支持
            if (isDraggable)
            {
                titlePanel.MouseDown += TitlePanel_MouseDown;
                titlePanel.MouseMove += TitlePanel_MouseMove;
                titlePanel.MouseUp += TitlePanel_MouseUp;
                titlePanel.Cursor = Cursors.SizeAll;
            }

            // 选项卡
            tabControl = new FluentTabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                TabPadding = new Padding(5),
                TabMinWidth = 60
            };

            allTab = new FluentTabPage("全部");
            allTab.ShowCloseButton = false;
            importantTab = new FluentTabPage("重要");
            importantTab.ShowCloseButton = false;

            // 全部通知面板
            allNotificationsPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(5)
            };

            // 重要通知面板
            importantNotificationsPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(5)
            };

            allTab.Controls.Add(allNotificationsPanel);
            importantTab.Controls.Add(importantNotificationsPanel);

            tabControl.TabPages.AddRange(new[] { allTab, importantTab });

            Controls.Add(tabControl);
            Controls.Add(titlePanel);

            // 失去焦点时关闭
            Deactivate += (s, e) =>
            {
                // 延迟一点再关闭, 避免点击通知项时立即关闭
                BeginInvoke(new Action(() =>
                {
                    if (!ContainsFocus && !owner.ContainsFocus)
                    {
                        Close();
                    }
                }));
            };
        }

        #endregion

        #region 拖动支持

        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDraggable)
            {
                isDragging = true;
                dragStartPoint = e.Location;
                titlePanel.Cursor = Cursors.SizeAll;
            }
        }

        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = Location;
                newLocation.X += e.X - dragStartPoint.X;
                newLocation.Y += e.Y - dragStartPoint.Y;
                Location = newLocation;
            }
        }

        private void TitlePanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            titlePanel.Cursor = Cursors.SizeAll;
        }

        /// <summary>
        /// 是否允许拖动
        /// </summary>
        public bool IsDraggable
        {
            get => isDraggable;
            set
            {
                isDraggable = value;
                titlePanel.Cursor = value ? Cursors.SizeAll : Cursors.Default;
            }
        }

        #endregion

        #region 刷新通知列表

        public void RefreshNotifications()
        {
            RefreshAllNotifications();
            RefreshImportantNotifications();
            UpdateTabTitles();
        }

        private void RefreshAllNotifications()
        {
            allNotificationsPanel.SuspendLayout();
            allNotificationsPanel.Controls.Clear();

            // 获取所有未读通知(按时间倒序)
            var unreadNotifications = owner.Notifications
                .Where(n => !n.IsRead)
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            if (unreadNotifications.Count == 0)
            {
                // 显示空状态
                var emptyLabel = new Label
                {
                    Text = "暂无未读通知",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(
                        (allNotificationsPanel.Width - 120) / 2,
                        (allNotificationsPanel.Height - 20) / 2)
                };
                allNotificationsPanel.Controls.Add(emptyLabel);
            }
            else
            {
                int top = 5;
                foreach (var notification in unreadNotifications)
                {
                    var item = new NotificationItemControl(notification, false);
                    item.Width = allNotificationsPanel.Width - 25;
                    item.Top = top;
                    item.Left = 5;
                    item.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                    item.Click += (s, e) =>
                    {
                        NotificationClicked?.Invoke(this, new NotificationEventArgs(notification));
                    };

                    allNotificationsPanel.Controls.Add(item);
                    top += item.Height + 5;
                }
            }

            allNotificationsPanel.ResumeLayout();
        }

        private void RefreshImportantNotifications()
        {
            importantNotificationsPanel.SuspendLayout();
            importantNotificationsPanel.Controls.Clear();

            // 获取所有重要通知(按时间倒序)
            var importantNotifications = owner.Notifications
                .Where(n => n.IsImportant)
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            if (importantNotifications.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无重要通知",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(
                        (importantNotificationsPanel.Width - 120) / 2,
                        (importantNotificationsPanel.Height - 20) / 2)
                };
                importantNotificationsPanel.Controls.Add(emptyLabel);
            }
            else
            {
                int top = 5;
                foreach (var notification in importantNotifications)
                {
                    var item = new NotificationItemControl(notification, true);
                    item.Width = importantNotificationsPanel.Width - 25;
                    item.Top = top;
                    item.Left = 5;
                    item.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                    item.Click += (s, e) =>
                    {
                        NotificationClicked?.Invoke(this, new NotificationEventArgs(notification));
                    };

                    // 重要通知可以删除
                    item.DeleteClicked += (s, e) =>
                    {
                        NotificationDeleted?.Invoke(this, new NotificationEventArgs(notification));
                    };

                    importantNotificationsPanel.Controls.Add(item);
                    top += item.Height + 5;
                }
            }

            importantNotificationsPanel.ResumeLayout();
        }

        private void UpdateTabTitles()
        {
            int unreadCount = owner.UnreadCount;
            int importantCount = owner.Notifications.Count(n => n.IsImportant);

            allTab.Text = unreadCount > 0 ? $"全部 ({unreadCount})" : "全部";
            importantTab.Text = importantCount > 0 ? $"重要 ({importantCount})" : "重要";
        }

        #endregion

        #region 主题

        private void ApplyTheme()
        {
            if (owner.UseTheme && owner.Theme != null)
            {
                BackColor = owner.Theme.Colors.Surface;
                titlePanel.BackColor = owner.Theme.Colors.BackgroundSecondary;
                titleLabel.ForeColor = owner.Theme.Colors.TextPrimary;
            }
        }

        #endregion

        #region 图标创建

        private Image CreateCloseIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(100, 100, 100), 2))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 4, 4, 12, 12);
                    g.DrawLine(pen, 12, 4, 4, 12);
                }
            }
            return bitmap;
        }

        private Image CreateMarkAllReadIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(100, 100, 100), 2))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    // 双勾
                    g.DrawLines(pen, new Point[] { new Point(2, 8), new Point(5, 11), new Point(9, 7) });
                    g.DrawLines(pen, new Point[] { new Point(7, 5), new Point(10, 8), new Point(14, 4) });
                }
            }
            return bitmap;
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制边框和阴影
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        #endregion

        #region 重写

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // 显示动画
            if (owner.EnableAnimation)
            {
                AnimateShow();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            PanelClosed?.Invoke(this, EventArgs.Empty);
        }

        private void AnimateShow()
        {
            if (IsHandleCreated && !IsDisposed)
            {
                Opacity = 0;
                var timer = new Timer { Interval = 16 };
                timer.Tick += (s, e) =>
                {
                    Opacity += 0.1;
                    if (Opacity >= 1.0)
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                };
                timer.Start();
            }

        }

        #endregion
    }


    #endregion

    #region 通知项控件

    /// <summary>
    /// 通知项控件
    /// </summary>
    internal class NotificationItemControl : Panel
    {
        private NotificationItem notification;
        private bool showInImportantTab;

        private PictureBox iconBox;
        private Label titleLabel;
        private Label contentLabel;
        private Label timeLabel;
        private Label statusLabel;
        private PictureBox deleteButton;

        public event EventHandler DeleteClicked;

        public NotificationItemControl(NotificationItem notification, bool showInImportantTab)
        {
            this.notification = notification ?? throw new ArgumentNullException(nameof(notification));
            this.showInImportantTab = showInImportantTab;

            InitializeComponent();
            ApplyNotificationData();
        }

        private void InitializeComponent()
        {
            Height = 85;
            Cursor = Cursors.Hand;
            Margin = new Padding(5);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            DoubleBuffered = true;

            // 图标
            iconBox = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            iconBox.MouseDown += ProxyMouseDown;
            iconBox.MouseUp += ProxyMouseUp;
            iconBox.Click += ProxyClick;

            // 标题
            titleLabel = new Label
            {
                Location = new Point(50, 8),
                AutoSize = false,
                Size = new Size(Width - 100, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            titleLabel.MouseDown += ProxyMouseDown;
            titleLabel.MouseUp += ProxyMouseUp;
            titleLabel.Click += ProxyClick;

            // 内容
            contentLabel = new Label
            {
                Location = new Point(50, 30),
                AutoSize = false,
                Size = new Size(Width - 100, 35),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            contentLabel.MouseDown += ProxyMouseDown;
            contentLabel.MouseUp += ProxyMouseUp;
            contentLabel.Click += ProxyClick;

            // 时间
            timeLabel = new Label
            {
                Location = new Point(50, Height - 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f),
                BackColor = Color.Transparent,
                ForeColor = Color.Gray
            };
            timeLabel.MouseDown += ProxyMouseDown;
            timeLabel.MouseUp += ProxyMouseUp;
            timeLabel.Click += ProxyClick;

            // 状态标签(已读/未读)
            statusLabel = new Label
            {
                Location = new Point(Width - 50, 8),
                Size = new Size(40, 18),
                Font = new Font("Segoe UI", 7, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            statusLabel.MouseDown += ProxyMouseDown;
            statusLabel.MouseUp += ProxyMouseUp;
            statusLabel.Click += ProxyClick;

            Controls.AddRange(new Control[] { iconBox, titleLabel, contentLabel, timeLabel, statusLabel });

            // 如果在重要标签页中, 显示删除按钮
            if (showInImportantTab)
            {
                deleteButton = new PictureBox
                {
                    Size = new Size(16, 16),
                    Location = new Point(Width - 25, Height - 25),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent,
                    Image = CreateDeleteIcon(),
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom
                };
                deleteButton.Click += (s, e) =>
                {
                    DeleteClicked?.Invoke(this, EventArgs.Empty);
                };

                var tooltip = new ToolTip();
                tooltip.SetToolTip(deleteButton, "删除");

                Controls.Add(deleteButton);
            }

            // 鼠标效果
            MouseEnter += (s, e) => BackColor = Color.FromArgb(245, 245, 245);
            MouseLeave += (s, e) => UpdateBackColor();
        }

        #region 将子控件的鼠标事件传递给父控件

        private void ProxyMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void ProxyMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void ProxyClick(object sender, EventArgs e)
        {
            OnClick(e);
        }

        #endregion

        private void ApplyNotificationData()
        {
            // 图标
            if (notification.Icon != null)
            {
                iconBox.Image = notification.Icon;
            }
            else
            {
                iconBox.Image = CreateDefaultIcon(notification.Type);
            }

            // 标题
            titleLabel.Text = string.IsNullOrWhiteSpace(notification.Title)
                ? GetDefaultTitle(notification.Type)
                : notification.Title;

            // 内容
            contentLabel.Text = notification.Content;

            // 时间
            timeLabel.Text = FormatTime(notification.Timestamp);

            // 状态
            if (showInImportantTab)
            {
                if (notification.IsRead)
                {
                    statusLabel.Text = "已读";
                    statusLabel.ForeColor = Color.Gray;
                }
                else
                {
                    statusLabel.Text = "未读";
                    statusLabel.ForeColor = Color.FromArgb(244, 67, 54);
                }
            }

            UpdateBackColor();
        }

        private void UpdateBackColor()
        {
            if (notification.IsRead)
            {
                BackColor = Color.White;
            }
            else
            {
                // 未读消息浅色背景
                switch (notification.Type)
                {
                    case MessageType.Error:
                        BackColor = Color.FromArgb(255, 245, 245);
                        break;
                    case MessageType.Warning:
                        BackColor = Color.FromArgb(255, 250, 240);
                        break;
                    case MessageType.Success:
                        BackColor = Color.FromArgb(245, 255, 245);
                        break;
                    default:
                        BackColor = Color.FromArgb(240, 248, 255);
                        break;
                }
            }
        }

        private string GetDefaultTitle(MessageType type)
        {

            switch (type)
            {
                case MessageType.Success:
                    return "成功";
                case MessageType.Warning:
                    return "警告";
                case MessageType.Error:
                    return "错误";
                case MessageType.Info:
                    return "信息";
                default:
                    return "通知";
            }
        }

        private string FormatTime(DateTime time)
        {
            var span = DateTime.Now - time;

            if (span.TotalMinutes < 1)
            {
                return "刚刚";
            }

            if (span.TotalMinutes < 60)
            {
                return $"{(int)span.TotalMinutes}分钟前";
            }

            if (span.TotalHours < 24)
            {
                return $"{(int)span.TotalHours}小时前";
            }

            if (span.TotalDays < 7)
            {
                return $"{(int)span.TotalDays}天前";
            }

            return time.ToString("MM-dd HH:mm");
        }

        private Image CreateDefaultIcon(MessageType type)
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Color iconColor;
                switch (type)
                {
                    case MessageType.Success:
                        iconColor = Color.FromArgb(76, 175, 80);
                        break;
                    case MessageType.Warning:
                        iconColor = Color.FromArgb(255, 152, 0);
                        break;
                    case MessageType.Error:
                        iconColor = Color.FromArgb(244, 67, 54);
                        break;
                    default:
                        iconColor = Color.FromArgb(33, 150, 243);
                        break;
                }

                using (var brush = new SolidBrush(iconColor))
                {
                    g.FillEllipse(brush, 4, 4, 24, 24);
                }

                using (var pen = new Pen(Color.White, 2.5f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    switch (type)
                    {
                        case MessageType.Success:
                            g.DrawLines(pen, new Point[] {
                            new Point(10, 16),
                            new Point(14, 20),
                            new Point(22, 12)
                        });
                            break;
                        case MessageType.Warning:
                            g.DrawLine(pen, 16, 10, 16, 18);
                            using (var dotBrush = new SolidBrush(Color.White))
                            {
                                g.FillEllipse(dotBrush, 14, 21, 4, 4);
                            }
                            break;
                        case MessageType.Error:
                            g.DrawLine(pen, 11, 11, 21, 21);
                            g.DrawLine(pen, 21, 11, 11, 21);
                            break;
                        case MessageType.Info:
                            using (var dotBrush = new SolidBrush(Color.White))
                            {
                                g.FillEllipse(dotBrush, 14, 9, 4, 4);
                            }
                            g.DrawLine(pen, 16, 15, 16, 23);
                            break;
                    }
                }
            }
            return bitmap;
        }

        private Image CreateDeleteIcon()
        {
            var bitmap = new Bitmap(12, 12);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(150, 150, 150), 1.5f))
                {
                    g.DrawLine(pen, 3, 3, 9, 9);
                    g.DrawLine(pen, 9, 3, 3, 9);
                }
            }
            return bitmap;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            // 绘制背景
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 绘制圆角边框
            using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = GetRoundedRectangle(rect, 6))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
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
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 角标位置
    /// </summary>
    public enum BadgePosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    /// <summary>
    /// 通知过滤模式
    /// </summary>
    public enum NotificationFilterMode
    {

        [Description("全部通知")]
        All,                            // 不过滤, 所有消息都添加到通知

        [Description("警告和错误")]
        WarningAndError,                // 仅警告和错误

        [Description("仅错误")]
        ErrorOnly,                      // 仅错误

        [Description("禁用")]
        None,                           // 不添加通知

        [Description("自定义")]
        Custom                          // 自定义(需要在设置Filter)
    }

    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(NotificationItem notification)
        {
            Notification = notification;
        }

        public NotificationItem Notification { get; }
    }

    #endregion

    #region 设计时支持

    public class FluentMessageNotifierDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentMessageNotifierActionList(Component));
                }
                return actionLists;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 移除可能导致序列化问题的属性
            properties.Remove("Filter");
        }
    }

    public class FluentMessageNotifierActionList : DesignerActionList
    {
        private FluentMessageNotifier control;

        public FluentMessageNotifierActionList(IComponent component) : base(component)
        {
            control = component as FluentMessageNotifier;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowBadge", "显示角标", "外观"));
            items.Add(new DesignerActionPropertyItem("BadgePosition", "角标位置", "外观"));

            // 过滤
            items.Add(new DesignerActionHeaderItem("过滤"));
            items.Add(new DesignerActionPropertyItem("FilterMode", "过滤模式", "过滤", "选择通知过滤模式"));

            // 操作
            items.Add(new DesignerActionHeaderItem("操作"));
            items.Add(new DesignerActionMethodItem(this, "TestNotification", "测试通知", "操作"));
            items.Add(new DesignerActionMethodItem(this, "ClearAll", "清除所有", "操作"));

            return items;
        }

        #region 属性

        public bool ShowBadge
        {
            get => control.ShowBadge;
            set => SetProperty("ShowBadge", value);
        }

        public BadgePosition BadgePosition
        {
            get => control.BadgePosition;
            set => SetProperty("BadgePosition", value);
        }

        public NotificationFilterMode FilterMode
        {
            get => control.FilterMode;
            set => SetProperty("FilterMode", value);
        }

        #endregion

        #region 方法

        private void TestNotification()
        {
            control.AddNotification(new NotificationItem
            {
                Title = "测试通知",
                Content = "这是一条测试通知消息",
                Type = MessageType.Info
            });
        }

        private void ClearAll()
        {
            control.ClearAll();
        }

        #endregion

        private void SetProperty(string propertyName, object value)
        {
            var property = TypeDescriptor.GetProperties(control)[propertyName];
            if (property != null)
            {
                var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                changeService?.OnComponentChanging(control, property);
                property.SetValue(control, value);
                changeService?.OnComponentChanged(control, property, null, null);
            }
        }
    }

    #endregion

    #region 管理类

    /// <summary>
    /// 消息通知管理器
    /// </summary>
    public class FluentMessageNotifierManager
    {
        private FluentMessageNotifier notifier;

        public FluentMessageNotifierManager(FluentMessageNotifier notifier)
        {
            this.notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        /// <summary>
        /// 显示消息并根据过滤器添加到通知
        /// </summary>
        public FluentMessage Show(MessageOptions options)
        {
            // 每次都从 notifier 获取最新的过滤器
            var currentFilter = notifier.Filter;

            if (currentFilter != null && currentFilter.ShouldNotify != null && currentFilter.ShouldNotify(options))
            {
                notifier.AddNotificationFromMessage(options);
            }

            // 然后显示消息
            return FluentMessageManager.Instance.Show(options);
        }

        public FluentMessage Success(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 3000, MessageDisplayMode displayMode = MessageDisplayMode.Application)
        {
            var options = new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Success,
                Duration = duration,
                Position = position,
                DisplayMode = displayMode
            };
            return Show(options);
        }

        public FluentMessage Warning(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 4000, MessageDisplayMode displayMode = MessageDisplayMode.Application)
        {
            var options = new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Warning,
                Duration = duration,
                Position = position,
                DisplayMode = displayMode
            };
            return Show(options);
        }

        public FluentMessage Error(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 4000, MessageDisplayMode displayMode = MessageDisplayMode.Application)
        {
            var options = new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Error,
                Duration = duration,
                Position = position,
                DisplayMode = displayMode
            };
            return Show(options);
        }

        public FluentMessage Info(string content, string title = null, MessagePosition position = MessagePosition.BottomRight, int duration = 3000, MessageDisplayMode displayMode = MessageDisplayMode.Application)
        {
            var options = new MessageOptions
            {
                Title = title,
                Content = content,
                Type = MessageType.Info,
                Duration = duration,
                Position = position,
                DisplayMode = displayMode
            };
            return Show(options);
        }
    }

    #endregion
}
