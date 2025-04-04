using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace BiermanTech.ProjectManager.Commands;

public class LoadProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ITaskRepository _taskRepository;
    private readonly TaskFileService _taskFileService;
    private readonly string _filePath;
    private Project _previousProjectState;
    private List<TaskItem> _previousTasks;

    public LoadProjectCommand(Project project, ITaskRepository taskRepository, TaskFileService taskFileService, string filePath)
    {
        _project = project;
        _taskRepository = taskRepository;
        _taskFileService = taskFileService;
        _filePath = filePath;

        // Store the previous state for undo (deep copy)
        _previousProjectState = DeepCopyProject(project);
        _previousTasks = DeepCopyTaskList(_taskRepository.GetTasks());
    }

    public void Execute()
    {
        // Load the project from the specified file
        var loadedProject = Task.Run(() => _taskFileService.LoadProjectAsync(_filePath)).GetAwaiter().GetResult();

        // Update the current project with the loaded data
        _project.Name = loadedProject.Name;
        _project.Author = loadedProject.Author;
        _project.Tasks.Clear();
        _project.Tasks.AddRange(loadedProject.Tasks);

        // Update the repository with the loaded tasks
        var currentTasks = _taskRepository.GetTasks();
        currentTasks.Clear();
        AddTasksRecursively(currentTasks, loadedProject.Tasks);
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

    // Helper method to add tasks and their children to the repository
    private void AddTasksRecursively(IList<TaskItem> target, IEnumerable<TaskItem> tasks)
    {
        foreach (var task in tasks)
        {
            target.Add(task);
            if (task.Children.Any())
            {
                AddTasksRecursively(target, task.Children);
            }
        }
    }
}