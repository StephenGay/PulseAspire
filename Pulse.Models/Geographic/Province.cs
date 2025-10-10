using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Geographic
{
    public class Province
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int ProvinceID { get; set; }
        [Required]
        [MaxLength(200)]
        public required string ProvinceName { get; set; }
        [Required]
        public required int CountryID { get; set; }
        [Required]
        public required Country Country { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Region> Regions { get; set; } = new();
    }
}
