using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class UpdateTaskCommand : ICommand
{
    private readonly TaskItem _originalTask;
    private readonly TaskItem _updatedTask;
    private readonly ProjectDbContext _context;
    private TaskItem _previousState;
    private TaskItem _parentTask;

    public UpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask, ProjectDbContext context)
    {
        _originalTask = originalTask ?? throw new ArgumentNullException(nameof(originalTask));
        _updatedTask = updatedTask ?? throw new ArgumentNullException(nameof(updatedTask));
        _context = context ?? throw new ArgumentNullException(nameof(context));

        // Capture previous state from database
        var dbTask = _context.Tasks
            .Include(t => t.TaskDependencies)
            .AsNoTracking() // Avoid tracking for previous state
            .FirstOrDefault(t => t.Id == originalTask.Id);
        if (dbTask != null)
        {
            _previousState = new TaskItem
            {
                Id = dbTask.Id,
                Name = dbTask.Name,
                StartDate = dbTask.StartDate,
                Duration = dbTask.Duration,
                PercentComplete = dbTask.PercentComplete,
                TaskDependencies = new List<TaskDependency>(dbTask.TaskDependencies),
                Children = dbTask.Children.ToList() // Shallow copy
            };
        }
    }

    public void Execute()
    {
        // Load the task to update with its relationships
        var taskToUpdate = _context.Tasks
            .Include(t => t.TaskDependencies)
            .Include(t => t.Children)
            .FirstOrDefault(t => t.Id == _originalTask.Id);
        if (taskToUpdate == null) return;

        // Update basic properties
        taskToUpdate.Name = _updatedTask.Name;
        taskToUpdate.StartDate = _updatedTask.StartDate;
        taskToUpdate.Duration = _updatedTask.Duration;
        taskToUpdate.PercentComplete = _updatedTask.PercentComplete;

        // Update dependencies
        var currentDependsOnIds = taskToUpdate.TaskDependencies.Select(td => td.DependsOnId).ToList();
        var newDependsOnIds = _updatedTask.DependsOnIds ?? new List<int>();

        // Remove old dependencies not in new list
        var dependenciesToRemove = taskToUpdate.TaskDependencies
            .Where(td => !newDependsOnIds.Contains(td.DependsOnId))
            .ToList();
        foreach (var dependency in dependenciesToRemove)
        {
            _context.TaskDependencies.Remove(dependency);
        }

        // Add new dependencies not in current list
        foreach (var newId in newDependsOnIds.Where(id => !currentDependsOnIds.Contains(id)))
        {
            if (_context.Tasks.Any(t => t.Id == newId)) // Validate existence
            {
                _context.TaskDependencies.Add(new TaskDependency
                {
                    TaskId = taskToUpdate.Id,
                    DependsOnId = newId
                });
            }
        }

        // Find and update parent
        _parentTask = _context.Tasks
            .Include(t => t.Children)
            .FirstOrDefault(t => t.Children.Any(c => c.Id == _originalTask.Id));
        if (_parentTask != null)
        {
            // EF will recalculate via properties; no need to null out
        }

        // Save changes
        _context.SaveChanges();
    }

    public void Undo()
    {
        var taskToRevert = _context.Tasks
            .Include(t => t.TaskDependencies)
            .FirstOrDefault(t => t.Id == _originalTask.Id);
        if (taskToRevert == null) return;

        // Revert basic properties
        taskToRevert.Name = _previousState.Name;
        taskToRevert.StartDate = _previousState.StartDate;
        taskToRevert.Duration = _previousState.Duration;
        taskToRevert.PercentComplete = _previousState.PercentComplete;

        // Revert dependencies
        var currentDependencies = taskToRevert.TaskDependencies.ToList();
        foreach (var dependency in currentDependencies)
        {
            _context.TaskDependencies.Remove(dependency);
        }
        foreach (var prevDependency in _previousState.TaskDependencies)
        {
            _context.TaskDependencies.Add(new TaskDependency
            {
                TaskId = prevDependency.TaskId,
                DependsOnId = prevDependency.DependsOnId
            });
        }

        // Save changes
        _context.SaveChanges();
    }
}