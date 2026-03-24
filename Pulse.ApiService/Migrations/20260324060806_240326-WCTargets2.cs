using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _240326WCTargets2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TextColour",
                table: "WorkCentreMaster",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                defaultValue: "#000000",
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9,
                oldNullable: true,
                oldDefaultValue: "#ffffff");

            migrationBuilder.AlterColumn<double>(
                name: "TargetMinUnitsPerDay",
                table: "WorkCentreMaster",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "TargetMaxUnitsPerDay",
                table: "WorkCentreMaster",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "WorkCentreMaster",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                defaultValue: "#ffffff",
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9,
                oldNullable: true,
                oldDefaultValue: "#3b82f6");

            migrationBuilder.AlterColumn<bool>(
                name: "ApplyTargets",
                table: "WorkCentreMaster",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<double>(
                name: "TargetMinUnitsPerDay",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "TargetMaxUnitsPerDay",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPlotted",
                table: "FactoryZones",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TextColour",
                table: "WorkCentreMaster",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                defaultValue: "#ffffff",
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9,
                oldNullable: true,
                oldDefaultValue: "#000000");

            migrationBuilder.AlterColumn<double>(
                name: "TargetMinUnitsPerDay",
                table: "WorkCentreMaster",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "TargetMaxUnitsPerDay",
                table: "WorkCentreMaster",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "WorkCentreMaster",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                defaultValue: "#3b82f6",
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9,
                oldNullable: true,
                oldDefaultValue: "#ffffff");

            migrationBuilder.AlterColumn<bool>(
                name: "ApplyTargets",
                table: "WorkCentreMaster",
                type: "bit",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<double>(
                name: "TargetMinUnitsPerDay",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "TargetMaxUnitsPerDay",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPlotted",
                table: "FactoryZones",
                type: "bit",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
