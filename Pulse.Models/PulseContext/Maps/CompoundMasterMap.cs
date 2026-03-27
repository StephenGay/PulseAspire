using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Compounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class CompoundMasterMap : IEntityTypeConfiguration<Compound>
    {
        public void Configure(EntityTypeBuilder<Compound> builder)
        {
            builder.ToTable("CompoundMaster");
            builder.HasKey(c => c.CompoundCode);

            builder.Property(c => c.CompoundCode)
                   .IsRequired()
                   .HasMaxLength(4)
                   .ValueGeneratedNever();

            // Scalar properties
            builder.Property(c => c.RevisionCode)
                   .HasMaxLength(3);

            builder.Property(c => c.CompoundDescription)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.PolymerName)
                   .HasMaxLength(50);

            builder.Property(c => c.HardnessType)
                   .HasMaxLength(50);

            builder.Property(c => c.Colour)
                   .HasMaxLength(100);

            builder.Property(c => c.CompoundType)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(c => c.State)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(c => c.RevisedBy)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.RevisionReason)
                   .IsRequired()
                   .HasMaxLength(255);

            // Decimal properties with precision
            builder.Property(c => c.CalculatedSpecificGravity)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 6)
                   .IsRequired();

            builder.Property(c => c.SpecificGravity)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 6)
                   .IsRequired();

            builder.Property(c => c.CalculatedCostPerKg)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4)
                   .IsRequired();

            builder.Property(c => c.CostingCostPerKg)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4)
                   .IsRequired();

            builder.Property(c => c.RoyaltyCharge)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(c => c.CarbonBlackCharge)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(c => c.CustomSaleFactor)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(c => c.CompoundDifficultyMultiplier)
                   .HasDefaultValue(1m)
                   .HasPrecision(5, 2)
                   .IsRequired();

            // Date & Boolean properties
            builder.Property(c => c.RevisionDate)
                   .IsRequired();

            builder.Property(c => c.OverRideSpecificGravity)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(c => c.OverRideCostPerKg)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(c => c.IsImported)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(c => c.DateCostUpdated)
                   .IsRequired(false);

            // === Relationships ===

            // Compound → CompoundRange (Many-to-One)
            builder.HasOne(c => c.CompoundRange)
                   .WithMany()                          // Add back-navigation on CompoundRange if needed later
                   .HasForeignKey(c => c.CompoundRangeId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many relationships (back-navigation from other entities will be configured on their side)
            builder.HasMany(c => c.IndustryRecommendedCovers)
                   .WithOne()                           // Configure on IndustryRecommendedCover side if needed
                   .HasForeignKey("CompoundCode")
                   .OnDelete(DeleteBehavior.Cascade);   // Usually cascade for child records

            builder.HasMany(c => c.ClientRollerSpecifications)
                   .WithOne()
                   .HasForeignKey("CompoundCode")
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.WorksOrders)
                   .WithOne()
                   .HasForeignKey("CompoundCode")
                   .OnDelete(DeleteBehavior.Restrict);  // Safer for WorksOrders

            // Indexes
            builder.HasIndex(c => c.CompoundType);
            builder.HasIndex(c => c.State);
            builder.HasIndex(c => c.IsImported);
            builder.HasIndex(c => c.CompoundRangeId);


            // Additional configuration can be added here as needed
        }
    
    }
}
