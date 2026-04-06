using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FluentControls.ControlExtension;
using FluentControls.Controls;

namespace FluentControls
{
    public static class FluentControlExtension
    {

        public static void Bind2Enum<TEnum>(this FluentComboBox control, Func<TEnum, bool> filter = null, bool sortByText = false)
            where TEnum : struct, Enum
        {
            if (control == null)
            {
                return;
            }

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

        public static void Bind2Enum<TEnum>(this FluentListBox control, Func<TEnum, bool> filter = null, bool sortByText = false)
            where TEnum : struct, Enum
        {
            if (control == null)
            {
                return;
            }

            control.Clear();
            var itemList = GetEnumBindingItems<TEnum>(filter, sortByText);

            if (itemList != null)
            {
                foreach (var item in itemList)
                {
                    FluentListItem listItem = new FluentListItem(item.Value, item.Text);
                    listItem.Tag = item;
                    control.Items.Add(listItem);
                }
            }
        }
    }
}
