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
    public class UserFavouriteQueryMap : IEntityTypeConfiguration<UserFavouriteQry>
    {
        public void Configure(EntityTypeBuilder<UserFavouriteQry> builder)
        {
            builder.ToTable("UserFavouriteQuery");
            builder.HasOne(us => us.User)
                .WithMany(q => q.UserFavouriteQueries)
                .HasForeignKey(us => us.UserId);
            builder.HasOne(q => q.AiQuery)
                .WithMany(f  => f.UserFavouriteQueries)
                .HasForeignKey(q => q.QueryId);

        }
    }
}
