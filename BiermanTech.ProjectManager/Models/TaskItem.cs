using System;
using System.Text.Json.Serialization;

namespace BiermanTech.ProjectManager.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public TimeSpan Duration { get; set; }

    [JsonIgnore]
    public DateTimeOffset EndDate => StartDate + Duration;

    public double PercentComplete { get; set; }

    [JsonIgnore]
    public TaskItem? DependsOn { get; set; }

    public Guid? DependsOnId { get; set; }
}