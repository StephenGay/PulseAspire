using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using Pulse.Models.Geographic;
using Pulse.Models.Industries;
using Pulse.Models.Organizational;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientMasterMap : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("ClientMaster");

            // Primary Key (explicit for clarity, even if [Key] in entity)
            builder.HasKey(c => c.FullClientID);

            // Properties with constraints (map lengths, required, defaults)
            builder.Property(c => c.FullClientID)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(c => c.CompanyID)
                .IsRequired();

            builder.Property(c => c.FullChargeClientID)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(c => c.ClientID)
                .HasMaxLength(6)
                .IsRequired();

            builder.Property(c => c.ClientName)
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(c => c.Address1).HasMaxLength(40);
            builder.Property(c => c.Address2).HasMaxLength(40);
            builder.Property(c => c.Address3).HasMaxLength(40);
            builder.Property(c => c.Address4).HasMaxLength(40);
            builder.Property(c => c.Address5).HasMaxLength(40);
            builder.Property(c => c.PostalAddress1).HasMaxLength(40);
            builder.Property(c => c.PostalAddress2).HasMaxLength(40);
            builder.Property(c => c.PostalAddress3).HasMaxLength(40);
            builder.Property(c => c.PostalAddress4).HasMaxLength(40);
            builder.Property(c => c.PostalAddress5).HasMaxLength(40);
            builder.Property(c => c.Phone).HasMaxLength(15);
            builder.Property(c => c.EMail).HasMaxLength(255);
            builder.Property(c => c.VATNo).HasMaxLength(16);

            builder.Property(c => c.TaxCodeID)
                .HasMaxLength(2)
                .IsRequired()
                .HasDefaultValue("00");

            builder.Property(c => c.RequireOrderNo)
                .HasDefaultValue(false);

            builder.Property(c => c.SisterCompany)
                .HasDefaultValue(false);

            builder.Property(c => c.Blocked)
                .HasDefaultValue(false);

            builder.Property(c => c.TermDays)
                .HasDefaultValue(0);

            builder.Property(c => c.CreditLimit)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(c => c.Ageing01)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(c => c.Ageing02)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(c => c.Ageing03)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(c => c.Ageing04)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(c => c.Ageing05)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            // Relationships with delete behaviors (prevent orphans)

            // NOTE: Cannot implement Delete behavious until Transfer Data Program
            // Updated properly - then reinstate

            builder.HasOne(c => c.Region)
                .WithMany(r => r.Customers)
                .HasForeignKey(c => c.RegionID);
            // .OnDelete(DeleteBehavior.SetNull); // Optional FK, set to null on delete

            builder.HasOne(c => c.Company)
                .WithMany(co => co.Customers)
                .HasForeignKey(c => c.CompanyID);
            //.OnDelete(DeleteBehavior.Cascade); // Cascade delete if company removed

            builder.HasOne(c => c.Industry)
                .WithMany(i => i.Customers)
                .HasForeignKey(c => c.IndustryID);
            //.OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.SalesRepresentative)
                .WithMany(s => s.Customers)
                .HasForeignKey(c => c.SalesRepID);
            //.OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.ClientSales)
                .WithOne(cs => cs.Customer)
                .HasForeignKey(cs => cs.FullClientID);
            //.OnDelete(DeleteBehavior.Cascade); // Delete sales if customer removed

            builder.HasMany(c => c.ClientContacts)
                .WithOne(cc => cc.Customer)
                .HasForeignKey(cc => cc.FullClientID);
            //.OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.ClientRollerSpecifications)
                .WithOne(crs => crs.Customer)
                .HasForeignKey(crs => crs.FullClientID);
                //.OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.WorksOrders)
                .WithOne(wo => wo.Customer)
                .HasForeignKey(wo => wo.FullClientID);
            //.OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(c => c.ClientName) // For name searches
                .HasDatabaseName("IX_ClientMaster_ClientName");

            builder.HasIndex(c => c.RegionID) // For region-based joins
                .HasDatabaseName("IX_ClientMaster_RegionID");

            builder.HasIndex(c => c.IndustryID) // For industry filters
                .HasDatabaseName("IX_ClientMaster_IndustryID");

            builder.HasIndex(c => c.SalesRepID) // For sales rep reports
                .HasDatabaseName("IX_ClientMaster_SalesRepID");
        }
    }
}


//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Pulse.Models.Customers;

//namespace Pulse.Models.PulseContext.Maps
//{
//    public class ClientMasterMap : IEntityTypeConfiguration<Customer>
//    {
//        public void Configure(EntityTypeBuilder<Customer> builder)
//        {
//            //throw new NotImplementedException();
//            builder.ToTable("ClientMaster");
//            builder.HasOne(c => c.Region)
//                   .WithMany(r => r.Customers)
//                   .HasForeignKey(c => c.RegionID);
//            builder.HasOne(c => c.Company)
//                   .WithMany(co => co.Customers)
//                   .HasForeignKey(c => c.CompanyID);
//            builder.HasOne(c => c.Industry)
//                   .WithMany(i => i.Customers)
//                   .HasForeignKey(c => c.IndustryID);
//            builder.HasOne(re => re.SalesRepresentative)
//                .WithMany(s => s.Customers)
//                .HasForeignKey(re => re.SalesRepID);
//            builder.HasMany(c => c.ClientSales)
//                .WithOne(cs => cs.Customer)
//                .HasForeignKey(cs => cs.FullClientID);
//            builder.HasMany(c => c.ClientContacts)
//                .WithOne(cc => cc.Customer)
//                .HasForeignKey(cc => cc.FullClientID);

//            builder.Property(e => e.Ageing01).HasPrecision(18, 2);
//            builder.Property(e => e.Ageing02).HasPrecision(18, 2);
//            builder.Property(e => e.Ageing03).HasPrecision(18, 2);
//            builder.Property(e => e.Ageing04).HasPrecision(18, 2);
//            builder.Property(e => e.Ageing05).HasPrecision(18, 2);
//            builder.Property(e => e.CreditLimit).HasPrecision(18, 2);

//        }
//    }
//}
