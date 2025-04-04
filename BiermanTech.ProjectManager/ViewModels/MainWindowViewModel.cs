using ReactiveUI;
using System;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly CommandManager _commandManager;

    public MainWindowViewModel(CommandManager commandManager)
    {
        _commandManager = commandManager;
    }

    // Expose CommandManager properties for binding
    public List<TaskItem> Tasks
    {
        get => _commandManager.Tasks;
        set => _commandManager.Tasks = value;
    }

    public TaskItem SelectedTask
    {
        get => _commandManager.SelectedTask;
        set => _commandManager.SelectedTask = value;
    }

    public Project Project
    {
        get => _commandManager.Project;
        set => _commandManager.Project = value;
    }

    public string NotificationMessage
    {
        get => _commandManager.NotificationMessage;
        set => _commandManager.NotificationMessage = value;
    }

    public string ProjectName => _commandManager.ProjectName;
    public string ProjectAuthor => _commandManager.ProjectAuthor;
    public ProjectNarrative Narrative => _commandManager.Narrative;
    public string ProjectSituation => _commandManager.ProjectSituation;
    public string ProjectCurrentState => _commandManager.ProjectCurrentState;
    public string ProjectPlan => _commandManager.ProjectPlan;
    public string ProjectResults => _commandManager.ProjectResults;

    // Expose commands
    public ReactiveCommand<Unit, Unit> CreateTaskCommand => _commandManager.CreateTaskCommand;
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand => _commandManager.UpdateTaskCommand;
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand => _commandManager.DeleteTaskCommand;
    public ReactiveCommand<Unit, Unit> UndoCommand => _commandManager.UndoCommand;
    public ReactiveCommand<Unit, Unit> RedoCommand => _commandManager.RedoCommand;
    public ReactiveCommand<Unit, Unit> SaveProjectCommand => _commandManager.SaveProjectCommand;
    public ReactiveCommand<Unit, Unit> SaveAsProjectCommand => _commandManager.SaveAsProjectCommand;
    public ReactiveCommand<Unit, Unit> LoadProjectCommand => _commandManager.LoadProjectCommand;
    public ReactiveCommand<Unit, Unit> NewProjectCommand => _commandManager.NewProjectCommand;
    public ReactiveCommand<Unit, Unit> EditNarrativeCommand => _commandManager.EditNarrativeCommand;
    public ReactiveCommand<Unit, Unit> SaveAsPdfCommand => _commandManager.SaveAsPdfCommand;

    public void SetMainWindow(Window mainWindow)
    {
        _commandManager.SetMainWindow(mainWindow);
    }

    public async Task Initialize()
    {
        await _commandManager.Initialize();
    }

    public void Dispose()
    {
        // No additional disposables to manage
    }
}