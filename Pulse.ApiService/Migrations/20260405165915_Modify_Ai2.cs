using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Ai2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlapperMessages_FlapperConversations_ConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

            //migrationBuilder.DropIndex(
            //    name: "IX_FlapperMessages_ConversationId",
            //    schema: "pai",
            //    table: "FlapperMessages");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
