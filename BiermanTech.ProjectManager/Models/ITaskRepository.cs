using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Models;

public interface ITaskRepository
{
    ObservableCollection<TaskItem> GetTasks();
    void AddTask(TaskItem task);
    void RemoveTask(TaskItem task); // Add this method
}