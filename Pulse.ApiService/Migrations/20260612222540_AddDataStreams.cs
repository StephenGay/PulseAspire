using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddDataStreams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUserStreams",
                columns: table => new
                {
                    ApplicationUserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StreamName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserStreams", x => new { x.ApplicationUserID, x.StreamName });
                });

            migrationBuilder.CreateTable(
                name: "DataStreams",
                schema: "app",
                columns: table => new
                {
                    StreamName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemStream = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStreams", x => new { x.StreamName, x.DivisionID });
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "DataStreams",
                columns: new[] { "DivisionID", "StreamName", "Description", "IsSystemStream", "OwnerID" },
                values: new object[] { "All", "Production", "Data stream for production events.", true, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserStreams");

            migrationBuilder.DropTable(
                name: "DataStreams",
                schema: "app");
        }
    }
}
