using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Services;

public class GanttChartRenderer
{
    public (double X, double Width, double Y) CalculateTaskPosition(TaskItem task, DateTimeOffset minDate, double pixelsPerDay, double rowHeight, int rowIndex)
    {
        double x = (task.StartDate - minDate).TotalDays * pixelsPerDay;
        double width = task.Duration.TotalDays * pixelsPerDay;
        double y = rowIndex * rowHeight;
        return (x, width, y);
    }

    public (double X, double TodayX) CalculateTodayLine(DateTimeOffset today, DateTimeOffset minDate, DateTimeOffset maxDate, double pixelsPerDay)
    {
        double todayX = (today - minDate).TotalDays * pixelsPerDay;
        bool isVisible = today >= minDate && today <= maxDate;
        return (isVisible ? todayX : -1, todayX);
    }

    public List<(Point Start, Point End, bool IsArrow)> CalculateDependencyLines(TaskItem task, TaskItem dependsOn, DateTimeOffset minDate, double pixelsPerDay, double rowHeight, int taskIndex, int depIndex)
    {
        var lines = new List<(Point Start, Point End, bool IsArrow)>();
        double depEndX = (dependsOn.EndDate - minDate).TotalDays * pixelsPerDay;
        double depY = depIndex * rowHeight + (rowHeight / 2);
        double startX = (task.StartDate - minDate).TotalDays * pixelsPerDay;
        double startY = taskIndex * rowHeight + (rowHeight / 2);

        lines.Add((new Point(depEndX, depY), new Point(depEndX + 10, depY), false));
        lines.Add((new Point(depEndX + 10, depY), new Point(depEndX + 10, startY), false));
        lines.Add((new Point(depEndX + 10, startY), new Point(startX, startY), false));
        lines.Add((new Point(startX, startY), new Point(startX - 5, startY - 5), true));
        lines.Add((new Point(startX, startY), new Point(startX - 5, startY + 5), true));

        return lines;
    }

    public void RenderHeader(Canvas headerCanvas, List<TaskItem> tasks, GanttChartLayout layout)
    {
        headerCanvas.Children.Clear();
        double monthRowHeight = 20;
        double dayTextTop = monthRowHeight;
        string lastMonthDisplayed = null;

        for (int day = 0; day <= layout.TotalDays; day++)
        {
            DateTimeOffset date = layout.MinDate.AddDays(day);
            double x = day * layout.PixelsPerDay;

            var line = new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, layout.HeaderHeight),
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            headerCanvas.Children.Add(line);

            if (date.Day == 1 || day == 0)
            {
                string monthName = date.ToString("MMMM");
                if (monthName != lastMonthDisplayed)
                {
                    var monthText = new TextBlock
                    {
                        Text = monthName,
                        Foreground = Brushes.Black,
                        [Canvas.LeftProperty] = x + 2,
                        [Canvas.TopProperty] = 5,
                        FontSize = 12,
                        FontWeight = FontWeight.Bold
                    };
                    headerCanvas.Children.Add(monthText);
                    lastMonthDisplayed = monthName;
                }
            }

            var dayText = new TextBlock
            {
                Text = date.ToString("dd"),
                Foreground = Brushes.Black,
                [Canvas.LeftProperty] = x + 2,
                [Canvas.TopProperty] = dayTextTop + 5,
                FontSize = 10
            };
            headerCanvas.Children.Add(dayText);
        }
    }

    public void RenderTasks(Canvas ganttCanvas, List<TaskItem> tasks, TaskItem selectedTask, GanttChartLayout layout, Action<TaskItem> onTaskSelected)
    {
        ganttCanvas.Children.Clear();
        int rowIndex = 0;
        foreach (var task in tasks)
        {
            var (x, width, y) = CalculateTaskPosition(task, layout.MinDate, layout.PixelsPerDay, layout.RowHeight, rowIndex);

            var rect = new Rectangle
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(layout.RowHeight - 10, 1),
                Fill = task == selectedTask ? Brushes.Yellow : Brushes.LightBlue,
                [Canvas.LeftProperty] = x,
                [Canvas.TopProperty] = y + 5,
                Tag = task
            };

            rect.PointerPressed += (s, e) =>
            {
                if (s is Rectangle r && r.Tag is TaskItem clickedTask)
                {
                    onTaskSelected(clickedTask);
                }
            };

            if (task.PercentComplete > 0)
            {
                double progressWidth = (task.PercentComplete / 100) * width;
                var progressRect = new Rectangle
                {
                    Width = Math.Max(progressWidth, 1),
                    Height = Math.Max(layout.RowHeight - 10, 1),
                    Fill = Brushes.Blue,
                    [Canvas.LeftProperty] = x,
                    [Canvas.TopProperty] = y + 5
                };
                ganttCanvas.Children.Add(progressRect);
            }

            ganttCanvas.Children.Add(rect);
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
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 }
            };
            ganttCanvas.Children.Add(todayLine);
        }
    }

    public void RenderDependencies(Canvas ganttCanvas, List<TaskItem> tasks, GanttChartLayout layout)
    {
        int rowIndex = 0;
        foreach (var task in tasks)
        {
            if (task.DependsOn != null)
            {
                int depIndex = tasks.IndexOf(task.DependsOn);
                var lines = CalculateDependencyLines(task, task.DependsOn, layout.MinDate, layout.PixelsPerDay, layout.RowHeight, rowIndex, depIndex);

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
                        ganttCanvas.Children.Add(arrow);
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
                        ganttCanvas.Children.Add(line);
                    }
                }
            }
            rowIndex++;
        }
    }
}