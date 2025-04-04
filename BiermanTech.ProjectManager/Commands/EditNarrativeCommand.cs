using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace BiermanTech.ProjectManager.Commands;

public class EditNarrativeCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectDbContext _context;
    private readonly ProjectNarrative _updatedNarrative;
    private ProjectNarrative _originalNarrative;

    public EditNarrativeCommand(Project project, ProjectNarrative originalNarrative, ProjectNarrative updatedNarrative, ProjectDbContext context)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _updatedNarrative = updatedNarrative ?? throw new ArgumentNullException(nameof(updatedNarrative));

        // Store original state from database or input
        _originalNarrative = originalNarrative != null
            ? new ProjectNarrative
            {
                Situation = originalNarrative.Situation,
                CurrentState = originalNarrative.CurrentState,
                Plan = originalNarrative.Plan,
                Results = originalNarrative.Results
            }
            : null;
    }

    public void Execute()
    {
        // Update the project's narrative in the database
        var projectToUpdate = _context.Projects
            .FirstOrDefault(p => p.Id == _project.Id);
        if (projectToUpdate == null)
        {
            throw new InvalidOperationException($"Project with ID {_project.Id} not found.");
        }

        projectToUpdate.Narrative = _updatedNarrative != null
            ? new ProjectNarrative
            {
                Situation = _updatedNarrative.Situation,
                CurrentState = _updatedNarrative.CurrentState,
                Plan = _updatedNarrative.Plan,
                Results = _updatedNarrative.Results
            }
            : null;

        _context.SaveChanges();

        // Update in-memory project to reflect the change
        _project.Narrative = projectToUpdate.Narrative;
    }

    public void Undo()
    {
        // Restore the previous narrative in the database
        var projectToRevert = _context.Projects
            .FirstOrDefault(p => p.Id == _project.Id);
        if (projectToRevert != null)
        {
            projectToRevert.Narrative = _originalNarrative != null
                ? new ProjectNarrative
                {
                    Situation = _originalNarrative.Situation,
                    CurrentState = _originalNarrative.CurrentState,
                    Plan = _originalNarrative.Plan,
                    Results = _originalNarrative.Results
                }
                : null;

            _context.SaveChanges();

            // Update in-memory project
            _project.Narrative = projectToRevert.Narrative;
        }
    }
}