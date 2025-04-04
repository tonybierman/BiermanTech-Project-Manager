using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class UpdateTaskCommand : ICommand
{
    private readonly TaskItem _originalTask;
    private readonly TaskItem _updatedTask;
    private readonly ITaskRepository _taskRepository;
    private TaskItem _previousState;
    private TaskItem _parentTask; // For recalculating parent dates

    public UpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask, ITaskRepository taskRepository)
    {
        _originalTask = originalTask;
        _updatedTask = updatedTask;
        _taskRepository = taskRepository;
        _previousState = new TaskItem
        {
            Id = originalTask.Id,
            Name = originalTask.Name,
            StartDate = originalTask.StartDate,
            Duration = originalTask.Duration,
            PercentComplete = originalTask.PercentComplete,
            DependsOnIds = new List<int>(originalTask.DependsOnIds), // Changed from List<Guid> to List<int>
            DependsOn = new List<TaskItem>(originalTask.DependsOn),
            Children = originalTask.Children.ToList() // Shallow copy, assuming children don’t change here
        };
    }

    public void Execute()
    {
        var tasks = _taskRepository.GetTasks();
        var taskToUpdate = FindTaskById(tasks, _originalTask.Id);
        if (taskToUpdate != null)
        {
            taskToUpdate.Name = _updatedTask.Name;
            taskToUpdate.StartDate = _updatedTask.StartDate;
            taskToUpdate.Duration = _updatedTask.Duration;
            taskToUpdate.PercentComplete = _updatedTask.PercentComplete;
            taskToUpdate.DependsOnIds = new List<int>(_updatedTask.DependsOnIds); // Changed from List<Guid> to List<int>
            taskToUpdate.DependsOn = new List<TaskItem>(_updatedTask.DependsOn);

            _parentTask = FindParentTask(tasks, taskToUpdate);
            if (_parentTask != null)
            {
                // Recalculate parent dates if this is a child task
                _parentTask.StartDate = null; // Clear to force recalculation
                _parentTask.Duration = null;
            }

            _taskRepository.NotifyTasksChanged();
        }
    }

    public void Undo()
    {
        var tasks = _taskRepository.GetTasks();
        var taskToRevert = FindTaskById(tasks, _originalTask.Id);
        if (taskToRevert != null)
        {
            taskToRevert.Name = _previousState.Name;
            taskToRevert.StartDate = _previousState.StartDate;
            taskToRevert.Duration = _previousState.Duration;
            taskToRevert.PercentComplete = _previousState.PercentComplete;
            taskToRevert.DependsOnIds = new List<int>(_previousState.DependsOnIds); // Changed from List<Guid> to List<int>
            taskToRevert.DependsOn = new List<TaskItem>(_previousState.DependsOn);

            if (_parentTask != null)
            {
                // Recalculate parent dates after reverting
                _parentTask.StartDate = null; // Clear to force recalculation
                _parentTask.Duration = null;
            }

            _taskRepository.NotifyTasksChanged();
        }
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