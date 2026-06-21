using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class ZoneUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBehindSchedule",
                table: "FactoryZones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WorkOrdersComplete",
                table: "FactoryZones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkOrdersPlanned",
                table: "FactoryZones",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBehindSchedule",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "WorkOrdersComplete",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "WorkOrdersPlanned",
                table: "FactoryZones");
        }
    }
}
