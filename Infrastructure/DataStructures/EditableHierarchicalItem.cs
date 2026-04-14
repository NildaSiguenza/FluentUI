using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// 可编辑的层次化结构
    /// </summary>
    public class EditableHierarchicalItem<T> : IHierarchicalItem<T> where T : class
    {
        private T item;
        private List<EditableHierarchicalItem<T>> children;
        private Func<T, string> displayTextSelector;
        private Func<T, Image> iconSelector;
        private Func<T, bool> isEnabledSelector;
        private bool isExpanded;

        public EditableHierarchicalItem(T item, Func<T, string> displayTextSelector = null, Func<T, Image> iconSelector = null,
            Func<T, bool> isEnabledSelector = null, bool isExpanded = true)
        {
            this.item = item;
            this.children = new List<EditableHierarchicalItem<T>>();
            this.displayTextSelector = displayTextSelector ?? (x => x?.ToString());
            this.iconSelector = iconSelector;
            this.isEnabledSelector = isEnabledSelector ?? (x => true);
            this.isExpanded = isExpanded;
        }

        public T CurrentItem
        {
            get => item;
            set => item = value;
        }

        public string DisplayText => displayTextSelector(item);

        public Image Icon => iconSelector?.Invoke(item);

        public bool IsEnabled => isEnabledSelector(item);

        public bool IsExpanded
        {
            get => isExpanded;
            set => isExpanded = value;
        }

        public object Tag => item;

        public IEnumerable<IHierarchicalItem<T>> Childs => children;

        /// <summary>
        /// 子项集合(可编辑)
        /// </summary>
        public List<EditableHierarchicalItem<T>> Children => children;

        /// <summary>
        /// 父项
        /// </summary>
        public EditableHierarchicalItem<T> Parent { get; private set; }

        /// <summary>
        /// 添加子项
        /// </summary>
        public EditableHierarchicalItem<T> AddChild(T childItem)
        {
            var child = new EditableHierarchicalItem<T>(
                childItem,
                displayTextSelector,
                iconSelector,
                isEnabledSelector)
            {
                Parent = this
            };
            children.Add(child);
            return child;
        }

        /// <summary>
        /// 添加子项
        /// </summary>
        public void AddChild(EditableHierarchicalItem<T> child)
        {
            if (child != null)
            {
                child.Parent = this;
                children.Add(child);
            }
        }

        /// <summary>
        /// 移除子项
        /// </summary>
        public bool RemoveChild(EditableHierarchicalItem<T> child)
        {
            if (child != null && children.Contains(child))
            {
                child.Parent = null;
                return children.Remove(child);
            }
            return false;
        }

        /// <summary>
        /// 清空子项
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in children)
            {
                child.Parent = null;
            }
            children.Clear();
        }

        /// <summary>
        /// 从现有的 IHierarchicalItem 创建可编辑版本
        /// </summary>
        public static EditableHierarchicalItem<T> FromHierarchicalItem(
            IHierarchicalItem<T> source,
            Func<T, string> displayTextSelector = null,
            Func<T, Image> iconSelector = null,
            Func<T, bool> isEnabledSelector = null)
        {
            var result = new EditableHierarchicalItem<T>(
                source.CurrentItem,
                displayTextSelector ?? (x => source.DisplayText),
                iconSelector ?? (x => source.Icon),
                isEnabledSelector ?? (x => source.IsEnabled),
                source.IsExpanded);

            if (source.Childs != null)
            {
                foreach (var child in source.Childs)
                {
                    var editableChild = FromHierarchicalItem(child, displayTextSelector, iconSelector, isEnabledSelector);
                    result.AddChild(editableChild);
                }
            }

            return result;
        }
    }
}
