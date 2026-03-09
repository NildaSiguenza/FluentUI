using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    [ToolboxBitmap(typeof(ToolTip))]
    [ProvideProperty("TooltipText", typeof(Control))]
    [ProvideProperty("TooltipTitle", typeof(Control))]
    [ProvideProperty("TooltipIcon", typeof(Control))]
    [Description("Fluent 风格的工具提示组件")]
    public class FluentTooltip : Component, IExtenderProvider
    {
        #region 字段

        private readonly Dictionary<Control, TooltipData> tooltipDataMap = new Dictionary<Control, TooltipData>();
        private FluentTooltipWindow tooltipWindow;
        private Control currentControl;
        private Timer showTimer;
        private Timer hideTimer;
        private Point lastMousePosition;
        private bool isDesignMode;

        // 默认样式
        private int showDelay = 500;
        private int hideDelay = 5000;
        private int fadeInDuration = 150;
        private int fadeOutDuration = 100;
        private Color backgroundColor = Color.WhiteSmoke;
        private Color foreColor = Color.DimGray;
        private Color titleColor = Color.Black;
        private Color titleBackgroundColor = Color.FromArgb(210, 210, 210);
        private Padding titlePadding = new Padding(0, 2, 0, 2);
        private Color borderColor = Color.Gray;
        private int borderRadius = 6;
        private int maxWidth = 350;
        private Padding contentPadding = new Padding(12, 10, 12, 10);
        private Font titleFont;
        private Font contentFont;
        private TooltipPosition preferredPosition = TooltipPosition.Bottom;
        private int offsetFromControl = 8;
        private bool showShadow = true;
        private bool isActive = true;

        #endregion        

        #region 构造函数

        public FluentTooltip()
        {
            Initialize();
        }

        public FluentTooltip(IContainer container) : this()
        {
            container?.Add(this);
        }

        private void Initialize()
        {
            titleFont = new Font("Segoe UI Semibold", 9F);
            contentFont = new Font("Segoe UI", 8.5F);

            // 紧凑的内边距
            contentPadding = new Padding(8, 6, 8, 6);
            titlePadding = new Padding(0, 2, 0, 2);

            // 较小的圆角
            borderRadius = 4;

            // 较小的最大宽度
            maxWidth = 300;

            // 较小的偏移
            offsetFromControl = 6;

            showTimer = new Timer { Interval = showDelay };
            showTimer.Tick += ShowTimer_Tick;

            hideTimer = new Timer { Interval = hideDelay };
            hideTimer.Tick += HideTimer_Tick;

            // 检测是否在设计模式
            isDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (!isDesignMode)
            {
                tooltipWindow = new FluentTooltipWindow(this);
            }
        }

        #endregion

        #region 属性

        /// <summary>
        /// 是否激活工具提示
        /// </summary>
        [Category("Behavior")]
        [Description("是否激活工具提示功能")]
        [DefaultValue(true)]
        public bool Active
        {
            get => isActive;
            set => isActive = value;
        }

        /// <summary>
        /// 显示延迟(毫秒)
        /// </summary>
        [Category("Behavior")]
        [Description("鼠标悬停后显示提示的延迟时间")]
        [DefaultValue(500)]
        public int ShowDelay
        {
            get => showDelay;
            set
            {
                showDelay = Math.Max(0, value);
                if (showTimer != null)
                {
                    showTimer.Interval = Math.Max(1, showDelay);
                }
            }
        }

        /// <summary>
        /// 自动隐藏延迟(毫秒)
        /// </summary>
        [Category("Behavior")]
        [Description("提示显示后自动隐藏的延迟时间, 0表示不自动隐藏")]
        [DefaultValue(5000)]
        public int AutoPopDelay
        {
            get => hideDelay;
            set
            {
                hideDelay = Math.Max(0, value);
                if (hideDelay > 0 && hideTimer != null)
                {
                    hideTimer.Interval = hideDelay;
                }
            }
        }

        /// <summary>
        /// 淡入动画时长
        /// </summary>
        [Category("Animation")]
        [Description("淡入动画持续时间")]
        [DefaultValue(150)]
        public int FadeInDuration
        {
            get => fadeInDuration;
            set => fadeInDuration = Math.Max(0, value);
        }

        /// <summary>
        /// 淡出动画时长
        /// </summary>
        [Category("Animation")]
        [Description("淡出动画持续时间")]
        [DefaultValue(100)]
        public int FadeOutDuration
        {
            get => fadeOutDuration;
            set => fadeOutDuration = Math.Max(0, value);
        }

        /// <summary>
        /// 背景颜色
        /// </summary>
        [Category("Appearance")]
        [Description("提示框背景颜色")]
        public Color BackgroundColor
        {
            get => backgroundColor;
            set => backgroundColor = value;
        }

        /// <summary>
        /// 内容文本颜色
        /// </summary>
        [Category("Appearance")]
        [Description("内容文本颜色")]
        public Color ForeColor
        {
            get => foreColor;
            set => foreColor = value;
        }

        /// <summary>
        /// 标题颜色
        /// </summary>
        [Category("Appearance")]
        [Description("标题文本颜色")]
        public Color TitleColor
        {
            get => titleColor;
            set => titleColor = value;
        }

        /// <summary>
        /// 标题背景颜色
        /// </summary>
        [Category("Appearance")]
        [Description("标题区域的背景颜色")]
        public Color TitleBackgroundColor
        {
            get => titleBackgroundColor;
            set => titleBackgroundColor = value;
        }

        /// <summary>
        /// 标题内边距
        /// </summary>
        [Category("Layout")]
        [Description("标题区域的内边距")]
        public Padding TitlePadding
        {
            get => titlePadding;
            set => titlePadding = value;
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        [Category("Appearance")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set => borderColor = value;
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        [Category("Appearance")]
        [Description("圆角半径")]
        [DefaultValue(6)]
        public int BorderRadius
        {
            get => borderRadius;
            set => borderRadius = Math.Max(0, value);
        }

        /// <summary>
        /// 最大宽度
        /// </summary>
        [Category("Layout")]
        [Description("提示框最大宽度")]
        [DefaultValue(350)]
        public int MaxWidth
        {
            get => maxWidth;
            set => maxWidth = Math.Max(100, value);
        }

        /// <summary>
        /// 内容内边距
        /// </summary>
        [Category("Layout")]
        [Description("内容区域的内边距")]
        public Padding ContentPadding
        {
            get => contentPadding;
            set => contentPadding = value;
        }

        /// <summary>
        /// 标题字体
        /// </summary>
        [Category("Appearance")]
        [Description("标题文本的字体")]
        public Font TitleFont
        {
            get => titleFont;
            set => titleFont = value ?? new Font("Segoe UI Semibold", 10F);
        }

        /// <summary>
        /// 内容字体
        /// </summary>
        [Category("Appearance")]
        [Description("内容文本的字体")]
        public Font ContentFont
        {
            get => contentFont;
            set => contentFont = value ?? new Font("Segoe UI", 9F);
        }

        /// <summary>
        /// 首选显示位置
        /// </summary>
        [Category("Layout")]
        [Description("提示框的首选显示位置")]
        [DefaultValue(TooltipPosition.Bottom)]
        public TooltipPosition Position
        {
            get => preferredPosition;
            set => preferredPosition = value;
        }

        /// <summary>
        /// 距离控件的偏移量
        /// </summary>
        [Category("Layout")]
        [Description("提示框距离目标控件的偏移量")]
        [DefaultValue(8)]
        public int OffsetFromControl
        {
            get => offsetFromControl;
            set => offsetFromControl = Math.Max(0, value);
        }

        /// <summary>
        /// 是否显示阴影
        /// </summary>
        [Category("Appearance")]
        [Description("是否显示阴影效果")]
        [DefaultValue(true)]
        public bool ShowShadow
        {
            get => showShadow;
            set => showShadow = value;
        }

        #endregion

        #region IExtenderProvider 实现

        /// <summary>
        /// 判断是否可以为指定对象提供扩展属性
        /// </summary>
        public bool CanExtend(object extendee)
        {
            // 可以扩展所有控件, 但不包括窗体本身
            return !(extendee is FluentTooltip) && extendee is Control && !(extendee is Form);
        }

        #endregion

        #region 扩展属性 - TooltipText

        /// <summary>
        /// 获取控件的提示文本
        /// </summary>
        [Category("FluentTooltip")]
        [Description("控件的工具提示文本")]
        [DefaultValue("")]
        [Localizable(true)]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string GetTooltipText(Control control)
        {
            if (control == null)
            {
                return string.Empty;
            }

            if (tooltipDataMap.TryGetValue(control, out var data))
            {
                return data.Text ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// 设置控件的提示文本
        /// </summary>
        public void SetTooltipText(Control control, string value)
        {
            if (control == null)
            {
                return;
            }

            var data = GetOrCreateTooltipData(control);
            data.Text = value ?? string.Empty;

            UpdateControlHooks(control);
        }

        #endregion

        #region 扩展属性 - TooltipTitle

        /// <summary>
        /// 获取控件的提示标题
        /// </summary>
        [Category("FluentTooltip")]
        [Description("控件的工具提示标题")]
        [DefaultValue("")]
        [Localizable(true)]
        public string GetTooltipTitle(Control control)
        {
            if (control == null)
            {
                return string.Empty;
            }

            if (tooltipDataMap.TryGetValue(control, out var data))
            {
                return data.Title ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// 设置控件的提示标题
        /// </summary>
        public void SetTooltipTitle(Control control, string value)
        {
            if (control == null)
            {
                return;
            }

            var data = GetOrCreateTooltipData(control);
            data.Title = value ?? string.Empty;

            UpdateControlHooks(control);
        }

        #endregion

        #region 扩展属性 - TooltipIcon

        /// <summary>
        /// 获取控件的提示图标
        /// </summary>
        [Category("FluentTooltip")]
        [Description("控件的工具提示图标")]
        [DefaultValue(null)]
        public Image GetTooltipIcon(Control control)
        {
            if (control == null)
            {
                return null;
            }

            if (tooltipDataMap.TryGetValue(control, out var data))
            {
                return data.Icon;
            }

            return null;
        }

        /// <summary>
        /// 设置控件的提示图标
        /// </summary>
        public void SetTooltipIcon(Control control, Image value)
        {
            if (control == null)
            {
                return;
            }

            var data = GetOrCreateTooltipData(control);
            data.Icon = value;

            UpdateControlHooks(control);
        }

        #endregion

        #region 辅助方法

        private TooltipData GetOrCreateTooltipData(Control control)
        {
            if (!tooltipDataMap.TryGetValue(control, out var data))
            {
                data = new TooltipData();
                tooltipDataMap[control] = data;
            }
            return data;
        }

        private void UpdateControlHooks(Control control)
        {
            if (control == null)
            {
                return;
            }

            var data = tooltipDataMap.TryGetValue(control, out var d) ? d : null;

            if (data != null && data.HasContent)
            {
                // 确保事件只订阅一次
                control.MouseEnter -= Control_MouseEnter;
                control.MouseLeave -= Control_MouseLeave;
                control.MouseMove -= Control_MouseMove;
                control.Disposed -= Control_Disposed;

                control.MouseEnter += Control_MouseEnter;
                control.MouseLeave += Control_MouseLeave;
                control.MouseMove += Control_MouseMove;
                control.Disposed += Control_Disposed;
            }
            else
            {
                // 没有内容时移除事件
                control.MouseEnter -= Control_MouseEnter;
                control.MouseLeave -= Control_MouseLeave;
                control.MouseMove -= Control_MouseMove;
                control.Disposed -= Control_Disposed;

                if (data != null && !data.HasContent)
                {
                    tooltipDataMap.Remove(control);
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置控件的完整提示信息
        /// </summary>
        public void SetTooltip(Control control, string text)
        {
            SetTooltipText(control, text);
        }

        /// <summary>
        /// 设置控件的完整提示信息(含标题)
        /// </summary>
        public void SetTooltip(Control control, string title, string text)
        {
            SetTooltipTitle(control, title);
            SetTooltipText(control, text);
        }

        /// <summary>
        /// 设置控件的完整提示信息
        /// </summary>
        public void SetTooltip(Control control, string title, string text, Image icon)
        {
            SetTooltipTitle(control, title);
            SetTooltipText(control, text);
            SetTooltipIcon(control, icon);
        }

        /// <summary>
        /// 设置动态内容提供器
        /// </summary>
        public void SetDynamicContent(Control control, Func<TooltipInfo> contentProvider)
        {
            if (control == null)
            {
                return;
            }

            var data = GetOrCreateTooltipData(control);
            data.DynamicContentProvider = contentProvider;

            // 设置一个占位文本以确保事件被挂钩
            if (string.IsNullOrEmpty(data.Text) && string.IsNullOrEmpty(data.Title))
            {
                data.Text = " "; // 占位符
            }

            UpdateControlHooks(control);
        }

        /// <summary>
        /// 移除控件的提示
        /// </summary>
        public void RemoveTooltip(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.MouseEnter -= Control_MouseEnter;
            control.MouseLeave -= Control_MouseLeave;
            control.MouseMove -= Control_MouseMove;
            control.Disposed -= Control_Disposed;

            tooltipDataMap.Remove(control);
        }

        /// <summary>
        /// 立即显示指定控件的提示
        /// </summary>
        public void Show(Control control)
        {
            if (control == null || !isActive || isDesignMode)
            {
                return;
            }

            if (tooltipDataMap.TryGetValue(control, out var data) && data.HasContent)
            {
                currentControl = control;
                ShowTooltipForControl(control, data);
            }
        }

        /// <summary>
        /// 立即显示自定义提示
        /// </summary>
        public void Show(Control control, string text, int duration = 0)
        {
            if (control == null || !isActive || isDesignMode)
            {
                return;
            }

            var info = new TooltipInfo { Content = text };
            ShowTooltipAtControl(control, info, duration);
        }

        /// <summary>
        /// 在指定位置显示提示
        /// </summary>
        public void Show(string text, Point location, int duration = 0)
        {
            if (!isActive || isDesignMode || tooltipWindow == null)
            {
                return;
            }

            var info = new TooltipInfo { Content = text };
            tooltipWindow.SetTooltipInfo(info);
            tooltipWindow.Location = location;
            tooltipWindow.ShowTooltip(fadeInDuration);

            if (duration > 0)
            {
                hideTimer.Stop();
                hideTimer.Interval = duration;
                hideTimer.Start();
            }
        }

        /// <summary>
        /// 隐藏提示
        /// </summary>
        public void Hide()
        {
            HideTooltip();
        }

        /// <summary>
        /// 移除所有提示
        /// </summary>
        public void RemoveAll()
        {
            foreach (var control in tooltipDataMap.Keys.ToList())
            {
                RemoveTooltip(control);
            }
        }

        #endregion

        #region 事件处理

        private void Control_MouseEnter(object sender, EventArgs e)
        {
            if (!isActive || isDesignMode)
            {
                return;
            }

            if (sender is Control control && tooltipDataMap.TryGetValue(control, out var data))
            {
                currentControl = control;
                lastMousePosition = Control.MousePosition;

                showTimer.Stop();
                showTimer.Start();
            }
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            showTimer.Stop();

            // 检查鼠标是否移动到了 tooltip 窗口上
            if (tooltipWindow != null && tooltipWindow.Visible)
            {
                var mousePos = Control.MousePosition;
                if (!tooltipWindow.Bounds.Contains(mousePos))
                {
                    HideTooltip();
                }
            }
            else
            {
                currentControl = null;
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            lastMousePosition = Control.MousePosition;
        }

        private void Control_Disposed(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                RemoveTooltip(control);
            }
        }

        private void ShowTimer_Tick(object sender, EventArgs e)
        {
            showTimer.Stop();

            if (currentControl != null && tooltipDataMap.TryGetValue(currentControl, out var data))
            {
                ShowTooltipForControl(currentControl, data);
            }
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            hideTimer.Stop();
            HideTooltip();
        }

        private void ShowTooltipForControl(Control control, TooltipData data)
        {
            if (tooltipWindow == null)
            {
                return;
            }

            TooltipInfo info;

            // 优先使用动态内容
            if (data.DynamicContentProvider != null)
            {
                info = data.DynamicContentProvider();
            }
            else
            {
                info = data.ToTooltipInfo();
            }

            ShowTooltipAtControl(control, info, 0);
        }

        private void ShowTooltipAtControl(Control control, TooltipInfo info, int duration)
        {
            if (tooltipWindow == null || info == null)
            {
                return;
            }

            tooltipWindow.SetTooltipInfo(info);

            // 计算显示位置
            var position = CalculatePosition(control, tooltipWindow.CalculateSize());
            tooltipWindow.Location = position;

            // 显示
            tooltipWindow.ShowTooltip(fadeInDuration);

            // 启动自动隐藏定时器
            if (duration > 0)
            {
                hideTimer.Stop();
                hideTimer.Interval = duration;
                hideTimer.Start();
            }
            else if (hideDelay > 0)
            {
                hideTimer.Stop();
                hideTimer.Interval = hideDelay;
                hideTimer.Start();
            }
        }

        private void HideTooltip()
        {
            showTimer.Stop();
            hideTimer.Stop();
            tooltipWindow?.HideTooltip(fadeOutDuration);
            currentControl = null;
        }

        private Point CalculatePosition(Control control, Size tooltipSize)
        {
            var screen = Screen.FromControl(control);
            var screenBounds = screen.WorkingArea;
            var controlBounds = control.RectangleToScreen(control.ClientRectangle);

            int x, y;

            // 先尝试首选位置
            var position = preferredPosition;
            bool fits = TryCalculatePosition(position, controlBounds, tooltipSize, screenBounds, out x, out y);

            // 如果不适合, 尝试其他位置
            if (!fits)
            {
                var alternatives = new[] { TooltipPosition.Bottom, TooltipPosition.Top, TooltipPosition.Right, TooltipPosition.Left };
                foreach (var alt in alternatives)
                {
                    if (alt != position && TryCalculatePosition(alt, controlBounds, tooltipSize, screenBounds, out x, out y))
                    {
                        fits = true;
                        break;
                    }
                }
            }

            // 最后保证在屏幕内
            x = Math.Max(screenBounds.Left, Math.Min(x, screenBounds.Right - tooltipSize.Width));
            y = Math.Max(screenBounds.Top, Math.Min(y, screenBounds.Bottom - tooltipSize.Height));

            return new Point(x, y);
        }

        private bool TryCalculatePosition(TooltipPosition position, Rectangle controlBounds, Size tooltipSize,
            Rectangle screenBounds, out int x, out int y)
        {
            switch (position)
            {
                case TooltipPosition.Top:
                    x = controlBounds.Left + (controlBounds.Width - tooltipSize.Width) / 2;
                    y = controlBounds.Top - tooltipSize.Height - offsetFromControl;
                    break;

                case TooltipPosition.Bottom:
                    x = controlBounds.Left + (controlBounds.Width - tooltipSize.Width) / 2;
                    y = controlBounds.Bottom + offsetFromControl;
                    break;

                case TooltipPosition.Left:
                    x = controlBounds.Left - tooltipSize.Width - offsetFromControl;
                    y = controlBounds.Top + (controlBounds.Height - tooltipSize.Height) / 2;
                    break;

                case TooltipPosition.Right:
                    x = controlBounds.Right + offsetFromControl;
                    y = controlBounds.Top + (controlBounds.Height - tooltipSize.Height) / 2;
                    break;

                case TooltipPosition.Cursor:
                default:
                    x = lastMousePosition.X + 16;
                    y = lastMousePosition.Y + 20;
                    break;
            }

            // 检查是否在屏幕范围内
            return x >= screenBounds.Left &&
                   x + tooltipSize.Width <= screenBounds.Right &&
                   y >= screenBounds.Top &&
                   y + tooltipSize.Height <= screenBounds.Bottom;
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                showTimer?.Stop();
                showTimer?.Dispose();
                hideTimer?.Stop();
                hideTimer?.Dispose();
                tooltipWindow?.Dispose();
                titleFont?.Dispose();
                contentFont?.Dispose();

                foreach (var control in tooltipDataMap.Keys.ToList())
                {
                    try
                    {
                        control.MouseEnter -= Control_MouseEnter;
                        control.MouseLeave -= Control_MouseLeave;
                        control.MouseMove -= Control_MouseMove;
                        control.Disposed -= Control_Disposed;
                    }
                    catch
                    {
                        // 控件可能已经被释放
                    }
                }
                tooltipDataMap.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 提示信息类

    /// <summary>
    /// 存储每个控件的 Tooltip 数据
    /// </summary>
    internal class TooltipData
    {
        public string Text { get; set; } = "";
        public string Title { get; set; } = "";
        public Image Icon { get; set; }
        public Size IconSize { get; set; } = new Size(24, 24);
        public Func<TooltipInfo> DynamicContentProvider { get; set; }

        public bool HasContent => !string.IsNullOrEmpty(Text) || !string.IsNullOrEmpty(Title);

        public TooltipInfo ToTooltipInfo()
        {
            return new TooltipInfo
            {
                Title = Title,
                Content = Text,
                Icon = Icon,
                IconSize = IconSize
            };
        }
    }

    /// <summary>
    /// 工具提示信息
    /// </summary>
    /// <summary>
    /// 工具提示信息
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class TooltipInfo
    {
        /// <summary>
        /// 提示标题(可选)
        /// </summary>
        [Category("Content")]
        [Description("提示标题")]
        [DefaultValue("")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 提示内容
        /// </summary>
        [Category("Content")]
        [Description("提示内容")]
        [DefaultValue("")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Content { get; set; } = "";

        /// <summary>
        /// 提示图标(可选)
        /// </summary>
        [Category("Content")]
        [Description("提示图标")]
        [DefaultValue(null)]
        public Image Icon { get; set; }

        /// <summary>
        /// 图标大小
        /// </summary>
        [Category("Layout")]
        [Description("图标大小")]
        public Size IconSize { get; set; } = new Size(24, 24);

        /// <summary>
        /// 是否有内容
        /// </summary>
        [Browsable(false)]
        public bool HasContent => !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Content);

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Title))
            {
                return Title;
            }
            if (!string.IsNullOrEmpty(Content))
            {
                return Content.Length > 30 ? Content.Substring(0, 30) + "..." : Content;
            }
            return "(无内容)";
        }
    }

    /// <summary>
    /// 工具提示显示窗口
    /// </summary>
    internal class FluentTooltipWindow : Form
    {
        #region Win32 API

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
            ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc,
            uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        private const int ULW_ALPHA = 0x02;
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_TOPMOST = 0x8;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

        #region 字段

        private readonly FluentTooltip owner;
        private TooltipInfo currentInfo;
        private Timer fadeTimer;
        private double currentOpacity = 0;
        private bool isFadingIn = false;
        private int targetFadeDuration;
        private Bitmap surfaceBitmap;

        #endregion

        #region 构造函数

        public FluentTooltipWindow(FluentTooltip owner)
        {
            this.owner = owner;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            fadeTimer = new Timer { Interval = 16 };
            fadeTimer.Tick += FadeTimer_Tick;
        }

        #endregion

        #region 属性重写

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                cp.ExStyle |= WS_EX_TOPMOST;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        #endregion

        #region 公共方法

        public void SetTooltipInfo(TooltipInfo info)
        {
            currentInfo = info;
            var size = CalculateSize();
            this.Size = size;
            UpdateSurface();
        }

        public Size CalculateSize()
        {
            if (currentInfo == null || !currentInfo.HasContent)
            {
                return new Size(80, 28);
            }

            using (var bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int width = owner.ContentPadding.Horizontal;
                int height = owner.ContentPadding.Vertical;

                // 计算内容起始位置(考虑图标)
                int contentStartX = owner.ContentPadding.Left;
                if (currentInfo.Icon != null)
                {
                    contentStartX += currentInfo.IconSize.Width + 6;
                }

                int maxContentWidth = owner.MaxWidth - contentStartX - owner.ContentPadding.Right;

                // 标题高度
                int titleHeight = 0;
                if (!string.IsNullOrEmpty(currentInfo.Title))
                {
                    var titleSize = g.MeasureString(currentInfo.Title, owner.TitleFont, maxContentWidth);
                    width = Math.Max(width, contentStartX + (int)Math.Ceiling(titleSize.Width) + owner.ContentPadding.Right);
                    titleHeight = (int)Math.Ceiling(titleSize.Height);
                    height += titleHeight + owner.TitlePadding.Vertical;
                }

                // 内容
                if (!string.IsNullOrEmpty(currentInfo.Content))
                {
                    var contentSize = g.MeasureString(currentInfo.Content, owner.ContentFont, maxContentWidth);
                    width = Math.Max(width, contentStartX + (int)Math.Ceiling(contentSize.Width) + owner.ContentPadding.Right);
                    height += (int)Math.Ceiling(contentSize.Height);

                    // 如果有标题, 添加间距
                    if (titleHeight > 0)
                    {
                        height += 2;
                    }
                }

                // 图标高度
                if (currentInfo.Icon != null)
                {
                    int iconAreaHeight = owner.ContentPadding.Vertical + currentInfo.IconSize.Height;
                    height = Math.Max(height, iconAreaHeight);
                }

                width = Math.Min(width, owner.MaxWidth);
                height = Math.Max(height, 24);
                width = Math.Max(width, height);

                // 添加阴影空间
                if (owner.ShowShadow)
                {
                    width += 6;
                    height += 6;
                }

                return new Size(width, height);
            }
        }

        public void ShowTooltip(int fadeDuration)
        {
            UpdateSurface();

            if (fadeDuration > 0)
            {
                isFadingIn = true;
                targetFadeDuration = fadeDuration;
                currentOpacity = 0;
                SetBitmapOpacity((byte)0);
                this.Show();
                fadeTimer.Start();
            }
            else
            {
                currentOpacity = 1;
                SetBitmapOpacity(255);
                this.Show();
            }
        }

        public void HideTooltip(int fadeDuration)
        {
            if (fadeDuration > 0 && this.Visible)
            {
                isFadingIn = false;
                targetFadeDuration = fadeDuration;
                fadeTimer.Start();
            }
            else
            {
                fadeTimer.Stop();
                this.Hide();
            }
        }

        #endregion

        #region 私有方法

        private void UpdateSurface()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            surfaceBitmap?.Dispose();
            surfaceBitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(surfaceBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 清除背景(完全透明)
                g.Clear(Color.Transparent);

                int shadowOffset = owner.ShowShadow ? 3 : 0;
                var contentRect = new Rectangle(0, 0, Width - shadowOffset * 2, Height - shadowOffset * 2);

                // 绘制阴影
                if (owner.ShowShadow)
                {
                    DrawShadow(g, contentRect, shadowOffset);
                }

                // 绘制主体
                DrawTooltipBody(g, contentRect);
            }

            SetBitmapOpacity((byte)(currentOpacity * 255));
        }

        private void DrawShadow(Graphics g, Rectangle contentRect, int offset)
        {
            // 多层阴影实现柔和效果
            for (int i = 4; i >= 1; i--)
            {
                int alpha = 15 * (5 - i);
                var shadowRect = new Rectangle(
                    contentRect.X + offset + i - 1,
                    contentRect.Y + offset + i,
                    contentRect.Width,
                    contentRect.Height);

                using (var shadowPath = CreateRoundedRectanglePath(shadowRect, owner.BorderRadius + i))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }
        }

        private void DrawTooltipBody(Graphics g, Rectangle rect)
        {
            if (currentInfo == null)
            {
                return;
            }

            int shadowOffset = owner.ShowShadow ? 3 : 0;
            var bodyRect = new Rectangle(shadowOffset, shadowOffset, rect.Width, rect.Height);

            using (var path = CreateRoundedRectanglePath(bodyRect, owner.BorderRadius))
            {
                // 填充背景
                using (var bgBrush = new SolidBrush(owner.BackgroundColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // 绘制标题背景(如果有标题且设置了标题背景色)
                if (!string.IsNullOrEmpty(currentInfo.Title) && owner.TitleBackgroundColor != Color.Empty
                    && owner.TitleBackgroundColor != owner.BackgroundColor)
                {
                    DrawTitleBackground(g, bodyRect);
                }

                // 绘制边框
                using (var borderPen = new Pen(owner.BorderColor, 1))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // 绘制内容
            DrawContent(g, bodyRect);
        }

        private void DrawTitleBackground(Graphics g, Rectangle bodyRect)
        {
            using (var measureBmp = new Bitmap(1, 1))
            using (var measureG = Graphics.FromImage(measureBmp))
            {
                var titleSize = measureG.MeasureString(currentInfo.Title, owner.TitleFont,
                    bodyRect.Width - owner.ContentPadding.Horizontal);
                int titleAreaHeight = (int)Math.Ceiling(titleSize.Height) + owner.TitlePadding.Vertical;

                var titleRect = new Rectangle(
                    bodyRect.X + 1,
                    bodyRect.Y + 1,
                    bodyRect.Width - 2,
                    titleAreaHeight);

                // 创建带圆角顶部的路径
                using (var titlePath = CreateTopRoundedRectanglePath(titleRect, owner.BorderRadius - 1))
                using (var titleBrush = new SolidBrush(owner.TitleBackgroundColor))
                {
                    g.FillPath(titleBrush, titlePath);
                }

                // 绘制分隔线
                using (var separatorPen = new Pen(Color.FromArgb(50, owner.BorderColor), 1))
                {
                    g.DrawLine(separatorPen,
                        bodyRect.X + owner.ContentPadding.Left,
                        bodyRect.Y + titleAreaHeight,
                        bodyRect.Right - owner.ContentPadding.Right,
                        bodyRect.Y + titleAreaHeight);
                }
            }
        }

        private void DrawContent(Graphics g, Rectangle bodyRect)
        {
            int x = bodyRect.X + owner.ContentPadding.Left;
            int y = bodyRect.Y + owner.ContentPadding.Top;
            int contentWidth = bodyRect.Width - owner.ContentPadding.Horizontal;

            // 图标
            int textStartX = x;
            if (currentInfo.Icon != null)
            {
                int iconY = y;
                if (!string.IsNullOrEmpty(currentInfo.Title))
                {
                    iconY += owner.TitlePadding.Top;
                }

                var iconRect = new Rectangle(x, iconY, currentInfo.IconSize.Width, currentInfo.IconSize.Height);
                g.DrawImage(currentInfo.Icon, iconRect);
                textStartX += currentInfo.IconSize.Width + 6;
                contentWidth -= currentInfo.IconSize.Width + 6;
            }

            // 标题
            if (!string.IsNullOrEmpty(currentInfo.Title))
            {
                int titleY = y + owner.TitlePadding.Top;
                var titleRect = new RectangleF(textStartX, titleY, contentWidth, 100);

                using (var titleBrush = new SolidBrush(owner.TitleColor))
                using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter })
                {
                    var titleSize = g.MeasureString(currentInfo.Title, owner.TitleFont, (int)titleRect.Width);
                    g.DrawString(currentInfo.Title, owner.TitleFont, titleBrush, titleRect, sf);
                    y += (int)Math.Ceiling(titleSize.Height) + owner.TitlePadding.Vertical + 2;
                }
            }

            // 内容
            if (!string.IsNullOrEmpty(currentInfo.Content))
            {
                var contentRect = new RectangleF(textStartX, y, contentWidth,
                    bodyRect.Bottom - y - owner.ContentPadding.Bottom);

                using (var contentBrush = new SolidBrush(owner.ForeColor))
                using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter })
                {
                    g.DrawString(currentInfo.Content, owner.ContentFont, contentBrush, contentRect, sf);
                }
            }
        }

        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            int diameter = radius * 2;

            var arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上角
            path.AddArc(arcRect, 180, 90);

            // 右上角
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // 右下角
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // 左下角
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateTopRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            int diameter = radius * 2;

            var arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上角(圆角)
            path.AddArc(arcRect, 180, 90);

            // 右上角(圆角)
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // 右下角(直角)
            path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);

            path.CloseFigure();
            return path;
        }

        private void SetBitmapOpacity(byte opacity)
        {
            if (surfaceBitmap == null || !IsHandleCreated)
            {
                return;
            }

            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = surfaceBitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmap = SelectObject(memDc, hBitmap);

                var blend = new BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = opacity,
                    AlphaFormat = AC_SRC_ALPHA
                };

                var ptSrc = new POINT { x = 0, y = 0 };
                var size = new SIZE { cx = surfaceBitmap.Width, cy = surfaceBitmap.Height };
                var ptDst = new POINT { x = Left, y = Top };

                UpdateLayeredWindow(Handle, screenDc, ref ptDst, ref size, memDc, ref ptSrc, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                if (oldBitmap != IntPtr.Zero)
                {
                    SelectObject(memDc, oldBitmap);
                }
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            double step = 16.0 / Math.Max(1, targetFadeDuration);

            if (isFadingIn)
            {
                currentOpacity += step;
                if (currentOpacity >= 1)
                {
                    currentOpacity = 1;
                    fadeTimer.Stop();
                }
            }
            else
            {
                currentOpacity -= step;
                if (currentOpacity <= 0)
                {
                    currentOpacity = 0;
                    fadeTimer.Stop();
                    this.Hide();
                    return;
                }
            }

            SetBitmapOpacity((byte)(currentOpacity * 255));
        }

        #endregion

        #region 重写

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (Visible && surfaceBitmap != null)
            {
                SetBitmapOpacity((byte)(currentOpacity * 255));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadeTimer?.Stop();
                fadeTimer?.Dispose();
                surfaceBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 工具提示位置
    /// </summary>
    public enum TooltipPosition
    {
        Cursor,         // 跟随鼠标
        Top,            // 控件上方
        Bottom,         // 控件下方
        Left,           // 控件左侧
        Right           // 控件右侧
    }

    #endregion
}
