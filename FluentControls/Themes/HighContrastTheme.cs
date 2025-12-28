using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Themes
{
    /// <summary>
    /// Fluent高对比度主题
    /// </summary>
    public class HighContrastTheme : IFluentTheme
    {
        public string Name => "HighContrast";
        public ThemeType Type => ThemeType.HighContrast;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public HighContrastTheme()
        {
            Colors = new HighContrastColorPalette();
            Typography = new HighContrastTypography();
            Spacing = new DefaultSpacing();
            Elevation = new HighContrastElevation();
            Animation = new DefaultAnimation();
        }

        private class HighContrastColorPalette : IColorPalette
        {
            public Color Primary => Color.FromArgb(255, 255, 0);  // 黄色
            public Color PrimaryLight => Color.FromArgb(255, 255, 128);
            public Color PrimaryDark => Color.FromArgb(204, 204, 0);

            public Color Accent => Color.FromArgb(0, 255, 255);  // 青色
            public Color AccentLight => Color.FromArgb(128, 255, 255);
            public Color AccentDark => Color.FromArgb(0, 204, 204);

            public Color Background => Color.FromArgb(0, 0, 0);  // 纯黑
            public Color BackgroundSecondary => Color.FromArgb(0, 0, 0);
            public Color Surface => Color.FromArgb(0, 0, 0);
            public Color SurfaceHover => Color.FromArgb(30, 30, 30);
            public Color SurfacePressed => Color.FromArgb(50, 50, 50);

            public Color TextPrimary => Color.FromArgb(255, 255, 255);  // 纯白
            public Color TextSecondary => Color.FromArgb(255, 255, 0);  // 黄色
            public Color TextDisabled => Color.FromArgb(128, 128, 128);
            public Color TextOnPrimary => Color.FromArgb(0, 0, 0);

            public Color Border => Color.FromArgb(255, 255, 255);  // 白色边框
            public Color BorderLight => Color.FromArgb(200, 200, 200);
            public Color BorderFocused => Color.FromArgb(255, 255, 0);  // 黄色焦点

            public Color Success => Color.FromArgb(0, 255, 0);  // 纯绿
            public Color Warning => Color.FromArgb(255, 255, 0);  // 纯黄
            public Color Error => Color.FromArgb(255, 0, 0);  // 纯红
            public Color Info => Color.FromArgb(0, 255, 255);  // 纯青

            public Color Shadow => Color.Transparent;  // 高对比度模式不使用阴影

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                // 高对比度模式不使用透明度
                return opacity > 0.5f ? baseColor : Color.Transparent;
            }
        }

        private class HighContrastTypography : ITypography
        {
            public string FontFamily => "Microsoft YaHei";
            public float BaseSize => 9.35f;

            // 高对比度模式下字体更粗
            public Font Display => new Font(FontFamily, BaseSize * 2.5f, FontStyle.Bold);
            public Font Headline => new Font(FontFamily, BaseSize * 1.5f, FontStyle.Bold);
            public Font Title => new Font(FontFamily, BaseSize * 1.25f, FontStyle.Bold);
            public Font Body => new Font(FontFamily, BaseSize, FontStyle.Regular);
            public Font Caption => new Font(FontFamily, BaseSize * 0.85f, FontStyle.Regular);
            public Font Button => new Font(FontFamily, BaseSize, FontStyle.Bold);

            public Font GetFont(FontStyle style, float sizeMultiplier = 1)
            {
                // 强制使用粗体以提高可读性
                var finalStyle = style | FontStyle.Bold;
                return new Font(FontFamily, BaseSize * sizeMultiplier, finalStyle);
            }
        }

        private class HighContrastElevation : IElevation
        {
            public int CornerRadius => 0;  // 高对比度模式使用直角
            public int CornerRadiusSmall => 0;
            public int CornerRadiusLarge => 0;

            public Shadow GetShadow(int level)
            {
                // 高对比度模式不使用阴影
                return null;
            }
        }

        private class DefaultSpacing : ISpacing
        {
            // 高对比度模式下增加间距
            public int XXSmall => 4;
            public int XSmall => 6;
            public int Small => 10;
            public int Medium => 14;
            public int Large => 18;
            public int XLarge => 26;
            public int XXLarge => 34;

            public Padding GetPadding(SpacingSize size)
            {
                int space = GetSpace(size);
                return new Padding(space);
            }

            public int GetSpace(SpacingSize size)
            {
                switch (size)
                {
                    case SpacingSize.XXSmall:
                        return XXSmall;
                    case SpacingSize.XSmall:
                        return XSmall;
                    case SpacingSize.Small:
                        return Small;
                    case SpacingSize.Medium:
                        return Medium;
                    case SpacingSize.Large:
                        return Large;
                    case SpacingSize.XLarge:
                        return XLarge;
                    case SpacingSize.XXLarge:
                        return XXLarge;
                    default:
                        return Medium;
                }
            }
        }

        private class DefaultAnimation : IAnimation
        {
            // 高对比度模式下减少动画
            public int FastDuration => 50;
            public int NormalDuration => 100;
            public int SlowDuration => 150;

            public EasingFunction DefaultEasing => (t) => t;  // 线性
            public EasingFunction AccelerateEasing => (t) => t;
            public EasingFunction DecelerateEasing => (t) => t;
        }
    }

}
