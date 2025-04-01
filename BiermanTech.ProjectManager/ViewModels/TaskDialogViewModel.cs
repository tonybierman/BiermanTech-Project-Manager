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
    private DateTimeOffset? _startDate; // Changed to nullable to match DatePicker
    private double _durationDays;
    private double _percentComplete;
    private TaskItem _dependsOn;
    private readonly List<TaskItem> _tasks;
    private readonly TaskItem _task;

    public string TaskName
    {
        get => _taskName;
        set => this.RaiseAndSetIfChanged(ref _taskName, value);
    }

    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
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
        set => this.RaiseAndSetIfChanged(ref _dependsOn, value);
    }

    public List<TaskItem> AvailableTasks
    {
        get => _tasks.Where(t => t != _task).ToList();
    }

    public ReactiveCommand<Unit, TaskItem> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private TaskItem _result;

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
            StartDate = DateTimeOffset.Now; // Default to today
            DurationDays = 1;
            PercentComplete = 0;
            DependsOn = null;
        }

        SaveCommand = ReactiveCommand.Create(() =>
        {
            // Ensure StartDate has a value before saving
            if (!StartDate.HasValue)
            {
                throw new InvalidOperationException("Start Date is required.");
            }

            _result = new TaskItem
            {
                Id = _task?.Id ?? Guid.NewGuid(),
                Name = TaskName,
                StartDate = StartDate.Value, // Use the value since we checked for null
                Duration = TimeSpan.FromDays(DurationDays),
                PercentComplete = PercentComplete,
                DependsOn = DependsOn
            };
            return _result;
        });

        CancelCommand = ReactiveCommand.Create(() =>
        {
            _result = null;
        });
    }

    public TaskItem GetResult()
    {
        return _result;
    }
}