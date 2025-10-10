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
    public class DivisionMasterMap : IEntityTypeConfiguration<Division>
    {
        public void Configure(EntityTypeBuilder<Division> builder)
        {
            builder.ToTable("DivisionMaster");
            builder.HasOne(d => d.Company)
                   .WithMany(c => c.Divisions)
                   .HasForeignKey(d => d.CompanyID);
            builder.HasOne(d => d.Branch)
                   .WithMany(b => b.Divisions)
                   .HasForeignKey(d => d.BranchID);
        }
    }
}
