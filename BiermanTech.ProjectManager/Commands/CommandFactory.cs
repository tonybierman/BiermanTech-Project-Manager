using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using BiermanTech.ProjectManager.Data;
using System;

namespace BiermanTech.ProjectManager.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly ProjectDbContext _context;
    private readonly int _projectId;
    private readonly TaskFileService _taskFileService;

    public CommandFactory(ProjectDbContext context, int projectId, TaskFileService taskFileService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _projectId = projectId;
        _taskFileService = taskFileService ?? throw new ArgumentNullException(nameof(taskFileService));
    }

    public ICommand CreateAddTaskCommand(TaskItem task, int? parentTaskId = null)
    {
        return new AddTaskCommand(task, _context, parentTaskId);
    }

    public ICommand CreateUpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask)
    {
        return new UpdateTaskCommand(originalTask, updatedTask, _context);
    }

    public ICommand CreateDeleteTaskCommand(TaskItem task)
    {
        return new DeleteTaskCommand(task, _context);
    }

    public ICommand CreateExportProjectCommand(Project project, string filePath)
    {
        return new ExportProjectCommand(project, _context, _taskFileService, filePath);
    }

    public ICommand CreateImportProjectCommand(Project project, string filePath)
    {
        return new ImportProjectCommand(project, _context, _taskFileService, filePath);
    }

    public ICommand CreateSaveProjectCommand(Project project)
    {
        return new SaveProjectCommand(project, _context);
    }

    public ICommand CreateLoadProjectCommand(Project project, int projectId)
    {
        return new LoadProjectCommand(project, _context, projectId);
    }

    public ICommand CreateNewProjectCommand(Project project)
    {
        return new NewProjectCommand(project, _context);
    }

    public ICommand CreateEditNarrativeCommand(Project project, ProjectNarrative originalNarrative, ProjectNarrative updatedNarrative)
    {
        return new EditNarrativeCommand(project, originalNarrative, updatedNarrative, _context);
    }
}