using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    public static class StyleExt
    {
        public static bool HasValue(this Font font)
        {
            if (font == null)
            {
                return false;
            }

            // 检查是否是系统默认字体
            Font defaultFont = SystemFonts.DefaultFont;
            return !(font.Name == defaultFont.Name &&
                     Math.Abs(font.Size - defaultFont.Size) < 0.01f &&
                     font.Style == defaultFont.Style);
        }
    }
}
