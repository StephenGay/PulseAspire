using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Organizational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class CompanyMasterMap : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("CompanyMaster");
            builder.HasKey(c => c.CompanyID);
            builder.Property(c => c.CompanyName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.IsActive).HasDefaultValue(true);
            
            builder.HasMany(c => c.Divisions)
                   .WithOne(d => d.Company)
                   .HasForeignKey(d => d.CompanyID);
            
            builder.HasMany(c => c.Customers)
                   .WithOne(cu => cu.Company)
                   .HasForeignKey(cu => cu.CompanyID);
            
            builder.HasMany(c => c.SalesRepresentatives)
                   .WithOne(sr => sr.Company)
                   .HasForeignKey(sr => sr.CompanyID);
        }
    }
    
}
