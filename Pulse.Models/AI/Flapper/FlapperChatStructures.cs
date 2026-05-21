using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Flapper;

public record ClarificationResponse(string type, string question);

public record FlapperChatRequest(
    FlapperDTO flapperDTO,
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
    public bool Done { get; set; } = false;
    public string EndReason { get; set; } = "unknown"; // "natural_stop", "max_tokens_reached", "context_window_exceeded"
    public string? Error { get; set; }
    public long PromptTokens { get; set; }
    public long OutputTokens { get; set; }
    public long TotalTokens => PromptTokens + OutputTokens;
}

public class FlapperToolCall
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class FlapperConversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "New Conversation";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active | Completed

    public List<FlapperMessage> Messages { get; set; } = new();

    protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlapperMessage>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);
    }
}

public class FlapperMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string Sender { get; set; } = string.Empty;        // "User" or "Flapper"
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "Text";         // Text | HTML | Clarification
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? ToolName { get; set; } = null;                 // If this message is a tool call, specify the tool name
    // Clarification specific
    public bool IsClarificationQuestion { get; set; } = false;
    public string? ClarificationType { get; set; } = "YesNo"; // YesNo | ShortAnswer
    public string? UserAnswer { get; set; } = null;

    // Navigation property
    public FlapperConversation Conversation { get; set; }
}

public class FlapperMessageDto
{
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsThinking { get; set; } = false;
}