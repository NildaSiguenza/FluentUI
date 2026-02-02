using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ComponentModel.TypeConverter;

namespace FluentControls.Themes
{
    /// <summary>
    /// 主题管理器
    /// </summary>
    public static class ThemeManager
    {
        private static IFluentTheme currentTheme;
        private static readonly Dictionary<string, IFluentTheme> themes = new Dictionary<string, IFluentTheme>();
        private static bool isThemeSet = false;

        public static event EventHandler ThemeChanged;

        static ThemeManager()
        {
            currentTheme = null;
            isThemeSet = false;

            // 注册默认主题
            RegisterTheme(new ClassicTheme());
            RegisterTheme(new LightTheme());
            RegisterTheme(new DarkTheme());
            RegisterTheme(new HighContrastTheme());
            RegisterTheme(new BlueTheme());
            RegisterTheme(new GreenTheme());
            RegisterTheme(new PurpleTheme());

        }

        public static IFluentTheme CurrentTheme
        {
            get => currentTheme;
            set
            {
                if (currentTheme != value)
                {
                    currentTheme = value;
                    isThemeSet = (value != null);
                    OnThemeChanged();
                }
            }
        }

        public static IFluentTheme DefaultTheme => new LightTheme();

        /// <summary>
        /// 是否已设置主题
        /// </summary>
        public static bool IsThemeSet => isThemeSet;

        public static void RegisterTheme(IFluentTheme theme)
        {
            if (theme != null && !string.IsNullOrEmpty(theme.Name))
            {
                themes[theme.Name] = theme;
            }
        }

        public static IFluentTheme GetTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
            {
                return null;
            }

            themes.TryGetValue(themeName, out var theme);
            return theme;
        }

        public static void ApplyTheme(string themeName)
        {
            CurrentTheme = GetTheme(themeName);
        }


        public static void ClearTheme()
        {
            CurrentTheme = null;
        }

        public static IEnumerable<string> GetAvailableThemeNames()
        {
            return themes.Keys;
        }

        public static IEnumerable<IFluentTheme> GetAvailableThemes()
        {
            return themes.Values;
        }

        private static void OnThemeChanged()
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 主题名称类型转换器
    /// </summary>
    public class ThemeNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var themeNames = new List<string> { "" }; // 空字符串表示不使用主题
            themeNames.AddRange(ThemeManager.GetAvailableThemeNames());
            return new StandardValuesCollection(themeNames);
        }
    }
}
