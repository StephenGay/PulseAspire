using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Production.NonConformance
{
    public class NonConformanceReport
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public int? NCRNo { get; set; }
        [MaxLength(100)]
        [Required]
        public required string TypeOfNCR { get; set; } = string.Empty;
        [MaxLength(50)]
        [Required]
        public required string Status { get; set; } = "New";
        [MaxLength(5)]
        public string? RaisedByDivisionID { get; set; }
        [MaxLength(100)]
        public string? RaisedByDivisionName { get; set; }
        [MaxLength(5)]
        public string? AssignedToDivisionID { get; set; }
        [MaxLength(100)]
        public string? AssignedToDivisionName { get; set; }
        [MaxLength(100)]
        [Required]
        public required string AgainstDepartment { get; set; } = string.Empty;
        public string? Details { get; set; }
        [MaxLength(1)]
        public string? AccountType { get; set; }
        [MaxLength(20)]
        public string? AccountID { get; set; }
        [MaxLength(255)]
        public string? CustSuppName { get; set; }
        [MaxLength(255)]
        public string? ProductDescription { get; set; }
        [Required]
        public required DateTime DiscoveryDate { get; set; } = DateTime.Now;
        public DateTime? SubmittedDate { get; set; }
        [MaxLength(100)]
        [Required]
        public required string Cause { get; set; } = string.Empty;
        [Required]
        [DefaultValue(false)]
        public bool IsRepetition { get; set; } = false;
        [MaxLength(100)]
        public string? OldWONos { get; set; }
        [MaxLength(100)]
        public string? OldCompoundCodes { get; set; }
        [MaxLength(100)]
        public string? BatchNos { get; set; }
        [MaxLength(100)]
        public string? Machine { get; set; }
        [Required]
        [MaxLength(20)]
        [DefaultValue("No")]
        public string Scrapped { get; set; } = "No";
        [Required]
        [DefaultValue(false)]
        public bool IsClaim { get; set; } = false;
        [Required, DefaultValue(0.00)]
        public required decimal ScrapValue { get; set; } = 0.00M;
        [Required, DefaultValue(0.00)]
        public required decimal ScrapKgs { get; set; } = 0.00M;
        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.Now;
        [MaxLength(255)]
        [Required]
        public required string CreatedBy { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? TechnicalReviewer { get; set; }
        [MaxLength(255)]
        public string? InvestigatorName { get; set; }
        public string? FurtherActionRequired { get; set; }
        public DateTime? DateClosed { get; set; }
        [MaxLength(15)]
        public string? TechnicalOutcome { get; set; }
        [MaxLength(100)]
        public string? RCATechnique { get; set; }
        [MaxLength(100)]
        public string? Reason4Ms { get; set; }
        public string? RootCause { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventativeAction { get; set; }
        public string? RevisedPreventativeAction { get; set; }
        public string? ExpertComments { get; set; }
        public string? InvestigationComments { get; set; }
        public string? TechReviewComments { get; set; }

    }
}
