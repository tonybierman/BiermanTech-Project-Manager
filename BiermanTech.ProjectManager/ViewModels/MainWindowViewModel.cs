using ReactiveUI;
using System;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Serilog;

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly CommandManager _commandManager;
    private Project _project;
    private List<TaskItem> _tasks;
    private TaskItem _selectedTask;
    private string _notificationMessage;

    public MainWindowViewModel(CommandManager commandManager)
    {
        _commandManager = commandManager;

        // Initialize reactive properties
        _project = _commandManager.Project;
        _tasks = _commandManager.Tasks;
        _selectedTask = _commandManager.SelectedTask;
        _notificationMessage = _commandManager.NotificationMessage;

        // Subscribe to changes in CommandManager
        _commandManager.WhenAnyValue(x => x.Project)
            .Subscribe(project => Project = project);

        _commandManager.WhenAnyValue(x => x.Tasks)
            .Subscribe(tasks => Tasks = tasks);

        _commandManager.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask => SelectedTask = selectedTask);

        _commandManager.WhenAnyValue(x => x.NotificationMessage)
            .Subscribe(message => NotificationMessage = message);

        // Add this to propagate to CommandManager
        this.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                _commandManager.SelectedTask = selectedTask;
            });
    }

    public Project Project
    {
        get => _project;
        set => this.RaiseAndSetIfChanged(ref _project, value);
    }

    public List<TaskItem> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
    }

    public string NotificationMessage
    {
        get => _notificationMessage;
        set => this.RaiseAndSetIfChanged(ref _notificationMessage, value);
    }

    public string ProjectName => Project?.Name;
    public string ProjectAuthor => Project?.Author;
    public ProjectNarrative Narrative => Project?.Narrative;
    public string ProjectSituation => Narrative?.Situation ?? string.Empty;
    public string ProjectCurrentState => Narrative?.CurrentState ?? string.Empty;
    public string ProjectPlan => Narrative?.Plan ?? string.Empty;
    public string ProjectResults => Narrative?.Results ?? string.Empty;

    // Expose commands
    public ReactiveCommand<Unit, Unit> CreateTaskCommand => _commandManager.CreateTaskCommand;
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand => _commandManager.UpdateTaskCommand;
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand => _commandManager.DeleteTaskCommand;
    public ReactiveCommand<Unit, Unit> UndoCommand => _commandManager.UndoCommand;
    public ReactiveCommand<Unit, Unit> RedoCommand => _commandManager.RedoCommand;
    public ReactiveCommand<Unit, Unit> SaveProjectCommand => _commandManager.SaveProjectCommand;
    public ReactiveCommand<Unit, Unit> SaveAsProjectCommand => _commandManager.SaveAsProjectCommand;
    public ReactiveCommand<int, Unit> LoadProjectCommand => _commandManager.LoadProjectCommand;
    public ReactiveCommand<Unit, Unit> LoadProjectFromFileCommand => _commandManager.LoadProjectFromFileCommand;
    public ReactiveCommand<Unit, Unit> NewProjectCommand => _commandManager.NewProjectCommand;
    public ReactiveCommand<Unit, Unit> EditNarrativeCommand => _commandManager.EditNarrativeCommand;
    public ReactiveCommand<Unit, Unit> SaveAsPdfCommand => _commandManager.SaveAsPdfCommand;

    public async Task Initialize()
    {
        await _commandManager.Initialize();
    }

    public void Dispose()
    {
        // No additional disposables to manage
    }
}