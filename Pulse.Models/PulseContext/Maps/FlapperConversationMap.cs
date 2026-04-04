using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.AI.Flapper;
using Pulse.Models.Production.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class FlapperConversationMap : IEntityTypeConfiguration<FlapperConversation>
{
    public void Configure(EntityTypeBuilder<FlapperConversation> builder)
    {
        // Table name (optional - EF will default to "FlapperConversations" if omitted)
        builder.ToTable("FlapperConversations","pai")
                .HasMany(c => c.Messages)
                .WithOne()
                .HasForeignKey(m => m.ConversationId);
    }
}
