using Pulse.Models.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI
{
    public record PulseAiResponse
    {
        public string SessionId { get; init; } = string.Empty;
        public string Sql { get; init; } = string.Empty;
        public string ModelUsed { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Content { get; set; } = string.Empty;
        public string? GeneratedSql { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<AiQueryValidationError>? ValidationErrors { get; set; }
        public AiQueryExecutionDetails? ExecutionDetails { get; set; }
        public List<Dictionary<string, object>>? Data { get; set; }
    }

    public record TablesResponse
    {
        public string SessionId { get; init; } = string.Empty;
        public string Sql { get; init; } = string.Empty;
        public string ModelUsed { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Content { get; set; } = string.Empty;
        public string? GeneratedSql { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<AiQueryValidationError>? ValidationErrors { get; set; }
        public AiQueryExecutionDetails? ExecutionDetails { get; set; }
        public List<Dictionary<string, object>>? Data { get; set; }
    }
}
