using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体图标提供者基类
    /// </summary>
    public abstract class IconFontProviderBase : IIconFontProvider
    {
        private readonly string fontFilePath;
        private readonly Dictionary<string, string> iconMapping;
        private PrivateFontCollection fontCollection;
        private FontFamily fontFamily;

        public abstract string FontFamilyName { get; }
        public abstract string DisplayName { get; }

        protected IconFontProviderBase(string fontFilePath, Dictionary<string, string> iconMapping)
        {
            this.fontFilePath = fontFilePath;
            this.iconMapping = iconMapping ?? new Dictionary<string, string>();
            InitializeFont();
        }

        private void InitializeFont()
        {
            if (!File.Exists(fontFilePath))
            {
                throw new FileNotFoundException($"字体文件不存在: {fontFilePath}");
            }

            fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fontFilePath);

            if (fontCollection.Families.Length > 0)
            {
                fontFamily = fontCollection.Families[0];
            }
            else
            {
                throw new InvalidOperationException($"无法从文件加载字体: {fontFilePath}");
            }
        }

        public virtual Font GetFont(float size)
        {
            if (fontFamily == null)
            {
                throw new InvalidOperationException("字体未初始化");
            }

            return new Font(fontFamily, size, FontStyle.Regular);
        }

        public virtual Dictionary<string, string> GetIconMapping()
        {
            return new Dictionary<string, string>(iconMapping);
        }

        public abstract Type GetIconEnumType();

        public virtual Image GetIcon(string iconName, float size, Color color, float rotation = 0)
        {
            if (!iconMapping.TryGetValue(iconName, out string unicode))
            {
                return null;
            }

            var font = GetFont(size);
            var imageSize = new Size((int)size, (int)size);

            return IconFontRenderer.ToImage(unicode, font, color, imageSize, rotation);
        }

        /// <summary>
        /// 通过枚举值获取图标
        /// </summary>
        public virtual Image GetIcon(Enum iconEnum, float size, Color color, float rotation = 0)
        {
            var unicode = char.ConvertFromUtf32((int)(object)iconEnum);
            var font = GetFont(size);
            var imageSize = new Size((int)size, (int)size);

            return IconFontRenderer.ToImage(unicode, font, color, imageSize, rotation);
        }

        /// <summary>
        /// 获取枚举值对应的Unicode字符
        /// </summary>
        public virtual string GetUnicode(Enum iconEnum)
        {
            return char.ConvertFromUtf32((int)(object)iconEnum);
        }

        /// <summary>
        /// 获取枚举值对应的名称
        /// </summary>
        public virtual string GetIconName(Enum iconEnum)
        {
            return iconEnum.ToString();
        }

        public void Dispose()
        {
            fontCollection?.Dispose();
        }
    }

    /// <summary>
    /// 动态字体图标提供者
    /// </summary>
    public class DynamicIconFontProvider : IconFontProviderBase
    {
        private string fontFamilyName;
        private string displayName;

        public DynamicIconFontProvider(string fontFamily, string displayName, string fontFilePath, Dictionary<string, string> mapping)
            : base(fontFilePath, mapping)
        {
            this.fontFamilyName = fontFamily;
            this.displayName = displayName;
        }

        public override string FontFamilyName => fontFamilyName;
        public override string DisplayName => displayName;


        public override Type GetIconEnumType()
        {
            // 动态提供者没有预定义的枚举类型
            return null;
        }
    }

}
