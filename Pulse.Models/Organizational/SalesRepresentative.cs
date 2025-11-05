using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Organizational
{
    public class SalesRepresentative
    {
        [Key]
        [Required]
        [MaxLength(10)]
        public required string RepresentativeID { get; set; }
        [Required]
        public required int CompanyID { get; set; }
        [Required]
        [MaxLength(50)]
        public required string RepresentativeName { get; set; }
        public int? LinkedUserID { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        public Company Company { get; set; }
        public ICollection<Customer>? Customers { get; set; } = new HashSet<Customer>();
    }
}
