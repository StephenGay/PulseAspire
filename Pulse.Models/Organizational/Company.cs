using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.Models.Customers;

namespace Pulse.Models.Organizational
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string CompanyName { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Division> Divisions { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public List<SalesRepresentative> SalesRepresentatives { get;set; } = new();
    }
}
