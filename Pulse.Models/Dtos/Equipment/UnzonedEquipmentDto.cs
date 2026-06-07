using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Dtos.Equipment
{
    public record UnzonedEquipmentDto
    {
        public string id { get; init; } = string.Empty;          // EquipmentID
        public string Description { get; init; } = string.Empty;       // e.g. Equipment Name
        //public string ProductionStageID { get; init; } = string.Empty;

    }
}
