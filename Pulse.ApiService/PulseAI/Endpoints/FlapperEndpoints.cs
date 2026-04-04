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
                .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

            if (conversation == null)
            {
                conversation = new FlapperConversation
                {
                    Id = req.ConversationId,
                    UserId = req.UserId,
                    Title = "Flapper Chat"
                };
                db.FlapperConversations.Add(conversation);
            }

            // Save user message
            var userMessage = new FlapperMessage
            {
                ConversationId = conversation.Id,
                Sender = "User",
                Content = req.Message,
                SentAt = DateTime.UtcNow
            };

            db.FlapperMessages.Add(userMessage);
            await db.SaveChangesAsync();

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

            db.FlapperMessages.Add(flapperMsg);
            await db.SaveChangesAsync();

            // Send via SignalR (real-time)
            var pulseMsg = new PulseMessage
            {
                SenderUserName = "Flapper",
                RecipientUserId = req.UserId,
                Role = "Flapper",
                Subject = result.RequiresClarification ? "Clarification Needed" : "Flapper Reply",
                ContentType = "HTML",
                Content = result.RequiresClarification
                    ? $"<p><strong>Flapper asks:</strong> {result.ClarificationQuestion}</p><p>Please answer with Yes or No.</p>"
                    : result.Content!,
                SentAt = DateTime.UtcNow
            };

            await hubContext.Clients.User(req.UserId).SendAsync("ReceiveMessage", pulseMsg);

            return Results.Ok(new { requiresClarification = result.RequiresClarification });
        });
        //.RequireAuthorization();
    }
}

// Simple request DTO
