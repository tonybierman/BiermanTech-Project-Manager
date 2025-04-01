namespace BiermanTech.ProjectManager.Commands;

public interface ICommand
{
    void Execute();
    void Undo();
}