using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Views;

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<TaskItem> _tasks;
    private TaskItem _selectedTask;
    private readonly Stack<Action> _undoStack;
    private readonly Window _parentWindow;

    public ObservableCollection<TaskItem> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
    }

    public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; }

    public MainWindowViewModel(Window parentWindow)
    {
        _parentWindow = parentWindow;
        _undoStack = new Stack<Action>();
        Tasks = new ObservableCollection<TaskItem>
        {
            new TaskItem { Name = "Planning", StartDate = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "Model Dev", StartDate = new DateTimeOffset(2025, 4, 4, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(4) },
            new TaskItem { Name = "ViewModel Dev", StartDate = new DateTimeOffset(2025, 4, 8, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(7) },
            new TaskItem { Name = "UI Design", StartDate = new DateTimeOffset(2025, 4, 6, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(5) },
            new TaskItem { Name = "Database Setup", StartDate = new DateTimeOffset(2025, 4, 10, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "API Integration", StartDate = new DateTimeOffset(2025, 4, 13, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(6) },
            new TaskItem { Name = "Unit Testing", StartDate = new DateTimeOffset(2025, 4, 15, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(4) },
            new TaskItem { Name = "Code Review", StartDate = new DateTimeOffset(2025, 4, 18, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "Documentation", StartDate = new DateTimeOffset(2025, 4, 20, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(5) },
            new TaskItem { Name = "QA Testing", StartDate = new DateTimeOffset(2025, 4, 23, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(4) },
            new TaskItem { Name = "Deployment Prep", StartDate = new DateTimeOffset(2025, 4, 25, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "Launch", StartDate = new DateTimeOffset(2025, 4, 28, 0, 0, 0, TimeSpan.Zero), Duration = TimeSpan.FromDays(2) }
        };

        // Set dependencies
        Tasks[1].DependsOn = Tasks[0];
        Tasks[2].DependsOn = Tasks[1];
        Tasks[3].DependsOn = Tasks[0];
        Tasks[4].DependsOn = Tasks[1];
        Tasks[5].DependsOn = Tasks[4];
        Tasks[6].DependsOn = Tasks[2];
        Tasks[7].DependsOn = Tasks[6];
        Tasks[8].DependsOn = Tasks[2];
        Tasks[9].DependsOn = Tasks[7];
        Tasks[10].DependsOn = Tasks[9];
        Tasks[11].DependsOn = Tasks[10];

        // Set percent complete
        DateTimeOffset today = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero);
        foreach (var task in Tasks)
        {
            if (today < task.StartDate)
                task.PercentComplete = 0;
            else if (today >= task.EndDate)
                task.PercentComplete = 100;
            else
            {
                double daysElapsed = (today - task.StartDate).TotalDays;
                double totalDays = task.Duration.TotalDays;
                task.PercentComplete = (daysElapsed / totalDays) * 100;
            }
        }

        // Commands
        CreateTaskCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var dialog = new TaskDialog(null, Tasks);
            var result = await dialog.ShowDialog<TaskItem>(_parentWindow);
            if (result != null)
            {
                Tasks.Add(result);
                _undoStack.Push(() => Tasks.Remove(result));
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

            var dialog = new TaskDialog(SelectedTask, Tasks);
            var result = await dialog.ShowDialog<TaskItem>(_parentWindow);
            if (result != null)
            {
                SelectedTask.Name = result.Name;
                SelectedTask.StartDate = result.StartDate;
                SelectedTask.Duration = result.Duration;
                SelectedTask.DependsOn = result.DependsOn;

                _undoStack.Push(() =>
                {
                    var taskToUpdate = Tasks.First(t => t.Id == originalTask.Id);
                    taskToUpdate.Name = originalTask.Name;
                    taskToUpdate.StartDate = originalTask.StartDate;
                    taskToUpdate.Duration = originalTask.Duration;
                    taskToUpdate.PercentComplete = originalTask.PercentComplete;
                    taskToUpdate.DependsOn = originalTask.DependsOn;
                });
            }
        }, this.WhenAnyValue(x => x.SelectedTask).Select(x => x != null));

        DeleteTaskCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedTask == null) return;

            var taskToDelete = SelectedTask;
            int index = Tasks.IndexOf(taskToDelete);
            Tasks.Remove(taskToDelete);

            var dependentTasks = Tasks.Where(t => t.DependsOn == taskToDelete).ToList();
            foreach (var dependent in dependentTasks)
            {
                dependent.DependsOn = null;
            }

            _undoStack.Push(() =>
            {
                Tasks.Insert(index, taskToDelete);
                foreach (var dependent in dependentTasks)
                {
                    dependent.DependsOn = taskToDelete;
                }
            });

            SelectedTask = null;
        }, this.WhenAnyValue(x => x.SelectedTask).Select(x => x != null));

        UndoCommand = ReactiveCommand.Create(() =>
        {
            if (_undoStack.Count > 0)
            {
                var undoAction = _undoStack.Pop();
                undoAction();
            }
        }, Observable.Return(true).Concat(Observable.Never<bool>()).StartWith(_undoStack.Count > 0));
    }
}