using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Organizational;
using Pulse.Models.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class WorkCentreMasterMap : IEntityTypeConfiguration<WorkCentre>
    {
        public void Configure(EntityTypeBuilder<WorkCentre> builder)
        {
            // Table mapping
            builder.ToTable("WorkCentreMaster");

            // Primary Key
            builder.HasKey(e => e.WorkCentreId);
            builder.Property(e => e.WorkCentreId)
                .ValueGeneratedNever(); // DatabaseGeneratedOption.None

            // Properties
            builder.Property(e => e.WorkCentreName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.DivisionID)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(e => e.BranchID)
                .IsRequired();

            builder.Property(e => e.Colour)
            .HasMaxLength(9)                        // #RRGGBBAA
            .HasDefaultValue("#ffffff");

            builder.Property(e => e.TextColour)
            .HasMaxLength(9)                        // #RRGGBBAA
            .HasDefaultValue("#000000");

            builder.Property(e => e.ApplyTargets)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.TargetMinUnitsPerDay)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasDefaultValue(0.0);

            builder.Property(e => e.TargetMaxUnitsPerDay)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasDefaultValue(0.0);

            builder.Property(e => e.Description)
                .IsRequired(false)
                .HasMaxLength(250);

            builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

            // Relationships

            // 1. Division (assuming Division exists in Pulse.Models.Organizational with PK DivisionID string)
            builder.HasOne<Division>() // Explicit type if needed; otherwise inferred from navigation
                .WithMany() // Add .HasMany(d => d.WorkCentres) if collection exists in Division
                .HasForeignKey(e => e.DivisionID);
            //.OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // 2. Branch (assuming Branch exists in Pulse.Models.Organizational with PK BranchID int)
            builder.HasOne<Branch>()
                .WithMany() // Add .HasMany(b => b.WorkCentres) if collection exists in Branch
                .HasForeignKey(e => e.BranchID)
            .OnDelete(DeleteBehavior.NoAction);

            // 3. WorkCentreFunctions (one-to-many)
            builder.HasMany(e => e.WorkCentreFunctions)
                .WithOne(wcf => wcf.WorkCentre) // Add .HasOne(wcf => wcf.WorkCentre) if navigation exists in WorkCentreFunction
                .HasForeignKey("WorkCentreId"); // Assumes FK column named WorkCentreId in WorkCentreFunctions table
                //.OnDelete(DeleteBehavior.Cascade); // Optional: cascade delete functions when centre is deleted
        }
    }
}
