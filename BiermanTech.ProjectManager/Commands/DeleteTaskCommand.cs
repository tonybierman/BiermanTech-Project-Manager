using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class DeleteTaskCommand : ICommand
{
    private readonly TaskItem _task;
    private readonly ITaskRepository _taskRepository;
    private TaskItem _parentTask; // For hierarchy
    private int _indexInParent; // Index in parent's Children
    private readonly Dictionary<TaskItem, List<TaskItem>> _previousDependencies; // Store original DependsOn lists

    public DeleteTaskCommand(TaskItem task, ITaskRepository taskRepository)
    {
        _task = task;
        _taskRepository = taskRepository;
        _previousDependencies = new Dictionary<TaskItem, List<TaskItem>>();
    }

    public void Execute()
    {
        var tasks = _taskRepository.GetTasks();
        _parentTask = FindParentTask(tasks, _task);

        if (_parentTask != null)
        {
            _indexInParent = _parentTask.Children.IndexOf(_task);
            _parentTask.Children.Remove(_task);
        }
        else
        {
            _indexInParent = tasks.IndexOf(_task);
            tasks.Remove(_task);
        }

        // Update all tasks that depend on this one
        var allTasks = FlattenTasks(tasks).ToList();
        foreach (var dependentTask in allTasks.Where(t => t.DependsOn.Contains(_task)))
        {
            _previousDependencies[dependentTask] = new List<TaskItem>(dependentTask.DependsOn);
            dependentTask.DependsOn.Remove(_task);
            dependentTask.DependsOnIds.Remove(_task.Id);
        }

        if (_parentTask != null)
        {
            _parentTask.StartDate = null; // Recalculate parent
            _parentTask.Duration = null;
        }

        _taskRepository.NotifyTasksChanged();
    }

    public void Undo()
    {
        var tasks = _taskRepository.GetTasks();
        if (_parentTask != null)
        {
            _parentTask.Children.Insert(_indexInParent, _task);
            _parentTask.StartDate = null; // Recalculate parent
            _parentTask.Duration = null;
        }
        else
        {
            tasks.Insert(_indexInParent, _task);
        }

        // Restore dependencies
        foreach (var kvp in _previousDependencies)
        {
            kvp.Key.DependsOn = new List<TaskItem>(kvp.Value);
            kvp.Key.DependsOnIds = kvp.Value.Select(t => t.Id).ToList();
        }

        _taskRepository.NotifyTasksChanged();
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

    private IEnumerable<TaskItem> FlattenTasks(IEnumerable<TaskItem> tasks)
    {
        foreach (var task in tasks)
        {
            yield return task;
            foreach (var child in FlattenTasks(task.Children))
            {
                yield return child;
            }
        }
    }
}