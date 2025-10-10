using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Misc
{
    public class Industry
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IndustryID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string IndustryName { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Customer>? Customers { get; set; }
    }
}
