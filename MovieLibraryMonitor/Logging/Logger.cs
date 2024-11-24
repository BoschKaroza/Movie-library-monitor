using Serilog;

namespace MovieLibraryMonitor.Logging;

public static class Logger
{
    public static ILogger Instance { get; private set; }

    static Logger()
    {
        Instance = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/movielibrarymonitor.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
