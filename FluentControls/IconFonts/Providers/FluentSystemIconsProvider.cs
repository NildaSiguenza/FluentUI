using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{

    /// <summary>
    /// FluentSystemIconsRegular
    /// 字体族: FluentSystemIconsRegular
    /// </summary>
    public class FluentSystemIconsRegularIconProvider : IconFontProviderBase
    {
        private static readonly Dictionary<string, string> iconMapping = InitializeMapping();

        public override string FontFamilyName => "FluentSystemIconsRegular";
        public override string DisplayName => "FluentSystemIconsRegular";

        public FluentSystemIconsRegularIconProvider(string fontFilePath)
            : base(fontFilePath, iconMapping)
        {
        }

        public override Type GetIconEnumType()
        {
            return typeof(FluentSystemIconsRegularIconChar);
        }

        private static Dictionary<string, string> InitializeMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 通过枚举自动生成映射
            foreach (FluentSystemIconsRegularIconChar icon in Enum.GetValues(typeof(FluentSystemIconsRegularIconChar)))
            {
                var name = GetEnumName(icon);
                var unicode = char.ConvertFromUtf32((int)icon);
                mapping[name] = unicode;
            }

            return mapping;
        }

        private static string GetEnumName(FluentSystemIconsRegularIconChar icon)
        {
            // 将枚举名称转换为 kebab-case
            var name = icon.ToString();
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
        }

    }

    /// <summary>
    /// FluentSystemIconsResizable
    /// 字体族: FluentSystemIconsResizable
    /// </summary>
    public class FluentSystemIconsResizableIconProvider : IconFontProviderBase
    {
        private static readonly Dictionary<string, string> iconMapping = InitializeMapping();

        public override string FontFamilyName => "FluentSystemIconsResizable";
        public override string DisplayName => "FluentSystemIconsResizable";

        public FluentSystemIconsResizableIconProvider(string fontFilePath)
            : base(fontFilePath, iconMapping)
        {
        }

        public override Type GetIconEnumType()
        {
            return typeof(FluentSystemIconsResizableIconChar);
        }

        private static Dictionary<string, string> InitializeMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 通过枚举自动生成映射
            foreach (FluentSystemIconsResizableIconChar icon in Enum.GetValues(typeof(FluentSystemIconsResizableIconChar)))
            {
                var name = GetEnumName(icon);
                var unicode = char.ConvertFromUtf32((int)icon);
                mapping[name] = unicode;
            }

            return mapping;
        }

        private static string GetEnumName(FluentSystemIconsResizableIconChar icon)
        {
            // 将枚举名称转换为 kebab-case
            var name = icon.ToString();
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
        }

    }

}
