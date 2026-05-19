using System.Collections.Generic;

namespace Pulse.Models.Inventory
{
    public class InventoryGroup
    {
        public long IdInventoryGroup { get; set; }
        public string? Description { get; set; }
        public string? SalesGL { get; set; }
        public string? CostGL { get; set; }
        public long InventoryTypeID { get; set; }

        // Navigation properties
        public InventoryType? InventoryType { get; set; }
        public ICollection<InventoryItem>? InventoryItems { get; set; }
        public ICollection<InventoryGroupDivisionSetting>? DivisionSettings { get; set; }
    }
}