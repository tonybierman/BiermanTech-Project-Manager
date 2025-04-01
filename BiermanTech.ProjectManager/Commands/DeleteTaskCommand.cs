using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Commands;

public class DeleteTaskCommand : ICommand
{
    private readonly TaskItem _task;
    private readonly int _index;
    private readonly List<TaskItem> _dependentTasks;
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskCommand(TaskItem task, int index, List<TaskItem> dependentTasks, ITaskRepository taskRepository)
    {
        _task = task;
        _index = index;
        _dependentTasks = dependentTasks;
        _taskRepository = taskRepository;
    }

    public void Execute()
    {
        var tasks = _taskRepository.GetTasks();
        tasks.Remove(_task);

        foreach (var dependentTask in _dependentTasks)
        {
            dependentTask.DependsOn = null;
        }

        _taskRepository.NotifyTasksChanged(); // Call the method instead of invoking the event
    }

    public void Undo()
    {
        var tasks = _taskRepository.GetTasks();
        tasks.Insert(_index, _task);

        foreach (var dependentTask in _dependentTasks)
        {
            dependentTask.DependsOn = _task;
        }

        _taskRepository.NotifyTasksChanged(); // Call the method instead of invoking the event
    }
}