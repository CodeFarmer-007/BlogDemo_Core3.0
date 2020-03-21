using log4net;
using log4net.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

namespace BlogDemo.Core.Common.LogHelper
{
    public class Log4NetLogger : ILogger
    {
        private readonly string _name;
        private readonly XmlElement _xmlElement;
        private readonly ILog _log;
        private ILoggerRepository _loggerRepository;

        public Log4NetLogger(string name, XmlElement xmlElement)
        {
            _name = name;
            _xmlElement = xmlElement;
            _loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            _log = LogManager.GetLogger(_loggerRepository.Name, name);
            log4net.Config.XmlConfigurator.Configure(_loggerRepository, xmlElement);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug: //调试
                    return _log.IsDebugEnabled;
                case LogLevel.Information: //信息
                    return _log.IsInfoEnabled;
                case LogLevel.Warning: //警告
                    return _log.IsWarnEnabled;
                case LogLevel.Error: //错误
                    return _log.IsErrorEnabled;
                case LogLevel.Critical: //关键的
                    return _log.IsFatalEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            //var colored = ConsoleColor.White;
            //switch (logLevel)
            //{
            //    case LogLevel.Trace:
            //    case LogLevel.Debug:
            //        colored = ConsoleColor.Cyan;
            //        break;
            //    case LogLevel.Information:
            //        colored = ConsoleColor.White;
            //        break;
            //    case LogLevel.Warning:
            //        colored = ConsoleColor.Yellow;
            //        break;
            //    case LogLevel.Error:
            //        colored = ConsoleColor.Red;
            //        break;
            //    case LogLevel.Critical:
            //        colored = ConsoleColor.Blue;
            //        break;
            //    default:
            //        colored = ConsoleColor.White;
            //        break;
            //}

            //var color = Console.ForegroundColor;
            //Console.ForegroundColor = colored;
            //Console.WriteLine($"{logLevel.ToString()} - {_name} - {formatter(state, exception)}");
            //Console.ForegroundColor = color;

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        _log.Debug(message);
                        break;
                    case LogLevel.Information:
                        _log.Info(message);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(message);
                        break;
                    case LogLevel.Error:
                        _log.Error(message);
                        break;
                    case LogLevel.Critical:
                        _log.Fatal(message);
                        break;
                    default:
                        _log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        _log.Info(message, exception);
                        break;
                }
            }
        }
    }
}
