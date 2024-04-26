using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ZMap.Infrastructure;

public static class Log
{
    // public static ILogger Logger = NullLogger.Instance;
    private static ILoggerFactory _loggerFactory;

    static Log()
    {
        _loggerFactory = new LoggerFactory();
        _loggerFactory.AddProvider(NullLoggerProvider.Instance);
    }

    public static void SetLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public static ILogger CreateLogger<T>()
    {
        return _loggerFactory.CreateLogger<T>();
    }

    public static ILogger CreateLogger(string name)
    {
        return _loggerFactory.CreateLogger(name);
    }
}