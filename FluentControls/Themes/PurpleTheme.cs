using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Themes
{
    public class PurpleTheme : IFluentTheme
    {
        public string Name => "Purple";
        public ThemeType Type => ThemeType.Light;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public PurpleTheme()
        {
            Colors = new PurpleColorPalette();
            Typography = new DefaultTypography();
            Spacing = new DefaultSpacing();
            Elevation = new DefaultElevation();
            Animation = new DefaultAnimation();
        }

        private class PurpleColorPalette : IColorPalette
        {
            public Color Primary => Color.FromArgb(156, 39, 176);
            public Color PrimaryLight => Color.FromArgb(186, 104, 200);
            public Color PrimaryDark => Color.FromArgb(123, 31, 162);

            public Color Accent => Color.FromArgb(171, 71, 188);
            public Color AccentLight => Color.FromArgb(206, 147, 216);
            public Color AccentDark => Color.FromArgb(142, 36, 170);

            public Color Background => Color.FromArgb(243, 229, 245);
            public Color BackgroundSecondary => Color.FromArgb(237, 231, 246);
            public Color Surface => Color.White;
            public Color SurfaceHover => Color.FromArgb(248, 241, 249);
            public Color SurfacePressed => Color.FromArgb(225, 190, 231);

            public Color TextPrimary => Color.FromArgb(74, 20, 140);
            public Color TextSecondary => Color.FromArgb(106, 27, 154);
            public Color TextDisabled => Color.FromArgb(158, 158, 158);
            public Color TextOnPrimary => Color.White;

            public Color Border => Color.FromArgb(225, 190, 231);
            public Color BorderLight => Color.FromArgb(243, 229, 245);
            public Color BorderFocused => Primary;

            public Color Success => Color.FromArgb(102, 187, 106);
            public Color Warning => Color.FromArgb(255, 167, 38);
            public Color Error => Color.FromArgb(239, 83, 80);
            public Color Info => Color.FromArgb(66, 165, 245);

            public Color Shadow => Color.FromArgb(206, 147, 216);

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                return Color.FromArgb((int)(255 * opacity), baseColor);
            }
        }
    }
}
