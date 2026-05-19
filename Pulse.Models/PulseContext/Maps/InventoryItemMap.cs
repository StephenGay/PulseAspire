using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Inventory;

namespace Pulse.Models.PulseContext.Maps
{
    public class InventoryItemMap : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");
            builder.HasKey(e => e.IdInventoryItem);

            builder.Property(e => e.IdInventoryItem).HasColumnName("idInventoryItem");
            builder.Property(e => e.InventoryCode).HasColumnName("InventoryCode");
            builder.Property(e => e.Description).HasColumnName("Description");
            builder.Property(e => e.InventoryTypeID).HasColumnName("InventoryTypeID");
            builder.Property(e => e.InventoryGroupID).HasColumnName("InventoryGroupID");
            builder.Property(e => e.InventoryUnitID).HasColumnName("InventoryUnitID");
            builder.Property(e => e.SalesGL).HasColumnName("SalesGL");
            builder.Property(e => e.CostGL).HasColumnName("CostGL");

            builder.HasOne(i => i.InventoryType)
                   .WithMany(t => t.InventoryItems)
                   .HasForeignKey(i => i.InventoryTypeID);

            builder.HasOne(i => i.InventoryGroup)
                   .WithMany(g => g.InventoryItems)
                   .HasForeignKey(i => i.InventoryGroupID);
        }
    }
}