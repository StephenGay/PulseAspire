using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;

namespace Pulse.Models.PulseContext.Maps
{
    public class UserFavouriteQueryMap : IEntityTypeConfiguration<UserFavouriteQry>
    {
        public void Configure(EntityTypeBuilder<UserFavouriteQry> builder)
        {
            // Table configuration
            builder.ToTable("UserFavouriteQuery");

            // Primary key
            builder.HasKey(q => q.Id);

            // Properties
            builder.Property(q => q.Id)
                .ValueGeneratedOnAdd();

            builder.Property(q => q.UserId)
                .IsRequired()
                .HasMaxLength(450)
                .HasColumnName("UserId"); // ApplicationUser.Id is nvarchar(450)

            builder.Property(q => q.QueryId)
                .IsRequired();

            // Foreign key relationships
            builder.HasOne(q => q.AiQuery)
                .WithMany(aq => aq.UserFavouriteQueries)
                .HasForeignKey(q => q.QueryId)
                .OnDelete(DeleteBehavior.Cascade);

            // User relationship with explicit foreign key column
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(q => q.UserId)
                .HasConstraintName("FK_UserFavouriteQuery_AspNetUsers_UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(q => q.UserId)
                .HasDatabaseName("IX_UserFavouriteQuery_UserId");

            builder.HasIndex(q => q.QueryId)
                .HasDatabaseName("IX_UserFavouriteQuery_QueryId");

            builder.HasIndex(q => new { q.UserId, q.QueryId })
                .IsUnique()
                .HasDatabaseName("IX_UserFavouriteQuery_UserId_QueryId");
        }
    }
}
