using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFactoryZonesAndEquipCapabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Y",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "X",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Width",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 150.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "FactoryZones",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FactoryZones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<double>(
                name: "Height",
                table: "FactoryZones",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 100.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "FactoryZones",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "#3b82f6",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "EquipmentCapabilityID",
                table: "FactoryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "FactoryZones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParentZoneId",
                table: "FactoryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkCentreID",
                table: "FactoryZones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactoryZoneId",
                table: "EquipmentCapabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactoryZones_WorkCentreID",
                table: "FactoryZones",
                column: "WorkCentreID");

            migrationBuilder.AddForeignKey(
                name: "FK_FactoryZones_WorkCentreMaster_WorkCentreID",
                table: "FactoryZones",
                column: "WorkCentreID",
                principalTable: "WorkCentreMaster",
                principalColumn: "WorkCentreId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FactoryZones_WorkCentreMaster_WorkCentreID",
                table: "FactoryZones");

            migrationBuilder.DropIndex(
                name: "IX_FactoryZones_WorkCentreID",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "EquipmentCapabilityID",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "ParentZoneId",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "WorkCentreID",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "FactoryZoneId",
                table: "EquipmentCapabilities");

            migrationBuilder.AlterColumn<double>(
                name: "Y",
                table: "FactoryZones",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "X",
                table: "FactoryZones",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "Width",
                table: "FactoryZones",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4,
                oldDefaultValue: 150.0);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "FactoryZones",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FactoryZones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<double>(
                name: "Height",
                table: "FactoryZones",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4,
                oldDefaultValue: 100.0);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "FactoryZones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9,
                oldDefaultValue: "#3b82f6");
        }
    }
}
