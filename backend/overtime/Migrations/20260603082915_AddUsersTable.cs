using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace overtime.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Time");

            migrationBuilder.CreateTable(
                name: "locations",
                schema: "Time",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "Time",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "Time",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "Time",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_ref = table.Column<string>(type: "text", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    customer_email = table.Column<string>(type: "text", nullable: false),
                    customer_phone = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_bookings_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "Time",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookings_locations_location_id",
                        column: x => x.location_id,
                        principalSchema: "Time",
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Time",
                table: "users",
                columns: new[] { "id", "created_at", "full_name", "password_hash", "role", "username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrator", "AQAAAAIAAYagAAAAEH1t3EW6Mm95tC4JrPTupgiQgLocybyVSjgf5CZGpgUN5ghO+ShIKGHH8fS/MSoU7A==", "admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_booking_ref",
                schema: "Time",
                table: "bookings",
                column: "booking_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_location_id",
                schema: "Time",
                table: "bookings",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "uq_employee_slot",
                schema: "Time",
                table: "bookings",
                columns: new[] { "employee_id", "slot_date", "start_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_location_id",
                schema: "Time",
                table: "employees",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_slug",
                schema: "Time",
                table: "locations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "Time",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings",
                schema: "Time");

            migrationBuilder.DropTable(
                name: "users",
                schema: "Time");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "Time");

            migrationBuilder.DropTable(
                name: "locations",
                schema: "Time");
        }
    }
}
