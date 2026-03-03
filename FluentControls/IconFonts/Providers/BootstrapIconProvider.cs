using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    public class BootstrapIconProvider : IconFontProviderBase
    {
        private static readonly Dictionary<string, string> iconMapping = InitializeMapping();

        public override string FontFamilyName => "bootstrap_icons";
        public override string DisplayName => "Bootstrap";

        public BootstrapIconProvider(string fontFilePath)
            : base(fontFilePath, iconMapping)
        {
        }

        public override Type GetIconEnumType()
        {
            return typeof(BootstrapIconChar);
        }

        private static Dictionary<string, string> InitializeMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 通过枚举自动生成映射
            foreach (BootstrapIconChar icon in Enum.GetValues(typeof(BootstrapIconChar)))
            {
                var name = GetEnumName(icon);
                var unicode = char.ConvertFromUtf32((int)icon);
                mapping[name] = unicode;
            }
            ;

            return mapping;
        }

        private static string GetEnumName(BootstrapIconChar icon)
        {
            // 将枚举名称转换为 kebab-case
            var name = icon.ToString();
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLower();
        }

    }
}
