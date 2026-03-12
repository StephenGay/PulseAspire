using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class AspNetPermissionCategoryConfigurationMap : IEntityTypeConfiguration<PermissionCategory>
{
    public void Configure(EntityTypeBuilder<PermissionCategory> builder)
    {
        builder.ToTable("AspNetPermissionCategories");  // Suggested table name – adjust if you prefer something else

        // Primary key = the Category string itself (natural key)
        builder.HasKey(pc => pc.Category);

        // Since it's marked required in the class + string is nullable by default in C#,
        // we explicitly enforce it here too (good practice)
        builder.Property(pc => pc.Category)
            .IsRequired()
            .HasMaxLength(100);  // ← Choose a reasonable length; adjust to match your needs
                                 // .ValueGeneratedNever();  // Optional: explicitly state no value generation (default for string PKs anyway)

        // Optional: make it case-insensitive unique (common for category names)
        // SQL Server is case-insensitive by default in most collations, but this makes intent clear
        builder.HasIndex(pc => pc.Category)
            .IsUnique();

        builder.HasData(
            new PermissionCategory { Category = "User Management" },
            new PermissionCategory { Category = "Group Management" },
            new PermissionCategory { Category = "Company Management" },
            new PermissionCategory { Category = "Division Management" },
            new PermissionCategory { Category = "Inventory" },
            new PermissionCategory { Category = "Pulse AI" },
            new PermissionCategory { Category = "Production" },
            new PermissionCategory { Category = "Notifications" },
            new PermissionCategory { Category = "Technical" },
            new PermissionCategory { Category = "Audit & Logging" },
            new PermissionCategory { Category = "Reports & Analytics" },
            new PermissionCategory { Category = "System Settings" },
            new PermissionCategory { Category = "Role & Permissions" }
        );
    }
}

