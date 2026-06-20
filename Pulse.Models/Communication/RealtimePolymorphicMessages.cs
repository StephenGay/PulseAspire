using Pulse.Models.Dtos.Production;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Pulse.Models.Communication;

/// <summary>
/// Base class for all real-time messages sent via SignalR.
/// Uses polymorphic JSON serialization.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(UserMessage), typeDiscriminator: "user")]
[JsonDerivedType(typeof(AIStreamChunk), typeDiscriminator: "ai_chunk")]
[JsonDerivedType(typeof(AIResponse), typeDiscriminator: "ai_response")]
[JsonDerivedType(typeof(SystemNotification), typeDiscriminator: "notification")]
[JsonDerivedType(typeof(ProductionUpdate), typeDiscriminator: "production")]
public abstract record RealtimePolymorphicMessage(string MessageType);

#region Conversation / AI Messages

public record UserMessage(
    string ConversationId,
    string FromUserId,
    string Content,
    DateTime Timestamp
) : RealtimePolymorphicMessage("user");

public record AIStreamChunk(
    string ConversationId,
    string Chunk,
    bool IsFinal,
    DateTime Timestamp
) : RealtimePolymorphicMessage("ai_chunk");

public record AIResponse(
    string ConversationId,
    string Content,
    DateTime Timestamp
) : RealtimePolymorphicMessage("ai_response");

#endregion

#region Notifications

public record SystemNotification(
    string Title,
    string Message,
    string Severity,           // "Info", "Warning", "Error", "Success"
    string? ActionUrl = null,
    DateTime Timestamp = default
) : RealtimePolymorphicMessage("notification");

#endregion

#region Live Production Data

public record ProductionUpdate(
    string LineId,
    string MachineId,
    string Status,
    int CurrentCount,
    DateTime Timestamp
) : RealtimePolymorphicMessage("production");

//public record ProductionStreamMessageDto
//{
//    public required string ProductionGroupName { get; init; }
//    public ProductionStreamType StreamType { get; init; }
//    public int? WorkOrderNo { get; init; }
//    public int? StepNo { get; init; }
//    public string? Status { get; init; }
//    public DateTime MsgTimestamp { get; init; } = DateTime.Now;
//    public DateTime? MsgActionDateTime { get; init; }
//    public string UserName { get; init; } = string.Empty;
//    public string? Title { get; init; }
//    public string? Result { get; init; }
//    public string? ProductionStageName { get; init; }
//    public string? Message { get; init; }
//}

#endregion

