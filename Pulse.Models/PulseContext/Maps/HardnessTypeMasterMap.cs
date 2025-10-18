using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Compounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class HardnessTypeMasterMap : IEntityTypeConfiguration<HardnessType>
    {
        public void Configure(EntityTypeBuilder<HardnessType> builder)
        {
            builder.ToTable("HardnessTypeMaster");
        }
    }
}
