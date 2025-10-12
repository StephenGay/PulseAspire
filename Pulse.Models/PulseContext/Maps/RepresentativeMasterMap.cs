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
    public class RepresentativeMasterMap : IEntityTypeConfiguration<SalesRepresentative>
    {
        public void Configure(EntityTypeBuilder<SalesRepresentative> builder)
        {
            builder.ToTable("RepresentativeMaster");
            builder.HasOne(c => c.Company)
                .WithMany(r => r.SalesRepresentatives)
                .HasForeignKey(c => c.CompanyID);

        }
    }
}
