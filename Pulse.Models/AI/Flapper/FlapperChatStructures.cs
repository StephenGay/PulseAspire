using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Flapper;

public record ClarificationResponse(string Type, string Question);

public record FlapperChatRequest(string UserId, Guid ConversationId, string Message);

public class FlapperResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? RawContent { get; set; }
    public bool RequiresClarification { get; set; }
    public string? ClarificationQuestion { get; set; }
}
