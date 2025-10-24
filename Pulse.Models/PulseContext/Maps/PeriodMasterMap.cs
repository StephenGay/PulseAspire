using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class PeriodMasterMap : IEntityTypeConfiguration<Period>
    {
        public void Configure(EntityTypeBuilder<Period> builder)
        {
            builder.ToTable("PeriodMaster");
            builder.HasMany(p => p.ClientSales)
                   .WithOne(cs => cs.Period)
                   .HasForeignKey(cs => cs.PeriodID);
        }
    }
}
