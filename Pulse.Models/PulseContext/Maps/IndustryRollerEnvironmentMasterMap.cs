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
    public class IndustryRollerEnvironmentMasterMap : IEntityTypeConfiguration<IndustryRollerEnvironment>
    {
        public void Configure(EntityTypeBuilder<IndustryRollerEnvironment> builder)
        {
            builder.ToTable("IndustryRollerEnvironmentMaster");
            builder.HasOne(ire => ire.Industry)
                   .WithMany(i => i.IndustryRollerEnvironments)
                   .HasForeignKey(ire => ire.IndustryId);
            builder.HasOne(ire => ire.RollerType)
                   .WithMany(rt => rt.IndustryRollerEnvironments)
                   .HasForeignKey(ire => ire.RollerTypeId);
            builder.HasOne(ire => ire.IndustryProcess)
                     .WithMany(ip => ip.IndustryRollerEnvironments)
                     .HasForeignKey(ire => ire.IndustryProcessId);
            builder.HasMany(ire => ire.IndustryRecommendedCovers)
                   .WithOne(irc => irc.IndustryRollerEnvironment)
                   .HasForeignKey(irc => irc.IndustryRollerEnvironmentId);
        }
    }
}
