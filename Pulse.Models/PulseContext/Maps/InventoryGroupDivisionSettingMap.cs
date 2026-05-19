using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Inventory;

namespace Pulse.Models.PulseContext.Maps
{
    public class InventoryGroupDivisionSettingMap : IEntityTypeConfiguration<InventoryGroupDivisionSetting>
    {
        public void Configure(EntityTypeBuilder<InventoryGroupDivisionSetting> builder)
        {
            builder.ToTable("InventoryGroupDivisionSettings");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.DivisionID).HasMaxLength(5).IsRequired();

            builder.HasIndex(e => new { e.InventoryGroupID, e.DivisionID }).IsUnique();

            builder.HasOne(e => e.InventoryGroup)
                   .WithMany(g => g.DivisionSettings)
                   .HasForeignKey(e => e.InventoryGroupID);

            builder.HasOne(e => e.Division)
                   .WithMany()
                   .HasForeignKey(e => e.DivisionID);
        }
    }
}