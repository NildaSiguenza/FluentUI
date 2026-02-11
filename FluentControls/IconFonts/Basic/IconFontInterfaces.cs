using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体图标字符接口
    /// </summary>
    public interface IIconFontChar
    {
        string Unicode { get; }
        string Name { get; }
        string FontFamily { get; }
    }

    /// <summary>
    /// 字体图标提供者接口
    /// </summary>
    public interface IIconFontProvider : IDisposable
    {
        string FontFamilyName { get; }
        string DisplayName { get; }
        Font GetFont(float size);
        Dictionary<string, string> GetIconMapping();
        Type GetIconEnumType();
        Image GetIcon(string iconName, float size, Color color, float rotation = 0);
        Image GetIcon(Enum iconEnum, float size, Color color, float rotation = 0);
        string GetUnicode(Enum iconEnum);
        string GetIconName(Enum iconEnum);
    }
}
