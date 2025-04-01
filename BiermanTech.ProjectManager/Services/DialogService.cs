using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Views;

namespace BiermanTech.ProjectManager.Services;

public class DialogService : IDialogService
{
    public async Task<TaskItem> ShowTaskDialog(TaskItem taskToEdit, ObservableCollection<TaskItem> allTasks, Window parentWindow)
    {
        var dialog = new TaskDialog(taskToEdit, allTasks);
        return await dialog.ShowDialog<TaskItem>(parentWindow);
    }
}