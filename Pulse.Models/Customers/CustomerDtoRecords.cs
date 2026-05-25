using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Customers
{
    public class CustomerUpdateDto
    {
        public string? ClientName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        public string? Address5 { get; set; }
        public string? PostalAddress1 { get; set; }
        public string? PostalAddress2 { get; set; }
        public string? PostalAddress3 { get; set; }
        public string? PostalAddress4 { get; set; }
        public string? PostalAddress5 { get; set; }
        public string? Phone { get; set; }
        public string? EMail { get; set; }
        public string? VATNo { get; set; }
        public string? TaxCodeID { get; set; }
        public bool? RequireOrderNo { get; set; }
        public string? SalesRepID { get; set; }
        public int? RegionID { get; set; }
        public int? IndustryID { get; set; }
        public bool? SisterCompany { get; set; }
        public bool? Blocked { get; set; }
        public int? TermDays { get; set; }
        public decimal? CreditLimit { get; set; }
        public string? AiSummary { get; set; }
        
    }
}
