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

            // Apply migrations and seed the database
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
                await dbContext.Database.MigrateAsync();

                // One-time seeding (to be disabled in final release)
                if (!await dbContext.Projects.AnyAsync())
                {
                    // Project 1
                    var project1 = new Project
                    {
                        Name = "Sample Project 1",
                        Author = "BiermanTech Team",
                        Narrative = new ProjectNarrative
                        {
                            Situation = "This is sample project 1 to demonstrate the project manager.",
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
                        Project = project1
                    };

                    var task2 = new TaskItem
                    {
                        Name = "Design System",
                        StartDate = DateTimeOffset.Now.AddDays(3),
                        Duration = TimeSpan.FromDays(5),
                        PercentComplete = 0,
                        Project = project1
                    };

                    var task3 = new TaskItem
                    {
                        Name = "Implement Features",
                        StartDate = DateTimeOffset.Now.AddDays(8),
                        Duration = TimeSpan.FromDays(10),
                        PercentComplete = 0,
                        Project = project1
                    };

                    project1.Tasks = new List<TaskItem> { task1, task2, task3 };

                    // Project 2
                    var project2 = new Project
                    {
                        Name = "Sample Project 2",
                        Author = "BiermanTech Team",
                        Narrative = new ProjectNarrative
                        {
                            Situation = "This is sample project 2 to demonstrate the project manager.",
                            CurrentState = "Development phase.",
                            Plan = "Implement features and test.",
                            Results = "In progress."
                        }
                    };

                    var task4 = new TaskItem
                    {
                        Name = "Setup Environment",
                        StartDate = DateTimeOffset.Now,
                        Duration = TimeSpan.FromDays(2),
                        PercentComplete = 80,
                        Project = project2
                    };

                    var task5 = new TaskItem
                    {
                        Name = "Develop API",
                        StartDate = DateTimeOffset.Now.AddDays(2),
                        Duration = TimeSpan.FromDays(7),
                        PercentComplete = 20,
                        Project = project2
                    };

                    project2.Tasks = new List<TaskItem> { task4, task5 };

                    dbContext.Projects.AddRange(project1, project2);
                    await dbContext.SaveChangesAsync();

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

                    await dbContext.SaveChangesAsync();
                }

                Log.Information("Database migrations applied and seeding completed.");
            }

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