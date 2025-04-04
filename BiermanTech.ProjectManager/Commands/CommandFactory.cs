using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly ITaskRepository _taskRepository;
    private readonly TaskFileService _taskFileService;

    public CommandFactory(ITaskRepository taskRepository, TaskFileService taskFileService)
    {
        _taskRepository = taskRepository;
        _taskFileService = taskFileService;
    }

    public ICommand CreateAddTaskCommand(TaskItem task, int? parentTaskId = null) // Changed from Guid? to int?
    {
        return new AddTaskCommand(task, _taskRepository, parentTaskId);
    }

    public ICommand CreateUpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask)
    {
        return new UpdateTaskCommand(originalTask, updatedTask, _taskRepository);
    }

    public ICommand CreateDeleteTaskCommand(TaskItem task)
    {
        return new DeleteTaskCommand(task, _taskRepository);
    }

    public ICommand CreateSaveProjectCommand(Project project, string filePath)
    {
        return new SaveProjectCommand(project, _taskRepository, _taskFileService, filePath);
    }

    public ICommand CreateLoadProjectCommand(Project project, string filePath)
    {
        return new LoadProjectCommand(project, _taskRepository, _taskFileService, filePath);
    }

    public ICommand CreateNewProjectCommand(Project project)
    {
        return new NewProjectCommand(project, _taskRepository);
    }

    public ICommand CreateEditNarrativeCommand(Project project, ProjectNarrative originalNarrative, ProjectNarrative updatedNarrative)
    {
        return new EditNarrativeCommand(project, originalNarrative, updatedNarrative);
    }
}