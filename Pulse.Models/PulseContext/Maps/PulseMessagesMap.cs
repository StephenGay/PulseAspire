using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Communication;

namespace Pulse.Models.PulseContext.Maps
{
    public class PulseMessagesMap : IEntityTypeConfiguration<PulseMessage>
    {
        public void Configure(EntityTypeBuilder<PulseMessage> builder)
        {
            // Table name (plural is conventional for messaging tables)
            builder.ToTable("PulseMessages");

            // Primary key
            builder.HasKey(m => m.Id);

            // Id configuration
            // Note: Id is currently nullable int?. 
            // If you want SQL Server IDENTITY (auto-increment), change the property to non-nullable int Id { get; set; }
            // EF Core will then automatically treat it as IDENTITY.
            // With nullable int?, IDENTITY is not applied (column will be nullable).
            builder.Property(m => m.Id)
                   .HasColumnType("int")
                    .ValueGeneratedOnAdd() // Assuming identity column for auto-increment
                    .IsRequired();

            // User-related fields (assuming AspNet Identity string IDs)
            builder.Property(m => m.RecipientUserId)
                   .HasMaxLength(450);

            builder.Property(m => m.RecipientUserName)
                   .HasMaxLength(256);

            builder.Property(m => m.SenderUserId)
                   .HasMaxLength(450);

            builder.Property(m => m.SenderUserName)
                   .HasMaxLength(256);

            builder.Property(m => m.Subject)
                   .HasMaxLength(200);

            // Role
            builder.Property(m => m.Role)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("user");

            // Content (message body - large text, so nvarchar(max) by default)
            builder.Property(m => m.Content)
                   .IsRequired();

            // ContentType
            builder.Property(m => m.ContentType)
                   .IsRequired()
                   .HasMaxLength(10)
                   .HasDefaultValue("HTML");

            // SentAt - use server time instead of client time
            builder.Property(m => m.SentAt)
                   .HasDefaultValueSql("GETUTCDATE()")
                   .ValueGeneratedOnAdd();

            // Delivered
            builder.Property(m => m.Delivered)
                   .HasDefaultValue(false)
                   .ValueGeneratedOnAdd();

            // Indexes - important for messaging performance
            // Individual indexes for quick lookups by user
            builder.HasIndex(m => m.SenderUserId);
            builder.HasIndex(m => m.RecipientUserId);

            // Additional index for time-based queries (e.g., recent messages)
            builder.HasIndex(m => m.SentAt);
        }
    }
}
