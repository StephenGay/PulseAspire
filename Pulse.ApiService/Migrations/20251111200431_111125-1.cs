using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _1111251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WorkCentreId",
                table: "WorkCentreFunctionsMapping",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaseMinutesAtStage",
                table: "ProductionStageMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseValue",
                table: "ProductionStageMaster",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyMeasurement",
                table: "ProductionStageMaster",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompoundDifficultyMultiplier",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalRollDifficultyMultiplier",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ClientCalendarEvents",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FullClientID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ClientSpecificationID = table.Column<int>(type: "int", nullable: true),
                    ClientRollerNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCalendarEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_ClientCalendarEvents_ClientMaster_FullClientID",
                        column: x => x.FullClientID,
                        principalTable: "ClientMaster",
                        principalColumn: "FullClientID");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentCategoryMaster",
                columns: table => new
                {
                    EquipmentCategoryID = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    EquipmentCategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategoryMaster", x => x.EquipmentCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ProductionPlanItems",
                columns: table => new
                {
                    ProductionPlanItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderNo = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ProductionStageID = table.Column<int>(type: "int", nullable: false),
                    EquipmentItemID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PlannedStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Unplanned"),
                    IsPulsePlan = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlanItems", x => x.ProductionPlanItemID);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentItems",
                columns: table => new
                {
                    EquipmentItemID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FixedAssetNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    EquipmentCategoryID = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    EquipmentItemDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManufacturerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EquipmentDifficultyMultiplier = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 1m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentItems", x => x.EquipmentItemID);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_DivisionMaster_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_EquipmentCategoryMaster_EquipmentCategoryID",
                        column: x => x.EquipmentCategoryID,
                        principalTable: "EquipmentCategoryMaster",
                        principalColumn: "EquipmentCategoryID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentCapabilities",
                columns: table => new
                {
                    EquipmentItemID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProductionStageID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCapabilities", x => new { x.EquipmentItemID, x.ProductionStageID });
                    table.ForeignKey(
                        name: "FK_EquipmentCapabilities_EquipmentItems_EquipmentItemID",
                        column: x => x.EquipmentItemID,
                        principalTable: "EquipmentItems",
                        principalColumn: "EquipmentItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentCapabilities_ProductionStageMaster_ProductionStageID",
                        column: x => x.ProductionStageID,
                        principalTable: "ProductionStageMaster",
                        principalColumn: "ProductionStageId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientCalendarEvents_FullClientID",
                table: "ClientCalendarEvents",
                column: "FullClientID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCapabilities_ProductionStageID",
                table: "EquipmentCapabilities",
                column: "ProductionStageID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_DivisionID",
                table: "EquipmentItems",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_EquipmentCategoryID",
                table: "EquipmentItems",
                column: "EquipmentCategoryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientCalendarEvents");

            migrationBuilder.DropTable(
                name: "EquipmentCapabilities");

            migrationBuilder.DropTable(
                name: "ProductionPlanItems");

            migrationBuilder.DropTable(
                name: "EquipmentItems");

            migrationBuilder.DropTable(
                name: "EquipmentCategoryMaster");

            migrationBuilder.DropColumn(
                name: "BaseMinutesAtStage",
                table: "ProductionStageMaster");

            migrationBuilder.DropColumn(
                name: "BaseValue",
                table: "ProductionStageMaster");

            migrationBuilder.DropColumn(
                name: "DifficultyMeasurement",
                table: "ProductionStageMaster");

            migrationBuilder.DropColumn(
                name: "CompoundDifficultyMultiplier",
                table: "CompoundMaster");

            migrationBuilder.DropColumn(
                name: "AdditionalRollDifficultyMultiplier",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.AlterColumn<int>(
                name: "WorkCentreId",
                table: "WorkCentreFunctionsMapping",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
