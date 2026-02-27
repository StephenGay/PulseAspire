using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2402261 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NonConformanceReports",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    NCRNo = table.Column<int>(type: "int", nullable: true),
                    TypeOfNCR = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "New"),
                    RaisedByDivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    RaisedByDivisionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedToDivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    AssignedToDivisionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AgainstDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    AccountID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CustSuppName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProductDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DiscoveryDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cause = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsRepetition = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OldWONos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldCompoundCodes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BatchNos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Machine = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Scrapped = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "No"),
                    IsClaim = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ScrapValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    ScrapKgs = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValue: 0.00m),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TechnicalReviewer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    InvestigatorName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FurtherActionRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateClosed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TechnicalOutcome = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    RCATechnique = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reason4Ms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreventativeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevisedPreventativeAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpertComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestigationComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechReviewComments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonConformanceReports", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NonConformanceReports");
        }
    }
}
