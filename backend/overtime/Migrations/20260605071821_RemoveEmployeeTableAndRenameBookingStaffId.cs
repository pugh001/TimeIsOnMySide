using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace overtime.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeTableAndRenameBookingStaffId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_employees_employee_id",
                schema: "Time",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "Time");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                schema: "Time",
                table: "bookings",
                newName: "staff_id");

            migrationBuilder.RenameIndex(
                name: "uq_employee_slot",
                schema: "Time",
                table: "bookings",
                newName: "uq_staff_slot");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_users_staff_id",
                schema: "Time",
                table: "bookings",
                column: "staff_id",
                principalSchema: "Time",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_users_staff_id",
                schema: "Time",
                table: "bookings");

            migrationBuilder.RenameColumn(
                name: "staff_id",
                schema: "Time",
                table: "bookings",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "uq_staff_slot",
                schema: "Time",
                table: "bookings",
                newName: "uq_employee_slot");

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "Time",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_employees_locations_location_id",
                        column: x => x.location_id,
                        principalSchema: "Time",
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employees_location_id",
                schema: "Time",
                table: "employees",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_employees_employee_id",
                schema: "Time",
                table: "bookings",
                column: "employee_id",
                principalSchema: "Time",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
