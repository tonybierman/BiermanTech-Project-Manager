using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using Serilog;
using System;

namespace BiermanTech.ProjectManager.Controls;

public class GanttChartViewModel : ReactiveObject
{
    private List<TaskItem> _tasks;
    private TaskItem _selectedTask;

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
            Log.Information("GanttChartViewModel SelectedTask set to: {TaskName}", value?.Name ?? "null");
        }
    }

    public GanttChartViewModel(MainWindowViewModel mainViewModel)
    {
        // Initialize with current tasks
        _tasks = mainViewModel.Tasks;
        _selectedTask = mainViewModel.SelectedTask;

        // Observe changes to MainWindowViewModel.Tasks
        mainViewModel.WhenAnyValue(x => x.Tasks)
            .Where(tasks => tasks != null)
            .Subscribe((Action<List<TaskItem>>)(tasks =>
            {
                Log.Information("MainWindowViewModel.Tasks changed, updating GanttChartViewModel.Tasks, count: {Count}", tasks?.Count ?? 0);
                Tasks = new List<TaskItem>(tasks);
            }));

        // Fix this subscription
        this.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                Log.Information("GanttChartViewModel.SelectedTask changed, updating MainWindowViewModel.SelectedTask: {TaskName}", selectedTask?.Name ?? "null");
                mainViewModel.SelectedTask = selectedTask; // Propagate to MainWindowViewModel
            });

        // Keep this for downward sync
        mainViewModel.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                Log.Information("MainWindowViewModel.SelectedTask changed, updating GanttChartViewModel.SelectedTask: {TaskName}", selectedTask?.Name ?? "null");
                SelectedTask = selectedTask;
            });
    }
}