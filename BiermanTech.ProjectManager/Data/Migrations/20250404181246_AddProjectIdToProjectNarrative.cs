using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiermanTech.ProjectManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIdToProjectNarrative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectNarratives_ProjectNarrativeId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskDependencies_Tasks_DependsOnId",
                table: "TaskDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_ParentId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectNarrativeId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectNarratives",
                table: "ProjectNarratives");

            migrationBuilder.RenameTable(
                name: "ProjectNarratives",
                newName: "Narratives");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Narratives",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Narratives",
                table: "Narratives",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Narratives_ProjectId",
                table: "Narratives",
                column: "ProjectId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Narratives_Projects_ProjectId",
                table: "Narratives",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskDependencies_Tasks_DependsOnId",
                table: "TaskDependencies",
                column: "DependsOnId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_ParentId",
                table: "Tasks",
                column: "ParentId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Narratives_Projects_ProjectId",
                table: "Narratives");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskDependencies_Tasks_DependsOnId",
                table: "TaskDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_ParentId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Narratives",
                table: "Narratives");

            migrationBuilder.DropIndex(
                name: "IX_Narratives_ProjectId",
                table: "Narratives");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Narratives");

            migrationBuilder.RenameTable(
                name: "Narratives",
                newName: "ProjectNarratives");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectNarratives",
                table: "ProjectNarratives",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectNarrativeId",
                table: "Projects",
                column: "ProjectNarrativeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectNarratives_ProjectNarrativeId",
                table: "Projects",
                column: "ProjectNarrativeId",
                principalTable: "ProjectNarratives",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskDependencies_Tasks_DependsOnId",
                table: "TaskDependencies",
                column: "DependsOnId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_ParentId",
                table: "Tasks",
                column: "ParentId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
