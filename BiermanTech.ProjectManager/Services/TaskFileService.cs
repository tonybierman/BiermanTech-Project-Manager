using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using System.Linq;

namespace BiermanTech.ProjectManager.Services;

public class TaskFileService
{
    private readonly string _defaultFilePath;

    public TaskFileService(string defaultFilePath = "default_tasks.json")
    {
        _defaultFilePath = defaultFilePath;
    }

    public async Task<Project> LoadProjectAsync(string filePath = null)
    {
        filePath ??= _defaultFilePath;
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Information("Project file {FilePath} does not exist. Returning empty project.", filePath);
                return new Project { Name = "Default Project", Author = "Unknown" };
            }

            var json = await File.ReadAllTextAsync(filePath);
            var project = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (project == null)
            {
                Log.Warning("Deserialized project is null. Returning empty project.");
                return new Project { Name = "Default Project", Author = "Unknown" };
            }

            // Build a dictionary of all tasks (including children) for dependency resolution
            var taskDictionary = new Dictionary<int, TaskItem>(); // Changed from Guid to int
            void AddTasksToDictionary(IEnumerable<TaskItem> tasks)
            {
                foreach (var task in tasks)
                {
                    taskDictionary[task.Id] = task;
                    if (task.Children.Any())
                    {
                        AddTasksToDictionary(task.Children);
                    }
                }
            }
            AddTasksToDictionary(project.Tasks);

            // Resolve DependsOn references for all tasks
            void ResolveDependencies(IEnumerable<TaskItem> tasks)
            {
                foreach (var task in tasks)
                {
                    task.DependsOn.Clear(); // Reset to avoid duplicates
                    foreach (var depId in task.DependsOnIds)
                    {
                        if (taskDictionary.TryGetValue(depId, out var dependsOn))
                        {
                            task.DependsOn.Add(dependsOn);
                        }
                        else
                        {
                            Log.Warning("Dependency ID {DepId} for task {TaskId} not found.", depId, task.Id);
                        }
                    }
                    if (task.Children.Any())
                    {
                        ResolveDependencies(task.Children);
                    }
                }
            }
            ResolveDependencies(project.Tasks);

            Log.Information("Loaded project '{ProjectName}' by {Author} with {TaskCount} top-level tasks from {FilePath}",
                project.Name, project.Author, project.Tasks.Count, filePath);
            return project;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load project from {FilePath}", filePath);
            throw;
        }
    }

    public async Task SaveProjectAsync(Project project, string filePath = null)
    {
        filePath ??= _defaultFilePath;
        try
        {
            var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // Ignore null values to keep JSON clean (optional)
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            await File.WriteAllTextAsync(filePath, json);
            Log.Information("Saved project '{ProjectName}' by {Author} with {TaskCount} top-level tasks to {FilePath}",
                project.Name, project.Author, project.Tasks.Count, filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save project to {FilePath}", filePath);
            throw;
        }
    }
}