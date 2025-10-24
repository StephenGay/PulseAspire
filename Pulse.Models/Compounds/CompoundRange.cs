using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Compounds
{
    public class CompoundRange
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompoundRangeId { get; set; }
        [Required]
        [MaxLength(100)]
        public required string RangeName { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public ICollection<CompoundRangeProperty> CompoundRangeProperties { get; set; } = new List<CompoundRangeProperty>();
        public ICollection<Compound> Compounds { get; set; } = new List<Compound>();

    }
}
