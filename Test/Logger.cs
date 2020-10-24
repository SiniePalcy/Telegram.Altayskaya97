using Microsoft.Extensions.Logging;
using System;

namespace Telegram.Altayskaya97.Test
{
    public class Logger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch(logLevel)
            {
                case LogLevel.Information:
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Error:
                case LogLevel.Critical:
                case LogLevel.Warning:
                    return true;
                default:
                    return false;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var outputStr = state as string;
            if (!string.IsNullOrEmpty(outputStr))
            {
                Console.WriteLine(outputStr);
            }
        }
    }
}
