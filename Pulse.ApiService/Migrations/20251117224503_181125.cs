using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _181125 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentCapabilities_ProductionStageMaster_ProductionStageID",
                table: "EquipmentCapabilities");

            migrationBuilder.AddColumn<int>(
                name: "StepNo",
                table: "ProductionPlanItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorksOrderNo",
                table: "ProductionPlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_EquipmentItemID",
                table: "ProductionPlanItems",
                column: "EquipmentItemID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_ProductionStageID",
                table: "ProductionPlanItems",
                column: "ProductionStageID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_WorkOrderNo",
                table: "ProductionPlanItems",
                column: "WorkOrderNo");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_WorksOrderNo",
                table: "ProductionPlanItems",
                column: "WorksOrderNo");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentCapabilities_ProductionStageMaster_ProductionStageID",
                table: "EquipmentCapabilities",
                column: "ProductionStageID",
                principalTable: "ProductionStageMaster",
                principalColumn: "ProductionStageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_EquipmentItems_EquipmentItemID",
                table: "ProductionPlanItems",
                column: "EquipmentItemID",
                principalTable: "EquipmentItems",
                principalColumn: "EquipmentItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_ProductionStageMaster_ProductionStageID",
                table: "ProductionPlanItems",
                column: "ProductionStageID",
                principalTable: "ProductionStageMaster",
                principalColumn: "ProductionStageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_WorksOrder_WorkOrderNo",
                table: "ProductionPlanItems",
                column: "WorkOrderNo",
                principalTable: "WorksOrder",
                principalColumn: "WorksOrderNo",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlanItems_WorksOrder_WorksOrderNo",
                table: "ProductionPlanItems",
                column: "WorksOrderNo",
                principalTable: "WorksOrder",
                principalColumn: "WorksOrderNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentCapabilities_ProductionStageMaster_ProductionStageID",
                table: "EquipmentCapabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_EquipmentItems_EquipmentItemID",
                table: "ProductionPlanItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_ProductionStageMaster_ProductionStageID",
                table: "ProductionPlanItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_WorksOrder_WorkOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlanItems_WorksOrder_WorksOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_EquipmentItemID",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_ProductionStageID",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_WorkOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlanItems_WorksOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropColumn(
                name: "StepNo",
                table: "ProductionPlanItems");

            migrationBuilder.DropColumn(
                name: "WorksOrderNo",
                table: "ProductionPlanItems");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentCapabilities_ProductionStageMaster_ProductionStageID",
                table: "EquipmentCapabilities",
                column: "ProductionStageID",
                principalTable: "ProductionStageMaster",
                principalColumn: "ProductionStageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
