using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _2310252 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientRollerSpecificationMaster",
                columns: table => new
                {
                    ClientRollerSpecificationID = table.Column<int>(type: "int", nullable: false),
                    FullClientID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RollerType = table.Column<int>(type: "int", nullable: true),
                    RollerFunction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessID = table.Column<int>(type: "int", nullable: true),
                    ProcessName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MachineType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ArticleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DrawingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShellDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShellLength = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShellWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShellMinimumDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShellTypeID = table.Column<int>(type: "int", nullable: true),
                    ShellTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompoundCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    HardnessRequired = table.Column<int>(type: "int", nullable: true),
                    HardnessToleranceAllowed = table.Column<int>(type: "int", nullable: true),
                    HardnessTypeRequired = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoverLength = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CoverDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CoverLeftOffset = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CoverMinimumDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRollerSpecificationMaster", x => x.ClientRollerSpecificationID);
                    table.ForeignKey(
                        name: "FK_ClientRollerSpecificationMaster_ClientMaster_FullClientID",
                        column: x => x.FullClientID,
                        principalTable: "ClientMaster",
                        principalColumn: "FullClientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientRollerSpecificationMaster_CompoundMaster_CompoundCode",
                        column: x => x.CompoundCode,
                        principalTable: "CompoundMaster",
                        principalColumn: "CompoundCode");
                });

            migrationBuilder.CreateTable(
                name: "ClientRollerMaster",
                columns: table => new
                {
                    ClientRollerID = table.Column<int>(type: "int", nullable: false),
                    ClientRollerSpecificationID = table.Column<int>(type: "int", nullable: false),
                    ClientRollerNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShellDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShellLength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShellWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoverDiameter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShellDefects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRollerMaster", x => x.ClientRollerID);
                    table.ForeignKey(
                        name: "FK_ClientRollerMaster_ClientRollerSpecificationMaster_ClientRollerSpecificationID",
                        column: x => x.ClientRollerSpecificationID,
                        principalTable: "ClientRollerSpecificationMaster",
                        principalColumn: "ClientRollerSpecificationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerMaster_ClientRollerSpecificationID",
                table: "ClientRollerMaster",
                column: "ClientRollerSpecificationID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerSpecificationMaster_CompoundCode",
                table: "ClientRollerSpecificationMaster",
                column: "CompoundCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRollerSpecificationMaster_FullClientID",
                table: "ClientRollerSpecificationMaster",
                column: "FullClientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientRollerMaster");

            migrationBuilder.DropTable(
                name: "ClientRollerSpecificationMaster");
        }
    }
}
