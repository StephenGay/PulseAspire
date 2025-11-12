using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0811252 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkCentreMaster_BranchMaster_BranchID",
                table: "WorkCentreMaster");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkCentreMaster_BranchMaster_BranchID",
                table: "WorkCentreMaster",
                column: "BranchID",
                principalTable: "BranchMaster",
                principalColumn: "BranchID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkCentreMaster_BranchMaster_BranchID",
                table: "WorkCentreMaster");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkCentreMaster_BranchMaster_BranchID",
                table: "WorkCentreMaster",
                column: "BranchID",
                principalTable: "BranchMaster",
                principalColumn: "BranchID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
