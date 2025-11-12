using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class EquipmentCategoryMasterMap : IEntityTypeConfiguration<EquipmentCategory>
    {
        public void Configure(EntityTypeBuilder<EquipmentCategory> builder)
        {
            builder.ToTable("EquipmentCategoryMaster");

            builder.HasKey(e => e.EquipmentCategoryID);

            builder.Property(e => e.EquipmentCategoryID)
                .HasColumnType("nvarchar(4)")
                .HasMaxLength(4)
                .IsRequired();

            builder.Property(e => e.EquipmentCategoryName)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            // If EquipmentItem has a foreign key to EquipmentCategoryID and a navigation property,
            // you can define the one-to-many relationship here:
            // builder.HasMany(e => e.EquipmentItems)
            //     .WithOne(i => i.EquipmentCategory) // Assuming EquipmentItem has 'public EquipmentCategory? EquipmentCategory { get; set; }'
            //     .HasForeignKey(i => i.EquipmentCategoryID) // Assuming EquipmentItem has 'public string? EquipmentCategoryID { get; set; }'
            //     .OnDelete(DeleteBehavior.Restrict); // Adjust delete behavior as needed (e.g., Cascade, SetNull)
        }
    }
}
