using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BiermanTech.ProjectManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = App.ServiceProvider.GetService<MainWindowViewModel>();
        DataContext = viewModel;
        viewModel.Initialize();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}