using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiSavedQueries",
                columns: table => new
                {
                    AiQueryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SqlQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSavedQueries", x => x.AiQueryID);
                });

            migrationBuilder.CreateTable(
                name: "BranchMaster",
                columns: table => new
                {
                    BranchID = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchMaster", x => x.BranchID);
                });

            migrationBuilder.CreateTable(
                name: "ColourMaster",
                columns: table => new
                {
                    ColourName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColourMaster", x => x.ColourName);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMaster",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMaster", x => x.CompanyID);
                });

            migrationBuilder.CreateTable(
                name: "CompoundRangeMaster",
                columns: table => new
                {
                    CompoundRangeId = table.Column<int>(type: "int", nullable: false),
                    RangeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompoundRangeMaster", x => x.CompoundRangeId);
                });

            migrationBuilder.CreateTable(
                name: "ContinentMaster",
                columns: table => new
                {
                    ContinentID = table.Column<int>(type: "int", nullable: false),
                    ContinentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContinentMaster", x => x.ContinentID);
                });

            migrationBuilder.CreateTable(
                name: "HardnessTypeMaster",
                columns: table => new
                {
                    HardnessTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardnessTypeMaster", x => x.HardnessTypeName);
                });

            migrationBuilder.CreateTable(
                name: "IndustryMaster",
                columns: table => new
                {
                    IndustryID = table.Column<int>(type: "int", nullable: false),
                    IndustryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryMaster", x => x.IndustryID);
                });

            migrationBuilder.CreateTable(
                name: "PeriodMaster",
                columns: table => new
                {
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CalendarYear = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    FinancialYear = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodMaster", x => x.PeriodID);
                });

            migrationBuilder.CreateTable(
                name: "PolymerMaster",
                columns: table => new
                {
                    PolymerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolymerMaster", x => x.PolymerName);
                });

            migrationBuilder.CreateTable(
                name: "RollerTypeMaster",
                columns: table => new
                {
                    RollerTypeID = table.Column<int>(type: "int", nullable: false),
                    RollerTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollerTypeMaster", x => x.RollerTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ShellTypeMaster",
                columns: table => new
                {
                    ShellTypeID = table.Column<int>(type: "int", nullable: false),
                    ShellTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShellTypeMaster", x => x.ShellTypeID);
                });

            migrationBuilder.CreateTable(
                name: "UserMaster",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaster", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "DivisionMaster",
                columns: table => new
                {
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DivisionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    BranchID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionMaster", x => x.DivisionID);
                    table.ForeignKey(
                        name: "FK_DivisionMaster_BranchMaster_BranchID",
                        column: x => x.BranchID,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DivisionMaster_CompanyMaster_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "CompanyMaster",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepresentativeMaster",
                columns: table => new
                {
                    RepresentativeID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    RepresentativeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkedUserID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepresentativeMaster", x => x.RepresentativeID);
                    table.ForeignKey(
                        name: "FK_RepresentativeMaster_CompanyMaster_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "CompanyMaster",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompoundMaster",
                columns: table => new
                {
                    CompoundCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    RevisionCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CompoundDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompoundRangeId = table.Column<int>(type: "int", nullable: true),
                    PolymerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HardnessMeasurement = table.Column<int>(type: "int", nullable: true),
                    HardnessTolerance = table.Column<int>(type: "int", nullable: true),
                    HardnessType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CalculatedSpecificGravity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverRideSpecificGravity = table.Column<bool>(type: "bit", nullable: false),
                    SpecificGravity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompoundType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalculatedCostPerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverRideCostPerKg = table.Column<bool>(type: "bit", nullable: false),
                    CostingCostPerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoyaltyCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarbonBlackCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsImported = table.Column<bool>(type: "bit", nullable: false),
                    CustomSaleFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RevisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevisedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RevisionReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateCostUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompoundMaster", x => x.CompoundCode);
                    table.ForeignKey(
                        name: "FK_CompoundMaster_CompoundRangeMaster_CompoundRangeId",
                        column: x => x.CompoundRangeId,
                        principalTable: "CompoundRangeMaster",
                        principalColumn: "CompoundRangeId");
                });

            migrationBuilder.CreateTable(
                name: "CompoundRangePropertyMaster",
                columns: table => new
                {
                    CompoundRangePropertyId = table.Column<int>(type: "int", nullable: false),
                    CompoundRangeId = table.Column<int>(type: "int", nullable: false),
                    PropertyText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompoundRangePropertyMaster", x => x.CompoundRangePropertyId);
                    table.ForeignKey(
                        name: "FK_CompoundRangePropertyMaster_CompoundRangeMaster_CompoundRangeId",
                        column: x => x.CompoundRangeId,
                        principalTable: "CompoundRangeMaster",
                        principalColumn: "CompoundRangeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CountryMaster",
                columns: table => new
                {
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContinentID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryMaster", x => x.CountryID);
                    table.ForeignKey(
                        name: "FK_CountryMaster_ContinentMaster_ContinentID",
                        column: x => x.ContinentID,
                        principalTable: "ContinentMaster",
                        principalColumn: "ContinentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndustryProcessMaster",
                columns: table => new
                {
                    IndustryProcessID = table.Column<int>(type: "int", nullable: false),
                    IndustryID = table.Column<int>(type: "int", nullable: false),
                    ProcessName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryProcessMaster", x => x.IndustryProcessID);
                    table.ForeignKey(
                        name: "FK_IndustryProcessMaster_IndustryMaster_IndustryID",
                        column: x => x.IndustryID,
                        principalTable: "IndustryMaster",
                        principalColumn: "IndustryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvinceMaster",
                columns: table => new
                {
                    ProvinceID = table.Column<int>(type: "int", nullable: false),
                    ProvinceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinceMaster", x => x.ProvinceID);
                    table.ForeignKey(
                        name: "FK_ProvinceMaster_CountryMaster_CountryID",
                        column: x => x.CountryID,
                        principalTable: "CountryMaster",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegionMaster",
                columns: table => new
                {
                    RegionID = table.Column<int>(type: "int", nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProvinceID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionMaster", x => x.RegionID);
                    table.ForeignKey(
                        name: "FK_RegionMaster_ProvinceMaster_ProvinceID",
                        column: x => x.ProvinceID,
                        principalTable: "ProvinceMaster",
                        principalColumn: "ProvinceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientMaster",
                columns: table => new
                {
                    FullClientID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    FullChargeClientID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClientID = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Address3 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Address4 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Address5 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PostalAddress1 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PostalAddress2 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PostalAddress3 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PostalAddress4 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PostalAddress5 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EMail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VATNo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    TaxCodeID = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    RequireOrderNo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SalesRepID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RegionID = table.Column<int>(type: "int", nullable: true),
                    IndustryID = table.Column<int>(type: "int", nullable: true),
                    SisterCompany = table.Column<bool>(type: "bit", nullable: false),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    TermDays = table.Column<int>(type: "int", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ageing01 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ageing02 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ageing03 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ageing04 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ageing05 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LastVisitDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMaster", x => x.FullClientID);
                    table.ForeignKey(
                        name: "FK_ClientMaster_CompanyMaster_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "CompanyMaster",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientMaster_IndustryMaster_IndustryID",
                        column: x => x.IndustryID,
                        principalTable: "IndustryMaster",
                        principalColumn: "IndustryID");
                    table.ForeignKey(
                        name: "FK_ClientMaster_RegionMaster_RegionID",
                        column: x => x.RegionID,
                        principalTable: "RegionMaster",
                        principalColumn: "RegionID");
                    table.ForeignKey(
                        name: "FK_ClientMaster_RepresentativeMaster_SalesRepID",
                        column: x => x.SalesRepID,
                        principalTable: "RepresentativeMaster",
                        principalColumn: "RepresentativeID");
                });

            migrationBuilder.CreateTable(
                name: "ClientSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullClientID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSales_ClientMaster_FullClientID",
                        column: x => x.FullClientID,
                        principalTable: "ClientMaster",
                        principalColumn: "FullClientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientSales_PeriodMaster_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "PeriodMaster",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMaster_CompanyID",
                table: "ClientMaster",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMaster_IndustryID",
                table: "ClientMaster",
                column: "IndustryID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMaster_RegionID",
                table: "ClientMaster",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMaster_SalesRepID",
                table: "ClientMaster",
                column: "SalesRepID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSales_FullClientID",
                table: "ClientSales",
                column: "FullClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSales_PeriodID",
                table: "ClientSales",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_CompoundMaster_CompoundRangeId",
                table: "CompoundMaster",
                column: "CompoundRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompoundRangePropertyMaster_CompoundRangeId",
                table: "CompoundRangePropertyMaster",
                column: "CompoundRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryMaster_ContinentID",
                table: "CountryMaster",
                column: "ContinentID");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionMaster_BranchID",
                table: "DivisionMaster",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionMaster_CompanyID",
                table: "DivisionMaster",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryProcessMaster_IndustryID",
                table: "IndustryProcessMaster",
                column: "IndustryID");

            migrationBuilder.CreateIndex(
                name: "IX_ProvinceMaster_CountryID",
                table: "ProvinceMaster",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_RegionMaster_ProvinceID",
                table: "RegionMaster",
                column: "ProvinceID");

            migrationBuilder.CreateIndex(
                name: "IX_RepresentativeMaster_CompanyID",
                table: "RepresentativeMaster",
                column: "CompanyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSavedQueries");

            migrationBuilder.DropTable(
                name: "ClientSales");

            migrationBuilder.DropTable(
                name: "ColourMaster");

            migrationBuilder.DropTable(
                name: "CompoundMaster");

            migrationBuilder.DropTable(
                name: "CompoundRangePropertyMaster");

            migrationBuilder.DropTable(
                name: "DivisionMaster");

            migrationBuilder.DropTable(
                name: "HardnessTypeMaster");

            migrationBuilder.DropTable(
                name: "IndustryProcessMaster");

            migrationBuilder.DropTable(
                name: "PolymerMaster");

            migrationBuilder.DropTable(
                name: "RollerTypeMaster");

            migrationBuilder.DropTable(
                name: "ShellTypeMaster");

            migrationBuilder.DropTable(
                name: "UserMaster");

            migrationBuilder.DropTable(
                name: "ClientMaster");

            migrationBuilder.DropTable(
                name: "PeriodMaster");

            migrationBuilder.DropTable(
                name: "CompoundRangeMaster");

            migrationBuilder.DropTable(
                name: "BranchMaster");

            migrationBuilder.DropTable(
                name: "IndustryMaster");

            migrationBuilder.DropTable(
                name: "RegionMaster");

            migrationBuilder.DropTable(
                name: "RepresentativeMaster");

            migrationBuilder.DropTable(
                name: "ProvinceMaster");

            migrationBuilder.DropTable(
                name: "CompanyMaster");

            migrationBuilder.DropTable(
                name: "CountryMaster");

            migrationBuilder.DropTable(
                name: "ContinentMaster");
        }
    }
}
