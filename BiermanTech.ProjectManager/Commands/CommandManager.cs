using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using ReactiveUI;
using Serilog;
using System.Reactive;
using System.Reactive.Linq;
using System.Timers;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class CommandManager : INotifyPropertyChanged, ICommandManager
{
    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
    private readonly ICommandFactory _commandFactory;
    private readonly IDialogService _dialogService;
    private readonly Services.IMessageBus _messageBus;
    private readonly ITaskRepository _taskRepository;
    private Window _mainWindow;
    private List<TaskItem> _tasks;
    private TaskItem _selectedTask;
    private Project _project;
    private string _notificationMessage;
    private readonly Timer _notificationTimer;
    private string _currentFilePath;

    public event PropertyChangedEventHandler PropertyChanged;

    private bool _canUndo;
    private bool _canRedo;
    private bool _canEditNarrative;

    public bool CanUndo
    {
        get => _canUndo;
        private set
        {
            if (_canUndo != value)
            {
                _canUndo = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanRedo
    {
        get => _canRedo;
        private set
        {
            if (_canRedo != value)
            {
                _canRedo = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanEditNarrative
    {
        get => _canEditNarrative;
        private set
        {
            if (_canEditNarrative != value)
            {
                _canEditNarrative = value;
                OnPropertyChanged();
            }
        }
    }

    public List<TaskItem> Tasks
    {
        get => _tasks;
        set
        {
            _tasks = value;
            OnPropertyChanged();
        }
    }

    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set
        {
            _selectedTask = value;
            OnPropertyChanged();
            Log.Information("CommandManager SelectedTask changed to: {TaskName}", _selectedTask?.Name ?? "null");
        }
    }

    public Project Project
    {
        get => _project;
        set
        {
            _project = value;
            CanEditNarrative = _project != null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProjectName));
            OnPropertyChanged(nameof(ProjectAuthor));
            OnPropertyChanged(nameof(Narrative));
            OnPropertyChanged(nameof(ProjectSituation));
            OnPropertyChanged(nameof(ProjectCurrentState));
            OnPropertyChanged(nameof(ProjectPlan));
            OnPropertyChanged(nameof(ProjectResults));
        }
    }

    public string NotificationMessage
    {
        get => _notificationMessage;
        set
        {
            _notificationMessage = value;
            OnPropertyChanged();
        }
    }

    public string ProjectName => Project?.Name;
    public string ProjectAuthor => Project?.Author;
    public ProjectNarrative Narrative => Project?.Narrative;
    public string ProjectSituation => Narrative?.Situation ?? string.Empty;
    public string ProjectCurrentState => Narrative?.CurrentState ?? string.Empty;
    public string ProjectPlan => Narrative?.Plan ?? string.Empty;
    public string ProjectResults => Narrative?.Results ?? string.Empty;

    public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> SaveProjectCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> SaveAsProjectCommand { get; private set; }
    public ReactiveCommand<int, Unit> LoadProjectCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> LoadProjectFromFileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EditNarrativeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> SaveAsPdfCommand { get; private set; }
    public Window MainWindow { get => _mainWindow; set => _mainWindow = value; }

    public CommandManager(
        ICommandFactory commandFactory,
        IDialogService dialogService,
        Services.IMessageBus messageBus,
        ITaskRepository taskRepository)
    {
        _commandFactory = commandFactory;
        _dialogService = dialogService;
        _messageBus = messageBus;
        _taskRepository = taskRepository;

        _tasks = new List<TaskItem>();
        _notificationTimer = new Timer(5000);
        _notificationTimer.Elapsed += (s, e) =>
        {
            NotificationMessage = string.Empty;
            _notificationTimer.Stop();
        };
        _notificationTimer.AutoReset = false;

        // Subscribe to task changes
        Observable.FromEventPattern<EventHandler, EventArgs>(
            h => _taskRepository.TasksChanged += h,
            h => _taskRepository.TasksChanged -= h)
            .Subscribe(_ =>
            {
                Log.Information("TasksChanged event received in CommandManager");
                var newTasks = _taskRepository.GetTasks();
                if (newTasks != null)
                {
                    Tasks = new List<TaskItem>(newTasks);
                    if (_selectedTask != null)
                    {
                        var newSelectedTask = FindTaskById(Tasks, _selectedTask.Id);
                        SelectedTask = newSelectedTask;
                    }
                }
                else
                {
                    Tasks = new List<TaskItem>();
                    SelectedTask = null;
                }
            });

        // Initialize commands
        CreateTaskCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (MainWindow == null) throw new InvalidOperationException("MainWindow not set.");
                var flatTasks = FlattenTasks(Tasks).ToList();
                var result = await _dialogService.ShowTaskDialog(null, flatTasks, MainWindow);
                if (result != null)
                {
                    result.Project = _project;
                    result.ProjectId = _project?.Id ?? 0;
                    var command = _commandFactory.CreateAddTaskCommand(result);
                    ExecuteCommand(command);
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
                if (SelectedTask == null || SelectedTask.IsParent) return;
                if (MainWindow == null) throw new InvalidOperationException("MainWindow not set.");

                var originalTask = DeepCopyTask(SelectedTask);
                var flatTasks = FlattenTasks(Tasks).ToList();
                var result = await _dialogService.ShowTaskDialog(SelectedTask, flatTasks, MainWindow);
                if (result != null)
                {
                    var command = _commandFactory.CreateUpdateTaskCommand(originalTask, result);
                    ExecuteCommand(command);
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
            .Do(canExecute => Log.Information("UpdateTaskCommand CanExecute: {CanExecute}", canExecute)));

        DeleteTaskCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                if (SelectedTask == null) return;

                var taskToDelete = SelectedTask;
                var command = _commandFactory.CreateDeleteTaskCommand(taskToDelete);
                ExecuteCommand(command);
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
            .Do(canExecute => Log.Information("DeleteTaskCommand CanExecute: {CanExecute}", canExecute)));

        UndoCommand = ReactiveCommand.Create(() =>
        {
            Undo();
            ShowNotification("Undo performed.");
        }, this.WhenAnyValue(x => x.CanUndo)
            .Do(canUndo => Log.Information("CanUndo changed: {CanUndo}", canUndo)));

        RedoCommand = ReactiveCommand.Create(() =>
        {
            Redo();
            ShowNotification("Redo performed.");
        }, this.WhenAnyValue(x => x.CanRedo)
            .Do(canRedo => Log.Information("CanRedo changed: {CanRedo}", canRedo)));

        SaveProjectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    await SaveAsProjectCommand.Execute();
                    return;
                }

                var command = _commandFactory.CreateExportProjectCommand(Project, _currentFilePath);
                ExecuteCommand(command);
                ShowNotification($"Project saved to {_currentFilePath}.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SaveProjectCommand");
                ShowNotification("Error saving project.");
                throw;
            }
        });

        SaveAsProjectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (MainWindow == null) throw new InvalidOperationException("MainWindow not set.");
                var storageProvider = MainWindow.StorageProvider;
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Project As",
                    SuggestedFileName = "project.json",
                    FileTypeChoices = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
                });

                if (file != null)
                {
                    _currentFilePath = file.Path.LocalPath;
                    var command = _commandFactory.CreateExportProjectCommand(Project, _currentFilePath);
                    ExecuteCommand(command);
                    ShowNotification($"Project saved to {_currentFilePath}.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SaveAsProjectCommand");
                ShowNotification("Error saving project.");
                throw;
            }
        });

        LoadProjectFromFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (MainWindow == null) throw new InvalidOperationException("MainWindow not set.");
                var storageProvider = MainWindow.StorageProvider;
                var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Load Project",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
                });

                if (files != null && files.Count > 0)
                {
                    _currentFilePath = files[0].Path.LocalPath;
                    var command = _commandFactory.CreateImportProjectCommand(Project, _currentFilePath);
                    ExecuteCommand(command);
                    ShowNotification($"Project loaded from {_currentFilePath}.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in LoadProjectFromFileCommand");
                ShowNotification("Error loading project.");
                throw;
            }
        });

        LoadProjectCommand = ReactiveCommand.Create<int>(projectId =>
        {
            try
            {
                var command = _commandFactory.CreateLoadProjectCommand(projectId);
                ExecuteCommand(command);
                Project = (command as LoadProjectCommand)?.LoadedProject;
                Tasks = new List<TaskItem>(_taskRepository.GetTasks());
                ShowNotification($"Project with ID {projectId} loaded.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading project with ID {projectId}");
                ShowNotification($"Error loading project: {ex.Message}");
                throw;
            }
        });

        NewProjectCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                var command = _commandFactory.CreateNewProjectCommand(Project);
                ExecuteCommand(command);
                _currentFilePath = null;
                ShowNotification("New project created.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in NewProjectCommand");
                ShowNotification("Error creating new project.");
                throw;
            }
        });

        EditNarrativeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (Project == null)
                {
                    Log.Warning("Cannot edit narrative: No project loaded.");
                    ShowNotification("No project loaded.");
                    return;
                }
                if (MainWindow == null) throw new InvalidOperationException("MainWindow not set.");

                var originalNarrative = Project.Narrative ?? new ProjectNarrative();
                var result = await _dialogService.ShowNarrativeDialog(originalNarrative, MainWindow);
                if (result != null)
                {
                    var command = _commandFactory.CreateEditNarrativeCommand(Project, originalNarrative, result);
                    ExecuteCommand(command);
                    Log.Information("Project narrative updated.");
                    await SaveProjectCommand.Execute();
                    ShowNotification("Project narrative updated and saved.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in EditNarrativeCommand");
                ShowNotification("Error updating project narrative.");
                throw;
            }
        }, this.WhenAnyValue(x => x.CanEditNarrative)
            .Do(canExecute => Log.Information("EditNarrativeCommand CanExecute: {CanExecute}", canExecute)));

        SaveAsPdfCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (MainWindow == null) throw new InvalidOperationException("MainWindow not set.");
                var storageProvider = MainWindow.StorageProvider;
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Window as PDF",
                    SuggestedFileName = $"{ProjectName}.pdf",
                    FileTypeChoices = new[] { new FilePickerFileType("PDF Files") { Patterns = new[] { "*.pdf" } } }
                });

                if (file != null)
                {
                    AvaloniaUI.PrintToPDF.Print.ToFile(file.Path.LocalPath, MainWindow);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SaveAsPdfCommand");
                ShowNotification("Error saving window as PDF.");
                throw;
            }
        });
    }

    public void SetMainWindow(Window mainWindow)
    {
        MainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    public async Task Initialize()
    {
        try
        {
            // Load the first project using LoadProjectCommand
            var command = _commandFactory.CreateLoadProjectCommand(1);
            // Skip ClearTasks if the task list is already empty (initial load)
            if (_taskRepository.GetTasks().Any())
            {
                _taskRepository.ClearTasks();
            }
            ExecuteCommand(command);
            Project = (command as LoadProjectCommand)?.LoadedProject;
            Tasks = new List<TaskItem>(_taskRepository.GetTasks());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in Initialize");
            ShowNotification("Error initializing Command Manager.");
            throw;
        }
    }

    //public async Task Initialize()
    //{
    //    try
    //    {
    //        // Load the first project using LoadProjectCommand
    //        var command = _commandFactory.CreateLoadProjectCommand(1);
    //        ExecuteCommand(command);
    //        Project = (command as LoadProjectCommand)?.LoadedProject;
    //        Tasks = new List<TaskItem>(_taskRepository.GetTasks());
    //    }
    //    catch (Exception ex)
    //    {
    //        Log.Error(ex, "Error in Initialize");
    //        ShowNotification("Error initializing Command Manager.");
    //        throw;
    //    }
    //}

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    public void Undo()
    {
        if (_undoStack.Count == 0) return;
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    public void Redo()
    {
        if (_redoStack.Count == 0) return;
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    public void ShowNotification(string message)
    {
        NotificationMessage = message;
        _notificationTimer.Stop();
        _notificationTimer.Start();
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private TaskItem FindTaskById(IEnumerable<TaskItem> tasks, int id)
    {
        foreach (var task in tasks)
        {
            if (task.Id == id) return task;
            var found = FindTaskById(task.Children, id);
            if (found != null) return found;
        }
        return null;
    }

    private IEnumerable<TaskItem> FlattenTasks(IEnumerable<TaskItem> tasks)
    {
        if (tasks == null) yield break;
        foreach (var task in tasks)
        {
            yield return task;
            foreach (var child in FlattenTasks(task.Children))
            {
                yield return child;
            }
        }
    }

    private TaskItem DeepCopyTask(TaskItem task)
    {
        return new TaskItem
        {
            Id = task.Id,
            Name = task.Name,
            StartDate = task.StartDate,
            Duration = task.Duration,
            PercentComplete = task.PercentComplete,
            TaskDependencies = task.TaskDependencies?.Select(td => new TaskDependency
            {
                TaskId = td.TaskId,
                DependsOnId = td.DependsOnId
            }).ToList() ?? new List<TaskDependency>(),
            DependsOn = new List<TaskItem>(task.DependsOn),
            Children = task.Children.Select(DeepCopyTask).ToList()
        };
    }
}