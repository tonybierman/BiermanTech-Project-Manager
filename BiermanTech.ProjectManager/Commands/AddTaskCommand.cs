using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class AddTaskCommand : ICommand
{
    private readonly TaskItem _task;
    private readonly ITaskRepository _taskRepository;
    private readonly int? _parentTaskId; // Optional parent ID
    private TaskItem _parentTask; // For undo

    public AddTaskCommand(TaskItem task, ITaskRepository taskRepository, int? parentTaskId = null)
    {
        _task = task;
        _taskRepository = taskRepository;
        _parentTaskId = parentTaskId;
    }

    public void Execute()
    {
        var tasks = _taskRepository.GetTasks();

        // Assign a unique ID if not already set
        if (_task.Id == 0) // Assuming 0 indicates an unset ID
        {
            // Find the maximum ID across all tasks, including children
            int maxId = tasks.Any() ? tasks.SelectMany(t => GetAllTaskIds(t)).Max() : 0;
            _task.Id = maxId + 1;
        }

        if (_parentTaskId.HasValue)
        {
            _parentTask = FindTaskById(tasks, _parentTaskId.Value);
            if (_parentTask != null)
            {
                _parentTask.Children.Add(_task);
            }
            else
            {
                tasks.Add(_task); // Fallback to top-level if parent not found
            }
        }
        else
        {
            tasks.Add(_task);
        }
        _taskRepository.NotifyTasksChanged();
    }

    public void Undo()
    {
        var tasks = _taskRepository.GetTasks();
        if (_parentTask != null)
        {
            _parentTask.Children.Remove(_task);
        }
        else
        {
            tasks.Remove(_task);
        }
        _taskRepository.NotifyTasksChanged();
    }

    private TaskItem FindTaskById(IEnumerable<TaskItem> tasks, int id)
    {
        foreach (var task in tasks)
        {
            if (task.Id == id) return task;
            var found = FindTaskById(task.Children, id);
            if (found != null) return found;
        }
        return null;
    }

    // Helper method to recursively collect all task IDs
    private IEnumerable<int> GetAllTaskIds(TaskItem task)
    {
        yield return task.Id;
        foreach (var child in task.Children)
        {
            foreach (var id in GetAllTaskIds(child))
            {
                yield return id;
            }
        }
    }
}