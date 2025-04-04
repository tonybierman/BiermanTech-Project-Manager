using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // Store the previous state for undo (deep copy)
        _previousProjectState = DeepCopyProject(project);
        _previousTasks = DeepCopyTaskList(_taskRepository.GetTasks());
    }

    public void Execute()
    {
        // Clear the current project and tasks
        _project.Name = "New Project";
        _project.Author = "Unknown";
        _project.Tasks.Clear();

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
        _project.Tasks.Clear();
        _project.Tasks.AddRange(_previousProjectState.Tasks);

        // Restore the previous tasks in the repository
        var currentTasks = _taskRepository.GetTasks();
        currentTasks.Clear();
        foreach (var task in _previousTasks)
        {
            currentTasks.Add(task);
        }
        _taskRepository.NotifyTasksChanged();
    }

    // Helper method to deep copy a Project
    private Project DeepCopyProject(Project source)
    {
        return new Project
        {
            Name = source.Name,
            Author = source.Author,
            Tasks = DeepCopyTaskList(source.Tasks),
            Narrative = source.Narrative != null ? new ProjectNarrative
            {
                Situation = source.Narrative.Situation,
                CurrentState = source.Narrative.CurrentState,
                Plan = source.Narrative.Plan,
                Results = source.Narrative.Results
            } : null
        };
    }

    // Helper method to deep copy a list of TaskItems, including children
    private List<TaskItem> DeepCopyTaskList(IEnumerable<TaskItem> source)
    {
        var copy = new List<TaskItem>();
        foreach (var task in source)
        {
            var newTask = new TaskItem
            {
                Id = task.Id,
                Name = task.Name,
                StartDate = task.StartDate,
                Duration = task.Duration,
                PercentComplete = task.PercentComplete,
                DependsOnIds = new List<int>(task.DependsOnIds), // Changed from List<Guid> to List<int>
                DependsOn = new List<TaskItem>(), // Will be resolved later if needed
                Children = DeepCopyTaskList(task.Children)
            };
            copy.Add(newTask);
        }
        return copy;
    }
}