using Pulse.Models.Dtos.Production;
using Pulse.Models.Production;
using Pulse.Models.Production.Layout;

namespace Pulse.Web.Services;

public class ProductionStateService
{
    public List<FactoryZone> Zones { get; private set; } = new();
    public List<ProductionPlanDto> ProductionPlanItems { get; private set; } = new(); // Add your model
    public Guid? SelectedZoneId { get; set; }

    public event Action? OnProductionPlanChange;

    public void UpdateZones(List<FactoryZone> zones)
    {
        Zones = zones ?? new();
        RecalculateZoneMetrics();
        NotifyProductionPlanChanged();
    }

    public void UpdateProductionPlanItems(List<ProductionPlanDto> items)
    {
        ProductionPlanItems = items ?? new();
        RecalculateZoneMetrics();
        NotifyProductionPlanChanged();
    }

    public void SelectZone(Guid? zoneId)
    {
        SelectedZoneId = zoneId;
        NotifyProductionPlanChanged();
    }

    public void ClearSelection()
    {
        SelectedZoneId = null;
        NotifyProductionPlanChanged();
    }

    private void RecalculateZoneMetrics()
    {
        if (Zones.Count == 0 || ProductionPlanItems.Count == 0)
            return;

        // Create a fast lookup for performance
        var planLookupByZoneId = ProductionPlanItems
            .Where(p => p.ZoneId.HasValue)
            .ToLookup(p => p.ZoneId!.Value);

        var planLookupByWorkCentre = ProductionPlanItems
            .Where(p => p.WorkCentreID.HasValue)
            .ToLookup(p => p.WorkCentreID!.Value);

        var planLookupByEquipment = ProductionPlanItems
            .Where(p => !string.IsNullOrEmpty(p.EquipmentItemID))
            .ToLookup(p => p.EquipmentItemID!);

        foreach (var zone in Zones)
        {
            // Collect plans matching on any of the three keys
            var matchingPlans = new List<ProductionPlanDto>();

            // Match by ZoneId (Guid)
            if (zone.Id != Guid.Empty)
                matchingPlans.AddRange(planLookupByZoneId[zone.Id]);

            // Match by WorkCentreID
            if (zone.WorkCentreID.HasValue)
                matchingPlans.AddRange(planLookupByWorkCentre[zone.WorkCentreID.Value]);

            // Match by EquipmentCapabilityID
            if (!string.IsNullOrEmpty(zone.EquipmentCapabilityID))
                matchingPlans.AddRange(planLookupByEquipment[zone.EquipmentCapabilityID]);

            // Remove duplicates (important if a plan matches on multiple keys)
            var distinctPlans = matchingPlans
                //.DistinctBy(p => p.ProductionPlanItemID)
                .ToList();

            // === Calculate metrics ===
            zone.WorkOrdersPlanned = distinctPlans.Count;

            // Completed = has ActualEndTime (based on your Progress logic)
            zone.WorkOrdersComplete = distinctPlans.Count(p => p.ActualEndTime != null);

            // Behind Schedule logic
            zone.IsBehindSchedule = distinctPlans.Any(p =>
                p.PlannedEndTime.HasValue &&
                p.PlannedEndTime < DateTime.Now &&
                p.ActualEndTime == null);
        }
    }

    public void NotifyProductionPlanChanged() => OnProductionPlanChange?.Invoke();
}
