using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using BiermanTech.ProjectManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BiermanTech.ProjectManager.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel(
            App.ServiceProvider.GetService<ITaskRepository>(),
            App.ServiceProvider.GetService<ICommandManager>(),
            App.ServiceProvider.GetService<ICommandFactory>(),
            App.ServiceProvider.GetService<IDialogService>(),
            App.ServiceProvider.GetService<IMessageBus>(),
            App.ServiceProvider.GetService<TaskDataSeeder>(),
            this
        );
        DataContext = _viewModel;
        _viewModel.Initialize();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}