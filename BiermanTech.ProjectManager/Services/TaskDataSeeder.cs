using BiermanTech.ProjectManager.Data;
using BiermanTech.ProjectManager.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BiermanTech.ProjectManager;

public class TaskDataSeeder
{
    private readonly ProjectDbContext _dbContext;
    private readonly ITaskRepository _taskRepository;

    public TaskDataSeeder(ProjectDbContext dbContext, ITaskRepository taskRepository)
    {
        _dbContext = dbContext;
        _taskRepository = taskRepository;
    }

    public async Task<Project> SeedSampleDataAsync()
    {
        Log.Information("Starting SeedSampleDataAsync");

        // Load the first project (seeding is handled by UseAsyncSeeding)
        var project = await _dbContext.Projects
            .Include(p => p.Tasks)
            .ThenInclude(t => t.TaskDependencies)
            .Include(p => p.Narrative)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            Log.Error("No project found in the database after seeding. This should not happen.");
            throw new InvalidOperationException("Database seeding failed to create a project.");
        }

        Log.Information("Loaded project: {Name}, Task count: {Count}, Has Narrative: {HasNarrative}",
            project.Name, project.Tasks?.Count ?? 0, project.Narrative != null);

        // Update the repository
        if (project.Tasks != null)
        {
            foreach (var task in project.Tasks)
            {
                _taskRepository.AddTask(task); // Ensure repository is in sync
                Log.Information("Added top-level task to repository: {TaskName}", task.Name);
            }
        }

        return project;
    }
}