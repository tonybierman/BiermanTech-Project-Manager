using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Models;

public class TaskItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private int _id;
    private string _name;
    private DateTimeOffset? _startDate;
    private TimeSpan? _duration;
    private double _percentComplete;
    private int? _parentId;
    private TaskItem _parent;
    private ObservableCollection<TaskItem> _children; // Changed to ObservableCollection
    private int _projectId;
    private Project _project;
    private List<TaskDependency> _taskDependencies;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value, alsoNotify: new[] { nameof(EndDate), nameof(CalculatedStartDate), nameof(CalculatedDuration) });
    }

    public TimeSpan? Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value, alsoNotify: new[] { nameof(EndDate), nameof(CalculatedDuration) });
    }

    [JsonPropertyName("PercentComplete")]
    public double PercentComplete
    {
        get => _percentComplete;
        set => SetProperty(ref _percentComplete, value);
    }

    public int? ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }

    [ForeignKey("ParentId")]
    [JsonIgnore]
    public TaskItem Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    [JsonPropertyName("Children")]
    public ObservableCollection<TaskItem> Children // Changed to ObservableCollection
    {
        get => _children;
        set => SetProperty(ref _children, value, alsoNotify: new[] { nameof(EndDate), nameof(IsParent), nameof(CalculatedStartDate), nameof(CalculatedDuration) });
    }

    public int ProjectId
    {
        get => _projectId;
        set => SetProperty(ref _projectId, value);
    }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public Project Project
    {
        get => _project;
        set => SetProperty(ref _project, value);
    }

    [JsonIgnore]
    public List<TaskDependency> TaskDependencies
    {
        get => _taskDependencies;
        set => SetProperty(ref _taskDependencies, value, alsoNotify: new[] { nameof(DependsOnIds) });
    }

    [NotMapped]
    [JsonPropertyName("DependsOnIds")]
    public List<int> DependsOnIds => TaskDependencies.Select(td => td.DependsOnId).ToList();

    [NotMapped]
    [JsonIgnore]
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

    public TaskItem()
    {
        _children = new ObservableCollection<TaskItem>();
        _taskDependencies = new List<TaskDependency>();
    }

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null, string[] alsoNotify = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (alsoNotify != null)
            {
                foreach (var additionalProperty in alsoNotify)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(additionalProperty));
                }
            }
        }
    }
}