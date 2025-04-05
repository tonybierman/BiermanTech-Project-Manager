using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Commands;

public class SaveProjectCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectDbContext _context;
    private Project _previousProjectState;

    public SaveProjectCommand(Project project, ProjectDbContext context)
    {
        _project = project;
        _context = context;
        _previousProjectState = DeepCopyProject(project);
    }

    public void Execute()
    {
        if (_project.Id == 0) // New project
        {
            _context.Projects.Add(_project);
        }
        else // Update existing
        {
            _context.Projects.Update(_project);
        }
        _context.SaveChanges();
    }

    public void Undo()
    {
        if (_project.Id == 0) // Was new
        {
            _context.Projects.Remove(_project);
        }
        else // Restore previous state
        {
            _context.Entry(_project).CurrentValues.SetValues(_previousProjectState);
            foreach (var task in _project.Tasks)
            {
                if (!_previousProjectState.Tasks.Any(t => t.Id == task.Id))
                {
                    _context.Tasks.Remove(task);
                }
            }
            foreach (var task in _previousProjectState.Tasks)
            {
                if (!_project.Tasks.Any(t => t.Id == task.Id))
                {
                    _context.Tasks.Add(task);
                }
            }
        }
        _context.SaveChanges();
    }

    private Project DeepCopyProject(Project source)
    {
        return new Project
        {
            Id = source.Id,
            Name = source.Name,
            Author = source.Author,
            Tasks = new ObservableCollection<TaskItem>(source.Tasks.Select(t => new TaskItem
            {
                Id = t.Id,
                Name = t.Name,
                StartDate = t.StartDate,
                Duration = t.Duration,
                PercentComplete = t.PercentComplete,
                ParentId = t.ParentId,
                ProjectId = t.ProjectId,
                Children = new ObservableCollection<TaskItem>(t.Children),
                TaskDependencies = new List<TaskDependency>(t.TaskDependencies)
            })),
            Narrative = source.Narrative != null ? new ProjectNarrative
            {
                Id = source.Narrative.Id,
                Situation = source.Narrative.Situation,
                CurrentState = source.Narrative.CurrentState,
                Plan = source.Narrative.Plan,
                Results = source.Narrative.Results
            } : null,
            ProjectNarrativeId = source.ProjectNarrativeId
        };
    }
}