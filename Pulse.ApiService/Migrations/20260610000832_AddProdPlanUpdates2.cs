using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddProdPlanUpdates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ZoneID",
                table: "EquipmentItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "EquipmentItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "EquipmentItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "EquipmentItems",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FactoryZoneId",
                table: "EquipmentItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_FactoryZoneId",
                table: "EquipmentItems",
                column: "FactoryZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_ZoneID",
                table: "EquipmentItems",
                column: "ZoneID");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentItems_FactoryZones_FactoryZoneId",
                table: "EquipmentItems",
                column: "FactoryZoneId",
                principalTable: "FactoryZones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentItems_FactoryZones_ZoneID",
                table: "EquipmentItems",
                column: "ZoneID",
                principalTable: "FactoryZones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentItems_FactoryZones_FactoryZoneId",
                table: "EquipmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentItems_FactoryZones_ZoneID",
                table: "EquipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentItems_FactoryZoneId",
                table: "EquipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentItems_ZoneID",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "FactoryZoneId",
                table: "EquipmentItems");

            migrationBuilder.AlterColumn<string>(
                name: "ZoneID",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "EquipmentItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
        }
    }
}
