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
    public class ClientContactMasterMap : IEntityTypeConfiguration<ClientContact>
    {
        public void Configure(EntityTypeBuilder<ClientContact> builder)
        {
            builder.ToTable("ClientContactMaster");
            builder.HasKey(cc => cc.ClientContactId);  // ✅ Add
            builder.Property(cc => cc.FullName).HasMaxLength(100).IsRequired();
            builder.Property(cc => cc.ContactEmail).HasMaxLength(255);
            builder.Property(cc => cc.IsActive).HasDefaultValue(true);
            
            builder.HasOne(cc => cc.Customer)
                   .WithMany(c => c.ClientContacts)
                   .HasForeignKey(cc => cc.FullClientID)
                   .HasPrincipalKey(c => c.FullClientID);
        }

    }
}
