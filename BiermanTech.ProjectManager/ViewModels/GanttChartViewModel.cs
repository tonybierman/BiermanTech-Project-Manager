using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
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
            // Ensure the property change is raised on the UI thread
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

        // Subscribe to external changes from the repository
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
                    Tasks = new List<TaskItem>(updatedTasks);
                }
                else
                {
                    Tasks = new List<TaskItem>();
                }
            });

        // Subscribe to Tasks changes to log
        this.WhenAnyValue(x => x.Tasks)
            .Where(tasks => tasks != null)
            .Subscribe(tasks =>
            {
                Log.Information("Tasks property changed, count: {Count}", tasks?.Count ?? 0);
            });

        // Subscribe to SelectedTask changes to log
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
}