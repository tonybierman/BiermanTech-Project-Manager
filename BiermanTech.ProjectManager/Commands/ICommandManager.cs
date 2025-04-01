namespace BiermanTech.ProjectManager.Commands;

public interface ICommandManager
{
    void ExecuteCommand(ICommand command);
    void Undo();
    void Redo();
    bool CanUndo { get; }
    bool CanRedo { get; }
}