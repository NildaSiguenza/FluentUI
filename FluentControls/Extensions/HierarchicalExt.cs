using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;
using Infrastructure;

namespace FluentControls
{
    public static class HierarchicalExtensions
    {
        public static IHierarchicalItem<T> AsHierarchical<T>(
            this T item,
            Func<T, IEnumerable<T>> childSelector,
            Func<T, string> displayTextSelector = null,
            Func<T, Image> iconSelector = null,
            Func<T, bool> isEnabledSelector = null,
            Func<T, bool> isExpandedSelector = null,
            Func<T, object> tagSelector = null)
        {
            return new HierarchicalItem<T>(
                item,
                childSelector,
                displayTextSelector,
                iconSelector,
                isEnabledSelector,
                isExpandedSelector,
                tagSelector);
        }

        /// <summary>
        /// 将层次结构项转换为树节点
        /// </summary>
        public static FluentTreeNode ToTreeNode<T>(this IHierarchicalItem<T> hierarchicalItem)
        {
            var node = new FluentTreeNode(hierarchicalItem.DisplayText, hierarchicalItem.Icon)
            {
                Tag = hierarchicalItem.Tag ?? hierarchicalItem.CurrentItem,
                IsEnabled = hierarchicalItem.IsEnabled,
                IsExpanded = hierarchicalItem.IsExpanded
            };

            foreach (var child in hierarchicalItem.Childs)
            {
                node.Nodes.Add(child.ToTreeNode());
            }

            return node;
        }

        /// <summary>
        /// 批量转换
        /// </summary>
        public static IEnumerable<FluentTreeNode> ToTreeNodes<T>(this IEnumerable<IHierarchicalItem<T>> items)
        {
            return items.Select(item => item.ToTreeNode());
        }
    }
}
