using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Commands;

public class NewProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ITaskRepository _taskRepository;
    private Project _previousProjectState;
    private List<TaskItem> _previousTasks;

    public NewProjectCommand(Project project, ITaskRepository taskRepository)
    {
        _project = project;
        _taskRepository = taskRepository;

        // Store the previous state for undo
        _previousProjectState = new Project
        {
            Name = project.Name,
            Author = project.Author,
            TaskItems = new List<TaskItem>(project.TaskItems)
        };
        _previousTasks = new List<TaskItem>(_taskRepository.GetTasks());
    }

    public void Execute()
    {
        // Clear the current project and tasks
        _project.Name = "New Project";
        _project.Author = "Unknown";
        _project.TaskItems.Clear();

        // Clear the repository
        var currentTasks = _taskRepository.GetTasks();
        currentTasks.Clear();
        _taskRepository.NotifyTasksChanged();
    }

    public void Undo()
    {
        // Restore the previous project state
        _project.Name = _previousProjectState.Name;
        _project.Author = _previousProjectState.Author;
        _project.TaskItems.Clear();
        _project.TaskItems.AddRange(_previousProjectState.TaskItems);

        // Restore the previous tasks in the repository
        var currentTasks = _taskRepository.GetTasks();
        currentTasks.Clear();
        foreach (var task in _previousTasks)
        {
            currentTasks.Add(task);
        }
        _taskRepository.NotifyTasksChanged();
    }
}