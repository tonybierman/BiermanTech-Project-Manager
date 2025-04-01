using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class UpdateTaskCommand : ICommand
{
    private readonly TaskItem _originalTask;
    private readonly TaskItem _updatedTask;
    private readonly ITaskRepository _taskRepository;
    private TaskItem _previousState;

    public UpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask, ITaskRepository taskRepository)
    {
        _originalTask = originalTask;
        _updatedTask = updatedTask;
        _taskRepository = taskRepository;
        // Store the original state for undo
        _previousState = new TaskItem
        {
            Id = originalTask.Id,
            Name = originalTask.Name,
            StartDate = originalTask.StartDate,
            Duration = originalTask.Duration,
            PercentComplete = originalTask.PercentComplete,
            DependsOn = originalTask.DependsOn
        };
    }

    public void Execute()
    {
        var tasks = _taskRepository.GetTasks();
        var taskToUpdate = tasks.FirstOrDefault(t => t.Id == _originalTask.Id);
        if (taskToUpdate != null)
        {
            // Update properties instead of replacing the item
            taskToUpdate.Name = _updatedTask.Name;
            taskToUpdate.StartDate = _updatedTask.StartDate;
            taskToUpdate.Duration = _updatedTask.Duration;
            taskToUpdate.PercentComplete = _updatedTask.PercentComplete;
            taskToUpdate.DependsOn = _updatedTask.DependsOn;
        }
    }

    public void Undo()
    {
        var tasks = _taskRepository.GetTasks();
        var taskToRevert = tasks.FirstOrDefault(t => t.Id == _originalTask.Id);
        if (taskToRevert != null)
        {
            // Revert to the previous state
            taskToRevert.Name = _previousState.Name;
            taskToRevert.StartDate = _previousState.StartDate;
            taskToRevert.Duration = _previousState.Duration;
            taskToRevert.PercentComplete = _previousState.PercentComplete;
            taskToRevert.DependsOn = _previousState.DependsOn;
        }
    }
}