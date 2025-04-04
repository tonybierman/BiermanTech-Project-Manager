using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.Commands
{
    public interface ICommandFactory
    {
        ICommand CreateAddTaskCommand(TaskItem task, int? parentTaskId = null);
        ICommand CreateDeleteTaskCommand(TaskItem task);
        ICommand CreateEditNarrativeCommand(Project project, ProjectNarrative originalNarrative, ProjectNarrative updatedNarrative);
        ICommand CreateExportProjectCommand(Project project, string filePath);
        ICommand CreateImportProjectCommand(Project project, string filePath);
        ICommand CreateLoadProjectCommand(Project project, int projectId);
        ICommand CreateNewProjectCommand(Project project);
        ICommand CreateSaveProjectCommand(Project project);
        ICommand CreateUpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask);
    }
}