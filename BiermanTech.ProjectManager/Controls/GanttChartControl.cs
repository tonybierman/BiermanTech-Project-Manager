﻿using Avalonia;
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
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.ReactiveUI;

namespace BiermanTech.ProjectManager.Controls;

public class GanttChartControl : TemplatedControl
{
    public static readonly DirectProperty<GanttChartControl, ObservableCollection<TaskItem>> TasksProperty =
        AvaloniaProperty.RegisterDirect<GanttChartControl, ObservableCollection<TaskItem>>(
            nameof(Tasks),
            o => o.Tasks,
            (o, v) => o.Tasks = v);

    public static readonly DirectProperty<GanttChartControl, TaskItem> SelectedTaskProperty =
        AvaloniaProperty.RegisterDirect<GanttChartControl, TaskItem>(
            nameof(SelectedTask),
            o => o.SelectedTask,
            (o, v) => o.SelectedTask = v);

    private readonly GanttChartViewModel _viewModel;
    private GanttChartRenderer _renderer;
    private Canvas _ganttCanvas;
    private Canvas _dependencyCanvas;
    private Canvas _headerCanvas;
    private Canvas _dateLinesCanvas;
    private ItemsControl _taskList;
    private ScrollViewer _taskListScrollViewer;
    private ScrollViewer _chartScrollViewer;
    private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

    public ObservableCollection<TaskItem> Tasks
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

    public GanttChartControl(GanttChartViewModel viewModel)
    {
        _viewModel = viewModel;

        // Subscribe to Tasks collection changes and property changes
        SubscribeToTaskChanges(_viewModel.Tasks);

        this.WhenAnyValue(x => x._viewModel.Tasks, x => x._viewModel.SelectedTask, x => x.Bounds)
            .Throttle(TimeSpan.FromMilliseconds(50))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ => UpdateGanttChart());

        _viewModel.WhenAnyValue(x => x.SelectedTask)
            .Subscribe(selectedTask =>
            {
                RaisePropertyChanged(SelectedTaskProperty, selectedTask, selectedTask);
            });

        _viewModel.WhenAnyValue(x => x.Tasks)
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(tasks =>
            {
                UnsubscribeFromTaskChanges();
                SubscribeToTaskChanges(tasks);
                if (_taskList != null)
                {
                    _taskList.ItemsSource = FlattenTasks(tasks);
                    Log.Information("GanttChartControl task list updated, task count: {TaskCount}", FlattenTasks(tasks)?.Count() ?? 0);
                }
            });
    }

    private void SubscribeToTaskChanges(ObservableCollection<TaskItem> tasks)
    {
        if (tasks == null) return;

        // Subscribe to collection changes
        tasks.CollectionChanged += (s, e) =>
        {
            Dispatcher.UIThread.Post(() => UpdateGanttChart());
            // Handle added or removed tasks
            if (e.NewItems != null)
            {
                foreach (TaskItem newTask in e.NewItems)
                {
                    SubscribeToTaskItem(newTask);
                }
            }
        };

        // Subscribe to existing tasks
        foreach (var task in tasks)
        {
            SubscribeToTaskItem(task);
        }
    }

    private void SubscribeToTaskItem(TaskItem task)
    {
        if (task == null) return;

        // Subscribe to property changes of this task
        task.PropertyChanged += Task_PropertyChanged;

        // Subscribe to children collection changes
        task.Children.CollectionChanged += (s, e) =>
        {
            Dispatcher.UIThread.Post(() => UpdateGanttChart());
            if (e.NewItems != null)
            {
                foreach (TaskItem newChild in e.NewItems)
                {
                    SubscribeToTaskItem(newChild);
                }
            }
        };

        // Subscribe to existing children
        foreach (var child in task.Children)
        {
            SubscribeToTaskItem(child);
        }
    }

    private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => UpdateGanttChart());
    }

    private void UnsubscribeFromTaskChanges()
    {
        foreach (var task in FlattenTasks(_viewModel.Tasks ?? new ObservableCollection<TaskItem>()))
        {
            task.PropertyChanged -= Task_PropertyChanged;
            task.Children.CollectionChanged -= (s, e) => Dispatcher.UIThread.Post(() => UpdateGanttChart());
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _ganttCanvas = e.NameScope.Find<Canvas>("PART_GanttCanvas");
        _dependencyCanvas = e.NameScope.Find<Canvas>("PART_DependencyCanvas");
        _headerCanvas = e.NameScope.Find<Canvas>("PART_HeaderCanvas");
        _dateLinesCanvas = e.NameScope.Find<Canvas>("PART_DateLinesCanvas");
        _taskList = e.NameScope.Find<ItemsControl>("PART_TaskList");
        _taskListScrollViewer = e.NameScope.Find<ScrollViewer>("PART_TaskListScrollViewer");
        _chartScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ChartScrollViewer");

        _renderer = new GanttChartRenderer(this);

        if (_taskListScrollViewer != null && _chartScrollViewer != null)
        {
            _ = new ScrollSynchronizer(_taskListScrollViewer, _chartScrollViewer);
            _ = new ScrollSynchronizer(_chartScrollViewer, _taskListScrollViewer);
        }

        if (_taskList != null)
        {
            _taskList.ItemsSource = FlattenTasks(_viewModel.Tasks);
        }

        UpdateGanttChart();
    }

    private bool IsValidForRendering()
    {
        return _renderer != null &&
               _ganttCanvas != null &&
               _dependencyCanvas != null &&
               _headerCanvas != null &&
               _dateLinesCanvas != null &&
               _taskList != null &&
               _viewModel.Tasks != null &&
               _viewModel.Tasks.Any() &&
               Bounds.Width > 0 &&
               Bounds.Height > 0;
    }

    private void UpdateGanttChart()
    {
        if (!IsValidForRendering())
        {
            Log.Information("Gantt chart not valid for rendering: Tasks={TaskCount}, Bounds={Width}x{Height}",
                _viewModel.Tasks?.Count ?? 0, Bounds.Width, Bounds.Height);
            return;
        }

        var flatTasks = FlattenTasks(_viewModel.Tasks).ToList();
        Log.Information("Rendering Gantt chart with {TaskCount} tasks", flatTasks.Count);
        foreach (var task in flatTasks)
        {
            Log.Information("Task: {Name}, Start: {Start}, Duration: {Duration}, IsParent: {IsParent}",
                task.Name, task.CalculatedStartDate, task.CalculatedDuration, task.IsParent);
        }
        var layout = new GanttChartLayout(flatTasks, Bounds.Width, Bounds.Height);

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

            _renderer.RenderDateLines(_dateLinesCanvas, layout);
            _renderer.RenderHeader(_headerCanvas, flatTasks, layout);
            _renderer.RenderTasks(_ganttCanvas, flatTasks, _viewModel.SelectedTask, layout, task => SelectedTask = task);
            _renderer.RenderTodayLine(_ganttCanvas, new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero), layout);
            _renderer.RenderDependencies(_dependencyCanvas, flatTasks, layout);

            _headerCanvas.Width = layout.TotalDays * layout.PixelsPerDay;
            _headerCanvas.Height = layout.HeaderHeight;
            _ganttCanvas.Width = layout.TotalDays * layout.PixelsPerDay;
            _ganttCanvas.Height = flatTasks.Count * layout.RowHeight;
            _dependencyCanvas.Width = layout.TotalDays * layout.PixelsPerDay;
            _dependencyCanvas.Height = flatTasks.Count * layout.RowHeight;
            _dateLinesCanvas.Width = layout.TotalDays * layout.PixelsPerDay;
            _dateLinesCanvas.Height = flatTasks.Count * layout.RowHeight;

            Log.Information("GanttCanvas size: {Width}x{Height}", _ganttCanvas.Width, _ganttCanvas.Height);
            Log.Information("DependencyCanvas size: {Width}x{Height}", _dependencyCanvas.Width, _dependencyCanvas.Height);
        });
    }

    private IEnumerable<TaskItem> FlattenTasks(IEnumerable<TaskItem> tasks)
    {
        if (tasks == null) yield break;
        foreach (var task in tasks)
        {
            yield return task;
            foreach (var child in FlattenTasks(task.Children))
            {
                yield return child;
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromTaskChanges();
        base.OnDetachedFromVisualTree(e);
    }
}