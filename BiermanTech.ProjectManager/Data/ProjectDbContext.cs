using Microsoft.EntityFrameworkCore;
using BiermanTech.ProjectManager.Models;

namespace BiermanTech.ProjectManager.Data;

public class ProjectDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<ProjectNarrative> ProjectNarratives { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Narrative)
            .WithOne(n => n.Project)
            .HasForeignKey<Project>(p => p.ProjectNarrativeId);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Parent)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure many-to-many relationship via TaskDependency
        modelBuilder.Entity<TaskDependency>()
            .HasKey(td => new { td.TaskId, td.DependsOnId }); // Composite primary key

        modelBuilder.Entity<TaskDependency>()
            .HasOne(td => td.Task)
            .WithMany(t => t.TaskDependencies)
            .HasForeignKey(td => td.TaskId)
            .OnDelete(DeleteBehavior.Cascade); // Remove dependencies when task is deleted

        modelBuilder.Entity<TaskDependency>()
            .HasOne(td => td.DependsOnTask)
            .WithMany()
            .HasForeignKey(td => td.DependsOnId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if depended on
    }
}