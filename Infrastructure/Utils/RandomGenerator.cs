using System;
using System.Collections.Generic;
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
    }
}
