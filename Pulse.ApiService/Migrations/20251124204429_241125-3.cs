using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2411253 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIAutoModel",
                table: "UserSettingsMaster");

            migrationBuilder.DropColumn(
                name: "AIDefaultModel",
                table: "UserSettingsMaster");

            migrationBuilder.AddColumn<int>(
                name: "AIDefaultPref",
                table: "UserSettingsMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIDefaultPref",
                table: "UserSettingsMaster");

            migrationBuilder.AddColumn<bool>(
                name: "AIAutoModel",
                table: "UserSettingsMaster",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "AIDefaultModel",
                table: "UserSettingsMaster",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
