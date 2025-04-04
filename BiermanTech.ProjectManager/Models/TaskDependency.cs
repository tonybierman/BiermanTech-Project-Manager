using System.ComponentModel.DataAnnotations.Schema;

namespace BiermanTech.ProjectManager.Models;

public class TaskDependency
{
    public int TaskId { get; set; } // Foreign key to the task that has the dependency
    [ForeignKey("TaskId")]
    public TaskItem Task { get; set; }

    public int DependsOnId { get; set; } // Foreign key to the task depended on
    [ForeignKey("DependsOnId")]
    public TaskItem DependsOnTask { get; set; }
}