using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using BiermanTech.ProjectManager.Models;
using BiermanTech.ProjectManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using Avalonia.Controls.Presenters;
using Microsoft.Extensions.DependencyInjection;

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

    private ObservableCollection<TaskItem> _tasks;
    private TaskItem _selectedTask;
    private List<TaskItem> _localTasks;
    private IDisposable _collectionSubscription;
    private Canvas _ganttCanvas;
    private Canvas _headerCanvas;
    private ItemsControl _taskList;
    private ScrollViewer _taskListScrollViewer;
    private ScrollViewer _chartScrollViewer;
    private readonly GanttChartRenderer _renderer;

    public ObservableCollection<TaskItem> Tasks
    {
        get => _tasks;
        set => SetAndRaise(TasksProperty, ref _tasks, value);
    }

    public TaskItem SelectedTask
    {
        get => _selectedTask;
        set => SetAndRaise(SelectedTaskProperty, ref _selectedTask, value);
    }

    public GanttChartControl() : this(App.ServiceProvider.GetService<GanttChartRenderer>())
    {
    }

    public GanttChartControl(GanttChartRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _localTasks = new List<TaskItem>();
        SizeChanged += (s, e) => UpdateGanttChart();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        this.WhenAnyValue(x => x.Tasks)
            .Subscribe(tasks =>
            {
                _collectionSubscription?.Dispose();
                _collectionSubscription = null;

                if (tasks != null)
                {
                    _localTasks = new List<TaskItem>(tasks);
                    if (_taskList != null)
                    {
                        _taskList.ItemsSource = _localTasks;
                    }

                    _collectionSubscription = Observable.FromEventPattern(tasks, nameof(tasks.CollectionChanged))
                        .Subscribe(_ =>
                        {
                            _localTasks = new List<TaskItem>(tasks);
                            UpdateGanttChart();
                        });
                }
                else
                {
                    _localTasks.Clear();
                    if (_taskList != null)
                    {
                        _taskList.ItemsSource = _localTasks;
                    }
                }
                UpdateGanttChart();
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
            _taskListScrollViewer.ScrollChanged += (s, args) =>
            {
                if (args.ExtentDelta.Y != 0 || args.OffsetDelta.Y != 0)
                {
                    _chartScrollViewer.Offset = _chartScrollViewer.Offset.WithY(_taskListScrollViewer.Offset.Y);
                }
            };
            _chartScrollViewer.ScrollChanged += (s, args) =>
            {
                if (args.ExtentDelta.Y != 0 || args.OffsetDelta.Y != 0)
                {
                    _taskListScrollViewer.Offset = _taskListScrollViewer.Offset.WithY(_chartScrollViewer.Offset.Y);
                }
            };
        }

        if (_taskList != null)
        {
            _taskList.ItemsSource = _localTasks;
        }

        UpdateGanttChart();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TasksProperty || change.Property == SelectedTaskProperty)
        {
            UpdateGanttChart();
        }
    }

    private void UpdateGanttChart()
    {
        if (_ganttCanvas == null || _headerCanvas == null || _taskList == null || _localTasks == null || !_localTasks.Any() || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        _ganttCanvas.Children.Clear();
        _headerCanvas.Children.Clear();

        double taskListWidth = GanttChartConfig.TaskListWidth;
        double chartWidth = Math.Max(Bounds.Width - taskListWidth, 1);
        double headerHeight = GanttChartConfig.HeaderHeight;
        double chartHeight = Math.Max(Bounds.Height - headerHeight, 1);

        double totalDays = (_localTasks.Max(t => t.EndDate) - _localTasks.Min(t => t.StartDate)).TotalDays;
        double pixelsPerDay = Math.Max(chartWidth / totalDays, GanttChartConfig.MinPixelsPerDay);
        double rowHeight = Math.Max(chartHeight / _localTasks.Count, GanttChartConfig.MinRowHeight);

        if (_taskList.ItemContainerGenerator != null)
        {
            foreach (var item in _taskList.GetRealizedContainers())
            {
                if (item is ContentPresenter presenter)
                {
                    presenter.MinHeight = rowHeight;
                    presenter.Height = rowHeight;
                }
            }
        }

        DateTimeOffset minDate = _localTasks.Min(t => t.StartDate);
        DateTimeOffset today = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero);

        // Draw header (dates)
        for (int day = 0; day <= totalDays; day++)
        {
            DateTimeOffset date = minDate.AddDays(day);
            double x = day * pixelsPerDay;
            var line = new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, headerHeight),
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            var text = new TextBlock
            {
                Text = date.ToString("MMM dd"),
                Foreground = Brushes.Black,
                [Canvas.LeftProperty] = x + 2,
                [Canvas.TopProperty] = 5,
                FontSize = 10
            };
            _headerCanvas.Children.Add(line);
            _headerCanvas.Children.Add(text);
        }
        _headerCanvas.Width = totalDays * pixelsPerDay;
        _headerCanvas.Height = headerHeight;

        // Draw tasks
        int rowIndex = 0;
        foreach (var task in _localTasks)
        {
            var (x, width, y) = _renderer.CalculateTaskPosition(task, minDate, pixelsPerDay, rowHeight, rowIndex);

            var rect = new Avalonia.Controls.Shapes.Rectangle
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(rowHeight - 10, 1),
                Fill = task == SelectedTask ? Brushes.Yellow : Brushes.LightBlue,
                [Canvas.LeftProperty] = x,
                [Canvas.TopProperty] = y + 5,
                Tag = task
            };

            rect.PointerPressed += (s, e) =>
            {
                if (s is Avalonia.Controls.Shapes.Rectangle r && r.Tag is TaskItem clickedTask)
                {
                    SelectedTask = clickedTask;
                }
            };

            if (task.PercentComplete > 0)
            {
                double progressWidth = (task.PercentComplete / 100) * width;
                var progressRect = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = Math.Max(progressWidth, 1),
                    Height = Math.Max(rowHeight - 10, 1),
                    Fill = Brushes.Blue,
                    [Canvas.LeftProperty] = x,
                    [Canvas.TopProperty] = y + 5
                };
                _ganttCanvas.Children.Add(progressRect);
            }

            _ganttCanvas.Children.Add(rect);
            rowIndex++;
        }

        // Draw TODAY line
        var (todayX, _) = _renderer.CalculateTodayLine(today, minDate, _localTasks.Max(t => t.EndDate), pixelsPerDay);
        if (todayX >= 0)
        {
            var todayLine = new Line
            {
                StartPoint = new Point(todayX, 0),
                EndPoint = new Point(todayX, chartHeight),
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 }
            };
            _ganttCanvas.Children.Add(todayLine);
        }

        // Draw dependencies
        rowIndex = 0;
        foreach (var task in _localTasks)
        {
            if (task.DependsOn != null)
            {
                int depIndex = _localTasks.IndexOf(task.DependsOn);
                var lines = _renderer.CalculateDependencyLines(task, task.DependsOn, minDate, pixelsPerDay, rowHeight, rowIndex, depIndex);

                foreach (var (start, end, isArrow) in lines)
                {
                    if (isArrow)
                    {
                        var arrow = new Polygon
                        {
                            Points = new Avalonia.Collections.AvaloniaList<Point>
                            {
                                start,
                                new Point(end.X, end.Y),
                                new Point(end.X, start.Y + (end.Y - start.Y) / 2)
                            },
                            Fill = Brushes.Black
                        };
                        _ganttCanvas.Children.Add(arrow);
                    }
                    else
                    {
                        var line = new Line
                        {
                            StartPoint = start,
                            EndPoint = end,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        _ganttCanvas.Children.Add(line);
                    }
                }
            }
            rowIndex++;
        }

        _ganttCanvas.Width = totalDays * pixelsPerDay;
        _ganttCanvas.Height = _localTasks.Count * rowHeight;
    }
}