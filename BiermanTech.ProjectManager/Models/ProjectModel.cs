using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BiermanTech.ProjectManager.Models;

public class Project
{
    public int Id { get; set; } // Added primary key for EF Core
    public string Name { get; set; }
    public string Author { get; set; }
    public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>(); // One-to-many with TaskItem

    [ForeignKey("ProjectNarrativeId")]
    public ProjectNarrative Narrative { get; set; } // One-to-one with ProjectNarrative
    public int? ProjectNarrativeId { get; set; } // Foreign key for Narrative (optional)
}