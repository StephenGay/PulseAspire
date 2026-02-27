using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Users;

namespace Pulse.Models.PulseContext.Maps
{
    internal class UserSettingsMasterMap : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder.ToTable("UserSettingsMaster");

            // Primary key configuration
            builder.HasKey(us => us.UserId);
            builder.Property(us => us.UserId).ValueGeneratedNever();

            // Property configurations
            builder.Property(us => us.AIHasVoice).HasDefaultValue(false);
            builder.Property(us => us.AIVoiceID).HasMaxLength(300);
            builder.Property(us => us.AIDefaultPref)
                .HasDefaultValue(0)
                .IsRequired();

            builder.HasOne(us => us.User)
                   .WithOne(u => u.UserSettings) 
                   .HasForeignKey<UserSettings>(us => us.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
