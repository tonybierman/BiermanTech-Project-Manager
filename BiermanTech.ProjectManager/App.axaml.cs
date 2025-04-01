using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Services;
using BiermanTech.ProjectManager.ViewModels;
using BiermanTech.ProjectManager.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BiermanTech.ProjectManager;

public class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();
        services.AddAppServices();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<TaskDialogViewModel>();
        ServiceProvider = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}