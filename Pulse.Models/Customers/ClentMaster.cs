using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.Models.Geographic;
using Pulse.Models.Industries;
using Pulse.Models.Organizational;

namespace Pulse.Models.Customers
{
    public class Customer
    {
        [Key]
        [MaxLength(10)]
        public required string FullClientID { get; set; }
        [Required]
        public required int CompanyID { get; set; }

        [Required]
        [MaxLength(10)]
        public required string FullChargeClientID { get; set; }

        [Required]
        [MaxLength(6)]
        public required string ClientID { get; set; }
        [Required]
        [MaxLength(40)]
        public required string ClientName { get; set; }

        [MaxLength(40)]
        public string? Address1 { get; set; }
        [MaxLength(40)]
        public string? Address2 { get; set; }
        [MaxLength(40)]
        public string? Address3 { get; set; }
        [MaxLength(40)]
        public string? Address4 { get; set; }
        [MaxLength(40)]
        public string? Address5 { get; set; }

        [MaxLength(40)]
        public string? PostalAddress1 { get; set; }
        [MaxLength(40)]
        public string? PostalAddress2 { get; set; }
        [MaxLength(40)]
        public string? PostalAddress3 { get; set; }
        [MaxLength(40)]
        public string? PostalAddress4 { get; set; }
        [MaxLength(40)]
        public string? PostalAddress5 { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? EMail { get; set; }

        [MaxLength(16)]
        public string? VATNo { get; set; }

        [DefaultValue("00")]
        [MaxLength(2)]
        [Required]
        public required string TaxCodeID { get; set; }

        [DefaultValue(false)]
        public bool RequireOrderNo { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(10)]
        public string? SalesRepID { get; set; }

        public int? RegionID { get; set; }

        public int? IndustryID { get; set; }

        [DefaultValue(false)]
        public bool SisterCompany { get; set; }

        [DefaultValue(false)]
        public bool Blocked { get; set; }

        [DefaultValue(0)]
        public int? TermDays { get; set; }

        [DefaultValue(0)]
        public decimal? CreditLimit { get; set; }

        [DefaultValue(0)]
        public decimal? Ageing01 { get; set; }
        [DefaultValue(0)]
        public decimal? Ageing02 { get; set; }
        [DefaultValue(0)]
        public decimal? Ageing03 { get; set; }
        [DefaultValue(0)]
        public decimal? Ageing04 { get; set; }
        [DefaultValue(0)]
        public decimal? Ageing05 { get; set; }

        public DateTime? LastVisitDate { get; set; }
        public Region? Region { get; set; }
        public Industry? Industry { get; set; }
        public Company? Company { get; set; }
        public SalesRepresentative? SalesRepresentative { get; set; }
        public List<ClientSales>? ClientSales { get; set; }

    }
}
