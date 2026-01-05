using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public static class FluentFormExt
    {

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;

        // 控制圆角形态 (0=默认，1=无圆角，2=标准圆角，3=小圆角)
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        // 控制非客户区渲染 (0=默认，1=强制开启，2=强制关闭)
        private const int DWMWA_NCRENDERING_POLICY = 2;
        // 控制暗色标题栏 (0=禁用，1=启用)
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        // 自定义边框颜色 (RGB 颜色值，0xFFFFFFFF 恢复默认)
        private const int DWMWA_BORDER_COLOR = 34;
        // 自定义标题栏颜色
        private const int DWMWA_CAPTION_COLOR = 35;
        // 自定义标题文字颜色
        private const int DWMWA_TEXT_COLOR = 36;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);


        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        public static int GWL_EXSTYLE_INDEX => GWL_EXSTYLE;

        public static void ToggleTaskbarVisibility(this FluentForm form, bool show)
        {
            IntPtr style = GetWindowLongPtr(form.Handle, GWL_EXSTYLE);
            long newStyle = style.ToInt64();

            if (show)
            {
                newStyle |= WS_EX_APPWINDOW;
                newStyle &= ~WS_EX_TOOLWINDOW;
            }
            else
            {
                newStyle &= ~WS_EX_APPWINDOW;
                newStyle |= WS_EX_TOOLWINDOW;
            }

            SetWindowLongPtr(form.Handle, GWL_EXSTYLE, (IntPtr)newStyle);

            // 通知窗口样式已更改，让 DWM 重新绘制(刷新阴影、圆角等)
            SetWindowPos(form.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        /// <summary>
        /// 设置为应用窗口
        /// </summary>
        public static void SetAppWindow(this FluentForm form, bool enable)
        {
            IntPtr style = GetWindowLongPtr(form.Handle, GWL_EXSTYLE);
            long newStyle = style.ToInt64();

            if (enable)
            {
                newStyle |= WS_EX_APPWINDOW;
                newStyle &= ~WS_EX_TOOLWINDOW;
            }
            else
            {
                newStyle &= ~WS_EX_APPWINDOW;
                newStyle |= WS_EX_TOOLWINDOW;
            }

            SetWindowLongPtr(form.Handle, GWL_EXSTYLE, (IntPtr)newStyle);
            RefreshNonClientArea(form.Handle);
        }

        private static void RefreshNonClientArea(IntPtr hWnd)
        {
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
        }

        /// <summary>
        /// 设置父窗口
        /// </summary>
        public static void SetParent(this FluentForm form, Form parentForm)
        {
            if (form != null)
            {
                form.Owner = parentForm;
            }
        }

        /// <summary>
        /// 设置圆角
        /// </summary>
        public static void SetWindowCorner(this FluentForm form, DwmWindowCorner corner)
        {
            int c = (int)corner;
            DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref c, sizeof(int));
            form.Invalidate();
        }

        /// <summary>
        /// 设置阴影
        /// </summary>
        public static void SetNcRendering(this FluentForm form, DwmNcRenderingPolicy policy)
        {
            int p = (int)policy;
            DwmSetWindowAttribute(form.Handle, DWMWA_NCRENDERING_POLICY, ref p, sizeof(int));
            form.Invalidate();
        }

        /// <summary>
        /// 自定义边框颜色
        /// </summary>
        public static void SetBorderColor(this FluentForm form, uint argb)
        {
            int color = unchecked((int)argb);
            DwmSetWindowAttribute(form.Handle, DWMWA_BORDER_COLOR, ref color, sizeof(int));
            form.Invalidate();
        }

        /// <summary>
        /// 启用暗色标题栏
        /// </summary>
        public static void EnableDarkTitleBar(this FluentForm form, bool enable)
        {
            int val = enable ? 1 : 0;
            DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref val, sizeof(int));
            form.Invalidate();
        }

    }


    public enum DwmWindowCorner
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    public enum DwmNcRenderingPolicy
    {
        Default = 0,
        Enabled = 1,
        Disabled = 2
    }
}
