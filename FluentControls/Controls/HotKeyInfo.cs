using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public class HotKeyInfo
    {
        public Keys Key { get; set; }

        public Keys Modifiers { get; set; }
    }

    public class HotKeyEventArgs : EventArgs
    {
        public HotKeyEventArgs(Keys key, Keys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public Keys Key { get; }

        public Keys Modifiers { get; }

    }

}
