using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Add_AI_Chat_IClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RollerShaftMaster",
                table: "RollerShaftMaster");

            migrationBuilder.RenameTable(
                name: "RollerShaftMaster",
                newName: "RollerShafts",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "ClientRollerSpecificationID",
                schema: "dbo",
                table: "RollerShafts",
                newName: "ClientRollerSpecificationId");

            migrationBuilder.AddColumn<string>(
                name: "RawContent",
                schema: "pai",
                table: "FlapperMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TorqueCapacity",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "Maximum torque (Nm) the shaft can transmit without failure",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "SurfaceRoughness",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 3.2m,
                comment: "Surface roughness Ra in micrometres (µm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "ShaftPosition",
                schema: "dbo",
                table: "RollerShafts",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "Center",
                comment: "Which side the shaft is on: 'Left', 'Right', or 'Center' for concentric shafts",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "RadialOffset",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: true,
                defaultValue: 0m,
                comment: "Offset from the roller centerline where positive = outward radially (mm)",
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OuterDiameterTolerance",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(6,3)",
                nullable: false,
                defaultValue: 0.1m,
                comment: "Tolerance for outer diameter (± mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "OuterDiameter",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                comment: "Outer diameter of the shaft (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ModifiedDate",
                schema: "dbo",
                table: "RollerShafts",
                type: "datetime2",
                nullable: true,
                comment: "UTC timestamp when the record was last modified",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "dbo",
                table: "RollerShafts",
                type: "nvarchar(100)",
                nullable: true,
                comment: "User ID or name who last modified this record",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Material",
                schema: "dbo",
                table: "RollerShafts",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "45Steel",
                comment: "Material composition (e.g., '20Cr', '45Steel', 'AlloyX')",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Length",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                comment: "Total length of the shaft including end faces (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "KeywayWidth",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "Width of the keyway if present (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "KeywayTolerance",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(6,3)",
                nullable: false,
                defaultValue: 0.1m,
                comment: "Tolerance for keyway dimensions (± mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "KeywayLength",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "Length of the keyway along the shaft (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "KeywayDepth",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "Depth of the keyway (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "RollerShafts",
                type: "bit",
                nullable: false,
                defaultValue: true,
                comment: "Indicates whether the shaft is active",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "HeatTreatmentState",
                schema: "dbo",
                table: "RollerShafts",
                type: "int",
                nullable: false,
                defaultValue: 1,
                comment: "Heat-treatment state of the shaft",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "EndFaceThickness",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "Thickness of the shaft end faces for mounting/bearing clearance (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "dbo",
                table: "RollerShafts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                comment: "UTC timestamp when the record was created",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "RollerShafts",
                type: "nvarchar(100)",
                nullable: true,
                comment: "User ID or name who created this record",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverOverbuild",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: true,
                comment: "Overbuild dimension of the cover (mm)",
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoverCompoundThickness",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(8,2)",
                nullable: true,
                comment: "Thickness of the cover compound (mm)",
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoverCompound",
                schema: "dbo",
                table: "RollerShafts",
                type: "nvarchar(100)",
                nullable: true,
                comment: "Cover compound used on the shaft if applicable",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Coating",
                schema: "dbo",
                table: "RollerShafts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Surface coating applied to the shaft",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "BearingBoreTolerance",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(6,3)",
                nullable: false,
                defaultValue: 0.05m,
                comment: "Tolerance for bearing bore diameter (± mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "BearingBoreDiameter",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(10,2)",
                nullable: false,
                comment: "Inner bore diameter that mates with the bearing (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "AxialPosition",
                schema: "dbo",
                table: "RollerShafts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "Position along the roller axis where 0 = left end, positive = rightward (mm)",
                oldClrType: typeof(double),
                oldType: "float");

            //migrationBuilder.AddColumn<int>(
            //    name: "ClientRollerSpecificationID",
            //    schema: "dbo",
            //    table: "RollerShafts",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RollerShafts",
                schema: "dbo",
                table: "RollerShafts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "InventoryTypes",
                columns: table => new
                {
                    idInventoryType = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultCostGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeepStock = table.Column<bool>(type: "bit", nullable: false),
                    ValuationMethodID = table.Column<int>(type: "int", nullable: false),
                    CountMethodID = table.Column<int>(type: "int", nullable: false),
                    DefaultVarianceGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultRevaluationGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultAccrualGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InventoryTrackingGroupID = table.Column<int>(type: "int", nullable: true),
                    TrackByGroup = table.Column<bool>(type: "bit", nullable: false),
                    UsedByDivision = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTypes", x => x.idInventoryType);
                });

            migrationBuilder.CreateTable(
                name: "InventoryGroups",
                columns: table => new
                {
                    idInventoryGroup = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InventoryTypeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryGroups", x => x.idInventoryGroup);
                    table.ForeignKey(
                        name: "FK_InventoryGroups_InventoryTypes_InventoryTypeID",
                        column: x => x.InventoryTypeID,
                        principalTable: "InventoryTypes",
                        principalColumn: "idInventoryType",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTypeDivisionSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryTypeID = table.Column<long>(type: "bigint", nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ValuationMethodID = table.Column<int>(type: "int", nullable: true),
                    CountMethodID = table.Column<int>(type: "int", nullable: true),
                    KeepStock = table.Column<bool>(type: "bit", nullable: true),
                    UsedByDivision = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TrackByGroup = table.Column<bool>(type: "bit", nullable: true),
                    InventoryTrackingGroupID = table.Column<int>(type: "int", nullable: true),
                    DefaultCostGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultVarianceGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultRevaluationGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultAccrualGL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTypeDivisionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTypeDivisionSettings_DivisionMaster_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTypeDivisionSettings_InventoryTypes_InventoryTypeID",
                        column: x => x.InventoryTypeID,
                        principalTable: "InventoryTypes",
                        principalColumn: "idInventoryType",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryGroupDivisionSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryGroupID = table.Column<long>(type: "bigint", nullable: false),
                    DivisionID = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    SalesGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostGL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryGroupDivisionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryGroupDivisionSettings_DivisionMaster_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "DivisionMaster",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryGroupDivisionSettings_InventoryGroups_InventoryGroupID",
                        column: x => x.InventoryGroupID,
                        principalTable: "InventoryGroups",
                        principalColumn: "idInventoryGroup",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    idInventoryItem = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InventoryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InventoryTypeID = table.Column<long>(type: "bigint", nullable: true),
                    InventoryGroupID = table.Column<long>(type: "bigint", nullable: true),
                    InventoryUnitID = table.Column<long>(type: "bigint", nullable: true),
                    SalesGL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostGL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.idInventoryItem);
                    table.ForeignKey(
                        name: "FK_InventoryItems_InventoryGroups_InventoryGroupID",
                        column: x => x.InventoryGroupID,
                        principalTable: "InventoryGroups",
                        principalColumn: "idInventoryGroup");
                    table.ForeignKey(
                        name: "FK_InventoryItems_InventoryTypes_InventoryTypeID",
                        column: x => x.InventoryTypeID,
                        principalTable: "InventoryTypes",
                        principalColumn: "idInventoryType");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RollerShaft_AxialPosition",
                schema: "dbo",
                table: "RollerShafts",
                column: "AxialPosition");

            migrationBuilder.CreateIndex(
                name: "IX_RollerShaft_Material",
                schema: "dbo",
                table: "RollerShafts",
                column: "Material");

            migrationBuilder.CreateIndex(
                name: "IX_RollerShaft_OuterDiameter",
                schema: "dbo",
                table: "RollerShafts",
                column: "OuterDiameter");

            migrationBuilder.CreateIndex(
                name: "IX_RollerShaft_PositionAxial",
                schema: "dbo",
                table: "RollerShafts",
                columns: new[] { "ShaftPosition", "AxialPosition" });

            migrationBuilder.CreateIndex(
                name: "IX_RollerShaft_ShaftPosition",
                schema: "dbo",
                table: "RollerShafts",
                column: "ShaftPosition");

            migrationBuilder.CreateIndex(
                name: "IX_RollerShafts_ClientRollerSpecificationId",
                schema: "dbo",
                table: "RollerShafts",
                column: "ClientRollerSpecificationId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RollerShaft_BoreLessThanOuter",
                schema: "dbo",
                table: "RollerShafts",
                sql: "[BearingBoreDiameter] < [OuterDiameter]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RollerShaft_DiameterPositive",
                schema: "dbo",
                table: "RollerShafts",
                sql: "[OuterDiameter] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RollerShaft_KeywayNonNegative",
                schema: "dbo",
                table: "RollerShafts",
                sql: "[KeywayWidth] >= 0 AND [KeywayDepth] >= 0 AND [KeywayLength] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RollerShaft_LengthPositive",
                schema: "dbo",
                table: "RollerShafts",
                sql: "[Length] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RollerShaft_ValidPosition",
                schema: "dbo",
                table: "RollerShafts",
                sql: "[ShaftPosition] IN ('Left', 'Center', 'Right')");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryGroupDivisionSettings_DivisionID",
                table: "InventoryGroupDivisionSettings",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryGroupDivisionSettings_InventoryGroupID_DivisionID",
                table: "InventoryGroupDivisionSettings",
                columns: new[] { "InventoryGroupID", "DivisionID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryGroups_InventoryTypeID",
                table: "InventoryGroups",
                column: "InventoryTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_InventoryGroupID",
                table: "InventoryItems",
                column: "InventoryGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_InventoryTypeID",
                table: "InventoryItems",
                column: "InventoryTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTypeDivisionSettings_DivisionID",
                table: "InventoryTypeDivisionSettings",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTypeDivisionSettings_InventoryTypeID_DivisionID",
                table: "InventoryTypeDivisionSettings",
                columns: new[] { "InventoryTypeID", "DivisionID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RollerShaft_RollerSpecification",
                schema: "dbo",
                table: "RollerShafts",
                column: "ClientRollerSpecificationId",
                principalTable: "ClientRollerSpecificationMaster",
                principalColumn: "ClientRollerSpecificationID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollerShaft_RollerSpecification",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropTable(
                name: "InventoryGroupDivisionSettings");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryTypeDivisionSettings");

            migrationBuilder.DropTable(
                name: "InventoryGroups");

            migrationBuilder.DropTable(
                name: "InventoryTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RollerShafts",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropIndex(
                name: "IX_RollerShaft_AxialPosition",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropIndex(
                name: "IX_RollerShaft_Material",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropIndex(
                name: "IX_RollerShaft_OuterDiameter",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropIndex(
                name: "IX_RollerShaft_PositionAxial",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropIndex(
                name: "IX_RollerShaft_ShaftPosition",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropIndex(
                name: "IX_RollerShafts_ClientRollerSpecificationId",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RollerShaft_BoreLessThanOuter",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RollerShaft_DiameterPositive",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RollerShaft_KeywayNonNegative",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RollerShaft_LengthPositive",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RollerShaft_ValidPosition",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.DropColumn(
                name: "RawContent",
                schema: "pai",
                table: "FlapperMessages");

            migrationBuilder.DropColumn(
                name: "ClientRollerSpecificationID",
                schema: "dbo",
                table: "RollerShafts");

            migrationBuilder.RenameTable(
                name: "RollerShafts",
                schema: "dbo",
                newName: "RollerShaftMaster");

            migrationBuilder.RenameColumn(
                name: "ClientRollerSpecificationId",
                table: "RollerShaftMaster",
                newName: "ClientRollerSpecificationID");

            migrationBuilder.AlterColumn<double>(
                name: "TorqueCapacity",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldDefaultValue: 0m,
                oldComment: "Maximum torque (Nm) the shaft can transmit without failure");

            migrationBuilder.AlterColumn<double>(
                name: "SurfaceRoughness",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldDefaultValue: 3.2m,
                oldComment: "Surface roughness Ra in micrometres (µm)");

            migrationBuilder.AlterColumn<string>(
                name: "ShaftPosition",
                table: "RollerShaftMaster",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldDefaultValue: "Center",
                oldComment: "Which side the shaft is on: 'Left', 'Right', or 'Center' for concentric shafts");

            migrationBuilder.AlterColumn<double>(
                name: "RadialOffset",
                table: "RollerShaftMaster",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldDefaultValue: 0m,
                oldComment: "Offset from the roller centerline where positive = outward radially (mm)");

            migrationBuilder.AlterColumn<double>(
                name: "OuterDiameterTolerance",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,3)",
                oldDefaultValue: 0.1m,
                oldComment: "Tolerance for outer diameter (± mm)");

            migrationBuilder.AlterColumn<double>(
                name: "OuterDiameter",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldComment: "Outer diameter of the shaft (mm)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ModifiedDate",
                table: "RollerShaftMaster",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "UTC timestamp when the record was last modified");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "RollerShaftMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true,
                oldComment: "User ID or name who last modified this record");

            migrationBuilder.AlterColumn<string>(
                name: "Material",
                table: "RollerShaftMaster",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldDefaultValue: "45Steel",
                oldComment: "Material composition (e.g., '20Cr', '45Steel', 'AlloyX')");

            migrationBuilder.AlterColumn<double>(
                name: "Length",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldComment: "Total length of the shaft including end faces (mm)");

            migrationBuilder.AlterColumn<double>(
                name: "KeywayWidth",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldDefaultValue: 0m,
                oldComment: "Width of the keyway if present (mm)");

            migrationBuilder.AlterColumn<double>(
                name: "KeywayTolerance",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,3)",
                oldDefaultValue: 0.1m,
                oldComment: "Tolerance for keyway dimensions (± mm)");

            migrationBuilder.AlterColumn<double>(
                name: "KeywayLength",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldDefaultValue: 0m,
                oldComment: "Length of the keyway along the shaft (mm)");

            migrationBuilder.AlterColumn<double>(
                name: "KeywayDepth",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldDefaultValue: 0m,
                oldComment: "Depth of the keyway (mm)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "RollerShaftMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true,
                oldComment: "Indicates whether the shaft is active");

            migrationBuilder.AlterColumn<int>(
                name: "HeatTreatmentState",
                table: "RollerShaftMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1,
                oldComment: "Heat-treatment state of the shaft");

            migrationBuilder.AlterColumn<double>(
                name: "EndFaceThickness",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldDefaultValue: 0m,
                oldComment: "Thickness of the shaft end faces for mounting/bearing clearance (mm)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "RollerShaftMaster",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()",
                oldComment: "UTC timestamp when the record was created");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "RollerShaftMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true,
                oldComment: "User ID or name who created this record");

            migrationBuilder.AlterColumn<double>(
                name: "CoverOverbuild",
                table: "RollerShaftMaster",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldComment: "Overbuild dimension of the cover (mm)");

            migrationBuilder.AlterColumn<double>(
                name: "CoverCompoundThickness",
                table: "RollerShaftMaster",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldComment: "Thickness of the cover compound (mm)");

            migrationBuilder.AlterColumn<string>(
                name: "CoverCompound",
                table: "RollerShaftMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true,
                oldComment: "Cover compound used on the shaft if applicable");

            migrationBuilder.AlterColumn<int>(
                name: "Coating",
                table: "RollerShaftMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0,
                oldComment: "Surface coating applied to the shaft");

            migrationBuilder.AlterColumn<double>(
                name: "BearingBoreTolerance",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,3)",
                oldDefaultValue: 0.05m,
                oldComment: "Tolerance for bearing bore diameter (± mm)");

            migrationBuilder.AlterColumn<double>(
                name: "BearingBoreDiameter",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldComment: "Inner bore diameter that mates with the bearing (mm)");

            migrationBuilder.AlterColumn<double>(
                name: "AxialPosition",
                table: "RollerShaftMaster",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldDefaultValue: 0m,
                oldComment: "Position along the roller axis where 0 = left end, positive = rightward (mm)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RollerShaftMaster",
                table: "RollerShaftMaster",
                column: "Id");
        }
    }
}
