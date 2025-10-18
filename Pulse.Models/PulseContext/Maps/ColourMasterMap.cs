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
    public class ColourMasterMap : IEntityTypeConfiguration<Colour>
        {
        public void Configure(EntityTypeBuilder<Colour> builder)
        {
            builder.ToTable("ColourMaster");
            builder.HasKey(c => c.ColourName);
            builder.Property(c => c.ColourName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        }
    }
}
