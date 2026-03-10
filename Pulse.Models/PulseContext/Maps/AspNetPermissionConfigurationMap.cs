using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class AspNetPermissionConfigurationMap : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("AspNetPermissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");  // or use .HasDefaultValue(DateTime.UtcNow) if preferred

        builder.HasOne(p => p.CategoryNavigation)
            .WithMany()                           // no collection on PermissionCategory side
            .HasForeignKey(p => p.Category)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional: unique index on Name (common for permissions)
        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasData(
            // User Management
            new Permission
            {
                Id = 1,
                Name = "Users:View",
                Description = "View list of users and their details",
                Category = "User Management",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 2,
                Name = "Users:Create",
                Description = "Create new user accounts",
                Category = "User Management",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 3,
                Name = "Users:Edit",
                Description = "Edit existing user profiles and roles",
                Category = "User Management",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 4,
                Name = "Users:Delete",
                Description = "Delete user accounts",
                Category = "User Management",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 5,
                Name = "Users:ResetPassword",
                Description = "Force password reset for any user",
                Category = "User Management",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // Role & Permissions
            new Permission
            {
                Id = 6,
                Name = "Roles:View",
                Description = "View all roles and their assigned permissions",
                Category = "Role & Permissions",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 7,
                Name = "Roles:Create",
                Description = "Create new roles",
                Category = "Role & Permissions",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 8,
                Name = "Roles:Edit",
                Description = "Edit role names and assign/remove permissions",
                Category = "Role & Permissions",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 9,
                Name = "Roles:Delete",
                Description = "Delete roles (only if not in use)",
                Category = "Role & Permissions",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 10,
                Name = "Permissions:Manage",
                Description = "Full access to view/create/edit/delete permissions",
                Category = "Role & Permissions",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // System Settings
            new Permission
            {
                Id = 11,
                Name = "Settings:View",
                Description = "View application configuration and system settings",
                Category = "System Settings",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 12,
                Name = "Settings:Edit",
                Description = "Modify global application settings",
                Category = "System Settings",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Reports & Analytics
            new Permission
            {
                Id = 13,
                Name = "Reports:View",
                Description = "Access and generate reports and dashboards",
                Category = "Reports & Analytics",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = 14,
                Name = "Reports:Export",
                Description = "Export reports to PDF, CSV, Excel",
                Category = "Reports & Analytics",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Audit & Logging
            new Permission
            {
                Id = 15,
                Name = "Audit:View",
                Description = "View audit logs and user activity history",
                Category = "Audit & Logging",
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }

        );
    }
}
