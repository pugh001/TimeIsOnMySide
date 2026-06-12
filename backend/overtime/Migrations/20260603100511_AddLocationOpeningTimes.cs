using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace overtime.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationOpeningTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "Time",
                table: "locations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "opening_hours",
                schema: "Time",
                table: "locations",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                schema: "Time",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "opening_hours",
                schema: "Time",
                table: "locations");
        }
    }
}
