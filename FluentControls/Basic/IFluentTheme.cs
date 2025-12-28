using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    /// <summary>
    /// Fluent主题接口
    /// </summary>
    public interface IFluentTheme
    {
        string Name { get; }

        ThemeType Type { get; }

        /// <summary>
        /// 调色板
        /// </summary>
        IColorPalette Colors { get; }

        /// <summary>
        /// 字体排版
        /// </summary>
        ITypography Typography { get; }

        /// <summary>
        /// 间距系统
        /// </summary>
        ISpacing Spacing { get; }

        /// <summary>
        /// 高度/阴影
        /// </summary>
        IElevation Elevation { get; }

        /// <summary>
        /// 动画
        /// </summary>
        IAnimation Animation { get; }
    }

    /// <summary>
    /// 支持主题继承的接口
    /// </summary>
    public interface IThemeContainer
    {
        /// <summary>
        /// 是否启用主题继承
        /// </summary>
        bool EnableThemeInheritance { get; set; }

        /// <summary>
        /// 应用主题到子控件
        /// </summary>
        void ApplyThemeToChildren(bool recursive);
    }

    public enum ThemeType
    {
        Light,
        Dark,
        HighContrast
    }
}
