using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using BiermanTech.ProjectManager.Controls;
using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Services;

public class GanttChartRenderer
{
    private readonly IResourceHost _resourceHost;

    public GanttChartRenderer(IResourceHost resourceHost)
    {
        _resourceHost = resourceHost ?? throw new ArgumentNullException(nameof(resourceHost));
    }

    private T GetResource<T>(string key)
    {
        if (_resourceHost.TryFindResource(key, out var resource) && resource is T typedResource)
        {
            return typedResource;
        }
        throw new InvalidOperationException($"Resource '{key}' not found or is of incorrect type.");
    }

    private DateTimeOffset NormalizeToMidnight(DateTimeOffset date)
    {
        return new DateTimeOffset(date.Date, date.Offset);
    }

    private int GetDayOffset(DateTimeOffset date, DateTimeOffset minDate)
    {
        DateTimeOffset normalizedDate = NormalizeToMidnight(date);
        DateTimeOffset normalizedMinDate = NormalizeToMidnight(minDate);
        return (int)(normalizedDate - normalizedMinDate).TotalDays;
    }

    private double CalculateXForDayOffset(int dayOffset, double pixelsPerDay)
    {
        return dayOffset * pixelsPerDay;
    }

    public (double X, double Width, double Y) CalculateTaskPosition(TaskItem task, DateTimeOffset minDate, double pixelsPerDay, double rowHeight, int rowIndex)
    {
        int dayOffset = GetDayOffset(task.CalculatedStartDate, minDate); // Use CalculatedStartDate
        double x = CalculateXForDayOffset(dayOffset, pixelsPerDay);
        double width = task.CalculatedDuration.TotalDays * pixelsPerDay; // Use CalculatedDuration
        double y = rowIndex * rowHeight;
        return (x, width, y);
    }

    public (double X, double TodayX) CalculateTodayLine(DateTimeOffset today, DateTimeOffset minDate, DateTimeOffset maxDate, double pixelsPerDay)
    {
        int dayOffset = GetDayOffset(today, minDate);
        double todayX = CalculateXForDayOffset(dayOffset, pixelsPerDay);
        bool isVisible = NormalizeToMidnight(today) >= NormalizeToMidnight(minDate) && NormalizeToMidnight(today) <= NormalizeToMidnight(maxDate);
        return (isVisible ? todayX : -1, todayX);
    }

    public List<(Point Start, Point End, bool IsArrow)> CalculateDependencyLines(TaskItem task, TaskItem dependsOn, DateTimeOffset minDate, double pixelsPerDay, double rowHeight, int taskIndex, int depIndex)
    {
        var lines = new List<(Point Start, Point End, bool IsArrow)>();
        int depDayOffset = GetDayOffset(dependsOn.EndDate, minDate);
        double depEndX = CalculateXForDayOffset(depDayOffset, pixelsPerDay);
        double depY = depIndex * rowHeight + (rowHeight / 2);
        int startDayOffset = GetDayOffset(task.CalculatedStartDate, minDate);
        double startX = CalculateXForDayOffset(startDayOffset, pixelsPerDay);
        double startY = taskIndex * rowHeight + (rowHeight / 2);

        double halfDayWidth = pixelsPerDay / 2;
        double depVerticalX = depEndX + halfDayWidth;
        double startVerticalX = startX + halfDayWidth;

        lines.Add((new Point(depEndX, depY), new Point(depVerticalX, depY), false));
        lines.Add((new Point(depVerticalX, depY), new Point(depVerticalX, startY), false));
        lines.Add((new Point(depVerticalX, startY), new Point(startX, startY), false));
        lines.Add((new Point(startX, startY), new Point(startX - GanttChartConfig.ArrowSize, startY - GanttChartConfig.ArrowSize), true));
        lines.Add((new Point(startX, startY), new Point(startX - GanttChartConfig.ArrowSize, startY + GanttChartConfig.ArrowSize), true));

        return lines;
    }

    public void RenderHeader(Canvas headerCanvas, List<TaskItem> tasks, GanttChartLayout layout)
    {
        headerCanvas.Children.Clear();
        double dayTextTop = GanttChartConfig.DayTextVerticalPosition;
        string lastMonthDisplayed = null;

        DateTimeOffset normalizedMinDate = NormalizeToMidnight(layout.MinDate);
        for (int dayOffset = 0; dayOffset <= layout.TotalDays; dayOffset++)
        {
            DateTimeOffset date = normalizedMinDate.AddDays(dayOffset);
            double x = CalculateXForDayOffset(dayOffset, layout.PixelsPerDay);

            var line = new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, layout.HeaderHeight),
                Stroke = GetResource<ISolidColorBrush>("DependencyLineBrush"),
                StrokeThickness = GetResource<double>("HeaderLineThickness")
            };
            headerCanvas.Children.Add(line);

            if (dayOffset < layout.TotalDays)
            {
                if (date.Day == 1 || dayOffset == 0)
                {
                    string monthName = date.ToString("MMMM");
                    if (monthName != lastMonthDisplayed)
                    {
                        var monthText = new TextBlock
                        {
                            Text = monthName,
                            Foreground = GetResource<ISolidColorBrush>("TextForegroundBrush"),
                            [Canvas.LeftProperty] = x + 2,
                            [Canvas.TopProperty] = GanttChartConfig.MonthTextTopOffset,
                            FontSize = GetResource<double>("MonthTextFontSize"),
                            FontWeight = GetResource<FontWeight>("HeaderFontWeight")
                        };
                        headerCanvas.Children.Add(monthText);
                        lastMonthDisplayed = monthName;
                    }
                }

                var dayText = new TextBlock
                {
                    Text = date.ToString("dd"),
                    Foreground = GetResource<ISolidColorBrush>("TextForegroundBrush"),
                    [Canvas.LeftProperty] = x + 2,
                    [Canvas.TopProperty] = dayTextTop,
                    FontSize = GetResource<double>("DayTextFontSize")
                };
                headerCanvas.Children.Add(dayText);
            }
        }
    }

    public void RenderTasks(Canvas ganttCanvas, List<TaskItem> tasks, TaskItem selectedTask, GanttChartLayout layout, Action<TaskItem> onTaskSelected)
    {
        ganttCanvas.Children.Clear();
        int rowIndex = 0;
        foreach (var task in tasks)
        {
            int dayOffset = GetDayOffset(task.CalculatedStartDate, layout.MinDate);
            double x = CalculateXForDayOffset(dayOffset, layout.PixelsPerDay);
            double y = rowIndex * layout.RowHeight;
            double taskHeight = Math.Min(Math.Max(layout.RowHeight - GanttChartConfig.TaskHeightPadding, 1), GanttChartConfig.TaskBarHeight);
            double centerY = y + (layout.RowHeight / 2);

            if (task.CalculatedDuration.TotalDays == 0)
            {
                double diamondWidth = layout.PixelsPerDay;
                double halfWidth = diamondWidth / 2;
                double halfHeight = taskHeight / 2;

                var diamond = new Polygon
                {
                    Points = new AvaloniaList<Point>
                    {
                        new Point(x + halfWidth, centerY - halfHeight),
                        new Point(x + diamondWidth, centerY),
                        new Point(x + halfWidth, centerY + halfHeight),
                        new Point(x, centerY)
                    },
                    Fill = Brushes.Black,
                    Stroke = GetResource<ISolidColorBrush>("TaskBorderBrush"),
                    StrokeThickness = GetResource<double>("TaskBorderThickness"),
                    Tag = task
                };

                diamond.PointerPressed += (s, e) =>
                {
                    if (s is Polygon p && p.Tag is TaskItem clickedTask)
                    {
                        onTaskSelected(clickedTask);
                    }
                };

                ganttCanvas.Children.Add(diamond);
            }
            else
            {
                double width = task.CalculatedDuration.TotalDays * layout.PixelsPerDay;

                var rect = new Rectangle
                {
                    Width = Math.Max(width, 1),
                    Height = taskHeight,
                    Fill = task == selectedTask ? GetResource<ISolidColorBrush>("TaskSelectedBrush") : GetResource<VisualBrush>("TaskDefaultBrush"),
                    Stroke = GetResource<ISolidColorBrush>("TaskBorderBrush"),
                    StrokeThickness = GetResource<double>("TaskBorderThickness"),
                    [Canvas.LeftProperty] = x,
                    [Canvas.TopProperty] = y + (layout.RowHeight - taskHeight) / 2,
                    Tag = task
                };

                rect.PointerPressed += (s, e) =>
                {
                    if (s is Rectangle r && r.Tag is TaskItem clickedTask)
                    {
                        onTaskSelected(clickedTask);
                    }
                };

                ganttCanvas.Children.Add(rect);

                if (task.PercentComplete > 0 && task.PercentComplete < 100)
                {
                    double progressWidth = (task.PercentComplete / 100) * width;
                    var progressRect = new Rectangle
                    {
                        Width = Math.Max(progressWidth, 1),
                        Height = taskHeight,
                        Fill = GetResource<ISolidColorBrush>("TaskProgressBrush"),
                        [Canvas.LeftProperty] = x,
                        [Canvas.TopProperty] = y + (layout.RowHeight - taskHeight) / 2
                    };
                    ganttCanvas.Children.Add(progressRect);
                }
            }

            rowIndex++;
        }
    }

    public void RenderTodayLine(Canvas ganttCanvas, DateTimeOffset today, GanttChartLayout layout)
    {
        var (todayX, _) = CalculateTodayLine(today, layout.MinDate, layout.MinDate.AddDays(layout.TotalDays), layout.PixelsPerDay);
        if (todayX >= 0)
        {
            var todayLine = new Line
            {
                StartPoint = new Point(todayX, 0),
                EndPoint = new Point(todayX, layout.ChartHeight),
                Stroke = GetResource<ISolidColorBrush>("TodayLineBrush"),
                StrokeThickness = GetResource<double>("TodayLineThickness"),
                StrokeDashArray = GetResource<AvaloniaList<double>>("TodayLineDashArray")
            };
            ganttCanvas.Children.Add(todayLine);
        }
    }

    public void RenderDependencies(Canvas ganttCanvas, List<TaskItem> tasks, GanttChartLayout layout)
    {
        ganttCanvas.Children.Clear(); // Clear previous dependencies
        int rowIndex = 0;
        foreach (var task in tasks)
        {
            if (task.DependsOn != null && task.DependsOn.Any())
            {
                foreach (var dependsOn in task.DependsOn)
                {
                    int depIndex = tasks.IndexOf(dependsOn);
                    if (depIndex >= 0) // Ensure dependency exists in the flat list
                    {
                        var lines = CalculateDependencyLines(task, dependsOn, layout.MinDate, layout.PixelsPerDay, layout.RowHeight, rowIndex, depIndex);

                        foreach (var (start, end, isArrow) in lines)
                        {
                            if (isArrow)
                            {
                                var arrow = new Polygon
                                {
                                    Points = new AvaloniaList<Point>
                                    {
                                        start,
                                        new Point(end.X, end.Y),
                                        new Point(end.X, start.Y + (end.Y - start.Y) / 2)
                                    },
                                    Fill = GetResource<ISolidColorBrush>("DependencyLineBrush")
                                };
                                ganttCanvas.Children.Add(arrow);
                            }
                            else
                            {
                                var line = new Line
                                {
                                    StartPoint = start,
                                    EndPoint = end,
                                    Stroke = GetResource<ISolidColorBrush>("DependencyLineBrush"),
                                    StrokeThickness = GetResource<double>("DependencyLineThickness")
                                };
                                ganttCanvas.Children.Add(line);
                            }
                        }
                    }
                }
            }
            rowIndex++;
        }
    }

    public void RenderDateLines(Canvas dateLinesCanvas, GanttChartLayout layout)
    {
        dateLinesCanvas.Children.Clear();
        DateTimeOffset normalizedMinDate = NormalizeToMidnight(layout.MinDate);

        for (int dayOffset = 0; dayOffset <= layout.TotalDays; dayOffset++)
        {
            double x = CalculateXForDayOffset(dayOffset, layout.PixelsPerDay);

            var dateLine = new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, layout.ChartHeight),
                Stroke = GetResource<ISolidColorBrush>("DateLineBrush"),
                StrokeThickness = GetResource<double>("HeaderLineThickness"),
                StrokeDashArray = GetResource<AvaloniaList<double>>("TodayLineDashArray")
            };

            dateLinesCanvas.Children.Add(dateLine);
        }
    }
}