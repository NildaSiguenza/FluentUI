using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;

namespace FluentControls
{
    public static class FluentMessageNotifierExtension
    {
        /// <summary>
        /// 设置自定义过滤器(运行时)
        /// </summary>
        public static FluentMessageNotifier WithCustomFilter(this FluentMessageNotifier notifier, Func<MessageOptions, bool> shouldNotify,
            Func<MessageOptions, bool> isImportant = null)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException(nameof(notifier));
            }

            notifier.Filter = new NotificationFilter
            {
                ShouldNotify = shouldNotify,
                IsImportant = isImportant ?? (options => options.Type == MessageType.Error)
            };

            return notifier;
        }

        /// <summary>
        /// 设置过滤模式(链式调用)
        /// </summary>
        public static FluentMessageNotifier WithFilterMode(this FluentMessageNotifier notifier, NotificationFilterMode mode)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException(nameof(notifier));
            }

            notifier.FilterMode = mode;
            return notifier;
        }

        /// <summary>
        /// 根据消息类型过滤
        /// </summary>
        public static FluentMessageNotifier FilterByTypes(this FluentMessageNotifier notifier, params MessageType[] types)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException(nameof(notifier));
            }

            var typeSet = new HashSet<MessageType>(types);

            notifier.Filter = new NotificationFilter
            {
                ShouldNotify = options => typeSet.Contains(options.Type),
                IsImportant = options => options.Type == MessageType.Error
            };

            return notifier;
        }

        /// <summary>
        /// 根据关键字过滤
        /// </summary>
        public static FluentMessageNotifier FilterByKeywords(this FluentMessageNotifier notifier, params string[] keywords)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException(nameof(notifier));
            }

            notifier.Filter = new NotificationFilter
            {
                ShouldNotify = options =>
                {
                    var text = $"{options.Title} {options.Content}".ToLower();
                    return keywords.Any(k => text.Contains(k.ToLower()));
                },
                IsImportant = options => options.Type == MessageType.Error
            };

            return notifier;
        }

        /// <summary>
        /// 组合多个条件(AND)
        /// </summary>
        public static FluentMessageNotifier FilterByConditions(this FluentMessageNotifier notifier, params Func<MessageOptions, bool>[] conditions)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException(nameof(notifier));
            }

            notifier.Filter = new NotificationFilter
            {
                ShouldNotify = options => conditions.All(condition => condition(options)),
                IsImportant = options => options.Type == MessageType.Error
            };

            return notifier;
        }

    }

}
