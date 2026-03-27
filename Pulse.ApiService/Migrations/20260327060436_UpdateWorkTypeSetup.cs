using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkTypeSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRollerSpecificationMaster_ClientMaster_FullClientID",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRollerSpecificationMaster_CompoundMaster_CompoundCode",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId",
                table: "CompoundMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_DivisionMaster_DivisionID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_ClientRollerSpecificationMaster_ClientRollerSpecificationID",
                table: "WorksOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_DivisionID",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_WorkTypeID",
                table: "ProductionStageMaster");

            migrationBuilder.AddColumn<string>(
                name: "Component",
                table: "WorkTypeMaster",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionForAI",
                table: "WorkTypeMaster",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FactoryWorkType",
                table: "WorkTypeMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "WorkTypeMaster",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompoundCode1",
                table: "WorksOrder",
                type: "nvarchar(4)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseValue",
                table: "ProductionStageMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "BaseMinutesAtStage",
                table: "ProductionStageMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "SpecificGravity",
                table: "CompoundMaster",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "RoyaltyCharge",
                table: "CompoundMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevisionDate",
                table: "CompoundMaster",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AlterColumn<bool>(
                name: "OverRideSpecificGravity",
                table: "CompoundMaster",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "OverRideCostPerKg",
                table: "CompoundMaster",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsImported",
                table: "CompoundMaster",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "CustomSaleFactor",
                table: "CompoundMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostingCostPerKg",
                table: "CompoundMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CompoundDifficultyMultiplier",
                table: "CompoundMaster",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 1m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CarbonBlackCharge",
                table: "CompoundMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CalculatedSpecificGravity",
                table: "CompoundMaster",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CalculatedCostPerKg",
                table: "CompoundMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "CompoundRangeId1",
                table: "CompoundMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellWeight",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellMinimumDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellLength",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RollerFunction",
                table: "ClientRollerSpecificationMaster",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ClientRollerSpecificationMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverMinimumDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverLength",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverLeftOffset",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalRollDifficultyMultiplier",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 1m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "CustomerFullClientID",
                table: "ClientRollerSpecificationMaster",
                type: "nvarchar(10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DivisionWorkTypes",
                columns: table => new
                {
                    WorkTypeID = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TargetWorkingDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionWorkTypes", x => new { x.WorkTypeID, x.DivisionId });
                    table.ForeignKey(
                        name: "FK_DivisionWorkTypes_DivisionMaster_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DivisionWorkTypes_WorkTypeMaster_WorkTypeID",
                        column: x => x.WorkTypeID,
                        principalTable: "WorkTypeMaster",
                        principalColumn: "WorkTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_CompoundCode1",
                table: "WorksOrder",
                column: "CompoundCode1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_DivisionID_WorkTypeID",
                table: "ProductionStageMaster",
                columns: new[] { "DivisionID", "WorkTypeID" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_DivisionID_WorkTypeID_StepNo",
                table: "ProductionStageMaster",
                columns: new[] { "DivisionID", "WorkTypeID", "StepNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_IsActive",
                table: "ProductionStageMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_StepNo",
                table: "ProductionStageMaster",
                column: "StepNo");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_WorkTypeID_DivisionID",
                table: "ProductionStageMaster",
                columns: new[] { "WorkTypeID", "DivisionID" });

            migrationBuilder.CreateIndex(
                name: "IX_CompoundMaster_CompoundRangeId1",
                table: "CompoundMaster",
                column: "CompoundRangeId1");

            migrationBuilder.CreateIndex(
                name: "IX_CompoundMaster_CompoundType",
                table: "CompoundMaster",
                column: "CompoundType");

            migrationBuilder.CreateIndex(
                name: "IX_CompoundMaster_IsImported",
                table: "CompoundMaster",
                column: "IsImported");

            migrationBuilder.CreateIndex(
                name: "IX_CompoundMaster_State",
                table: "CompoundMaster",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerSpecificationMaster_CustomerFullClientID",
                table: "ClientRollerSpecificationMaster",
                column: "CustomerFullClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerSpecificationMaster_FullClientID_IsActive",
                table: "ClientRollerSpecificationMaster",
                columns: new[] { "FullClientID", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerSpecificationMaster_IsActive",
                table: "ClientRollerSpecificationMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionWorkTypes_DivisionId",
                table: "DivisionWorkTypes",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionWorkTypes_DivisionId_IsActive",
                table: "DivisionWorkTypes",
                columns: new[] { "DivisionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DivisionWorkTypes_IsActive",
                table: "DivisionWorkTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionWorkTypes_SortOrder",
                table: "DivisionWorkTypes",
                column: "SortOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRollerSpecificationMaster_ClientMaster_CustomerFullClientID",
                table: "ClientRollerSpecificationMaster",
                column: "CustomerFullClientID",
                principalTable: "ClientMaster",
                principalColumn: "FullClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRollerSpecificationMaster_ClientMaster_FullClientID",
                table: "ClientRollerSpecificationMaster",
                column: "FullClientID",
                principalTable: "ClientMaster",
                principalColumn: "FullClientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRollerSpecificationMaster_CompoundMaster_CompoundCode",
                table: "ClientRollerSpecificationMaster",
                column: "CompoundCode",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId",
                table: "CompoundMaster",
                column: "CompoundRangeId",
                principalTable: "CompoundRangeMaster",
                principalColumn: "CompoundRangeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId1",
                table: "CompoundMaster",
                column: "CompoundRangeId1",
                principalTable: "CompoundRangeMaster",
                principalColumn: "CompoundRangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_DivisionMaster_DivisionID",
                table: "ProductionStageMaster",
                column: "DivisionID",
                principalTable: "DivisionMaster",
                principalColumn: "DivisionID",
                onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_ProductionStageMaster_DivisionWorkTypes_WorkTypeID_DivisionID",
            //    table: "ProductionStageMaster",
            //    columns: new[] { "WorkTypeID", "DivisionID" },
            //    principalTable: "DivisionWorkTypes",
            //    principalColumns: new[] { "WorkTypeID", "DivisionId" },
            //    onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster",
                column: "WorkTypeID",
                principalTable: "WorkTypeMaster",
                principalColumn: "WorkTypeID",
                onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorksOrder_ClientRollerSpecificationMaster_ClientRollerSpecificationID",
            //    table: "WorksOrder",
            //    column: "ClientRollerSpecificationID",
            //    principalTable: "ClientRollerSpecificationMaster",
            //    principalColumn: "ClientRollerSpecificationID",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorksOrder_CompoundMaster_CompoundCode",
            //    table: "WorksOrder",
            //    column: "CompoundCode",
            //    principalTable: "CompoundMaster",
            //    principalColumn: "CompoundCode",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorksOrder_CompoundMaster_CompoundCode1",
            //    table: "WorksOrder",
            //    column: "CompoundCode1",
            //    principalTable: "CompoundMaster",
            //    principalColumn: "CompoundCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRollerSpecificationMaster_ClientMaster_CustomerFullClientID",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRollerSpecificationMaster_ClientMaster_FullClientID",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRollerSpecificationMaster_CompoundMaster_CompoundCode",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId",
                table: "CompoundMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId1",
                table: "CompoundMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_DivisionMaster_DivisionID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_DivisionWorkTypes_WorkTypeID_DivisionID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_ClientRollerSpecificationMaster_ClientRollerSpecificationID",
                table: "WorksOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode1",
                table: "WorksOrder");

            migrationBuilder.DropTable(
                name: "DivisionWorkTypes");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_CompoundCode1",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_DivisionID_WorkTypeID",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_DivisionID_WorkTypeID_StepNo",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_IsActive",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_StepNo",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_WorkTypeID_DivisionID",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_CompoundMaster_CompoundRangeId1",
                table: "CompoundMaster");

            migrationBuilder.DropIndex(
                name: "IX_CompoundMaster_CompoundType",
                table: "CompoundMaster");

            migrationBuilder.DropIndex(
                name: "IX_CompoundMaster_IsImported",
                table: "CompoundMaster");

            migrationBuilder.DropIndex(
                name: "IX_CompoundMaster_State",
                table: "CompoundMaster");

            migrationBuilder.DropIndex(
                name: "IX_ClientRollerSpecificationMaster_CustomerFullClientID",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropIndex(
                name: "IX_ClientRollerSpecificationMaster_FullClientID_IsActive",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropIndex(
                name: "IX_ClientRollerSpecificationMaster_IsActive",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.DropColumn(
                name: "Component",
                table: "WorkTypeMaster");

            migrationBuilder.DropColumn(
                name: "DescriptionForAI",
                table: "WorkTypeMaster");

            migrationBuilder.DropColumn(
                name: "FactoryWorkType",
                table: "WorkTypeMaster");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "WorkTypeMaster");

            migrationBuilder.DropColumn(
                name: "CompoundCode1",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "CompoundRangeId1",
                table: "CompoundMaster");

            migrationBuilder.DropColumn(
                name: "CustomerFullClientID",
                table: "ClientRollerSpecificationMaster");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseValue",
                table: "ProductionStageMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "BaseMinutesAtStage",
                table: "ProductionStageMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "SpecificGravity",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "RoyaltyCharge",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevisionDate",
                table: "CompoundMaster",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "OverRideSpecificGravity",
                table: "CompoundMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "OverRideCostPerKg",
                table: "CompoundMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsImported",
                table: "CompoundMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "CustomSaleFactor",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostingCostPerKg",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompoundDifficultyMultiplier",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldDefaultValue: 1m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CarbonBlackCharge",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CalculatedSpecificGravity",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CalculatedCostPerKg",
                table: "CompoundMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellWeight",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellMinimumDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellLength",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "RollerFunction",
                table: "ClientRollerSpecificationMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ClientRollerSpecificationMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverMinimumDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverLength",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverLeftOffset",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverDiameter",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalRollDifficultyMultiplier",
                table: "ClientRollerSpecificationMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldDefaultValue: 1m);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_DivisionID",
                table: "ProductionStageMaster",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_WorkTypeID",
                table: "ProductionStageMaster",
                column: "WorkTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRollerSpecificationMaster_ClientMaster_FullClientID",
                table: "ClientRollerSpecificationMaster",
                column: "FullClientID",
                principalTable: "ClientMaster",
                principalColumn: "FullClientID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRollerSpecificationMaster_CompoundMaster_CompoundCode",
                table: "ClientRollerSpecificationMaster",
                column: "CompoundCode",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode");

            migrationBuilder.AddForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId",
                table: "CompoundMaster",
                column: "CompoundRangeId",
                principalTable: "CompoundRangeMaster",
                principalColumn: "CompoundRangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_DivisionMaster_DivisionID",
                table: "ProductionStageMaster",
                column: "DivisionID",
                principalTable: "DivisionMaster",
                principalColumn: "DivisionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster",
                column: "WorkTypeID",
                principalTable: "WorkTypeMaster",
                principalColumn: "WorkTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_ClientRollerSpecificationMaster_ClientRollerSpecificationID",
                table: "WorksOrder",
                column: "ClientRollerSpecificationID",
                principalTable: "ClientRollerSpecificationMaster",
                principalColumn: "ClientRollerSpecificationID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder",
                column: "CompoundCode",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode");
        }
    }
}
