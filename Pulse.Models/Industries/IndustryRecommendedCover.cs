using Pulse.Models.Compounds;
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
    public class IndustryRecommendedCover
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IndustryRecommendedCoverId { get; set; }
        [Required]
        [MaxLength(4)]
        public required string CompoundCode { get; set; }
        [Required]
        public required int IndustryRollerEnvironmentId { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        
        public Compound? Compound { get; set; }
        
        public IndustryRollerEnvironment? IndustryRollerEnvironment { get; set; }
    }
}
