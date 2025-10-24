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
    public class CountryMasterMap : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("CountryMaster");
            builder.HasOne(c => c.Continent)
                   .WithMany(ct => ct.Countries)
                   .HasForeignKey(c => c.ContinentID);
            builder.HasMany(c => c.Provinces)
                   .WithOne(p => p.Country)
                   .HasForeignKey(p => p.CountryID);
        }
    }
}
