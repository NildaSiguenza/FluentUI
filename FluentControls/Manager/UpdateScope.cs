using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    /// <summary>
    /// 批量更新
    /// </summary>
    public class UpdateScope : IDisposable
    {
        private readonly FluentControlBase control;
        private bool disposed = false;

        public UpdateScope(FluentControlBase control)
        {
            this.control = control ?? throw new ArgumentNullException(nameof(control));
            control.BeginUpdate();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                control.EndUpdate();
                disposed = true;
            }
        }
    }
}
