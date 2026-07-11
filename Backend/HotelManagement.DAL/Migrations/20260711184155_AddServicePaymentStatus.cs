using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePaymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicePaymentStatus",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServicePaymentStatus",
                table: "Bookings");
        }
    }
}
