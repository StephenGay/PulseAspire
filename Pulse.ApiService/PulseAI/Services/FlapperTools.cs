using Microsoft.Extensions.AI;
using Pulse.Models.AI;
using Pulse.Models.AI.Tools;

namespace Pulse.ApiService.PulseAI.Services;   // or adjust namespace to match your structure

public static class FlapperTools
{
    public static readonly IEnumerable<AITool> GetFlappersTools = new List<AITool>
    {
        AskTables,
        CreateChartTool,
        WebSearch,
        WebFetch
    };

    private static readonly AIFunction CreateChartTool = AIFunctionFactory.Create(
        (ChartConfig config) =>
        {
            // Optional: add validation or logging here
            return config;
        },
        name: "CreateChart",
        description: """
            Creates a visual chart for the user. 
            Use this tool whenever the user asks for a chart, graph, visualization, trend, comparison, or "show me" data.
            Always use real data (call the Tables tool first if you need to query the database).
            """);

    private static readonly AIFunction WebSearch = AIFunctionFactory.Create(
                name: "WebSearch",
                description: "Search the internet for current information not available in our database.",
                method: (string query) => Task.FromResult("")
            );

    private static readonly AIFunction AskTables = AIFunctionFactory.Create(
                name: "AskTables",
                description: "Use this tool when you need real data from the Pulse database. Provide a clear natural language description of the data required.",
                method: (string query) => Task.FromResult("")
            );

    private static readonly AIFunction WebFetch = AIFunctionFactory.Create(
                name: "WebFetch",
                description: "Fetch the full content of a specific webpage.",
                method: (string url) => Task.FromResult("")
            );
}