using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BiermanTech.ProjectManager.Services;

public class LoggingObservableCollection<T> : ObservableCollection<T>
{
    public LoggingObservableCollection() : base() { }

    public LoggingObservableCollection(IEnumerable<T> collection) : base(collection) { }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        Console.WriteLine($"MoveItem called: OldIndex={oldIndex}, NewIndex={newIndex}, StackTrace={Environment.StackTrace}");
        base.MoveItem(oldIndex, newIndex);
    }
}