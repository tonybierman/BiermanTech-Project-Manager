using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BiermanTech.ProjectManager.Models;

public class Project
{
    public string Name { get; set; }
    public string Author { get; set; }

    [JsonPropertyName("Tasks")]
    public List<TaskItem> TaskItems { get; set; } = new List<TaskItem>();

    [JsonPropertyName("Narrative")]
    public ProjectNarrative Narrative { get; set; } = new ProjectNarrative();
}