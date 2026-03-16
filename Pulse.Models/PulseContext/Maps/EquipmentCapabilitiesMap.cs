using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production;

namespace Pulse.Models.PulseContext.Maps
{
    public class EquipmentCapabilitiesMap : IEntityTypeConfiguration<EquipmentCapability>
    {
        public void Configure(EntityTypeBuilder<EquipmentCapability> builder)
        {
            builder.ToTable("EquipmentCapabilities");

            // Composite primary key
            builder.HasKey(e => new { e.EquipmentItemID, e.ProductionStageID });

            // EquipmentItemID (part of PK)
            builder.Property(e => e.EquipmentItemID)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired();

            // ProductionStageID (part of PK)
            builder.Property(e => e.ProductionStageID)
                .HasColumnType("int")
                .IsRequired();

            // FactoryZoneId (FK - optional)
            builder.Property(e => e.FactoryZoneId)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // Relationship to EquipmentItem (many-to-one)
            builder.HasOne(e => e.EquipmentItem)
                .WithMany(i => i.EquipmentCapabilities)
                .HasForeignKey(e => e.EquipmentItemID)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship to ProductionStage (many-to-one)
            builder.HasOne(e => e.ProductionStage)
                .WithMany(s => s.EquipmentCapabilities)
                .HasForeignKey(e => e.ProductionStageID)
                .OnDelete(DeleteBehavior.NoAction);

            //// Relationship to FactoryZone (many-to-one) - NEW
            //builder.HasOne(e => e.FactoryZone)
            //    .WithMany() // FactoryZone may not have a collection back-reference
            //    .HasForeignKey(e => e.FactoryZoneId)
            //    .OnDelete(DeleteBehavior.SetNull); // If zone is deleted, set FactoryZoneId to null
        }
    }
}
