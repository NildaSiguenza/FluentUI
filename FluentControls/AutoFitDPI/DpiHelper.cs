using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls
{
    public static class DpiHelper
    {
        #region Native Methods

        // Windows 10 1607+ (Build 14393)
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetDpiForWindow(IntPtr hWnd);

        // Windows 8.1+ (需要 Shcore.dll)
        [DllImport("shcore.dll", SetLastError = true)]
        private static extern int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType,
            out uint dpiX, out uint dpiY);

        // Windows 7+ - 获取显示器句柄
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        // Windows 7+ - GDI 方式获取 DPI
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        // 检查 DLL 中是否存在指定函数
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        #region Constants

        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const int DEFAULT_DPI = 96;

        private enum MonitorDpiType
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2
        }

        #endregion

        #region Cached Values

        private static readonly Lazy<DpiApiSupport> _apiSupport = new Lazy<DpiApiSupport>(DetectApiSupport);
        private static readonly object _lockObject = new object();

        #endregion

        #region API Support Detection

        private enum DpiApiSupport
        {
            None,           // Windows 7/8 - 使用 GetDeviceCaps
            PerMonitor,     // Windows 8.1 - 使用 GetDpiForMonitor
            PerWindow       // Windows 10 1607+ - 使用 GetDpiForWindow
        }

        /// <summary>
        /// 检测系统支持的 DPI API 级别
        /// </summary>
        private static DpiApiSupport DetectApiSupport()
        {
            // 方法1：通过检查函数是否存在
            IntPtr user32 = GetModuleHandle("user32.dll");
            if (user32 != IntPtr.Zero)
            {
                // 检查 GetDpiForWindow (Windows 10 1607+)
                if (GetProcAddress(user32, "GetDpiForWindow") != IntPtr.Zero)
                {
                    return DpiApiSupport.PerWindow;
                }
            }

            // 检查 GetDpiForMonitor (Windows 8.1+)
            IntPtr shcore = IntPtr.Zero;
            try
            {
                shcore = LoadLibrary("shcore.dll");
                if (shcore != IntPtr.Zero)
                {
                    if (GetProcAddress(shcore, "GetDpiForMonitor") != IntPtr.Zero)
                    {
                        return DpiApiSupport.PerMonitor;
                    }
                }
            }
            catch
            {
                // shcore.dll 不存在
            }
            finally
            {
                if (shcore != IntPtr.Zero)
                {
                    FreeLibrary(shcore);
                }
            }

            // Windows 7/8 - 使用 GDI
            return DpiApiSupport.None;
        }

        /// <summary>
        /// 获取当前 API 支持级别(用于调试)
        /// </summary>
        public static string GetApiSupportLevel()
        {
            switch (_apiSupport.Value)
            {
                case DpiApiSupport.PerWindow:
                    return "Windows 10 1607+ (GetDpiForWindow)";
                case DpiApiSupport.PerMonitor:
                    return "Windows 8.1 (GetDpiForMonitor)";
                default:
                    return "Windows 7/8 (GetDeviceCaps)";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取窗口的 DPI(兼容所有 Windows 版本)
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>DPI 值(默认 96)</returns>
        public static int GetDpiForWindowSafe(IntPtr hWnd)
        {
            try
            {
                switch (_apiSupport.Value)
                {
                    case DpiApiSupport.PerWindow:
                        return GetDpiForWindowInternal(hWnd);

                    case DpiApiSupport.PerMonitor:
                        return GetDpiForMonitorInternal(hWnd);

                    default:
                        return GetSystemDpi();
                }
            }
            catch
            {
                // 出错时返回默认 DPI
                return DEFAULT_DPI;
            }
        }

        /// <summary>
        /// 获取控件的 DPI
        /// </summary>
        public static int GetDpiForControl(Control control)
        {
            if (control == null)
            {
                return GetSystemDpi();
            }

            if (control.IsHandleCreated)
            {
                return GetDpiForWindowSafe(control.Handle);
            }

            // 句柄未创建时, 使用 Graphics 获取
            return GetDpiUsingGraphics(control);
        }

        /// <summary>
        /// 获取窗体的 DPI
        /// </summary>
        public static int GetDpiForForm(Form form)
        {
            if (form == null)
            {
                return GetSystemDpi();
            }

            if (form.IsHandleCreated)
            {
                return GetDpiForWindowSafe(form.Handle);
            }

            return GetDpiUsingGraphics(form);
        }

        /// <summary>
        /// 获取系统 DPI(主显示器)
        /// </summary>
        public static int GetSystemDpi()
        {
            IntPtr hdc = IntPtr.Zero;
            try
            {
                hdc = GetDC(IntPtr.Zero);
                if (hdc != IntPtr.Zero)
                {
                    return GetDeviceCaps(hdc, LOGPIXELSX);
                }
            }
            catch
            {
                // 忽略错误
            }
            finally
            {
                if (hdc != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }

            return DEFAULT_DPI;
        }

        /// <summary>
        /// 获取 DPI 缩放比例
        /// </summary>
        public static float GetScaleFactor(IntPtr hWnd)
        {
            int dpi = GetDpiForWindowSafe(hWnd);
            return dpi / (float)DEFAULT_DPI;
        }

        /// <summary>
        /// 获取 DPI 缩放比例
        /// </summary>
        public static float GetScaleFactor(Control control)
        {
            int dpi = GetDpiForControl(control);
            return dpi / (float)DEFAULT_DPI;
        }

        /// <summary>
        /// 根据 DPI 缩放值
        /// </summary>
        public static int Scale(int value, IntPtr hWnd)
        {
            float scale = GetScaleFactor(hWnd);
            return (int)Math.Round(value * scale);
        }

        /// <summary>
        /// 根据 DPI 缩放值
        /// </summary>
        public static int Scale(int value, Control control)
        {
            float scale = GetScaleFactor(control);
            return (int)Math.Round(value * scale);
        }

        /// <summary>
        /// 根据 DPI 缩放尺寸
        /// </summary>
        public static Size Scale(Size size, Control control)
        {
            float scale = GetScaleFactor(control);
            return new Size(
                (int)Math.Round(size.Width * scale),
                (int)Math.Round(size.Height * scale));
        }

        /// <summary>
        /// 根据 DPI 缩放点
        /// </summary>
        public static Point Scale(Point point, Control control)
        {
            float scale = GetScaleFactor(control);
            return new Point(
                (int)Math.Round(point.X * scale),
                (int)Math.Round(point.Y * scale));
        }

        /// <summary>
        /// 检查是否为高 DPI 环境
        /// </summary>
        public static bool IsHighDpi(Control control = null)
        {
            int dpi = control != null ? GetDpiForControl(control) : GetSystemDpi();
            return dpi > DEFAULT_DPI;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Windows 10 1607+ 方式获取 DPI
        /// </summary>
        private static int GetDpiForWindowInternal(IntPtr hWnd)
        {
            try
            {
                int dpi = GetDpiForWindow(hWnd);
                return dpi > 0 ? dpi : DEFAULT_DPI;
            }
            catch
            {
                return GetSystemDpi();
            }
        }

        /// <summary>
        /// Windows 8.1 方式获取 DPI
        /// </summary>
        private static int GetDpiForMonitorInternal(IntPtr hWnd)
        {
            try
            {
                IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
                if (hMonitor != IntPtr.Zero)
                {
                    int hr = GetDpiForMonitor(hMonitor, MonitorDpiType.MDT_EFFECTIVE_DPI,
                        out uint dpiX, out uint dpiY);

                    if (hr == 0) // S_OK
                    {
                        return (int)dpiX;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }

            return GetSystemDpi();
        }

        /// <summary>
        /// 使用 Graphics 对象获取 DPI
        /// </summary>
        private static int GetDpiUsingGraphics(Control control)
        {
            try
            {
                using (Graphics g = control.CreateGraphics())
                {
                    return (int)g.DpiX;
                }
            }
            catch
            {
                return GetSystemDpi();
            }
        }

        #endregion
    }
}
