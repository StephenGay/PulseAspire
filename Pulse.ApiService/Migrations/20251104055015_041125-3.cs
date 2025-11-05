using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0411253 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CoverCompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_CoverCompoundCode",
                table: "WorksOrder");

            migrationBuilder.AddColumn<string>(
                name: "CompoundCode",
                table: "WorksOrder",
                type: "nvarchar(4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_CompoundCode",
                table: "WorksOrder",
                column: "CompoundCode");

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder",
                column: "CompoundCode",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorksOrder_CompoundCode",
                table: "WorksOrder");

            migrationBuilder.DropColumn(
                name: "CompoundCode",
                table: "WorksOrder");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_CoverCompoundCode",
                table: "WorksOrder",
                column: "CoverCompoundCode");

            migrationBuilder.AddForeignKey(
                name: "FK_WorksOrder_CompoundMaster_CoverCompoundCode",
                table: "WorksOrder",
                column: "CoverCompoundCode",
                principalTable: "CompoundMaster",
                principalColumn: "CompoundCode");
        }
    }
}
