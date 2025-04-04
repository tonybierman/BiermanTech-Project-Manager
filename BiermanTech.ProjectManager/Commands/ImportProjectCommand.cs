using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.Commands;

public class ImportProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectDbContext _context;
    private readonly TaskFileService _taskFileService;
    private readonly string _filePath;
    private Project _previousProjectState;
    private List<TaskItem> _previousTasks;

    public ImportProjectCommand(Project project, ProjectDbContext context, TaskFileService taskFileService, string filePath)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _taskFileService = taskFileService ?? throw new ArgumentNullException(nameof(taskFileService));
        _filePath = filePath;

        // Store the previous state from the database
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
        // Load the project from JSON file
        var loadedProject = Task.Run(() => _taskFileService.LoadProjectAsync(_filePath)).GetAwaiter().GetResult();

        // Clear existing tasks and dependencies for this project in the database
        var existingTasks = _context.Tasks.Where(t => t.ProjectId == _project.Id).ToList();
        foreach (var task in existingTasks)
        {
            var dependencies = _context.TaskDependencies.Where(td => td.TaskId == task.Id || td.DependsOnId == task.Id).ToList();
            _context.TaskDependencies.RemoveRange(dependencies);
            _context.Tasks.Remove(task);
        }

        // Update project properties
        var projectToUpdate = _context.Projects.FirstOrDefault(p => p.Id == _project.Id);
        if (projectToUpdate != null)
        {
            projectToUpdate.Name = loadedProject.Name;
            projectToUpdate.Author = loadedProject.Author;
            projectToUpdate.Narrative = loadedProject.Narrative != null ? new ProjectNarrative
            {
                Situation = loadedProject.Narrative.Situation,
                CurrentState = loadedProject.Narrative.CurrentState,
                Plan = loadedProject.Narrative.Plan,
                Results = loadedProject.Narrative.Results
            } : null;
        }
        else
        {
            throw new InvalidOperationException("Project not found in database for import.");
        }

        // Import tasks and dependencies
        var taskIdMap = new Dictionary<int, int>(); // Map old IDs to new IDs
        foreach (var task in FlattenTasks(loadedProject.Tasks))
        {
            var newTask = new TaskItem
            {
                Name = task.Name,
                StartDate = task.StartDate,
                Duration = task.Duration,
                PercentComplete = task.PercentComplete,
                ProjectId = _project.Id,
                ParentId = task.ParentId.HasValue ? taskIdMap[task.ParentId.Value] : null
            };
            _context.Tasks.Add(newTask);
            _context.SaveChanges(); // Save to get new ID
            taskIdMap[task.Id] = newTask.Id;

            // Add dependencies
            if (task.DependsOnIds != null)
            {
                foreach (var oldDependsOnId in task.DependsOnIds)
                {
                    if (taskIdMap.TryGetValue(oldDependsOnId, out int newDependsOnId))
                    {
                        _context.TaskDependencies.Add(new TaskDependency
                        {
                            TaskId = newTask.Id,
                            DependsOnId = newDependsOnId
                        });
                    }
                }
            }
        }

        // Save all changes
        _context.SaveChanges();

        // Update in-memory project
        _project.Name = loadedProject.Name;
        _project.Author = loadedProject.Author;
        _project.Tasks.Clear();
        _project.Tasks.AddRange(_context.Tasks.Where(t => t.ProjectId == _project.Id).ToList());
    }

    public void Undo()
    {
        // Clear current tasks and dependencies
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
        _project.Tasks.AddRange(_previousTasks);
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

    private List<TaskItem> DeepCopyTaskList(IEnumerable<TaskItem> source)
    {
        var copy = new List<TaskItem>();
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

    private IEnumerable<TaskItem> FlattenTasks(IEnumerable<TaskItem> tasks)
    {
        foreach (var task in tasks)
        {
            yield return task;
            foreach (var child in FlattenTasks(task.Children))
            {
                yield return child;
            }
        }
    }
}