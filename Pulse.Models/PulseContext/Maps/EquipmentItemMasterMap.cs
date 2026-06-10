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
    internal class EquipmentItemMasterMap : IEntityTypeConfiguration<EquipmentItem>
    {
        public void Configure(EntityTypeBuilder<EquipmentItem> builder)
        {
            builder.ToTable("EquipmentItems");

            builder.HasKey(e => e.EquipmentItemID);

            builder.Property(e => e.EquipmentItemID)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.FixedAssetNo)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20);

            builder.Property(e => e.DivisionID)
                .HasColumnType("nvarchar(5)")
                .HasMaxLength(5)
                .IsRequired();

            builder.Property(e => e.EquipmentCategoryID)
                .HasColumnType("nvarchar(4)")
                .HasMaxLength(4);

            builder.Property(e => e.EquipmentItemDescription)
                .HasColumnType("nvarchar(150)")
                .HasMaxLength(150);

            builder.Property(e => e.EquipmentItemDescription2)
                .HasColumnType("nvarchar(150)")
                .HasMaxLength(150);

            builder.Property(e => e.ManufacturerName)
                .HasColumnType("nvarchar(150)")
                .HasMaxLength(150);

            builder.Property(e => e.EquipmentDifficultyMultiplier)
                .HasColumnType("decimal(18,4)") // Assuming reasonable precision; adjust if needed
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(e => e.ZoneID);
            
            builder.Property(e => e.SerialNo)
                .HasColumnType("nvarchar(150)")
                .HasMaxLength(150);

            builder.Property(e => e.Model)
                .HasColumnType("nvarchar(150)")
                .HasMaxLength(150);

            builder.Property(e => e.Barcode)
                .HasColumnType("nvarchar(MAX)");
                
            builder.Property(e => e.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(e => e.IsOperational)
                .HasColumnType("bit")
                .HasDefaultValue(true);
                
            // Relationship to EquipmentCategory (many-to-one)
            builder.HasOne(e => e.EquipmentCategory)
                .WithMany(c => c.EquipmentItems)
                .HasForeignKey(e => e.EquipmentCategoryID)
                .OnDelete(DeleteBehavior.SetNull); // Or Restrict/Cascade as per business rules

            // Relationship to Division (many-to-one; assuming Division has ICollection<EquipmentItem>)
            builder.HasOne(e => e.Division)
                .WithMany() // Adjust if Division doesn't have this navigation
                .HasForeignKey(e => e.DivisionID);
            //.OnDelete(DeleteBehavior.Restrict); // Prevent deleting Division if items exist

            builder.HasOne(e => e.FactoryZone)
                .WithMany() // Adjust if FactoryZone doesn't have this navigation
                .HasForeignKey(e => e.ZoneID);

            builder.HasMany(e => e.ProductionPlanItems)
                 .WithOne(p => p.EquipmentItem)
                 .HasForeignKey(p => p.EquipmentItemID);

            //     .WithOne(c => c.EquipmentItem)
            //     .HasForeignKey(c => c.EquipmentItemID)
            // Relationship to EquipmentCapabilities (one-to-many; assuming EquipmentCapability has FK to EquipmentItemID)
            // builder.HasMany(e => e.EquipmentCapabilities)
            //     .WithOne(c => c.EquipmentItem)
            //     .HasForeignKey(c => c.EquipmentItemID)
            //     .OnDelete(DeleteBehavior.Cascade); // Uncomment and adjust once EquipmentCapability is defined
        }
    }
}