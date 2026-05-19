namespace Pulse.Models.Inventory
{
    public class InventoryItem
    {
        public string IdInventoryItem { get; set; } = string.Empty;
        public string? InventoryCode { get; set; }
        public string? Description { get; set; }
        public long? InventoryTypeID { get; set; }
        public long? InventoryGroupID { get; set; }
        public long? InventoryUnitID { get; set; }
        public string? SalesGL { get; set; }
        public string? CostGL { get; set; }

        // Navigation properties (matching legacy cls* references)
        public InventoryType? InventoryType { get; set; }
        public InventoryGroup? InventoryGroup { get; set; }

        // Add InventoryUnit navigation later when you create that entity
    }
}