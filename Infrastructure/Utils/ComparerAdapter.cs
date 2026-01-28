using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    /// <summary>
    /// Comparer适配器
    /// </summary>
    public class ComparerAdapter<T> : IComparer<T>
    {
        private readonly IComparer comparer;

        public ComparerAdapter(IComparer comparer)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int Compare(T x, T y)
        {
            return comparer.Compare(x, y);
        }
    }
}
