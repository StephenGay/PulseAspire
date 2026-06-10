using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddProdPlanUpdates3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentItems_FactoryZones_FactoryZoneId",
                table: "EquipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentItems_FactoryZoneId",
                table: "EquipmentItems");

            migrationBuilder.DropColumn(
                name: "FactoryZoneId",
                table: "EquipmentItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FactoryZoneId",
                table: "EquipmentItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_FactoryZoneId",
                table: "EquipmentItems",
                column: "FactoryZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentItems_FactoryZones_FactoryZoneId",
                table: "EquipmentItems",
                column: "FactoryZoneId",
                principalTable: "FactoryZones",
                principalColumn: "Id");
        }
    }
}
