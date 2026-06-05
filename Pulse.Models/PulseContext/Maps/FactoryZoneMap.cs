using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production;
using Pulse.Models.Production.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class FactoryZoneMap : IEntityTypeConfiguration<FactoryZone>
{
    public void Configure(EntityTypeBuilder<FactoryZone> builder)
    {
        // Table name (optional - EF will default to "FactoryZones" if omitted)
        builder.ToTable("FactoryZones");

        // Primary Key
        builder.HasKey(z => z.Id);

        // Id - Guid, value generated on add
        builder.Property(z => z.Id)
            .ValueGeneratedOnAdd();

        // DivisionId - required, max length 5
        builder.Property(z => z.DivisionId)
            .IsRequired()
            .HasMaxLength(5);

        // Name - required (you can decide if it should be required or not)
        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(200);     // ← reasonable upper limit, adjust as needed
        
        // Description - optional, max length 50
        builder.Property(z => z.Description)
            .IsRequired(false)
            .HasMaxLength(50);

        // Level - default 0
        builder.Property(z => z.Level)
            .HasDefaultValue(0);

        // ParentZoneId - optional (self-referencing foreign key)
        builder.Property(z => z.ParentZoneId)
            .IsRequired(false);

        // Self-referencing relationship (hierarchical zones)
        //builder.HasOne<FactoryZone>()
        //    .WithMany()
        //    .HasForeignKey(z => z.ParentZoneId)
        //    .OnDelete(DeleteBehavior.Restrict);     // or Cascade / SetNull depending on business rules

        // WorkCentreID - optional foreign key
        builder.Property(z => z.WorkCentreID)
            .IsRequired(false);

        builder.HasOne(z => z.WorkCentre)
            .WithMany()                             // assuming WorkCentre doesn't have collection of zones
            .HasForeignKey(z => z.WorkCentreID)
            .OnDelete(DeleteBehavior.SetNull);      // or Restrict / NoAction

        builder.Property(z => z.EquipmentID)
            .IsRequired(false);
        // EquipmentCapabilityID - optional foreign key
        builder.Property(z => z.EquipmentCapabilityID)
            .IsRequired(false);

        //builder.HasOne(z => z.equipmentCapability)
        //    .WithMany()             // assuming no navigation back
        //    .HasForeignKey(z => z.EquipmentCapabilityID)
        //    .OnDelete(DeleteBehavior.SetNull);

        // Spatial / layout properties (no special config needed for doubles)
        builder.Property(z => z.X)
            .HasPrecision(10, 4)
            .HasDefaultValue(5.0);                   // reasonable precision for coordinates

        builder.Property(z => z.Y)
            .HasPrecision(10, 4)
            .HasDefaultValue(5.0);

        builder.Property(z => z.Width)
            .HasPrecision(10, 4)
            .HasDefaultValue(150.0);

        builder.Property(z => z.Height)
            .HasPrecision(10, 4)
            .HasDefaultValue(100.0);

        builder.Property(z => z.IsPlotted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(z => z.TargetMinUnitsPerDay)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0.0);

        builder.Property(z => z.TargetMaxUnitsPerDay)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0.0);

        // Color - stored as string (hex), max length for safety
        builder.Property(z => z.Color)
            .HasMaxLength(9)                        // #RRGGBBAA
            .HasDefaultValue("#3b82f6");

        builder.Property(z => z.TextColor)
            .HasMaxLength(9)                        // #RRGGBBAA
            .HasDefaultValue("#ffffff");

        // Notes - optional long text
        builder.Property(z => z.Notes)
            .IsRequired(false)
            .HasMaxLength(2000);
    }
}
