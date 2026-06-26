using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditingLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "HousekeepingTasks",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "FoodOrders",
                newName: "GeneratedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Bookings",
                newName: "BookedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "FoodOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    PrimaryKey = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ChangedByEmail = table.Column<string>(type: "text", nullable: true),
                    ChangedByName = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "FoodOrders");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "HousekeepingTasks",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "GeneratedAt",
                table: "FoodOrders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "BookedAt",
                table: "Bookings",
                newName: "CreatedAt");
        }
    }
}
