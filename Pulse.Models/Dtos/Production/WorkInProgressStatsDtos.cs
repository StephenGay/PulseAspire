using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Dtos.Production;

public record WorkCentreWipStatsDto
{
    public int WorkCentreID { get; init; }
    public string WorkCentreName { get; init; } = string.Empty;
    public int TotalWOCount { get; init; } = 0;
    public int PlannedTodayCount { get; init; } = 0;
    public int CompletedTodayCount { get; init; } = 0;
    public int InProgress { get; init; } = 0;
    public int StatusPercent => PlannedTodayCount > 0 ? (100 / PlannedTodayCount) * CompletedTodayCount : 0;

}
