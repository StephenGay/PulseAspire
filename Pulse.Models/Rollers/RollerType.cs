using Pulse.Models.Industries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Rollers
{
    public class RollerType
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int RollerTypeID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string RollerTypeName { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<IndustryRollerEnvironment>? IndustryRollerEnvironments { get; set; } = new HashSet<IndustryRollerEnvironment>();
    }
}
