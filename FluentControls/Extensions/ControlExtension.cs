using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls
{
    public static class ControlExtension
    {
        public static T FindChild<T>(this Control parent) where T : Control
        {
            if (parent == null)
            {
                return null;
            }

            // 先从直接子级寻找
            foreach (Control child in parent.Controls)
            {
                if (child is T tChild)
                {
                    return tChild;
                }
            }

            foreach (Control child in parent.Controls)
            {
                var result = FindChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static IEnumerable<T> FindChildren<T>(this Control parent) where T : Control
        {
            if (parent == null)
            {
                yield break;
            }
            foreach (Control child in parent.Controls)
            {
                if (child is T tChild)
                {
                    yield return tChild;
                }
                foreach (var descendant in FindChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 将指定枚举类型绑定到 ListControl
        /// 支持过滤(ComboBox、ListBox 等)
        /// </summary>
        public static void Bind2Enum<TEnum>(this ListControl control, Func<TEnum, bool> filter = null, bool sortByText = false)
            where TEnum : struct, Enum
        {
            if (control == null)
            {
                return;
            }

            var itemList = GetEnumBindingItems<TEnum>(filter, sortByText);

            if (itemList != null)
            {
                control.DisplayMember = nameof(EnumBindingItem<TEnum>.Text);
                control.ValueMember = nameof(EnumBindingItem<TEnum>.Value);
                control.DataSource = itemList;
            }
        }

        public static List<EnumBindingItem<TEnum>> GetEnumBindingItems<TEnum>(Func<TEnum, bool> filter = null, bool sortByText = false)
            where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                return null;
            }

            // 获取所有枚举值
            IEnumerable<TEnum> values = Enum
                .GetValues(enumType)
                .Cast<TEnum>();

            // 过滤
            if (filter != null)
            {
                values = values.Where(filter);
            }

            // 构造绑定项
            var items = values
                .Select(v => new EnumBindingItem<TEnum>
                {
                    Value = v,
                    Text = GetEnumDescription(v)
                });

            // 按显示文本排序
            if (sortByText)
            {
                items = items.OrderBy(x => x.Text);
            }

            return items.ToList();
        }

        /// <summary>
        /// 绑定所有枚举值
        /// (无过滤/无排序)
        /// </summary>
        public static void Bind2Enum<TEnum>(this ListControl control)
            where TEnum : struct, Enum
        {
            Bind2Enum<TEnum>(control, filter: null, sortByText: false);
        }

        /// <summary>
        /// 获取单个枚举值的显示文本
        /// </summary>
        public static string GetEnumDescription<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            var name = Enum.GetName(type, value);
            if (name == null)
            {
                return value.ToString();
            }

            var field = type.GetField(name);
            if (field == null)
            {
                return name;
            }

            var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(
                field, typeof(DescriptionAttribute));

            if (attr != null && !string.IsNullOrEmpty(attr.Description))
            {
                return attr.Description;
            }

            return name;
        }

        /// <summary>
        /// 用于绑定的数据项类型
        ///     - Value = 枚举值
        ///     - Text = 显示文本
        /// </summary>
        public class EnumBindingItem<T>
        {
            public T Value { get; set; }
            public string Text { get; set; }
        }
    }
}
