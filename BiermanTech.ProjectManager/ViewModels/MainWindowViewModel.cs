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
using Avalonia.Controls;
using System.Timers;
using Avalonia.Platform.Storage;

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICommandManager _commandManager;
    private readonly ICommandFactory _commandFactory;
    private readonly IDialogService _dialogService;
    private readonly BiermanTech.ProjectManager.Services.IMessageBus _messageBus;
    private readonly TaskDataSeeder _taskDataSeeder;
    private readonly Window _mainWindow;
    private List<TaskItem> _tasks;
    private TaskItem _selectedTask;
    private IDisposable _taskChangedSubscription;
    private Project _project;
    private string _notificationMessage;
    private readonly Timer _notificationTimer;

    public List<TaskItem> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTask, value);
            Log.Information("MainWindowViewModel SelectedTask changed to: {TaskName}", _selectedTask?.Name ?? "null");
        }
    }

    public string ProjectName => _project?.Name;
    public string ProjectAuthor => _project?.Author;

    public string NotificationMessage
    {
        get => _notificationMessage;
        set => this.RaiseAndSetIfChanged(ref _notificationMessage, value);
    }

    public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }

    public MainWindowViewModel(
        ITaskRepository taskRepository,
        ICommandManager commandManager,
        ICommandFactory commandFactory,
        IDialogService dialogService,
        BiermanTech.ProjectManager.Services.IMessageBus messageBus,
        TaskDataSeeder taskDataSeeder,
        Window mainWindow)
    {
        _taskRepository = taskRepository;
        _commandManager = commandManager;
        _commandFactory = commandFactory;
        _dialogService = dialogService;
        _messageBus = messageBus;
        _taskDataSeeder = taskDataSeeder;
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        // Initialize Tasks
        _tasks = new List<TaskItem>();

        // Set up notification timer (clears message after 5 seconds)
        _notificationTimer = new Timer(5000);
        _notificationTimer.Elapsed += (s, e) =>
        {
            NotificationMessage = string.Empty;
            _notificationTimer.Stop();
        };
        _notificationTimer.AutoReset = false;

        // Subscribe to task changes from the repository
        _taskChangedSubscription = Observable.FromEventPattern<EventHandler, EventArgs>(
            h => _taskRepository.TasksChanged += h,
            h => _taskRepository.TasksChanged -= h)
            .Subscribe(_ =>
            {
                Log.Information("TasksChanged event received in MainWindowViewModel");
                var newTasks = _taskRepository.GetTasks();
                if (newTasks != null)
                {
                    // Update Tasks
                    Tasks = new List<TaskItem>(newTasks);

                    // Update SelectedTask to point to the corresponding task in the new list
                    if (_selectedTask != null)
                    {
                        var newSelectedTask = Tasks.FirstOrDefault(t => t.Id == _selectedTask.Id);
                        if (newSelectedTask != null)
                        {
                            SelectedTask = newSelectedTask;
                        }
                        else
                        {
                            SelectedTask = null; // Task no longer exists (e.g., deleted)
                        }
                    }
                }
                else
                {
                    Tasks = new List<TaskItem>();
                    SelectedTask = null;
                }
            });

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
                    ShowNotification($"Task '{result.Name}' added.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateTaskCommand");
                ShowNotification("Error adding task.");
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
                    ShowNotification($"Task '{result.Name}' updated.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateTaskCommand");
                ShowNotification("Error updating task.");
                throw;
            }
        }, this.WhenAnyValue(x => x.SelectedTask)
            .Select(x => x != null)
            .Do(canExecute => Log.Information("UpdateTaskCommand CanExecute: {CanExecute}", canExecute))
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
                ShowNotification($"Task '{taskToDelete.Name}' deleted.");
                SelectedTask = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in DeleteTaskCommand");
                ShowNotification("Error deleting task.");
                throw;
            }
        }, this.WhenAnyValue(x => x.SelectedTask)
            .Select(x => x != null)
            .Do(canExecute => Log.Information("DeleteTaskCommand CanExecute: {CanExecute}", canExecute))
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in DeleteTaskCommand CanExecute");
                return Observable.Return(false);
            }));

        var canUndoObservable = this.WhenAnyValue(x => x._commandManager.CanUndo)
            .Do(canUndo => Log.Information("CanUndo changed: {CanUndo}", canUndo))
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in CanUndo observable");
                return Observable.Return(false);
            });

        var canRedoObservable = this.WhenAnyValue(x => x._commandManager.CanRedo)
            .Do(canRedo => Log.Information("CanRedo changed: {CanRedo}", canRedo))
            .Catch<bool, Exception>(ex =>
            {
                Log.Error(ex, "Error in CanRedo observable");
                return Observable.Return(false);
            });

        UndoCommand = ReactiveCommand.Create(() =>
        {
            _commandManager.Undo();
            ShowNotification("Undo performed.");
        }, canUndoObservable);

        RedoCommand = ReactiveCommand.Create(() =>
        {
            _commandManager.Redo();
            ShowNotification("Redo performed.");
        }, canRedoObservable);

        SaveProjectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var storageProvider = _mainWindow.StorageProvider;
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Project",
                    SuggestedFileName = "project.json",
                    FileTypeChoices = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
                });

                if (file != null)
                {
                    var filePath = file.Path.LocalPath;
                    var command = _commandFactory.CreateSaveProjectCommand(_project, filePath);
                    _commandManager.ExecuteCommand(command);
                    ShowNotification($"Project saved to {filePath}.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SaveProjectCommand");
                ShowNotification("Error saving project.");
                throw;
            }
        });

        LoadProjectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var storageProvider = _mainWindow.StorageProvider;
                var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Load Project",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
                });

                if (files != null && files.Count > 0)
                {
                    var filePath = files[0].Path.LocalPath;
                    var command = _commandFactory.CreateLoadProjectCommand(_project, filePath);
                    _commandManager.ExecuteCommand(command);
                    ShowNotification($"Project loaded from {filePath}.");
                    // Notify UI of project metadata changes
                    this.RaisePropertyChanged(nameof(ProjectName));
                    this.RaisePropertyChanged(nameof(ProjectAuthor));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in LoadProjectCommand");
                ShowNotification("Error loading project.");
                throw;
            }
        });

        NewProjectCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                var command = _commandFactory.CreateNewProjectCommand(_project);
                _commandManager.ExecuteCommand(command);
                ShowNotification("New project created.");
                // Notify UI of project metadata changes
                this.RaisePropertyChanged(nameof(ProjectName));
                this.RaisePropertyChanged(nameof(ProjectAuthor));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in NewProjectCommand");
                ShowNotification("Error creating new project.");
                throw;
            }
        });
    }

    public async Task Initialize()
    {
        _project = await _taskDataSeeder.SeedSampleDataAsync();
        Tasks = new List<TaskItem>(_taskRepository.GetTasks());
        this.RaisePropertyChanged(nameof(ProjectName));
        this.RaisePropertyChanged(nameof(ProjectAuthor));
    }

    public void Dispose()
    {
        _taskChangedSubscription?.Dispose();
        _notificationTimer?.Dispose();
    }

    private void ShowNotification(string message)
    {
        NotificationMessage = message;
        _notificationTimer.Stop();
        _notificationTimer.Start();
    }
}