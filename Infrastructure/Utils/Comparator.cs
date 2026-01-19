using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// 泛型比较器
    /// </summary>
    public static class Comparator<T>
    {
        public static IComparer<T> CreateComparer(IComparer comparer)
        {
            return new ComparerAdapter<T>(comparer);
        }

        public static IComparer<T> CreateComparer<V>(Func<T, V> keySelector)
        {
            return new ComparerAdapter<T, V>(keySelector);
        }

        public static IComparer<T> CreateComparer<V>(Func<T, V> keySelector, IComparer<V> comparer)
        {
            return new ComparerAdapter<T, V>(keySelector, comparer);
        }
    }

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

    public class ComparerAdapter<T, V> : IComparer<T>
    {
        private Func<T, V> keySelector;
        private IComparer<V> comparer;

        public ComparerAdapter(Func<T, V> keySelector, IComparer<V> comparer)
        {
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public ComparerAdapter(Func<T, V> keySelector)
            : this(keySelector, Comparer<V>.Default)
        { }

        public int Compare(T x, T y)
        {
            return comparer.Compare(keySelector(x), keySelector(y));
        }
    }
}
