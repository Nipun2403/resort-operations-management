using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HousekeepingTasks_Rooms_RoomId",
                table: "HousekeepingTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceTasks_Rooms_RoomId",
                table: "MaintenanceTasks");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "MaintenanceTasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "HousekeepingTasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "HousekeepingTasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HousekeepingTasks_Rooms_RoomId",
                table: "HousekeepingTasks",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceTasks_Rooms_RoomId",
                table: "MaintenanceTasks",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HousekeepingTasks_Rooms_RoomId",
                table: "HousekeepingTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceTasks_Rooms_RoomId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "HousekeepingTasks");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "HousekeepingTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HousekeepingTasks_Rooms_RoomId",
                table: "HousekeepingTasks",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceTasks_Rooms_RoomId",
                table: "MaintenanceTasks",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
