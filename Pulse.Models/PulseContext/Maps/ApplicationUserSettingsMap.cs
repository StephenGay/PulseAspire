using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;

namespace Pulse.Models.PulseContext.Maps
{
    public class ApplicationUserSettingsMap : IEntityTypeConfiguration<ApplicationUserSettings>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserSettings> builder)
        {
            builder.HasKey(x => x.UserId);

            builder.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x => x.PreferredUserName)
                .IsRequired();

            builder.Property(x => x.AIHasVoice)
                .HasDefaultValue(false);

            builder.Property(x => x.AIVoiceID)
                .HasMaxLength(300);

            builder.Property(x => x.UserTheme)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("pulse");

            builder.ToTable("AspNetUserSettings");
        }
    }
}