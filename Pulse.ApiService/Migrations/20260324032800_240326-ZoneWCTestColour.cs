using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _240326ZoneWCTestColour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "FactoryZones",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "#ffffff");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "FactoryZones");
        }
    }
}
