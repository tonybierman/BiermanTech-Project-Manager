using BiermanTech.ProjectManager.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace BiermanTech.ProjectManager.ViewModels;

public class TaskDialogViewModel : ViewModelBase
{
    private string _taskName;
    private DateTimeOffset? _startDate;
    private double _durationDays;
    private double _percentComplete;
    private List<TaskItem> _dependsOnList; // Already List<TaskItem>
    private readonly List<TaskItem> _tasks;
    private readonly TaskItem _task;
    private string _errorMessage;

    public string TaskName
    {
        get => _taskName;
        set => this.RaiseAndSetIfChanged(ref _taskName, value);
    }

    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _startDate, value);
            Validate();
        }
    }

    public double DurationDays
    {
        get => _durationDays;
        set => this.RaiseAndSetIfChanged(ref _durationDays, value);
    }

    public double PercentComplete
    {
        get => _percentComplete;
        set => this.RaiseAndSetIfChanged(ref _percentComplete, value);
    }

    public List<TaskItem> DependsOnList
    {
        get => _dependsOnList;
        set
        {
            this.RaiseAndSetIfChanged(ref _dependsOnList, value);
            Validate();
        }
    }

    public List<TaskItem> AvailableTasks
    {
        get => _tasks.Where(t => t != _task && !t.IsParent).ToList(); // Exclude parents
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, TaskItem> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private TaskItem _result;
    private bool _canSave;

    public TaskDialogViewModel(TaskItem task, List<TaskItem> tasks)
    {
        _task = task;
        _tasks = tasks;

        if (task != null)
        {
            TaskName = task.Name;
            StartDate = task.StartDate;
            DurationDays = task.Duration.HasValue ? task.Duration.Value.TotalDays : 0;
            PercentComplete = task.PercentComplete;
            DependsOnList = task.DependsOn != null ? new List<TaskItem>(task.DependsOn) : new List<TaskItem>();
        }
        else
        {
            TaskName = string.Empty;
            StartDate = DateTimeOffset.Now;
            DurationDays = 1;
            PercentComplete = 0;
            DependsOnList = new List<TaskItem>();
        }

        Validate();

        SaveCommand = ReactiveCommand.Create(
            () =>
            {
                _result = new TaskItem
                {
                    Id = _task?.Id ?? 0, // Changed from Guid.NewGuid() to 0 for new tasks
                    Name = TaskName,
                    StartDate = StartDate.Value,
                    Duration = TimeSpan.FromDays(DurationDays),
                    PercentComplete = PercentComplete,
                    DependsOnIds = DependsOnList.Select(t => t.Id).ToList(), // Already int, no change needed
                    DependsOn = new List<TaskItem>(DependsOnList) // Set DependsOn for runtime
                };
                return _result;
            },
            this.WhenAnyValue(
                x => x.StartDate,
                x => x.DependsOnList,
                x => x.TaskName,
                (startDate, dependsOnList, taskName) =>
                    !string.IsNullOrWhiteSpace(taskName) && startDate.HasValue && IsValid()
            )
        );

        CancelCommand = ReactiveCommand.Create(() =>
        {
            _result = null;
        });
    }

    public TaskItem GetResult()
    {
        return _result;
    }

    private void Validate()
    {
        ErrorMessage = null;
        _canSave = true;

        if (string.IsNullOrWhiteSpace(TaskName))
        {
            ErrorMessage = "Task Name is required.";
            _canSave = false;
            return;
        }

        if (!StartDate.HasValue)
        {
            ErrorMessage = "Start Date is required.";
            _canSave = false;
            return;
        }

        if (DependsOnList != null && DependsOnList.Any())
        {
            var latestDependencyEnd = DependsOnList.Max(t => t.EndDate);
            if (StartDate.Value < latestDependencyEnd)
            {
                var latestTask = DependsOnList.First(t => t.EndDate == latestDependencyEnd);
                ErrorMessage = $"Start Date cannot be earlier than the end date of '{latestTask.Name}' ({latestTask.EndDate:MMM dd, yyyy}).";
                _canSave = false;
            }
        }
    }

    private bool IsValid()
    {
        Validate();
        return _canSave;
    }
}