using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Add_AnalysisRequest_Queue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisRequests",
                schema: "pai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetEntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyProperty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResultSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoadedObjectJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_Status",
                schema: "pai",
                table: "AnalysisRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisRequests",
                schema: "pai");
        }
    }
}
