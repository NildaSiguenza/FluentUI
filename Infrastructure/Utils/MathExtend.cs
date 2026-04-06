using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class MathEx
    {
        private const double Log2E = 1.4426950408889634;

        /// <summary>
        /// 限制值在 min 和 max 之间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
            {
                return min;
            }

            if (value.CompareTo(max) > 0)
            {
                return max;
            }

            return value;
        }

        public static int Clamp(this int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        /// <summary>
        /// 返回以 2 为底的对数
        /// </summary>
        public static double Log2(this double x)
        {
            return Math.Log(x) * Log2E;
        }

        /// <summary>
        /// 返回整数的以 2 为底的对数(向下取整)
        /// (等效于 .NET 6+ 的 BitOperations.Log2)
        /// </summary>
        public static int ILog2(this uint value)
        {
            if (value == 0)
            {
                throw new ArgumentException("value must be greater than 0");
            }

            int log = 0;
            while ((value >>= 1) != 0)
            {
                log++;
            }
            return log;
        }

        /// <summary>
        /// 高性能整数 Log2
        /// </summary>
        public static int ILog2Fast(this uint value)
        {
            if (value == 0)
            {
                throw new ArgumentException("value must be greater than 0");
            }

            int n = 0;
            if (value >= 0x10000) { value >>= 16; n += 16; }
            if (value >= 0x100) { value >>= 8; n += 8; }
            if (value >= 0x10) { value >>= 4; n += 4; }
            if (value >= 0x4) { value >>= 2; n += 2; }
            if (value >= 0x2) { n += 1; }
            return n;
        }
    }
}
