using BiermanTech.ProjectManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace BiermanTech.ProjectManager.Data;

public class ProjectDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<ProjectNarrative> Narratives { get; set; }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<TaskDependency>()
            .HasKey(td => new { td.TaskId, td.DependsOnId });

        modelBuilder.Entity<TaskDependency>()
            .HasOne(td => td.DependsOnTask)
            .WithMany()
            .HasForeignKey(td => td.DependsOnId);

        modelBuilder.Entity<TaskDependency>()
            .HasOne(td => td.Task)
            .WithMany(t => t.TaskDependencies)
            .HasForeignKey(td => td.TaskId);

        modelBuilder.Entity<ProjectNarrative>()
            .HasOne(n => n.Project)
            .WithOne(p => p.Narrative)
            .HasForeignKey<ProjectNarrative>(n => n.ProjectId);
    }
}