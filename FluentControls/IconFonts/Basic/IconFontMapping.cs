using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体图标映射信息
    /// </summary>
    [Serializable]
    public class IconFontMapping
    {
        public string FontFamily { get; set; }

        public string DisplayName { get; set; }

        public string FontFilePath { get; set; }

        public Dictionary<string, string> Icons { get; set; } = new Dictionary<string, string>();

        public string EnumTypeName { get; set; }
    }

    /// <summary>
    /// 字体图标配置
    /// </summary>
    [Serializable]
    public class IconFontConfig
    {
        public List<IconFontProviderConfig> Providers { get; set; } = new List<IconFontProviderConfig>();
    }

    [Serializable]
    public class IconFontProviderConfig
    {
        /// <summary>
        /// 完整类型名称，如 "MyApp.Icons.FontAwesomeIconProvider"
        /// </summary>
        public string ProviderTypeName { get; set; } 

        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName { get; set; } 

        /// <summary>
        /// 相对于 IconFonts 目录的路径
        /// </summary>
        public string FontFilePath { get; set; } 

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }


    public class IconEnumItem
    {
        public Enum EnumValue { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unicode { get; set; }

        public override string ToString() => Name;
    }

}
