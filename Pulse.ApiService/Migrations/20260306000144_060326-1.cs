using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0603261 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "UserId",
            //    table: "ContextualPromptMaster");

            //migrationBuilder.AddColumn<string>(
            //    name: "RecipientUserName",
            //    table: "PulseMessages",
            //    type: "nvarchar(256)",
            //    maxLength: 256,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Subject",
            //    table: "PulseMessages",
            //    type: "nvarchar(200)",
            //    maxLength: 200,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ApplicationUserId",
            //    table: "ContextualPromptMaster",
            //    type: "nvarchar(450)",
            //    maxLength: 450,
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<string>(
            //    name: "AreaAiPrompt",
            //    table: "ContextualAreaMaster",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ExpectedRequestDataFormat",
            //    table: "ContextualAreaMaster",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "CompanyOperations",
            //    table: "CompanyMaster",
            //    type: "nvarchar(max)",
            //    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "RecipientUserName",
            //    table: "PulseMessages");

            //migrationBuilder.DropColumn(
            //    name: "Subject",
            //    table: "PulseMessages");

            //migrationBuilder.DropColumn(
            //    name: "ApplicationUserId",
            //    table: "ContextualPromptMaster");

            //migrationBuilder.DropColumn(
            //    name: "AreaAiPrompt",
            //    table: "ContextualAreaMaster");

            //migrationBuilder.DropColumn(
            //    name: "ExpectedRequestDataFormat",
            //    table: "ContextualAreaMaster");

            //migrationBuilder.DropColumn(
            //    name: "CompanyOperations",
            //    table: "CompanyMaster");

            //migrationBuilder.AddColumn<int>(
            //    name: "UserId",
            //    table: "ContextualPromptMaster",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);
        }
    }
}
