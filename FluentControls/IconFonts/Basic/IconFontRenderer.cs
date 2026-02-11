using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体图标工具类
    /// (将字体字符转换为Image)
    /// </summary>
    public static class IconFontRenderer
    {
        /// <summary>
        /// 将字体图标转换为Image
        /// </summary>
        public static Image ToImage(string unicode, Font font, Color color, Size size, float rotation = 0)
        {
            if (string.IsNullOrEmpty(unicode) || font == null)
            {
                return null;
            }

            size = new Size((int)(size.Width * 0.9f), (int)(size.Height * 0.9f));
            var bitmap = new Bitmap(size.Width, size.Height);

            var previewFont = new Font(font.FontFamily, size.Height * 0.7f, FontStyle.Regular);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 应用旋转
                if (rotation != 0)
                {
                    g.TranslateTransform(size.Width / 2f, size.Height / 2f);
                    g.RotateTransform(rotation);
                    g.TranslateTransform(-size.Width / 2f, -size.Height / 2f);
                }

                var textRect = new Rectangle(0, 0, size.Width, size.Height);
                // 计算文本大小
                var textSize = TextRenderer.MeasureText(g, unicode, previewFont, size, TextFormatFlags.NoPadding);
                // 居中
                var x = (size.Width - textSize.Width) / 2;
                var y = (size.Height - textSize.Height) / 2;
                var drawRect = new Rectangle(x, y, textSize.Width, textSize.Height);

                try
                {
                    using (var brush = new SolidBrush(color))
                    {
                        var format = new StringFormat()
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        g.DrawString(unicode, previewFont, brush, drawRect, format);
                    }
                }
                catch (Exception ex)
                {
                    // 回退方案：使用 TextRenderer
                    TextRenderer.DrawText(g, unicode, previewFont, drawRect, color, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// 从字体文件创建Font对象
        /// </summary>
        public static Font CreateFontFromFile(string fontFilePath, float size)
        {
            if (!File.Exists(fontFilePath))
            {
                throw new FileNotFoundException($"字体文件不存在: {fontFilePath}");
            }

            var collection = new PrivateFontCollection();
            collection.AddFontFile(fontFilePath);

            if (collection.Families.Length == 0)
            {
                throw new InvalidOperationException($"无法从文件加载字体: {fontFilePath}");
            }

            return new Font(collection.Families[0], size, FontStyle.Regular);
        }

        /// <summary>
        /// 解析字体文件获取字符映射
        /// </summary>
        public static Dictionary<string, string> ParseFontFile(string fontFilePath)
        {
            // 这里需要使用第三方库如 Typography.OpenFont 来解析字体文件
            // 简化实现，返回空字典
            var mapping = new Dictionary<string, string>();

            // TODO: 实现实际的字体文件解析逻辑
            // 可以使用 Typography.OpenFont 或其他字体解析库

            return mapping;
        }
    }

}
