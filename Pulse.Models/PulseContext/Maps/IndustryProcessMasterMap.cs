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
    public class IndustryProcessMasterMap : IEntityTypeConfiguration<IndustryProcess>
    {
        public void Configure(EntityTypeBuilder<IndustryProcess> builder)
        {
            builder.ToTable("IndustryProcessMaster");
            builder.HasOne(ip => ip.Industry)
                   .WithMany(i => i.IndustryProcesses)
                   .HasForeignKey(ip => ip.IndustryID);
        }
    }
}
