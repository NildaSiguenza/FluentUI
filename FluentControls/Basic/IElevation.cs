using System.Drawing;

namespace FluentControls
{
    /// <summary>
    /// 高度/阴影系统
    /// </summary>
    public interface IElevation
    {
        Shadow GetShadow(int level); // level: 0-24

        int CornerRadius { get; }

        int CornerRadiusSmall { get; }

        int CornerRadiusLarge { get; }
    }

    public class Shadow
    {
        public Color Color { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public int Blur { get; set; }

        public int Spread { get; set; }

        public float Opacity { get; set; }
    }

}