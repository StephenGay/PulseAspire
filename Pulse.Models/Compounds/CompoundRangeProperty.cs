using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Compounds
{
    public class CompoundRangeProperty
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int CompoundRangePropertyId { get; set; }
        [Required]
        public required int CompoundRangeId { get; set; }
        [Required]
        [MaxLength(255)]
        public required string PropertyText { get; set; }
        [Required]
        public required CompoundRange CompoundRange { get; set; }
    }
}
