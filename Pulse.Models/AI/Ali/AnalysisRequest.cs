// Pulse.Models/AI/Ali/AnalysisRequest.cs
using System.Text.Json;

namespace Pulse.Models.AI.Ali;

public class AnalysisRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    
    public string ApplicationUserId { get; set; } = string.Empty;
    public int ContextualAreaID { get; set; }

    /// <summary>
    /// The actual value to look up
    /// e.g. "1-COL201"
    /// </summary>
    public string KeyValue { get; set; } = string.Empty;
    public string AnalysisTitle { get; set; } = string.Empty; // Optional user-friendly title for the analysis (e.g. "Sales Analysis for Client 1-COL201")

    public string UserQuery { get; set; }

    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Queued";           // Queued | Processing | Completed | Failed
    public string? ResultSummary { get; set; }                // What Ali returns (or link to a separate Results table)
    public string? ErrorMessage { get; set; }

    // Optional: store the full JSON of the loaded object after retrieval (handy for debugging Ali)
    public string? LoadedObjectJson { get; set; }

    public int? CurrentTurn { get; set; } = 0;                    // 0 = initial, 1+, clarification turns
    public string? LastClarificationQuestion { get; set; }        // Question Ali asked
    public string? LastUserClarificationAnswer { get; set; }      // User's Yes/No or short answer
    public string? FullConversationHistory { get; set; }
}