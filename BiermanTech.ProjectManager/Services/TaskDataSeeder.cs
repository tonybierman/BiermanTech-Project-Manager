using BiermanTech.ProjectManager.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using System.Text.Json;

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
        Log.Information("Starting SeedSampleDataAsync");
        Project project;
        try
        {
            project = await _taskFileService.LoadProjectAsync();
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize default_tasks.json. Using empty project.");
            project = new Project { Name = "Default Project", Author = "Unknown" };
        }
        Log.Information("Loaded project: {Name}, Task count: {Count}", project.Name, project.Tasks?.Count ?? 0);
        if (project.Tasks == null || project.Tasks.Count == 0)
        {
            Log.Warning("No tasks loaded from file");
            return project;
        }
        // Only add top-level tasks
        foreach (var task in project.Tasks)
        {
            _taskRepository.AddTask(task); // Children stay nested
            Log.Information("Added top-level task: {TaskName}", task.Name);
        }
        Log.Information("Seeding complete");
        return project;
    }
}