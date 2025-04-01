using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;

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

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
        NotifyTasksChanged();
    }

    public void RemoveTask(TaskItem task)
    {
        _tasks.Remove(task);
        NotifyTasksChanged();
    }

    public void NotifyTasksChanged()
    {
        TasksChanged?.Invoke(this, EventArgs.Empty);
    }
}