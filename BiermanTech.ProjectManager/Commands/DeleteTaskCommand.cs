using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class DeleteTaskCommand : ICommand
{
    private readonly TaskItem _task;
    private readonly ProjectDbContext _context;
    private TaskItem _parentTask; // For hierarchy
    private int _indexInParent; // Index in parent's Children
    private readonly Dictionary<TaskItem, List<int>> _previousDependsOnIds; // Store original DependsOnIds

    public DeleteTaskCommand(TaskItem task, ProjectDbContext context)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _previousDependsOnIds = new Dictionary<TaskItem, List<int>>();
    }

    public void Execute()
    {
        // Load task with relationships
        var taskToDelete = _context.Tasks
            .Include(t => t.Children)
            .Include(t => t.TaskDependencies)
            .FirstOrDefault(t => t.Id == _task.Id);
        if (taskToDelete == null) return;

        // Find parent
        _parentTask = _context.Tasks
            .Include(t => t.Children)
            .FirstOrDefault(t => t.Children.Any(c => c.Id == _task.Id));
        if (_parentTask != null)
        {
            _indexInParent = _parentTask.Children.ToList().FindIndex(t => t.Id == _task.Id);
            _parentTask.Children.Remove(taskToDelete);
        }

        // Store and update dependencies
        var dependentTasks = _context.Tasks
            .Include(t => t.TaskDependencies)
            .Where(t => t.TaskDependencies.Any(td => td.DependsOnId == _task.Id))
            .ToList();
        foreach (var dependentTask in dependentTasks)
        {
            _previousDependsOnIds[dependentTask] = new List<int>(dependentTask.TaskDependencies.Select(td => td.DependsOnId));
            var dependencyToRemove = dependentTask.TaskDependencies.FirstOrDefault(td => td.DependsOnId == _task.Id);
            if (dependencyToRemove != null)
            {
                _context.TaskDependencies.Remove(dependencyToRemove);
            }
        }

        // Remove the task (cascade will handle TaskDependencies where TaskId matches)
        _context.Tasks.Remove(taskToDelete);

        // Save changes
        _context.SaveChanges();
    }

    public void Undo()
    {
        // Re-add the task
        _context.Tasks.Add(_task);
        if (_parentTask != null)
        {
            _parentTask.Children.Insert(_indexInParent, _task);
        }

        // Restore dependencies
        foreach (var kvp in _previousDependsOnIds)
        {
            foreach (var dependsOnId in kvp.Value)
            {
                if (!_context.TaskDependencies.Any(td => td.TaskId == kvp.Key.Id && td.DependsOnId == dependsOnId))
                {
                    _context.TaskDependencies.Add(new TaskDependency
                    {
                        TaskId = kvp.Key.Id,
                        DependsOnId = dependsOnId
                    });
                }
            }
        }

        // Save changes
        _context.SaveChanges();
    }
}