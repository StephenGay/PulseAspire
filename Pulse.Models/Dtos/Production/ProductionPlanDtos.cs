using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Dtos.Production;

public record ProductionPlanDto
{
    public int ProductionPlanItemID { get; init; }
    public int WorkOrderNo { get; init; }
    public string DivisionID { get; init; }
    public string? ClientName { get; init; } = null;
    public string? WODescription { get; init; } = null;
    public int StepNo { get; init; }
    public int ProductionStageID { get; init; }
    public string? ProductionStageDescription { get; init; } = null;
    public string? EquipmentItemID { get; init; }
    public string? EquipmentItemDescription { get; init; } = null;
    public Guid? ZoneId { get; init; }
    public string? FactoryZoneName { get; init; } = null;
    public int? WorkCentreID { get; init; }
    public string? WorkCentreName { get; init; } = null;
    public DateTime? PlannedStartTime { get; init; }
    public DateTime? PlannedEndTime { get; init; }
    public DateTime? ActualStartTime { get; init; }
    public DateTime? ActualEndTime { get; init; }
    public string? ClosedByUserID { get; init; }
    public string? Status { get; init; }
    public bool IsPulsePlan { get; init; } 

}

public record ProductionPlanResourceDto
{
    public string id { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string? parentId { get; init; } = string.Empty;

}