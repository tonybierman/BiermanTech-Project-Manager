using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Services;

public class DbTaskRepository : ITaskRepository
{
    private readonly ProjectDbContext _context;
    private readonly int _projectId;
    private readonly List<TaskItem> _tasks;

    public event EventHandler TasksChanged;

    public DbTaskRepository(ProjectDbContext context, int projectId)
    {
        _context = context;
        _projectId = projectId;
        _tasks = new List<TaskItem>();
        LoadTasks();
    }

    public IEnumerable<TaskItem> GetTasks()
    {
        return _tasks.AsReadOnly();
    }

    public void AddTask(TaskItem task)
    {
        // Check if the task already exists in the database
        if (task.Id != 0 && _context.Tasks.Any(t => t.Id == task.Id))
        {
            // Task already exists in the database; just add it to the in-memory list
            if (!_tasks.Any(t => t.Id == task.Id))
            {
                _tasks.Add(task);
                OnTasksChanged();
            }
        }
        else
        {
            // New task; add to both the database and in-memory list
            _tasks.Add(task);
            _context.Tasks.Add(task);
            _context.SaveChanges();
            OnTasksChanged();
        }
    }

    public void UpdateTask(TaskItem task)
    {
        var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
        if (existingTask != null)
        {
            existingTask.Name = task.Name;
            existingTask.StartDate = task.StartDate;
            existingTask.Duration = task.Duration;
            existingTask.PercentComplete = task.PercentComplete;
            existingTask.TaskDependencies = task.TaskDependencies;
            existingTask.DependsOn = task.DependsOn;
            existingTask.Children = task.Children;

            _context.Tasks.Update(existingTask);
            _context.SaveChanges();
            OnTasksChanged();
        }
    }

    public void DeleteTask(TaskItem task)
    {
        var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
        if (existingTask != null)
        {
            _tasks.Remove(existingTask);
            _context.Tasks.Remove(existingTask);
            _context.SaveChanges();
            OnTasksChanged();
        }
    }

    public void ClearTasks()
    {
        _tasks.Clear();
        OnTasksChanged();
    }

    private void LoadTasks()
    {
        var tasks = _context.Tasks
            .Include(t => t.TaskDependencies)
            .ThenInclude(td => td.DependsOnTask)
            .Include(t => t.Children)
            .Where(t => t.ProjectId == _projectId)
            .ToList();

        foreach (var task in tasks)
        {
            task.DependsOn = task.TaskDependencies?
                .Select(td => td.DependsOnTask)
                .Where(t => t != null)
                .ToList() ?? new List<TaskItem>();

            _tasks.Add(task);
        }
    }

    private void OnTasksChanged()
    {
        TasksChanged?.Invoke(this, EventArgs.Empty);
    }
}