using Avalonia;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ATC4_HQ;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var services = ConfigureServices();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        LoggerHelper.Initialize(logger);
        logger.LogInformation("应用程序启动");
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        var logPath = System.IO.Path.Combine(AppContext.BaseDirectory, "logs");
        
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddFile(logPath, LogLevel.Information);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services.BuildServiceProvider();
    }
}
