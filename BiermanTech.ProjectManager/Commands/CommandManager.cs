using System.Collections.Generic;
using System.ComponentModel; // Add for INotifyPropertyChanged
using System.Runtime.CompilerServices; // Add for CallerMemberName

namespace BiermanTech.ProjectManager.Commands;

public class CommandManager : ICommandManager, INotifyPropertyChanged
{
    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    public event PropertyChangedEventHandler PropertyChanged;

    private bool _canUndo;
    private bool _canRedo;

    public bool CanUndo
    {
        get => _canUndo;
        private set
        {
            if (_canUndo != value)
            {
                _canUndo = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanRedo
    {
        get => _canRedo;
        private set
        {
            if (_canRedo != value)
            {
                _canRedo = value;
                OnPropertyChanged();
            }
        }
    }

    public CommandManager()
    {
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    public void Undo()
    {
        if (_undoStack.Count == 0) return;
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    public void Redo()
    {
        if (_redoStack.Count == 0) return;
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}