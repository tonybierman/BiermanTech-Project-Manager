using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Models;

public class DbTaskRepository : ITaskRepository
{
    private readonly ProjectDbContext _context;
    private readonly int _projectId;

    public event EventHandler TasksChanged;

    public DbTaskRepository(ProjectDbContext context, int projectId)
    {
        _context = context;
        _projectId = projectId;
    }

    public List<TaskItem> GetTasks()
    {
        var tasks = _context.Tasks
            .Where(t => t.ProjectId == _projectId)
            .Include(t => t.Children)
            .Include(t => t.TaskDependencies)
            .ToList();

        // Populate DependsOn for runtime use
        foreach (var task in tasks)
        {
            task.DependsOn = _context.Tasks
                .Where(t => task.TaskDependencies.Select(td => td.DependsOnId).Contains(t.Id))
                .ToList();
        }

        return tasks;
    }

    public void AddTask(TaskItem task, int? parentTaskId = null)
    {
        task.ProjectId = _projectId;
        if (parentTaskId.HasValue) task.ParentId = parentTaskId.Value;

        _context.Tasks.Add(task);
        _context.SaveChanges(); // Save to get task.Id

        // Add dependencies from DependsOnIds (set by TaskDialogViewModel or JSON)
        if (task.DependsOnIds != null && task.DependsOnIds.Any())
        {
            foreach (var depId in task.DependsOnIds)
            {
                if (_context.Tasks.Any(t => t.Id == depId)) // Validate existence
                {
                    _context.TaskDependencies.Add(new TaskDependency
                    {
                        TaskId = task.Id,
                        DependsOnId = depId
                    });
                }
            }
            _context.SaveChanges();
        }

        NotifyTasksChanged();
    }

    public void RemoveTask(TaskItem task)
    {
        _context.Tasks.Remove(task); // Cascade will remove TaskDependencies entries
        _context.SaveChanges();
        NotifyTasksChanged();
    }

    public void NotifyTasksChanged()
    {
        TasksChanged?.Invoke(this, EventArgs.Empty);
    }
}