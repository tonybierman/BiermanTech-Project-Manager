using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.ViewModels;
using System.Threading.Tasks;
using System;
using BiermanTech.ProjectManager.Models;
using System.Reactive.Linq;
using System.Reactive;

namespace BiermanTech.ProjectManager.Views;

public partial class TaskDialog : Window
{
    public TaskDialog()
    {
        InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is TaskDialogViewModel vm)
            {
                vm.SaveCommand.Subscribe(new Action<TaskItem>(result =>
                {
                    Close(result);
                }));

                vm.CancelCommand.Subscribe(new Action<Unit>(_ =>
                {
                    Close(null);
                }));
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public new async Task<TaskItem> ShowDialog(Window parent)
    {
        return await ShowDialog<TaskItem>(parent);
    }
}