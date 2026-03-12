using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pulse.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class FixWrongTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "AspNetRolePermissions");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "AspNetPermissions");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "AspNetRolePermissions",
                newName: "IX_AspNetRolePermissions_PermissionId");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetRolePermissions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedDate",
                table: "AspNetRolePermissions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetPermissions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRolePermissions",
                table: "AspNetRolePermissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetPermissions",
                table: "AspNetPermissions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AspNetPermissionCategories",
                columns: table => new
                {
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetPermissionCategories", x => x.Category);
                });

            migrationBuilder.InsertData(
                table: "AspNetPermissionCategories",
                column: "Category",
                values: new object[]
                {
                    "Audit & Logging",
                    "Company Management",
                    "Division Management",
                    "Group Management",
                    "Inventory",
                    "Notifications",
                    "Production",
                    "Pulse AI",
                    "Reports & Analytics",
                    "Role & Permissions",
                    "System Settings",
                    "Technical",
                    "User Management"
                });

            migrationBuilder.InsertData(
                table: "AspNetPermissions",
                columns: new[] { "Id", "Category", "CreatedDate", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "User Management", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View list of users and their details", "Users:View" },
                    { 2, "User Management", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create new user accounts", "Users:Create" },
                    { 3, "User Management", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit existing user profiles and roles", "Users:Edit" },
                    { 4, "User Management", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete user accounts", "Users:Delete" },
                    { 5, "User Management", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Force password reset for any user", "Users:ResetPassword" },
                    { 6, "Role & Permissions", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View all roles and their assigned permissions", "Roles:View" },
                    { 7, "Role & Permissions", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create new roles", "Roles:Create" },
                    { 8, "Role & Permissions", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit role names and assign/remove permissions", "Roles:Edit" },
                    { 9, "Role & Permissions", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete roles (only if not in use)", "Roles:Delete" },
                    { 10, "Role & Permissions", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Full access to view/create/edit/delete permissions", "Permissions:Manage" },
                    { 11, "System Settings", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View application configuration and system settings", "Settings:View" },
                    { 12, "System Settings", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Modify global application settings", "Settings:Edit" },
                    { 13, "Reports & Analytics", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Access and generate reports and dashboards", "Reports:View" },
                    { 14, "Reports & Analytics", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Export reports to PDF, CSV, Excel", "Reports:Export" },
                    { 15, "Audit & Logging", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View audit logs and user activity history", "Audit:View" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRolePermissions_RoleId_PermissionId",
                table: "AspNetRolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetPermissions_Category",
                table: "AspNetPermissions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetPermissions_Name",
                table: "AspNetPermissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetPermissionCategories_Category",
                table: "AspNetPermissionCategories",
                column: "Category",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetPermissions_AspNetPermissionCategories_Category",
                table: "AspNetPermissions",
                column: "Category",
                principalTable: "AspNetPermissionCategories",
                principalColumn: "Category",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRolePermissions_AspNetPermissions_PermissionId",
                table: "AspNetRolePermissions",
                column: "PermissionId",
                principalTable: "AspNetPermissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetPermissions_AspNetPermissionCategories_Category",
                table: "AspNetPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRolePermissions_AspNetPermissions_PermissionId",
                table: "AspNetRolePermissions");

            migrationBuilder.DropTable(
                name: "AspNetPermissionCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRolePermissions",
                table: "AspNetRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRolePermissions_RoleId_PermissionId",
                table: "AspNetRolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetPermissions",
                table: "AspNetPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetPermissions_Category",
                table: "AspNetPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetPermissions_Name",
                table: "AspNetPermissions");

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.RenameTable(
                name: "AspNetRolePermissions",
                newName: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "AspNetPermissions",
                newName: "Permissions");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRolePermissions_PermissionId",
                table: "RolePermissions",
                newName: "IX_RolePermissions_PermissionId");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RolePermissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedDate",
                table: "RolePermissions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Permissions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
