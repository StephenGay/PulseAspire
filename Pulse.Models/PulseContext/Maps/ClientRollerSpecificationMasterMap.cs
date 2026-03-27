using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientRollerSpecificationMasterMap : IEntityTypeConfiguration<ClientRollerSpecification>
    {
        public void Configure(EntityTypeBuilder<ClientRollerSpecification> builder)
        {
            builder.ToTable("ClientRollerSpecificationMaster");

            // Primary Key
            builder.HasKey(crs => crs.ClientRollerSpecificationID);

            builder.Property(crs => crs.ClientRollerSpecificationID)
                   .IsRequired()
                   .ValueGeneratedNever();

            // Required fields
            builder.Property(crs => crs.FullClientID)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(crs => crs.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // General Details
            builder.Property(crs => crs.Description)
                   .HasMaxLength(100);

            builder.Property(crs => crs.RollerFunction)
                   .HasMaxLength(100);

            builder.Property(crs => crs.ProcessName)
                   .HasMaxLength(100);

            builder.Property(crs => crs.MachineType)
                   .HasMaxLength(100);

            // Reference Details
            builder.Property(crs => crs.ArticleNumber)
                   .HasMaxLength(50);

            builder.Property(crs => crs.DrawingNumber)
                   .HasMaxLength(50);

            // Shell Details
            builder.Property(crs => crs.ShellTypeName)
                   .HasMaxLength(100);

            // Cover Details
            builder.Property(crs => crs.CompoundCode)
                   .HasMaxLength(4);

            builder.Property(crs => crs.HardnessTypeRequired)
                   .HasMaxLength(50);

            // Numeric fields with precision
            builder.Property(crs => crs.ShellDiameter)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.ShellLength)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.ShellWeight)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.ShellMinimumDiameter)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.CoverLength)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.CoverDiameter)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.CoverLeftOffset)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.CoverMinimumDiameter)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4);

            builder.Property(crs => crs.AdditionalRollDifficultyMultiplier)
                   .HasDefaultValue(1m)
                   .HasPrecision(5, 2)
                   .IsRequired();

            // === Relationships ===

            // ClientRollerSpecification → Customer (Many-to-One)
            builder.HasOne(crs => crs.Customer)
                   .WithMany()                          // Add back nav on Customer if needed
                   .HasForeignKey(crs => crs.FullClientID)
                   .HasPrincipalKey(c => c.FullClientID) // Assuming Customer has FullClientID as key
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // ClientRollerSpecification → Compound (Many-to-One)
            builder.HasOne(crs => crs.Compound)
                   .WithMany(c => c.ClientRollerSpecifications)
                   .HasForeignKey(crs => crs.CompoundCode)
                   .HasPrincipalKey(c => c.CompoundCode)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            // ClientRollerSpecification → ClientRoller (One-to-Many)
            builder.HasMany(crs => crs.ClientRollers)
                   .WithOne()
                   .HasForeignKey("ClientRollerSpecificationID")
                   .OnDelete(DeleteBehavior.Cascade);

            // ClientRollerSpecification → WorksOrder (One-to-Many)
            builder.HasMany(crs => crs.WorksOrders)
                   .WithOne()
                   .HasForeignKey("ClientRollerSpecificationID")
                   .OnDelete(DeleteBehavior.Restrict);

            // === Indexes for performance ===
            builder.HasIndex(crs => crs.FullClientID);
            builder.HasIndex(crs => crs.CompoundCode);
            builder.HasIndex(crs => crs.IsActive);
            builder.HasIndex(crs => new { crs.FullClientID, crs.IsActive });
        }
    }
}
