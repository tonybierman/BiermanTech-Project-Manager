using BiermanTech.ProjectManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace BiermanTech.ProjectManager.Data;

public class ProjectDbContextFactory : IDesignTimeDbContextFactory<ProjectDbContext>
{
    public ProjectDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "tasks.db");
        var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new ProjectDbContext(optionsBuilder.Options);
    }
}