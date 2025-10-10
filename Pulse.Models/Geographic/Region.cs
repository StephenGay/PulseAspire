using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Geographic
{
    public class Region
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int RegionID { get; set; }
        [Required]
        [MaxLength(200)]
        public required string RegionName { get; set; }
        [Required]
        public required int ProvinceID { get; set; }
        [Required]
        public required Province Province { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Customer> Customers { get; set; } = new();
    }
}
