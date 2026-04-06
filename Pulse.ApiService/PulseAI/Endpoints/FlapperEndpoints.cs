//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Pulse.ApiService.Hubs;
//using Pulse.ApiService.PulseAI.Characters;
//using Pulse.Models.AI.Flapper;
//using Pulse.Models.Communication;
//using Pulse.Models.PulseContext;
//using System.Text;

//namespace Pulse.ApiService.PulseAI.Endpoints;

//// Pulse.ApiService/PulseAI/Endpoints/FlapperEndpoints.cs  or in Program.cs
//public static class FlapperEndpoints
//{
//    public static void MapFlapperEndpoints(this IEndpointRouteBuilder app)
//    {
//        var group = app.MapGroup("/PulseAI/Flapper").WithTags("FlapperEndpoints");

//        group.MapPost("/chat", async (
//            FlapperChatRequest req,
//            FlapperAPI flapper,
//            PulseDbContext db,
//            IHubContext<MessageHub> hubContext,
//            PresenceService presence) =>
//        {
//            var conversation = await db.FlapperConversations
//                .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

//            if (conversation == null)
//            {
//                conversation = new FlapperConversation
//                {
//                    Id = req.ConversationId,
//                    UserId = req.UserId,
//                    Title = "Flapper Chat"
//                };
//                db.FlapperConversations.Add(conversation);
//            }

//            // Save user message
//            var userMessage = new FlapperMessage
//            {
//                ConversationId = conversation.Id,
//                Sender = "User",
//                Content = req.Message,
//                SentAt = DateTime.UtcNow
//            };

//            db.FlapperMessages.Add(userMessage);
//            await db.SaveChangesAsync();

//            conversation.LastActivity = DateTime.UtcNow;

//            var result = await flapper.ProcessMessageAsync(conversation, req.Message);

//            var flapperMsg = new FlapperMessage
//            {
//                ConversationId = conversation.Id,
//                Sender = "Flapper",
//                Content = result.RequiresClarification
//                    ? result.ClarificationQuestion!
//                    : result.Content!,
//                ContentType = result.RequiresClarification ? "Clarification" : "HTML",
//                IsClarificationQuestion = result.RequiresClarification
//            };

//            db.FlapperMessages.Add(flapperMsg);
//            await db.SaveChangesAsync();

//            // Send via SignalR (real-time)
//            var pulseMsg = new PulseMessage
//            {
//                SenderUserName = "Flapper",
//                RecipientUserId = req.UserId,
//                Role = "Flapper",
//                Subject = result.RequiresClarification ? "Clarification Needed" : "Flapper Reply",
//                ContentType = "HTML",
//                Content = result.RequiresClarification
//                    ? result.ClarificationQuestion
//                    : result.Content!,
//                SentAt = DateTime.UtcNow
//            };

//            await hubContext.Clients.User(req.UserId).SendAsync("ReceiveMessage", pulseMsg);

//            return Results.Ok(new { requiresClarification = result.RequiresClarification });
//        });
//        //.RequireAuthorization();

//        // In FlapperEndpoints.cs or Program.cs
//        group.MapPost("/StreamChat", async (
//            FlapperChatRequest req,
//            FlapperAPI flapper,
//            PulseDbContext db,
//            IHubContext<MessageHub> hubContext) =>
//        {
//            var conversation = await db.FlapperConversations
//                .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

//            if (conversation == null)
//            {
//                conversation = new FlapperConversation
//                {
//                    Id = req.ConversationId,
//                    UserId = req.UserId,
//                    Title = "Flapper Chat"
//                };
//                db.FlapperConversations.Add(conversation);
//            }

//            var userMessage = new FlapperMessage
//            {
//                ConversationId = conversation.Id,
//                Sender = "User",
//                Content = req.Message,
//                SentAt = DateTime.UtcNow
//            };
//            db.FlapperMessages.Add(userMessage);

//            await db.SaveChangesAsync();

//            conversation.LastActivity = DateTime.UtcNow;

//            var fullResponseBuilder = new StringBuilder();
//            bool isClarification = false;
//            string clarificationQuestion = "";
//            string finalResponse = string.Empty;

//            await flapper.StreamResponseAsync(conversation, req.Message, async chunk =>
//            {

//                // Check for clarification marker
//                //if (chunk.StartsWith("[CLARIFICATION]"))
//                if (chunk.StartsWith("[") && string.IsNullOrEmpty(finalResponse))
//                {
//                    isClarification = true;
//                }

//                finalResponse += chunk;

//                if (!isClarification)
//                {
//                    var pulseMessage = new PulseMessage
//                    {
//                        SenderUserName = "Flapper",
//                        RecipientUserId = req.UserId,
//                        Role = "Flapper",
//                        Subject = "Flapper Reply",
//                        ContentType = "HTML",
//                        Content = chunk,
//                        SentAt = DateTime.UtcNow
//                    };
//                    await hubContext.Clients.User(req.UserId.ToString())
//                    .SendAsync("ReceiveFlapperChunk", pulseMessage);
//                }
//            });

//            if (isClarification)
//            {
//                finalResponse = finalResponse.Replace("[CLARIFICATION] [", "");
//                finalResponse = finalResponse.Substring(0, finalResponse.Length - 1);

//                var clarMessage = new PulseMessage
//                {
//                    SenderUserName = "Flapper",
//                    RecipientUserId = req.UserId,
//                    Role = "Flapper",
//                    Subject = "Clarification Needed",
//                    ContentType = "HTML",
//                    Content = finalResponse,
//                    SentAt = DateTime.UtcNow
//                };
//                await hubContext.Clients.User(req.UserId.ToString())
//                    .SendAsync("ReceiveMessage", clarMessage);
//            }

//            db.FlapperMessages.Add(new FlapperMessage
//            {
//                ConversationId = conversation.Id,
//                Sender = "Flapper",
//                Content = finalResponse,
//                SentAt = DateTime.UtcNow,
//                ContentType = isClarification ? "Clarification" : "HTML",
//                IsClarificationQuestion = isClarification,
//            });

//            await db.SaveChangesAsync();

//            return Results.Ok();
//        });
//        //.RequireAuthorization();
//    }
//}

//// Simple request DTO
