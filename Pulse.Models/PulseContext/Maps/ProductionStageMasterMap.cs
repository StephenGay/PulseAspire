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
    public class ProductionStageMasterMap : IEntityTypeConfiguration<ProductionStage>
    {
        public void Configure(EntityTypeBuilder<ProductionStage> builder)
        {
            // Table mapping (infer table name from entity; customize if needed)
            builder.ToTable("ProductionStageMaster");

            // Primary Key
            builder.HasKey(e => e.ProductionStageId);
            builder.Property(e => e.ProductionStageId)
                .ValueGeneratedNever(); // No auto-generation as per DatabaseGeneratedOption.None

            // Properties with attributes translated to Fluent API
            builder.Property(e => e.ProductionStageName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.DivisionID)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(e => e.WorkTypeID)
                .IsRequired();

            builder.Property(e => e.StepNo)
                .IsRequired();

            builder.Property(e => e.HasMaterial)
                .HasDefaultValue(false);

            builder.Property(e => e.IsDefault)
                .HasDefaultValue(false);

            builder.Property(e => e.IsOptional)
                .HasDefaultValue(false);

            builder.Property(e => e.ProcessPercentage)
            .HasPrecision(10, 4)
            .HasDefaultValue(0.0);

            builder.Property(e => e.ResultsPage)
                .HasMaxLength(200)
                .IsRequired(false); // Optional as per nullable type

            builder.Property(e => e.RequiresSignOff)
                .HasDefaultValue(false);

            builder.Property(e => e.RequiresPlanning)
                .HasDefaultValue(false);

            builder.Property(e => e.DifficultyMeasurement)
               .HasMaxLength(50);

            builder.Property(e => e.BaseValue)
                   .HasDefaultValue(0m)
                   .HasPrecision(18, 4)   // Good precision for decimal values
                   .IsRequired();

            builder.Property(e => e.BaseMinutesAtStage)
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Relationships (assuming one-to-many; adjust if Division/WorkType have collections)
            builder.HasOne(e => e.Division)
                .WithMany() // Add .HasMany(d => d.ProductionStages) if collection exists in Division
                .HasForeignKey(e => e.DivisionID)
                .HasPrincipalKey(d => d.DivisionID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); 
                

            builder.HasOne(e => e.WorkType)
                .WithMany() // Add .HasMany(w => w.ProductionStages) if collection exists in WorkType
                .HasForeignKey(e => e.WorkTypeID)
                .HasPrincipalKey(w => w.WorkTypeID)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.DivisionWorkType)
               .WithMany()                                      // You can add a back navigation later if needed
               .HasForeignKey(e => new { e.WorkTypeID, e.DivisionID })   // Composite FK
               .HasPrincipalKey(dwt => new { dwt.WorkTypeID, dwt.DivisionId })
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.StepNo);
            builder.HasIndex(e => new { e.DivisionID, e.WorkTypeID });        // Useful for the composite relationship
            builder.HasIndex(e => new { e.DivisionID, e.WorkTypeID, e.StepNo });
        }
    }
}
