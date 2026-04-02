using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Flapper;

// Pulse.Models/AI/Flapper/FlapperMessage.cs
public class FlapperMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string Sender { get; set; } = string.Empty;        // "User" or "Flapper"
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "Text";         // Text | HTML | Clarification
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Clarification specific
    public bool IsClarificationQuestion { get; set; } = false;
    public string? ClarificationType { get; set; } = "YesNo"; // YesNo | ShortAnswer
}
