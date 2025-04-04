using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class LoadProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectDbContext _context;
    private readonly int _projectId;
    private Project _previousProjectState;

    public LoadProjectCommand(Project project, ProjectDbContext context, int projectId)
    {
        _project = project;
        _context = context;
        _projectId = projectId;
        //_previousProjectState = DeepCopyProject(project);
    }

    public void Execute()
    {
        var loadedProject = _context.Projects
            .Include(p => p.Tasks).ThenInclude(t => t.Children)
            .Include(p => p.Tasks).ThenInclude(t => t.TaskDependencies)
            .Include(p => p.Narrative)
            .FirstOrDefault(p => p.Id == _projectId);
        if (loadedProject != null)
        {
            _project.Id = loadedProject.Id;
            _project.Name = loadedProject.Name;
            _project.Author = loadedProject.Author;
            _project.Tasks.Clear();
            _project.Tasks.AddRange(loadedProject.Tasks);
            _project.Narrative = loadedProject.Narrative;
            _project.ProjectNarrativeId = loadedProject.ProjectNarrativeId;

            // Populate DependsOn for runtime use
            foreach (var task in _project.Tasks)
            {
                task.DependsOn = _context.Tasks
                    .Where(t => task.TaskDependencies.Select(td => td.DependsOnId).Contains(t.Id))
                    .ToList();
            }
        }
    }

    public void Undo()
    {
        // TODO: Implement Undo

        //_project.Id = _previousProjectState.Id;
        //_project.Name = _previousProjectState.Name;
        //_project.Author = _previousProjectState.Author;
        //_project.Tasks.Clear();
        //_project.Tasks.AddRange(_previousProjectState.Tasks);
        //_project.Narrative = _previousProjectState.Narrative;
        //_project.ProjectNarrativeId = _previousProjectState.ProjectNarrativeId;
    }
}