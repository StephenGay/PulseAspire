using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _240326WCTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyTargets",
                table: "WorkCentreMaster",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TargetMaxUnitsPerDay",
                table: "WorkCentreMaster",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TargetMinUnitsPerDay",
                table: "WorkCentreMaster",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "Y",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "X",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlotted",
                table: "FactoryZones",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TargetMaxUnitsPerDay",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TargetMinUnitsPerDay",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyTargets",
                table: "WorkCentreMaster");

            migrationBuilder.DropColumn(
                name: "TargetMaxUnitsPerDay",
                table: "WorkCentreMaster");

            migrationBuilder.DropColumn(
                name: "TargetMinUnitsPerDay",
                table: "WorkCentreMaster");

            migrationBuilder.DropColumn(
                name: "IsPlotted",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "TargetMaxUnitsPerDay",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "TargetMinUnitsPerDay",
                table: "FactoryZones");

            migrationBuilder.AlterColumn<double>(
                name: "Y",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4,
                oldDefaultValue: 5.0);

            migrationBuilder.AlterColumn<double>(
                name: "X",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4,
                oldDefaultValue: 5.0);
        }
    }
}
