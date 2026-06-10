using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Production.Layout
{
    public class FactoryZone
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(5)]
        public string DivisionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } = 0; // Level in the hierarchy, e.g., 0 for top-level zones, 1 for sub-zones, etc.
        public string? ParentZoneId { get; set; } // Nullable for top-level zones
        public int? WorkCentreID { get; set; }
        public string? EquipmentID { get; set; }
        public string? EquipmentCapabilityID { get; set; }
        [MaxLength(50)]
        public string? Description { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsPlotted { get; set; } = false;
        [Required]
        [DefaultValue(0.0)]
        public double TargetMinUnitsPerDay { get; set; } = 0.0;
        [Required]
        [DefaultValue(0.0)]
        public double TargetMaxUnitsPerDay { get; set; } = 0.0;
        public double X { get; set; } = 5;
        public double Y { get; set; } = 5;
        public double Width { get; set; } = 150;
        public double Height { get; set; } = 100;
        public string Color { get; set; } = "#3b82f6"; // Fluent blue by default
        public string TextColor { get; set; } = "#ffffff"; // Fluent blue by default
        public string? Notes { get; set; }
        public WorkCentre? WorkCentre { get; set; }
        public ICollection<ProductionPlanItem>? ProductionPlans { get; set; }
        public ICollection<EquipmentItem>? EquipmentItems { get; set; }

        //public EquipmentCapability? equipmentCapability { get; set; }
    }
}
