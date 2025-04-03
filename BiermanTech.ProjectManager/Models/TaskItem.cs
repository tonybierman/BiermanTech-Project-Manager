using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BiermanTech.ProjectManager.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }

    // For leaf tasks: manually set; for parent tasks: calculated
    public DateTimeOffset? StartDate { get; set; }
    public TimeSpan? Duration { get; set; }

    [JsonIgnore]
    public DateTimeOffset EndDate => StartDate.HasValue && Duration.HasValue
        ? StartDate.Value + Duration.Value
        : Children.Any()
            ? Children.Max(t => t.EndDate)
            : DateTimeOffset.MinValue;

    public double PercentComplete { get; set; }

    // Child tasks for hierarchical structure
    public List<TaskItem> Children { get; set; } = new List<TaskItem>();

    // Multiple dependencies
    public List<Guid> DependsOnIds { get; set; } = new List<Guid>();

    [JsonIgnore]
    public List<TaskItem> DependsOn { get; set; } = new List<TaskItem>();

    // Calculated properties for parent tasks
    [JsonIgnore]
    public bool IsParent => Children.Any();

    [JsonIgnore]
    public DateTimeOffset CalculatedStartDate => IsParent
        ? Children.Min(t => t.CalculatedStartDate)
        : StartDate ?? DateTimeOffset.MinValue;

    [JsonIgnore]
    public TimeSpan CalculatedDuration => IsParent
        ? Children.Max(t => t.EndDate) - Children.Min(t => t.CalculatedStartDate)
        : Duration ?? TimeSpan.Zero;
}