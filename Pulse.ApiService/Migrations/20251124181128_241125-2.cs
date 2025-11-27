using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2411252 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettingsMaster",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AIHasVoice = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AIVoiceID = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AIAutoModel = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AIDefaultModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettingsMaster", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettingsMaster_UserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettingsMaster");
        }
    }
}
