using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddDataStreamUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivateStream",
                schema: "app",
                table: "DataStreams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                schema: "app",
                table: "DataStreams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInvite",
                table: "AspNetUserStreams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StreamDivisionID",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreamName1",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "app",
                table: "DataStreams",
                keyColumns: new[] { "DivisionID", "StreamName" },
                keyValues: new object[] { "All", "Production" },
                column: "OwnerName",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserStreams_ApplicationUserID",
                table: "AspNetUserStreams",
                column: "ApplicationUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserStreams_StreamName1_StreamDivisionID",
                table: "AspNetUserStreams",
                columns: new[] { "StreamName1", "StreamDivisionID" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserStreams_DataStreams_StreamName1_StreamDivisionID",
                table: "AspNetUserStreams",
                columns: new[] { "StreamName1", "StreamDivisionID" },
                principalSchema: "app",
                principalTable: "DataStreams",
                principalColumns: new[] { "StreamName", "DivisionID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserStreams_DataStreams_StreamName1_StreamDivisionID",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_ApplicationUserID",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamName1_StreamDivisionID",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "IsPrivateStream",
                schema: "app",
                table: "DataStreams");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                schema: "app",
                table: "DataStreams");

            migrationBuilder.DropColumn(
                name: "IsInvite",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamDivisionID",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamName1",
                table: "AspNetUserStreams");
        }
    }
}
