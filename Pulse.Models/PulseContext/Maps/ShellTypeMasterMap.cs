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
    public class ShellTypeMasterMap : IEntityTypeConfiguration<ShellType>
    {
        public void Configure(EntityTypeBuilder<ShellType> builder)
        {
            builder.ToTable("ShellTypeMaster");
        }
    }
    
    
}
