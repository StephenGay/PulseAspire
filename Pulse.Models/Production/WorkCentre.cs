using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class WorkCentre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WorkCentreId { get; set; }
        [Required]
        [MaxLength(50)]
        public string WorkCentreName { get; set; } = string.Empty;
        [Required]
        [MaxLength(5)]
        public string DivisionID { get; set; }
        [Required]
        public int BranchID { get; set; }
        public string? Description { get; set; }
        [Required]
        [DefaultValue(0)]
        public bool ApplyTargets { get; set; } = false;
        [Required]
        [DefaultValue(0.0)]
        public double TargetMinUnitsPerDay { get; set; } = 0.0;
        [Required]
        [DefaultValue(0.0)]
        public double TargetMaxUnitsPerDay { get; set; } = 0.0;
        [MaxLength(9)]
        
        public string? Colour { get; set; } = "#ffffff";
        public string? TextColour { get; set; } = "#000000";
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
        public ICollection<WorkCentreFunctions>? WorkCentreFunctions { get; set; }
    }
}
