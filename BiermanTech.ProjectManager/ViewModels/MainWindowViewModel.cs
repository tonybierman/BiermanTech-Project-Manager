using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using Serilog;
using Avalonia.Controls; // Add this for Window

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICommandManager _commandManager;
    private readonly ICommandFactory _commandFactory;
    private readonly IDialogService _dialogService;
    private readonly BiermanTech.ProjectManager.Services.IMessageBus _messageBus;
    private readonly TaskDataSeeder _taskDataSeeder;
    private readonly Window _mainWindow; // Add this field
    private TaskItem _selectedTask;

    public List<TaskItem> Tasks => _taskRepository.GetTasks();

    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
    }

    public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; }

    public MainWindowViewModel(
        ITaskRepository taskRepository,
        ICommandManager commandManager,
        ICommandFactory commandFactory,
        IDialogService dialogService,
        BiermanTech.ProjectManager.Services.IMessageBus messageBus,
        TaskDataSeeder taskDataSeeder,
        Window mainWindow) // Add this parameter
    {
        _taskRepository = taskRepository;
        _commandManager = commandManager;
        _commandFactory = commandFactory;
        _dialogService = dialogService;
        _messageBus = messageBus;
        _taskDataSeeder = taskDataSeeder;
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        CreateTaskCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var result = await _dialogService.ShowTaskDialog(null, Tasks, _mainWindow);
                if (result != null)
                {
                    var command = _commandFactory.CreateAddTaskCommand(result);
                    _commandManager.ExecuteCommand(command);
                    Log.Information("Publishing TaskAdded for task: {TaskName}", result.Name);
                    _messageBus.Publish(new TaskAdded(result));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateTaskCommand");
                throw;
            }
        });

        UpdateTaskCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (SelectedTask == null) return;

                var originalTask = new TaskItem
                {
                    Id = SelectedTask.Id,
                    Name = SelectedTask.Name,
                    StartDate = SelectedTask.StartDate,
                    Duration = SelectedTask.Duration,
                    PercentComplete = SelectedTask.PercentComplete,
                    DependsOn = SelectedTask.DependsOn
                };

                var result = await _dialogService.ShowTaskDialog(SelectedTask, Tasks, _mainWindow);
                if (result != null)
                {
                    var command = _commandFactory.CreateUpdateTaskCommand(originalTask, result);
                    _commandManager.ExecuteCommand(command);
                    Log.Information("Publishing TaskUpdated for task: {TaskName}", result.Name);
                    _messageBus.Publish(new TaskUpdated(result));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateTaskCommand");
                throw;
            }
        }, this.WhenAnyValue(x => x.SelectedTask)
            .Select(x => x != null)
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in UpdateTaskCommand CanExecute");
                return Observable.Return(false);
            }));

        DeleteTaskCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                if (SelectedTask == null) return;

                var taskToDelete = SelectedTask;
                int index = Tasks.IndexOf(taskToDelete);
                var dependentTasks = Tasks.Where(t => t.DependsOn == taskToDelete).ToList();
                var command = _commandFactory.CreateDeleteTaskCommand(taskToDelete, index, dependentTasks);
                _commandManager.ExecuteCommand(command);
                Log.Information("Publishing TaskDeleted for task: {TaskName}", taskToDelete.Name);
                _messageBus.Publish(new TaskDeleted(taskToDelete));
                SelectedTask = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in DeleteTaskCommand");
                throw;
            }
        }, this.WhenAnyValue(x => x.SelectedTask)
            .Select(x => x != null)
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in DeleteTaskCommand CanExecute");
                return Observable.Return(false);
            }));

        var canUndoObservable = this.WhenAnyValue(x => x._commandManager.CanUndo)
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in CanUndo observable");
                return Observable.Return(false);
            });

        var canRedoObservable = this.WhenAnyValue(x => x._commandManager.CanRedo)
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in CanRedo observable");
                return Observable.Return(false);
            });

        UndoCommand = ReactiveCommand.Create(() =>
        {
            _commandManager.Undo();
        }, canUndoObservable);

        RedoCommand = ReactiveCommand.Create(() =>
        {
            _commandManager.Redo();
        }, canRedoObservable);
    }

    public void Initialize()
    {
        _taskDataSeeder.SeedSampleData();
    }
}