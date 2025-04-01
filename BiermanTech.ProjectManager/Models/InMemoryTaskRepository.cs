using BiermanTech.ProjectManager.Models;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Services;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly ObservableCollection<TaskItem> _tasks;

    public InMemoryTaskRepository()
    {
        _tasks = new ObservableCollection<TaskItem>();
    }

    public ObservableCollection<TaskItem> GetTasks() => _tasks;

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
    }

    public void RemoveTask(TaskItem task)
    {
        _tasks.Remove(task);
    }
}