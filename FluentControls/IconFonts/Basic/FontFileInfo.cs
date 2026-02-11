using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{

    /// <summary>
    /// 字体文件信息
    /// </summary>
    public class FontFileInfo
    {
        public string FontFamily { get; set; }
        public string FilePath { get; set; }
        public int GlyphCount { get; set; }
        public ushort UnitsPerEm { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
    }

    /// <summary>
    /// 字形信息
    /// </summary>
    public class GlyphInfo
    {
        public int Unicode { get; set; }
        public string UnicodeHex { get; set; }
        public string UnicodeChar { get; set; }
        public ushort GlyphIndex { get; set; }
        public string GlyphName { get; set; }
        public string DefaultName { get; set; }
        public string FriendlyName { get; set; }
    }

}
