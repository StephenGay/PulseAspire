using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0401261 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PulseMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SenderUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "user"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "HTML"),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    Delivered = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PulseMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PulseMessages_RecipientUserId",
                table: "PulseMessages",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PulseMessages_SenderUserId",
                table: "PulseMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PulseMessages_SentAt",
                table: "PulseMessages",
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PulseMessages");
        }
    }
}
