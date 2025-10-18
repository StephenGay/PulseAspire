using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Rollers
{
    public class ShellType
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int ShellTypeID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string ShellTypeName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
