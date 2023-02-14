using System;
using System.Diagnostics;
using System.Text;

namespace OCSoft.Common
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public delegate void EventLogHandler(EventLogger sender, EventLog value);

    public class EventLogger : System.IO.TextWriter
    {
        public event EventLogHandler OnDebug;
        public event EventLogHandler OnTrace;
        public event EventLogHandler OnWarning;
        public event EventLogHandler OnError;

        private static EventLogger _instance;
        public static EventLogger Instance => _instance ?? (_instance = new EventLogger());
        
        private readonly StringBuilder _logs = new StringBuilder();
        private readonly Lazy<Stopwatch> _watch = new Lazy<Stopwatch>(Stopwatch.StartNew);

        public Stopwatch Watch => _watch.Value;

        public override Encoding Encoding => Encoding.UTF32;

        public override void Write(char value)
        {
            _logs.Append(value);

            if (value == '\n')
            {
                Invoke(OnDebug, ToString(), LogLevel.Debug);
            }
        }

        public static void RegisterHandler(EventLogHandler handler, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    Instance.OnDebug += handler;
                    break;

                case LogLevel.Info:
                    Instance.OnTrace += handler;
                    Instance.OnWarning += handler;
                    Instance.OnError += handler;
                    break;

                case LogLevel.Warning:
                    Instance.OnWarning += handler;
                    Instance.OnError += handler;
                    break;

                case LogLevel.Error:
                    Instance.OnError += handler;
                    break;

                default:
                    break;
            }
        }

        public static void Debug(object format, params object[] args)
        {
            Instance.WriteLineWithTime(format, args);
        }

        public static void Trace(object format, params object[] args)
        {
            Instance.LogLineImpl(LogLevel.Info, format, args);
        }

        public static void Warning(object format, params object[] args)
        {
            Instance.LogLineImpl(LogLevel.Warning, format, args);
        }
        
        public static void Error(object format, params object[] args)
        {
            Instance.LogLineImpl(LogLevel.Error, format, args);
        }
        
        public override string ToString()
        {
            var logs = Instance._logs.ToString();
            Instance._logs.Clear();

            return logs;
        }

        private void LogLineImpl(LogLevel level, object format, object[] args)
        {
            WriteLineWithTime($"{level}: {format}", args);

            string message = string.Format($"{level}: {format}{Environment.NewLine}", args);
            switch (level)
            {
                case LogLevel.Info:
                    Invoke(OnTrace, message, level);
                    break;

                case LogLevel.Warning:
                    Invoke(OnWarning, message, level);
                    break;

                case LogLevel.Error:
                    Invoke(OnError, message, level);
                    break;

                default:
                    break;
            }
        }

        private void WriteLineWithTime(object format, params object[] args)
        {
            WriteLine($"{Watch.Elapsed} - {format}", args);
        }

        private void Invoke(EventLogHandler handler, string message, LogLevel level)
        {
            if (handler != null)
            {
                var entry = new EventLog(message, level);
                handler.Invoke(this, entry);
            }
        }
    }

    public class EventLog
    {
        private readonly string _value;
        
        public LogLevel Level { get; }

        public EventLog(string value, LogLevel level)
        {
            _value = value;
            Level = level;
        }

        public override string ToString()
        {
            return _value; 
        }
    }
}
