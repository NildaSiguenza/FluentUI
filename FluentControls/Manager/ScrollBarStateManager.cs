using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Manager
{
    public static class ScrollBarStateManager
    {
        private static Dictionary<Control, ScrollBarState> states = new Dictionary<Control, ScrollBarState>();

        private class ScrollBarState
        {
            public bool AutoScroll { get; set; }

            public Size AutoScrollMinSize { get; set; }

            public Point AutoScrollPosition { get; set; }

            public bool HScrollVisible { get; set; }

            public bool VScrollVisible { get; set; }
        }

        /// <summary>
        /// 锁定控件的滚动条状态
        /// </summary>
        public static void LockScrollBars(Control control)
        {
            if (control == null)
            {
                return;
            }

            if (control is ScrollableControl scrollable)
            {
                var state = new ScrollBarState
                {
                    AutoScroll = scrollable.AutoScroll,
                    AutoScrollMinSize = scrollable.AutoScrollMinSize,
                    AutoScrollPosition = scrollable.AutoScrollPosition,
                    HScrollVisible = scrollable.HorizontalScroll.Visible,
                    VScrollVisible = scrollable.VerticalScroll.Visible
                };

                states[control] = state;

                // 临时禁用自动滚动
                scrollable.AutoScroll = false;
            }
        }

        /// <summary>
        /// 解锁并恢复控件的滚动条状态
        /// </summary>
        public static void UnlockScrollBars(Control control)
        {
            if (control == null)
            {
                return;
            }

            if (control is ScrollableControl scrollable && states.ContainsKey(control))
            {
                var state = states[control];

                // 恢复状态
                scrollable.AutoScroll = state.AutoScroll;
                scrollable.AutoScrollMinSize = state.AutoScrollMinSize;
                scrollable.AutoScrollPosition = state.AutoScrollPosition;

                states.Remove(control);
            }
        }

        /// <summary>
        /// 在动作执行期间锁定滚动条
        /// </summary>
        public static void ExecuteWithLockedScrollBars(Control control, Action action)
        {
            if (control == null || action == null)
            {
                return;
            }

            try
            {
                LockScrollBars(control);
                action();
            }
            finally
            {
                UnlockScrollBars(control);
            }
        }
    }

}
