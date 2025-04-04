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
using System.Threading.Tasks;
using BiermanTech.ProjectManager.Controls;
using System.Collections.Generic;
using System.Linq;

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
            var services = ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();

            // Apply migrations (this will trigger UseAsyncSeeding)
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
                await dbContext.Database.MigrateAsync();
                Log.Information("Database migrations applied and seeding completed.");
            }

            // Start the application
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
            options.UseSqlite($"Data Source={dbPath}")
                   .UseAsyncSeeding(async (context, _, ct) =>
                   {
                       // Cast context to ProjectDbContext
                       var projectContext = (ProjectDbContext)context;

                       if (await projectContext.Projects.AnyAsync(ct)) // Line 78
                       {
                           return;
                       }

                       var project = new Project
                       {
                           Name = "Sample Project",
                           Author = "BiermanTech Team",
                           Narrative = new ProjectNarrative
                           {
                               Situation = "This is a sample project to demonstrate the project manager.",
                               CurrentState = "Initial planning phase.",
                               Plan = "Develop a project plan, assign tasks, and track progress.",
                               Results = "Not yet completed."
                           }
                       };

                       var task1 = new TaskItem
                       {
                           Name = "Define Requirements",
                           StartDate = DateTimeOffset.Now,
                           Duration = TimeSpan.FromDays(3),
                           PercentComplete = 50,
                           Project = project
                       };

                       var task2 = new TaskItem
                       {
                           Name = "Design System",
                           StartDate = DateTimeOffset.Now.AddDays(3),
                           Duration = TimeSpan.FromDays(5),
                           PercentComplete = 0,
                           Project = project
                       };

                       var task3 = new TaskItem
                       {
                           Name = "Implement Features",
                           StartDate = DateTimeOffset.Now.AddDays(8),
                           Duration = TimeSpan.FromDays(10),
                           PercentComplete = 0,
                           Project = project
                       };

                       project.Tasks = new List<TaskItem> { task1, task2, task3 };

                       projectContext.Projects.Add(project);
                       await projectContext.SaveChangesAsync(ct);

                       var task2Dependency = new TaskDependency
                       {
                           TaskId = task2.Id,
                           DependsOnId = task1.Id
                       };

                       var task3Dependency = new TaskDependency
                       {
                           TaskId = task3.Id,
                           DependsOnId = task2.Id
                       };

                       task2.TaskDependencies = new List<TaskDependency> { task2Dependency };
                       task3.TaskDependencies = new List<TaskDependency> { task3Dependency };

                       await projectContext.SaveChangesAsync(ct);
                   }));

        services.AddSingleton<TaskFileService>();

        services.AddSingleton<ITaskRepository>(provider =>
        {
            var dbContext = provider.GetRequiredService<ProjectDbContext>();
            var projectId = dbContext.Projects.First().Id; // Line 125
            return new DbTaskRepository(dbContext, projectId);
        });

        services.AddSingleton<ICommandFactory>(provider =>
        {
            var dbContext = provider.GetRequiredService<ProjectDbContext>();
            var projectId = dbContext.Projects.First().Id;
            var taskFileService = provider.GetRequiredService<TaskFileService>();
            return new CommandFactory(dbContext, projectId, taskFileService);
        });

        services.AddSingleton<CommandManager>(provider => new CommandManager(
            provider.GetRequiredService<ICommandFactory>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<IMessageBus>(),
            provider.GetRequiredService<ITaskRepository>(),
            provider.GetRequiredService<TaskDataSeeder>()
        ));

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<GanttChartViewModel>();
        services.AddTransient<TaskDialogViewModel>();
        services.AddTransient<MenuBarViewModel>();

        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<TaskDataSeeder>(provider => new TaskDataSeeder(
            provider.GetRequiredService<ProjectDbContext>(),
            provider.GetRequiredService<ITaskRepository>()
        ));

        return services;
    }
}