using System.Text.Json.Serialization;

namespace Pulse.Models.AI.Tools;

public sealed record ChartConfig(
    [property: JsonPropertyName("chartType")] string ChartType = "Bar",
    [property: JsonPropertyName("title")] string Title = "",
    [property: JsonPropertyName("xAxisTitle")] string? XAxisTitle = null,
    [property: JsonPropertyName("yAxisTitle")] string? YAxisTitle = null,
    [property: JsonPropertyName("series")] List<ChartSeriesConfig>? Series = null,
    [property: JsonPropertyName("footerNote")] string? FooterNote = null
);

public sealed record ChartSeriesConfig(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("dataPoints")] List<ChartDataPoint> Data
);

public sealed record ChartDataPoint(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("value")] double Value
);