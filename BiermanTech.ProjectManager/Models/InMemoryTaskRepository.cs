using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Services;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks;

    public event EventHandler TasksChanged;

    public InMemoryTaskRepository()
    {
        _tasks = new List<TaskItem>();
    }

    public List<TaskItem> GetTasks() => _tasks;

    public void AddTask(TaskItem task, int? parentTaskId = null) // Changed from Guid? to int?
    {
        if (parentTaskId.HasValue)
        {
            var parent = FindTaskById(_tasks, parentTaskId.Value);
            if (parent != null)
            {
                parent.Children.Add(task);
            }
            else
            {
                _tasks.Add(task); // Fallback to top-level if parent not found
            }
        }
        else
        {
            _tasks.Add(task);
        }
        NotifyTasksChanged();
    }

    public void RemoveTask(TaskItem task)
    {
        var parent = FindParentTask(_tasks, task);
        if (parent != null)
        {
            parent.Children.Remove(task);
        }
        else
        {
            _tasks.Remove(task);
        }
        NotifyTasksChanged();
    }

    public void NotifyTasksChanged()
    {
        TasksChanged?.Invoke(this, EventArgs.Empty);
    }

    private TaskItem FindTaskById(IEnumerable<TaskItem> tasks, int id) // Changed from Guid to int
    {
        foreach (var task in tasks)
        {
            if (task.Id == id) return task;
            var found = FindTaskById(task.Children, id);
            if (found != null) return found;
        }
        return null;
    }

    private TaskItem FindParentTask(IEnumerable<TaskItem> tasks, TaskItem child)
    {
        foreach (var task in tasks)
        {
            if (task.Children.Contains(child)) return task;
            var found = FindParentTask(task.Children, child);
            if (found != null) return found;
        }
        return null;
    }
}