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
    public class PolymerMasterMap : IEntityTypeConfiguration<Polymer>
    {
        public void Configure(EntityTypeBuilder<Polymer> builder)
        {
            builder.ToTable("PolymerMaster");
            builder.HasKey(p => p.PolymerName);
            builder.Property(p => p.PolymerName).HasMaxLength(100).IsRequired();
            builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        }
    }
}
