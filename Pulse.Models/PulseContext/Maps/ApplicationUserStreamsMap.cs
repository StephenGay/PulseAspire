using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class ApplicationUserStreamsMap : IEntityTypeConfiguration<ApplicationUserStream>
{
    public void Configure(EntityTypeBuilder<ApplicationUserStream> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserID)
            .IsRequired();

        builder.Property(x => x.StreamType)
            .IsRequired();
        
        builder.Property(x => x.StreamID)
            .IsRequired();

        builder.Property(x => x.StreamDivisionID)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(x => x.StreamName)
            .HasMaxLength(50);

        builder.HasIndex(x => x.ApplicationUserID);
        builder.HasIndex(x => x.StreamID);
        builder.HasIndex(x => x.StreamType);
        builder.HasIndex(x => x.StreamDivisionID);

        builder.ToTable("AspNetUserStreams");
    }
}
