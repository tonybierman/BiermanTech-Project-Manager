using Avalonia.Controls;
using BiermanTech.ProjectManager.Commands;
using BiermanTech.ProjectManager.Models;
using ReactiveUI;
using Serilog;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace BiermanTech.ProjectManager.ViewModels;

public class MenuBarViewModel : ViewModelBase
{
    private readonly CommandManager _commandManager;
    private string _projectName;
    private string _projectAuthor;
    private bool _canUndo;
    private bool _canRedo;
    private bool _canEditNarrative;
    private bool _canUpdateTask;

    public string ProjectName
    {
        get => _projectName;
        set => this.RaiseAndSetIfChanged(ref _projectName, value);
    }

    public string ProjectAuthor
    {
        get => _projectAuthor;
        set => this.RaiseAndSetIfChanged(ref _projectAuthor, value);
    }

    public TaskItem SelectedTask => _commandManager.SelectedTask;

    public bool CanUndo
    {
        get => _canUndo;
        set => this.RaiseAndSetIfChanged(ref _canUndo, value);
    }

    public bool CanRedo
    {
        get => _canRedo;
        set => this.RaiseAndSetIfChanged(ref _canRedo, value);
    }

    public bool CanEditNarrative
    {
        get => _canEditNarrative;
        set => this.RaiseAndSetIfChanged(ref _canEditNarrative, value);
    }

    public bool CanUpdateTask
    {
        get => _canUpdateTask;
        set => this.RaiseAndSetIfChanged(ref _canUpdateTask, value);
    }

    public ReactiveCommand<Unit, Unit> NewProjectCommand => _commandManager.NewProjectCommand;
    public ReactiveCommand<Unit, Unit> SaveProjectCommand => _commandManager.SaveProjectCommand;
    public ReactiveCommand<Unit, Unit> SaveAsProjectCommand => _commandManager.SaveAsProjectCommand;
    public ReactiveCommand<int, Unit> LoadProjectCommand => _commandManager.LoadProjectCommand;
    public ReactiveCommand<Unit, Unit> LoadProjectFromFileCommand => _commandManager.LoadProjectFromFileCommand;
    public ReactiveCommand<Unit, Unit> EditNarrativeCommand => _commandManager.EditNarrativeCommand;
    public ReactiveCommand<Unit, Unit> SaveAsPdfCommand => _commandManager.SaveAsPdfCommand;
    public ReactiveCommand<Unit, Unit> CreateTaskCommand => _commandManager.CreateTaskCommand;
    public ReactiveCommand<Unit, Unit> UpdateTaskCommand => _commandManager.UpdateTaskCommand;
    public ReactiveCommand<Unit, Unit> DeleteTaskCommand => _commandManager.DeleteTaskCommand;
    public ReactiveCommand<Unit, Unit> UndoCommand => _commandManager.UndoCommand;
    public ReactiveCommand<Unit, Unit> RedoCommand => _commandManager.RedoCommand;

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    public ReactiveCommand<Unit, Unit> AboutCommand { get; }

    public MenuBarViewModel(CommandManager commandManager)
    {
        _commandManager = commandManager;

        ProjectName = _commandManager.ProjectName;
        ProjectAuthor = _commandManager.ProjectAuthor;

        _commandManager
            .WhenAnyValue(x => x.ProjectName)
            .BindTo(this, x => x.ProjectName);

        _commandManager
            .WhenAnyValue(x => x.ProjectAuthor)
            .BindTo(this, x => x.ProjectAuthor);

        // Bind CanExecute states
        _commandManager.UndoCommand.CanExecute
            .Subscribe(canExecute => CanUndo = canExecute);

        _commandManager.RedoCommand.CanExecute
            .Subscribe(canExecute => CanRedo = canExecute);

        _commandManager.EditNarrativeCommand.CanExecute
            .Subscribe(canExecute => CanEditNarrative = canExecute);

        _commandManager.UpdateTaskCommand.CanExecute
                    .Subscribe(canExecute =>
                    {
                        CanUpdateTask = canExecute;
                        Log.Information("MenuBarViewModel: UpdateTaskCommand CanExecute: {CanExecute}", canExecute);
                    });

        // Define ExitCommand
        ExitCommand = ReactiveCommand.Create(() =>
        {
            if (_commandManager.MainWindow != null)
            {
                _commandManager.MainWindow.Close();
            }
        });

        // Define AboutCommand
        AboutCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (_commandManager.MainWindow == null) return;
            var dialog = new Window
            {
                Title = "About",
                Width = 300,
                Height = 175,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = "\r\n\r\nBierman Technologies Project Manager\r\n\r\nVersion 1.0\r\n\r\nwww.tonybierman.com",
                    Margin = new Avalonia.Thickness(10),
                    TextAlignment = Avalonia.Media.TextAlignment.Center
                }
            };
            await dialog.ShowDialog(_commandManager.MainWindow);
        });
    }

    public void SetMainWindow(Window mainWindow)
    {
        _commandManager.SetMainWindow(mainWindow);
    }
}