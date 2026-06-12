using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace overtime.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWorkingTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shift_end",
                schema: "Time",
                table: "users");

            migrationBuilder.DropColumn(
                name: "shift_start",
                schema: "Time",
                table: "users");

            migrationBuilder.DropColumn(
                name: "work_days",
                schema: "Time",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "working_times",
                schema: "Time",
                table: "users",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "working_times",
                schema: "Time",
                table: "users");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "shift_end",
                schema: "Time",
                table: "users",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(17, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "shift_start",
                schema: "Time",
                table: "users",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(9, 0, 0));

            migrationBuilder.AddColumn<string[]>(
                name: "work_days",
                schema: "Time",
                table: "users",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.InsertData(
                schema: "Time",
                table: "users",
                columns: new[] { "id", "created_at", "first_name", "full_name", "last_name", "location_id", "password_hash", "role", "shift_end", "shift_start", "username", "work_days" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrator", "Administrator", "", null, "AQAAAAIAAYagAAAAEH1t3EW6Mm95tC4JrPTupgiQgLocybyVSjgf5CZGpgUN5ghO+ShIKGHH8fS/MSoU7A==", "admin", new TimeOnly(17, 0, 0), new TimeOnly(9, 0, 0), "admin", new string[0] });
        }
    }
}
