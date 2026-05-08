using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Modify_ProdPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClosedByUserID",
                table: "ProductionPlanItems",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkCentreID",
                table: "ProductionPlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneId",
                table: "ProductionPlanItems",
                type: "nvarchar(MAX)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkCentreID",
                table: "ProductionPlanItems");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "ProductionPlanItems");

            migrationBuilder.AlterColumn<int>(
                name: "ClosedByUserID",
                table: "ProductionPlanItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
        }
    }
}
