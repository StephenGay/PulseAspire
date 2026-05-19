using Pulse.Models.Api;
using Pulse.Models.CustomComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Tables;

public class TablesChatStructures

{
    public class TablesRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? SessionId { get; set; }
        public required string UserId { get; set; }
        public required string UserEmail { get; set; }
        public required string ModelName { get; set; } = "Default";
        public List<OllamaChatMessage>? ConversationHistory { get; set; }
        public string UserRequest { get; set; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public OllamaOptions? AiOptions { get; set; }
    }
    public record TablesResponse
    {
        public string SessionId { get; init; } = string.Empty;
        public string Sql { get; init; } = string.Empty;
        public int AttemptsUsed { get; set; } = 1;
        public string ModelUsed { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Content { get; set; } = string.Empty;
        public string? GeneratedSql { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<AiQueryValidationError>? ValidationErrors { get; set; }
        public AiQueryExecutionDetails? ExecutionDetails { get; set; }
        public List<Dictionary<string, object>>? Data { get; set; }
    }

    public record TablesProgressUpdate(
        string SessionId,
        string Status,                    // Received, Generating, Validating, SelfCorrecting, Executing, Success, Failed
        string Message,
        int CurrentAttempt = 1,
        int MaxAttempts = 3,
        string? GeneratedSql = null,
        string? ErrorSummary = null,
        long? ExecutionTimeMs = null
    );
}
