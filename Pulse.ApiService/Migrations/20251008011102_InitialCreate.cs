using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchMaster",
                columns: table => new
                {
                    BranchID = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchMaster", x => x.BranchID);
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
                name: "ProvinceMaster",
                columns: table => new
                {
                    ProvinceID = table.Column<int>(type: "int", nullable: false),
                    ProvinceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                    RegionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProvinceID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                    Address1 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address3 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address4 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address5 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PostalAddress1 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PostalAddress2 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PostalAddress3 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PostalAddress4 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PostalAddress5 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EMail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VATNo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    TaxCodeID = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    RequireOrderNo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SalesRepID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    RegionID = table.Column<int>(type: "int", nullable: true),
                    IndustryID = table.Column<int>(type: "int", nullable: true),
                    SisterCompany = table.Column<bool>(type: "bit", nullable: false),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    TermDays = table.Column<int>(type: "int", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ageing01 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ageing02 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ageing03 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ageing04 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ageing05 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                name: "IX_ProvinceMaster_CountryID",
                table: "ProvinceMaster",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_RegionMaster_ProvinceID",
                table: "RegionMaster",
                column: "ProvinceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientMaster");

            migrationBuilder.DropTable(
                name: "DivisionMaster");

            migrationBuilder.DropTable(
                name: "IndustryMaster");

            migrationBuilder.DropTable(
                name: "RegionMaster");

            migrationBuilder.DropTable(
                name: "BranchMaster");

            migrationBuilder.DropTable(
                name: "CompanyMaster");

            migrationBuilder.DropTable(
                name: "ProvinceMaster");

            migrationBuilder.DropTable(
                name: "CountryMaster");

            migrationBuilder.DropTable(
                name: "ContinentMaster");
        }
    }
}
