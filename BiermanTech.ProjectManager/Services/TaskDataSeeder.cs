using BiermanTech.ProjectManager.Models;
using System;
using System.Linq;
using System.Collections.Generic;
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
        if (project.Tasks == null || project.Tasks.Count == 0)
        {
            return project;
        }

        // Add all tasks (including children) to the repository
        void AddTasksToRepository(IEnumerable<TaskItem> tasks)
        {
            foreach (var task in tasks)
            {
                _taskRepository.AddTask(task);
                if (task.Children.Any())
                {
                    AddTasksToRepository(task.Children);
                }
            }
        }
        AddTasksToRepository(project.Tasks);

        return project;
    }
}