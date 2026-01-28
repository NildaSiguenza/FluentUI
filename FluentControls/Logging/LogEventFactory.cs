using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Logging
{
    /// <summary>
    /// 日志事件工厂
    /// </summary>
    public static class LogEventFactory
    {
        /// <summary>
        /// 从对象创建日志事件
        /// </summary>
        public static ILogEvent CreateLogEvent(object logEvent)
        {
            if (logEvent == null)
            {
                return null;
            }

            if (logEvent is ILogEvent logEventInterface)
            {
                return logEventInterface;
            }

            var typeName = logEvent.GetType().FullName;

            // Serilog
            if (typeName.Contains("Serilog"))
            {
                return new SerilogEventAdapter(logEvent);
            }

            // NLog
            if (typeName.Contains("NLog"))
            {
                return new NLogEventAdapter(logEvent);
            }

            // 未知类型，尝试作为默认类型
            return new DefaultLogEvent
            {
                Message = logEvent.ToString(),
                Level = LogLevel.Info
            };
        }
    }

}
