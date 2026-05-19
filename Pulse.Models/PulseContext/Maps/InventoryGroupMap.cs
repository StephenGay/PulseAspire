using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Inventory;

namespace Pulse.Models.PulseContext.Maps
{
    public class InventoryGroupMap : IEntityTypeConfiguration<InventoryGroup>
    {
        public void Configure(EntityTypeBuilder<InventoryGroup> builder)
        {
            builder.ToTable("InventoryGroups");
            builder.HasKey(e => e.IdInventoryGroup);

            builder.Property(e => e.IdInventoryGroup).HasColumnName("idInventoryGroup");
            builder.Property(e => e.Description).HasColumnName("Description");
            builder.Property(e => e.SalesGL).HasColumnName("SalesGL");
            builder.Property(e => e.CostGL).HasColumnName("CostGL");
            builder.Property(e => e.InventoryTypeID).HasColumnName("InventoryTypeID");

            builder.HasOne(g => g.InventoryType)
                   .WithMany(t => t.InventoryGroups)
                   .HasForeignKey(g => g.InventoryTypeID);
        }
    }
}