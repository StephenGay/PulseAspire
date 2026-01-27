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
    public class IndustryMasterMap : IEntityTypeConfiguration<Industry>
    {
        public void Configure(EntityTypeBuilder<Industry> builder)
        {
            builder.ToTable("IndustryMaster");

            // Primary Key
            builder.HasKey(i => i.IndustryID);

            // Properties
            builder.Property(i => i.IndustryID)
                   .HasColumnName("IndustryID")
                   .ValueGeneratedNever();

            builder.Property(i => i.IndustryName)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("IndustryName");

            builder.Property(i => i.IsActive)
                   .HasDefaultValue(true)
                   .HasColumnName("IsActive");

            // Indexes
            builder.HasIndex(i => i.IndustryName)
                   .HasDatabaseName("IX_IndustryMaster_IndustryName")
                   .IsUnique();

            builder.HasIndex(i => i.IsActive)
                   .HasDatabaseName("IX_IndustryMaster_IsActive");

            // Relationships
            builder.HasMany(i => i.Customers)
                   .WithOne(c => c.Industry)
                   .HasForeignKey(c => c.IndustryID)
                   .HasConstraintName("FK_Customer_Industry")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.IndustryProcesses)
                   .WithOne(ip => ip.Industry)
                   .HasForeignKey(ip => ip.IndustryID)
                   .HasConstraintName("FK_IndustryProcess_Industry")
                   .OnDelete(DeleteBehavior.Restrict);

            // Note: IndustryRollerEnvironment relationship removed
            // IndustryRollerEnvironment now accesses Industry through IndustryProcess
            // See: IndustryRollerEnvironment.cs and IndustryRollerEnvironmentMasterMap.cs
        }
    }
}
