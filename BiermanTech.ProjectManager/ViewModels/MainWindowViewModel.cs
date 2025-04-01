using ReactiveUI;
using System.Collections.ObjectModel;
using BiermanTech.ProjectManager.Models;
using System;

namespace BiermanTech.ProjectManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<TaskItem> _tasks;

    public ObservableCollection<TaskItem> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    public MainWindowViewModel()
    {
        Tasks = new ObservableCollection<TaskItem>
        {
            new TaskItem { Name = "Planning", StartDate = new DateTime(2025, 4, 1), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "Model Dev", StartDate = new DateTime(2025, 4, 4), Duration = TimeSpan.FromDays(4) },
            new TaskItem { Name = "ViewModel Dev", StartDate = new DateTime(2025, 4, 8), Duration = TimeSpan.FromDays(7) },
            new TaskItem { Name = "UI Design", StartDate = new DateTime(2025, 4, 6), Duration = TimeSpan.FromDays(5) },
            new TaskItem { Name = "Database Setup", StartDate = new DateTime(2025, 4, 10), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "API Integration", StartDate = new DateTime(2025, 4, 13), Duration = TimeSpan.FromDays(6) },
            new TaskItem { Name = "Unit Testing", StartDate = new DateTime(2025, 4, 15), Duration = TimeSpan.FromDays(4) },
            new TaskItem { Name = "Code Review", StartDate = new DateTime(2025, 4, 18), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "Documentation", StartDate = new DateTime(2025, 4, 20), Duration = TimeSpan.FromDays(5) },
            new TaskItem { Name = "QA Testing", StartDate = new DateTime(2025, 4, 23), Duration = TimeSpan.FromDays(4) },
            new TaskItem { Name = "Deployment Prep", StartDate = new DateTime(2025, 4, 25), Duration = TimeSpan.FromDays(3) },
            new TaskItem { Name = "Launch", StartDate = new DateTime(2025, 4, 28), Duration = TimeSpan.FromDays(2) }
        };

        // Set dependencies
        Tasks[1].DependsOn = Tasks[0]; // Model Dev depends on Planning
        Tasks[2].DependsOn = Tasks[1]; // ViewModel Dev depends on Model Dev
        Tasks[3].DependsOn = Tasks[0]; // UI Design depends on Planning
        Tasks[4].DependsOn = Tasks[1]; // Database Setup depends on Model Dev
        Tasks[5].DependsOn = Tasks[4]; // API Integration depends on Database Setup
        Tasks[6].DependsOn = Tasks[2]; // Unit Testing depends on ViewModel Dev
        Tasks[7].DependsOn = Tasks[6]; // Code Review depends on Unit Testing
        Tasks[8].DependsOn = Tasks[2]; // Documentation depends on ViewModel Dev
        Tasks[9].DependsOn = Tasks[7]; // QA Testing depends on Code Review
        Tasks[10].DependsOn = Tasks[9]; // Deployment Prep depends on QA Testing
        Tasks[11].DependsOn = Tasks[10]; // Launch depends on Deployment Prep

        // Set percent complete based on current date (April 1, 2025)
        DateTime today = new DateTime(2025, 4, 1);
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
    }
}