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
    public class BranchMasterMap : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("BranchMaster");
            builder.HasKey(b => b.BranchID);
            builder.Property(b => b.BranchName).HasMaxLength(100).IsRequired();
            builder.Property(b => b.IsActive).IsRequired().HasDefaultValue(true);
        }
    }
}
