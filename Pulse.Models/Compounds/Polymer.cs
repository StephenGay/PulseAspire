using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Compounds
{
    public class Polymer
    {
        [Key]
        [Required]
        [MaxLength(50)]
        public required string PolymerName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
