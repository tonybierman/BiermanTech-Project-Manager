using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.Services;

public record TaskAdded(TaskItem Task);
public record TaskUpdated(TaskItem Task);
public record TaskDeleted(TaskItem Task);