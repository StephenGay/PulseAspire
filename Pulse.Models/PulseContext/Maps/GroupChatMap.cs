using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Communication;
using System;
using System.Collections.Generic;
using System.Text;
using static Pulse.Models.Api.ApiEndpoints.User;

namespace Pulse.Models.PulseContext.Maps;

public class GroupChatMap : IEntityTypeConfiguration<GroupChat>
{
    public void Configure(EntityTypeBuilder<GroupChat> builder)
    {
        builder.HasKey(x => x.GroupID);

        builder.Property(x => x.GroupName)
            .IsRequired();

        builder.Property(x => x.DivisionID)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(256);

        builder.Property(x => x.OwnerID);

        builder.Property(x => x.IsPrivateGroup)
            .HasDefaultValue(false);

        builder.ToTable("GroupChats", "app");

    }
}

