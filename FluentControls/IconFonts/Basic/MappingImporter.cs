using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 映射文件导入器
    /// </summary>
    public class MappingImporter
    {
        /// <summary>
        /// 从JSON导入
        /// </summary>
        public static Dictionary<string, string> ImportFromJson(string jsonContent)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON导入失败: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 从CSV导入
        /// </summary>
        public static Dictionary<string, string> ImportFromCsv(string csvContent)
        {
            var mapping = new Dictionary<string, string>();

            try
            {
                var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        var name = parts[0].Trim();
                        var unicode = parts[1].Trim();
                        mapping[name] = unicode;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CSV导入失败: {ex.Message}");
            }

            return mapping;
        }

        /// <summary>
        /// 从CSS解析(Font Awesome格式)
        /// </summary>
        public static Dictionary<string, string> ImportFromCss(string cssContent)
        {
            var mapping = new Dictionary<string, string>();

            try
            {
                // 正则匹配 .fa-home:before { content: "\f015"; }
                var regex = new Regex(@"\.fa-([a-z0-9-]+):before\s*\{\s*content:\s*[""']\\([0-9a-fA-F]+)[""']");
                var matches = regex.Matches(cssContent);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var name = match.Groups[1].Value;
                        var unicode = $"\\u{match.Groups[2].Value}";
                        mapping[name] = ConvertUnicodeString(unicode);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CSS解析失败: {ex.Message}");
            }

            return mapping;
        }

        /// <summary>
        /// 从C#枚举代码解析
        /// </summary>
        public static Dictionary<string, string> ImportFromEnumCode(string code, Dictionary<string, string> unicodeMapping)
        {
            var mapping = new Dictionary<string, string>();

            try
            {
                // 解析枚举定义
                var regex = new Regex(@"([A-Za-z_][A-Za-z0-9_]*)\s*=\s*0x([0-9a-fA-F]+)");
                var matches = regex.Matches(code);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var name = match.Groups[1].Value;
                        var hex = match.Groups[2].Value;
                        var unicode = $"\\u{hex}";
                        mapping[name] = ConvertUnicodeString(unicode);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"枚举代码解析失败: {ex.Message}");
            }

            return mapping;
        }

        /// <summary>
        /// 转换Unicode字符串
        /// </summary>
        private static string ConvertUnicodeString(string unicode)
        {
            if (unicode.StartsWith("\\u"))
            {
                var hex = unicode.Substring(2);
                var codePoint = Convert.ToInt32(hex, 16);
                return char.ConvertFromUtf32(codePoint);
            }
            return unicode;
        }

        /// <summary>
        /// 导出为JSON
        /// </summary>
        public static string ExportToJson(Dictionary<string, string> mapping)
        {
            return JsonConvert.SerializeObject(mapping, Formatting.Indented);
        }

        /// <summary>
        /// 导出为CSV
        /// </summary>
        public static string ExportToCsv(Dictionary<string, string> mapping)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,Unicode");

            foreach (var kvp in mapping)
            {
                var unicode = string.Join("", kvp.Value.Select(c => $"\\u{((int)c):X4}"));
                sb.AppendLine($"{kvp.Key},{unicode}");
            }

            return sb.ToString();
        }
    }
}
