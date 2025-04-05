using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Commands;

public class AddTaskCommand : ICommand
{
    private readonly TaskItem _task;
    private readonly ProjectDbContext _context;
    private readonly int? _parentTaskId;
    private TaskItem _parentTask;
    private readonly ILogger<AddTaskCommand> _logger;

    public AddTaskCommand(TaskItem task, ProjectDbContext context, int? parentTaskId = null, ILogger<AddTaskCommand> logger = null)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _parentTaskId = parentTaskId;
        _logger = logger;
    }

    public void Execute()
    {
        try
        {
            // Log initial task state
            LogInfo($"Adding task: Name={_task.Name}, Id={_task.Id}, ParentId={_task.ParentId ?? -1}, ProjectId={GetProjectId() ?? -1}, DependsOnIds={string.Join(",", _task.DependsOnIds ?? new List<int>())}");

            // Validate and set ParentId
            // TODO: What to do if no parent task Id set
            if (_parentTaskId.HasValue || _task.ParentId.HasValue)
            {
                int effectiveParentId = _parentTaskId ?? _task.ParentId.Value;
                _parentTask = _context.Tasks.FirstOrDefault(t => t.Id == effectiveParentId);
                if (_parentTask == null)
                {
                    LogError($"Parent task with ID {effectiveParentId} not found for task {_task.Name}.");
                    throw new InvalidOperationException($"Parent task with ID {effectiveParentId} not found.");
                }
                _task.ParentId = effectiveParentId;
                LogInfo($"Validated and set ParentId={_task.ParentId} for task {_task.Name}");
            }
            else
            {
                _task.ParentId = null;
                LogInfo($"No ParentId set for task {_task.Name}");
            }

            // Validate ProjectId
            // TODO: This doesn't work
            int? projectId = GetProjectId();
            if (!ValidateProjectId(projectId))
            {
                LogError($"ProjectId validation failed for task {_task.Name}. ProjectId={projectId ?? -1}. Available Project IDs: {string.Join(",", _context.Projects.Select(p => p.Id))}");
                throw new InvalidOperationException($"ProjectId validation failed for task {_task.Name}.");
            }
            LogInfo($"Validated ProjectId={projectId} for task {_task.Name}");

            // Save the task
            _context.Tasks.Add(_task);
            LogInfo($"Attempting to save task {_task.Name}...");
            _context.SaveChanges();
            LogInfo($"Task {_task.Name} saved with ID {_task.Id}");

            // Handle dependencies
            if (_task.DependsOnIds != null && _task.DependsOnIds.Any())
            {
                foreach (var dependsOnId in _task.DependsOnIds)
                {
                    if (!_context.Tasks.Any(t => t.Id == dependsOnId))
                    {
                        LogError($"Dependency task with ID {dependsOnId} not found for task {_task.Name}.");
                        throw new InvalidOperationException($"Dependency task with ID {dependsOnId} not found.");
                    }
                    _context.TaskDependencies.Add(new TaskDependency
                    {
                        TaskId = _task.Id,
                        DependsOnId = dependsOnId
                    });
                }
                LogInfo($"Saving {_task.DependsOnIds.Count} dependencies for task {_task.Name}...");
                _context.SaveChanges();
                LogInfo($"Dependencies saved for task {_task.Name}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error adding task {_task.Name}: {ex.Message}");
            LogError($"Task state: Id={_task.Id}, Name={_task.Name}, ParentId={_task.ParentId ?? -1}, ProjectId={GetProjectId() ?? -1}, DependsOnIds={string.Join(",", _task.DependsOnIds ?? new List<int>())}");
            throw; // Rethrow to maintain current crash behavior
        }
    }

    public void Undo()
    {
        try
        {
            var taskToRemove = _context.Tasks
                .Include(t => t.TaskDependencies)
                .FirstOrDefault(t => t.Id == _task.Id);
            if (taskToRemove == null)
            {
                LogInfo($"Task ID {_task.Id} not found for undo.");
                return;
            }

            foreach (var dependency in taskToRemove.TaskDependencies.ToList())
            {
                _context.TaskDependencies.Remove(dependency);
            }

            if (_parentTask != null)
            {
                _context.Entry(_parentTask).Collection(t => t.Children).Load();
                _parentTask.Children.Remove(taskToRemove);
            }

            _context.Tasks.Remove(taskToRemove);
            LogInfo($"Undoing task {_task.Name} (ID: {_task.Id})...");
            _context.SaveChanges();
            LogInfo($"Task {_task.Name} undone.");
        }
        catch (Exception ex)
        {
            LogError($"Error undoing task {_task.Name}: {ex.Message}");
            throw;
        }
    }

    // Helper to get ProjectId (adjust based on TaskItem definition)
    private int? GetProjectId()
    {
        var property = _task.GetType().GetProperty("ProjectId");
        return property != null ? (int?)property.GetValue(_task) : null;
    }

    // Validate ProjectId
    private bool ValidateProjectId(int? projectId)
    {
        if (!projectId.HasValue || projectId == 0)
        {
            LogError($"ProjectId is null or 0 for task {_task.Name}. It must reference an existing project.");
            return false;
        }

        var exists = _context.Projects.Any(p => p.Id == projectId.Value);
        if (!exists)
        {
            LogError($"Project with ID {projectId.Value} not found for task {_task.Name}.");
            return false;
        }
        return true;
    }

    private void LogInfo(string message)
    {
        _logger?.LogInformation(message);
        Console.WriteLine($"[INF] {message}");
    }

    private void LogError(string message)
    {
        _logger?.LogError(message);
        Console.WriteLine($"[ERR] {message}");
    }
}