using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class DataStreamFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserStreams_DataStreams_StreamName1_StreamDivisionID",
                table: "AspNetUserStreams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamName1_StreamDivisionID",
                table: "AspNetUserStreams");

            migrationBuilder.AlterColumn<string>(
                name: "StreamDivisionID",
                table: "AspNetUserStreams",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreamDivisionID1",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams",
                columns: new[] { "ApplicationUserID", "StreamName", "StreamDivisionID" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserStreams_DataStreams_StreamName1_StreamDivisionID1",
                table: "AspNetUserStreams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserStreams_StreamName1_StreamDivisionID1",
                table: "AspNetUserStreams");

            migrationBuilder.DropColumn(
                name: "StreamDivisionID1",
                table: "AspNetUserStreams");

            migrationBuilder.AlterColumn<string>(
                name: "StreamDivisionID",
                table: "AspNetUserStreams",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserStreams",
                table: "AspNetUserStreams",
                columns: new[] { "ApplicationUserID", "StreamName" });

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
    }
}
