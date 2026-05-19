using System.Collections.Generic;

namespace Pulse.Models.Inventory
{
    public class InventoryType
    {
        public long IdInventoryType { get; set; }
        public string? Description { get; set; }
        public string? DefaultCostGL { get; set; }
        public bool KeepStock { get; set; }
        public int ValuationMethodID { get; set; }
        public int CountMethodID { get; set; }
        public string? DefaultVarianceGL { get; set; }
        public string? DefaultRevaluationGL { get; set; }
        public string? DefaultAccrualGL { get; set; }
        public int? InventoryTrackingGroupID { get; set; }

        /// <summary>
        /// Indicates if tracking is by group (from legacy booTrackByGroup).
        /// Adjust column name in the map if your DB column differs.
        /// </summary>
        public bool TrackByGroup { get; set; }
        public bool UsedByDivision { get; set; } = false;

        // Navigation properties
        public ICollection<InventoryGroup>? InventoryGroups { get; set; }
        public ICollection<InventoryItem>? InventoryItems { get; set; }
        public ICollection<InventoryTypeDivisionSetting>? DivisionSettings { get; set; }
    }
}