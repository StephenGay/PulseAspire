using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class ClientCalendarEventsMap : IEntityTypeConfiguration<ClientCalendarEvent>
    {
        public void Configure(EntityTypeBuilder<ClientCalendarEvent> builder)
        {
            builder.ToTable("ClientCalendarEvents");

            builder.HasKey(c => c.EventId);

            builder.Property(c => c.EventId)
                .HasColumnType("int")
                .ValueGeneratedOnAdd() // Assuming identity column for auto-increment
                .IsRequired();

            builder.Property(c => c.Title)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .HasDefaultValue(string.Empty);

            builder.Property(c => c.Start)
                .HasColumnType("datetime2")
                .IsRequired(); // Assuming required since not nullable in the entity

            builder.Property(c => c.End)
                .HasColumnType("datetime2");

            builder.Property(c => c.FullClientID)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10);

            builder.Property(c => c.ClientSpecificationID)
                .HasColumnType("int");

            builder.Property(c => c.ClientRollerNumber)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(c => c.ClientName)
                .HasColumnType("nvarchar(40)")
                .HasMaxLength(40);

            builder.Property(c => c.Category)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            // Relationship to Customer (many-to-one)
            // Assuming FullClientID is the foreign key to Customer's primary key (adjust FK if it's ClientSpecificationID or another)
            // Also assuming Customer has 'public ICollection<ClientCalendarEvent>? ClientCalendarEvents { get; set; }'
            builder.HasOne(c => c.Customer)
                .WithMany(cu => cu.ClientCalendarEvents) // Adjust navigation name if different in Customer
                .HasForeignKey(c => c.FullClientID); // Change to c => c.ClientSpecificationID if that's the FK and it's int
                //.OnDelete(DeleteBehavior.SetNull);  Or Restrict/Cascade as per business rules
        }
    }
}
