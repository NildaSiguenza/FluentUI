using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    public class RemixiconIconProvider : IconFontProviderBase
    {
        private static readonly Dictionary<string, string> iconMapping = InitializeMapping();

        public override string FontFamilyName => "remixicon";
        public override string DisplayName => "Remixicon";

        public RemixiconIconProvider(string fontFilePath)
            : base(fontFilePath, iconMapping)
        {
        }

        public override Type GetIconEnumType()
        {
            return typeof(RemixiconIconChar);
        }

        private static Dictionary<string, string> InitializeMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 通过枚举自动生成映射
            foreach (RemixiconIconChar icon in Enum.GetValues(typeof(RemixiconIconChar)))
            {
                var name = GetEnumName(icon);
                var unicode = char.ConvertFromUtf32((int)icon);
                mapping[name] = unicode;
            }
            ;

            return mapping;
        }

        private static string GetEnumName(RemixiconIconChar icon)
        {
            // 将枚举名称转换为 kebab-case
            var name = icon.ToString();
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
        }
    }

}
