using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Animation
{
    /// <summary>
    /// 动画函数
    /// </summary>
    public static class Easing
    {
        /// <summary>
        /// 线性
        /// </summary>
        public static double Linear(double t) => t;

        #region 二次函数

        public static double QuadIn(double t) => t * t;
        public static double QuadOut(double t) => t * (2 - t);
        public static double QuadInOut(double t) => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

        #endregion

        #region 三次函数

        public static double CubicIn(double t) => t * t * t;
        public static double CubicOut(double t) => 1 + (--t) * t * t;
        public static double CubicInOut(double t) => t < 0.5 ? 4 * t * t * t : 1 + (--t) * (2 * (--t)) * (2 * t);


        #endregion

        #region 四次函数

        public static double QuartIn(double t) => t * t * t * t;
        public static double QuartOut(double t) => 1 - (--t) * t * t * t;
        public static double QuartInOut(double t) => t < 0.5 ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;


        #endregion

        #region 五次函数

        public static double QuintIn(double t) => t * t * t * t * t;
        public static double QuintOut(double t) => 1 + (--t) * t * t * t * t;
        public static double QuintInOut(double t) => t < 0.5 ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;


        #endregion

        #region 正弦函数

        public static double SineIn(double t) => 1 - Math.Cos(t * Math.PI / 2);
        public static double SineOut(double t) => Math.Sin(t * Math.PI / 2);
        public static double SineInOut(double t) => -(Math.Cos(Math.PI * t) - 1) / 2;


        #endregion

        #region 指数函数

        public static double ExpoIn(double t) => t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));
        public static double ExpoOut(double t) => t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);
        public static double ExpoInOut(double t)
        {
            if (t == 0)
            {
                return 0;
            }

            if (t == 1)
            {
                return 1;
            }

            if (t < 0.5)
            {
                return Math.Pow(2, 20 * t - 10) / 2;
            }

            return (2 - Math.Pow(2, -20 * t + 10)) / 2;
        }


        #endregion

        #region 圆形函数

        public static double CircIn(double t) => 1 - Math.Sqrt(1 - t * t);
        public static double CircOut(double t) => Math.Sqrt(1 - (--t) * t);
        public static double CircInOut(double t) => t < 0.5
            ? (1 - Math.Sqrt(1 - 4 * t * t)) / 2
            : (Math.Sqrt(1 - (-2 * t + 2) * (-2 * t + 2)) + 1) / 2;


        #endregion

        #region 弹性函数

        public static double ElasticIn(double t)
        {
            if (t == 0 || t == 1)
            {
                return t;
            }

            double p = 0.3;
            return -Math.Pow(2, 10 * (t - 1)) * Math.Sin((t - 1 - p / 4) * (2 * Math.PI) / p);
        }

        public static double ElasticOut(double t)
        {
            if (t == 0 || t == 1)
            {
                return t;
            }

            double p = 0.3;
            return Math.Pow(2, -10 * t) * Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
        }

        public static double ElasticInOut(double t)
        {
            if (t == 0 || t == 1)
            {
                return t;
            }

            double p = 0.45;
            if (t < 0.5)
            {
                return -0.5 * Math.Pow(2, 20 * t - 10) * Math.Sin((20 * t - 11.125) * (2 * Math.PI) / p);
            }

            return Math.Pow(2, -20 * t + 10) * Math.Sin((20 * t - 11.125) * (2 * Math.PI) / p) / 2 + 1;
        }


        #endregion

        #region 回弹函数

        public static double BackIn(double t)
        {
            double s = 1.70158;
            return t * t * ((s + 1) * t - s);
        }

        public static double BackOut(double t)
        {
            double s = 1.70158;
            return 1 + (--t) * t * ((s + 1) * t + s);
        }

        public static double BackInOut(double t)
        {
            double s = 1.70158 * 1.525;
            if (t < 0.5)
            {
                return 2 * t * t * ((s + 1) * 2 * t - s);
            }

            return 2 * (t - 1) * (t - 1) * ((s + 1) * (t * 2 - 2) + s) + 2;
        }


        #endregion

        #region 弹跳函数

        public static double BounceIn(double t) => 1 - BounceOut(1 - t);

        public static double BounceOut(double t)
        {
            if (t < (1 / 2.75))
            {
                return 7.5625 * t * t;
            }
            else if (t < (2 / 2.75))
            {
                return 7.5625 * (t -= (1.5 / 2.75)) * t + 0.75;
            }
            else if (t < (2.5 / 2.75))
            {
                return 7.5625 * (t -= (2.25 / 2.75)) * t + 0.9375;
            }
            else
            {
                return 7.5625 * (t -= (2.625 / 2.75)) * t + 0.984375;
            }
        }

        public static double BounceInOut(double t) => t < 0.5
            ? BounceIn(t * 2) / 2
            : BounceOut(t * 2 - 1) / 2 + 0.5;

        #endregion
    }
}
