using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.ViewModels;

namespace BiermanTech.ProjectManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}