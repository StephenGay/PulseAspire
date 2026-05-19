using Pulse.Models.Organizational;

namespace Pulse.Models.Inventory
{
    /// <summary>
    /// Allows per-division overrides of Inventory Type defaults
    /// (Count Method, Valuation Method, GL accounts, etc.)
    /// </summary>
    public class InventoryTypeDivisionSetting
    {
        public long Id { get; set; }                    // Surrogate key
        public long InventoryTypeID { get; set; }
        public string DivisionID { get; set; } = string.Empty;

        // Overridable fields (null = use base value from InventoryType)
        public int? ValuationMethodID { get; set; }
        public int? CountMethodID { get; set; }
        public bool? KeepStock { get; set; }
        public bool UsedByDivision { get; set; } = true;
        /// <summary>
        public bool? TrackByGroup { get; set; }
        public int? InventoryTrackingGroupID { get; set; }
        public string? DefaultCostGL { get; set; }
        public string? DefaultVarianceGL { get; set; }
        public string? DefaultRevaluationGL { get; set; }
        public string? DefaultAccrualGL { get; set; }

        // Navigation
        public InventoryType? InventoryType { get; set; }
        public Division? Division { get; set; }
    }
}