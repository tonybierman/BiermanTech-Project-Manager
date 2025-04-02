using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        // Load the project from the specified file
        var loadedProject = Task.Run(() => _taskFileService.LoadProjectAsync(_filePath)).GetAwaiter().GetResult();

        // Update the current project with the loaded data
        _project.Name = loadedProject.Name;
        _project.Author = loadedProject.Author;
        _project.TaskItems.Clear();
        _project.TaskItems.AddRange(loadedProject.TaskItems);

        // Update the repository with the loaded tasks
        var currentTasks = _taskRepository.GetTasks();
        currentTasks.Clear();
        foreach (var task in _project.TaskItems)
        {
            currentTasks.Add(task);
        }
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