using Avalonia;
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
}