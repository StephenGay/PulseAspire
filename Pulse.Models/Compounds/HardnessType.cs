using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Compounds
{
    public class HardnessType
    {
        [Key]
        [Required]
        [MaxLength(50)]
        public required string HardnessTypeName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
