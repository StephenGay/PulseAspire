using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public record WorkInProgressDto
    {
        public int WorksOrderNo { get; init; }
        public string DivisionID { get; init; }
        public string WorkTypeName { get; init; }
        public int? TargetWorkingDays { get; init; }
        public string ClientName { get; init; }
        public decimal? UndelQty { get; init; } // Assuming decimal for quantities
        public decimal? MaterialCost { get; init; }
        public decimal? SellPrice { get; init; }
        public int? ProductionStageID { get; init; }
        public string? ProductionStageName { get; init; }
        public DateTime? ProgressChange { get; init; } // Adjust type if it's not string
        public DateTime? RequiredDate { get; init; }
        public string? WorkCentreName { get; init; }
        public DateTime? DateStarted { get; init; }
        public string? Description { get; init; }
        public int? WorkCentreID { get; init; }
    }
}
