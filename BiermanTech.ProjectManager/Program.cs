using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager;

class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var services = new ServiceCollection();
            services.AddAppServices();
            using var serviceProvider = services.BuildServiceProvider();

            // Start the application
            BuildAvaloniaApp(serviceProvider)
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application crashed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI()
            .AfterSetup(builder =>
            {
                ((App)builder.Instance).ServiceProvider = serviceProvider;
            });
}