using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Analysis_Requests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentTurn",
                schema: "pai",
                table: "AnalysisRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullConversationHistory",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastClarificationQuestion",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUserClarificationAnswer",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadedObjectJson",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentTurn",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "FullConversationHistory",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "LastClarificationQuestion",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "LastUserClarificationAnswer",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "LoadedObjectJson",
                schema: "pai",
                table: "AnalysisRequests");
        }
    }
}
