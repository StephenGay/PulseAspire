using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Flapper;

public class FlapperConversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "New Conversation";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active | Completed

    public List<FlapperMessage> Messages { get; set; } = new();
}

