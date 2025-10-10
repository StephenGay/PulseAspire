using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using Pulse.Models.Geographic;
using Pulse.Models.Organizational;
using Pulse.Models.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientMasterMap : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            //throw new NotImplementedException();
            builder.ToTable("ClientMaster");
            builder.HasOne(c => c.Region)
                   .WithMany(r => r.Customers)
                   .HasForeignKey(c => c.RegionID);
            builder.HasOne(c => c.Company)
                   .WithMany(co => co.Customers)
                   .HasForeignKey(c => c.CompanyID);
            builder.HasOne(c => c.Industry)
                   .WithMany(i => i.Customers)
                   .HasForeignKey(c => c.IndustryID);
        }
    }
}
