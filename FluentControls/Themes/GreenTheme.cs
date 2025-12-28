using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Themes
{
    public class GreenTheme : IFluentTheme
    {
        public string Name => "Green";
        public ThemeType Type => ThemeType.Light;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public GreenTheme()
        {
            Colors = new GreenColorPalette();
            Typography = new DefaultTypography();
            Spacing = new DefaultSpacing();
            Elevation = new DefaultElevation();
            Animation = new DefaultAnimation();
        }

        private class GreenColorPalette : IColorPalette
        {
            public Color Primary => Color.FromArgb(76, 175, 80);
            public Color PrimaryLight => Color.FromArgb(129, 199, 132);
            public Color PrimaryDark => Color.FromArgb(56, 142, 60);

            public Color Accent => Color.FromArgb(139, 195, 74);
            public Color AccentLight => Color.FromArgb(178, 223, 119);
            public Color AccentDark => Color.FromArgb(104, 159, 56);

            public Color Background => Color.FromArgb(241, 248, 233);
            public Color BackgroundSecondary => Color.FromArgb(220, 237, 200);
            public Color Surface => Color.White;
            public Color SurfaceHover => Color.FromArgb(248, 255, 240);
            public Color SurfacePressed => Color.FromArgb(232, 245, 233);

            public Color TextPrimary => Color.FromArgb(27, 94, 32);
            public Color TextSecondary => Color.FromArgb(85, 139, 47);
            public Color TextDisabled => Color.FromArgb(158, 158, 158);
            public Color TextOnPrimary => Color.White;

            public Color Border => Color.FromArgb(200, 230, 201);
            public Color BorderLight => Color.FromArgb(232, 245, 233);
            public Color BorderFocused => Primary;

            public Color Success => Primary;
            public Color Warning => Color.FromArgb(255, 193, 7);
            public Color Error => Color.FromArgb(211, 47, 47);
            public Color Info => Color.FromArgb(33, 150, 243);

            public Color Shadow => Color.FromArgb(165, 214, 167);

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                return Color.FromArgb((int)(255 * opacity), baseColor);
            }
        }
    }

}
