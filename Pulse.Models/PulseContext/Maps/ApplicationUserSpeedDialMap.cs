using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;

namespace Pulse.Models.PulseContext.Maps;

/// <summary>
/// Entity configuration for <see cref="ApplicationUserSpeedDial"/>.
/// </summary>
public class ApplicationUserSpeedDialMap : IEntityTypeConfiguration<ApplicationUserSpeedDial>
{
    public void Configure(EntityTypeBuilder<ApplicationUserSpeedDial> builder)
    {
        // Table configuration
        builder.ToTable("AspNetUserSpeedDials", schema: "dbo");

        // Primary key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(x => x.ApplicationUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ItemCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ItemName)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes for performance
        builder.HasIndex(x => x.ApplicationUserId)
            .HasDatabaseName("IX_AspNetUserSpeedDials_ApplicationUserId");

        builder.HasIndex(x => new { x.ApplicationUserId, x.Category })
            .HasDatabaseName("IX_AspNetUserSpeedDials_ApplicationUserId_Category");
    }
}
