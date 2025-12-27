using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    /// <summary>
    /// 层次结构项接口
    /// </summary>
    public interface IHierarchicalItem<T>
    {
        T CurrentItem { get; }

        IEnumerable<IHierarchicalItem<T>> Childs { get; }

        string DisplayText { get; }

        Image Icon { get; }

        bool IsEnabled { get; }

        bool IsExpanded { get; }

        object Tag { get; }
    }

    /// <summary>
    /// 默认实现
    /// </summary>
    public class HierarchicalItem<T> : IHierarchicalItem<T>
    {
        private readonly T item;
        private readonly Func<T, IEnumerable<T>> childItemsSelector;
        private readonly Func<T, string> displayTextSelector;
        private readonly Func<T, Image> iconSelector;
        private readonly Func<T, bool> isEnabledSelector;
        private readonly Func<T, bool> isExpandedSelector;
        private readonly Func<T, object> tagSelector;

        public HierarchicalItem(
            T item,
            Func<T, IEnumerable<T>> childSelector,
            Func<T, string> displayTextSelector = null,
            Func<T, Image> iconSelector = null,
            Func<T, bool> isEnabledSelector = null,
            Func<T, bool> isExpandedSelector = null,
            Func<T, object> tagSelector = null)
        {
            this.item = item;
            this.childItemsSelector = childSelector;
            this.displayTextSelector = displayTextSelector ?? (x => x?.ToString());
            this.iconSelector = iconSelector;
            this.isEnabledSelector = isEnabledSelector ?? (x => true);
            this.isExpandedSelector = isExpandedSelector ?? (x => false);
            this.tagSelector = tagSelector ?? (x => x);
        }

        public T CurrentItem => item;

        public string DisplayText => displayTextSelector(item);

        public Image Icon => iconSelector?.Invoke(item);

        public bool IsEnabled => isEnabledSelector(item);

        public bool IsExpanded => isExpandedSelector(item);

        public object Tag => tagSelector(item);

        public IEnumerable<IHierarchicalItem<T>> Childs
        {
            get
            {
                var childs = childItemsSelector(item);
                if (childs == null)
                    yield break;

                foreach (T childItem in childs)
                {
                    yield return new HierarchicalItem<T>(
                        childItem,
                        childItemsSelector,
                        displayTextSelector,
                        iconSelector,
                        isEnabledSelector,
                        isExpandedSelector,
                        tagSelector);
                }
            }
        }
    }

}
