using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.Commands
{
    public interface ICommandFactory
    {
        ICommand CreateAddTaskCommand(TaskItem task, int? parentTaskId = null);
        ICommand CreateDeleteTaskCommand(TaskItem task);
        ICommand CreateEditNarrativeCommand(Project project, ProjectNarrative originalNarrative, ProjectNarrative updatedNarrative);
        ICommand CreateLoadProjectCommand(Project project, string filePath);
        ICommand CreateNewProjectCommand(Project project);
        ICommand CreateSaveProjectCommand(Project project, string filePath);
        ICommand CreateUpdateTaskCommand(TaskItem originalTask, TaskItem updatedTask);
    }
}