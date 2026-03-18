using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    /// <summary>
    /// 忽略编辑特性
    /// 标记此特性的属性不会在PropertyGrid中显示
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyIgnoreEditAttribute : Attribute
    {
    }

    /// <summary>
    /// 属性显示名称特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyDisplayNameAttribute : Attribute
    {
        public PropertyDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }

    /// <summary>
    /// 属性描述特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyDescriptionAttribute : Attribute
    {
        public PropertyDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }

    /// <summary>
    /// 属性分类特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyCategoryAttribute : Attribute
    {
        public PropertyCategoryAttribute(string category)
        {
            Category = category;
        }

        public string Category { get; }
    }

    /// <summary>
    /// 只读属性特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyReadOnlyAttribute : Attribute
    {
        public PropertyReadOnlyAttribute(bool isReadOnly = true)
        {
            IsReadOnly = isReadOnly;
        }

        public bool IsReadOnly { get; }
    }
}
