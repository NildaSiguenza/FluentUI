using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class EnumExtend
    {
        public static bool TryParseEnum(this Type enumType, string value, bool ignoreCase, out object result)
        {
            result = null;

            if (enumType == null || !enumType.IsEnum)
            {
                return false;
            }

            try
            {
                object parsed = Enum.Parse(enumType, value, ignoreCase);
                result = parsed;
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
