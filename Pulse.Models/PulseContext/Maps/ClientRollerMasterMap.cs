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
    public class ClientRollerMasterMap : IEntityTypeConfiguration<ClientRoller>
    {
        public void Configure(EntityTypeBuilder<ClientRoller> builder)
        {
            builder.ToTable("ClientRollerMaster");

            // Primary Key (manual / not database generated)
            builder.HasKey(e => e.ClientRollerID);

            builder.Property(e => e.ClientRollerID)
                .ValueGeneratedNever()
                .IsRequired();

            // Required scalar properties
            builder.Property(e => e.ClientRollerSpecificationID).IsRequired();

            builder.Property(e => e.ClientRollerNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.ShellDiameter)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(e => e.ShellLength)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(e => e.ShellWeight)
                .IsRequired()
                .HasPrecision(12, 3);

            builder.Property(e => e.CoverDiameter)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(e => e.ShellDefects)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            //builder.HasOne(e => e.ClientRollerSpecification)
            //    .WithMany()
            //    .HasForeignKey(e => e.ClientRollerSpecificationID)
            //    .OnDelete(DeleteBehavior.Restrict)
            //    .IsRequired();

            //builder.HasMany(e => e.WorksOrders)
            //    .WithOne()
            //    .HasForeignKey("ClientRollerID")   // Uses convention/shadow property if FK not yet exposed on WorksOrder
            //    .OnDelete(DeleteBehavior.Restrict);

            

            builder.HasOne(crs => crs.ClientRollerSpecification)
                .WithMany(cr => cr.ClientRollers)
                .HasForeignKey(crs => crs.ClientRollerSpecificationID);

            builder.HasMany(w => w.WorksOrders)
                .WithOne(cr => cr.ClientRoller)
                .HasForeignKey(w => w.ClientRollerID);

            // Useful indexes for common lookups
            builder.HasIndex(e => e.ClientRollerNumber);
            builder.HasIndex(e => e.ClientRollerSpecificationID);
            builder.HasIndex(e => e.IsActive);
        }
    }
}
