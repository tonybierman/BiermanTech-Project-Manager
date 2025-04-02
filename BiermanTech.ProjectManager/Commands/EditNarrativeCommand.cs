using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.Commands;

public class EditNarrativeCommand : ICommand
{
    private readonly Project _project;
    private readonly ProjectNarrative _originalNarrative;
    private readonly ProjectNarrative _updatedNarrative;

    public EditNarrativeCommand(Project project, ProjectNarrative originalNarrative, ProjectNarrative updatedNarrative)
    {
        _project = project;
        _originalNarrative = new ProjectNarrative
        {
            Situation = originalNarrative.Situation,
            CurrentState = originalNarrative.CurrentState,
            Plan = originalNarrative.Plan,
            Results = originalNarrative.Results
        };
        _updatedNarrative = new ProjectNarrative
        {
            Situation = updatedNarrative.Situation,
            CurrentState = updatedNarrative.CurrentState,
            Plan = updatedNarrative.Plan,
            Results = updatedNarrative.Results
        };
    }

    public void Execute()
    {
        _project.Narrative = new ProjectNarrative
        {
            Situation = _updatedNarrative.Situation,
            CurrentState = _updatedNarrative.CurrentState,
            Plan = _updatedNarrative.Plan,
            Results = _updatedNarrative.Results
        };
    }

    public void Undo()
    {
        _project.Narrative = new ProjectNarrative
        {
            Situation = _originalNarrative.Situation,
            CurrentState = _originalNarrative.CurrentState,
            Plan = _originalNarrative.Plan,
            Results = _originalNarrative.Results
        };
    }
}