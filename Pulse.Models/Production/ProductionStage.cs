using Pulse.Models.Organizational;
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
    public class ProductionStage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProductionStageId { get; set; }
        [Required]
        [MaxLength(50)]
        public string ProductionStageName { get; set; } = string.Empty;
        [Required]
        [MaxLength (5)]
        public string DivisionID { get; set; }
        [Required]
        public int WorkTypeID { get; set; }
        [Required]
        public int StepNo { get; set; }
        [DefaultValue(false)]
        public bool HasMaterial { get; set; }
        [DefaultValue(false)]
        public bool IsDefault { get; set; }
        [DefaultValue(false)]
        public bool IsOptional { get; set; }
        [MaxLength(200)]
        public string? ResultsPage {  get; set; }
        [DefaultValue(false)]
        public bool RequiresSignOff { get; set; }
        [DefaultValue(false)]
        public bool RequiresPlanning { get; set; }
        [DefaultValue(0)]
        public double ProcessPercentage { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [MaxLength (50)]
        public string? DifficultyMeasurement { get; set; } 
        [DefaultValue (0)]
        public decimal BaseValue { get; set; } = decimal.Zero;
        [DefaultValue(0)]
        public int BaseMinutesAtStage { get; set; } = 0;
        public Division? Division { get; set; }
        public WorkType? WorkType { get; set; }
        public ICollection<EquipmentCapability>? EquipmentCapabilities { get; set; }

    }
}
