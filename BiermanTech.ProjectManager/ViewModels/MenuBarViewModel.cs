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
    public ReactiveCommand<Unit, Unit> NewProjectCommand => _mainViewModel.NewProjectCommand;
    public ReactiveCommand<Unit, Unit> LoadProjectCommand => _mainViewModel.LoadProjectCommand;
    public ReactiveCommand<Unit, Unit> SaveProjectCommand => _mainViewModel.SaveProjectCommand;
    public ReactiveCommand<Unit, Unit> SaveAsProjectCommand => _mainViewModel.SaveAsProjectCommand;
    public ReactiveCommand<Unit, Unit> EditNarrativeCommand => _mainViewModel.EditNarrativeCommand;

    // MenuBar-specific commands
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    public ReactiveCommand<Unit, Unit> AboutCommand { get; }

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