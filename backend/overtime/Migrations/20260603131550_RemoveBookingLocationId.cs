using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace overtime.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBookingLocationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_locations_location_id",
                schema: "Time",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "IX_bookings_location_id",
                schema: "Time",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "location_id",
                schema: "Time",
                table: "bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                schema: "Time",
                table: "bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_bookings_location_id",
                schema: "Time",
                table: "bookings",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_locations_location_id",
                schema: "Time",
                table: "bookings",
                column: "location_id",
                principalSchema: "Time",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
