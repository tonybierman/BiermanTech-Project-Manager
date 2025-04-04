using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BiermanTech.ProjectManager.Commands;

public class LoadProjectCommand : ICommand
{
    private readonly ProjectDbContext _context;
    private readonly int _projectId;
    private readonly ITaskRepository _taskRepository;
    private Project _previousProjectState;
    private Project _loadedProject;

    public LoadProjectCommand(ProjectDbContext context, int projectId, ITaskRepository taskRepository)
    {
        _context = context;
        _projectId = projectId;
        _taskRepository = taskRepository;
    }

    public Project LoadedProject => _loadedProject;

    public void Execute()
    {
        _loadedProject = _context.Projects
            .Include(p => p.Tasks).ThenInclude(t => t.Children)
            .Include(p => p.Tasks).ThenInclude(t => t.TaskDependencies)
            .ThenInclude(td => td.DependsOnTask)
            .Include(p => p.Narrative)
            .FirstOrDefault(p => p.Id == _projectId);

        if (_loadedProject == null)
        {
            throw new InvalidOperationException($"Project with ID {_projectId} not found.");
        }

        // Update the repository with the new project's tasks
        _taskRepository.ClearTasks();
        if (_loadedProject.Tasks != null)
        {
            foreach (var task in _loadedProject.Tasks)
            {
                task.DependsOn = task.TaskDependencies?
                    .Select(td => td.DependsOnTask)
                    .Where(t => t != null)
                    .ToList() ?? new List<TaskItem>();

                _taskRepository.AddTask(task);
            }
        }
    }

    public void Undo()
    {
        // TODO: Implement Undo
        // For now, we'll just clear the tasks and project
        _taskRepository.ClearTasks();
        _loadedProject = null;
    }
}