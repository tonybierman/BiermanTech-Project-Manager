using BiermanTech.ProjectManager.Models;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace BiermanTech.ProjectManager.ViewModels;

public class NarrativeDialogViewModel : ViewModelBase
{
    private readonly ProjectNarrative _narrative;
    private string _situation;
    private string _currentState;
    private string _plan;
    private string _results;
    private string _errorMessage;

    public string Situation
    {
        get => _situation;
        set => this.RaiseAndSetIfChanged(ref _situation, value);
    }

    public string CurrentState
    {
        get => _currentState;
        set => this.RaiseAndSetIfChanged(ref _currentState, value);
    }

    public string Plan
    {
        get => _plan;
        set => this.RaiseAndSetIfChanged(ref _plan, value);
    }

    public string Results
    {
        get => _results;
        set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, ProjectNarrative> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private ProjectNarrative _result;
    private bool _canSave;

    public NarrativeDialogViewModel(ProjectNarrative narrative)
    {
        _narrative = narrative;

        // Initialize properties from the existing narrative
        Situation = narrative.Situation ?? string.Empty;
        CurrentState = narrative.CurrentState ?? string.Empty;
        Plan = narrative.Plan ?? string.Empty;
        Results = narrative.Results ?? string.Empty;

        // Initialize CanSave based on initial validation
        Validate();

        // Define SaveCommand with a CanExecute condition
        SaveCommand = ReactiveCommand.Create<Unit, ProjectNarrative>(
            _ =>
            {
                _result = new ProjectNarrative
                {
                    Situation = Situation,
                    CurrentState = CurrentState,
                    Plan = Plan,
                    Results = Results
                };
                return _result;
            },
            this.WhenAnyValue(
                x => x.Situation,
                x => x.CurrentState,
                x => x.Plan,
                x => x.Results,
                (situation, currentState, plan, results) =>
                    !string.IsNullOrWhiteSpace(situation) &&
                    !string.IsNullOrWhiteSpace(currentState) &&
                    !string.IsNullOrWhiteSpace(plan) &&
                    !string.IsNullOrWhiteSpace(results) &&
                    IsValid()
            )
        );

        CancelCommand = ReactiveCommand.Create(() =>
        {
            _result = null;
        });
    }

    public ProjectNarrative GetResult()
    {
        return _result;
    }

    private void Validate()
    {
        ErrorMessage = null;
        _canSave = true;

        if (string.IsNullOrWhiteSpace(Situation))
        {
            ErrorMessage = "Situation is required.";
            _canSave = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentState))
        {
            ErrorMessage = "Current State is required.";
            _canSave = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(Plan))
        {
            ErrorMessage = "Plan is required.";
            _canSave = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(Results))
        {
            ErrorMessage = "Results is required.";
            _canSave = false;
            return;
        }
    }

    private bool IsValid()
    {
        Validate();
        return _canSave;
    }
}