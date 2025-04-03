using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BiermanTech.ProjectManager.Models;

public class Project
{
    public string Name { get; set; }
    public string Author { get; set; }
    public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ProjectNarrative Narrative { get; set; }
}