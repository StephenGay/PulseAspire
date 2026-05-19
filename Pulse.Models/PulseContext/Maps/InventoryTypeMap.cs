using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Inventory;

namespace Pulse.Models.PulseContext.Maps
{
    public class InventoryTypeMap : IEntityTypeConfiguration<InventoryType>
    {
        public void Configure(EntityTypeBuilder<InventoryType> builder)
        {
            builder.ToTable("InventoryTypes");
            builder.HasKey(e => e.IdInventoryType);

            builder.Property(e => e.IdInventoryType).HasColumnName("idInventoryType");
            builder.Property(e => e.Description).HasColumnName("Description");
            builder.Property(e => e.DefaultCostGL).HasColumnName("DefaultCostGL");
            builder.Property(e => e.KeepStock).HasColumnName("KeepStock");
            builder.Property(e => e.UsedByDivision).HasColumnName("UsedByDivision");
            builder.Property(e => e.ValuationMethodID).HasColumnName("ValuationMethodID");
            builder.Property(e => e.CountMethodID).HasColumnName("CountMethodID");
            builder.Property(e => e.DefaultVarianceGL).HasColumnName("DefaultVarianceGL");
            builder.Property(e => e.DefaultRevaluationGL).HasColumnName("DefaultRevaluationGL");
            builder.Property(e => e.DefaultAccrualGL).HasColumnName("DefaultAccrualGL");
            builder.Property(e => e.InventoryTrackingGroupID).HasColumnName("InventoryTrackingGroupID");
            builder.Property(e => e.TrackByGroup).HasColumnName("TrackByGroup"); // adjust if column name differs

            // Relationships
            builder.HasMany(e => e.InventoryGroups)
                   .WithOne(g => g.InventoryType)
                   .HasForeignKey(g => g.InventoryTypeID);

            builder.HasMany(e => e.InventoryItems)
                   .WithOne(i => i.InventoryType)
                   .HasForeignKey(i => i.InventoryTypeID);
        }
    }
}