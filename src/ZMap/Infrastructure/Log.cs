namespace ZMap.Infrastructure;

public static class Log
{
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
        var logger = _loggerFactory.CreateLogger<T>();
        return logger;
    }

    public static ILogger CreateLogger(string name)
    {
        return _loggerFactory.CreateLogger(name);
    }

    public static ILogger CreateLogger(Type type)
    {
        return _loggerFactory.CreateLogger(type);
    }
}