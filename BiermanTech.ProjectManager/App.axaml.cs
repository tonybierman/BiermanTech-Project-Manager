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
            // Resolve all view models via DI
            var mainWindowViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            var ganttViewModel = ServiceProvider.GetRequiredService<GanttChartViewModel>();
            var menuBarViewModel = ServiceProvider.GetRequiredService<MenuBarViewModel>();

            // Create MainWindow
            var mainWindow = new MainWindow();

            // Set the MainWindow on view models (which delegate to CommandManager)
            mainWindowViewModel.SetMainWindow(mainWindow);
            menuBarViewModel.SetMainWindow(mainWindow);

            // Configure MainWindow with view models
            mainWindow.SetViewModels(mainWindowViewModel, ganttViewModel, menuBarViewModel);

            // Assign MainWindow to the application lifetime
            desktop.MainWindow = mainWindow;

            // Initialize the view model
            mainWindowViewModel.Initialize();
        }

        base.OnFrameworkInitializationCompleted();
    }
}