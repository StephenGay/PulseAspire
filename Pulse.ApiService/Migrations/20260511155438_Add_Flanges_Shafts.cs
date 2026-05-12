using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Add_Flanges_Shafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RollerShaftMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientRollerSpecificationID = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<double>(type: "float", nullable: false),
                    OuterDiameter = table.Column<double>(type: "float", nullable: false),
                    BearingBoreDiameter = table.Column<double>(type: "float", nullable: false),
                    KeywayWidth = table.Column<double>(type: "float", nullable: false),
                    KeywayDepth = table.Column<double>(type: "float", nullable: false),
                    KeywayLength = table.Column<double>(type: "float", nullable: false),
                    EndFaceThickness = table.Column<double>(type: "float", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurfaceRoughness = table.Column<double>(type: "float", nullable: false),
                    OuterDiameterTolerance = table.Column<double>(type: "float", nullable: false),
                    BearingBoreTolerance = table.Column<double>(type: "float", nullable: false),
                    KeywayTolerance = table.Column<double>(type: "float", nullable: false),
                    CoverCompound = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverCompoundThickness = table.Column<double>(type: "float", nullable: true),
                    CoverOverbuild = table.Column<double>(type: "float", nullable: true),
                    AxialPosition = table.Column<double>(type: "float", nullable: false),
                    ShaftPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RadialOffset = table.Column<double>(type: "float", nullable: true),
                    HeatTreatmentState = table.Column<int>(type: "int", nullable: false),
                    Coating = table.Column<int>(type: "int", nullable: false),
                    TorqueCapacity = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollerShaftMaster", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RollerShaftMaster");
        }
    }
}
