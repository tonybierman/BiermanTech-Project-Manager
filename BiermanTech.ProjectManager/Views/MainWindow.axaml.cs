using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Controls;
using BiermanTech.ProjectManager.ViewModels;
using Serilog;
using System;

namespace BiermanTech.ProjectManager.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetViewModels(MainWindowViewModel viewModel, GanttChartViewModel ganttViewModel, MenuBarViewModel menuBarViewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        var menuBarControl = this.FindControl<MenuBarControl>("MenuBarControl");
        if (menuBarControl != null)
        {
            menuBarControl.DataContext = menuBarViewModel;
        }
        else
        {
            Log.Error("Failed to find MenuBarControl in MainWindow");
        }

        var ganttControl = new GanttChartControl(ganttViewModel);
        this.FindControl<ContentControl>("GanttChartPlaceholder").Content = ganttControl;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel?.Dispose();
    }
}