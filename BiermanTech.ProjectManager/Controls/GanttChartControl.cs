using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using Avalonia.Controls.Presenters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Avalonia.ReactiveUI;

namespace BiermanTech.ProjectManager.Controls;

public class GanttChartControl : TemplatedControl
{
    public static readonly DirectProperty<GanttChartControl, List<TaskItem>> TasksProperty =
        AvaloniaProperty.RegisterDirect<GanttChartControl, List<TaskItem>>(
            nameof(Tasks),
            o => o.Tasks,
            (o, v) => o.Tasks = v);

    public static readonly DirectProperty<GanttChartControl, TaskItem> SelectedTaskProperty =
        AvaloniaProperty.RegisterDirect<GanttChartControl, TaskItem>(
            nameof(SelectedTask),
            o => o.SelectedTask,
            (o, v) => o.SelectedTask = v);

    private readonly GanttChartViewModel _viewModel;
    private readonly GanttChartRenderer _renderer;
    private Canvas _ganttCanvas;
    private Canvas _headerCanvas;
    private ItemsControl _taskList;
    private ScrollViewer _taskListScrollViewer;
    private ScrollViewer _chartScrollViewer;

    public List<TaskItem> Tasks
    {
        get => _viewModel.Tasks;
        set => _viewModel.Tasks = value;
    }

    public TaskItem SelectedTask
    {
        get => _viewModel.SelectedTask;
        set
        {
            if (_viewModel.SelectedTask != value)
            {
                var oldValue = _viewModel.SelectedTask;
                _viewModel.SelectedTask = value;
                RaisePropertyChanged(SelectedTaskProperty, oldValue, value);
            }
        }
    }

    public GanttChartControl() : this(
        App.ServiceProvider.GetService<ITaskRepository>(),
        App.ServiceProvider.GetService<GanttChartRenderer>())
    {
    }

    public GanttChartControl(ITaskRepository taskRepository, GanttChartRenderer renderer)
    {
        _viewModel = new GanttChartViewModel(taskRepository);
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        this.WhenAnyValue(x => x._viewModel.Tasks, x => x._viewModel.SelectedTask, x => x.Bounds)
            .Throttle(TimeSpan.FromMilliseconds(50))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ => UpdateGanttChart());

        // Subscribe to changes in _viewModel.SelectedTask to raise property changed notification
        _viewModel.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                // Raise property changed for SelectedTaskProperty to notify the binding
                RaisePropertyChanged(SelectedTaskProperty, selectedTask, selectedTask);
                Log.Information("GanttChartControl SelectedTask changed, task: {TaskName}", selectedTask?.Name ?? "null");
            });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _ganttCanvas = e.NameScope.Find<Canvas>("PART_GanttCanvas");
        _headerCanvas = e.NameScope.Find<Canvas>("PART_HeaderCanvas");
        _taskList = e.NameScope.Find<ItemsControl>("PART_TaskList");
        _taskListScrollViewer = e.NameScope.Find<ScrollViewer>("PART_TaskListScrollViewer");
        _chartScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ChartScrollViewer");

        if (_taskListScrollViewer != null && _chartScrollViewer != null)
        {
            _ = new ScrollSynchronizer(_taskListScrollViewer, _chartScrollViewer);
            _ = new ScrollSynchronizer(_chartScrollViewer, _taskListScrollViewer);
        }

        if (_taskList != null)
        {
            _taskList.ItemsSource = _viewModel.Tasks;
        }

        UpdateGanttChart();
    }

    private bool IsValidForRendering()
    {
        return _ganttCanvas != null && _headerCanvas != null && _taskList != null &&
               _viewModel.Tasks != null && _viewModel.Tasks.Any() &&
               Bounds.Width > 0 && Bounds.Height > 0;
    }

    private void UpdateGanttChart()
    {
        if (!IsValidForRendering()) return;

        var layout = new GanttChartLayout(_viewModel.Tasks, Bounds.Width, Bounds.Height);

        Dispatcher.UIThread.Post(() =>
        {
            if (_taskList.ItemContainerGenerator != null)
            {
                foreach (var item in _taskList.GetRealizedContainers())
                {
                    if (item is ContentPresenter presenter)
                    {
                        presenter.MinHeight = layout.RowHeight;
                        presenter.Height = layout.RowHeight;
                    }
                }
            }

            _renderer.RenderHeader(_headerCanvas, _viewModel.Tasks, layout);
            _renderer.RenderTasks(_ganttCanvas, _viewModel.Tasks, _viewModel.SelectedTask, layout, task => SelectedTask = task);
            _renderer.RenderTodayLine(_ganttCanvas, new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero), layout);
            _renderer.RenderDependencies(_ganttCanvas, _viewModel.Tasks, layout);

            _headerCanvas.Width = layout.TotalDays * layout.PixelsPerDay;
            _headerCanvas.Height = layout.HeaderHeight;
            _ganttCanvas.Width = layout.TotalDays * layout.PixelsPerDay;
            _ganttCanvas.Height = _viewModel.Tasks.Count * layout.RowHeight;
        });
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _viewModel.Dispose();
    }
}