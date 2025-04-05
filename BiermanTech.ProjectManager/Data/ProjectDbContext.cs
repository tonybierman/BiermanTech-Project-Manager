using BiermanTech.ProjectManager.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiermanTech.ProjectManager.Data;

public class ProjectDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<ProjectNarrative> Narratives { get; set; }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseSeeding((context, _) =>
        {
            ProjectDbContext dbContext = (ProjectDbContext)context;

            // Project 1
            var project1 = new Project
            {
                Name = "Sample Project 1",
                Author = "BiermanTech Team",
                Narrative = new ProjectNarrative
                {
                    Situation = "This is sample project 1 to demonstrate the project manager.",
                    CurrentState = "Initial planning phase.",
                    Plan = "Develop a project plan, assign tasks, and track progress.",
                    Results = "Not yet completed."
                }
            };

            var task1 = new TaskItem
            {
                Name = "Define Requirements",
                StartDate = DateTimeOffset.Now,
                Duration = TimeSpan.FromDays(3),
                PercentComplete = 50,
                Project = project1
            };

            var task2 = new TaskItem
            {
                Name = "Design System",
                StartDate = DateTimeOffset.Now.AddDays(3),
                Duration = TimeSpan.FromDays(5),
                PercentComplete = 0,
                Project = project1
            };

            var task3 = new TaskItem
            {
                Name = "Implement Features",
                StartDate = DateTimeOffset.Now.AddDays(8),
                Duration = TimeSpan.FromDays(10),
                PercentComplete = 0,
                Project = project1
            };

            project1.Tasks = new ObservableCollection<TaskItem> { task1, task2, task3 };

            // Project 2
            var project2 = new Project
            {
                Name = "Sample Project 2",
                Author = "BiermanTech Team",
                Narrative = new ProjectNarrative
                {
                    Situation = "This is sample project 2 to demonstrate the project manager.",
                    CurrentState = "Development phase.",
                    Plan = "Implement features and test.",
                    Results = "In progress."
                }
            };

            var task4 = new TaskItem
            {
                Name = "Setup Environment",
                StartDate = DateTimeOffset.Now,
                Duration = TimeSpan.FromDays(2),
                PercentComplete = 80,
                Project = project2
            };

            var task5 = new TaskItem
            {
                Name = "Develop API",
                StartDate = DateTimeOffset.Now.AddDays(2),
                Duration = TimeSpan.FromDays(7),
                PercentComplete = 20,
                Project = project2
            };

            project2.Tasks = new ObservableCollection<TaskItem> { task4, task5 };

            dbContext.Projects.AddRange(project1, project2);
            dbContext.SaveChanges();

            var task2Dependency = new TaskDependency
            {
                TaskId = task2.Id,
                DependsOnId = task1.Id
            };

            var task3Dependency = new TaskDependency
            {
                TaskId = task3.Id,
                DependsOnId = task2.Id
            };

            task2.TaskDependencies = new List<TaskDependency> { task2Dependency };
            task3.TaskDependencies = new List<TaskDependency> { task3Dependency };

            dbContext.SaveChanges();
        });

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