using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class EntityConfigFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId1",
                table: "CompoundMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID1",
                table: "ProductionStageMaster");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_WorksOrder_CompoundMaster_CompoundCode",
            //    table: "WorksOrder");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_WorksOrder_CompoundMaster_CompoundCode1",
            //    table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_CompoundCode1",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageMaster_WorkTypeID1",
                table: "ProductionStageMaster");

            migrationBuilder.DropIndex(
                name: "IX_CompoundMaster_CompoundRangeId1",
                table: "CompoundMaster");

            migrationBuilder.DropColumn(
                name: "CompoundCode1",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "WorkTypeID1",
                table: "ProductionStageMaster");

            migrationBuilder.DropColumn(
                name: "CompoundRangeId1",
                table: "CompoundMaster");

            migrationBuilder.AlterColumn<string>(
                name: "CoverCompoundCode",
                table: "WorksOrder",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellWeight",
                table: "ClientRollerMaster",
                type: "decimal(12,3)",
                precision: 12,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellLength",
                table: "ClientRollerMaster",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellDiameter",
                table: "ClientRollerMaster",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "ShellDefects",
                table: "ClientRollerMaster",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ClientRollerMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverDiameter",
                table: "ClientRollerMaster",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_CoverCompoundCode",
                table: "WorksOrder",
                column: "CoverCompoundCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerMaster_ClientRollerNumber",
                table: "ClientRollerMaster",
                column: "ClientRollerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerMaster_IsActive",
                table: "ClientRollerMaster",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster",
                column: "WorkTypeID",
                principalTable: "WorkTypeMaster",
                principalColumn: "WorkTypeID",
                onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorksOrder_CompoundMaster_CompoundCode",
            //    table: "WorksOrder",
            //    column: "CompoundCode",
            //    principalTable: "CompoundMaster",
            //    principalColumn: "CompoundCode");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorksOrder_CompoundMaster_CoverCompoundCode",
            //    table: "WorksOrder",
            //    column: "CoverCompoundCode",
            //    principalTable: "CompoundMaster",
            //    principalColumn: "CompoundCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CoverCompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_CoverCompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_ClientRollerMaster_ClientRollerNumber",
                table: "ClientRollerMaster");

            migrationBuilder.DropIndex(
                name: "IX_ClientRollerMaster_IsActive",
                table: "ClientRollerMaster");

            migrationBuilder.AlterColumn<string>(
                name: "CoverCompoundCode",
                table: "WorksOrder",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompoundCode1",
                table: "WorksOrder",
                type: "nvarchar(4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkTypeID1",
                table: "ProductionStageMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompoundRangeId1",
                table: "CompoundMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellWeight",
                table: "ClientRollerMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,3)",
                oldPrecision: 12,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellLength",
                table: "ClientRollerMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShellDiameter",
                table: "ClientRollerMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "ShellDefects",
                table: "ClientRollerMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ClientRollerMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverDiameter",
                table: "ClientRollerMaster",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_CompoundCode1",
                table: "WorksOrder",
                column: "CompoundCode1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageMaster_WorkTypeID1",
                table: "ProductionStageMaster",
                column: "WorkTypeID1");

            migrationBuilder.CreateIndex(
                name: "IX_CompoundMaster_CompoundRangeId1",
                table: "CompoundMaster",
                column: "CompoundRangeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId1",
                table: "CompoundMaster",
                column: "CompoundRangeId1",
                principalTable: "CompoundRangeMaster",
                principalColumn: "CompoundRangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID",
                table: "ProductionStageMaster",
                column: "WorkTypeID",
                principalTable: "WorkTypeMaster",
                principalColumn: "WorkTypeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageMaster_WorkTypeMaster_WorkTypeID1",
                table: "ProductionStageMaster",
                column: "WorkTypeID1",
                principalTable: "WorkTypeMaster",
                principalColumn: "WorkTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder",
                column: "CompoundCode",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode1",
                table: "WorksOrder",
                column: "CompoundCode1",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode");
        }
    }
}
