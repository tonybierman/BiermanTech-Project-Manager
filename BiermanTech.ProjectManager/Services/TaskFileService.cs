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
    private readonly string _filePath;

    public TaskFileService(string filePath = "default_tasks.json")
    {
        _filePath = filePath;
    }

    public async Task<Project> LoadProjectAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Log.Information("Project file {FilePath} does not exist. Returning empty project.", _filePath);
                return new Project { Name = "Default Project", Author = "Unknown" };
            }

            var json = await File.ReadAllTextAsync(_filePath);
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
                project.Name, project.Author, project.TaskItems.Count, _filePath);
            return project;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load project from {FilePath}", _filePath);
            throw;
        }
    }

    public async Task SaveProjectAsync(Project project)
    {
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

            await File.WriteAllTextAsync(_filePath, json);
            Log.Information("Saved project '{ProjectName}' by {Author} with {TaskCount} tasks to {FilePath}",
                project.Name, project.Author, project.TaskItems.Count, _filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save project to {FilePath}", _filePath);
            throw;
        }
    }
}