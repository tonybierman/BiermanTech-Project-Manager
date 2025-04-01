using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using BiermanTech.ProjectManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BiermanTech.ProjectManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            App.ServiceProvider.GetService<ITaskRepository>(),
            App.ServiceProvider.GetService<ICommandManager>(),
            App.ServiceProvider.GetService<ICommandFactory>(),
            App.ServiceProvider.GetService<IDialogService>(),
            App.ServiceProvider.GetService<IMessageBus>(),
            App.ServiceProvider.GetService<TaskDataSeeder>(),
            this // Pass the MainWindow instance
        );
        (DataContext as MainWindowViewModel)?.Initialize();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}