using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.ViewModels;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager.Views;

public partial class NarrativeDialog : Window
{
    public NarrativeDialog()
    {
        InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is NarrativeDialogViewModel vm)
            {
                vm.SaveCommand.Subscribe(new Action<ProjectNarrative>(result =>
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

    public new async Task<ProjectNarrative> ShowDialog(Window parent)
    {
        return await ShowDialog<ProjectNarrative>(parent);
    }
}