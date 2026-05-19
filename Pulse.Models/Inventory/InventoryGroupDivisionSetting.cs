using Pulse.Models.Organizational;

namespace Pulse.Models.Inventory
{
    /// <summary>
    /// Allows per-division overrides of Inventory Group defaults (mainly GL accounts)
    /// </summary>
    public class InventoryGroupDivisionSetting
    {
        public long Id { get; set; }
        public long InventoryGroupID { get; set; }
        public string DivisionID { get; set; } = string.Empty;

        public string? SalesGL { get; set; }
        public string? CostGL { get; set; }

        // Navigation
        public InventoryGroup? InventoryGroup { get; set; }
        public Division? Division { get; set; }
    }
}