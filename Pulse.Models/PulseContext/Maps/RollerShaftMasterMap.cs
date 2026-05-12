using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Rollers;
using Pulse.Models.Customers;

namespace Pulse.Models.PulseContext.Maps
{
    /// <summary>
    /// Entity Framework Core configuration for RollerShaft.
    /// Defines database schema, relationships, constraints, and validation rules.
    /// </summary>
    public class RollerShaftMasterMap : IEntityTypeConfiguration<RollerShaft>
    {
        public void Configure(EntityTypeBuilder<RollerShaft> builder)
        {
            // Table configuration
            builder.ToTable("RollerShafts", schema: "dbo");

            // Primary Key
            builder.HasKey(rs => rs.Id);

            builder.Property(rs => rs.ClientRollerSpecificationID)
                .HasColumnType("int")
                .IsRequired();

            #region Core Geometry - Column Configuration

            builder.Property(rs => rs.Length)
                .HasColumnType("decimal(10, 2)")
                .IsRequired()
                .HasComment("Total length of the shaft including end faces (mm)");

            builder.Property(rs => rs.OuterDiameter)
                .HasColumnType("decimal(10, 2)")
                .IsRequired()
                .HasComment("Outer diameter of the shaft (mm)");

            builder.Property(rs => rs.BearingBoreDiameter)
                .HasColumnType("decimal(10, 2)")
                .IsRequired()
                .HasComment("Inner bore diameter that mates with the bearing (mm)");

            builder.Property(rs => rs.KeywayWidth)
                .HasColumnType("decimal(8, 2)")
                .HasDefaultValue(0)
                .HasComment("Width of the keyway if present (mm)");

            builder.Property(rs => rs.KeywayDepth)
                .HasColumnType("decimal(8, 2)")
                .HasDefaultValue(0)
                .HasComment("Depth of the keyway (mm)");

            builder.Property(rs => rs.KeywayLength)
                .HasColumnType("decimal(8, 2)")
                .HasDefaultValue(0)
                .HasComment("Length of the keyway along the shaft (mm)");

            builder.Property(rs => rs.EndFaceThickness)
                .HasColumnType("decimal(8, 2)")
                .HasDefaultValue(0)
                .HasComment("Thickness of the shaft end faces for mounting/bearing clearance (mm)");

            #endregion

            #region Material & Finish Configuration

            builder.Property(rs => rs.Material)
                .HasColumnType("nvarchar(50)")
                .IsRequired()
                .HasDefaultValue("45Steel")
                .HasComment("Material composition (e.g., '20Cr', '45Steel', 'AlloyX')");

            builder.Property(rs => rs.SurfaceRoughness)
                .HasColumnType("decimal(6, 2)")
                .HasDefaultValue(3.2)
                .HasComment("Surface roughness Ra in micrometres (µm)");

            #endregion

            #region Tolerances Configuration

            builder.Property(rs => rs.OuterDiameterTolerance)
                .HasColumnType("decimal(6, 3)")
                .HasDefaultValue(0.1)
                .HasComment("Tolerance for outer diameter (± mm)");

            builder.Property(rs => rs.BearingBoreTolerance)
                .HasColumnType("decimal(6, 3)")
                .HasDefaultValue(0.05)
                .HasComment("Tolerance for bearing bore diameter (± mm)");

            builder.Property(rs => rs.KeywayTolerance)
                .HasColumnType("decimal(6, 3)")
                .HasDefaultValue(0.1)
                .HasComment("Tolerance for keyway dimensions (± mm)");

            #endregion

            #region Cover Configuration

            builder.Property(rs => rs.CoverCompound)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false)
                .HasComment("Cover compound used on the shaft if applicable");

            builder.Property(rs => rs.CoverCompoundThickness)
                .HasColumnType("decimal(8, 2)")
                .IsRequired(false)
                .HasComment("Thickness of the cover compound (mm)");

            builder.Property(rs => rs.CoverOverbuild)
                .HasColumnType("decimal(8, 2)")
                .IsRequired(false)
                .HasComment("Overbuild dimension of the cover (mm)");

            #endregion

            #region Positioning Configuration

            builder.Property(rs => rs.AxialPosition)
                .HasColumnType("decimal(10, 2)")
                .HasDefaultValue(0)
                .HasComment("Position along the roller axis where 0 = left end, positive = rightward (mm)");

            builder.Property(rs => rs.ShaftPosition)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasDefaultValue("Center")
                .HasComment("Which side the shaft is on: 'Left', 'Right', or 'Center' for concentric shafts");

            builder.Property(rs => rs.RadialOffset)
                .HasColumnType("decimal(8, 2)")
                .HasDefaultValue(0)
                .HasComment("Offset from the roller centerline where positive = outward radially (mm)");

            #endregion

            #region Additional Properties Configuration

            builder.Property(rs => rs.HeatTreatmentState)
                .HasColumnType("int")
                .HasDefaultValue(HeatTreatment.NoTreatment)
                .HasComment("Heat-treatment state of the shaft");

            builder.Property(rs => rs.Coating)
                .HasColumnType("int")
                .HasDefaultValue(SurfaceCoating.None)
                .HasComment("Surface coating applied to the shaft");

            builder.Property(rs => rs.TorqueCapacity)
                .HasColumnType("decimal(10, 2)")
                .HasDefaultValue(0)
                .HasComment("Maximum torque (Nm) the shaft can transmit without failure");

            builder.Property(rs => rs.IsActive)
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .HasComment("Indicates whether the shaft is active");

            #endregion

            #region Audit Fields Configuration (Optional)

            builder.Property(rs => rs.CreatedDate)
                .HasColumnType("datetime2")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("UTC timestamp when the record was created");

            builder.Property(rs => rs.ModifiedDate)
                .HasColumnType("datetime2")
                .IsRequired(false)
                .HasComment("UTC timestamp when the record was last modified");

            builder.Property(rs => rs.CreatedBy)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false)
                .HasComment("User ID or name who created this record");

            builder.Property(rs => rs.ModifiedBy)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false)
                .HasComment("User ID or name who last modified this record");

            #endregion

            #region Indexes

            // Index for common queries
            builder.HasIndex(rs => rs.ShaftPosition)
                .HasDatabaseName("IX_RollerShaft_ShaftPosition");
            //.HasComment("Index for filtering shafts by position (Left, Center, Right)");

            builder.HasIndex(rs => rs.AxialPosition)
                .HasDatabaseName("IX_RollerShaft_AxialPosition");
            //.HasComment("Index for sorting shafts by axial position");

            builder.HasIndex(rs => rs.OuterDiameter)
                .HasDatabaseName("IX_RollerShaft_OuterDiameter");
            //.HasComment("Index for filtering by diameter");

            builder.HasIndex(rs => rs.Material)
                .HasDatabaseName("IX_RollerShaft_Material");
            //.HasComment("Index for filtering by material type");

            // Composite index for roller assembly lookups
            builder.HasIndex(rs => new { rs.ShaftPosition, rs.AxialPosition })
                .HasDatabaseName("IX_RollerShaft_PositionAxial");
                //.HasComment("Composite index for assembly positioning queries");

            #endregion

            #region Constraints & Validation

            // Validation constraints via Fluent API
            builder.Property(rs => rs.Length)
                .HasPrecision(10, 2);

            builder.Property(rs => rs.OuterDiameter)
                .HasPrecision(10, 2);

            // Check constraint: BearingBoreDiameter must be less than OuterDiameter
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerShaft_BoreLessThanOuter",
                "[BearingBoreDiameter] < [OuterDiameter]"));

            // Check constraint: Keyway dimensions cannot be negative
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerShaft_KeywayNonNegative",
                "[KeywayWidth] >= 0 AND [KeywayDepth] >= 0 AND [KeywayLength] >= 0"));

            // Check constraint: Length must be positive
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerShaft_LengthPositive",
                "[Length] > 0"));

            // Check constraint: Diameter must be positive
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerShaft_DiameterPositive",
                "[OuterDiameter] > 0"));

            // Check constraint: Valid shaft positions
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_RollerShaft_ValidPosition",
                "[ShaftPosition] IN ('Left', 'Center', 'Right')"));

            #endregion

            #region Relationship Configuration

            // If RollerShaft has a relationship to RollerSpecification
            // Uncomment and adjust as needed:

            builder.HasOne<ClientRollerSpecification>()
                .WithMany()
                .HasForeignKey("ClientRollerSpecificationId")
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RollerShaft_RollerSpecification");

            #endregion

            #region Query Filters (Soft Delete Support)

            // Optional: Add query filter if soft delete is implemented
            // builder.HasQueryFilter(rs => !rs.IsDeleted);

            #endregion
        }
    }
}
