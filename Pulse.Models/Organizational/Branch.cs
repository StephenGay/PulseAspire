using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Organizational
{
    public class Branch
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int BranchID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string BranchName { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Division> Divisions { get; set; } = new();

    }
}
