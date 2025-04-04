using Avalonia.Controls;
using BiermanTech.ProjectManager.ViewModels;
using BiermanTech.ProjectManager.Controls;
using Serilog;
using System.Threading.Tasks;
using BiermanTech.ProjectManager.Commands;
using System;

namespace BiermanTech.ProjectManager.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly CommandManager _commandManager;
    private readonly GanttChartViewModel _ganttViewModel;
    private readonly MenuBarViewModel _menuBarViewModel;

    public MainWindow(MainWindowViewModel viewModel, CommandManager commandManager, GanttChartViewModel ganttViewModel, MenuBarViewModel menuBarViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _commandManager = commandManager;
        _ganttViewModel = ganttViewModel;
        _menuBarViewModel = menuBarViewModel;

        // Subscribe to the Opened event to perform async initialization
        this.Opened += async (sender, args) =>
        {
            await InitializeViewModelAsync();
        };
    }

    private async Task InitializeViewModelAsync()
    {
        try
        {
            // Initialize the view model (this sets CommandManager.Project)
            await _viewModel.Initialize();

            // Set the MainWindow on view models (which delegate to CommandManager)
            _viewModel.SetMainWindow(this);
            _menuBarViewModel.SetMainWindow(this);

            // Configure MainWindow with view models
            SetViewModels(_viewModel, _ganttViewModel, _menuBarViewModel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize MainWindowViewModel");
            // Show an error message to the user
            Close();
        }
    }

    public void SetViewModels(MainWindowViewModel viewModel, GanttChartViewModel ganttViewModel, MenuBarViewModel menuBarViewModel)
    {
        if (viewModel != _viewModel)
        {
            throw new ArgumentException("The provided viewModel does not match the existing _viewModel instance.", nameof(viewModel));
        }

        DataContext = _viewModel;

        if (_viewModel.Project == null)
        {
            Log.Error("MainWindowViewModel.Project is null after initialization");
        }
        else
        {
            Log.Information("MainWindowViewModel.Project set: {ProjectName}", _viewModel.Project.Name);
        }

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
}