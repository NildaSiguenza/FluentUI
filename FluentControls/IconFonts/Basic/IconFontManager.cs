using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System.Reflection;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体图标管理器
    /// </summary>
    public class IconFontManager
    {
        private static IconFontManager instance;
        private static readonly object lockObj = new object();

        private Dictionary<string, IIconFontProvider> providers;
        private Dictionary<string, Type> enumTypes;
        private string configFilePath;


        private IconFontManager()
        {
            providers = new Dictionary<string, IIconFontProvider>();
            enumTypes = new Dictionary<string, Type>();
            configFilePath = Path.Combine(GetDefaultIconFontsFolder(), "config.json");

            LoadConfiguration();
        }

        public static IconFontManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new IconFontManager();
                        }
                    }
                }
                return instance;
            }
        }

        public string GetDefaultIconFontsFolder()
        {
            return Path.Combine(Application.StartupPath, "IconFonts");
        }

        /// <summary>
        /// 注册字体图标提供者
        /// </summary>
        public void RegisterProvider(IIconFontProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            providers[provider.FontFamilyName] = provider;

            var enumType = provider.GetIconEnumType();
            if (enumType != null)
            {
                enumTypes[provider.FontFamilyName] = enumType;
            }
        }

        /// <summary>
        /// 手动注册Provider(启动时调用)
        /// </summary>
        public void RegisterProvider<TProvider>(string fontFilePath) where TProvider : IconFontProviderBase
        {
            try
            {
                var fullPath = Path.Combine(GetDefaultIconFontsFolder(), fontFilePath);
                var provider = (IIconFontProvider)Activator.CreateInstance(typeof(TProvider), fullPath);
                RegisterProvider(provider);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"注册Provider失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取提供者
        /// </summary>
        public IIconFontProvider GetProvider(string fontFamily)
        {
            if (providers.TryGetValue(fontFamily, out var provider))
            {
                return provider;
            }
            return null;
        }

        /// <summary>
        /// 获取枚举类型
        /// </summary>
        public Type GetIconEnumType(string fontFamily)
        {
            if (enumTypes.TryGetValue(fontFamily, out var enumType))
            {
                return enumType;
            }
            return null;
        }

        /// <summary>
        /// 获取所有提供者
        /// </summary>
        public IEnumerable<IIconFontProvider> GetAllProviders()
        {
            return providers.Values;
        }

        /// <summary>
        /// 通过枚举获取图标
        /// </summary>
        public Image GetIcon(string fontFamily, Enum iconEnum, float size, Color color, float rotation = 0)
        {
            var provider = GetProvider(fontFamily);
            return provider?.GetIcon(iconEnum, size, color, rotation);
        }

        /// <summary>
        /// 通过名称获取图标
        /// </summary>
        public Image GetIcon(string fontFamily, string iconName, float size, Color color, float rotation = 0)
        {
            var provider = GetProvider(fontFamily);
            return provider?.GetIcon(iconName, size, color, rotation);
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfiguration()
        {
            if (!File.Exists(configFilePath))
            {
                CreateDefaultConfiguration();
                return;
            }

            try
            {
                var json = File.ReadAllText(configFilePath);
                var config = JsonConvert.DeserializeObject<IconFontConfig>(json);

                foreach (var providerConfig in config.Providers)
                {
                    if (!providerConfig.Enabled)
                    {
                        continue;
                    }

                    try
                    {
                        // 动态加载Provider类型
                        var assembly = string.IsNullOrEmpty(providerConfig.AssemblyName)
                            ? Assembly.GetExecutingAssembly()
                            : Assembly.Load(providerConfig.AssemblyName);

                        var providerType = assembly.GetType(providerConfig.ProviderTypeName);
                        if (providerType == null)
                        {
                            Debug.WriteLine($"找不到Provider类型: {providerConfig.ProviderTypeName}");
                            continue;
                        }

                        var fontFilePath = Path.Combine(GetDefaultIconFontsFolder(), providerConfig.FontFilePath);
                        if (!File.Exists(fontFilePath))
                        {
                            Debug.WriteLine($"字体文件不存在: {fontFilePath}");
                            continue;
                        }

                        var provider = (IIconFontProvider)Activator.CreateInstance(providerType, fontFilePath);
                        RegisterProvider(provider);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"加载Provider失败 {providerConfig.ProviderTypeName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载字体图标配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        //public void SaveConfiguration()
        //{
        //    var config = new IconFontConfig();

        //    foreach (var provider in providers.Values)
        //    {
        //        var mapping = new IconFontMapping
        //        {
        //            FontFamily = provider.FontFamilyName,
        //            DisplayName = provider.DisplayName,
        //            Icons = provider.GetIconMapping()
        //        };

        //        config.FontMappings.Add(mapping);
        //    }

        //    var json = JsonConvert.SerializeObject(config, Formatting.Indented);

        //    var directory = Path.GetDirectoryName(configFilePath);
        //    if (!Directory.Exists(directory))
        //    {
        //        Directory.CreateDirectory(directory);
        //    }

        //    File.WriteAllText(configFilePath, json);
        //}

        /// <summary>
        /// 创建默认配置
        /// </summary>
        private void CreateDefaultConfiguration()
        {
            var directory = Path.GetDirectoryName(configFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new IconFontConfig
            {
                Providers = new List<IconFontProviderConfig>
                {
                    new IconFontProviderConfig
                    {
                        ProviderTypeName = "FluentControls.Icons.FontAwesomeFreeSolidIconProvider",
                        AssemblyName = "FluentControls",
                        FontFilePath = "Fonts/FontAwesome/Font Awesome 7 Free-Solid-900.otf",
                        Enabled = true
                    }
                }
            };

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFilePath, json);
        }

        /// <summary>
        /// 从文件添加字体图标库
        /// </summary>
        public bool AddFontFromFile(string fontFilePath, string displayName, Dictionary<string, string> iconMapping)
        {
            try
            {
                if (!File.Exists(fontFilePath))
                {
                    return false;
                }

                // 复制字体文件到应用目录
                var fontDirectory = Path.Combine(GetDefaultIconFontsFolder(), "Fonts");
                if (!Directory.Exists(fontDirectory))
                {
                    Directory.CreateDirectory(fontDirectory);
                }

                var fontFileName = Path.GetFileName(fontFilePath);
                var targetPath = Path.Combine(fontDirectory, fontFileName);

                if (!File.Exists(targetPath))
                {
                    File.Copy(fontFilePath, targetPath);
                }

                // 从字体文件获取字体名称
                using (var collection = new PrivateFontCollection())
                {
                    collection.AddFontFile(targetPath);
                    var fontFamily = collection.Families[0].Name;

                    var provider = new DynamicIconFontProvider(
                        fontFamily,
                        displayName,
                        targetPath,
                        iconMapping);

                    RegisterProvider(provider);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"添加字体文件失败: {ex.Message}");
                return false;
            }
        }
    }

}
