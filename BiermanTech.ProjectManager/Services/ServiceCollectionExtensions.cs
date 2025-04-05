using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Controls;
using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.ViewModels;
using BiermanTech.ProjectManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace BiermanTech.ProjectManager.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.db");
        services.AddDbContext<ProjectDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Apply migrations immediately after configuring DbContext
        using (var serviceProvider = services.BuildServiceProvider())
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
                dbContext.Database.Migrate();
            }
        }

        services.AddSingleton<TaskFileService>();

        services.AddSingleton<ITaskRepository>(provider =>
        {
            var dbContext = provider.GetRequiredService<ProjectDbContext>();
            var projectId = dbContext.Projects.FirstOrDefault()?.Id
                ?? throw new InvalidOperationException("No project found in the database.");
            return new DbTaskRepository(dbContext, projectId);
        });

        services.AddSingleton<ICommandFactory>(provider =>
        {
            var dbContext = provider.GetRequiredService<ProjectDbContext>();
            var projectId = dbContext.Projects.FirstOrDefault()?.Id
                ?? throw new InvalidOperationException("No project found in the database.");
            var taskFileService = provider.GetRequiredService<TaskFileService>();
            var taskRepository = provider.GetRequiredService<ITaskRepository>();
            return new CommandFactory(dbContext, projectId, taskFileService, taskRepository);
        });

        services.AddSingleton<CommandManager>(provider => new CommandManager(
            provider.GetRequiredService<ICommandFactory>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<IMessageBus>(),
            provider.GetRequiredService<ITaskRepository>()
        ));

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<GanttChartViewModel>(provider => new GanttChartViewModel(
            provider.GetRequiredService<MainWindowViewModel>()
        ));
        services.AddTransient<MenuBarViewModel>();
        services.AddTransient<TaskDialogViewModel>();
        services.AddTransient<MainWindow>();

        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBus, MessageBus>();

        return services;
    }
}