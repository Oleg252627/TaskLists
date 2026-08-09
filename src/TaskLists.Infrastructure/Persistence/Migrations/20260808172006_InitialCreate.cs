using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskLists.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskListShares",
                columns: table => new
                {
                    TaskListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskListShares", x => new { x.TaskListId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TaskListShares_TaskLists_TaskListId",
                        column: x => x.TaskListId,
                        principalTable: "TaskLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_CreatedAtUtc",
                table: "TaskLists",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_OwnerId",
                table: "TaskLists",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskListShares_UserId",
                table: "TaskListShares",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskListShares");

            migrationBuilder.DropTable(
                name: "TaskLists");
        }
    }
}
