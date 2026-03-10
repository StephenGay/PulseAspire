using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

internal class AspNetRolePermissionConfigurationMap : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("AspNetRolePermissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rp => rp.RoleId)
            .IsRequired()
            .HasMaxLength(450);     // Matches typical ASP.NET Identity Role Id length

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.Property(rp => rp.AssignedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Foreign keys + relationships
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);   // or Restrict if you prefer to block deletion

        // Optional but recommended: composite unique index to prevent duplicate role → permission assignments
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        
    }
}

