using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.OpenFont;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体文件解析器
    /// </summary>
    public class FontFileParser
    {
        /// <summary>
        /// 从字体文件中提取所有字符码点
        /// </summary>
        public static List<GlyphInfo> ExtractGlyphs(string fontFilePath)
        {
            var glyphs = new List<GlyphInfo>();

            try
            {
                using (var fs = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    var typeface = reader.Read(fs);

                    if (typeface == null)
                    {
                        return glyphs;
                    }

                    // 直接使用 GetGlyphIndex 方法遍历 Unicode 范围
                    var unicodeRanges = new[]
                    {
                        // 基本拉丁字母
                        new { Start = 0x0020, End = 0x007F },
                        // 拉丁补充
                        new { Start = 0x00A0, End = 0x00FF },
                        // 私有使用区 (PUA) - 图标字体常用
                        new { Start = 0xE000, End = 0xF8FF },
                        // 补充私有使用区 A
                        new { Start = 0xF0000, End = 0xFFFFD },
                        // 常用符号
                        new { Start = 0x2000, End = 0x2BFF },
                        // 数学运算符
                        new { Start = 0x2200, End = 0x22FF },
                        // 其他符号
                        new { Start = 0x2600, End = 0x26FF },
                        // 装饰符号
                        new { Start = 0x2700, End = 0x27BF },
                        // 杂项符号
                        new { Start = 0x2B00, End = 0x2BFF },
                    };

                    var foundGlyphs = new Dictionary<int, ushort>();

                    foreach (var range in unicodeRanges)
                    {
                        for (int unicode = range.Start; unicode <= range.End; unicode++)
                        {
                            try
                            {
                                ushort glyphIndex = typeface.GetGlyphIndex(unicode);

                                // 0 表示字形不存在
                                if (glyphIndex > 0 && !foundGlyphs.ContainsKey(unicode))
                                {
                                    foundGlyphs[unicode] = glyphIndex;
                                }
                            }
                            catch
                            {
                                // 忽略无效的 Unicode 码点
                            }
                        }
                    }

                    // 转换为 GlyphInfo 列表
                    foreach (var kvp in foundGlyphs.OrderBy(k => k.Key))
                    {
                        var unicode = kvp.Key;
                        var glyphIndex = kvp.Value;

                        glyphs.Add(new GlyphInfo
                        {
                            Unicode = unicode,
                            UnicodeHex = $"{unicode:X4}",
                            UnicodeChar = char.ConvertFromUtf32(unicode),
                            GlyphIndex = glyphIndex,
                            GlyphName = $"glyph_{glyphIndex}",
                            DefaultName = GenerateDefaultName(unicode)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"解析字体文件失败: {ex.Message}");
            }

            return glyphs;
        }

        /// <summary>
        /// 生成默认图标名称
        /// </summary>
        private static string GenerateDefaultName(int unicode)
        {
            return $"icon_{unicode:X4}".ToLower();
        }

        /// <summary>
        /// 从字体文件提取基本信息
        /// </summary>
        public static FontFileInfo GetFontInfo(string fontFilePath)
        {
            try
            {
                using (var fs = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    var typeface = reader.Read(fs);

                    if (typeface == null)
                    {
                        return null;
                    }

                    return new FontFileInfo
                    {
                        FontFamily = typeface.Name,
                        FilePath = fontFilePath,
                        GlyphCount = typeface.GlyphCount,
                        UnitsPerEm = typeface.UnitsPerEm,
                        IsBold = GetIsBold(typeface),
                        IsItalic = GetIsItalic(typeface)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取字体信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 判断是否为粗体
        /// </summary>
        private static bool GetIsBold(Typeface typeface)
        {
            try
            {
                // 从 OS2Table 的 usWeightClass 判断
                if (typeface.OS2Table != null)
                {
                    // usWeightClass >= 600 通常认为是粗体
                    return typeface.OS2Table.usWeightClass >= 600;
                }

                // 或者从名称中判断
                var name = typeface.Name?.ToLower() ?? "";
                return name.Contains("bold") || name.Contains("black") || name.Contains("heavy");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否为斜体
        /// </summary>
        private static bool GetIsItalic(Typeface typeface)
        {
            try
            {
                // 从 OS2Table 的 fsSelection 判断
                if (typeface.OS2Table != null)
                {
                    // fsSelection 的第 0 位表示斜体
                    var fsSelection = typeface.OS2Table.fsSelection;
                    return (fsSelection & 0x01) != 0;
                }

                // 或者从名称中判断
                var name = typeface.Name?.ToLower() ?? "";
                return name.Contains("italic") || name.Contains("oblique");
            }
            catch
            {
                return false;
            }
        }

    }
}
