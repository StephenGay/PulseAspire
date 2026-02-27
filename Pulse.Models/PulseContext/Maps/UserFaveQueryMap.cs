using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps
{
    public class UserFaveQueryMap : IEntityTypeConfiguration<UserFavouriteQuery>
    {
        public void Configure(EntityTypeBuilder<UserFavouriteQuery> builder)
        {
            builder.ToTable("UserFaveQueries");

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

            builder.HasIndex(q => q.UserId)
                .HasDatabaseName("IX_UserFaveQueries_UserId");
        }
    }
}