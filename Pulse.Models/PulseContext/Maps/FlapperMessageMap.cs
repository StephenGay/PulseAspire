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
        // Table name
        builder.ToTable("FlapperMessages", "pai");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(x => x.Sender)
            .HasMaxLength(50);

        builder.Property(x => x.Content)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ContentType)
            .HasMaxLength(50);

        builder.Property(x => x.ClarificationType)
            .HasMaxLength(50);

        builder.Property(x => x.UserAnswer)
            .HasColumnType("nvarchar(max)");

        // Configure the relationship with proper FK name
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}