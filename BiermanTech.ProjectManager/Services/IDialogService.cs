using BiermanTech.ProjectManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.Services;

public interface IDialogService
{
    Task<TaskItem> ShowTaskDialog(TaskItem task, List<TaskItem> tasks, Avalonia.Controls.Window parent);
}