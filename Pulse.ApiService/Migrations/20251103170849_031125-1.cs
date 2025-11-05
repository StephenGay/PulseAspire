using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0311251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkTypeMaster",
                columns: table => new
                {
                    WorkTypeID = table.Column<int>(type: "int", nullable: false),
                    WorkTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetWorkingDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WorkTypeMaterial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LockToProductCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTypeMaster", x => x.WorkTypeID);
                });

            migrationBuilder.CreateTable(
                name: "WorksOrder",
                columns: table => new
                {
                    WorksOrderNo = table.Column<int>(type: "int", nullable: false),
                    FullClientID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    DateStarted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkTypeID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", nullable: false),
                    ClientRollerSpecificationID = table.Column<int>(type: "int", nullable: true),
                    ClientRollerID = table.Column<int>(type: "int", nullable: true),
                    ClientRollNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverCompoundCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    ClientOrderNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientPRNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientRFQNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShellLength = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShellDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CoverDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UndelQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorksOrder", x => x.WorksOrderNo);
                    table.ForeignKey(
                        name: "FK_WorksOrder_ClientMaster_FullClientID",
                        column: x => x.FullClientID,
                        principalTable: "ClientMaster",
                        principalColumn: "FullClientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorksOrder_ClientRollerMaster_ClientRollerID",
                        column: x => x.ClientRollerID,
                        principalTable: "ClientRollerMaster",
                        principalColumn: "ClientRollerID");
                    table.ForeignKey(
                        name: "FK_WorksOrder_ClientRollerSpecificationMaster_ClientRollerSpecificationID",
                        column: x => x.ClientRollerSpecificationID,
                        principalTable: "ClientRollerSpecificationMaster",
                        principalColumn: "ClientRollerSpecificationID");
                    table.ForeignKey(
                        name: "FK_WorksOrder_CompoundMaster_CoverCompoundCode",
                        column: x => x.CoverCompoundCode,
                        principalTable: "CompoundMaster",
                        principalColumn: "CompoundCode");
                    table.ForeignKey(
                        name: "FK_WorksOrder_DivisionMaster_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_WorksOrder_PeriodMaster_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "PeriodMaster",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorksOrder_WorkTypeMaster_WorkTypeID",
                        column: x => x.WorkTypeID,
                        principalTable: "WorkTypeMaster",
                        principalColumn: "WorkTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_ClientRollerID",
                table: "WorksOrder",
                column: "ClientRollerID");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_ClientRollerSpecificationID",
                table: "WorksOrder",
                column: "ClientRollerSpecificationID");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_CoverCompoundCode",
                table: "WorksOrder",
                column: "CoverCompoundCode");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_DivisionID",
                table: "WorksOrder",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_FullClientID",
                table: "WorksOrder",
                column: "FullClientID");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_PeriodID",
                table: "WorksOrder",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_WorksOrder_WorkTypeID",
                table: "WorksOrder",
                column: "WorkTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkType_IsActive",
                table: "WorkTypeMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkType_SortOrder",
                table: "WorkTypeMaster",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorksOrder");

            migrationBuilder.DropTable(
                name: "WorkTypeMaster");
        }
    }
}
