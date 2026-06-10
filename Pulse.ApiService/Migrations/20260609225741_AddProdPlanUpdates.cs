using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddProdPlanUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_WorksOrder_WorksOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_WorksOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropColumn(
                name: "WorksOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ZoneId",
                table: "ProductionPlanItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_WorkCentreID",
                table: "ProductionPlanItems",
                column: "WorkCentreID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_ZoneId",
                table: "ProductionPlanItems",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_FactoryZones_ZoneId",
                table: "ProductionPlanItems",
                column: "ZoneId",
                principalTable: "FactoryZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_WorkCentreMaster_WorkCentreID",
                table: "ProductionPlanItems",
                column: "WorkCentreID",
                principalTable: "WorkCentreMaster",
                principalColumn: "WorkCentreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_FactoryZones_ZoneId",
                table: "ProductionPlanItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_WorkCentreMaster_WorkCentreID",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_WorkCentreID",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_ZoneId",
                table: "ProductionPlanItems");

            migrationBuilder.AlterColumn<string>(
                name: "ZoneId",
                table: "ProductionPlanItems",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorksOrderNo",
                table: "ProductionPlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_WorksOrderNo",
                table: "ProductionPlanItems",
                column: "WorksOrderNo");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_WorksOrder_WorksOrderNo",
                table: "ProductionPlanItems",
                column: "WorksOrderNo",
                principalTable: "WorksOrder",
                principalColumn: "WorksOrderNo");
        }
    }
}
