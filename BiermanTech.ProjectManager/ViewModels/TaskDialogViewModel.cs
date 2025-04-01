using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.ViewModels;

public class TaskDialogViewModel : ViewModelBase
{
    private string _taskName;
    private DateTimeOffset? _startDate;
    private double _durationDays;
    private TaskItem _dependsOn;
    private readonly ObservableCollection<TaskItem> _availableTasks;
    private readonly TaskItem _taskToEdit;
    private readonly Window _dialogWindow;

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

    public TaskItem DependsOn
    {
        get => _dependsOn;
        set => this.RaiseAndSetIfChanged(ref _dependsOn, value);
    }

    public ObservableCollection<TaskItem> AvailableTasks
    {
        get => _availableTasks;
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public TaskDialogViewModel(TaskItem taskToEdit = null, ObservableCollection<TaskItem> allTasks = null, Window dialogWindow = null)
    {
        _taskToEdit = taskToEdit;
        _dialogWindow = dialogWindow;
        _availableTasks = new ObservableCollection<TaskItem>(allTasks ?? new ObservableCollection<TaskItem>());
        _availableTasks.Insert(0, null);

        if (taskToEdit != null)
        {
            TaskName = taskToEdit.Name;
            StartDate = taskToEdit.StartDate;
            DurationDays = taskToEdit.Duration.TotalDays;
            DependsOn = taskToEdit.DependsOn;
        }
        else
        {
            TaskName = "New Task";
            StartDate = DateTimeOffset.Now;
            DurationDays = 3;
            DependsOn = null;
        }

        var canSave = this.WhenAnyValue(x => x.TaskName, x => x.DurationDays)
            .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && x.Item2 > 0);

        SaveCommand = ReactiveCommand.Create(() =>
        {
            var task = _taskToEdit ?? new TaskItem();
            task.Name = TaskName;
            task.StartDate = StartDate ?? DateTimeOffset.Now;
            task.Duration = TimeSpan.FromDays(DurationDays);
            task.DependsOn = DependsOn;
            _dialogWindow.Close(task);
        }, canSave);

        CancelCommand = ReactiveCommand.Create(() =>
        {
            _dialogWindow.Close(null);
        });
    }
}