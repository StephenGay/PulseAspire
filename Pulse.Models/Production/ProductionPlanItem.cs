using Pulse.Models.Production.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class ProductionPlanItem
    {
        [Key]
        public int ProductionPlanItemID { get; set; }
        [Required]
        public int WorkOrderNo { get; set; }
        [Required]
        [MaxLength(5)]
        public string DivisionID { get; set; }
        [Required]
        public int StepNo { get; set; }
        [Required]
        public int ProductionStageID { get; set; }
        [MaxLength(10)]
        public string? EquipmentItemID { get; set; }
        public Guid? ZoneId { get; set; }
        public int? WorkCentreID { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public DateTime? PlannedEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public string? ClosedByUserID { get; set; }
        [Required, MaxLength(15)]
        [DefaultValue("Unplanned")]
        public string Status { get; set; } = "Unplanned";
        [Required]
        [DefaultValue(false)]
        public bool IsPulsePlan { get; set; } = false;

        public WorksOrder? WorksOrder { get; set; }
        public ProductionStage? ProductionStage { get; set; }
        public EquipmentItem? EquipmentItem { get; set; }
        public WorkCentre? WorkCentre { get; set; }
        public FactoryZone? FactoryZone { get; set; }
    }
}
