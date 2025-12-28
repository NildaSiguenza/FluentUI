using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Themes
{
    public class DefaultTypography : ITypography
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

    public class DefaultSpacing : ISpacing
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
                case SpacingSize.XXSmall: return XXSmall;
                case SpacingSize.XSmall: return XSmall;
                case SpacingSize.Small: return Small;
                case SpacingSize.Medium: return Medium;
                case SpacingSize.Large: return Large;
                case SpacingSize.XLarge: return XLarge;
                case SpacingSize.XXLarge: return XXLarge;
                default: return Medium;
            }
        }
    }

    public class DefaultElevation : IElevation
    {
        public int CornerRadius => 4;
        public int CornerRadiusSmall => 2;
        public int CornerRadiusLarge => 8;

        public Shadow GetShadow(int level)
        {
            switch (level)
            {
                case 0: return null;
                case 1: return new Shadow { Color = Color.Black, OffsetY = 1, Blur = 2, Opacity = 0.1f };
                case 2: return new Shadow { Color = Color.Black, OffsetY = 2, Blur = 4, Opacity = 0.12f };
                case 4: return new Shadow { Color = Color.Black, OffsetY = 4, Blur = 8, Opacity = 0.14f };
                case 8: return new Shadow { Color = Color.Black, OffsetY = 8, Blur = 16, Opacity = 0.16f };
                default: return new Shadow { Color = Color.Black, OffsetY = level, Blur = level * 2, Opacity = Math.Min(0.24f, 0.1f + level * 0.01f) };
            }
        }
    }

    public class DefaultAnimation : IAnimation
    {
        public int FastDuration => 150;
        public int NormalDuration => 300;
        public int SlowDuration => 500;

        public EasingFunction DefaultEasing => CubicBezier;
        public EasingFunction AccelerateEasing => (t) => t * t;
        public EasingFunction DecelerateEasing => (t) => 1 - Math.Pow(1 - t, 2);

        private double CubicBezier(double t)
        {
            return t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        }
    }
}
