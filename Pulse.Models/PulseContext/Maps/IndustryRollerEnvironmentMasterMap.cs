using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Industries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class IndustryRollerEnvironmentMasterMap : IEntityTypeConfiguration<IndustryRollerEnvironment>
    {
        public void Configure(EntityTypeBuilder<IndustryRollerEnvironment> builder)
        {
            builder.ToTable("IndustryRollerEnvironmentMaster");

            // Primary Key
            builder.HasKey(ire => ire.IndustryRollerEnvironmentId);

            // Properties
            builder.Property(ire => ire.IndustryRollerEnvironmentId)
                   .HasColumnName("IndustryRollerEnvironmentId")
                   .ValueGeneratedNever();

            builder.Property(ire => ire.RollerTypeId)
                   .IsRequired()
                   .HasColumnName("RollerTypeId");

            builder.Property(ire => ire.IndustryProcessId)
                   .IsRequired()
                   .HasColumnName("IndustryProcessId");

            builder.Property(ire => ire.EnvironmentConditions)
                   .HasMaxLength(255)
                   .HasColumnName("EnvironmentConditions");

            builder.Property(ire => ire.IsActive)
                   .HasDefaultValue(true)
                   .HasColumnName("IsActive");

            // Indexes for foreign keys and common queries
            builder.HasIndex(ire => ire.RollerTypeId)
                   .HasDatabaseName("IX_IndustryRollerEnvironment_RollerTypeId");

            builder.HasIndex(ire => ire.IndustryProcessId)
                   .HasDatabaseName("IX_IndustryRollerEnvironment_IndustryProcessId");

            builder.HasIndex(ire => ire.IsActive)
                   .HasDatabaseName("IX_IndustryRollerEnvironment_IsActive");

            // Composite index for common queries
            builder.HasIndex(ire => new { ire.IndustryProcessId, ire.RollerTypeId })
                   .HasDatabaseName("IX_IndustryRollerEnvironment_ProcessRoller")
                   .IsUnique();

            // Relationships
            builder.HasOne(ire => ire.RollerType)
                   .WithMany(rt => rt.IndustryRollerEnvironments)
                   .HasForeignKey(ire => ire.RollerTypeId)
                   .HasConstraintName("FK_IndustryRollerEnvironment_RollerType")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ire => ire.IndustryProcess)
                   .WithMany(ip => ip.IndustryRollerEnvironments)
                   .HasForeignKey(ire => ire.IndustryProcessId)
                   .HasConstraintName("FK_IndustryRollerEnvironment_IndustryProcess")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ire => ire.IndustryRecommendedCovers)
                   .WithOne(irc => irc.IndustryRollerEnvironment)
                   .HasForeignKey(irc => irc.IndustryRollerEnvironmentId)
                   .HasConstraintName("FK_IndustryRecommendedCover_IndustryRollerEnvironment")
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
