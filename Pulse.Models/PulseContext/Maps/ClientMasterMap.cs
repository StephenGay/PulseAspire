using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;

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
            builder.HasOne(re => re.SalesRepresentative)
                .WithMany(s => s.Customers)
                .HasForeignKey(re => re.SalesRepID);
            builder.HasMany(c => c.ClientSales)
                .WithOne(cs => cs.Customer)
                .HasForeignKey(cs => cs.FullClientID);
            builder.HasMany(c => c.ClientContacts)
                .WithOne(cc => cc.Customer)
                .HasForeignKey(cc => cc.FullClientID);

            builder.Property(e => e.Ageing01).HasPrecision(18, 2);
            builder.Property(e => e.Ageing02).HasPrecision(18, 2);
            builder.Property(e => e.Ageing03).HasPrecision(18, 2);
            builder.Property(e => e.Ageing04).HasPrecision(18, 2);
            builder.Property(e => e.Ageing05).HasPrecision(18, 2);
            builder.Property(e => e.CreditLimit).HasPrecision(18, 2);
        
        }
    }
}
