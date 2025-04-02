using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BiermanTech.ProjectManager.Controls;

public partial class MenuBarControl : UserControl
{
    public MenuBarControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}