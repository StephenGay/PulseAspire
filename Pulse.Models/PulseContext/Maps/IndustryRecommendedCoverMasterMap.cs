using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Industries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class IndustryRecommendedCoverMasterMap : IEntityTypeConfiguration<IndustryRecommendedCover>
    {
        public void Configure(EntityTypeBuilder<IndustryRecommendedCover> builder)
        {
            builder.ToTable("IndustryRecommendedCoverMaster");
            builder.HasOne(irc => irc.IndustryRollerEnvironment)
                   .WithMany(ire => ire.IndustryRecommendedCovers)
                   .HasForeignKey(irc => irc.IndustryRollerEnvironmentId);
            builder.HasOne(irc => irc.Compound)
                     .WithMany(c => c.IndustryRecommendedCovers)
                     .HasForeignKey(irc => irc.CompoundCode);
        }
    }
}
