namespace BiermanTech.ProjectManager.Controls;

public static class GanttChartConfig
{
    public const double TaskListWidth = 330; // Updated to match total column width (150 + 100 + 80)
    public const double HeaderHeight = 40;
    public const double MinPixelsPerDay = 10;
    public const double MinRowHeight = 30;
    public const double MonthRowHeight = 20;
    public const double TaskBarHeight = 20;

    public const double DependencyLineOffset = 10;    // Horizontal offset for dependency lines
    public const double ArrowSize = 5;               // Size of dependency arrow heads
    public const double MonthTextTopOffset = 5;      // Vertical offset for month text
    public const double DayTextTopOffset = 5;        // Vertical offset for day text
    public const double DayTextVerticalPosition = MonthRowHeight + DayTextTopOffset; // Position for day text
    public const double DependencyLineTurnLength = 10; // Length of dependency line turns
    public const double TaskHeightPadding = 10;      // Padding used for task rectangle height
}