using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiermanTech.ProjectManager.Models;

public class TaskItem
{
    public int Id { get; set; } // EF Core PK, JSON-compatible

    public string Name { get; set; } // JSON-compatible

    public DateTimeOffset? StartDate { get; set; } // JSON-compatible

    public TimeSpan? Duration { get; set; } // JSON-compatible

    [JsonPropertyName("PercentComplete")] // Retain JSON naming
    public double PercentComplete { get; set; } // EF Core and JSON-compatible

    // EF Core parent-child relationship
    public int? ParentId { get; set; }
    [ForeignKey("ParentId")]
    [JsonIgnore] // Exclude from JSON to avoid circular reference
    public TaskItem Parent { get; set; }

    [JsonPropertyName("Children")] // Explicitly name for JSON
    public List<TaskItem> Children { get; set; } = new List<TaskItem>(); // JSON-compatible, EF Core navigation

    // EF Core project relationship
    public int ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    [JsonIgnore] // Exclude from JSON to avoid circular reference
    public Project Project { get; set; }

    // EF Core many-to-many relationship via TaskDependency
    [JsonIgnore] // Exclude from JSON serialization
    public List<TaskDependency> TaskDependencies { get; set; } = new List<TaskDependency>(); // Navigation property

    [NotMapped] // Computed for JSON, not stored in DB
    [JsonPropertyName("DependsOnIds")] // Matches original JSON structure
    public List<int> DependsOnIds => TaskDependencies.Select(td => td.DependsOnId).ToList();

    [NotMapped] // Runtime-only, not stored in DB
    [JsonIgnore] // Retain original JSON exclusion
    public List<TaskItem> DependsOn { get; set; } = new List<TaskItem>();

    [NotMapped]
    [JsonIgnore]
    public DateTimeOffset EndDate => StartDate.HasValue && Duration.HasValue
        ? StartDate.Value + Duration.Value
        : Children.Any()
            ? Children.Max(t => t.EndDate)
            : DateTimeOffset.MinValue;

    [NotMapped]
    [JsonIgnore]
    public bool IsParent => Children.Any();

    [NotMapped]
    [JsonIgnore]
    public DateTimeOffset CalculatedStartDate => IsParent
        ? Children.Min(t => t.CalculatedStartDate)
        : StartDate ?? DateTimeOffset.MinValue;

    [NotMapped]
    [JsonIgnore]
    public TimeSpan CalculatedDuration => IsParent
        ? Children.Max(t => t.EndDate) - Children.Min(t => t.CalculatedStartDate)
        : Duration ?? TimeSpan.Zero;
}