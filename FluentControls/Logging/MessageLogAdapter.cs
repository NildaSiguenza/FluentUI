using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;

namespace FluentControls.Logging
{
    /// <summary>
    /// 消息日志适配器
    /// </summary>
    public static class MessageLogAdapter
    {
        /// <summary>
        /// 将消息类型转换为日志级别
        /// </summary>
        public static LogLevel ToLogLevel(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Success:
                    return LogLevel.Info;
                case MessageType.Warning:
                    return LogLevel.Warn;
                case MessageType.Info:
                    return LogLevel.Info;
                case MessageType.Error:
                    return LogLevel.Error;
                default:
                    return LogLevel.Info;
            }
        }

        /// <summary>
        /// 创建消息日志事件
        /// </summary>
        public static ILogEvent CreateMessageLogEvent(MessageOptions options)
        {
            if (options == null)
            {
                return null;
            }

            var logEvent = new DefaultLogEvent
            {
                Timestamp = DateTime.Now,
                Level = ToLogLevel(options.Type),
                Message = options.Content,
                Source = "MessageLogger",
                Group = "FluentMessage"
            };

            // 如果有标题，添加到属性中
            if (!string.IsNullOrWhiteSpace(options.Title))
            {
                logEvent.Properties["Title"] = options.Title;
            }

            // 添加消息类型
            logEvent.Properties["MessageType"] = options.Type.ToString();

            // 添加显示位置
            logEvent.Properties["Position"] = options.Position.ToString();

            // 添加显示模式
            logEvent.Properties["DisplayMode"] = options.DisplayMode.ToString();

            return logEvent;
        }

        /// <summary>
        /// 格式化消息日志
        /// </summary>
        public static string FormatMessageLog(MessageOptions options)
        {
            var sb = new StringBuilder();
            sb.Append($"[{options.Type}]");

            if (!string.IsNullOrWhiteSpace(options.Title))
            {
                sb.Append($" {options.Title}");
            }

            sb.Append($" - {options.Content}");

            return sb.ToString();
        }
    }
}
