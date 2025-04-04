using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Models
{
    public interface ITaskRepository
    {
        event EventHandler TasksChanged;

        void AddTask(TaskItem task);
        void ClearTasks();
        void DeleteTask(TaskItem task);
        IEnumerable<TaskItem> GetTasks();
        void UpdateTask(TaskItem task);
    }
}