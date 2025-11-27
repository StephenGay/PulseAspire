using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2411251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContextualAreaMaster",
                columns: table => new
                {
                    ContextAreaId = table.Column<int>(type: "int", nullable: false),
                    AreaDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContextualAreaMaster", x => x.ContextAreaId);
                });

            migrationBuilder.CreateTable(
                name: "ContextualPromptMaster",
                columns: table => new
                {
                    ContextualPromptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContextualAreaId = table.Column<int>(type: "int", nullable: false),
                    ContextualPromptTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContextualPromptDescription = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Likes = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContextualPromptMaster", x => x.ContextualPromptId);
                    table.ForeignKey(
                        name: "FK_ContextualPromptMaster_ContextualAreaMaster_ContextualAreaId",
                        column: x => x.ContextualAreaId,
                        principalTable: "ContextualAreaMaster",
                        principalColumn: "ContextAreaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContextualPromptMaster_ContextualAreaId",
                table: "ContextualPromptMaster",
                column: "ContextualAreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContextualPromptMaster");

            migrationBuilder.DropTable(
                name: "ContextualAreaMaster");
        }
    }
}
