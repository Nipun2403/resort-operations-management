using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "HousekeepingTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_AssignedToUserId",
                table: "MaintenanceTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId",
                table: "HousekeepingTasks",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HousekeepingTasks_Users_AssignedToUserId",
                table: "HousekeepingTasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceTasks_Users_AssignedToUserId",
                table: "MaintenanceTasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HousekeepingTasks_Users_AssignedToUserId",
                table: "HousekeepingTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceTasks_Users_AssignedToUserId",
                table: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_AssignedToUserId",
                table: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "HousekeepingTasks");
        }
    }
}
