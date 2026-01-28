using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Logging
{
    /// <summary>
    /// 默认日志事件实现
    /// </summary>
    public class DefaultLogEvent : ILogEvent
    {
        public DefaultLogEvent()
        {
            Timestamp = DateTime.Now;
            Properties = new Dictionary<string, object>();
        }

        public DefaultLogEvent(LogLevel level, string message, string source = null, Exception exception = null, string group = null)
            : this()
        {
            Level = level;
            Message = message;
            Source = source;
            Exception = exception;
            Group = group;
        }

        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public Exception Exception { get; set; }
        public string Group { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    /// <summary>
    /// 通用日志事件接口
    /// </summary>
    public interface ILogEvent
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// 日志级别
        /// </summary>
        LogLevel Level { get; }

        /// <summary>
        /// 日志消息
        /// </summary>
        string Message { get; }

        /// <summary>
        /// 日志源/记录器名称
        /// </summary>
        string Source { get; }

        /// <summary>
        /// 异常信息
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// 日志分组
        /// </summary>
        string Group { get; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        Dictionary<string, object> Properties { get; }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    [Flags]
    public enum LogLevel : byte
    {
        None = 0,
        Trace = 1,
        Debug = 2,
        Info = 4,
        Warn = 8,
        Error = 16,
        Fatal = 32,
        All = Trace | Debug | Info | Warn | Error | Fatal
    }

}
