using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;
using Infrastructure;
using static System.ComponentModel.TypeConverter;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// IconChar 类型转换器
    /// </summary>
    public class IconCharConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string strValue)
            {
                if (context?.Instance is FluentFontIcon control && !string.IsNullOrEmpty(control.FontFamily))
                {
                    var enumType = IconFontManager.Instance.GetIconEnumType(control.FontFamily);
                    if (enumType != null && enumType.TryParseEnum(strValue, true, out var enumValue))
                    {
                        return enumValue;
                    }
                }
                return strValue;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null)
            {
                if (value is Enum enumValue)
                {
                    return enumValue.ToString();
                }
                return value.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is FluentFontIcon control && !string.IsNullOrEmpty(control.FontFamily))
            {
                var enumType = IconFontManager.Instance.GetIconEnumType(control.FontFamily);
                if (enumType != null)
                {
                    var values = Enum.GetValues(enumType);
                    return new StandardValuesCollection(values);
                }
            }

            return new StandardValuesCollection(new object[0]);
        }
    }

}
