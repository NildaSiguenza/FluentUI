using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;
using FluentControls.IconFonts;
using Infrastructure;

namespace FluentControls
{
    public static class IconFontControlExtension
    {
        /// <summary>
        /// 运行时通过 IconFontManager 设置图标
        /// </summary>
        public static void SetIconRuntime(this FluentFontIcon control, string fontFamily, string iconChar, float size, Color color, float rotation = 0)
        {
            try
            {
                var provider = IconFontManager.Instance.GetProvider(fontFamily);
                if (provider == null)
                {
                    Debug.WriteLine($"找不到字体提供者: {fontFamily}");
                    return;
                }

                var enumType = provider.GetIconEnumType();
                Image icon = null;

                if (enumType != null && enumType.TryParseEnum(iconChar, true, out var enumValue))
                {
                    icon = provider.GetIcon((Enum)enumValue, size, color, rotation);
                }
                else
                {
                    icon = provider.GetIcon(iconChar, size, color, rotation);
                }

                if (icon != null)
                {
                    control.SetIconImage(icon);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"运行时设置图标失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 运行时通过枚举设置图标
        /// </summary>
        public static void SetIconRuntime(this FluentFontIcon control, string fontFamily, Enum iconEnum, float size, Color color, float rotation = 0)
        {
            try
            {
                var provider = IconFontManager.Instance.GetProvider(fontFamily);
                if (provider == null)
                {
                    Debug.WriteLine($"找不到字体提供者: {fontFamily}");
                    return;
                }

                var icon = provider.GetIcon(iconEnum, size, color, rotation);
                if (icon != null)
                {
                    control.SetIconImage(icon);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"运行时设置图标失败: {ex.Message}");
            }
        }
    }

}
