using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class WorkTypeMasterMap : IEntityTypeConfiguration<WorkType>
    {
        public void Configure(EntityTypeBuilder<WorkType> builder)
        {
            // Table mapping 
            builder.ToTable("WorkTypeMaster");

            // Primary Key (non-identity, as per [DatabaseGenerated(DatabaseGeneratedOption.None)])
            builder.HasKey(w => w.WorkTypeID);

            // Properties with constraints and defaults
            builder.Property(w => w.WorkTypeName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(w => w.TargetWorkingDays)
                .HasDefaultValue(0);

            builder.Property(w => w.WorkTypeMaterial)
                .HasMaxLength(50); // Optional: Add length if needed

            builder.Property(w => w.LockToProductCode)
                .HasMaxLength(3); // Optional: Adjust based on your needs

            builder.Property(w => w.SortOrder)
                .HasDefaultValue(0); // Optional default

            builder.Property(w => w.IsActive)
                .HasDefaultValue(true);

            // Indexes for performance (e.g., sorting or filtering on SortOrder or IsActive)
            builder.HasIndex(w => w.SortOrder)
                .HasDatabaseName("IX_WorkType_SortOrder");

            builder.HasIndex(w => w.IsActive)
                .HasDatabaseName("IX_WorkType_IsActive");
        }
    }
}
