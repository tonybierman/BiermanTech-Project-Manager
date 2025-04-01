using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace BiermanTech.ProjectManager.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
        services.AddSingleton<ICommandManager, CommandManager>();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<GanttChartRenderer>();
        services.AddSingleton<TaskDataSeeder>();
        return services;
    }
}