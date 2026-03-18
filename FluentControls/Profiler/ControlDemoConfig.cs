using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Controls;

namespace FluentControls
{
    /// <summary>
    /// 控件演示配置基类
    /// </summary>
    public abstract class ControlDemoConfigBase
    {
        [PropertyCategory("主题")]
        [PropertyDisplayName("使用主题")]
        [PropertyDescription("是否启用主题样式")]
        public bool UseTheme { get; set; } = true;

        [PropertyCategory("主题")]
        [PropertyDisplayName("主题名称")]
        [PropertyDescription("选择要应用的主题")]
        public DemoThemeName ThemeName { get; set; } = DemoThemeName.FluentLight;

        /// <summary>
        /// 将配置写入控件
        /// </summary>
        public virtual void ApplyTo(Control ctrl)
        {
            if (ctrl is FluentControlBase fcb)
            {
                string name = ThemeName == DemoThemeName.None ? "" : ThemeName.ToString();
                fcb.UseTheme = UseTheme;
                if (!string.IsNullOrEmpty(name))
                {
                    fcb.ThemeName = name;
                }
                else if (!UseTheme)
                {
                    fcb.UseTheme = false;
                }
            }
        }

        /// <summary>
        /// 从控件读取配置
        /// </summary>
        public virtual void ReadFrom(Control ctrl)
        {
            if (ctrl is FluentControlBase fcb)
            {
                UseTheme = fcb.UseTheme;
                if (Enum.TryParse<DemoThemeName>(fcb.ThemeName, out var tn))
                {
                    ThemeName = tn;
                }
                else
                {
                    ThemeName = DemoThemeName.None;
                }
            }
        }
    }

    /// <summary>
    /// 控件测试配置
    /// </summary>
    public class ControlTestConfig
    {
        /// <summary>
        /// 控件显示名称 
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// 控件分类 
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 控件类型 
        /// </summary>
        public Type ControlType { get; set; }

        /// <summary>
        /// 对应的 DemoConfig 类型, 用于自动生成属性变更场景(null 则跳过属性遍历) 
        /// </summary>
        public Type DemoConfigType { get; set; }

        /// <summary>
        /// 控件工厂方法 
        /// </summary>
        public Func<Control> Factory { get; set; }

        /// <summary>
        /// 控件创建后的额外初始化
        /// </summary>
        public Action<Control> PostSetup { get; set; }

        public int PropertyFloodIterations { get; set; } = 500;
        public int ResizeIterations { get; set; } = 1000;
        public int InvalidateIterations { get; set; } = 2000;
        public int ColorCycleIterations { get; set; } = 500;

        /// <summary>
        /// 快速创建配置(适用于有 DemoConfig 且可直接 new 的控件)
        /// </summary>
        public static ControlTestConfig Create<TControl, TConfig>(string name, string category, Action<Control> postSetup = null)
            where TControl : Control, new()
            where TConfig : ControlDemoConfigBase, new()
        {
            return new ControlTestConfig
            {
                ControlName = name,
                Category = category,
                ControlType = typeof(TControl),
                DemoConfigType = typeof(TConfig),
                Factory = () => new TControl(),
                PostSetup = postSetup,
            };
        }

        /// <summary>
        /// 快速创建配置(适用于无 DemoConfig的通用场景)
        /// </summary>
        public static ControlTestConfig Create<TControl>(string name, string category, Action<Control> postSetup = null)
            where TControl : Control, new()
        {
            return new ControlTestConfig
            {
                ControlName = name,
                Category = category,
                ControlType = typeof(TControl),
                DemoConfigType = null,
                Factory = () => new TControl(),
                PostSetup = postSetup,
            };
        }
    }

    /// <summary>
    /// 基于反射的属性变异器
    /// 通过 DemoConfig 的属性元数据自动生成测试值
    /// </summary>
    public static class PropertyMutator
    {
        private static readonly HashSet<string> SkipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // 主题
                "UseTheme", "ThemeName",
                // 尺寸
                "Width", "Height",
                // 图像资源
                "Image", "Icon", "HeaderIcon", "CardImage", "TrayImage",
                // 快捷键
                "ShortcutKeys", "ToggleHotKey",
                // 控制状态
                "Enabled", "Visible", "ReadOnly", "CountdownMode",
                // 数据/只读属性
                "SampleItemCount", "TabCount",
            };

        private static readonly HashSet<Type> SkipTypes = new HashSet<Type>
        {
            typeof(Image), typeof(Bitmap),
            typeof(System.Drawing.Icon), typeof(Keys),
        };

        /// <summary>
        /// 获取 DemoConfig 上可用于变异的属性列表
        /// </summary>
        public static List<PropertyInfo> GetMutableProperties(Type configType)
        {
            return configType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => p.DeclaringType != typeof(ControlDemoConfigBase)) // 跳过基类
                .Where(p => !SkipNames.Contains(p.Name))
                .Where(p => !SkipTypes.Contains(p.PropertyType))
                .Where(p => !HasSkipAttribute(p))
                .Where(p => GenerateValue(p.PropertyType, 0) != null)
                .ToList();
        }

        /// <summary>
        /// 变更所有可变属性
        /// </summary>
        public static void MutateAll(object config, List<PropertyInfo> properties, int iteration)
        {
            foreach (var prop in properties)
            {
                try
                {
                    var value = GenerateValue(prop.PropertyType, iteration);
                    if (value != null)
                    {
                        prop.SetValue(config, value);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 根据类型和迭代索引生成测试值
        /// </summary>
        public static object GenerateValue(Type type, int iteration)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
            {
                type = underlying;
            }

            if (type == typeof(string))
            {
                return $"Test_{iteration}";
            }

            if (type == typeof(bool))
            {
                return iteration % 2 == 0;
            }

            if (type == typeof(int))
            {
                return 1 + Math.Abs(iteration % 50);
            }

            if (type == typeof(double))
            {
                return (double)Math.Abs(iteration % 100);
            }

            if (type == typeof(float))
            {
                return Math.Abs(iteration % 100) * 0.1f;
            }

            if (type == typeof(decimal))
            {
                return (decimal)Math.Abs(iteration % 100);
            }

            if (type == typeof(Color))
            {
                return Color.FromArgb(255,
                    Math.Abs(iteration * 37) % 256,
                    Math.Abs(iteration * 73) % 256,
                    Math.Abs(iteration * 113) % 256);
            }

            if (type == typeof(Size))
            {
                return new Size(
                    20 + Math.Abs(iteration % 30),
                    20 + Math.Abs(iteration % 20));
            }

            if (type == typeof(Point))
            {
                return new Point(
                    Math.Abs(iteration % 50),
                    Math.Abs(iteration % 50));
            }

            if (type == typeof(Padding))
            {
                return new Padding(Math.Abs(iteration % 10));
            }

            if (type == typeof(DateTime))
            {
                return new DateTime(2024, 1, 1).AddMinutes(iteration);
            }

            if (type == typeof(ContentAlignment))
            {
                var values = new[]
                {
                    ContentAlignment.TopLeft, ContentAlignment.TopCenter,
                    ContentAlignment.TopRight, ContentAlignment.MiddleLeft,
                    ContentAlignment.MiddleCenter, ContentAlignment.MiddleRight,
                    ContentAlignment.BottomLeft, ContentAlignment.BottomCenter,
                    ContentAlignment.BottomRight,
                };
                return values[Math.Abs(iteration) % values.Length];
            }

            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                return values.Length > 0
                    ? values.GetValue(Math.Abs(iteration) % values.Length)
                    : null;
            }

            return null;
        }

        private static bool HasSkipAttribute(PropertyInfo prop)
        {
            return prop.GetCustomAttributes(true).Any(a =>
            {
                var name = a.GetType().Name;
                return name == "PropertyIgnoreEditAttribute"
                    || name == "PropertyReadOnlyAttribute";
            });
        }
    }

    public enum DemoThemeName
    {
        None,
        Classic,
        FluentLight,
        FluentDark,
        HighContrast,
        Blue,
        Green,
        Purple
    }

}
