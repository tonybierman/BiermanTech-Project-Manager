using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.Commands
{
    public interface ICommandManager
    {
        bool CanEditNarrative { get; }
        bool CanRedo { get; }
        bool CanUndo { get; }
        ReactiveCommand<Unit, Unit> CreateTaskCommand { get; }
        ReactiveCommand<Unit, Unit> DeleteTaskCommand { get; }
        ReactiveCommand<Unit, Unit> EditNarrativeCommand { get; }
        ReactiveCommand<int, Unit> LoadProjectCommand { get; }
        ReactiveCommand<Unit, Unit> LoadProjectFromFileCommand { get; }
        Window MainWindow { get; set; }
        ProjectNarrative Narrative { get; }
        ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
        string NotificationMessage { get; set; }
        Project Project { get; set; }
        string ProjectAuthor { get; }
        string ProjectCurrentState { get; }
        string ProjectName { get; }
        string ProjectPlan { get; }
        string ProjectResults { get; }
        string ProjectSituation { get; }
        ReactiveCommand<Unit, Unit> RedoCommand { get; }
        ReactiveCommand<Unit, Unit> SaveAsPdfCommand { get; }
        ReactiveCommand<Unit, Unit> SaveAsProjectCommand { get; }
        ReactiveCommand<Unit, Unit> SaveProjectCommand { get; }
        TaskItem SelectedTask { get; set; }
        ObservableCollection<TaskItem> Tasks { get; set; }
        ReactiveCommand<Unit, Unit> UndoCommand { get; }
        ReactiveCommand<Unit, Unit> UpdateTaskCommand { get; }

        event PropertyChangedEventHandler PropertyChanged;

        void ExecuteCommand(ICommand command);
        Task Initialize();
        void Redo();
        void SetMainWindow(Window mainWindow);
        void ShowNotification(string message);
        void Undo();
    }
}