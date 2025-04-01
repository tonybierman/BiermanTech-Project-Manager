using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.Services;

public interface IDialogService
{
    Task<TaskItem> ShowTaskDialog(TaskItem taskToEdit, ObservableCollection<TaskItem> allTasks, Window parentWindow);
}