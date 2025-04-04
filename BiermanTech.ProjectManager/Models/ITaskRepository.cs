using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Models
{
    public interface ITaskRepository
    {
        event EventHandler TasksChanged;

        void AddTask(TaskItem task, int? parentTaskId = null);
        List<TaskItem> GetTasks();
        void NotifyTasksChanged();
        void RemoveTask(TaskItem task);
    }
}