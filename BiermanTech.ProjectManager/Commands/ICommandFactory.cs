using BiermanTech.ProjectManager.Models;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Commands;

public interface ICommandFactory
{
    ICommand CreateAddTaskCommand(TaskItem task);
    ICommand CreateUpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask);
    ICommand CreateDeleteTaskCommand(TaskItem task, int index, List<TaskItem> dependentTasks);
    ICommand CreateSaveProjectCommand(Project project, string filePath);
    ICommand CreateLoadProjectCommand(Project project, string filePath);
    ICommand CreateNewProjectCommand(Project project);
}