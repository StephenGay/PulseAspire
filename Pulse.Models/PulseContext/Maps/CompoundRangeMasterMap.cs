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
    public class CompoundRangeMasterMap : IEntityTypeConfiguration<CompoundRange>
    {
        public void Configure(EntityTypeBuilder<CompoundRange> builder)
        {
            builder.ToTable("CompoundRangeMaster");
            builder.HasKey(cr => cr.CompoundRangeId);
            builder.HasMany(cr => cr.CompoundRangeProperties)
                   .WithOne(crp => crp.CompoundRange)
                   .HasForeignKey(crp => crp.CompoundRangeId);
            builder.HasMany(cr => cr.Compounds)
                   .WithOne(c => c.CompoundRange)
                   .HasForeignKey(c => c.CompoundRangeId);

        }
    }
}
