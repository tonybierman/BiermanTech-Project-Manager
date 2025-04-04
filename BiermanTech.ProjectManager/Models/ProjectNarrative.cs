using System.Text.Json.Serialization;

namespace BiermanTech.ProjectManager.Models;

public class ProjectNarrative
{
    public int Id { get; set; } // Primary key for EF Core
    public int ProjectId { get; set; } // Foreign key to Project
    public string Situation { get; set; }
    public string CurrentState { get; set; }
    public string Plan { get; set; }
    public string Results { get; set; }

    [JsonIgnore] // Prevent circular reference in JSON serialization
    public Project Project { get; set; } // Navigation property back to Project
}