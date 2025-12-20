using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _0912251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientBudgetMaster",
                columns: table => new
                {
                    ClientBudgetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullClientId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    FinancialYear = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    BudgetedSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientBudgetMaster", x => x.ClientBudgetId);
                    table.ForeignKey(
                        name: "FK_ClientBudgetMaster_ClientMaster_FullClientId",
                        column: x => x.FullClientId,
                        principalTable: "ClientMaster",
                        principalColumn: "FullClientID");
                    table.ForeignKey(
                        name: "FK_ClientBudgetMaster_PeriodMaster_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "PeriodMaster",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientBudgetMaster_PeriodID",
                table: "ClientBudgetMaster",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBudgets_FullClientId",
                table: "ClientBudgetMaster",
                column: "FullClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientBudgetMaster");
        }
    }
}
