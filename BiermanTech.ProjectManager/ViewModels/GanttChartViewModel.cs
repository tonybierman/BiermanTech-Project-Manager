using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Serilog;
using Avalonia.Threading;
using Avalonia.ReactiveUI;

namespace BiermanTech.ProjectManager.Controls;

public class GanttChartViewModel : ReactiveObject
{
    private readonly ITaskRepository _taskRepository;
    private List<TaskItem> _tasks;
    private TaskItem _selectedTask;
    private IDisposable _tasksSubscription;

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
            Dispatcher.UIThread.Post(() =>
            {
                this.RaiseAndSetIfChanged(ref _selectedTask, value);
                Log.Information("GanttChartViewModel SelectedTask set to: {TaskName}", _selectedTask?.Name ?? "null");
            });
        }
    }

    public GanttChartViewModel(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _tasks = new List<TaskItem>();

        _tasksSubscription = Observable.FromEventPattern<EventHandler, EventArgs>(
            h => _taskRepository.TasksChanged += h,
            h => _taskRepository.TasksChanged -= h)
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ =>
            {
                Log.Information("TasksChanged event received, updating tasks");
                var updatedTasks = _taskRepository.GetTasks();
                if (updatedTasks != null)
                {
                    Tasks = new List<TaskItem>(updatedTasks); // Keep hierarchy intact
                    if (_selectedTask != null)
                    {
                        var newSelectedTask = FindTaskById(Tasks, _selectedTask.Id);
                        SelectedTask = newSelectedTask; // Could be null if deleted
                    }
                }
                else
                {
                    Tasks = new List<TaskItem>();
                    SelectedTask = null;
                }
            });

        this.WhenAnyValue(x => x.Tasks)
            .Where(tasks => tasks != null)
            .Subscribe(tasks =>
            {
                Log.Information("Tasks property changed, count: {Count}", tasks?.Count ?? 0);
            });

        this.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                Log.Information("SelectedTask property changed, task: {TaskName}", selectedTask?.Name ?? "null");
            });
    }

    public void UpdateTasks(List<TaskItem> tasks)
    {
        if (tasks != null)
        {
            Tasks = new List<TaskItem>(tasks);
        }
        else
        {
            Tasks = new List<TaskItem>();
        }
    }

    public void Dispose()
    {
        _tasksSubscription?.Dispose();
    }

    private TaskItem FindTaskById(IEnumerable<TaskItem> tasks, int id) // Changed from Guid to int
    {
        foreach (var task in tasks)
        {
            if (task.Id == id) return task;
            var found = FindTaskById(task.Children, id);
            if (found != null) return found;
        }
        return null;
    }
}