using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Industries
{
    public class IndustryProcess
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int IndustryProcessID { get; set; }
        [Required]
        public required int IndustryID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string ProcessName { get; set; }
        public bool IsActive { get; set; } = true;
        public Industry? Industry { get; set; }


    }
}
