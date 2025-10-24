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
    public class CompoundRangePropertyMasterMap : IEntityTypeConfiguration<CompoundRangeProperty>
    {
        public void Configure(EntityTypeBuilder<CompoundRangeProperty> builder)
        {
            builder.ToTable("CompoundRangePropertyMaster");
            builder.HasKey(crp => crp.CompoundRangePropertyId);
            builder.HasOne(crp => crp.CompoundRange)
                   .WithMany(cr => cr.CompoundRangeProperties)
                   .HasForeignKey(crp => crp.CompoundRangeId);
        }
    }
}
