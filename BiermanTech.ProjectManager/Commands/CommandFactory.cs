using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using System;
using BiermanTech.ProjectManager.Services;

namespace BiermanTech.ProjectManager.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly ProjectDbContext _context;
    private readonly int _projectId;
    private readonly TaskFileService _taskFileService;
    private readonly ITaskRepository _taskRepository;

    public CommandFactory(ProjectDbContext context, int projectId, TaskFileService taskFileService, ITaskRepository taskRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _projectId = projectId;
        _taskFileService = taskFileService ?? throw new ArgumentNullException(nameof(taskFileService));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
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

    public ICommand CreateLoadProjectCommand(int projectId)
    {
        return new LoadProjectCommand(_context, projectId, _taskRepository);
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