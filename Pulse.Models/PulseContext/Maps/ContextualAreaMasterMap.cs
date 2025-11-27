using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps
{
    public class ContextualAreaMasterMap : IEntityTypeConfiguration<ContextualArea>
    {
        public void Configure(EntityTypeBuilder<ContextualArea> builder)
        {
            // Table name (optional, defaults to DbSet name)
            builder.ToTable("ContextualAreaMaster");

            // Primary Key
            builder.HasKey(ca => ca.ContextAreaId);

            // Property configurations
            builder.Property(ca => ca.ContextAreaId)
                   .ValueGeneratedNever(); // Matches DatabaseGeneratedOption.None

            builder.Property(ca => ca.AreaDescription)
                   .IsRequired()
                   .HasMaxLength(100);

            // Relationships
            builder.HasMany(ca => ca.ContextualPrompts)
                   .WithOne(cp => cp.ContextualArea) // assumes ContextualPrompt has navigation property
                   .HasForeignKey(cp => cp.ContextualAreaId);
                   //.OnDelete(DeleteBehavior.Cascade); // optional, depending on your design
        }
    }
}
