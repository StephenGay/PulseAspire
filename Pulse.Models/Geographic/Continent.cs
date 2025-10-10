using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Geographic
{
    public class Continent
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int ContinentID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string ContinentName { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Country> Countries { get; set; } = new();

    }
}
