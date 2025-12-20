using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientBudgetMasterMap : IEntityTypeConfiguration<ClientBudgets>
    {
        public void Configure(EntityTypeBuilder<ClientBudgets> builder)
        {
            builder.ToTable("ClientBudgetMaster");
            builder.HasKey(e => e.ClientBudgetId);
            builder.Property(e => e.FullClientId)
            .IsRequired()
            .HasMaxLength(10);
            builder.Property(e => e.PeriodID)
            .IsRequired();
            builder.Property(e => e.FinancialYear)
            .IsRequired()
            .HasMaxLength(4);
            builder.Property(e => e.BudgetedSales)
            .IsRequired()
            .HasPrecision(18, 2) // Assuming standard decimal precision for financial values; adjust if needed
            .HasDefaultValue(0);

            builder.HasIndex(e => e.FullClientId)
            .HasDatabaseName("IX_ClientBudgets_FullClientId");

            // Navigation relationships
            builder.HasOne(e => e.customer)
            .WithMany(c => c.clientBudgets) // Assuming Customer has a collection of ClientBudgets; specify navigation if available (e.g., .WithMany(c => c.ClientBudgets))
            .HasForeignKey(e => e.FullClientId)
            .OnDelete(DeleteBehavior.ClientCascade); // Adjust cascade behavior as per domain rules
            builder.HasOne(e => e.period)
            .WithMany() // Assuming Period has a collection of ClientBudgets; specify navigation if available (e.g., .WithMany(p => p.ClientBudgets))
            .HasForeignKey(e => e.PeriodID)
            .OnDelete(DeleteBehavior.Restrict); // Adjust cascade behavior as per domain rules
        }
    }
}
