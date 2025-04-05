using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.ViewModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Serilog;
using System;

namespace BiermanTech.ProjectManager.Controls;

public class GanttChartViewModel : ReactiveObject
{
    private ObservableCollection<TaskItem> _tasks;
    private TaskItem _selectedTask;

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

    public GanttChartViewModel(MainWindowViewModel mainViewModel)
    {
        // Share the same ObservableCollection instance
        _tasks = mainViewModel.Tasks;
        _selectedTask = mainViewModel.SelectedTask;

        // Observe changes to MainWindowViewModel.Tasks
        mainViewModel.WhenAnyValue(x => x.Tasks)
            .Where(tasks => tasks != null)
            .Subscribe(tasks =>
            {
                Log.Information("MainWindowViewModel.Tasks changed, updating GanttChartViewModel.Tasks, count: {Count}", tasks?.Count ?? 0);
                Tasks = tasks; // Assign the same ObservableCollection
            });

        this.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                mainViewModel.SelectedTask = selectedTask; // Propagate to MainWindowViewModel
            });

        // Keep this for downward sync
        mainViewModel.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                SelectedTask = selectedTask;
            });
    }
}