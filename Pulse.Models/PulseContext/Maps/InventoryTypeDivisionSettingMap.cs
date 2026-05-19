using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Inventory;

namespace Pulse.Models.PulseContext.Maps
{
    public class InventoryTypeDivisionSettingMap : IEntityTypeConfiguration<InventoryTypeDivisionSetting>
    {
        public void Configure(EntityTypeBuilder<InventoryTypeDivisionSetting> builder)
        {
            builder.ToTable("InventoryTypeDivisionSettings");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.DivisionID).HasMaxLength(5).IsRequired();
            builder.Property(e => e.UsedByDivision).HasDefaultValue(true);

            builder.HasIndex(e => new { e.InventoryTypeID, e.DivisionID }).IsUnique();

            builder.HasOne(e => e.InventoryType)
                   .WithMany(t => t.DivisionSettings)
                   .HasForeignKey(e => e.InventoryTypeID);

            builder.HasOne(e => e.Division)
                   .WithMany()
                   .HasForeignKey(e => e.DivisionID);
        }
    }
}