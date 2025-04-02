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

            // Resolve DependsOn references
            var taskDictionary = project.TaskItems.ToDictionary(t => t.Id, t => t);
            foreach (var task in project.TaskItems)
            {
                if (task.DependsOnId.HasValue && taskDictionary.TryGetValue(task.DependsOnId.Value, out var dependsOn))
                {
                    task.DependsOn = dependsOn;
                }
            }

            Log.Information("Loaded project '{ProjectName}' by {Author} with {TaskCount} tasks from {FilePath}",
                project.Name, project.Author, project.TaskItems.Count, filePath);
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
            // Prepare tasks for serialization by setting DependsOnId
            foreach (var task in project.TaskItems)
            {
                task.DependsOnId = task.DependsOn?.Id;
            }

            var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
            Log.Information("Saved project '{ProjectName}' by {Author} with {TaskCount} tasks to {FilePath}",
                project.Name, project.Author, project.TaskItems.Count, filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save project to {FilePath}", filePath);
            throw;
        }
    }
}