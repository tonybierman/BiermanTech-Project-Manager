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
    private TaskItem _dependsOn;
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
            Validate(); // Validate when StartDate changes
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

    public TaskItem DependsOn
    {
        get => _dependsOn;
        set
        {
            this.RaiseAndSetIfChanged(ref _dependsOn, value);
            Validate(); // Validate when DependsOn changes
        }
    }

    public List<TaskItem> AvailableTasks
    {
        get => _tasks.Where(t => t != _task).ToList();
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
            DurationDays = task.Duration.TotalDays;
            PercentComplete = task.PercentComplete;
            DependsOn = task.DependsOn;
        }
        else
        {
            TaskName = string.Empty;
            StartDate = DateTimeOffset.Now;
            DurationDays = 1;
            PercentComplete = 0;
            DependsOn = null;
        }

        // Initialize CanSave based on initial validation
        Validate();

        // Define SaveCommand with a CanExecute condition
        SaveCommand = ReactiveCommand.Create(
            () =>
            {
                _result = new TaskItem
                {
                    Id = _task?.Id ?? Guid.NewGuid(),
                    Name = TaskName,
                    StartDate = StartDate.Value,
                    Duration = TimeSpan.FromDays(DurationDays),
                    PercentComplete = PercentComplete,
                    DependsOn = DependsOn
                };
                return _result;
            },
            this.WhenAnyValue(
                x => x.StartDate,
                x => x.DependsOn,
                x => x.TaskName,
                (startDate, dependsOn, taskName) =>
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

        // Rule: Task name cannot be empty
        if (string.IsNullOrWhiteSpace(TaskName))
        {
            ErrorMessage = "Task Name is required.";
            _canSave = false;
            return;
        }

        // Rule: Start date must be set
        if (!StartDate.HasValue)
        {
            ErrorMessage = "Start Date is required.";
            _canSave = false;
            return;
        }

        // Rule: If the task depends on another task, its start date cannot be earlier than the depended task's start date
        if (DependsOn != null && StartDate.HasValue)
        {
            if (StartDate.Value < DependsOn.StartDate)
            {
                ErrorMessage = $"Start Date cannot be earlier than the start date of '{DependsOn.Name}' ({DependsOn.StartDate:MMM dd, yyyy}).";
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