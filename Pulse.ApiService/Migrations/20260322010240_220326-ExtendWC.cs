using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _220326ExtendWC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "WorkCentreMaster",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                defaultValue: "#3b82f6");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkCentreMaster",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkCentreMaster",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "ProcessPercentage",
                table: "ProductionStageMaster",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colour",
                table: "WorkCentreMaster");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkCentreMaster");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkCentreMaster");

            migrationBuilder.DropColumn(
                name: "ProcessPercentage",
                table: "ProductionStageMaster");
        }
    }
}
