using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Flapper;

public record ClarificationResponse(string type, string question);

public record FlapperChatRequest(
    string UserId, 
    Guid ConversationId, 
    string Message,
    string PreferredUserName = "Me",
    bool IsStreaming = true,
    int CompanyId = 0,
    string? DivisionId = null,
    string? CompanyInfo = null,
    string? FlapperSystemPrompt = null
    );

public class FlapperResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? RawContent { get; set; }
    public string? Thinking { get; set; }          // Chain-of-Thought
    public List<FlapperToolCall> ToolCalls { get; set; } = new();
    public bool RequiresClarification { get; set; }
    public string? ClarificationQuestion { get; set; }
}

public class FlapperToolCall
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}