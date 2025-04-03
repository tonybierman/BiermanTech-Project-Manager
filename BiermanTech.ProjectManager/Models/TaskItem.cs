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

    // Private backing field for leaf task percent complete
    private double _percentComplete;

    [JsonPropertyName("PercentComplete")]
    public double PercentComplete
    {
        get
        {
            if (IsParent)
            {
                // Calculate average completion of all leaf tasks
                var leafTasks = GetAllLeafTasks();
                if (!leafTasks.Any())
                    return 0;

                // Simple average
                return leafTasks.Average(t => t.PercentComplete);

                // Optional: Weighted average by duration
                // double totalDuration = leafTasks.Sum(t => t.CalculatedDuration.TotalDays);
                // return totalDuration > 0 
                //     ? leafTasks.Sum(t => t.PercentComplete * t.CalculatedDuration.TotalDays) / totalDuration 
                //     : 0;
            }
            return _percentComplete;
        }
        set
        {
            if (!IsParent) // Only allow setting for leaf tasks
                _percentComplete = value;
            // For parent tasks, this is ignored as it's calculated
        }
    }

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

    // Helper method to get all leaf tasks recursively
    private IEnumerable<TaskItem> GetAllLeafTasks()
    {
        if (!IsParent)
            return new List<TaskItem> { this };

        var leafTasks = new List<TaskItem>();
        foreach (var child in Children)
        {
            leafTasks.AddRange(child.GetAllLeafTasks());
        }
        return leafTasks;
    }
}