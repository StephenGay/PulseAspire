using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EquipmentID",
                table: "FactoryZones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerName",
                table: "EquipmentItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EquipmentItemDescription",
                table: "EquipmentItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentItemDescription2",
                table: "EquipmentItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperational",
                table: "EquipmentItems",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNo",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneID",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentID",
                table: "FactoryZones");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "EquipmentItemDescription2",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "IsOperational",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "SerialNo",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "ZoneID",
                table: "EquipmentItems");

            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerName",
                table: "EquipmentItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EquipmentItemDescription",
                table: "EquipmentItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);
        }
    }
}
