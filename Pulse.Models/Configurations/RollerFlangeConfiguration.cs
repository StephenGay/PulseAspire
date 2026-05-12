using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Rollers;

namespace Pulse.Models.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration for RollerFlange.
    /// Defines database schema, relationships, constraints, and validation rules.
    /// </summary>
    public class RollerFlangeConfiguration : IEntityTypeConfiguration<RollerFlange>
    {
        public void Configure(EntityTypeBuilder<RollerFlange> builder)
        {
            // Table configuration
            builder.ToTable("RollerFlanges", schema: "dbo");

            // Primary Key
            builder.HasKey(rf => rf.Id);

            #region Core Geometry - Column Configuration

            builder.Property(rf => rf.OuterDiameter)
                .HasColumnType("decimal(10, 2)")
                .IsRequired()
                .HasComment("Outer diameter of the flange (mm)");

            builder.Property(rf => rf.InnerDiameter)
                .HasColumnType("decimal(10, 2)")
                .IsRequired()
                .HasComment("Inner diameter of the flange bore (mm)");

            builder.Property(rf => rf.Width)
                .HasColumnType("decimal(8, 2)")
                .IsRequired()
                .HasComment("Width (thickness) of the flange (mm)");

            builder.Property(rf => rf.BoltSetCount)
                .HasColumnType("int")
                .IsRequired()
                .HasDefaultValue(4)
                .HasComment("Number of bolt-hole sets (e.g., 4-point, 6-point pattern)");

            builder.Property(rf => rf.BoltHoleDiameter)
                .HasColumnType("decimal(8, 2)")
                .IsRequired()
                .HasDefaultValue(12)
                .HasComment("Diameter of each bolt hole in mm (typical: 12=M12, 16=M16, 20=M20)");

            builder.Property(rf => rf.BoltHolePitch)
                .HasColumnType("decimal(10, 2)")
                .IsRequired()
                .HasComment("Center-to-center spacing between adjacent bolt holes (mm)");

            builder.Property(rf => rf.BoltHoleCountPerSet)
                .HasColumnType("int")
                .IsRequired()
                .HasDefaultValue(4)
                .HasComment("Number of bolt holes per set (e.g., 4-point or 6-point pattern)");

            builder.Property(rf => rf.LipThickness)
                .HasColumnType("decimal(8, 2)")
                .HasDefaultValue(0)
                .HasComment("Optional lip thickness that projects beyond the flange face (mm)");

            #endregion

            #region Material & Finish Configuration

            builder.Property(rf => rf.Material)
                .HasColumnType("nvarchar(50)")
                .IsRequired()
                .HasDefaultValue("45Steel")
                .HasComment("Material composition (e.g., '20Cr', '45Steel')");

            builder.Property(rf => rf.SurfaceRoughness)
                .HasColumnType("decimal(6, 2)")
                .HasDefaultValue(3.2)
                .HasComment("Surface roughness Ra in micrometres (µm)");

            #endregion

            #region Tolerances Configuration

            builder.Property(rf => rf.OuterDiameterTolerance)
                .HasColumnType("decimal(6, 3)")
                .HasDefaultValue(0.1)
                .HasComment("Tolerance for outer diameter (± mm)");

            builder.Property(rf => rf.InnerDiameterTolerance)
                .HasColumnType("decimal(6, 3)")
                .HasDefaultValue(0.05)
                .HasComment("Tolerance for inner diameter (± mm)");

            builder.Property(rf => rf.WidthTolerance)
                .HasColumnType("decimal(6, 3)")
                .HasDefaultValue(0.1)
                .HasComment("Tolerance for width (± mm)");

            #endregion

            #region Additional Properties Configuration

            builder.Property(rf => rf.HeatTreatmentState)
                .HasColumnType("int")
                .HasDefaultValue(HeatTreatment.NoTreatment)
                .HasComment("Heat-treatment state of the flange");

            builder.Property(rf => rf.Coating)
                .HasColumnType("int")
                .HasDefaultValue(SurfaceCoating.None)
                .HasComment("Surface coating applied to the flange");

            builder.Property(rf => rf.TorqueCapacity)
                .HasColumnType("decimal(10, 2)")
                .HasDefaultValue(0)
                .HasComment("Maximum torque (Nm) the flange can transmit without failure");

            #endregion

            #region Audit Fields Configuration

            builder.Property(rf => rf.CreatedDate)
                .HasColumnType("datetime2")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("UTC timestamp when the record was created");

            builder.Property(rf => rf.ModifiedDate)
                .HasColumnType("datetime2")
                .IsRequired(false)
                .HasComment("UTC timestamp when the record was last modified");

            builder.Property(rf => rf.CreatedBy)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false)
                .HasComment("User ID or name who created this record");

            builder.Property(rf => rf.ModifiedBy)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false)
                .HasComment("User ID or name who last modified this record");

            #endregion

            #region Indexes

            // Index for filtering by common properties
            builder.HasIndex(rf => rf.Material)
                .HasDatabaseName("IX_RollerFlange_Material");

            builder.HasIndex(rf => rf.OuterDiameter)
                .HasDatabaseName("IX_RollerFlange_OuterDiameter");

            builder.HasIndex(rf => rf.BoltSetCount)
                .HasDatabaseName("IX_RollerFlange_BoltSetCount");

            builder.HasIndex(rf => rf.BoltHoleDiameter)
                .HasDatabaseName("IX_RollerFlange_BoltHoleDiameter");

            // Composite index for assembly lookups
            builder.HasIndex(rf => new { rf.OuterDiameter, rf.BoltSetCount })
                .HasDatabaseName("IX_RollerFlange_DiameterBolts");

            #endregion

            #region Constraints & Validation

            // Check constraint: InnerDiameter must be less than OuterDiameter
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_InnerLessThanOuter",
                "[InnerDiameter] < [OuterDiameter]"));

            // Check constraint: Diameters must be positive
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_DiamtersPositive",
                "[OuterDiameter] > 0 AND [InnerDiameter] > 0"));

            // Check constraint: Width must be positive
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_WidthPositive",
                "[Width] > 0"));

            // Check constraint: BoltHoleDiameter must be positive and less than InnerDiameter
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_BoltHoleValid",
                "[BoltHoleDiameter] > 0 AND [BoltHoleDiameter] < [InnerDiameter]"));

            // Check constraint: BoltSetCount and BoltHoleCountPerSet must be positive
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_BoltCountValid",
                "[BoltSetCount] > 0 AND [BoltHoleCountPerSet] > 0"));

            // Check constraint: BoltHolePitch must be positive
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_BoltPitchPositive",
                "[BoltHolePitch] > 0"));

            // Check constraint: LipThickness cannot be negative
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_LipThicknessNonNegative",
                "[LipThickness] >= 0"));

            // Check constraint: Tolerances cannot be negative
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerFlange_TolerancesNonNegative",
                "[OuterDiameterTolerance] >= 0 AND [InnerDiameterTolerance] >= 0 AND [WidthTolerance] >= 0"));

            #endregion

            #region Relationship Configuration

            // If RollerFlange belongs to a RollerSpecification or Assembly
            // Uncomment and adjust as needed:

            // builder.HasOne<ClientRollerSpecification>()
            //     .WithMany()
            //     .HasForeignKey("RollerSpecificationId")
            //     .OnDelete(DeleteBehavior.Cascade)
            //     .HasConstraintName("FK_RollerFlange_RollerSpecification");

            #endregion

            #region Query Filters (Soft Delete Support)

            // Optional: Add query filter if soft delete is implemented
            // builder.HasQueryFilter(rf => !rf.IsDeleted);

            #endregion
        }
    }
}
