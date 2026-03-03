using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps
{
    internal class ContextualPromptMasterMap : IEntityTypeConfiguration<ContextualPrompt>
    {
        public void Configure(EntityTypeBuilder<ContextualPrompt> builder)
        {
            // Table name (optional, defaults to DbSet name)
            builder.ToTable("ContextualPromptMaster");

            // Primary Key
            builder.HasKey(cp => cp.ContextualPromptId);

            // Properties
            builder.Property(cp => cp.ContextualPromptTitle)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(cp => cp.ContextualPromptDescription)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(cp => cp.Prompt)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(cp => cp.ApplicationUserId)
                   .HasMaxLength(450);

            builder.Property(cp => cp.Likes)
                   .IsRequired()
                   .HasDefaultValue(0); // Matches [DefaultValue(0)]

            // Relationships
            builder.HasOne(cp => cp.ContextualArea)
                   .WithMany(ca => ca.ContextualPrompts)
                   .HasForeignKey(cp => cp.ContextualAreaId);
                   //.OnDelete(DeleteBehavior.Cascade); // optional, adjust if you want Restrict/SetNull
        }
    }
}
