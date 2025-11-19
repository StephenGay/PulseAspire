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
    public class ProductionPlanItemsMap : IEntityTypeConfiguration<ProductionPlanItem>
    {
        public void Configure(EntityTypeBuilder<ProductionPlanItem> builder)
        {
            builder.ToTable("ProductionPlanItems");

            builder.HasKey(p => p.ProductionPlanItemID);

            builder.Property(p => p.ProductionPlanItemID)
                .HasColumnType("int")
                .ValueGeneratedOnAdd() // Assuming identity column for auto-increment
                .IsRequired();

            builder.Property(p => p.WorkOrderNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.DivisionID)
                .HasColumnType("nvarchar(5)")
                .HasMaxLength(5)
                .IsRequired();

            builder.Property(p => p.ProductionStageID)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.EquipmentItemID)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10);

            builder.Property(p => p.PlannedStartTime)
                .HasColumnType("datetime2");

            builder.Property(p => p.PlannedEndTime)
                .HasColumnType("datetime2");

            builder.Property(p => p.ActualStartTime)
                .HasColumnType("datetime2");

            builder.Property(p => p.ActualEndTime)
                .HasColumnType("datetime2");

            builder.Property(p => p.ClosedByUserID)
                .HasColumnType("int");

            builder.Property(p => p.Status)
                .HasColumnType("nvarchar(15)")
                .HasMaxLength(15)
                .HasDefaultValue("Unplanned")
                .IsRequired();

            builder.Property(p => p.IsPulsePlan)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            //If there are relationships(e.g., to WorkOrder, Division, ProductionStage, User), add them here.
            // For example:
            builder.HasOne(p => p.WorksOrder)
                .WithMany()
                .HasForeignKey(p => p.WorkOrderNo);
            //     .OnDelete(DeleteBehavior.Cascade); // Adjust as needed

            builder.HasOne(p => p.ProductionStage)
                .WithMany()
                .HasForeignKey(p => p.ProductionStageID);
            builder.HasOne(p => p.EquipmentItem)
                .WithMany()
                .HasForeignKey(p => p.EquipmentItemID);
        }
    }
}