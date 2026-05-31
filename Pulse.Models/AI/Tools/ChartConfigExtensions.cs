using System.Text.Json;
using Pulse.Models.AI.Tools;

public static class ChartConfigExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static string ToJson(this ChartConfig config)
        => JsonSerializer.Serialize(config, _options);

    public static ChartConfig? TryGetChartConfig(this string? rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ChartConfig>(rawContent, _options);
        }
        catch
        {
            return null;
        }
    }
}