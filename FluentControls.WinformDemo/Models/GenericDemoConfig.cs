
using System.Drawing;
using System.Windows.Forms;
using FluentControls.Controls;

namespace FluentControls.WinformDemo
{
    public class GenericDemoConfig : ControlDemoConfigBase
    {
        [PropertyCategory("基本")]
        [PropertyDisplayName("是否启用")]
        public bool Enabled { get; set; } = true;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("宽度")]
        public int Width { get; set; } = 200;

        [PropertyCategory("尺寸")]
        [PropertyDisplayName("高度")]
        public int Height { get; set; } = 40;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("背景色")]
        public Color BackColor { get; set; } = Color.White;

        [PropertyCategory("颜色")]
        [PropertyDisplayName("前景色")]
        public Color ForeColor { get; set; } = Color.Black;

        public override void ApplyTo(Control ctrl)
        {
            base.ApplyTo(ctrl);
            ctrl.Enabled = Enabled;
            ctrl.Size = new Size(Width, Height);
            try { ctrl.BackColor = BackColor; } catch { }
            try { ctrl.ForeColor = ForeColor; } catch { }
        }

        public override void ReadFrom(Control ctrl)
        {
            base.ReadFrom(ctrl);
            Enabled = ctrl.Enabled;
            Width = ctrl.Width;
            Height = ctrl.Height;
            BackColor = ctrl.BackColor;
            ForeColor = ctrl.ForeColor;
        }
    }
}