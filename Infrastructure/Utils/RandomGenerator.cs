using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class RandomGenerator
    {
        private static readonly Random r = new Random();

        public static T Next<T>()
        {
            return (T)Next(typeof(T));
        }

        public static object Next(Type type)
        {
            if (type == null)
            {
                return null;
            }

            //  枚举支持 
            if (type.IsEnum)
            {
                var underlying = Enum.GetUnderlyingType(type);
                var raw = Next(underlying);
                return Enum.ToObject(type, raw);
            }

            //  基础数值类型 
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    return (byte)r.Next(byte.MinValue, byte.MaxValue + 1);

                case TypeCode.SByte:
                    return (sbyte)r.Next(sbyte.MinValue, sbyte.MaxValue + 1);

                case TypeCode.Int16:
                    return (short)r.Next(short.MinValue, short.MaxValue + 1);

                case TypeCode.UInt16:
                    return (ushort)r.Next(ushort.MinValue, ushort.MaxValue + 1);

                case TypeCode.Int32:
                    return r.Next();

                case TypeCode.UInt32:
                    return (uint)(r.NextDouble() * uint.MaxValue);

                case TypeCode.Int64:
                    return NextInt64();

                case TypeCode.UInt64:
                    return NextUInt64();

                case TypeCode.Single:
                    return (float)r.NextDouble();

                case TypeCode.Double:
                    return r.NextDouble();

                default:
                    throw new NotSupportedException("不支持类型: " + type.FullName);
            }
        }

        private static long NextInt64()
        {
            var buffer = new byte[8];
            r.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        private static ulong NextUInt64()
        {
            var buffer = new byte[8];
            r.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static Color NextColor(double saturation = 0.7, double value = 0.9)
        {
            double hue;
            lock (r) // 如果你可能多线程调用
            {
                hue = r.NextDouble() * 360.0;
            }

            return ColorFromHsv(hue, saturation, value);
        }

        private static Color ColorFromHsv(double h, double s, double v)
        {
            int hi = (int)Math.Floor(h / 60) % 6;
            double f = h / 60 - Math.Floor(h / 60);

            v = v * 255;
            int vi = (int)v;
            int p = (int)(v * (1 - s));
            int q = (int)(v * (1 - f * s));
            int t = (int)(v * (1 - (1 - f) * s));

            switch (hi)
            {
                case 0: return Color.FromArgb(vi, t, p);
                case 1: return Color.FromArgb(q, vi, p);
                case 2: return Color.FromArgb(p, vi, t);
                case 3: return Color.FromArgb(p, q, vi);
                case 4: return Color.FromArgb(t, p, vi);
                default: return Color.FromArgb(vi, p, q);
            }
        }
    }
}
