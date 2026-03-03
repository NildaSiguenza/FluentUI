using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    /// <summary>
    /// DPI 感知模式
    /// </summary>
    public static class DpiAwarenessHelper
    {
        #region Native Methods

        // Windows 10 1703+
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetProcessDpiAwarenessContext(IntPtr value);

        // Windows 8.1+
        [DllImport("shcore.dll", SetLastError = true)]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        // Windows Vista+
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        #endregion

        #region Enums

        private enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }

        // DPI_AWARENESS_CONTEXT values
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_UNAWARE = new IntPtr(-1);
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = new IntPtr(-2);
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = new IntPtr(-3);
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = new IntPtr(-5);

        #endregion

        /// <summary>
        /// 设置进程 DPI 感知(应在 Main 函数最开始调用)
        /// </summary>
        /// <param name="perMonitorV2">是否使用 Per-Monitor V2(Windows 10 1703+)</param>
        /// <returns>是否设置成功</returns>
        public static bool SetDpiAwareness(bool perMonitorV2 = true)
        {
            try
            {
                // 尝试 Windows 10 1703+ API
                IntPtr user32 = GetModuleHandle("user32.dll");
                if (user32 != IntPtr.Zero &&
                    GetProcAddress(user32, "SetProcessDpiAwarenessContext") != IntPtr.Zero)
                {
                    var context = perMonitorV2
                        ? DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2
                        : DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE;

                    if (SetProcessDpiAwarenessContext(context) != 0)
                    {
                        return true;
                    }

                    // 如果 V2 失败, 尝试 V1
                    if (perMonitorV2)
                    {
                        if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE) != 0)
                        {
                            return true;
                        }
                    }
                }

                // 尝试 Windows 8.1+ API
                IntPtr shcore = LoadLibrary("shcore.dll");
                if (shcore != IntPtr.Zero &&
                    GetProcAddress(shcore, "SetProcessDpiAwareness") != IntPtr.Zero)
                {
                    int hr = SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);
                    if (hr == 0) // S_OK
                    {
                        return true;
                    }
                }

                // Windows Vista/7 - 使用基本的 DPI 感知
                return SetProcessDPIAware();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 设置系统级 DPI 感知(不支持 Per-Monitor)
        /// </summary>
        public static bool SetSystemDpiAwareness()
        {
            try
            {
                return SetProcessDPIAware();
            }
            catch
            {
                return false;
            }
        }
    }
}
