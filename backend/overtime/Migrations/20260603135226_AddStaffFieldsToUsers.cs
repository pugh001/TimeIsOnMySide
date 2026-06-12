using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace overtime.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffFieldsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "first_name",
                schema: "Time",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                schema: "Time",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                schema: "Time",
                table: "users",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.UpdateData(
                schema: "Time",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "first_name", "last_name", "location_id", "shift_end", "shift_start", "work_days" },
                values: new object[] { "Administrator", "", null, new TimeOnly(17, 0, 0), new TimeOnly(9, 0, 0), new string[0] });

            migrationBuilder.CreateIndex(
                name: "IX_users_location_id",
                schema: "Time",
                table: "users",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_locations_location_id",
                schema: "Time",
                table: "users",
                column: "location_id",
                principalSchema: "Time",
                principalTable: "locations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_locations_location_id",
                schema: "Time",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_location_id",
                schema: "Time",
                table: "users");

            migrationBuilder.DropColumn(
                name: "first_name",
                schema: "Time",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_name",
                schema: "Time",
                table: "users");

            migrationBuilder.DropColumn(
                name: "location_id",
                schema: "Time",
                table: "users");

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
        }
    }
}
