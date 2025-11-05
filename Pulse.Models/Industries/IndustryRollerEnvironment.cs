using Pulse.Models.Rollers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Industries
{
    public class IndustryRollerEnvironment
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IndustryRollerEnvironmentId { get; set; }
        [Required]
        public int IndustryId { get; set; }
        [Required]
        public int RollerTypeId { get; set; }
        public int? IndustryProcessId { get; set; }
        [MaxLength(255)]
        public string? EnvironmentConditions { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [Required]
        public Industry Industry { get; set; }
        [Required]
        public RollerType RollerType { get; set; }
        public IndustryProcess? IndustryProcess { get; set; }
        public ICollection<IndustryRecommendedCover>? IndustryRecommendedCovers { get; set; } = new HashSet<IndustryRecommendedCover>();

    }
}
