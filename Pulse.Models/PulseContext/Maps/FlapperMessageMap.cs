using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.AI.Flapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class FlapperMessageMap : IEntityTypeConfiguration<FlapperMessage>
{
    public void Configure(EntityTypeBuilder<FlapperMessage> builder)
    {
        // Table name (optional - EF will default to "FlapperMessages" if omitted)
        builder.ToTable("FlapperMessages", "pai");
    }
}