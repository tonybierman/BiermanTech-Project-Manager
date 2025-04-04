using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Views;
using BiermanTech.ProjectManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using BiermanTech.ProjectManager.Controls;
using System;

namespace BiermanTech.ProjectManager;

public class App : Application
{
    public IServiceProvider ServiceProvider { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            var ganttViewModel = ServiceProvider.GetRequiredService<GanttChartViewModel>();
            var menuBarViewModel = ServiceProvider.GetRequiredService<MenuBarViewModel>();
            desktop.MainWindow = new MainWindow(viewModel, ganttViewModel, menuBarViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }
}