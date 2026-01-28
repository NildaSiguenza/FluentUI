using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Logging
{
    /// <summary>
    /// NLog日志事件适配器
    /// </summary>
    public class NLogEventAdapter : ILogEvent
    {
        private readonly object nlogEvent; // NLog.LogEventInfo
        private readonly Type logEventType;
        private DateTime? timestamp;
        private LogLevel? level;
        private string message;
        private string source;
        private Exception exception;
        private string group;
        private Dictionary<string, object> properties;

        public NLogEventAdapter(object logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            nlogEvent = logEvent;
            logEventType = logEvent.GetType();
        }

        public DateTime Timestamp
        {
            get
            {
                if (!timestamp.HasValue)
                {
                    var prop = logEventType.GetProperty("TimeStamp");
                    if (prop != null)
                    {
                        timestamp = (DateTime)prop.GetValue(nlogEvent);
                    }
                    else
                    {
                        timestamp = DateTime.Now;
                    }
                }
                return timestamp.Value;
            }
        }

        public LogLevel Level
        {
            get
            {
                if (!level.HasValue)
                {
                    var prop = logEventType.GetProperty("Level");
                    if (prop != null)
                    {
                        var nlogLevel = prop.GetValue(nlogEvent);
                        var levelName = nlogLevel.GetType().GetProperty("Name")?.GetValue(nlogLevel) as string;
                        level = ConvertNLogLevel(levelName);
                    }
                    else
                    {
                        level = LogLevel.Info;
                    }
                }
                return level.Value;
            }
        }

        public string Message
        {
            get
            {
                if (message == null)
                {
                    var prop = logEventType.GetProperty("FormattedMessage");
                    if (prop != null)
                    {
                        message = prop.GetValue(nlogEvent) as string ?? "";
                    }
                    else
                    {
                        message = nlogEvent.ToString();
                    }
                }
                return message;
            }
        }

        public string Source
        {
            get
            {
                if (source == null)
                {
                    var prop = logEventType.GetProperty("LoggerName");
                    if (prop != null)
                    {
                        source = prop.GetValue(nlogEvent) as string ?? "";
                    }
                    else
                    {
                        source = "";
                    }
                }
                return source;
            }
        }

        public Exception Exception
        {
            get
            {
                if (exception == null)
                {
                    var prop = logEventType.GetProperty("Exception");
                    if (prop != null)
                    {
                        exception = prop.GetValue(nlogEvent) as Exception;
                    }
                }
                return exception;
            }
        }

        public string Group
        {
            get
            {
                if (group == null)
                {
                    group = "";
                    var props = Properties;
                    if (props.ContainsKey("Group"))
                    {
                        group = props["Group"]?.ToString() ?? "";
                    }
                }
                return group;
            }
        }

        public Dictionary<string, object> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                    try
                    {
                        var propsProp = logEventType.GetProperty("Properties");
                        if (propsProp != null)
                        {
                            var propsDict = propsProp.GetValue(nlogEvent) as System.Collections.IDictionary;
                            if (propsDict != null)
                            {
                                foreach (var key in propsDict.Keys)
                                {
                                    properties[key.ToString()] = propsDict[key];
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 忽略属性解析错误
                    }
                }
                return properties;
            }
        }

        private LogLevel ConvertNLogLevel(string nlogLevel)
        {
            switch (nlogLevel)
            {
                case "Trace":
                    return LogLevel.Trace;
                case "Debug":
                    return LogLevel.Debug;
                case "Info":
                    return LogLevel.Info;
                case "Warn":
                    return LogLevel.Warn;
                case "Error":
                    return LogLevel.Error;
                case "Fatal":
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Info;
            }
        }
    }

}
