using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Serilog;
using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.ViewModels;

public class MenuBarViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly Window _window;
    private TaskItem _selectedTask;

    // Commands from MainWindowViewModel
    public ReactiveCommand<Unit, Unit> CreateTaskCommand => _mainViewModel.CreateTaskCommand;
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand => _mainViewModel.UpdateTaskCommand;
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand => _mainViewModel.DeleteTaskCommand;
    public ReactiveCommand<Unit, Unit> UndoCommand => _mainViewModel.UndoCommand;
    public ReactiveCommand<Unit, Unit> RedoCommand => _mainViewModel.RedoCommand;

    // MenuBar-specific commands
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    public ReactiveCommand<Unit, Unit> AboutCommand { get; }

    // Reactive property for SelectedTask
    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
    }

    public MenuBarViewModel(MainWindowViewModel mainViewModel, Window window)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _window = window ?? throw new ArgumentNullException(nameof(window));

        // Observe MainWindowViewModel.SelectedTask and update our SelectedTask property
        _mainViewModel.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                SelectedTask = selectedTask;
            });

        NewProjectCommand = ReactiveCommand.Create(() =>
        {
            Log.Information("New Project command executed");
            // TODO: Implement new project functionality
        });

        OpenProjectCommand = ReactiveCommand.Create(() =>
        {
            Log.Information("Open Project command executed");
            // TODO: Implement open project functionality
        });

        SaveProjectCommand = ReactiveCommand.Create(() =>
        {
            Log.Information("Save Project command executed");
            // TODO: Implement save project functionality
        });

        ExitCommand = ReactiveCommand.Create(() =>
        {
            Log.Information("Exit command executed");
            _window.Close();
        });

        AboutCommand = ReactiveCommand.Create(() =>
        {
            Log.Information("About command executed");
            // TODO: Implement about dialog
        });
    }
}