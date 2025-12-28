using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Themes
{
    public class ClassicTheme : IFluentTheme
    {
        public string Name => "Classic";
        public ThemeType Type => ThemeType.Light;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public ClassicTheme()
        {
            Colors = new ClassicColorPalette();
            Typography = new ClassicTypography();
            Spacing = new DefaultSpacing();
            Elevation = new ClassicElevation();
            Animation = new DefaultAnimation();
        }

        private class ClassicColorPalette : IColorPalette
        {
            public Color Primary => SystemColors.Highlight;
            public Color PrimaryLight => Color.FromArgb(173, 214, 255);
            public Color PrimaryDark => Color.FromArgb(0, 84, 153);

            public Color Accent => SystemColors.HotTrack;
            public Color AccentLight => Color.FromArgb(166, 202, 240);
            public Color AccentDark => Color.FromArgb(51, 94, 168);

            public Color Background => SystemColors.Control;
            public Color BackgroundSecondary => Color.FromArgb(236, 233, 216);
            public Color Surface => SystemColors.Window;
            public Color SurfaceHover => SystemColors.ControlLight;
            public Color SurfacePressed => SystemColors.ControlDark;

            public Color TextPrimary => SystemColors.ControlText;
            public Color TextSecondary => SystemColors.GrayText;
            public Color TextDisabled => SystemColors.InactiveCaptionText;
            public Color TextOnPrimary => SystemColors.HighlightText;

            public Color Border => SystemColors.ActiveBorder;
            public Color BorderLight => SystemColors.ControlLight;
            public Color BorderFocused => SystemColors.Highlight;

            public Color Success => Color.FromArgb(0, 128, 0);
            public Color Warning => Color.FromArgb(255, 140, 0);
            public Color Error => Color.FromArgb(220, 20, 60);
            public Color Info => SystemColors.HotTrack;

            public Color Shadow => SystemColors.ControlDarkDark;

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                return Color.FromArgb((int)(255 * opacity), baseColor);
            }
        }

        private class ClassicTypography : ITypography
        {
            public string FontFamily => SystemFonts.DefaultFont.FontFamily.Name;
            public float BaseSize => SystemFonts.DefaultFont.Size;

            public Font Display => new Font(FontFamily, BaseSize * 2f);
            public Font Headline => new Font(FontFamily, BaseSize * 1.5f, FontStyle.Bold);
            public Font Title => new Font(FontFamily, BaseSize * 1.25f);
            public Font Body => SystemFonts.DefaultFont;
            public Font Caption => new Font(FontFamily, BaseSize * 0.9f);
            public Font Button => SystemFonts.DefaultFont;

            public Font GetFont(FontStyle style, float sizeMultiplier = 1)
            {
                return new Font(FontFamily, BaseSize * sizeMultiplier, style);
            }
        }

        private class ClassicElevation : IElevation
        {
            public Shadow GetShadow(int level)
            {
                // 经典主题使用简单的实线边框, 不使用阴影
                return null;
            }

            public int CornerRadius => 0;
            public int CornerRadiusSmall => 0;
            public int CornerRadiusLarge => 0;

        }
    }

}
