using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Misc
{
    public class Colour
    {
        [Key]
        [Required]
        [MaxLength(100)]
        public required string ColourName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
