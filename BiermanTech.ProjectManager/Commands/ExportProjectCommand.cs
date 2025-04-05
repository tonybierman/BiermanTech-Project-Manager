using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Commands;

public class ExportProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectDbContext _context;
    private readonly TaskFileService _taskFileService;
    private readonly string _filePath;
    private Project _previousProjectState;
    private ObservableCollection<TaskItem> _previousTasks;

    public ExportProjectCommand(Project project, ProjectDbContext context, TaskFileService taskFileService, string filePath)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _taskFileService = taskFileService ?? throw new ArgumentNullException(nameof(taskFileService));
        _filePath = filePath;

        var dbProject = _context.Projects
            .Include(p => p.Tasks)
                .ThenInclude(t => t.TaskDependencies)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Children)
            .Include(p => p.Narrative)
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == project.Id);
        if (dbProject != null)
        {
            _previousProjectState = DeepCopyProject(dbProject);
            _previousTasks = DeepCopyTaskList(dbProject.Tasks);
        }
    }

    public void Execute()
    {
        // Load current project state from database
        var dbProject = _context.Projects
            .Include(p => p.Tasks)
                .ThenInclude(t => t.TaskDependencies)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Children)
            .Include(p => p.Narrative)
            .FirstOrDefault(p => p.Id == _project.Id);
        if (dbProject == null)
        {
            throw new InvalidOperationException("Project not found in database for export.");
        }

        // Create a new Project instance for export to avoid modifying tracked entities
        var exportProject = new Project
        {
            Id = dbProject.Id,
            Name = dbProject.Name,
            Author = dbProject.Author,
            Narrative = dbProject.Narrative != null ? new ProjectNarrative
            {
                Situation = dbProject.Narrative.Situation,
                CurrentState = dbProject.Narrative.CurrentState,
                Plan = dbProject.Narrative.Plan,
                Results = dbProject.Narrative.Results
            } : null,
            Tasks = new ObservableCollection<TaskItem>()
        };

        // Populate exportProject.Tasks with export-ready TaskItems
        foreach (var task in dbProject.Tasks)
        {
            var exportTask = new TaskItem
            {
                Id = task.Id,
                Name = task.Name,
                StartDate = task.StartDate,
                Duration = task.Duration,
                PercentComplete = task.PercentComplete,
                ProjectId = task.ProjectId,
                ParentId = task.ParentId,
                TaskDependencies = task.TaskDependencies.Select(td => new TaskDependency
                {
                    TaskId = td.TaskId,
                    DependsOnId = td.DependsOnId
                }).ToList(),
                Children = new ObservableCollection<TaskItem>(task.Children) // Shallow copy; children included via Include
            };
            exportProject.Tasks.Add(exportTask);
        }

        // Export to JSON file
        Task.Run(() => _taskFileService.SaveProjectAsync(exportProject, _filePath)).GetAwaiter().GetResult();

        // Update in-memory project (optional, depending on your intent)
        _project.Name = exportProject.Name;
        _project.Author = exportProject.Author;
        _project.Narrative = exportProject.Narrative;
        _project.Tasks.Clear();
        foreach (var task in exportProject.Tasks)
        {
            _project.Tasks.Add(task);
        }
    }

    public void Undo()
    {
        // Clear current tasks and dependencies from database
        var currentTasks = _context.Tasks.Where(t => t.ProjectId == _project.Id).ToList();
        foreach (var task in currentTasks)
        {
            var dependencies = _context.TaskDependencies.Where(td => td.TaskId == task.Id || td.DependsOnId == task.Id).ToList();
            _context.TaskDependencies.RemoveRange(dependencies);
            _context.Tasks.Remove(task);
        }

        // Restore previous project state
        var projectToRevert = _context.Projects.FirstOrDefault(p => p.Id == _project.Id);
        if (projectToRevert != null)
        {
            projectToRevert.Name = _previousProjectState.Name;
            projectToRevert.Author = _previousProjectState.Author;
            projectToRevert.Narrative = _previousProjectState.Narrative != null ? new ProjectNarrative
            {
                Situation = _previousProjectState.Narrative.Situation,
                CurrentState = _previousProjectState.Narrative.CurrentState,
                Plan = _previousProjectState.Narrative.Plan,
                Results = _previousProjectState.Narrative.Results
            } : null;
        }

        // Restore previous tasks and dependencies
        foreach (var task in _previousTasks)
        {
            _context.Tasks.Add(task);
            foreach (var dependency in task.TaskDependencies)
            {
                _context.TaskDependencies.Add(new TaskDependency
                {
                    TaskId = dependency.TaskId,
                    DependsOnId = dependency.DependsOnId
                });
            }
        }

        // Save changes
        _context.SaveChanges();

        // Update in-memory project
        _project.Name = _previousProjectState.Name;
        _project.Author = _previousProjectState.Author;
        _project.Tasks.Clear();
        foreach (var task in _previousTasks)
        {
            _project.Tasks.Add(task);
        }
    }

    private Project DeepCopyProject(Project source)
    {
        return new Project
        {
            Id = source.Id,
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

    private ObservableCollection<TaskItem> DeepCopyTaskList(IEnumerable<TaskItem> source)
    {
        var copy = new ObservableCollection<TaskItem>();
        foreach (var task in source)
        {
            var newTask = new TaskItem
            {
                Id = task.Id,
                Name = task.Name,
                StartDate = task.StartDate,
                Duration = task.Duration,
                PercentComplete = task.PercentComplete,
                ProjectId = task.ProjectId,
                ParentId = task.ParentId,
                TaskDependencies = task.TaskDependencies?.Select(td => new TaskDependency
                {
                    TaskId = td.TaskId,
                    DependsOnId = td.DependsOnId
                }).ToList() ?? new List<TaskDependency>(),
                Children = DeepCopyTaskList(task.Children)
            };
            copy.Add(newTask);
        }
        return copy;
    }
}