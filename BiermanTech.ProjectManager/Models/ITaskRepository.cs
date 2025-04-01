using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Models;

public interface ITaskRepository
{
    List<TaskItem> GetTasks();
    void AddTask(TaskItem task);
    void RemoveTask(TaskItem task);
    event EventHandler TasksChanged;
    void NotifyTasksChanged(); // Add this method to invoke the event
}