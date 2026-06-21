using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiQuest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPathLevelProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPathLevelProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsPerfect = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPathLevelProgresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPathLevelProgresses_PathId",
                table: "UserPathLevelProgresses",
                column: "PathId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPathLevelProgresses_PathLevelId",
                table: "UserPathLevelProgresses",
                column: "PathLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPathLevelProgresses_UserId",
                table: "UserPathLevelProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPathLevelProgresses_UserId_PathId_LevelNumber",
                table: "UserPathLevelProgresses",
                columns: new[] { "UserId", "PathId", "LevelNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPathLevelProgresses");
        }
    }
}
