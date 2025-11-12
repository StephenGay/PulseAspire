using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0811251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LockToProductCode",
                table: "WorkTypeMaster",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoicedDate",
                table: "WorksOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionStageID",
                table: "WorksOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProductionStageName",
                table: "WorksOrder",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProgressChange",
                table: "WorksOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressComment",
                table: "WorksOrder",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequiredDate",
                table: "WorksOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductionStageMaster",
                columns: table => new
                {
                    ProductionStageId = table.Column<int>(type: "int", nullable: false),
                    ProductionStageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    WorkTypeID = table.Column<int>(type: "int", nullable: false),
                    StepNo = table.Column<int>(type: "int", nullable: false),
                    HasMaterial = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResultsPage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequiresSignOff = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresPlanning = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WorkTypeID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStageMaster", x => x.ProductionStageId);
                    table.ForeignKey(
                        name: "FK_ProductionStageMaster_DivisionMaster_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                        column: x => x.WorkTypeID,
                        principalTable: "WorkTypeMaster",
                        principalColumn: "WorkTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID1",
                        column: x => x.WorkTypeID1,
                        principalTable: "WorkTypeMaster",
                        principalColumn: "WorkTypeID");
                });

            migrationBuilder.CreateTable(
                name: "WorkCentreMaster",
                columns: table => new
                {
                    WorkCentreId = table.Column<int>(type: "int", nullable: false),
                    WorkCentreName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    BranchID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCentreMaster", x => x.WorkCentreId);
                    table.ForeignKey(
                        name: "FK_WorkCentreMaster_BranchMaster_BranchID",
                        column: x => x.BranchID,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkCentreMaster_DivisionMaster_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "WorkCentreFunctionsMapping",
                columns: table => new
                {
                    WorkCentreID = table.Column<int>(type: "int", nullable: false),
                    ProductionStageID = table.Column<int>(type: "int", nullable: false)
                    
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCentreFunctionsMapping", x => new { x.WorkCentreID, x.ProductionStageID });
                    table.ForeignKey(
                        name: "FK_WorkCentreFunctionsMapping_ProductionStageMaster_ProductionStageID",
                        column: x => x.ProductionStageID,
                        principalTable: "ProductionStageMaster",
                        principalColumn: "ProductionStageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkCentreFunctionsMapping_WorkCentreMaster_WorkCentreID",
                        column: x => x.WorkCentreID,
                        principalTable: "WorkCentreMaster",
                        principalColumn: "WorkCentreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_ProductionStageID",
                table: "WorksOrder",
                column: "ProductionStageID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_DivisionID",
                table: "ProductionStageMaster",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_WorkTypeID",
                table: "ProductionStageMaster",
                column: "WorkTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_WorkTypeID1",
                table: "ProductionStageMaster",
                column: "WorkTypeID1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCentreFunctionsMapping_ProductionStageID",
                table: "WorkCentreFunctionsMapping",
                column: "ProductionStageID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCentreMaster_BranchID",
                table: "WorkCentreMaster",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCentreMaster_DivisionID",
                table: "WorkCentreMaster",
                column: "DivisionID");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorksOrder_ProductionStageMaster_ProductionStageID",
            //    table: "WorksOrder",
            //    column: "ProductionStageID",
            //    principalTable: "ProductionStageMaster",
            //    principalColumn: "ProductionStageId",
            //    onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_ProductionStageMaster_ProductionStageID",
                table: "WorksOrder");

            migrationBuilder.DropTable(
                name: "WorkCentreFunctionsMapping");

            migrationBuilder.DropTable(
                name: "ProductionStageMaster");

            migrationBuilder.DropTable(
                name: "WorkCentreMaster");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_ProductionStageID",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "InvoicedDate",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "ProductionStageID",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "ProductionStageName",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "ProgressChange",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "ProgressComment",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "RequiredDate",
                table: "WorksOrder");

            migrationBuilder.AlterColumn<string>(
                name: "LockToProductCode",
                table: "WorkTypeMaster",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);
        }
    }
}
