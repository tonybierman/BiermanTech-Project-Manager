using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Avalonia.Controls.Presenters;
using System.Reactive.Linq;

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
    private IDisposable _collectionSubscription;
    private Canvas _ganttCanvas;
    private Canvas _headerCanvas;
    private ItemsControl _taskList;
    private ScrollViewer _taskListScrollViewer;
    private ScrollViewer _chartScrollViewer;

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

    public GanttChartControl()
    {
        this.WhenAnyValue(x => x.Tasks)
            .Subscribe(tasks =>
            {
                _collectionSubscription?.Dispose();

                if (tasks != null)
                {
                    _collectionSubscription = this.WhenAnyValue(x => x.Tasks)
                        .Select(t => t)
                        .Subscribe(t => t.CollectionChanged += (s, e) => UpdateGanttChart());
                }
                UpdateGanttChart();
            });

        SizeChanged += (s, e) => UpdateGanttChart();
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
        if (_ganttCanvas == null || _headerCanvas == null || _taskList == null || Tasks == null || !Tasks.Any() || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        _ganttCanvas.Children.Clear();
        _headerCanvas.Children.Clear();
        _taskList.ItemsSource = Tasks;

        double taskListWidth = 150;
        double chartWidth = Math.Max(Bounds.Width - taskListWidth, 1);
        double headerHeight = 30;
        double chartHeight = Math.Max(Bounds.Height - headerHeight, 1);

        double totalDays = (Tasks.Max(t => t.EndDate) - Tasks.Min(t => t.StartDate)).TotalDays;
        double pixelsPerDay = Math.Max(chartWidth / totalDays, 10);
        double rowHeight = Math.Max(chartHeight / Tasks.Count, 30);

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

        DateTimeOffset minDate = Tasks.Min(t => t.StartDate);
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
        foreach (var task in Tasks)
        {
            double x = (task.StartDate - minDate).TotalDays * pixelsPerDay;
            double width = task.Duration.TotalDays * pixelsPerDay;
            double y = rowIndex * rowHeight;

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
        double todayX = (today - minDate).TotalDays * pixelsPerDay;
        if (today >= minDate && today <= Tasks.Max(t => t.EndDate))
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
        foreach (var task in Tasks)
        {
            if (task.DependsOn != null)
            {
                int depIndex = Tasks.IndexOf(task.DependsOn);
                double depEndX = (task.DependsOn.EndDate - minDate).TotalDays * pixelsPerDay;
                double depY = depIndex * rowHeight + (rowHeight / 2);
                double startX = (task.StartDate - minDate).TotalDays * pixelsPerDay;
                double startY = rowIndex * rowHeight + (rowHeight / 2);

                var hLine1 = new Line
                {
                    StartPoint = new Point(depEndX, depY),
                    EndPoint = new Point(depEndX + 10, depY),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                var vLine = new Line
                {
                    StartPoint = new Point(depEndX + 10, depY),
                    EndPoint = new Point(depEndX + 10, startY),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                var hLine2 = new Line
                {
                    StartPoint = new Point(depEndX + 10, startY),
                    EndPoint = new Point(startX, startY),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                var arrow = new Polygon
                {
                    Points = new Avalonia.Collections.AvaloniaList<Point>
                    {
                        new Point(startX, startY),
                        new Point(startX - 5, startY - 5),
                        new Point(startX - 5, startY + 5)
                    },
                    Fill = Brushes.Black
                };

                _ganttCanvas.Children.Add(hLine1);
                _ganttCanvas.Children.Add(vLine);
                _ganttCanvas.Children.Add(hLine2);
                _ganttCanvas.Children.Add(arrow);
            }
            rowIndex++;
        }

        _ganttCanvas.Width = totalDays * pixelsPerDay;
        _ganttCanvas.Height = Tasks.Count * rowHeight;
    }
}