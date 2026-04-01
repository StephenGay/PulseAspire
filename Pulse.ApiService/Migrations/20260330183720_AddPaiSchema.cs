using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddPaiSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pai");

            migrationBuilder.RenameTable(
                name: "ContextualPromptMaster",
                newName: "ContextualPromptMaster",
                newSchema: "pai");

            migrationBuilder.RenameTable(
                name: "ContextualAreaMaster",
                newName: "ContextualAreaMaster",
                newSchema: "pai");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                schema: "pai",
                table: "ContextualPromptMaster",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ContextualPromptMaster",
                schema: "pai",
                newName: "ContextualPromptMaster");

            migrationBuilder.RenameTable(
                name: "ContextualAreaMaster",
                schema: "pai",
                newName: "ContextualAreaMaster");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ContextualPromptMaster",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
