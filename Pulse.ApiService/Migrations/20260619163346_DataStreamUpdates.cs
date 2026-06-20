using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class DataStreamUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserStreams_DataStreams_StreamName1_StreamDivisionID1",
                table: "AspNetUserStreams");

            migrationBuilder.DropTable(
                name: "DataStreams",
                schema: "app");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamName1_StreamDivisionID1",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "IsInvite",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamDivisionID1",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamName1",
                table: "AspNetUserStreams");

            migrationBuilder.AlterColumn<string>(
                name: "StreamName",
                table: "AspNetUserStreams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AspNetUserStreams",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "StreamID",
                table: "AspNetUserStreams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StreamType",
                table: "AspNetUserStreams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GroupChats",
                schema: "app",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    OwnerID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrivateGroup = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupChats", x => x.GroupID);
                });

            migrationBuilder.CreateTable(
                name: "SystemDataStreams",
                schema: "app",
                columns: table => new
                {
                    StreamID = table.Column<int>(type: "int", nullable: false),
                    StreamName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemDataStreams", x => x.StreamID);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "SystemDataStreams",
                columns: new[] { "StreamID", "Description", "StreamName" },
                values: new object[] { 1, "Data stream for production events.", "Production" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserStreams_StreamDivisionID",
                table: "AspNetUserStreams",
                column: "StreamDivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserStreams_StreamID",
                table: "AspNetUserStreams",
                column: "StreamID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserStreams_StreamType",
                table: "AspNetUserStreams",
                column: "StreamType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupChats",
                schema: "app");

            migrationBuilder.DropTable(
                name: "SystemDataStreams",
                schema: "app");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamDivisionID",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamID",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamType",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamID",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamType",
                table: "AspNetUserStreams");

            migrationBuilder.AlterColumn<string>(
                name: "StreamName",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInvite",
                table: "AspNetUserStreams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StreamDivisionID1",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreamName1",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams",
                columns: new[] { "ApplicationUserID", "StreamName", "StreamDivisionID" });

            migrationBuilder.CreateTable(
                name: "DataStreams",
                schema: "app",
                columns: table => new
                {
                    StreamName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsPrivateStream = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSystemStream = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OwnerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStreams", x => new { x.StreamName, x.DivisionID });
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "DataStreams",
                columns: new[] { "DivisionID", "StreamName", "Description", "IsSystemStream", "OwnerID", "OwnerName" },
                values: new object[] { "All", "Production", "Data stream for production events.", true, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserStreams_StreamName1_StreamDivisionID1",
                table: "AspNetUserStreams",
                columns: new[] { "StreamName1", "StreamDivisionID1" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserStreams_DataStreams_StreamName1_StreamDivisionID1",
                table: "AspNetUserStreams",
                columns: new[] { "StreamName1", "StreamDivisionID1" },
                principalSchema: "app",
                principalTable: "DataStreams",
                principalColumns: new[] { "StreamName", "DivisionID" });
        }
    }
}
