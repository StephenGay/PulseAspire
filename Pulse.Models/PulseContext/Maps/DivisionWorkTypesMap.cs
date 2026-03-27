using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production.WorkTypes;

namespace Pulse.Models.PulseContext.Maps;

public class DivisionWorkTypeMap : IEntityTypeConfiguration<DivisionWorkType>
{
    public void Configure(EntityTypeBuilder<DivisionWorkType> builder)
    {
        builder.ToTable("DivisionWorkTypes");

        // Composite Primary Key
        builder.HasKey(dwt => new { dwt.WorkTypeID, dwt.DivisionId });

        // Properties
        builder.Property(dwt => dwt.WorkTypeID)
               .IsRequired()
               .ValueGeneratedNever();

        builder.Property(dwt => dwt.DivisionId)
               .IsRequired()
               .HasMaxLength(5)
               .ValueGeneratedNever();

        builder.Property(dwt => dwt.TargetWorkingDays)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(dwt => dwt.SortOrder)
               .IsRequired();

        builder.Property(dwt => dwt.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        // === Relationships ===

        // DivisionWorkTypes → WorkType (Many-to-One)
        builder.HasOne(dwt => dwt.WorkType)
               .WithMany(w => w.DivisionWorkTypes)
               .HasForeignKey(dwt => dwt.WorkTypeID)
               .HasPrincipalKey(w => w.WorkTypeID)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        // DivisionWorkTypes → Division (Many-to-One)
        builder.HasOne(dwt => dwt.Division)
               .WithMany(d => d.DivisionWorkTypes)
               .HasForeignKey(dwt => dwt.DivisionId)
               .HasPrincipalKey(d => d.DivisionID)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        // === NEW: DivisionWorkTypes → ProductionStage (One-to-Many) ===
        // Back-navigation from DivisionWorkTypes to multiple ProductionStages
        builder.HasMany(dwt => dwt.ProductionStages)
               .WithOne(ps => ps.DivisionWorkType)
               .HasForeignKey(ps => new { ps.WorkTypeID, ps.DivisionID })   // Composite FK on ProductionStage
               .HasPrincipalKey(dwt => new { dwt.WorkTypeID, dwt.DivisionId })
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        // === Indexes for performance ===
        builder.HasIndex(dwt => dwt.DivisionId);
        builder.HasIndex(dwt => dwt.IsActive);
        builder.HasIndex(dwt => dwt.SortOrder);
        builder.HasIndex(dwt => new { dwt.DivisionId, dwt.IsActive });   // Common query pattern
    }
}