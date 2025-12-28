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
    /// Fluent暗色主题
    /// </summary>
    public class DarkTheme : IFluentTheme
    {
        public string Name => "FluentDark";
        public ThemeType Type => ThemeType.Dark;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public DarkTheme()
        {
            Colors = new DarkColorPalette();
            Typography = new DarkTypography();
            Spacing = new DefaultSpacing();
            Elevation = new DarkElevation();
            Animation = new DefaultAnimation();
        }

        private class DarkColorPalette : IColorPalette
        {
            public Color Primary => Color.FromArgb(96, 205, 255);
            public Color PrimaryLight => Color.FromArgb(130, 220, 255);
            public Color PrimaryDark => Color.FromArgb(64, 169, 220);

            public Color Accent => Color.FromArgb(118, 185, 237);
            public Color AccentLight => Color.FromArgb(150, 210, 255);
            public Color AccentDark => Color.FromArgb(86, 156, 214);

            public Color Background => Color.FromArgb(32, 32, 32);
            public Color BackgroundSecondary => Color.FromArgb(28, 28, 28);
            public Color Surface => Color.FromArgb(43, 43, 43);
            public Color SurfaceHover => Color.FromArgb(53, 53, 53);
            public Color SurfacePressed => Color.FromArgb(38, 38, 38);

            public Color TextPrimary => Color.FromArgb(255, 255, 255);
            public Color TextSecondary => Color.FromArgb(200, 200, 200);
            public Color TextDisabled => Color.FromArgb(110, 110, 110);
            public Color TextOnPrimary => Color.FromArgb(0, 0, 0);

            public Color Border => Color.FromArgb(64, 64, 64);
            public Color BorderLight => Color.FromArgb(51, 51, 51);
            public Color BorderFocused => Primary;

            public Color Success => Color.FromArgb(87, 187, 87);
            public Color Warning => Color.FromArgb(255, 200, 60);
            public Color Error => Color.FromArgb(249, 88, 96);
            public Color Info => Color.FromArgb(96, 205, 255);

            public Color Shadow => Color.FromArgb(0, 0, 0);

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                return Color.FromArgb((int)(255 * opacity), baseColor);
            }
        }

        private class DarkTypography : ITypography
        {
            public string FontFamily => "Microsoft YaHei";
            public float BaseSize => 9.35f;

            public Font Display => new Font(FontFamily, BaseSize * 2.5f, FontStyle.Regular);
            public Font Headline => new Font(FontFamily, BaseSize * 1.5f, FontStyle.Bold);
            public Font Title => new Font(FontFamily, BaseSize * 1.25f, FontStyle.Regular);
            public Font Body => new Font(FontFamily, BaseSize, FontStyle.Regular);
            public Font Caption => new Font(FontFamily, BaseSize * 0.85f, FontStyle.Regular);
            public Font Button => new Font(FontFamily, BaseSize, FontStyle.Regular);

            public Font GetFont(FontStyle style, float sizeMultiplier = 1)
            {
                return new Font(FontFamily, BaseSize * sizeMultiplier, style);
            }
        }

        private class DarkElevation : IElevation
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
                            Blur = 3,
                            Opacity = 0.3f
                        };
                    case 2:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 2,
                            Blur = 6,
                            Opacity = 0.35f
                        };
                    case 4:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 4,
                            Blur = 10,
                            Opacity = 0.4f
                        };
                    case 8:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = 8,
                            Blur = 20,
                            Opacity = 0.45f
                        };
                    default:
                        return new Shadow
                        {
                            Color = Color.Black,
                            OffsetY = level,
                            Blur = level * 2,
                            Opacity = Math.Min(0.6f, 0.3f + level * 0.02f)
                        };
                }
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
