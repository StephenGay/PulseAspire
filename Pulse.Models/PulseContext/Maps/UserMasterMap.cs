using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            builder.HasOne(us => us.UserSettings)
                   .WithOne(u => u.User)
                   .HasForeignKey<User>(us => us.UserID)
                   .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
    
}

