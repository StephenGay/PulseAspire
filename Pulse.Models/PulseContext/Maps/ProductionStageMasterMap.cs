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

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Relationships (assuming one-to-many; adjust if Division/WorkType have collections)
            builder.HasOne(e => e.Division)
                .WithMany() // Add .HasMany(d => d.ProductionStages) if collection exists in Division
                .HasForeignKey(e => e.DivisionID);
                //.OnDelete(DeleteBehavior.Restrict); // Or Cascade/NoAction as per business rules

            builder.HasOne(e => e.WorkType)
                .WithMany() // Add .HasMany(w => w.ProductionStages) if collection exists in WorkType
                .HasForeignKey(e => e.WorkTypeID);
                //.OnDelete(DeleteBehavior.Restrict); // Or Cascade/NoAction as per business rules
        }
    }
}
