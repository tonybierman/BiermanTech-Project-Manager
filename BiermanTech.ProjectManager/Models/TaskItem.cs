using System;

namespace BiermanTech.ProjectManager.Models;

public class TaskItem
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime EndDate => StartDate + Duration;
    public double PercentComplete { get; set; } // 0 to 100
    public TaskItem? DependsOn { get; set; } // Reference to a task this one depends on
}