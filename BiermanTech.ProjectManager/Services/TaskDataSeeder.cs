using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Services;

public class TaskDataSeeder
{
    private readonly ITaskRepository _taskRepository;

    public TaskDataSeeder(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public void SeedSampleData()
    {
        var tasks = new[]
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

        // Add tasks to the repository
        foreach (var task in tasks)
        {
            _taskRepository.AddTask(task);
        }

        // Get the task list for setting dependencies
        var taskList = _taskRepository.GetTasks();

        // Set dependencies
        taskList[1].DependsOn = taskList[0];
        taskList[2].DependsOn = taskList[1];
        taskList[3].DependsOn = taskList[0];
        taskList[4].DependsOn = taskList[1];
        taskList[5].DependsOn = taskList[4];
        taskList[6].DependsOn = taskList[2];
        taskList[7].DependsOn = taskList[6];
        taskList[8].DependsOn = taskList[2];
        taskList[9].DependsOn = taskList[7];
        taskList[10].DependsOn = taskList[9];
        taskList[11].DependsOn = taskList[10];

        // Calculate percent complete
        DateTimeOffset today = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero);
        foreach (var task in taskList)
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