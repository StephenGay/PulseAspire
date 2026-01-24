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
    public class ContinentMasterMap : IEntityTypeConfiguration<Continent>
    {
        public void Configure(EntityTypeBuilder<Continent> builder)
        {
            builder.ToTable("ContinentMaster");
            builder.HasKey(c => c.ContinentID);  // ✅ Add
            builder.Property(c => c.ContinentName).HasMaxLength(100).IsRequired();  // ✅ Add
            builder.Property(c => c.IsActive).HasDefaultValue(true);  // ✅ Add
            
            builder.HasMany(ct => ct.Countries)
                   .WithOne(c => c.Continent)
                   .HasForeignKey(c => c.ContinentID);
        }

        
    }
    
}

