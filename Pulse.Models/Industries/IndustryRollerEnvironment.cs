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
    /// <summary>
    /// Represents the relationship between a roller type and an industry process,
    /// including environment-specific conditions and recommended covers.
    /// Access to the Industry is via the IndustryProcess navigation property:
    /// <example>var industry = rollerEnvironment.IndustryProcess.Industry;</example>
    /// </summary>
    public class IndustryRollerEnvironment
    {
        [Key]
        public int IndustryRollerEnvironmentId { get; set; }

        [Required]
        public int RollerTypeId { get; set; }

        [Required]
        public int IndustryProcessId { get; set; }

        [MaxLength(255)]
        public string? EnvironmentConditions { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [Required]
        public RollerType RollerType { get; set; }

        [Required]
        public IndustryProcess IndustryProcess { get; set; }

        public ICollection<IndustryRecommendedCover>? IndustryRecommendedCovers { get; set; } = new HashSet<IndustryRecommendedCover>();
    }
}
