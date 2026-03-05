using log4net;
using log4net.Core;

namespace WidgetSample.Avalonia;

public static class LoggingExtensions
{
    public static void Notice(this ILog log, string message, params object[] args)
    {
        LoggingEventData data = new() { Message = message, Level = Level.Notice };

        if (args?.Length > 0)
        {
            try { data.Message = string.Format(message, args); }
            catch { /* ignore format exceptions */ }
        }

        log.Logger.Log(new LoggingEvent(data));
    }
}

