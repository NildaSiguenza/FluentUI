using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Themes
{
    public class BlueTheme : IFluentTheme
    {
        public string Name => "Blue";
        public ThemeType Type => ThemeType.Light;

        public IColorPalette Colors { get; }
        public ITypography Typography { get; }
        public ISpacing Spacing { get; }
        public IElevation Elevation { get; }
        public IAnimation Animation { get; }

        public BlueTheme()
        {
            Colors = new BlueColorPalette();
            Typography = new DefaultTypography();
            Spacing = new DefaultSpacing();
            Elevation = new DefaultElevation();
            Animation = new DefaultAnimation();
        }

        private class BlueColorPalette : IColorPalette
        {
            public Color Primary => Color.FromArgb(33, 150, 243);
            public Color PrimaryLight => Color.FromArgb(100, 181, 246);
            public Color PrimaryDark => Color.FromArgb(25, 118, 210);

            public Color Accent => Color.FromArgb(3, 169, 244);
            public Color AccentLight => Color.FromArgb(129, 212, 250);
            public Color AccentDark => Color.FromArgb(2, 136, 209);

            public Color Background => Color.FromArgb(236, 239, 241);
            public Color BackgroundSecondary => Color.FromArgb(207, 216, 220);
            public Color Surface => Color.White;
            public Color SurfaceHover => Color.FromArgb(250, 250, 250);
            public Color SurfacePressed => Color.FromArgb(224, 224, 224);

            public Color TextPrimary => Color.FromArgb(33, 33, 33);
            public Color TextSecondary => Color.FromArgb(117, 117, 117);
            public Color TextDisabled => Color.FromArgb(189, 189, 189);
            public Color TextOnPrimary => Color.White;

            public Color Border => Color.FromArgb(189, 189, 189);
            public Color BorderLight => Color.FromArgb(224, 224, 224);
            public Color BorderFocused => Primary;

            public Color Success => Color.FromArgb(76, 175, 80);
            public Color Warning => Color.FromArgb(255, 152, 0);
            public Color Error => Color.FromArgb(244, 67, 54);
            public Color Info => Primary;

            public Color Shadow => Color.FromArgb(158, 158, 158);

            public Color GetColorWithOpacity(Color baseColor, float opacity)
            {
                return Color.FromArgb((int)(255 * opacity), baseColor);
            }
        }
    }

}
