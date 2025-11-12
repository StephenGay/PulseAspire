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
    public class EquipmentCapabilitiesMap : IEntityTypeConfiguration<EquipmentCapability>
    {
        public void Configure(EntityTypeBuilder<EquipmentCapability> builder)
        {
            builder.ToTable("EquipmentCapabilities");

            // Composite primary key
            builder.HasKey(e => new { e.EquipmentItemID, e.ProductionStageID });

            builder.Property(e => e.EquipmentItemID)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.ProductionStageID)
                .HasColumnType("int")
                .IsRequired();

            // Relationship to EquipmentItem (many-to-one)
            builder.HasOne(e => e.EquipmentItem)
                .WithMany(i => i.EquipmentCapabilities) // Matches the ICollection in EquipmentItem
                .HasForeignKey(e => e.EquipmentItemID)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete: if EquipmentItem is deleted, remove capabilities

            // Relationship to ProductionStage (many-to-one)
            // Assuming ProductionStage has 'public ICollection<EquipmentCapability>? EquipmentCapabilities { get; set; }'
            builder.HasOne(e => e.ProductionStage)
                .WithMany(s => s.EquipmentCapabilities) // Adjust 'EquipmentCapabilities' if named differently in ProductionStage
                .HasForeignKey(e => e.ProductionStageID)
                .OnDelete(DeleteBehavior.NoAction); // Cascade delete: if ProductionStage is deleted, remove capabilities
        }
    }
}
