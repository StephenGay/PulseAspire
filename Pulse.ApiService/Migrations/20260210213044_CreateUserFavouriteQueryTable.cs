using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserFavouriteQueryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavouriteQuery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    QueryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavouriteQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavouriteQuery_AiSavedQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "AiSavedQueries",
                        principalColumn: "AiQueryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavouriteQuery_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteQuery_QueryId",
                table: "UserFavouriteQuery",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteQuery_UserId",
                table: "UserFavouriteQuery",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteQuery_UserId_QueryId",
                table: "UserFavouriteQuery",
                columns: new[] { "UserId", "QueryId" },
                unique: true);

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "UserFavouriteQuery");

            
        }
    }
}
