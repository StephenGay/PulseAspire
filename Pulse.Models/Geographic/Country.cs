using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Geographic
{
    public class Country
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CountryID { get; set; }
        [Required]
        [MaxLength(200)]
        public required string CountryName { get; set; }
        [Required]
        public required int ContinentID { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public required Continent Continent { get; set; }
        public ICollection<Province>? Provinces { get; set; } = new HashSet<Province>(); 

    }
}
