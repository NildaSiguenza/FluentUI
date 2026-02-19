using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    public class MynauiIconProvider : IconFontProviderBase
    {
        private static readonly Dictionary<string, string> iconMapping = InitializeMapping();

        public override string FontFamilyName => "mynaui";
        public override string DisplayName => "Mynaui";

        public MynauiIconProvider(string fontFilePath)
            : base(fontFilePath, iconMapping)
        {
        }

        public override Type GetIconEnumType()
        {
            return typeof(MynauiIconChar);
        }


        private static Dictionary<string, string> InitializeMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 通过枚举自动生成映射
            foreach (MynauiIconChar icon in Enum.GetValues(typeof(MynauiIconChar)))
            {
                var name = GetEnumName(icon);
                var unicode = char.ConvertFromUtf32((int)icon);
                mapping[name] = unicode;
            }

            return mapping;
        }

        private static string GetEnumName(MynauiIconChar icon)
        {
            // 将枚举名称转换为 kebab-case
            var name = icon.ToString();
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
        }
    }

    public class MynauiSolidIconProvider : IconFontProviderBase
    {
        private static readonly Dictionary<string, string> iconMapping = InitializeMapping();

        public override string FontFamilyName => "mynaui-solid";
        public override string DisplayName => "MynauiSolid";

        public MynauiSolidIconProvider(string fontFilePath)
            : base(fontFilePath, iconMapping)
        {
        }

        public override Type GetIconEnumType()
        {
            return typeof(MynauiSolidIconChar);
        }

        private static Dictionary<string, string> InitializeMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 通过枚举自动生成映射
            foreach (MynauiSolidIconChar icon in Enum.GetValues(typeof(MynauiSolidIconChar)))
            {
                var name = GetEnumName(icon);
                var unicode = char.ConvertFromUtf32((int)icon);
                mapping[name] = unicode;
            }
            ;

            return mapping;
        }

        private static string GetEnumName(MynauiSolidIconChar icon)
        {
            // 将枚举名称转换为 kebab-case
            var name = icon.ToString();
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
        }
    }

}
