using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _250126 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientMaster_IndustryMaster_IndustryID",
                table: "ClientMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryProcessMaster_IndustryMaster_IndustryID",
                table: "IndustryProcessMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRecommendedCoverMaster_IndustryRollerEnvironmentMaster_IndustryRollerEnvironmentId",
                table: "IndustryRecommendedCoverMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRollerEnvironmentMaster_IndustryMaster_IndustryId",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRollerEnvironmentMaster_IndustryProcessMaster_IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRollerEnvironmentMaster_RollerTypeMaster_RollerTypeId",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryRollerEnvironmentMaster_IndustryId",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.RenameIndex(
                name: "IX_IndustryRollerEnvironmentMaster_RollerTypeId",
                table: "IndustryRollerEnvironmentMaster",
                newName: "IX_IndustryRollerEnvironment_RollerTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_IndustryRollerEnvironmentMaster_IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster",
                newName: "IX_IndustryRollerEnvironment_IndustryProcessId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "IndustryRollerEnvironmentMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "IndustryProcessMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "IndustryMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CountryMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ContinentMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CompanyMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ClientContactMaster",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRollerEnvironment_IsActive",
                table: "IndustryRollerEnvironmentMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRollerEnvironment_ProcessRoller",
                table: "IndustryRollerEnvironmentMaster",
                columns: new[] { "IndustryProcessId", "RollerTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryProcessMaster_Industry_ProcessName",
                table: "IndustryProcessMaster",
                columns: new[] { "IndustryID", "ProcessName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryProcessMaster_IsActive",
                table: "IndustryProcessMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryMaster_IndustryName",
                table: "IndustryMaster",
                column: "IndustryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryMaster_IsActive",
                table: "IndustryMaster",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Industry",
                table: "ClientMaster",
                column: "IndustryID",
                principalTable: "IndustryMaster",
                principalColumn: "IndustryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryProcess_Industry",
                table: "IndustryProcessMaster",
                column: "IndustryID",
                principalTable: "IndustryMaster",
                principalColumn: "IndustryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRecommendedCover_IndustryRollerEnvironment",
                table: "IndustryRecommendedCoverMaster",
                column: "IndustryRollerEnvironmentId",
                principalTable: "IndustryRollerEnvironmentMaster",
                principalColumn: "IndustryRollerEnvironmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRollerEnvironment_IndustryProcess",
                table: "IndustryRollerEnvironmentMaster",
                column: "IndustryProcessId",
                principalTable: "IndustryProcessMaster",
                principalColumn: "IndustryProcessID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRollerEnvironment_RollerType",
                table: "IndustryRollerEnvironmentMaster",
                column: "RollerTypeId",
                principalTable: "RollerTypeMaster",
                principalColumn: "RollerTypeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Industry",
                table: "ClientMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryProcess_Industry",
                table: "IndustryProcessMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRecommendedCover_IndustryRollerEnvironment",
                table: "IndustryRecommendedCoverMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRollerEnvironment_IndustryProcess",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryRollerEnvironment_RollerType",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryRollerEnvironment_IsActive",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryRollerEnvironment_ProcessRoller",
                table: "IndustryRollerEnvironmentMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryProcessMaster_Industry_ProcessName",
                table: "IndustryProcessMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryProcessMaster_IsActive",
                table: "IndustryProcessMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryMaster_IndustryName",
                table: "IndustryMaster");

            migrationBuilder.DropIndex(
                name: "IX_IndustryMaster_IsActive",
                table: "IndustryMaster");

            migrationBuilder.RenameIndex(
                name: "IX_IndustryRollerEnvironment_RollerTypeId",
                table: "IndustryRollerEnvironmentMaster",
                newName: "IX_IndustryRollerEnvironmentMaster_RollerTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_IndustryRollerEnvironment_IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster",
                newName: "IX_IndustryRollerEnvironmentMaster_IndustryProcessId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "IndustryRollerEnvironmentMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IndustryId",
                table: "IndustryRollerEnvironmentMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "IndustryProcessMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "IndustryMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CountryMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ContinentMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CompanyMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ClientContactMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryRollerEnvironmentMaster_IndustryId",
                table: "IndustryRollerEnvironmentMaster",
                column: "IndustryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientMaster_IndustryMaster_IndustryID",
                table: "ClientMaster",
                column: "IndustryID",
                principalTable: "IndustryMaster",
                principalColumn: "IndustryID");

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryProcessMaster_IndustryMaster_IndustryID",
                table: "IndustryProcessMaster",
                column: "IndustryID",
                principalTable: "IndustryMaster",
                principalColumn: "IndustryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRecommendedCoverMaster_IndustryRollerEnvironmentMaster_IndustryRollerEnvironmentId",
                table: "IndustryRecommendedCoverMaster",
                column: "IndustryRollerEnvironmentId",
                principalTable: "IndustryRollerEnvironmentMaster",
                principalColumn: "IndustryRollerEnvironmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRollerEnvironmentMaster_IndustryMaster_IndustryId",
                table: "IndustryRollerEnvironmentMaster",
                column: "IndustryId",
                principalTable: "IndustryMaster",
                principalColumn: "IndustryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRollerEnvironmentMaster_IndustryProcessMaster_IndustryProcessId",
                table: "IndustryRollerEnvironmentMaster",
                column: "IndustryProcessId",
                principalTable: "IndustryProcessMaster",
                principalColumn: "IndustryProcessID");

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryRollerEnvironmentMaster_RollerTypeMaster_RollerTypeId",
                table: "IndustryRollerEnvironmentMaster",
                column: "RollerTypeId",
                principalTable: "RollerTypeMaster",
                principalColumn: "RollerTypeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
