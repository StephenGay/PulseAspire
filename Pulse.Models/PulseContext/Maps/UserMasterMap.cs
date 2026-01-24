using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;

namespace Pulse.Models.PulseContext.Maps
{
    public class UserMasterMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("UserMaster");
            builder.HasKey(u => u.UserID);
            builder.Property(u => u.Email).HasMaxLength(200);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            builder.Property(u => u.IsActive).IsRequired();
            builder.HasMany(q => q.UserFavouriteQueries)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserId);
            builder.HasOne(u => u.UserSettings)
                   .WithOne(us => us.User)
                   .HasForeignKey<UserSettings>(us => us.UserId)  // FK on dependent entity
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}

