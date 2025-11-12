using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class WorkCentreFunctionsMappingMap : IEntityTypeConfiguration<WorkCentreFunctions>
    {
        public void Configure(EntityTypeBuilder<WorkCentreFunctions> builder)
        {
            // Table name
            builder.ToTable("WorkCentreFunctionsMapping");

            // Composite Primary Key
            builder.HasKey(wcf => new { wcf.WorkCentreID, wcf.ProductionStageID });

            // Column order (optional, but matches your [Column(Order = n)])
            builder.Property(wcf => wcf.WorkCentreID)
                .ValueGeneratedNever()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw); // Enforce no auto-gen

            builder.Property(wcf => wcf.ProductionStageID)
                .ValueGeneratedNever()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            // Relationships

            // 1. WorkCentre (Many-to-One)
            builder.HasOne(wcf => wcf.WorkCentre)
                .WithMany(wc => wc.WorkCentreFunctions) // Assumes WorkCentre has ICollection<WorkCentreFunctions> WorkCentreFunctions
                .HasForeignKey(wcf => wcf.WorkCentreID)
                .OnDelete(DeleteBehavior.Cascade); // Delete functions if WorkCentre is deleted

            // 2. ProductionStage (Many-to-One)
            builder.HasOne(wcf => wcf.ProductionStage)
                .WithMany() // Add .HasMany(ps => ps.WorkCentreFunctions) if navigation exists in ProductionStage
                .HasForeignKey(wcf => wcf.ProductionStageID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a stage if referenced
        }
    }
}
