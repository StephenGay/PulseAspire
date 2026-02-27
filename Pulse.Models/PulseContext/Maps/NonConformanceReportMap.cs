using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pulse.Models.Production.NonConformance;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.PulseContext.Maps
{
    public class NonConformanceReportMap : IEntityTypeConfiguration<NonConformanceReport>
    {
        public void Configure(EntityTypeBuilder<NonConformanceReport> builder)
        {
            builder.ToTable("NonConformanceReports");
            builder.HasKey(n => n.ID);

            builder.Property(n => n.ID)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(n => n.NCRNo)
                .IsRequired(false);

            builder.Property(n => n.TypeOfNCR)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.Status)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("New");

            builder.Property(n => n.AgainstDepartment)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.Cause)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(n => n.Scrapped)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("No");

            // ───────────────────────────────────────────────
            // Database defaults for timestamps
            // ───────────────────────────────────────────────
            builder.Property(n => n.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(n => n.DiscoveryDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();

            // If you prefer UTC (recommended for multi-region apps):
            // .HasDefaultValueSql("GETUTCDATE()")

            // Other dates remain nullable / no default
            builder.Property(n => n.SubmittedDate);
            builder.Property(n => n.DateClosed);

            // ───────────────────────────────────────────────
            // Flags / booleans
            // ───────────────────────────────────────────────
            builder.Property(n => n.IsRepetition)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.IsClaim)
                .IsRequired()
                .HasDefaultValue(false);

            // ───────────────────────────────────────────────
            // Numeric fields (money / weight)
            // ───────────────────────────────────────────────
            builder.Property(n => n.ScrapValue)
                .IsRequired()
                .HasDefaultValue(0.00M)
                .HasPrecision(18, 2);   // money-like → 2 decimals

            builder.Property(n => n.ScrapKgs)
                .IsRequired()
                .HasDefaultValue(0.00M)
                .HasPrecision(18, 3);   // weight → usually 3 decimals

            // Optional strings (max lengths already set)
            builder.Property(n => n.RaisedByDivisionID).HasMaxLength(5);
            builder.Property(n => n.RaisedByDivisionName).HasMaxLength(100);
            builder.Property(n => n.AssignedToDivisionID).HasMaxLength(5);
            builder.Property(n => n.AssignedToDivisionName).HasMaxLength(100);
            builder.Property(n => n.AccountType).HasMaxLength(1);
            builder.Property(n => n.AccountID).HasMaxLength(20);
            builder.Property(n => n.CustSuppName).HasMaxLength(255);
            builder.Property(n => n.ProductDescription).HasMaxLength(255);
            builder.Property(n => n.OldWONos).HasMaxLength(100);
            builder.Property(n => n.OldCompoundCodes).HasMaxLength(100);
            builder.Property(n => n.BatchNos).HasMaxLength(100);
            builder.Property(n => n.Machine).HasMaxLength(100);
            builder.Property(n => n.TechnicalReviewer).HasMaxLength(255);
            builder.Property(n => n.InvestigatorName).HasMaxLength(255);
            builder.Property(n => n.TechnicalOutcome).HasMaxLength(15);
            builder.Property(n => n.RCATechnique).HasMaxLength(100);
            builder.Property(n => n.Reason4Ms).HasMaxLength(100);

            // Long text fields → nvarchar(max)
            builder.Property(n => n.Details);
            builder.Property(n => n.FurtherActionRequired);
            builder.Property(n => n.RootCause);
            builder.Property(n => n.CorrectiveAction);
            builder.Property(n => n.PreventativeAction);
            builder.Property(n => n.RevisedPreventativeAction);
            builder.Property(n => n.ExpertComments);
            builder.Property(n => n.InvestigationComments);
            builder.Property(n => n.TechReviewComments);
        }
    }
}