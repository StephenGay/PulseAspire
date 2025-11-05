using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0411252 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_DivisionMaster_DivisionID",
                table: "WorksOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorksOrder",
                table: "WorksOrder");

            migrationBuilder.AlterColumn<decimal>(
                name: "UndelQty",
                table: "WorksOrder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorksOrder",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SellPrice",
                table: "WorksOrder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialCost",
                table: "WorksOrder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "DivisionID",
                table: "WorksOrder",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorksOrder",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateStarted",
                table: "WorksOrder",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

           
            migrationBuilder.AlterColumn<string>(
                name: "ClientRollNo",
                table: "WorksOrder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientRFQNo",
                table: "WorksOrder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientPRNo",
                table: "WorksOrder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientOrderNo",
                table: "WorksOrder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__WorksOrder",
                table: "WorksOrder",
                column: "WorksOrderNo");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_ClientPeriodStatus",
                table: "WorksOrder",
                columns: new[] { "FullClientID", "PeriodID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_DateStarted",
                table: "WorksOrder",
                column: "DateStarted");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_Status",
                table: "WorksOrder",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_DivisionMaster_DivisionID",
                table: "WorksOrder",
                column: "DivisionID",
                principalTable: "DivisionMaster",
                principalColumn: "DivisionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_DivisionMaster_DivisionID",
                table: "WorksOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK__WorksOrder",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_ClientPeriodStatus",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_DateStarted",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_Status",
                table: "WorksOrder");

            migrationBuilder.AlterColumn<decimal>(
                name: "UndelQty",
                table: "WorksOrder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorksOrder",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SellPrice",
                table: "WorksOrder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialCost",
                table: "WorksOrder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "DivisionID",
                table: "WorksOrder",
                type: "nvarchar(5)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorksOrder",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateStarted",
                table: "WorksOrder",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "CoverCompoundCode",
                table: "WorksOrder",
                type: "nvarchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientRollNo",
                table: "WorksOrder",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientRFQNo",
                table: "WorksOrder",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientPRNo",
                table: "WorksOrder",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientOrderNo",
                table: "WorksOrder",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorksOrder",
                table: "WorksOrder",
                column: "WorksOrderNo");

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_DivisionMaster_DivisionID",
                table: "WorksOrder",
                column: "DivisionID",
                principalTable: "DivisionMaster",
                principalColumn: "DivisionID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
