using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Permissions;
using Pulse.Models.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps;

public class PulseThemeConfigurationMap : IEntityTypeConfiguration<PulseTheme>
{
    public void Configure(EntityTypeBuilder<PulseTheme> builder)
    {
        builder.ToTable("PulseThemes","app");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.SettingsJson)
            .HasColumnType("nvarchar(max)");

       

        
    }
}
