using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientSalesMap : IEntityTypeConfiguration<ClientSales>
    {
        public void Configure(EntityTypeBuilder<ClientSales> builder)
        {
            builder.ToTable("ClientSales");
            builder.HasOne(x => x.Customer)
                .WithMany(y => y.ClientSales)
                .HasForeignKey(x => x.FullClientID);
            builder.HasOne(a => a.Period)
                .WithMany(z => z.ClientSales)
                .HasForeignKey(a => a.PeriodID);
            builder.Property(e => e.Amount).HasPrecision(18, 2);
        }
    }
}
