using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEvent.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatCapacityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "SeatTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "SeatTypes");
        }
    }
}
