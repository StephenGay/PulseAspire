using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Communication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class SystemDataStreamsMap : IEntityTypeConfiguration<SystemDataStream>
{
    public void Configure(EntityTypeBuilder<SystemDataStream> builder)
    {
        builder.HasKey(x => x.StreamID);

        builder.Property(x => x.StreamName)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(256);

        builder.ToTable("SystemDataStreams", "app");

        builder.HasData(
            new SystemDataStream
            {
                StreamID = 1,
                StreamName = "Production",
                Description = "Data stream for production events.",
            }
        );
    }
}
