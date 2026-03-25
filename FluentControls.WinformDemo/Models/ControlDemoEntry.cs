using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.WinformDemo
{
    public class ControlDemoEntry
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        /// <summary>
        /// 创建预览控件实例
        /// </summary>
        public Func<Control> CreateControl { get; set; }

        /// <summary>
        /// 根据控件实例创建配置对象
        /// </summary>
        public Func<Control, ControlDemoConfigBase> CreateConfig { get; set; }

        /// <summary>
        /// 是否需要特殊展示方式
        /// </summary>
        public bool IsSpecialDemo { get; set; } = false;
    }

}
