namespace Pulse.Models.AI.Tools;

public sealed record ChartConfig(
    string ChartType = "Bar",                    // "Bar", "Line", "Pie", "Doughnut", "Area", "StackedBar"
    string Title = "",
    string? XAxisTitle = null,
    string? YAxisTitle = null,
    List<ChartSeriesConfig>? Series = null,
    string? FooterNote = null
);

public sealed record ChartSeriesConfig(
    string Name,
    List<ChartDataPoint> Data
);

public sealed record ChartDataPoint(string Label, double Value);