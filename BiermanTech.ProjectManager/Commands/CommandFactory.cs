using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly ITaskRepository _taskRepository;

    public CommandFactory(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public ICommand CreateAddTaskCommand(TaskItem task)
    {
        return new AddTaskCommand(task, _taskRepository);
    }

    public ICommand CreateUpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask)
    {
        return new UpdateTaskCommand(originalTask, updatedTask, _taskRepository);
    }

    public ICommand CreateDeleteTaskCommand(TaskItem task, int index, ObservableCollection<TaskItem> dependentTasks)
    {
        return new DeleteTaskCommand(task, index, dependentTasks, _taskRepository);
    }
}