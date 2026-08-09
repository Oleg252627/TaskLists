using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskLists.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskListShares_UserId",
                table: "TaskListShares");

            migrationBuilder.CreateIndex(
                name: "IX_TaskListShares_UserId_TaskListId",
                table: "TaskListShares",
                columns: new[] { "UserId", "TaskListId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_OwnerId_CreatedAtUtc_Id",
                table: "TaskLists",
                columns: new[] { "OwnerId", "CreatedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskListShares_UserId_TaskListId",
                table: "TaskListShares");

            migrationBuilder.DropIndex(
                name: "IX_TaskLists_OwnerId_CreatedAtUtc_Id",
                table: "TaskLists");

            migrationBuilder.CreateIndex(
                name: "IX_TaskListShares_UserId",
                table: "TaskListShares",
                column: "UserId");
        }
    }
}
