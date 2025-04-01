using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.ViewModels;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Views;

public partial class TaskDialog : Window
{
    public TaskDialog()
    {
        InitializeComponent();
    }

    public TaskDialog(TaskItem taskToEdit, ObservableCollection<TaskItem> allTasks)
    {
        InitializeComponent();
        DataContext = new TaskDialogViewModel(taskToEdit, allTasks, this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}