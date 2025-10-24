using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2310253 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownVote",
                table: "AiSavedQueries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpVote",
                table: "AiSavedQueries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserFavouriteQuery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_UserFavouriteQuery_UserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavouriteQuery");

            migrationBuilder.DropColumn(
                name: "DownVote",
                table: "AiSavedQueries");

            migrationBuilder.DropColumn(
                name: "UpVote",
                table: "AiSavedQueries");
        }
    }
}
