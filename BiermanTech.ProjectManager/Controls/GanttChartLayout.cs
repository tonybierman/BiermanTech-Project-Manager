using BiermanTech.ProjectManager.Controls;
using BiermanTech.ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiermanTech.ProjectManager.Services;

public class GanttChartLayout
{
    public double TaskListWidth { get; }
    public double ChartWidth { get; }
    public double HeaderHeight { get; }
    public double ChartHeight { get; }
    public double TotalDays { get; }
    public double PixelsPerDay { get; }
    public double RowHeight { get; }
    public DateTimeOffset MinDate { get; }

    public GanttChartLayout(List<TaskItem> tasks, double controlWidth, double controlHeight)
    {
        TaskListWidth = GanttChartConfig.TaskListWidth;
        ChartWidth = Math.Max(controlWidth - TaskListWidth, 1);
        HeaderHeight = GanttChartConfig.HeaderHeight;
        ChartHeight = Math.Max(controlHeight - HeaderHeight, 1);
        MinDate = tasks.Min(t => t.StartDate);
        TotalDays = (tasks.Max(t => t.EndDate) - MinDate).TotalDays + 1;
        PixelsPerDay = Math.Max(ChartWidth / TotalDays, GanttChartConfig.MinPixelsPerDay);
        RowHeight = Math.Max(ChartHeight / tasks.Count, GanttChartConfig.MinRowHeight);
    }
}