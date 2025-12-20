using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using Pulse.Models.Organizational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientSalesMap : IEntityTypeConfiguration<ClientSale>
    {
        public void Configure(EntityTypeBuilder<ClientSale> builder)
        {
            builder.ToTable("ClientSales");

            // Primary Key
            builder.HasKey(cs => cs.Id);

            // Properties with constraints
            builder.Property(cs => cs.FullClientID)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(cs => cs.PeriodID)
                .IsRequired();

            builder.Property(cs => cs.CompanyID)
                .IsRequired();

            builder.Property(cs => cs.Amount)
                .HasPrecision(18, 2) // Standard for financial data (e.g., $1234.56)
                .IsRequired()
                .HasDefaultValue(0m); // Enforce DB default

            builder.HasOne(x => x.Customer)
                .WithMany(y => y.ClientSales)
                .HasForeignKey(x => x.FullClientID);

            builder.HasOne(a => a.Period)
                .WithMany(z => z.ClientSales)
                .HasForeignKey(a => a.PeriodID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Company>() // No navigation property in ClientSales, but FK exists
                .WithMany() // Assuming Company has no collection for ClientSales
                .HasForeignKey(cs => cs.CompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.HasIndex(cs => cs.FullClientID)
                .HasDatabaseName("IX_ClientSales_FullClientID"); // For joins with ClientMaster

            builder.HasIndex(cs => cs.PeriodID)
                .HasDatabaseName("IX_ClientSales_PeriodID"); // For sales by period

            builder.HasIndex(cs => cs.CompanyID)
                .HasDatabaseName("IX_ClientSales_CompanyID");
        }
    }
}
