using Avalonia.Controls;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.ViewModels;
using BiermanTech.ProjectManager.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.Services;

public class DialogService : IDialogService
{
    public async Task<TaskItem> ShowTaskDialog(TaskItem task, List<TaskItem> tasks, Window parent)
    {
        var dialog = new TaskDialog();
        var viewModel = new TaskDialogViewModel(task, tasks);
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(parent);

        return viewModel.GetResult();
    }

    public async Task<ProjectNarrative> ShowNarrativeDialog(ProjectNarrative narrative, Window parent)
    {
        var dialog = new NarrativeDialog();
        var viewModel = new NarrativeDialogViewModel(narrative);
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(parent);

        return viewModel.GetResult();
    }

    public async Task ShowAboutDialog(Window owner)
    {
        //var dialog = new AboutDialog();
        //await dialog.ShowDialog(owner);
    }
}