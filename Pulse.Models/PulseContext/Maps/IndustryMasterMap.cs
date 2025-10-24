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
    public class IndustryMasterMap : IEntityTypeConfiguration<Industry>
    {
        public void Configure(EntityTypeBuilder<Industry> builder)
        {
            builder.ToTable("IndustryMaster");
            builder.HasMany(i => i.Customers)
                   .WithOne(c => c.Industry)
                   .HasForeignKey(c => c.IndustryID);
            builder.HasMany(i => i.IndustryProcesses)
                   .WithOne(ip => ip.Industry)
                   .HasForeignKey(ip => ip.IndustryID);
            builder.HasMany(i => i.IndustryRollerEnvironments)
                     .WithOne(ire => ire.Industry)
                     .HasForeignKey(ire => ire.IndustryId);
            

        }
    }
}
