using BiermanTech.ProjectManager.Models;
using System.Collections.Generic;
using System;

namespace BiermanTech.ProjectManager.Services;

public interface ITaskRepository
{
    event EventHandler TasksChanged;
    List<TaskItem> GetTasks();
    void AddTask(TaskItem task, Guid? parentTaskId = null); // Updated signature
    void RemoveTask(TaskItem task);
    void NotifyTasksChanged();
}