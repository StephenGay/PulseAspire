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
    public class ClientRollerSpecificationMasterMap : IEntityTypeConfiguration<ClientRollerSpecification>
    {
        public void Configure(EntityTypeBuilder<ClientRollerSpecification> builder)
        {
            builder.ToTable("ClientRollerSpecificationMaster");
            
            builder.HasOne(c => c.Customer)
                .WithMany(cs => cs.ClientRollerSpecifications)
                .HasForeignKey(c => c.FullClientID);
            builder.HasOne(c => c.Compound)
                .WithMany(cs => cs.ClientRollerSpecifications)
                .HasForeignKey(c => c.CompoundCode);

            builder.HasMany(cr => cr.ClientRollers)
                    .WithOne(crs => crs.ClientRollerSpecification)
                    .HasForeignKey(crs => crs.ClientRollerSpecificationID);
        }
    }
}
