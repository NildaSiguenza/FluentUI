using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;
using System.Windows.Forms;

namespace FluentControls
{
    /// <summary>
    /// Fluent项目容器接口
    /// </summary>
    public interface IFluentItemContainer
    {
        /// <summary>
        /// 是否使用主题
        /// </summary>
        bool UseTheme { get; }

        /// <summary>
        /// 主题
        /// </summary>
        IFluentTheme Theme { get; }

        /// <summary>
        /// 字体
        /// </summary>
        Font Font { get; }

        /// <summary>
        /// 控件集合
        /// </summary>
        Control.ControlCollection Controls { get; }

        /// <summary>
        /// 站点
        /// </summary>
        ISite Site { get; }

        /// <summary>
        /// 是否已创建句柄
        /// </summary>
        bool IsHandleCreated { get; }

        /// <summary>
        /// 句柄创建事件
        /// </summary>
        event EventHandler HandleCreated;

        /// <summary>
        /// 执行布局
        /// </summary>
        void PerformLayout();

        /// <summary>
        /// 使控件无效
        /// </summary>
        void Invalidate();

        /// <summary>
        /// 项目状态改变
        /// </summary>
        void ItemStateChanged(FluentToolStripItem item);

        /// <summary>
        /// 将点转换为屏幕坐标
        /// </summary>
        Point PointToScreen(Point point);
    }
}
