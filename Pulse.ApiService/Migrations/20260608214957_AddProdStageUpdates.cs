using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddProdStageUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkCentreID",
                table: "ProductionStageMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_WorkCentreID",
                table: "ProductionStageMaster",
                column: "WorkCentreID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_WorkCentreMaster_WorkCentreID",
                table: "ProductionStageMaster",
                column: "WorkCentreID",
                principalTable: "WorkCentreMaster",
                principalColumn: "WorkCentreId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_WorkCentreMaster_WorkCentreID",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_WorkCentreID",
                table: "ProductionStageMaster");

            migrationBuilder.DropColumn(
                name: "WorkCentreID",
                table: "ProductionStageMaster");
        }
    }
}
