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
    public static class HierarchicalExtension
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

        /// <summary>
        /// 获取所有后代项
        /// </summary>
        public static IEnumerable<IHierarchicalItem<T>> GetAllDescendants<T>(this IHierarchicalItem<T> item)
        {
            if (item.Childs == null)
            {
                yield break;
            }

            foreach (var child in item.Childs)
            {
                yield return child;
                foreach (var descendant in child.GetAllDescendants())
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 根据条件查找项
        /// </summary>
        public static IHierarchicalItem<T> Find<T>(this IHierarchicalItem<T> item, Func<IHierarchicalItem<T>, bool> predicate)
        {
            if (predicate(item))
            {
                return item;
            }

            if (item.Childs != null)
            {
                foreach (var child in item.Childs)
                {
                    var found = child.Find(predicate);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 根据显示文本查找项
        /// </summary>
        public static IHierarchicalItem<T> FindByDisplayText<T>(this IHierarchicalItem<T> item, string displayText)
        {
            return item.Find(x => x.DisplayText == displayText);
        }

        /// <summary>
        /// 是否有子项
        /// </summary>
        public static bool HasChildren<T>(this IHierarchicalItem<T> item)
        {
            return item.Childs != null && item.Childs.Any();
        }

        /// <summary>
        /// 获取层级深度
        /// </summary>
        public static int GetDepth<T>(this IHierarchicalItem<T> item, IHierarchicalItem<T> root)
        {
            int depth = 0;
            var current = root.Find(x => ReferenceEquals(x, item));
            // 深度需要通过遍历计算
            return depth;
        }

        /// <summary>
        /// 转换为列表
        /// </summary>
        public static List<T> ToFlatList<T>(this IHierarchicalItem<T> root)
        {
            var result = new List<T>();

            if (root.CurrentItem != null)
            {
                result.Add(root.CurrentItem);
            }

            foreach (var descendant in root.GetAllDescendants())
            {
                if (descendant.CurrentItem != null)
                {
                    result.Add(descendant.CurrentItem);
                }
            }

            return result;
        }
    }
}
