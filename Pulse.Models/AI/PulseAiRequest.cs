using Pulse.Models.CustomComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI
{
    public class PulseAiRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? SessionId { get; set; }
        public required string UserId { get; set; }
        public required string UserEmail { get; set; }
        public required string ModelName { get; set; }
        public required AiName AiName { get; set; }
        public List<OllamaChatMessage>? ConversationHistory { get; set; }
        public OllamaChatMessage UserRequest { get; set; } = new OllamaChatMessage();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public OllamaOptions? AiOptions { get; set; }
    }
}
public enum AiName
{
    Ali,
    Flapper,
    Tables
}
