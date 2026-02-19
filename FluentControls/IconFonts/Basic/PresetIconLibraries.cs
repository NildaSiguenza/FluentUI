using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 预设图标库管理器
    /// </summary>
    public class PresetIconLibraries
    {
        /// <summary>
        /// 获取Font Awesome映射
        /// </summary>
        public static Dictionary<string, string> GetFontAwesomeMapping()
        {
            // 这里可以内置一些常用的映射，或者从资源文件加载
            return new Dictionary<string, string>
            {
                { "home", "\uf015" },
                { "user", "\uf007" },
                { "cog", "\uf013" },
                { "search", "\uf002" },
                { "envelope", "\uf0e0" },
                { "heart", "\uf004" },
                { "star", "\uf005" },
                { "trash", "\uf1f8" },
                { "edit", "\uf044" },
                { "save", "\uf0c7" }
            };
        }

        /// <summary>
        /// 从GitHub下载映射文件
        /// </summary>
        public static async Task<Dictionary<string, string>> DownloadMappingFromGitHub(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var json = await client.GetStringAsync(url);
                    return MappingImporter.ImportFromJson(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"下载映射文件失败: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 获取预设库列表
        /// </summary>
        public static List<PresetLibraryInfo> GetPresetLibraries()
        {
            return new List<PresetLibraryInfo>
            {
                new PresetLibraryInfo
                {
                    Name = "Font Awesome 5 Free",
                    Url = "https://raw.githubusercontent.com/FortAwesome/Font-Awesome/master/metadata/icons.json",
                    FontUrl = "https://github.com/FortAwesome/Font-Awesome/raw/master/webfonts/fa-solid-900.ttf",
                    Type = "FontAwesome"
                },
                new PresetLibraryInfo
                {
                    Name = "Material Design Icons",
                    Url = "https://raw.githubusercontent.com/Templarian/MaterialDesign/master/meta.json",
                    Type = "Material"
                }
                // 可以添加更多预设库
            };
        }
    }

    /// <summary>
    /// 预设库信息
    /// </summary>
    public class PresetLibraryInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string FontUrl { get; set; }
        public string Type { get; set; }
    }
}
