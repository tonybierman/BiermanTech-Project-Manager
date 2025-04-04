﻿using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using BiermanTech.ProjectManager.Controls;
using BiermanTech.ProjectManager.Models;
using Serilog;
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
        Log.Error("Resource '{Key}' not found or is of incorrect type.", key);
        // Fallback for visibility during debugging
        if (typeof(T) == typeof(ISolidColorBrush)) return (T)(object)new SolidColorBrush(Colors.Red);
        if (typeof(T) == typeof(VisualBrush)) return (T)(object)new SolidColorBrush(Colors.Blue);
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

        // Dependency task end (center of end)
        int depDayOffset = GetDayOffset(dependsOn.EndDate, minDate);
        double depEndX = CalculateXForDayOffset(depDayOffset, pixelsPerDay);
        double depY = depIndex * rowHeight + (rowHeight / 2);

        // Dependent task start (center of start)
        int startDayOffset = GetDayOffset(task.CalculatedStartDate, minDate);
        double startX = CalculateXForDayOffset(startDayOffset, pixelsPerDay);
        double startY = taskIndex * rowHeight + (rowHeight / 2);

        // Define key points
        double rightX = depEndX + pixelsPerDay / 4; // Quarter day right of dependency end
        double aboveY = startY - rowHeight / 2;      // Half row above task bar
        double leftX = startX - pixelsPerDay / 4;    // Quarter day left of task start

        // Path segments
        lines.Add((new Point(depEndX, depY), new Point(rightX, depY), false));        // Right from dependency end (quarter day)
        lines.Add((new Point(rightX, depY), new Point(rightX, aboveY), false));       // Down/Up to above task bar
        lines.Add((new Point(rightX, aboveY), new Point(leftX, aboveY), false));      // Left to quarter day before task
        lines.Add((new Point(leftX, aboveY), new Point(leftX, startY), false));       // Down to task center
        lines.Add((new Point(leftX, startY), new Point(startX, startY), false));      // Right to task start

        // Arrowhead
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
        Log.Information("Starting RenderTasks with {TaskCount} tasks", tasks.Count);
        ganttCanvas.Children.Clear();
        int rowIndex = 0;
        foreach (var task in tasks)
        {
            int dayOffset = GetDayOffset(task.CalculatedStartDate, layout.MinDate);
            double x = CalculateXForDayOffset(dayOffset, layout.PixelsPerDay);
            double y = rowIndex * layout.RowHeight;
            double taskHeight = Math.Min(Math.Max(layout.RowHeight - GanttChartConfig.TaskHeightPadding, 1), GanttChartConfig.TaskBarHeight);
            double centerY = y + (layout.RowHeight / 2);
            double width = task.CalculatedDuration.TotalDays * layout.PixelsPerDay;

            Log.Information("Task: {Name}, X: {X}, Width: {Width}, Y: {Y}, Height: {Height}, IsParent: {IsParent}",
                task.Name, x, width, y, taskHeight, task.IsParent);

            if (task.CalculatedDuration.TotalDays == 0)
            {
                Log.Information("Rendering diamond for {Name}", task.Name);
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
                    Fill = Brushes.Black, // Keep consistent; could differentiate if desired
                    Stroke = task.IsParent ? GetResource<ISolidColorBrush>("ParentTaskBorderBrush") : GetResource<ISolidColorBrush>("TaskBorderBrush"),
                    StrokeThickness = task.IsParent ? GetResource<double>("ParentTaskBorderThickness") : GetResource<double>("TaskBorderThickness"),
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
                Log.Information("Added diamond for {Name}", task.Name);
            }
            else
            {
                Log.Information("Rendering rectangle for {Name}", task.Name);
                var rect = new Rectangle
                {
                    Width = Math.Max(width, 1),
                    Height = taskHeight,
                    Fill = task == selectedTask
                        ? GetResource<ISolidColorBrush>("TaskSelectedBrush")
                        : (task.IsParent ? GetResource<VisualBrush>("ParentTaskDefaultBrush") : GetResource<IBrush>("TaskDefaultBrush")),
                    Stroke = task.IsParent ? GetResource<ISolidColorBrush>("ParentTaskBorderBrush") : GetResource<ISolidColorBrush>("TaskBorderBrush"),
                    StrokeThickness = task.IsParent ? GetResource<double>("ParentTaskBorderThickness") : GetResource<double>("TaskBorderThickness"),
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
                Log.Information("Added rectangle for {Name} at X={X}, Y={Y}", task.Name, x, y);

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
                    Log.Information("Added progress rectangle for {Name}, Width={Width}", task.Name, progressWidth);
                }
            }
            rowIndex++;
        }
        Log.Information("Finished RenderTasks, Children count: {Count}", ganttCanvas.Children.Count);
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

    public void RenderDependencies(Canvas dependencyCanvas, List<TaskItem> tasks, GanttChartLayout layout)
    {
        Log.Information("Starting RenderDependencies with {TaskCount} tasks", tasks.Count);
        dependencyCanvas.Children.Clear(); // Clear only dependency canvas
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
                                dependencyCanvas.Children.Add(arrow);
                                Log.Information("Added dependency arrow from {DepName} to {TaskName}", dependsOn.Name, task.Name);
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
                                dependencyCanvas.Children.Add(line);
                                Log.Information("Added dependency line from {DepName} to {TaskName}", dependsOn.Name, task.Name);
                            }
                        }
                    }
                }
            }
            rowIndex++;
        }
        Log.Information("Finished RenderDependencies, Children count: {Count}", dependencyCanvas.Children.Count);
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