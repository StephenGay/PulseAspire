using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.Models.AI.Flapper;
using Pulse.Models.Communication;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.PulseAI.Endpoints;

// Pulse.ApiService/PulseAI/Endpoints/FlapperEndpoints.cs  or in Program.cs
public static class FlapperEndpoints
{
    public static void MapFlapperEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/PulseAI/Flapper").WithTags("FlapperEndpoints");
        group.MapPost("/chat", async (
            FlapperChatRequest req,
            FlapperAPI flapper,
            PulseDbContext db,
            IHubContext<MessageHub> hubContext,
            PresenceService presence) =>
        {
            var conversation = await db.FlapperConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

            if (conversation == null)
                return Results.NotFound();

            conversation.LastActivity = DateTime.UtcNow;

            var result = await flapper.ProcessMessageAsync(conversation, req.Message);

            var flapperMsg = new FlapperMessage
            {
                ConversationId = conversation.Id,
                Sender = "Flapper",
                Content = result.RequiresClarification
                    ? result.ClarificationQuestion!
                    : result.Content!,
                ContentType = result.RequiresClarification ? "Clarification" : "HTML",
                IsClarificationQuestion = result.RequiresClarification
            };

            conversation.Messages.Add(flapperMsg);
            await db.SaveChangesAsync();

            // Send via SignalR (real-time)
            var pulseMsg = new PulseMessage
            {
                SenderUserName = "Flapper",
                RecipientUserId = req.UserId,
                Role = "PulseAI",
                Subject = result.RequiresClarification ? "Clarification Needed" : "Flapper Reply",
                ContentType = "HTML",
                Content = result.RequiresClarification
                    ? $"<p><strong>Flapper asks:</strong> {result.ClarificationQuestion}</p><p>Please answer with Yes or No.</p>"
                    : result.Content!,
                SentAt = DateTime.UtcNow
            };

            await hubContext.Clients.User(req.UserId).SendAsync("ReceiveMessage", pulseMsg);

            return Results.Ok(new { requiresClarification = result.RequiresClarification });
        })
        .RequireAuthorization();
    }
}

// Simple request DTO
public record FlapperChatRequest(string UserId, Guid ConversationId, string Message);