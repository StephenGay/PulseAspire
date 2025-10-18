using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Geographic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class RegionMasterMap : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ToTable("RegionMaster");
            builder.HasKey(x => x.RegionID);
            builder.Property(r => r.RegionName).HasMaxLength(100).IsRequired();
            builder.Property(r => r.IsActive).IsRequired().HasDefaultValue(true);
            builder.HasOne(r => r.Province)
                   .WithMany(p => p.Regions)
                   .HasForeignKey(r => r.ProvinceID);
        }
    }
    
}

