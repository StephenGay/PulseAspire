using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
    public record CalendarEvent
    {
        public int Id { get; init; }          // WorksOrderNo or any unique key
        public string Title { get; init; } = string.Empty;       // e.g. Client + Order
        public DateTime Start { get; init; }
        public DateTime? End { get; init; }
        public string? BackgroundColor { get; init; }
        //public string? Url { get; init; }                       // optional link to detail page
    }

    public record ProductionPlanEvent
    {
        public int Id { get; init; }          // WorksOrderNo or any unique key
        public string Title { get; init; } = string.Empty;       // e.g. Client + Order
        public DateTime? Start { get; init; }
        public DateTime? End { get; init; }
        public string? ResourceId { get; init; }      // EquipmentID
        public string? BackgroundColor { get; init; }
        public string? ClientName { get; init; } = string.Empty;
        public string? Description { get; init; } = string.Empty;
        public string? WorkType { get; init; } = string.Empty;
        public string? Stage { get; init; } = string.Empty;
        public string[]? ClassNames { get; init; } = [string.Empty];

        //public string? Url { get; init; }                       // optional link to detail page
    }

    public record ProductionUnPlannedEvent
    {
        public int Id { get; init; }          // WorksOrderNo or any unique key
        public string Title { get; init; } = string.Empty;       // e.g. Client + Order
        public string? BackgroundColor { get; init; }
        public string? ClientName { get; init; } = string.Empty;
        public string? Description { get; init; } = string.Empty;
        public string? WorkType { get; init; } = string.Empty;
        public string? Stage { get; init; } = string.Empty;
        //public string? Url { get; init; }                       // optional link to detail page
    }
    public record ProductionPlanResource
    {
        public string id { get; init; } = string.Empty;          // EquipmentID
        public string title { get; init; } = string.Empty;       // e.g. Equipment Name

    }
}
