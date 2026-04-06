using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Ai4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ToolName",
                schema: "pai",
                table: "FlapperMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolName",
                schema: "pai",
                table: "FlapperMessages");
        }
    }
}
