using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2010251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndustryRecommendedCoverMaster",
                columns: table => new
                {
                    IndustryRecommendedCoverId = table.Column<int>(type: "int", nullable: false),
                    CompoundCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    IndustryRollerEnvironmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryRecommendedCoverMaster", x => x.IndustryRecommendedCoverId);
                    table.ForeignKey(
                        name: "FK_IndustryRecommendedCoverMaster_CompoundMaster_CompoundCode",
                        column: x => x.CompoundCode,
                        principalTable: "CompoundMaster",
                        principalColumn: "CompoundCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndustryRecommendedCoverMaster_IndustryRollerEnvironmentMaster_IndustryRollerEnvironmentId",
                        column: x => x.IndustryRollerEnvironmentId,
                        principalTable: "IndustryRollerEnvironmentMaster",
                        principalColumn: "IndustryRollerEnvironmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRecommendedCoverMaster_CompoundCode",
                table: "IndustryRecommendedCoverMaster",
                column: "CompoundCode");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRecommendedCoverMaster_IndustryRollerEnvironmentId",
                table: "IndustryRecommendedCoverMaster",
                column: "IndustryRollerEnvironmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndustryRecommendedCoverMaster");
        }
    }
}
