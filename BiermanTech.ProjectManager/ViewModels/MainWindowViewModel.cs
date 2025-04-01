using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using BiermanTech.ProjectManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICommandManager _commandManager;
    private readonly ICommandFactory _commandFactory;
    private readonly IDialogService _dialogService;
    private readonly BiermanTech.ProjectManager.Services.IMessageBus _messageBus;
    private readonly TaskDataSeeder _taskDataSeeder;
    private TaskItem _selectedTask;

    public ObservableCollection<TaskItem> Tasks => _taskRepository.GetTasks();

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
        TaskDataSeeder taskDataSeeder)
    {
        _taskRepository = taskRepository;
        _commandManager = commandManager;
        _commandFactory = commandFactory;
        _dialogService = dialogService;
        _messageBus = messageBus;
        _taskDataSeeder = taskDataSeeder;

        CreateTaskCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await _dialogService.ShowTaskDialog(null, Tasks, App.ServiceProvider.GetService<MainWindow>());
            if (result != null)
            {
                var command = _commandFactory.CreateAddTaskCommand(result);
                _commandManager.ExecuteCommand(command);
                _messageBus.Publish(new TaskAdded(result));
            }
        });

        UpdateTaskCommand = ReactiveCommand.CreateFromTask(async () =>
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

            var result = await _dialogService.ShowTaskDialog(SelectedTask, Tasks, App.ServiceProvider.GetService<MainWindow>());
            if (result != null)
            {
                var command = _commandFactory.CreateUpdateTaskCommand(originalTask, result);
                _commandManager.ExecuteCommand(command);
                _messageBus.Publish(new TaskUpdated(result));
            }
        }, this.WhenAnyValue(x => x.SelectedTask)
            .Select(x => x != null)
            .Catch<bool, Exception>(ex =>
            {
                Console.WriteLine($"Error in UpdateTaskCommand CanExecute: {ex.Message}");
                return Observable.Return(false);
            }));

        DeleteTaskCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedTask == null) return;

            var taskToDelete = SelectedTask;
            int index = Tasks.IndexOf(taskToDelete);
            var dependentTasks = new ObservableCollection<TaskItem>(Tasks.Where(t => t.DependsOn == taskToDelete));
            var command = _commandFactory.CreateDeleteTaskCommand(taskToDelete, index, dependentTasks);
            _commandManager.ExecuteCommand(command);
            _messageBus.Publish(new TaskDeleted(taskToDelete));
            SelectedTask = null;
        }, this.WhenAnyValue(x => x.SelectedTask)
            .Select(x => x != null)
            .Catch<bool, Exception>(ex =>
            {
                Console.WriteLine($"Error in DeleteTaskCommand CanExecute: {ex.Message}");
                return Observable.Return(false);
            }));

        var canUndoObservable = this.WhenAnyValue(x => x._commandManager.CanUndo)
            .Catch<bool, Exception>(ex =>
            {
                Console.WriteLine($"Error in CanUndo observable: {ex.Message}");
                return Observable.Return(false);
            });

        var canRedoObservable = this.WhenAnyValue(x => x._commandManager.CanRedo)
            .Catch<bool, Exception>(ex =>
            {
                Console.WriteLine($"Error in CanRedo observable: {ex.Message}");
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