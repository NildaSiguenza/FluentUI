using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls
{
    public static class FluentControlExt
    {
        public static T FindChild<T>(this Control parent) where T : Control
        {
            if (parent == null)
            {
                return null;
            }

            // 先从直接子级寻找
            foreach (Control child in parent.Controls)
            {
                if (child is T tChild)
                {
                    return tChild;
                }
            }

            foreach (Control child in parent.Controls)
            {
                var result = FindChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static IEnumerable<T> FindChildren<T>(this Control parent) where T : Control
        {
            if (parent == null)
            {
                yield break;
            }
            foreach (Control child in parent.Controls)
            {
                if (child is T tChild)
                {
                    yield return tChild;
                }
                foreach (var descendant in FindChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }


    }
}
