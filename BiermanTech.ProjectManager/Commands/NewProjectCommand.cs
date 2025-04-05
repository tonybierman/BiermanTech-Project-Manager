using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using DynamicData;

namespace BiermanTech.ProjectManager.Commands;

public class NewProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectDbContext _context;
    private Project _previousProjectState;
    private ObservableCollection<TaskItem> _previousTasks;

    public NewProjectCommand(Project project, ProjectDbContext context)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _context = context ?? throw new ArgumentNullException(nameof(context));

        // Store the previous state from the database
        var dbProject = _context.Projects
            .Include(p => p.Tasks)
                .ThenInclude(t => t.TaskDependencies)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Children)
            .Include(p => p.Narrative)
            .AsNoTracking() // Avoid tracking for previous state
            .FirstOrDefault(p => p.Id == project.Id);
        if (dbProject != null)
        {
            _previousProjectState = DeepCopyProject(dbProject);
            _previousTasks = DeepCopyTaskList(dbProject.Tasks);
        }
    }

    public void Execute()
    {
        // Clear existing tasks for this project in the database
        var existingTasks = _context.Tasks.Where(t => t.ProjectId == _project.Id).ToList();
        foreach (var task in existingTasks)
        {
            // Remove associated dependencies first
            var dependencies = _context.TaskDependencies.Where(td => td.TaskId == task.Id || td.DependsOnId == task.Id).ToList();
            _context.TaskDependencies.RemoveRange(dependencies);
            _context.Tasks.Remove(task);
        }

        // Update project properties
        var projectToUpdate = _context.Projects.FirstOrDefault(p => p.Id == _project.Id);
        if (projectToUpdate != null)
        {
            projectToUpdate.Name = "New Project";
            projectToUpdate.Author = "Unknown";
            projectToUpdate.Narrative = null; // Optionally reset narrative
        }
        else
        {
            // If project doesn’t exist, create a new one
            _project.Name = "New Project";
            _project.Author = "Unknown";
            _project.Tasks.Clear(); // Ensure no tasks
            _context.Projects.Add(_project);
        }

        // Save changes
        _context.SaveChanges();

        // Update in-memory project to match database state
        _project.Name = "New Project";
        _project.Author = "Unknown";
        _project.Tasks.Clear();
    }

    public void Undo()
    {
        // Remove current tasks (should be none, but ensure consistency)
        var currentTasks = _context.Tasks.Where(t => t.ProjectId == _project.Id).ToList();
        _context.Tasks.RemoveRange(currentTasks);

        // Restore previous project state
        var projectToRevert = _context.Projects.FirstOrDefault(p => p.Id == _project.Id);
        if (projectToRevert != null)
        {
            projectToRevert.Name = _previousProjectState.Name;
            projectToRevert.Author = _previousProjectState.Author;
            if (_previousProjectState.Narrative != null)
            {
                projectToRevert.Narrative = new ProjectNarrative
                {
                    Situation = _previousProjectState.Narrative.Situation,
                    CurrentState = _previousProjectState.Narrative.CurrentState,
                    Plan = _previousProjectState.Narrative.Plan,
                    Results = _previousProjectState.Narrative.Results
                };
            }
        }

        // Restore previous tasks
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
                TaskDependencies = task.TaskDependencies.Select(td => new TaskDependency
                {
                    TaskId = td.TaskId,
                    DependsOnId = td.DependsOnId
                }).ToList(),
                Children = DeepCopyTaskList(task.Children)
            };
            copy.Add(newTask);
        }
        return copy;
    }
}