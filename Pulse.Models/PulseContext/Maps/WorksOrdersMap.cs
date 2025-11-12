using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.PulseContext.Maps
{
    public class WorksOrderMap : IEntityTypeConfiguration<WorksOrder>
    {
        public void Configure(EntityTypeBuilder<WorksOrder> builder)
        {
            // Table & Schema
            builder.ToTable("WorksOrder");

            // Primary Key
            builder.HasKey(wo => wo.WorksOrderNo)
                   .HasName("PK__WorksOrder");

            // Identity (auto-increment)

            // Required Fields
            builder.Property(wo => wo.FullClientID)
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(wo => wo.PeriodID)
                   .IsRequired();

            builder.Property(wo => wo.WorkTypeID)
                   .IsRequired();

            builder.Property(wo => wo.Quantity)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // Optional Fields
            builder.Property(wo => wo.Description)
                   .HasMaxLength(1000);

            builder.Property(wo => wo.DivisionID)
                   .HasMaxLength(5);

            builder.Property(wo => wo.ClientRollNo)
                   .HasMaxLength(50);

            builder.Property(wo => wo.CoverCompoundCode)
                   .HasMaxLength(5);

            builder.Property(wo => wo.ClientOrderNo)
                   .HasMaxLength(100);

            builder.Property(wo => wo.ClientPRNo)
                   .HasMaxLength(100);

            builder.Property(wo => wo.ClientRFQNo)
                   .HasMaxLength(100);

            builder.Property(wo => wo.Status)
                   .HasMaxLength(50);

            // Decimal Precision
            builder.Property(wo => wo.ShellLength)
                   .HasColumnType("decimal(18,2)");

            builder.Property(wo => wo.ShellDiameter)
                   .HasColumnType("decimal(18,2)");

            builder.Property(wo => wo.CoverDiameter)
                   .HasColumnType("decimal(18,2)");

            builder.Property(wo => wo.UndelQty)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(wo => wo.MaterialCost)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(wo => wo.SellPrice)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            // Date
            builder.Property(wo => wo.DateStarted)
                   .HasDefaultValueSql("GETUTCDATE()");

            // === Foreign Keys & Relationships ===

            // Customer (FullClientID → Customer.FullClientID)
            builder.HasOne(wo => wo.Customer)
                   .WithMany(c => c.WorksOrders)
                   .HasForeignKey(wo => wo.FullClientID);
            //.OnDelete(DeleteBehavior.Restrict)
            //.HasConstraintName("FK_WorksOrder_Customer");

            // Period
            builder.HasOne(wo => wo.Period)
                   .WithMany()
                   .HasForeignKey(wo => wo.PeriodID);
                   //.OnDelete(DeleteBehavior.Restrict)
                   //.HasConstraintName("FK_WorksOrder_Period");

            // WorkType
            builder.HasOne(wo => wo.WorkType)
                   .WithMany()
                   .HasForeignKey(wo => wo.WorkTypeID);
                   //.OnDelete(DeleteBehavior.Restrict)
                   //.HasConstraintName("FK_WorksOrder_WorkType");

            // Division
            builder.HasOne(wo => wo.Division)
                   .WithMany()
                   .HasForeignKey(wo => wo.DivisionID)
                   .OnDelete(DeleteBehavior.NoAction);
                   //.HasConstraintName("FK_WorksOrder_Division");

            // ClientRollerSpecification
            builder.HasOne(wo => wo.ClientRollerSpecification)
                   .WithMany(crs => crs.WorksOrders)
                   .HasForeignKey(wo => wo.ClientRollerSpecificationID);
                   //.OnDelete(DeleteBehavior.SetNull)
                   //.HasConstraintName("FK_WorksOrder_ClientRollerSpecification");

            // ClientRoller
            builder.HasOne(wo => wo.ClientRoller)
                   .WithMany(cr => cr.WorksOrders)
                   .HasForeignKey(wo => wo.ClientRollerID);
            //.OnDelete(DeleteBehavior.SetNull)
            //.HasConstraintName("FK_WorksOrder_ClientRoller");
            builder.HasOne(wo => wo.ProductionStage)
                 .WithMany()
                 .HasForeignKey(wo => wo.ProductionStageID);

         
                   //.OnDelete(DeleteBehavior.SetNull)
                   //.HasConstraintName("FK_WorksOrder_Compound");

            // === Indexes ===
            builder.HasIndex(wo => wo.FullClientID)
                   .HasDatabaseName("IX_WorksOrder_FullClientID");

            builder.HasIndex(wo => wo.PeriodID)
                   .HasDatabaseName("IX_WorksOrder_PeriodID");

            builder.HasIndex(wo => wo.Status)
                   .HasDatabaseName("IX_WorksOrder_Status");

            builder.HasIndex(wo => wo.DateStarted)
                   .HasDatabaseName("IX_WorksOrder_DateStarted");

            // Composite index for common queries
            builder.HasIndex(wo => new { wo.FullClientID, wo.PeriodID, wo.Status })
                   .HasDatabaseName("IX_WorksOrder_ClientPeriodStatus");
        }
    }
}
