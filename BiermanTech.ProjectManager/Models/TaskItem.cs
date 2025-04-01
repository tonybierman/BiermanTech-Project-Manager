using System;

namespace BiermanTech.ProjectManager.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTimeOffset EndDate => StartDate + Duration;
    public double PercentComplete { get; set; }
    public TaskItem? DependsOn { get; set; }
}