using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Compounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class CompoundMasterMap : IEntityTypeConfiguration<Compound>
    {
        public void Configure(EntityTypeBuilder<Compound> builder)
        {
            builder.ToTable("CompoundMaster")
                    .Property(e => e.RevisionDate)
                    .HasDefaultValueSql("getdate()")
                    .ValueGeneratedOnAddOrUpdate();
            builder.HasKey(c => c.CompoundCode);
            builder.HasOne(c => c.CompoundRange)
                   .WithMany(cr => cr.Compounds)
                   .HasForeignKey(c => c.CompoundRangeId);
            builder.HasMany(c => c.IndustryRecommendedCovers)
                   .WithOne(irc => irc.Compound)
                   .HasForeignKey(irc => irc.CompoundCode);
            builder.HasMany(crs => crs.ClientRollerSpecifications)
                    .WithOne(cs  => cs.Compound)
                    .HasForeignKey(cs => cs.CompoundCode);

            // Additional configuration can be added here as needed
        }
    
    }
}
