using Avalonia;
using Avalonia.ReactiveUI;
using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.ViewModels;
using BiermanTech.ProjectManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using BiermanTech.ProjectManager.Controls;

namespace BiermanTech.ProjectManager;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var services = ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
                dbContext.Database.Migrate();

                if (!dbContext.Projects.Any())
                {
                    var defaultProject = new Project
                    {
                        Name = "Default Project",
                        Author = "Unknown"
                    };
                    dbContext.Projects.Add(defaultProject);
                    dbContext.SaveChanges();
                    Log.Information("Created default project with ID {ProjectId}", defaultProject.Id);
                }
            }

            BuildAvaloniaApp(serviceProvider)
                .StartWithClassicDesktopLifetime(args);
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

    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.db");
        services.AddDbContext<ProjectDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddSingleton<TaskFileService>();

        services.AddSingleton<ITaskRepository>(provider =>
        {
            var dbContext = provider.GetRequiredService<ProjectDbContext>();
            var projectId = dbContext.Projects.First().Id;
            return new DbTaskRepository(dbContext, projectId);
        });

        services.AddSingleton<ICommandFactory>(provider =>
        {
            var dbContext = provider.GetRequiredService<ProjectDbContext>();
            var projectId = dbContext.Projects.First().Id;
            var taskFileService = provider.GetRequiredService<TaskFileService>();
            return new CommandFactory(dbContext, projectId, taskFileService);
        });

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<GanttChartViewModel>();
        services.AddTransient<TaskDialogViewModel>();
        services.AddTransient<MenuBarViewModel>();

        services.AddSingleton<ICommandManager, CommandManager>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<TaskDataSeeder>();

        return services;
    }
}