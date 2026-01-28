using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls.Logging
{
    /// <summary>
    /// Serilog日志事件适配器
    /// </summary>
    public class SerilogEventAdapter : ILogEvent
    {
        private readonly object serilogEvent; // Serilog.Events.LogEvent
        private readonly Type logEventType;
        private DateTime? timestamp;
        private LogLevel? level;
        private string message;
        private string source;
        private Exception exception;
        private string group;
        private Dictionary<string, object> properties;

        public SerilogEventAdapter(object logEvent)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));

            serilogEvent = logEvent;
            logEventType = logEvent.GetType();
        }

        public DateTime Timestamp
        {
            get
            {
                if (!timestamp.HasValue)
                {
                    try
                    {
                        var prop = logEventType.GetProperty("Timestamp");
                        if (prop != null)
                        {
                            var dateTimeOffset = (DateTimeOffset)prop.GetValue(serilogEvent);
                            timestamp = dateTimeOffset.DateTime;
                        }
                        else
                        {
                            timestamp = DateTime.Now;
                        }
                    }
                    catch
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
                    try
                    {
                        var prop = logEventType.GetProperty("Level");
                        if (prop != null)
                        {
                            var serilogLevel = prop.GetValue(serilogEvent).ToString();
                            level = ConvertSerilogLevel(serilogLevel);
                        }
                        else
                        {
                            level = LogLevel.Info;
                        }
                    }
                    catch
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
                    try
                    {
                        // 方法1: 使用 MessageTemplate.Text 属性(最简单可靠)
                        var messageTemplateProp = logEventType.GetProperty("MessageTemplate");
                        if (messageTemplateProp != null)
                        {
                            var messageTemplate = messageTemplateProp.GetValue(serilogEvent);
                            if (messageTemplate != null)
                            {
                                var textProp = messageTemplate.GetType().GetProperty("Text");
                                if (textProp != null)
                                {
                                    var templateText = textProp.GetValue(messageTemplate) as string ?? "";

                                    // 替换模板中的占位符
                                    message = RenderMessageTemplate(templateText);
                                }
                            }
                        }

                        // 如果上面失败，使用 ToString
                        if (string.IsNullOrEmpty(message))
                        {
                            message = serilogEvent.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SerilogEventAdapter.Message error: {ex.Message}");
                        message = serilogEvent.ToString();
                    }
                }
                return message;
            }
        }

        /// <summary>
        /// 渲染消息模板
        /// </summary>
        private string RenderMessageTemplate(string template)
        {
            try
            {
                var props = Properties;
                var result = template;

                // 简单的模板替换(支持 {PropertyName} 格式)
                foreach (var kvp in props)
                {
                    var placeholder = "{" + kvp.Key + "}";
                    if (result.Contains(placeholder))
                    {
                        result = result.Replace(placeholder, kvp.Value?.ToString() ?? "");
                    }
                }

                return result;
            }
            catch
            {
                return template;
            }
        }

        public string Source
        {
            get
            {
                if (source == null)
                {
                    try
                    {
                        // 首先尝试从 Properties 中获取 SourceContext
                        var props = Properties;
                        if (props.ContainsKey("SourceContext"))
                        {
                            source = props["SourceContext"]?.ToString() ?? "";
                        }
                        else
                        {
                            // 尝试从异常中获取
                            var exceptionProp = logEventType.GetProperty("Exception");
                            if (exceptionProp != null)
                            {
                                var ex = exceptionProp.GetValue(serilogEvent) as Exception;
                                source = ex?.Source ?? "";
                            }
                            else
                            {
                                source = "";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SerilogEventAdapter.Source error: {ex.Message}");
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
                    try
                    {
                        var prop = logEventType.GetProperty("Exception");
                        if (prop != null)
                        {
                            exception = prop.GetValue(serilogEvent) as Exception;
                        }
                    }
                    catch
                    {
                        exception = null;
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
                    try
                    {
                        var props = Properties;
                        if (props.ContainsKey("Group"))
                        {
                            group = props["Group"]?.ToString() ?? "";
                        }
                    }
                    catch
                    {
                        group = "";
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
                            var propsDict = propsProp.GetValue(serilogEvent);
                            if (propsDict != null)
                            {
                                // 使用反射遍历字典
                                var dictType = propsDict.GetType();

                                // 尝试使用 foreach
                                var enumerableType = typeof(System.Collections.IEnumerable);
                                if (enumerableType.IsAssignableFrom(dictType))
                                {
                                    foreach (var kvp in (System.Collections.IEnumerable)propsDict)
                                    {
                                        if (kvp != null)
                                        {
                                            var kvpType = kvp.GetType();
                                            var keyProp = kvpType.GetProperty("Key");
                                            var valueProp = kvpType.GetProperty("Value");

                                            if (keyProp != null && valueProp != null)
                                            {
                                                var key = keyProp.GetValue(kvp) as string;
                                                var value = valueProp.GetValue(kvp);

                                                // 尝试获取 ScalarValue 的值
                                                if (value != null && value.GetType().Name == "ScalarValue")
                                                {
                                                    var valuePropOfScalar = value.GetType().GetProperty("Value");
                                                    value = valuePropOfScalar?.GetValue(value);
                                                }

                                                if (key != null)
                                                {
                                                    properties[key] = value;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SerilogEventAdapter.Properties error: {ex.Message}");
                    }
                }
                return properties;
            }
        }

        private LogLevel ConvertSerilogLevel(string serilogLevel)
        {
            switch (serilogLevel)
            {
                case "Verbose":
                    return LogLevel.Trace;
                case "Debug":
                    return LogLevel.Debug;
                case "Information":
                    return LogLevel.Info;
                case "Warning":
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
