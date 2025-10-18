using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Rollers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class RollerTypeMasterMap : IEntityTypeConfiguration<RollerType>
    {
        public void Configure(EntityTypeBuilder<RollerType> builder)
        {
            builder.ToTable("RollerTypeMaster");
        }
    }
}
