using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SibersTestSolution.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class _002_AddTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskOwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskPerformerId = table.Column<int>(type: "INTEGER", nullable: true),
                    TaskStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    TaskPriority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Employees_TaskOwnerId",
                        column: x => x.TaskOwnerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Employees_TaskPerformerId",
                        column: x => x.TaskPerformerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskOwnerId",
                table: "Tasks",
                column: "TaskOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskPerformerId",
                table: "Tasks",
                column: "TaskPerformerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
