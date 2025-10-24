using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _1910257 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RevisionDate",
                table: "CompoundMaster",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "IndustryRollerEnvironmentMaster",
                columns: table => new
                {
                    IndustryRollerEnvironmentId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    RollerTypeId = table.Column<int>(type: "int", nullable: false),
                    IndustryProcessId = table.Column<int>(type: "int", nullable: true),
                    EnvironmentConditions = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryRollerEnvironmentMaster", x => x.IndustryRollerEnvironmentId);
                    table.ForeignKey(
                        name: "FK_IndustryRollerEnvironmentMaster_IndustryMaster_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "IndustryMaster",
                        principalColumn: "IndustryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndustryRollerEnvironmentMaster_IndustryProcessMaster_IndustryProcessId",
                        column: x => x.IndustryProcessId,
                        principalTable: "IndustryProcessMaster",
                        principalColumn: "IndustryProcessID");
                    table.ForeignKey(
                        name: "FK_IndustryRollerEnvironmentMaster_RollerTypeMaster_RollerTypeId",
                        column: x => x.RollerTypeId,
                        principalTable: "RollerTypeMaster",
                        principalColumn: "RollerTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRollerEnvironmentMaster_IndustryId",
                table: "IndustryRollerEnvironmentMaster",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRollerEnvironmentMaster_IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster",
                column: "IndustryProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRollerEnvironmentMaster_RollerTypeId",
                table: "IndustryRollerEnvironmentMaster",
                column: "RollerTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndustryRollerEnvironmentMaster");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevisionDate",
                table: "CompoundMaster",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");
        }
    }
}
