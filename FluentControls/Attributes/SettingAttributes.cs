using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    /// <summary>
    /// 标记属性在设置面板中应被忽略
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SettingIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// 设置项显示配置特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SettingDisplayAttribute : Attribute
    {
        public SettingDisplayAttribute()
        {
            Order = 0;
            ReadOnly = false;
        }

        public SettingDisplayAttribute(string displayName) : this()
        {
            DisplayName = displayName;
        }

        public SettingDisplayAttribute(string displayName, string groupName) : this(displayName)
        {
            GroupName = groupName;
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 所属分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 占位符文本(用于文本框)
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool ReadOnly { get; set; }

    }

    /// <summary>
    /// 数值范围特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SettingRangeAttribute : Attribute
    {
        public SettingRangeAttribute(double minimum, double maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
            DecimalPlaces = 2;
        }

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public int DecimalPlaces { get; set; }
    }

    /// <summary>
    /// 分组特性(用于类级别)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SettingGroupAttribute : Attribute
    {
        public SettingGroupAttribute(string groupName)
        {
            GroupName = groupName;
            Order = 0;
        }

        public string GroupName { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }
}
