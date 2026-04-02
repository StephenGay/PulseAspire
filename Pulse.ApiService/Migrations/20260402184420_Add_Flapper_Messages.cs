using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Add_Flapper_Messages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadedObjectJson",
                schema: "pai",
                table: "AnalysisRequests");

            migrationBuilder.CreateTable(
                name: "FlapperConversations",
                schema: "pai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlapperConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlapperMessages",
                schema: "pai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClarificationQuestion = table.Column<bool>(type: "bit", nullable: false),
                    ClarificationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlapperConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlapperMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlapperMessages_FlapperConversations_FlapperConversationId",
                        column: x => x.FlapperConversationId,
                        principalSchema: "pai",
                        principalTable: "FlapperConversations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlapperMessages_FlapperConversationId",
                schema: "pai",
                table: "FlapperMessages",
                column: "FlapperConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlapperMessages",
                schema: "pai");

            migrationBuilder.DropTable(
                name: "FlapperConversations",
                schema: "pai");

            migrationBuilder.AddColumn<string>(
                name: "LoadedObjectJson",
                schema: "pai",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
