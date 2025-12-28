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
    /// Fluent亮色主题
    /// </summary>
    public class LightTheme : IFluentTheme
    {
        public string Name => "FluentLight";
        public ThemeType Type => ThemeType.Light;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public LightTheme()
        {
            Colors = new LightColorPalette();
            Typography = new DefaultTypography();
            Spacing = new DefaultSpacing();
            Elevation = new DefaultElevation();
            Animation = new DefaultAnimation();
        }

        private class LightColorPalette : IColorPalette
        {
            public Color Primary => Color.FromArgb(0, 120, 212);
            public Color PrimaryLight => Color.FromArgb(38, 147, 255);
            public Color PrimaryDark => Color.FromArgb(0, 84, 153);

            public Color Accent => Color.FromArgb(0, 120, 212);
            public Color AccentLight => Color.FromArgb(76, 194, 255);
            public Color AccentDark => Color.FromArgb(0, 84, 153);

            public Color Background => Color.FromArgb(243, 243, 243);
            public Color BackgroundSecondary => Color.FromArgb(249, 249, 249);
            public Color Surface => Color.White;
            public Color SurfaceHover => Color.FromArgb(251, 251, 251);
            public Color SurfacePressed => Color.FromArgb(243, 243, 243);

            public Color TextPrimary => Color.FromArgb(32, 32, 32);
            public Color TextSecondary => Color.FromArgb(96, 96, 96);
            public Color TextDisabled => Color.FromArgb(161, 161, 161);
            public Color TextOnPrimary => Color.White;

            public Color Border => Color.FromArgb(229, 229, 229);
            public Color BorderLight => Color.FromArgb(243, 243, 243);
            public Color BorderFocused => Primary;

            public Color Success => Color.FromArgb(16, 124, 16);
            public Color Warning => Color.FromArgb(255, 185, 0);
            public Color Error => Color.FromArgb(232, 17, 35);
            public Color Info => Color.FromArgb(0, 120, 212);

            public Color Shadow => Color.FromArgb(0, 0, 0);

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                return Color.FromArgb((int)(255 * opacity), baseColor);
            }
        }

        private class DefaultTypography : ITypography
        {
            public string FontFamily => "Microsoft YaHei";
            public float BaseSize => 9.35f;

            public Font Display => new Font(FontFamily, BaseSize * 2.5f);
            public Font Headline => new Font(FontFamily, BaseSize * 1.5f, FontStyle.Bold);
            public Font Title => new Font(FontFamily, BaseSize * 1.25f);
            public Font Body => new Font(FontFamily, BaseSize);
            public Font Caption => new Font(FontFamily, BaseSize * 0.85f);
            public Font Button => new Font(FontFamily, BaseSize, FontStyle.Regular);

            public Font GetFont(FontStyle style, float sizeMultiplier = 1)
            {
                return new Font(FontFamily, BaseSize * sizeMultiplier, style);
            }
        }

        private class DefaultSpacing : ISpacing
        {
            public int XXSmall => 2;
            public int XSmall => 4;
            public int Small => 8;
            public int Medium => 12;
            public int Large => 16;
            public int XLarge => 24;
            public int XXLarge => 32;

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

        private class DefaultElevation : IElevation
        {
            public int CornerRadius => 4;
            public int CornerRadiusSmall => 2;
            public int CornerRadiusLarge => 8;

            public Shadow GetShadow(int level)
            {
                switch (level)
                {
                    case 0:
                        return null;
                    case 1:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 1,
                            Blur = 2,
                            Opacity = 0.1f
                        };
                    case 2:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 2,
                            Blur = 4,
                            Opacity = 0.12f
                        };
                    case 4:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 4,
                            Blur = 8,
                            Opacity = 0.14f
                        };
                    case 8:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 8,
                            Blur = 16,
                            Opacity = 0.16f
                        };
                    default:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = level,
                            Blur = level * 2,
                            Opacity = Math.Min(0.24f, 0.1f + level * 0.01f)
                        };
                }
            }
        }

        private class DefaultAnimation : IAnimation
        {
            public int FastDuration => 150;
            public int NormalDuration => 300;
            public int SlowDuration => 500;

            public EasingFunction DefaultEasing => CubicBezier;
            public EasingFunction AccelerateEasing => (t) => t * t;
            public EasingFunction DecelerateEasing => (t) => 1 - Math.Pow(1 - t, 2);

            private double CubicBezier(double t)
            {
                return t < 0.5
                    ? 4 * t * t * t
                    : 1 - Math.Pow(-2 * t + 2, 3) / 2;
            }
        }

    }
}
