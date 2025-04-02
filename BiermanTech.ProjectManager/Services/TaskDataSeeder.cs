using BiermanTech.ProjectManager.Models;
using System;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.Services;

public class TaskDataSeeder
{
    private readonly ITaskRepository _taskRepository;
    private readonly TaskFileService _taskFileService;

    public TaskDataSeeder(ITaskRepository taskRepository, TaskFileService taskFileService)
    {
        _taskRepository = taskRepository;
        _taskFileService = taskFileService;
    }

    public async Task<Project> SeedSampleDataAsync()
    {
        // Load the project from the JSON file
        var project = await _taskFileService.LoadProjectAsync();

        // If no tasks were loaded (e.g., file doesn't exist), log a warning and return
        if (project.TaskItems == null || project.TaskItems.Count == 0)
        {
            return project;
        }

        // Add tasks to the repository
        foreach (var task in project.TaskItems)
        {
            _taskRepository.AddTask(task);
        }

        // Calculate percent complete
        DateTimeOffset today = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero);
        foreach (var task in _taskRepository.GetTasks())
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

        return project;
    }
}