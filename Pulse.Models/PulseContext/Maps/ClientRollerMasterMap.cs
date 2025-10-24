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
    public class ClientRollerMasterMap : IEntityTypeConfiguration<ClientRoller>
    {
        public void Configure(EntityTypeBuilder<ClientRoller> builder)
        {
            builder.ToTable("ClientRollerMaster");
            builder.HasOne(crs => crs.ClientRollerSpecification)
                .WithMany(cr => cr.ClientRollers)
                .HasForeignKey(crs => crs.ClientRollerSpecificationID);
        }
    }
}
