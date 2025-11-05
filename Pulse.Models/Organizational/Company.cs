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
        public ICollection<Division>? Divisions { get; set; } = new HashSet<Division>();
        public ICollection<Customer>? Customers { get; set; } = new HashSet<Customer>();
        public ICollection<SalesRepresentative> SalesRepresentatives { get;set; } = new HashSet<SalesRepresentative>();
    }
}
