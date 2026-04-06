using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Ai3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlapperMessages_FlapperConversations_FlapperConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

            //migrationBuilder.DropIndex(
            //    name: "IX_FlapperMessages_FlapperConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

            //migrationBuilder.DropColumn(
            //    name: "FlapperConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Sender",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "nvarchar(50)",
            //    maxLength: 50,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "ContentType",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "nvarchar(50)",
            //    maxLength: 50,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "ClarificationType",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "nvarchar(50)",
            //    maxLength: 50,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<Guid>(
            //    name: "Id",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "uniqueidentifier",
            //    nullable: false,
            //    defaultValueSql: "NEWID()",
            //    oldClrType: typeof(Guid),
            //    oldType: "uniqueidentifier");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FlapperMessages_ConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    column: "ConversationId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_FlapperMessages_FlapperConversations_ConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    column: "ConversationId",
            //    principalSchema: "pai",
            //    principalTable: "FlapperConversations",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlapperMessages_FlapperConversations_ConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

            //migrationBuilder.DropIndex(
            //    name: "IX_FlapperMessages_ConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Sender",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(50)",
            //    oldMaxLength: 50);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ContentType",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(50)",
            //    oldMaxLength: 50);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ClarificationType",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(50)",
            //    oldMaxLength: 50,
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<Guid>(
            //    name: "Id",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "uniqueidentifier",
            //    nullable: false,
            //    oldClrType: typeof(Guid),
            //    oldType: "uniqueidentifier",
            //    oldDefaultValueSql: "NEWID()");

            //migrationBuilder.AddColumn<Guid>(
            //    name: "FlapperConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    type: "uniqueidentifier",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_FlapperMessages_FlapperConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    column: "FlapperConversationId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_FlapperMessages_FlapperConversations_FlapperConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages",
            //    column: "FlapperConversationId",
            //    principalSchema: "pai",
            //    principalTable: "FlapperConversations",
            //    principalColumn: "Id");
        }
    }
}
