using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.AI.Ali;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps
{
    public class AnalysisRequestMap : IEntityTypeConfiguration<AnalysisRequest>
    {
        public void Configure(EntityTypeBuilder<AnalysisRequest> builder)
        {
            builder.ToTable("AnalysisRequests","pai");
            builder.HasIndex(r => r.Status);
        }
    }
}
