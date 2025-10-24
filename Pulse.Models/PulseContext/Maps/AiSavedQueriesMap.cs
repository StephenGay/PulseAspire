using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Misc;

namespace Pulse.Models.PulseContext.Maps
{
    public class AiSavedQueriesMap : IEntityTypeConfiguration<AiQuery>
    {
        public void Configure(EntityTypeBuilder<AiQuery> builder)
        {
            builder.ToTable("AiSavedQueries");
            builder.HasKey(a => a.AiQueryID);
            builder.Property(a => a.Question).IsRequired();
            builder.Property(a => a.SqlQuery).IsRequired();
            builder.Property(a => a.Timestamp).IsRequired();
            builder.Property(a => a.IsActive).HasDefaultValue(true);
            builder.HasMany(f => f.UserFavouriteQueries)
                    .WithOne(q => q.AiQuery)
                    .HasForeignKey(q => q.QueryId);
        }
    }
}

