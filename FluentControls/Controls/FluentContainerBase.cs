using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent风格容器控件基类
    /// </summary>
    public abstract class FluentContainerBase : FluentControlBase, IThemeContainer
    {
        private bool enableChildThemeInheritance = true;

        /// <summary>
        /// 是否启用子控件主题继承
        /// </summary>
        [Category("Fluent")]
        [DefaultValue(true)]
        [Description("是否将主题应用到子控件")]
        public bool EnableChildThemeInheritance
        {
            get => enableChildThemeInheritance;
            set
            {
                if (enableChildThemeInheritance != value)
                {
                    enableChildThemeInheritance = value;
                    if (value && UseTheme && !string.IsNullOrEmpty(ThemeName))
                    {
                        ApplyThemeToChildren(true);
                    }
                }
            }
        }

        bool IThemeContainer.EnableThemeInheritance
        {
            get => EnableChildThemeInheritance;
            set => EnableChildThemeInheritance = value;
        }

        /// <summary>
        /// 当控件被添加时
        /// </summary>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            // 如果是在设计时且启用了主题继承
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                ApplyThemeToControl(e.Control, true);
            }
        }

        /// <summary>
        /// 当主题改变时
        /// </summary>
        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();

            // 传播主题变更到子控件
            if (EnableChildThemeInheritance && UseTheme && !string.IsNullOrEmpty(ThemeName))
            {
                ApplyThemeToChildren(true);
            }
        }

        /// <summary>
        /// 应用主题到所有子控件
        /// </summary>
        public virtual void ApplyThemeToChildren(bool recursive)
        {
            if (!EnableChildThemeInheritance || !UseTheme || string.IsNullOrEmpty(ThemeName))
            {
                return;
            }

            foreach (Control child in Controls)
            {
                ApplyThemeToControl(child, recursive);
            }
        }

        /// <summary>
        /// 应用主题到指定控件
        /// </summary>
        protected virtual void ApplyThemeToControl(Control control, bool recursive)
        {
            if (control == null)
            {
                return;
            }

            // 如果是FluentControlBase派生类
            if (control is FluentControlBase fluentControl)
            {
                // 强制继承主题(不管 EnableThemeInheritance 的值)
                fluentControl.InheritThemeFrom(this);
            }

            // 递归处理子控件的子控件
            if (recursive && control.HasChildren)
            {
                if (control is IThemeContainer container)
                {
                    // 如果子控件也是容器, 让它继承主题并处理自己的子控件
                    if (control is FluentControlBase fc)
                    {
                        fc.InheritThemeFrom(this);
                    }
                    container.ApplyThemeToChildren(true);
                }
                else
                {
                    // 否则递归处理
                    foreach (Control child in control.Controls)
                    {
                        ApplyThemeToControl(child, true);
                    }
                }
            }
        }

    }
}
