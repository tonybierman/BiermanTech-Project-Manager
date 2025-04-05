using Avalonia.Controls;
using BiermanTech.ProjectManager.ViewModels;
using BiermanTech.ProjectManager.Controls;
using Serilog;
using System;
using System.Threading.Tasks;
using BiermanTech.ProjectManager.Commands;

namespace BiermanTech.ProjectManager.Views
{
    public partial class MainWindow : Window
    {
        private readonly CommandManager _commandManager;
        private readonly GanttChartViewModel _ganttViewModel;
        private readonly MenuBarViewModel _menuBarViewModel;

        public MainWindow(
            MainWindowViewModel viewModel,
            CommandManager commandManager,
            GanttChartViewModel ganttViewModel,
            MenuBarViewModel menuBarViewModel)
        {
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            _ganttViewModel = ganttViewModel ?? throw new ArgumentNullException(nameof(ganttViewModel));
            _menuBarViewModel = menuBarViewModel ?? throw new ArgumentNullException(nameof(menuBarViewModel));

            InitializeComponent();
            SetupWindow(viewModel);
        }

        private void SetupWindow(MainWindowViewModel viewModel)
        {
            Opened += async (_, _) => await InitializeAsync(viewModel);
        }

        private async Task InitializeAsync(MainWindowViewModel viewModel)
        {
            try
            {
                await viewModel.Initialize();
                ConfigureViewModels(viewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize MainWindowViewModel");
                Close();
            }
        }

        private void ConfigureViewModels(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            _commandManager.SetMainWindow(this);
            _menuBarViewModel.SetMainWindow(this);

            LogProjectStatus(viewModel);
            SetupMenuBar();
            SetupGanttChart();
        }

        private void LogProjectStatus(MainWindowViewModel viewModel)
        {
            if (viewModel.Project == null)
            {
                Log.Error("MainWindowViewModel.Project is null after initialization");
            }
            else
            {
                Log.Information("MainWindowViewModel.Project set: {ProjectName}", viewModel.Project.Name);
            }
        }

        private void SetupMenuBar()
        {
            if (this.FindControl<MenuBarControl>("MenuBarControl") is { } menuBarControl)
            {
                menuBarControl.DataContext = _menuBarViewModel;
            }
            else
            {
                Log.Error("Failed to find MenuBarControl in MainWindow");
            }
        }

        private void SetupGanttChart()
        {
            var ganttControl = new GanttChartControl(_ganttViewModel);
            if (this.FindControl<ContentControl>("GanttChartPlaceholder") is { } placeholder)
            {
                placeholder.Content = ganttControl;
            }
        }
    }
}