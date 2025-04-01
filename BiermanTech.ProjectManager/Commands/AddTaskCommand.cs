using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;

namespace BiermanTech.ProjectManager.Commands;

public class AddTaskCommand : ICommand
{
    private readonly TaskItem _task;
    private readonly ITaskRepository _taskRepository;

    public AddTaskCommand(TaskItem task, ITaskRepository taskRepository)
    {
        _task = task;
        _taskRepository = taskRepository;
    }

    public void Execute()
    {
        _taskRepository.AddTask(_task);
    }

    public void Undo()
    {
        _taskRepository.RemoveTask(_task);
    }
}