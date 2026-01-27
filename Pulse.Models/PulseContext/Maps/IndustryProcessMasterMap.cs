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
    public class IndustryProcessMasterMap : IEntityTypeConfiguration<IndustryProcess>
    {
        public void Configure(EntityTypeBuilder<IndustryProcess> builder)
        {
            builder.ToTable("IndustryProcessMaster");

            // Primary Key
            builder.HasKey(ip => ip.IndustryProcessID);

            // Properties
            builder.Property(ip => ip.IndustryProcessID)
                   .HasColumnName("IndustryProcessID")
                   .ValueGeneratedNever();

            builder.Property(ip => ip.IndustryID)
                   .IsRequired()
                   .HasColumnName("IndustryID");

            builder.Property(ip => ip.ProcessName)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("ProcessName");

            builder.Property(ip => ip.IsActive)
                   .HasDefaultValue(true)
                   .HasColumnName("IsActive");

            // Indexes
            builder.HasIndex(ip => ip.IndustryID)
                   .HasDatabaseName("IX_IndustryProcessMaster_IndustryID");

            builder.HasIndex(ip => ip.IsActive)
                   .HasDatabaseName("IX_IndustryProcessMaster_IsActive");

            builder.HasIndex(ip => new { ip.IndustryID, ip.ProcessName })
                   .HasDatabaseName("IX_IndustryProcessMaster_Industry_ProcessName")
                   .IsUnique();

            // Relationships
            builder.HasOne(ip => ip.Industry)
                   .WithMany(i => i.IndustryProcesses)
                   .HasForeignKey(ip => ip.IndustryID)
                   .HasConstraintName("FK_IndustryProcess_Industry")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ip => ip.IndustryRollerEnvironments)
                   .WithOne(ire => ire.IndustryProcess)
                   .HasForeignKey(ire => ire.IndustryProcessId)
                   .HasConstraintName("FK_IndustryRollerEnvironment_IndustryProcess")
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
