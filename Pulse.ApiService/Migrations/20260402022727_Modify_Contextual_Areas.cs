using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Contextual_Areas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisType",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "KeyProperty",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "TargetEntityType",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.RenameColumn(
                name: "ExpectedRequestDataFormat",
                schema: "pai",
                table: "ContextualAreaMaster",
                newName: "TargetEntityType");

            migrationBuilder.AddColumn<string>(
                name: "KeyProperty",
                schema: "pai",
                table: "ContextualAreaMaster",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ContextualAreaID",
                schema: "pai",
                table: "AnalysisRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyProperty",
                schema: "pai",
                table: "ContextualAreaMaster");

            migrationBuilder.DropColumn(
                name: "ContextualAreaID",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.RenameColumn(
                name: "TargetEntityType",
                schema: "pai",
                table: "ContextualAreaMaster",
                newName: "ExpectedRequestDataFormat");

            migrationBuilder.AddColumn<string>(
                name: "AnalysisType",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeyProperty",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetEntityType",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
