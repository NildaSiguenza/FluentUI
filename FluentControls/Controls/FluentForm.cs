using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Themes;
using System.Windows.Forms;
using FluentControls.Animation;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;

namespace FluentControls.Controls
{

    public class FluentForm : Form
    {
        #region Win32 API

        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        #endregion

        #region 字段

        private const int RESIZE_BORDER_SIZE = 3;

        private IFluentTheme theme;
        private FluentTitleBar titleBar;
        private List<FluentTitleBarButton> titleBarButtons;
        private Dictionary<int, HotKeyInfo> hotKeys;
        private Dictionary<Keys, Action> localHotKeys = new Dictionary<Keys, Action>();
        private int hotKeyId = 0;

        private bool canDrag = true;
        private bool isFixed = false;
        private int cornerRadius = 8;
        private bool isDragging = false;
        private Point dragStartPoint;

        private DateTime lastTitleBarClickTime = DateTime.MinValue;
        private const int DOUBLE_CLICK_TIME = 500; // 毫秒

        #endregion

        #region 构造函数

        public FluentForm()
        {
            InitializeForm();
            InitializeTitleBar();
            ApplyTheme();
            EnableShadow();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            UpdateStyles(); // 减少闪烁

            MinimumSize = new Size(300, 200);
            hotKeys = new Dictionary<int, HotKeyInfo>();
            titleBarButtons = new List<FluentTitleBarButton>();

        }

        private void InitializeTitleBar()
        {
            titleBar = new FluentTitleBar(this)
            {
                Height = 32,
                Title = Text,
                ShowIcon = ShowIcon,
                Icon = Icon.ToBitmap()
            };

            // 系统按钮事件
            titleBar.CloseButton.Click += (s, e) => CloseForm();
            titleBar.MaximizeButton.Click += (s, e) => ToggleMaximize();
            titleBar.MinimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;

            // 更新Padding以为标题栏留出空间
            Padding = new Padding(Padding.Left,
                                 titleBar.Height,
                                 Padding.Right,
                                 Padding.Bottom);
        }

        #endregion

        #region 属性

        public new Padding Padding
        {
            get
            {
                if (WindowState == FormWindowState.Maximized)
                {
                    // 最大化时只需要为标题栏预留空间
                    return new Padding(0, titleBar?.Height ?? 32, 0, 0);
                }
                else if (CanResize && !IsFixed)
                {
                    // 正常状态且可调整大小时, 预留边框空间
                    return new Padding(
                        RESIZE_BORDER_SIZE,
                        titleBar?.Height ?? 32,
                        RESIZE_BORDER_SIZE,
                        RESIZE_BORDER_SIZE);
                }
                else
                {
                    // 不可调整大小时, 只预留标题栏空间
                    return new Padding(0, titleBar?.Height ?? 32, 0, 0);
                }
            }
            set
            {
                // 忽略外部设置, 始终使用计算的值
                base.Padding = value;
            }
        }

        [Category("Fluent")]
        [Description("窗体主题")]
        public IFluentTheme Theme
        {
            get => theme ?? (ThemeManager.CurrentTheme ?? ThemeManager.DefaultTheme);
            set
            {
                theme = value;
                ApplyTheme();
            }
        }

        [Category("Fluent")]
        [Description("继承主题")]
        public bool InheritTheme { get; set; } = true;

        [Category("Fluent")]
        [Description("标题栏")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FluentTitleBar TitleBar => titleBar;

        [Category("Fluent")]
        [Description("圆角半径")]
        [DefaultValue(8)]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, Math.Min(15, value));
                ApplyRoundedCorners();
            }
        }

        [Category("Fluent")]
        [Description("阴影级别")]
        [DefaultValue(4)]
        public int ShadowLevel { get; set; } = 4;

        [Category("Fluent")]
        [Description("是否可拖动")]
        [DefaultValue(true)]
        public bool CanDrag
        {
            get => canDrag && !isFixed;
            set => canDrag = value;
        }

        [Category("Fluent")]
        [Description("是否固定窗口")]
        [DefaultValue(false)]
        public bool IsFixed
        {
            get => isFixed;
            set
            {
                isFixed = value;
                UpdateFixedState();
            }
        }

        [Category("Fluent")]
        [Description("是否可调整大小")]
        [DefaultValue(true)]
        public bool CanResize { get; set; } = true;

        [Category("Fluent")]
        [Description("是否限制在屏幕内")]
        [DefaultValue(true)]
        public bool KeepInScreen { get; set; } = true;

        [Category("Fluent")]
        [Description("关闭时确认")]
        [DefaultValue(false)]
        public bool ConfirmOnClose { get; set; } = false;

        [Category("Fluent")]
        [Description("关闭确认消息")]
        [DefaultValue("确定要关闭窗口吗？")]
        public string CloseConfirmMessage { get; set; } = "确定要关闭窗口吗？";

        [Category("Fluent")]
        [Description("全屏时是否覆盖任务栏")]
        [DefaultValue(false)]
        public bool FullScreenOverTaskbar { get; set; } = false;

        private bool isFullScreen = false;
        private FormWindowState previousWindowState;
        private Rectangle previousBounds;

        [Browsable(false)]
        public bool IsFullScreen
        {
            get => isFullScreen;
            set
            {
                if (isFullScreen != value)
                {
                    isFullScreen = value;
                    if (value)
                    {
                        EnterFullScreen();
                    }
                    else
                    {
                        ExitFullScreen();
                    }
                }
            }
        }

        #endregion

        #region 重写方法

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x00020000; // WS_MINIMIZEBOX
                cp.Style |= 0x00010000; // WS_MAXIMIZEBOX
                //cp.Style |= 0x00080000; // WS_SYSMEN   //可控制窗口是否可以拖动边缘

                if (!DesignMode)
                {
                    cp.ExStyle |= 0x02000000; //启用双缓冲 WS_EX_COMPOSITED
                }
                return cp;
            }
        }


        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (titleBar != null)
            {
                titleBar.Title = Text;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 绘制标题栏背景
            if (titleBar != null)
            {
                using (var brush = new SolidBrush(titleBar.BackColor))
                {
                    if (CornerRadius > 0 && WindowState != FormWindowState.Maximized)
                    {
                        // 使用路径绘制带圆角的标题栏背景
                        using (var path = new GraphicsPath())
                        {
                            // 创建顶部圆角矩形
                            path.AddArc(0, 0, CornerRadius * 2, CornerRadius * 2, 180, 90);
                            path.AddArc(Width - CornerRadius * 2, 0, CornerRadius * 2, CornerRadius * 2, 270, 90);
                            path.AddLine(Width, titleBar.Height, 0, titleBar.Height);
                            path.CloseFigure();

                            g.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        // 无圆角或最大化时直接填充矩形
                        g.FillRectangle(brush, 0, 0, Width, titleBar.Height);
                    }
                }
            }

            // 绘制标题栏内容
            titleBar?.Draw(g);

            // 绘制主体背景
            using (var backBrush = new SolidBrush(BackColor))
            {
                if (CornerRadius > 0 && WindowState != FormWindowState.Maximized)
                {
                    // 创建底部圆角矩形
                    using (var path = new GraphicsPath())
                    {
                        path.AddRectangle(new Rectangle(0, titleBar.Height, Width, Height - titleBar.Height - CornerRadius));
                        path.AddArc(0, Height - CornerRadius * 2, CornerRadius * 2, CornerRadius * 2, 90, 90);
                        path.AddArc(Width - CornerRadius * 2, Height - CornerRadius * 2, CornerRadius * 2, CornerRadius * 2, 0, 90);

                        g.FillPath(backBrush, path);
                    }
                }
                else
                {
                    g.FillRectangle(backBrush, 0, titleBar.Height, Width, Height - titleBar.Height);
                }
            }

            // 绘制边框
            if (WindowState != FormWindowState.Maximized && Theme != null)
            {
                // 绘制标题栏部分的边框(使用标题栏颜色)
                using (var titlePen = new Pen(titleBar.BackColor, 1))
                {
                    if (CornerRadius > 0)
                    {
                        using (var path = new GraphicsPath())
                        {
                            // 只绘制顶部圆角边框
                            path.AddArc(0, 0, CornerRadius * 2, CornerRadius * 2, 180, 90);
                            path.AddArc(Width - CornerRadius * 2 - 1, 0, CornerRadius * 2, CornerRadius * 2, 270, 90);
                            g.DrawPath(titlePen, path);
                        }
                    }
                }

                // 绘制主体部分的边框
                using (var pen = new Pen(Theme.Colors.Border, 2))
                {
                    if (CornerRadius > 0)
                    {
                        using (var path = new GraphicsPath())
                        {
                            // 左边线
                            path.AddLine(0, titleBar.Height, 0, Height - CornerRadius);
                            // 左下圆角
                            path.AddArc(0, Height - CornerRadius * 2 - 1, CornerRadius * 2, CornerRadius * 2, 90, 90);
                            // 底边线
                            path.AddLine(CornerRadius, Height - 1, Width - CornerRadius, Height - 1);
                            // 右下圆角
                            path.AddArc(Width - CornerRadius * 2 - 1, Height - CornerRadius * 2 - 1, CornerRadius * 2, CornerRadius * 2, 0, 90);
                            // 右边线
                            path.AddLine(Width - 1, Height - CornerRadius, Width - 1, titleBar.Height);

                            g.DrawPath(pen, path);
                        }
                    }
                    else
                    {
                        // 绘制无圆角时的边框
                        using (var path = new GraphicsPath())
                        {
                            path.AddLine(1, titleBar.Height, 1, Height - 1);
                            path.AddLine(1, Height - 1, Width - 1, Height - 1);
                            path.AddLine(Width - 1, Height - 1, Width - 1, titleBar.Height);
                            g.DrawPath(pen, path);
                        }

                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // 处理标题栏按钮悬停
            if (titleBar != null && e.Y <= titleBar.Height)
            {
                bool needInvalidate = titleBar.OnMouseMove(e);
                if (needInvalidate)
                {
                    Invalidate(titleBar.Bounds);
                }

                // 更新鼠标指针
                Cursor = titleBar.IsPointOnButton(e.Location) ? Cursors.Hand : Cursors.Default;
            }
            else if (titleBar != null)
            {
                // 鼠标离开标题栏区域
                if (titleBar.HasHoveredButton())
                {
                    titleBar.ClearHoverState();
                    Invalidate(titleBar.Bounds);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && titleBar != null)
            {
                // 检查是否点击了标题栏按钮
                if (e.Y <= titleBar.Height && titleBar.OnMouseDown(e))
                {
                    Invalidate(titleBar.Bounds);
                    return;
                }

                // 检查是否在标题栏区域(非按钮区域)
                if (e.Y <= titleBar.Height && !titleBar.IsPointOnButton(e.Location))
                {
                    // 检测双击
                    var now = DateTime.Now;
                    if ((now - lastTitleBarClickTime).TotalMilliseconds < DOUBLE_CLICK_TIME)
                    {
                        // 双击标题栏
                        if (titleBar.ShowMaximizeButton)
                        {
                            ToggleMaximize();
                        }
                        lastTitleBarClickTime = DateTime.MinValue;
                    }
                    else
                    {
                        lastTitleBarClickTime = now;

                        // 拖动窗口
                        if (CanDrag)
                        {
                            ReleaseCapture();
                            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (isDragging)
            {
                isDragging = false;
            }

            if (titleBar != null && e.Y <= titleBar.Height)
            {
                titleBar.OnMouseUp(e);
                Invalidate(titleBar.Bounds);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (titleBar != null && titleBar.HasHoveredButton())
            {
                titleBar.ClearHoverState();
                Invalidate(titleBar.Bounds);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && CanResize && WindowState != FormWindowState.Maximized && !IsFixed)
            {
                Point pos = PointToClient(Cursor.Position);
                const int RESIZE_BORDER = 8;

                // 防止标题栏区域被识别为调整大小区域
                if (pos.Y <= titleBar.Height)
                {
                    base.WndProc(ref m);
                    return;
                }

                if (pos.X <= RESIZE_BORDER && pos.Y <= RESIZE_BORDER)
                {
                    m.Result = (IntPtr)13; // HTTOPLEFT
                }
                else if (pos.X >= Width - RESIZE_BORDER && pos.Y <= RESIZE_BORDER)
                {
                    m.Result = (IntPtr)14; // HTTOPRIGHT
                }
                else if (pos.X <= RESIZE_BORDER && pos.Y >= Height - RESIZE_BORDER)
                {
                    m.Result = (IntPtr)16; // HTBOTTOMLEFT
                }
                else if (pos.X >= Width - RESIZE_BORDER && pos.Y >= Height - RESIZE_BORDER)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                }
                else if (pos.X <= RESIZE_BORDER)
                {
                    m.Result = (IntPtr)10; // HTLEFT
                }
                else if (pos.X >= Width - RESIZE_BORDER)
                {
                    m.Result = (IntPtr)11; // HTRIGHT
                }
                else if (pos.Y <= RESIZE_BORDER)
                {
                    m.Result = (IntPtr)12; // HTTOP
                }
                else if (pos.Y >= Height - RESIZE_BORDER)
                {
                    m.Result = (IntPtr)15; // HTBOTTOM
                }
                else
                {
                    base.WndProc(ref m);
                }
            }
            else if (m.Msg == WM_HOTKEY)
            {
                HandleHotKey(m.WParam.ToInt32());
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            // 暂停布局
            SuspendLayout();

            base.OnResize(e);
            base.Padding = Padding;

            ApplyRoundedCorners();

            if (KeepInScreen && WindowState == FormWindowState.Normal)
            {
                var screen = Screen.FromControl(this);
                var workingArea = screen.WorkingArea;

                if (Left < workingArea.Left)
                {
                    Left = workingArea.Left;
                }

                if (Top < workingArea.Top)
                {
                    Top = workingArea.Top;
                }

                if (Right > workingArea.Right)
                {
                    Left = workingArea.Right - Width;
                }

                if (Bottom > workingArea.Bottom)
                {
                    Top = workingArea.Bottom - Height;
                }
            }

            if (titleBar != null)
            {
                titleBar.Width = Width;
                titleBar.UpdateButtonPositions();
            }

            // 恢复布局
            ResumeLayout();
            Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            // 确保控件不会覆盖标题栏区域
            if (titleBar != null && e.Control.Top < titleBar.Height)
            {
                e.Control.Top = titleBar.Height;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 检查本地快捷键
            if (localHotKeys.ContainsKey(keyData))
            {
                OnHotKeyPressed(new HotKeyEventArgs(keyData & Keys.KeyCode, keyData & Keys.Modifiers));
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.Padding = Padding;
            base.OnFormClosing(e);
        }

        #endregion

        #region 私有方法

        private void ApplyTheme()
        {
            if (Theme != null)
            {
                BackColor = Theme.Colors.Background;
                ForeColor = Theme.Colors.TextPrimary;
                Font = Theme.Typography.Body;

                titleBar?.ApplyTheme(Theme);

                Invalidate();
            }
        }

        private void ApplyRoundedCorners()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                // 最大化时清除区域
                SetWindowRgn(Handle, IntPtr.Zero, true);
                return;
            }

            IntPtr hRgn = CreateRoundRectRgn(0, 0, Width, Height, CornerRadius, CornerRadius);
            SetWindowRgn(this.Handle, hRgn, true);

            Invalidate();
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void EnableShadow()
        {
            if (ShadowLevel <= 0)
            {
                return;
            }

            var v = 2;
            DwmSetWindowAttribute(Handle, 2, ref v, 4);

            var margins = new MARGINS
            {
                Left = 0,
                Right = 0,
                Top = 0,
                Bottom = 1
            };
            DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        private void UpdateFixedState()
        {
            var pinButton = titleBar?.GetButton("PinButton");
            if (pinButton != null)
            {
                pinButton.Text = isFixed ? "📌" : "📍";
                pinButton.ToolTip = isFixed ? "取消固定" : "固定窗口";
                Invalidate(titleBar.Bounds);
            }
        }

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }

            // 更新最大化按钮的图标
            if (titleBar != null)
            {
                titleBar.Width = Width;
                titleBar.UpdateButtonPositions();

                if (titleBar.MaximizeButton != null)
                {
                    titleBar.MaximizeButton.Text = WindowState == FormWindowState.Maximized ? "❐" : "□";
                    titleBar.MaximizeButton.ToolTip = WindowState == FormWindowState.Maximized ? "还原" : "最大化";
                }
            }
        }

        private void CloseForm()
        {
            if (ConfirmOnClose)
            {
                var result = MessageBox.Show(
                    CloseConfirmMessage,
                    "确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            Close();
        }

        private void EnterFullScreen()
        {
            previousWindowState = WindowState;
            previousBounds = Bounds;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;

            if (FullScreenOverTaskbar)
            {
                Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                Bounds = Screen.PrimaryScreen.WorkingArea;
            }
        }

        private void ExitFullScreen()
        {
            WindowState = previousWindowState;
            Bounds = previousBounds;
        }

        #endregion

        #region 公有方法

        public void AddPresetButtons()
        {
            AddPinButton();
            AddThemeButton();
        }

        public void AddPinButton()
        {
            // 固定按钮
            var pinButton = new FluentTitleBarButton
            {
                Name = "PinButton",
                Type = TitleBarButtonType.Pin,
                Text = "📍",
                ToolTip = "固定窗口",
                Width = 36
            };
            pinButton.Click += (s, e) => IsFixed = !IsFixed;
            titleBar.AddButton(pinButton);
        }

        public void AddThemeButton()
        {
            // 主题切换按钮
            var themeButton = new FluentTitleBarButton
            {
                Name = "ThemeButton",
                Type = TitleBarButtonType.Custom,
                Text = "🎨",
                ToolTip = "切换主题",
                Width = 36,
                EnableDropDown = true
            };

            themeButton.AddDropDownItem("浅色主题", (s, e) =>
            {
                ThemeManager.ApplyTheme("FluentLight");
                ApplyTheme();
            });
            themeButton.AddDropDownItem("深色主题", (s, e) =>
            {
                ThemeManager.ApplyTheme("FluentDark");
                ApplyTheme();
            });
            themeButton.AddDropDownItem("高对比度", (s, e) =>
            {
                ThemeManager.ApplyTheme("HighContrast");
                ApplyTheme();
            });

            titleBar.AddButton(themeButton);
        }

        #endregion

        #region 热键

        public bool RegisterHotKey(Keys key, Keys modifiers = Keys.None)
        {
            int id = hotKeyId++;
            uint mod = (uint)modifiers;
            uint vk = (uint)key;

            // 尝试注册热键
            if (RegisterHotKey(Handle, id, mod, vk))
            {
                hotKeys[id] = new HotKeyInfo { Key = key, Modifiers = modifiers };
                return true;
            }

            // 如果注册失败, 尝试使用本地快捷键
            return RegisterLocalHotKey(key, modifiers);
        }

        public bool RegisterLocalHotKey(Keys key, Keys modifiers = Keys.None)
        {
            var fullKey = key | modifiers;
            if (!localHotKeys.ContainsKey(fullKey))
            {
                localHotKeys[fullKey] = null;
                return true;
            }
            return false;
        }

        public void UnregisterAllHotKeys()
        {
            foreach (var id in hotKeys.Keys)
            {
                UnregisterHotKey(Handle, id);
            }
            hotKeys.Clear();
        }

        private void HandleHotKey(int id)
        {
            if (hotKeys.TryGetValue(id, out var info))
            {
                OnHotKeyPressed(new HotKeyEventArgs(info.Key, info.Modifiers));
            }
        }

        protected virtual void OnHotKeyPressed(HotKeyEventArgs e)
        {
            // 可以在派生类中重写
        }

        /// <summary>
        /// 注册带回调的快捷键
        /// </summary>
        public void RegisterHotKey(Keys key, Keys modifiers, Action callback)
        {
            var fullKey = key | modifiers;

            // 先尝试全局热键
            if (RegisterHotKey(key, modifiers))
            {
                // 成功注册为全局热键
                return;
            }

            // 降级为本地快捷键
            localHotKeys[fullKey] = callback;
        }

        #endregion

    }
}
