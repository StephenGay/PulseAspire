using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using OllamaSharp;
using OllamaSharp.Tools;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.AI.Flapper;
using Pulse.Models.AI.Tools;
using Pulse.Models.Api;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Models.PulseContext;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static OllamaSharp.Models.Chat.Message;
using static Pulse.Models.AI.Tables.TablesChatStructures;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;

namespace Pulse.ApiService.PulseAI.Endpoints;

//Pulse.ApiService/PulseAI/Endpoints/FlapperEndpoints.cs or in Program.cs
public static class FlapperEndpoints
{
	public static void MapFlapperEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/PulseAI/Flapper").WithTags("FlapperEndpoints");

		group.MapPost("/ChatSession", FlapperChat)
			.WithName("FlapperChat")
			.WithDescription("Send a message to Flapper and receive a response. Supports streaming responses and clarification questions.")
			.Produces<FlapperResponse>(200);

		group.MapPost("/OllamaChatSession", FlapperIChat)
			.WithName("FlapperOllamaChat")
			.WithDescription("Send a message to Flapper and receive a response. Supports streaming responses and clarification questions.")
			.Produces<FlapperResponse>(200);

		group.MapGet("/Conversations/UserHistory/{userId}", GetUserConversationHistory)
			.WithName("GetUserConversationHistory")
			.WithDescription("Get the conversation history for a specific user.")
			.Produces<ApiResponse<List<FlapperConversation>>>(200);

		group.MapGet("/Conversations/GetMessages/{conversationId}", GetConversationMessages)
			.WithName("GetConversationMessages")
			.WithDescription("Get the messages for a specific conversation.")
			.Produces<ApiResponse<List<FlapperMessage>>>(200);

	}

	private static async Task<IResult> FlapperChat(
		FlapperChatRequest req,
		AiShared _aiShared,
		FlapperAPI flapperApi,
		TablesAPI tablesApi,           // Your SQL expert
		PulseDbContext db,
		IHubContext<MessageHub> hubContext,
		CancellationToken cancellationToken)
	{
		try
		{
			// 1. Load or create conversation
			var conversation = await db.FlapperConversations
				.FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

			if (conversation == null)
			{
				conversation = new FlapperConversation
				{
					Id = req.ConversationId,
					UserId = req.UserId,
					StartedAt = DateTime.Now,
					Title = req.Message.Length > 50 ? req.Message.Substring(0, 50) + "..." : req.Message
				};
				db.FlapperConversations.Add(conversation);
				await db.SaveChangesAsync();
			}

			// 2. Save user message
			db.FlapperMessages.Add(new FlapperMessage
			{
				ConversationId = conversation.Id,
				Sender = req.IsClarificationResponse ? "tool" : "User",
				ContentType = req.IsClarificationResponse ? "UserAnswer" : "UserQuery",
				Content = req.Message,
				SentAt = DateTime.Now
			});

			conversation.LastActivity = DateTime.Now;
			db.Update(conversation);
			await db.SaveChangesAsync();

			// 3. Build available tools
			var tools = BuildFlapperTools();

			// 4. Multi-turn reasoning loop with tool invocation
			int MaxTurns = req.flapperDTO.MaxToolRetries;
			int turn = 0;
			FlapperResponse result = new FlapperResponse { Success = true };
			bool isClarification = false;
			ChartConfig? finalChart = null;

			do
			{
				turn++;
				System.Diagnostics.Debug.WriteLine($"=== Tool Loop Turn {turn}/{MaxTurns} ===");
				cancellationToken.ThrowIfCancellationRequested();

				// Reload conversation messages
				conversation.Messages = await db.FlapperMessages
					.Where(c => c.ConversationId == conversation.Id)
					.OrderBy(m => m.SentAt)
					.ToListAsync(cancellationToken);

				// Stream the response and collect tool calls
				result = await ProcessWithToolsStreamingAsync(
					flapperApi,
					conversation,
					req.Message,
					req.flapperDTO,
					tools,
					hubContext,
					req.UserId,
					accumulatedContent => accumulatedContent,
					cancellationToken);

				System.Diagnostics.Debug.WriteLine($"Response content length: {result.Content?.Length ?? 0}");

				if (!string.IsNullOrEmpty(result.Thinking))
				{
					db.FlapperMessages.Add(new FlapperMessage
					{
						ConversationId = conversation.Id,
						Sender = "Flapper",
						Content = result.Thinking,
						ContentType = "AiThinking",
						SentAt = DateTime.Now,
					});
					await db.SaveChangesAsync();
				}

				// Check for tool calls in the response content using non-streaming approach
				//var toolCalls = ExtractToolCallsFromStreamingContent(result.Content ?? result.RawContent ?? "");

				if (result.ToolCalls?.Count > 0)
				{
					System.Diagnostics.Debug.WriteLine($"Found {result.ToolCalls.Count} tool calls in response");

					// Execute each tool call
					foreach (var toolCall in result.ToolCalls)
					{
						System.Diagnostics.Debug.WriteLine($"Executing tool: {toolCall.ToolName}");
						string toolResult;

						if (toolCall.ToolName == "CreateChart")
						{
							// Extract the chart config properly
							finalChart = result.ToolResults?
								.FirstOrDefault(r => r.ToolName == "CreateChart")?
								.Result as ChartConfig
								?? ExtractChartFromToolCall(toolCall);   // fallback

							toolResult = finalChart != null
								? $"Chart created successfully: {finalChart.Title}"
								: "Failed to create chart";
						}
						else
						{
							toolResult = toolCall.ToolName switch
							{
								"ask_tables" or "AskTables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message, conversation.Id.ToString()),
								"web_search" or "WebSearch" => await ExecuteWebSearchIClientAsync(toolCall.Parameters, flapperApi),
								"web_fetch" or "WebFetch" => await ExecuteWebFetchIClientAsync(toolCall.Parameters, flapperApi),
								_ => $"Unknown tool: {toolCall.ToolName}"
							};
						}

						// Save tool result back into conversation so Flapper can reason again
						db.FlapperMessages.Add(new FlapperMessage
						{
							ConversationId = conversation.Id,
							Sender = "tool",
							Content = toolResult,
							ContentType = toolCall.ToolName == "CreateChart" ? "Chart" : "ToolResult",
							RawContent = toolCall.ToolName == "CreateChart" ? JsonSerializer.Serialize(finalChart) : null,
							SentAt = DateTime.Now,
							ToolName = toolCall.ToolName
						});
					}

					await db.SaveChangesAsync();
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("No tool calls found in response, exiting loop");
					break;
				}

			} while (turn < MaxTurns);

			if (result.Content?.StartsWith("[CLARIFICATION] ") == true)
			{
				isClarification = true;
				result.RequiresClarification = true;
				result.ClarificationQuestion = result.Content.Replace("[CLARIFICATION] ", "").TrimEnd("]").ToString();
			}

			if (finalChart != null)
			{
				result.Chart = finalChart;
				System.Diagnostics.Debug.WriteLine($"Final chart attached: {finalChart.Title}");
			}
			// 6. Save Flapper's final response
			db.FlapperMessages.Add(new FlapperMessage
			{
				ConversationId = conversation.Id,
				Sender = "Flapper",
				Content = result.Content ?? "No response generated.",
				SentAt = DateTime.Now,
				ContentType = isClarification ? "AiQuestion" : "AiResponse",
				RawContent = finalChart != null ? JsonSerializer.Serialize(finalChart) : null,
				IsClarificationQuestion = isClarification
			});

			await db.SaveChangesAsync();

			// 7. Send final message to UI via your existing hub
			var finalMessage = new PulseMessage
			{
				SenderUserName = "Flapper",
				RecipientUserId = req.UserId,
				Role = "Flapper",
				Subject = isClarification ? "Clarification" : "Flapper Reply",
				ContentType = "HTML",
				Content = result.Content ?? result.Thinking ?? "",
				SentAt = DateTime.Now
			};

			//await hubContext.Clients.User(req.UserId.ToString())
			//	.SendAsync("ReceiveMessage", finalMessage);
			await hubContext.Clients.User(req.UserId.ToString())
				.SendAsync("FlapperChatResponse", result);

			return Results.Ok(result);
		}
		catch (Exception ex)
		{
			return Results.Ok(new FlapperResponse
			{
				Success = false,
				Content = $"An error occurred: {ex.Message}"
			});
		}
	}

	private static ChartConfig? ExtractChartFromToolCall(PulseToolCall toolCall)
	{
		if (toolCall.Parameters == null) return null;

		try
		{
			// If the model sent the chart config directly in parameters
			if (toolCall.Parameters.TryGetValue("config", out var configObj) && configObj is string json)
			{
				return System.Text.Json.JsonSerializer.Deserialize<ChartConfig>(json);
			}

			// Or try to deserialize the entire parameters as ChartConfig
			var jsonString = System.Text.Json.JsonSerializer.Serialize(toolCall.Parameters);
			return System.Text.Json.JsonSerializer.Deserialize<ChartConfig>(jsonString);
		}
		catch
		{
			return null;
		}
	}
	/// <summary>
	/// Processes chat with streaming support and automatic tool invocation.
	/// </summary>
	private static async Task<FlapperResponse> ProcessWithToolsStreamingAsync(
		FlapperAPI flapperApi,
		FlapperConversation conversation,
		string userMessage,
		FlapperDTO flapperDTO,
		IEnumerable<AITool> tools,
		IHubContext<MessageHub> hubContext,
		string userId,
		Func<string, string> onChunk,
		CancellationToken ct)
	{
		try
		{
			// Build messages
			var history = conversation.Messages.Select(m => new ChatMessage(
				role: m.Sender == "User" ? ChatRole.User : (m.Sender == "tool" ? ChatRole.Tool : ChatRole.Assistant),
				content: m.Content
			)).ToList();

			var messages = new List<ChatMessage>();
			messages.AddRange(history);
			//messages.Add(new ChatMessage(ChatRole.User, userMessage));

			var toolList = tools.ToList(); // FlapperTools.GetFlappersTools.ToList();
			var toolCalls = new List<PulseToolCall>();

			var options = new ChatOptions
			{
				ModelId = flapperDTO.Model,
				MaxOutputTokens = flapperDTO.Options.NumCtx,
				ConversationId = conversation.Id.ToString(),
				Tools = toolList // Pass the tools to the API for validation
			};

			var accumulatedText = new StringBuilder();
			var accumulatedThinking = string.Empty;
			var accumulatedAnswer = string.Empty;
			bool inThinking = false;
			var promptTokens = 0L;
			var outputTokens = 0L;
			string? doneReason = null;
			string? error = null;

			// ✅ Stream the response
			await foreach (ChatResponseUpdate update in flapperApi.GetChatClientStreamAsync(messages, options, ct))
			{
				var streamTxt = string.Empty;
				var FlapperChunk = new FlapperResponse();

				try
				{
					var ollama = ((OllamaSharp.Models.Chat.ChatResponseStream)update.RawRepresentation);



					if (!string.IsNullOrEmpty(ollama.Message?.Thinking))
					{
						var thinkingText = ollama.Message.Thinking;
						if (!inThinking)
						{
							inThinking = true;
							streamTxt = "<thinking>";
							thinkingText = $"Thinking:\n{thinkingText}";
						}
						streamTxt += thinkingText;
						FlapperChunk.Thinking = thinkingText;
						accumulatedThinking += ollama.Message?.Thinking;
					}
					if (!string.IsNullOrEmpty(ollama.Message?.Content))
					{
						if (inThinking)
						{
							inThinking = false;
							streamTxt += "</thinking>";
						}
						streamTxt += ollama.Message.Content;
						FlapperChunk.Content = ollama.Message.Content;
						accumulatedAnswer += ollama.Message.Content;
					}
					if (ollama?.Message?.ToolCalls != null && ollama.Message.ToolCalls.Any())
					{
						// For simplicity, we append tool calls as JSON strings in the thinking stream
						var toolInfo = JsonSerializer.Serialize(ollama.Message.ToolCalls);
						foreach (var tc in ollama.Message.ToolCalls)
						{
							toolCalls.Add(new PulseToolCall
							{
								ToolName = tc.Function.Name,
								Parameters = (Dictionary<string, object>)tc.Function.Arguments
							});
						}

					}
					FlapperChunk.Done = ollama.Done;
					FlapperChunk.EndReason = update.FinishReason?.ToString().ToLowerInvariant();
					FlapperChunk.PromptTokens = update.AdditionalProperties?.TryGetValue("prompt_eval_count", out var ptc) == true ? Convert.ToInt64(ptc) : 0;
					FlapperChunk.OutputTokens = update.AdditionalProperties?.TryGetValue("eval_count", out var etc) == true ? Convert.ToInt64(etc) : 0;

					if (ollama?.Done == true)
					{
						doneReason = update.FinishReason?.ToString().ToLowerInvariant();
					}
					if (update.AdditionalProperties?.TryGetValue("done_reason", out var dr) == true)
					{
						doneReason = dr?.ToString()?.ToLowerInvariant();
					}

					if (update.AdditionalProperties?.TryGetValue("prompt_eval_count", out var pt) == true)
						promptTokens = Convert.ToInt64(pt);

					if (update.AdditionalProperties?.TryGetValue("eval_count", out var et) == true)
						outputTokens = Convert.ToInt64(et);

				}
				catch (Exception ex)
				{
					if (ex.Message.Contains("context", StringComparison.OrdinalIgnoreCase))
					{
						doneReason = "context";
					}
					// If RawRepresentation access fails, try the standard text
					streamTxt = update.Text ?? string.Empty;
				}

				accumulatedText.Append(streamTxt);

				// Stream chunk to UI in real-time
				if (!string.IsNullOrEmpty(streamTxt))
				{
					var streamMessage = new PulseMessage
					{
						SenderUserName = "Flapper",
						RecipientUserId = userId.ToString(),
						Role = "Flapper",
						Subject = "Flapper is thinking...",
						ContentType = "HTML",
						Content = streamTxt,
						SentAt = DateTime.Now
					};

					await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveFlapperResponseChunk", FlapperChunk);
					//await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveFlapperChunk", streamMessage);
				}
			}

			var rawReply = accumulatedText.ToString().Trim();
			var thinking = accumulatedThinking;
			var finalContent = accumulatedAnswer;

			var endReason = doneReason switch
			{
				"length" => "max_tokens_reached",
				"stop" or "eos" => "natural_stop",
				"context" => "context_length_exceeded",
				_ => doneReason ?? "unknown"
			};

			return new FlapperResponse
			{
				Success = true,
				Thinking = string.IsNullOrWhiteSpace(thinking) ? null : thinking,
				ToolCalls = toolCalls.Count > 0 ? toolCalls : null,
				Content = finalContent,
				RequiresClarification = false,
				EndReason = endReason,
				PromptTokens = promptTokens,
				OutputTokens = outputTokens
				//RawContent = rawReply
			};
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error in ProcessWithToolsStreamingAsync: {ex.Message}");

			return new FlapperResponse
			{
				Success = false,
				Content = $"Error: {ex.Message}"
			};
		}
	}

	/// <summary>
	/// Extracts tool calls from response content (handles multiple formats).
	/// </summary>
	private static List<PulseToolCall> ExtractToolCallsFromStreamingContent(string content)
	{
		var toolCalls = new List<PulseToolCall>();
		if (string.IsNullOrEmpty(content)) return toolCalls;

		System.Diagnostics.Debug.WriteLine($"Extracting tool calls from content of length: {content.Length}");

		// Pattern 1: [TOOL_CALL]...[/TOOL_CALL] format
		var toolCallBlockPattern = @"\[TOOL_CALL\](.*?)\[/TOOL_CALL\]";
		var matches1 = System.Text.RegularExpressions.Regex.Matches(content, toolCallBlockPattern, System.Text.RegularExpressions.RegexOptions.Singleline);

		foreach (System.Text.RegularExpressions.Match match in matches1)
		{
			var jsonStr = match.Groups[1].Value.Trim();
			try
			{
				using (var doc = JsonDocument.Parse(jsonStr))
				{
					var root = doc.RootElement;
					if (root.TryGetProperty("tool_name", out var toolNameElem) && root.TryGetProperty("parameters", out var paramsElem))
					{
						var toolName = toolNameElem.GetString();
						var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(paramsElem.GetRawText())
							?? new Dictionary<string, object>();

						if (!string.IsNullOrEmpty(toolName))
						{
							toolCalls.Add(new PulseToolCall
							{
								ToolName = toolName,
								Parameters = parameters
							});
							System.Diagnostics.Debug.WriteLine($"Extracted tool: {toolName}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to parse tool (format 1): {ex.Message}");
			}
		}

		// Pattern 2: Standard JSON with tool_name
		var toolCallPattern2 = @"\{\s*""tool_name"":\s*""([^""]+)""\s*,\s*""parameters"":\s*(\{[^}]*\})";
		var matches2 = System.Text.RegularExpressions.Regex.Matches(content, toolCallPattern2);

		foreach (System.Text.RegularExpressions.Match match in matches2)
		{
			var toolName = match.Groups[1].Value;
			var parametersJson = match.Groups[2].Value;

			try
			{
				var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson)
					?? new Dictionary<string, object>();

				toolCalls.Add(new PulseToolCall
				{
					ToolName = toolName,
					Parameters = parameters
				});
				System.Diagnostics.Debug.WriteLine($"Extracted tool: {toolName}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to parse tool (format 2): {ex.Message}");
			}
		}

		// Pattern 3: Ollama format with 'name'
		var toolCallPattern3 = @"""name"":\s*""([^""]+)""\s*,\s*""parameters"":\s*(\{[^}]*\})";
		var matches3 = System.Text.RegularExpressions.Regex.Matches(content, toolCallPattern3);

		foreach (System.Text.RegularExpressions.Match match in matches3)
		{
			var toolName = match.Groups[1].Value;
			var parametersJson = match.Groups[2].Value;

			try
			{
				var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson)
					?? new Dictionary<string, object>();

				toolCalls.Add(new PulseToolCall
				{
					ToolName = toolName,
					Parameters = parameters
				});
				System.Diagnostics.Debug.WriteLine($"Extracted tool: {toolName}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to parse tool (format 3): {ex.Message}");
			}
		}

		return toolCalls;
	}

	/// <summary>
	/// Helper method to extract reasoning from text (for backward compatibility).
	/// </summary>
	private static string ExtractReasoningFromText(string text)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		var startIndex = text.IndexOf("<thinking>", StringComparison.Ordinal);
		if (startIndex == -1)
			return string.Empty;

		startIndex += "<thinking>".Length;
		var endIndex = text.IndexOf("</thinking>", startIndex, StringComparison.Ordinal);
		if (endIndex == -1)
			return string.Empty;

		return text.Substring(startIndex, endIndex - startIndex).Trim();
	}

	private static async Task<IResult> GetConversationMessages(Guid conversationId, PulseDbContext db)
	{
		var messages = await db.FlapperMessages
			.Where(m => m.ConversationId == conversationId && m.Sender != "tool")
			.OrderBy(m => m.SentAt)
			.ToListAsync();

		//var messageDtos = messages.Select(m => new FlapperMessageDto
		//{
		//	Sender = m.Sender,
		//	Content = m.Content,
		//	IsThinking = m.Content.StartsWith("<thinking>") ? true : false,
		//}).ToList();

		return Results.Ok(new ApiResponse<List<FlapperMessage>>(messages));
	}

	// Place this in your FlapperEndpoints.cs or as a static helper method
	// Add this method in FlapperEndpoints.cs or a static helper
	private static IEnumerable<AIFunction> BuildFlapperTools()
	{
		return new List<AIFunction>
		{
			AIFunctionFactory.Create(
				name: "AskTables",
				description: "Use this tool when you need real data from the Pulse database. Provide a clear natural language description of the data required.",
				method: (string query) => Task.FromResult("")
			),

			AIFunctionFactory.Create(
				name: "WebSearch",
				description: "Search the internet for current information not available in our database.",
				method: (string query) => Task.FromResult("")
			),

			AIFunctionFactory.Create(
				name: "WebFetch",
				description: "Fetch the full content of a specific webpage.",
				method: (string url) => Task.FromResult("")
			),
			AIFunctionFactory.Create(
				(string chartType,
	 string title,
	 string xAxisTitle = "",           // ← Changed from string? to string with default
     string yAxisTitle = "",           // ← Changed from string? to string with default
     string dataJson = "[]",
	 string footerNote = "") =>
	{
		try
		{
			var dataPoints = System.Text.Json.JsonSerializer
				.Deserialize<List<ChartDataPoint>>(dataJson) ?? new();

			return new ChartConfig(
				ChartType: chartType,
				Title: title,
				XAxisTitle: string.IsNullOrWhiteSpace(xAxisTitle) ? null : xAxisTitle,
				YAxisTitle: string.IsNullOrWhiteSpace(yAxisTitle) ? null : yAxisTitle,
				Series: new List<ChartSeriesConfig>
				{
					new ChartSeriesConfig("Series", dataPoints)
				},
				FooterNote: string.IsNullOrWhiteSpace(footerNote) ? null : footerNote
			);
		}
		catch (Exception ex)
		{
			return new ChartConfig("Bar", "Chart Error", null, null, null,
				$"Failed to parse data: {ex.Message}");
		}
	},
	name: "CreateChart",
	description: """
        Creates a chart. 
        Parameters:
        - chartType: "Bar", "Line", "Pie", "Doughnut", or "Area"
        - title: Short descriptive title
        - xAxisTitle, yAxisTitle: Axis labels (optional)
        - dataJson: JSON array of objects with "label" and "value", e.g. [{"label":"North","value":125000}]
        - footerNote: One sentence insight (optional)
        """)
		};

	}

	public static async Task<IResult> FlapperOllamaChat(
		FlapperChatRequest req,
		AiShared _aiShared,
		FlapperOllamaAPI flapperOllamaApi,
		TablesAPI tablesApi,           // Your SQL expert
		PulseDbContext db,
		IHubContext<MessageHub> hubContext,
	CancellationToken cancellationToken)
	{
		try
		{


			// 1. Load or create conversation
			var conversation = db.FlapperConversations
				.Where(c => c.Id == req.ConversationId && c.UserId == req.UserId)
				.FirstOrDefault();

			if (conversation == null)
			{
				conversation = new FlapperConversation
				{
					Id = req.ConversationId,
					UserId = req.UserId,
					StartedAt = DateTime.Now,
					Title = req.Message.Length > 50 ? req.Message.Substring(0, 50) + "..." : req.Message
				};
				db.FlapperConversations.Add(conversation);
			}

			// 2. Save user message
			db.FlapperMessages.Add(new FlapperMessage
			{
				ConversationId = conversation.Id,
				Sender = "User",
				Content = req.Message,
				SentAt = DateTime.Now
			});
			await db.SaveChangesAsync();

			conversation.LastActivity = DateTime.Now;

			// 3. Build rich system prompt (with schema)
			var systemMessageData = new SystemMessageData
			{
				CompanyName = "H&M Rollers",
				CompanyInformation = AiPromptHelperService.GetTempCoInfo(),
				UserName = req.PreferredUserName,
				DbSchema = await _aiShared.GetDetailedSchemaAsync(),
				UserQuery = req.Message
				// Add any other fields your BuildSystemMessagev2 needs
			};

			Flapper flapper = new Flapper();
			string? systemPrompt;
			var uMsg = req.Message;

			//if (req.flapperDTO.SpokenResponses)
			//{
			//    //systemPrompt = flapper.BuildSystemMessagev3(systemMessageData);
			//    uMsg = $"[SPEECH] {uMsg}";
			//}
			//else
			//{
			//    //systemPrompt = flapper.BuildSystemMessagev2(systemMessageData);
			//    uMsg = $"[REPORT] {uMsg}";

			//}

			// 4. Build available tools
			var tools = flapperOllamaApi.BuildFlapperOllamaTools();

			// 5. Multi-turn reasoning loop (Flapper can call tools multiple times)
			int MaxTurns = req.flapperDTO.MaxToolRetries;
			int turn = 0;
			FlapperResponse result;
			bool isClarification = false;
			string curMsg = string.Empty;
			do
			{
				turn++;
				cancellationToken.ThrowIfCancellationRequested();
				curMsg = string.Empty;
				conversation.Messages = await db.FlapperMessages
					.Where(c => c.ConversationId == conversation.Id)
					.OrderBy(m => m.SentAt)
					.ToListAsync(cancellationToken);

				result = await flapperOllamaApi.OllamaProcessWithToolsAsync(
					conversation,
					req.flapperDTO,
					uMsg,

					tools, async chunk =>
					{
						if (chunk.StartsWith("[") && string.IsNullOrEmpty(curMsg))
						{
							isClarification = true;
							//turn = MaxTurns;
						}
						curMsg += chunk;

						if (!isClarification)
						{
							// Stream thinking/content chunks to UI in real-time
							var streamMessage = new PulseMessage
							{
								SenderUserName = "Flapper",
								RecipientUserId = req.UserId,
								Role = "Flapper",
								Subject = "Flapper is thinking...",
								ContentType = "HTML",
								Content = chunk,
								SentAt = DateTime.Now
							};

							await hubContext.Clients.User(req.UserId.ToString())
							.SendAsync("ReceiveFlapperChunk", streamMessage);
						}
					}, cancellationToken);

				// If no tool calls → we're done
				if (result.ToolCalls == null || !result.ToolCalls.Any() || isClarification)
					break;

				db.FlapperMessages.Add(new FlapperMessage
				{
					ConversationId = conversation.Id,
					Sender = "Flapper",
					Content = curMsg,
					SentAt = DateTime.Now
				});
				await db.SaveChangesAsync();

				// Execute each tool call
				foreach (var toolCall in result.ToolCalls)
				{
					string toolResult = toolCall.ToolName switch
					{
						"ask_tables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message, conversation.Id.ToString()),
						"web_search" => await ExecuteWebSearchAsync(toolCall.Parameters, flapperOllamaApi),
						"web_fetch" => await ExecuteWebFetchAsync(toolCall.Parameters, flapperOllamaApi),
						_ => $"Unknown tool: {toolCall.ToolName}"
					};

					// Save tool result back into conversation so Flapper can reason again
					db.FlapperMessages.Add(new FlapperMessage
					{
						ConversationId = conversation.Id,
						Sender = "tool",
						Content = toolResult,
						SentAt = DateTime.Now,
						ToolName = toolCall.ToolName
					});
				}

				await db.SaveChangesAsync();

			} while (turn < MaxTurns);

			if (result.Content.StartsWith("[CLARIFICATION] ["))
			{
				isClarification = true;
			}
			// 6. Save Flapper's final response
			db.FlapperMessages.Add(new FlapperMessage
			{
				ConversationId = conversation.Id,
				Sender = "Flapper",
				Content = result.Content ?? result.Thinking ?? "No response generated.",
				SentAt = DateTime.Now,
				ContentType = isClarification ? "Clarification" : "HTML",
				IsClarificationQuestion = isClarification
			});

			await db.SaveChangesAsync();

			// 7. Send final message to UI via your existing hub
			var finalMessage = new PulseMessage
			{
				SenderUserName = "Flapper",
				RecipientUserId = req.UserId,
				Role = "Flapper",
				Subject = isClarification ? "Clarification" : "Flapper Reply",
				ContentType = "HTML",
				Content = isClarification ? curMsg.Replace("[CLARIFICATION] [", "").TrimEnd("]").ToString() : result.Content ?? result.Thinking ?? "",
				SentAt = DateTime.Now
			};

			await hubContext.Clients.User(req.UserId.ToString())
				.SendAsync("ReceiveMessage", finalMessage);

			return Results.Ok(result);

		}
		catch (OperationCanceledException)
		{
			return Results.Ok(new FlapperResponse
			{
				Success = false,
				Content = "The operation was cancelled."
			});
		}

		catch (Exception ex)
		{
			return Results.Ok(new FlapperResponse
			{
				Success = false,
				Content = $"An error occurred: {ex.Message}"
			});
		}
	}

	private static async Task<string> ExecuteAskTablesAsync(TablesAPI tablesApi, Dictionary<string, object> parameters, string originalQuery, string conversationId)
	{
		if (!parameters.TryGetValue("query", out var queryObj))
			return "Error: Missing query parameter";

		var query = queryObj.ToString();
		//Call your existing TablesAPI
		TablesRequest tablesRequest = new TablesRequest
		{
			UserId = "Flapper", // You can pass actual user ID if needed for logging
			UserRequest = query,
			UserEmail = "flapper@example.com",
			ModelName = "Default",
			SessionId = conversationId
		};
		var result = await tablesApi.AskTablesAsync(tablesRequest);
		if (result.Success)
		{
			originalQuery = originalQuery.Replace("[REPORT]", "").Replace("[SPEECH]", "").Trim();
			var tblResponse = $"**User asked**: {originalQuery}\n**Data from database**:\n{JsonSerializer.Serialize(result.Data)}";
			return tblResponse;
		}
		else
		{
			return $"Error: {result.Message ?? "Unknown error"}";
		}
	}

	private static async Task<string> ExecuteWebSearchAsync(Dictionary<string, object> parameters, FlapperOllamaAPI flapperOllamaAPI)
	{
		if (!parameters.TryGetValue("query", out var queryProp))
			return "Error: Missing 'query' argument";

		var query = queryProp.ToString();
		if (string.IsNullOrEmpty(query))
			return "Error: Query is empty";

		var response = await flapperOllamaAPI.SearchWeb(query);

		if (response?.Results is not { Count: > 0 })
			return $"No search results found for '{query}' related to user question"; //: {context.OriginalUserQuery}";

		var results = string.Join(" | ", response.Results.Select(r =>
			$"[{r.Title}]({r.Url}): {r.Snippet}"));

		// ✅ Include original user query in the tool result
		//var contextualResult = $"**User asked**: {context.OriginalUserQuery}\n\n" +
		var contextualResult = $"**Web search results for '{query}'**:\n{results}";

		return contextualResult.Length > 15000 ? contextualResult[..15000] + "..." : contextualResult;
		//if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
		//    return "Error: Missing 'query' argument";

		//if (string.IsNullOrEmpty(query))
		//    return "Error: Query is empty";

		//Placeholder implementation -integrate with your actual web search service
		//For now, return a stub response
		//return $"Web search results for '{query}': [Integration pending with external search service]";
	}

	private static async Task<string> ExecuteWebFetchAsync(Dictionary<string, object> parameters, FlapperOllamaAPI flapperOllamaAPI)
	{



		if (!parameters.TryGetValue("url", out var urlObj))
			return "Error: Missing 'url' argument";

		var url = urlObj.ToString();
		if (string.IsNullOrEmpty(url))
			return "Error: URL is empty";

		var response = await flapperOllamaAPI.FetchWebPage(url);
		//Placeholder implementation -integrate with your actual web fetch service
		var content = response?.Content ?? "";

		// ✅ Include original user query and URL context in the tool result
		//var contextualResult = $"**Original user question**: {context.OriginalUserQuery}\n\n" +
		var contextualResult = $"**Content fetched from {url}**:\n{content}";

		return contextualResult.Length > 12000 ? contextualResult[..12000] + "..." : contextualResult;
	}

	private static async Task<IResult> GetUserConversationHistory(string userId, PulseDbContext db)
	{
		var conversations = await db.FlapperConversations
			.Where(c => c.UserId == userId)
			//.Include(c => c.Messages.OrderBy(m => m.SentAt))
			.OrderByDescending(c => c.LastActivity)
			.ToListAsync();

		return Results.Ok(new ApiResponse<List<FlapperConversation>>
		{
			Success = true,
			Data = conversations,
			Message = $"Retrieved {conversations.Count} conversations for user {userId}"
		});
	}

	private static async Task<string> ExecuteWebSearchIClientAsync(Dictionary<string, object> parameters, FlapperAPI flapperApi)
	{
		if (!parameters.TryGetValue("query", out var queryProp))
			return "Error: Missing 'query' argument";

		var query = queryProp.ToString();
		if (string.IsNullOrEmpty(query))
			return "Error: Query is empty";

		var response = await flapperApi.IClientSearchWeb(query);

		if (response?.Results is not { Count: > 0 })
			return $"No search results found for '{query}' related to user question"; //: {context.OriginalUserQuery}";

		var results = string.Join(" | ", response.Results.Select(r =>
			$"[{r.Title}]({r.Url}): {r.Snippet}"));

		// ✅ Include original user query in the tool result
		//var contextualResult = $"**User asked**: {context.OriginalUserQuery}\n\n" +
		var contextualResult = $"**Web search results for '{query}'**:\n{results}";

		return contextualResult.Length > 15000 ? contextualResult[..15000] + "..." : contextualResult;
		//if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
		//    return "Error: Missing 'query' argument";

		//if (string.IsNullOrEmpty(query))
		//    return "Error: Query is empty";

		//Placeholder implementation -integrate with your actual web search service
		//For now, return a stub response
		//return $"Web search results for '{query}': [Integration pending with external search service]";
	}

	private static async Task<string> ExecuteWebFetchIClientAsync(Dictionary<string, object> parameters, FlapperAPI flapperApi)
	{



		if (!parameters.TryGetValue("url", out var urlObj))
			return "Error: Missing 'url' argument";

		var url = urlObj.ToString();
		if (string.IsNullOrEmpty(url))
			return "Error: URL is empty";

		var response = await flapperApi.IClientFetchWebPage(url);
		//Placeholder implementation -integrate with your actual web fetch service
		var content = response?.Content ?? "";

		// ✅ Include original user query and URL context in the tool result
		//var contextualResult = $"**Original user question**: {context.OriginalUserQuery}\n\n" +
		var contextualResult = $"**Content fetched from {url}**:\n{content}";

		return contextualResult.Length > 12000 ? contextualResult[..12000] + "..." : contextualResult;
	}

	public static async Task<IResult> FlapperIChat(
		FlapperChatRequest req,
		AiShared _aiShared,
		FlapperOllamaAPI flapperOllamaApi,
		TablesAPI tablesApi,           // Your SQL expert
		PulseDbContext db,
		IHubContext<MessageHub> hubContext,
	CancellationToken cancellationToken)
	{ 
		var response = await flapperOllamaApi.FlapperStreamingIChat(req, _aiShared,tablesApi, db, hubContext, cancellationToken);
        await hubContext.Clients.User(req.UserId.ToString())
                .SendAsync("FlapperChatResponse", response);
        return Results.Ok(new ApiResponse<FlapperResponse>
        {
            Success = true,
            Data = response,
            Message = $"Retrieved "
        });

    }


}

#region Old Flapper Endpoint Logic (for reference, not included in final code)

//private static async Task<IResult> FlapperChat(
//        FlapperChatRequest req,
//        AiShared _aiShared,
//        FlapperAPI flapperApi,
//        TablesAPI tablesApi,
//        PulseDbContext db,
//        IHubContext<MessageHub> hubContext)
//    {
//        var conversation = await db.FlapperConversations
//            .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);
//        if (conversation == null)
//        {
//            conversation = new FlapperConversation
//            {
//                Id = req.ConversationId,
//                UserId = req.UserId,
//                Title = "Flapper Chat"
//            };
//            db.FlapperConversations.Add(conversation);
//        }
//        var userMessage = new FlapperMessage
//        {
//            ConversationId = conversation.Id,
//            Sender = "User",
//            Content = req.Message,
//            SentAt = DateTime.UtcNow
//        };
//        db.FlapperMessages.Add(userMessage);
//        await db.SaveChangesAsync();
//        conversation.LastActivity = DateTime.UtcNow;


//        SystemMessageData systemMessageData = new SystemMessageData
//        {
//            CompanyName = "H&M Rollers",
//            CompanyInformation = AiPromptHelperService.GetTempCoInfo(),
//            UserName = req.PreferredUserName,
//            DbSchema = await _aiShared.GetDetailedSchemaAsync()
//        };
//        Flapper flapper = new Flapper();

//        var flapperSystemPrompt = flapper.BuildSystemMessagev2(systemMessageData);

//        //Define available tools(without AskUser - you already have clarification handled separately)
//        var tools = BuildFlapperTools();

//        int maxTurns = 5;           // Safety limit for multi-turn loops
//        int currentTurn = 0;
//        FlapperResponse finalResult = null!;

//        while (currentTurn < maxTurns)
//        {
//            currentTurn++;

//            //Let Flapper reason and possibly call tools
//            finalResult = await flapperApi.ProcessWithToolsAsync(
//                conversation, req.Message, flapperSystemPrompt, tools);

//            //If no tool calls, we're done
//            if (finalResult.ToolCalls == null || !finalResult.ToolCalls.Any())
//                break;

//            //Execute all tool calls
//            foreach (var toolCall in finalResult.ToolCalls)
//            {
//                string toolResult = toolCall.ToolName switch
//                {
//                    "AskTables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message),
//                    "WebSearch" => await ExecuteWebSearchAsync(toolCall.Parameters),
//                    "WebFetch" => await ExecuteWebFetchAsync(toolCall.Parameters),
//                    _ => $"Unknown tool: {toolCall.ToolName}"
//                };

//                //Add tool result back into conversation history so Flapper can continue reasoning
//                db.FlapperMessages.Add(new FlapperMessage
//                {
//                    ConversationId = conversation.Id,
//                    Sender = "tool",
//                    Content = toolResult,
//                    SentAt = DateTime.UtcNow,

//                    ToolName = toolCall.ToolName
//                });
//            }

//            await db.SaveChangesAsync();
//        }

//        //Save Flapper's final message
//        db.FlapperMessages.Add(new FlapperMessage
//        {
//            ConversationId = conversation.Id,
//            Sender = "Flapper",
//            Content = finalResult.Content ?? finalResult.Thinking ?? "No response generated.",
//            SentAt = DateTime.UtcNow,
//            ContentType = "HTML"
//        });

//        await db.SaveChangesAsync();

//        //Send final response to UI
//        var finalMessage = new PulseMessage
//        {
//            SenderUserName = "Flapper",
//            RecipientUserId = req.UserId,
//            Role = "Flapper",
//            ContentType = "HTML",
//            Content = finalResult.Content ?? finalResult.Thinking ?? "",
//            SentAt = DateTime.UtcNow

//        };

//        await hubContext.Clients.User(req.UserId.ToString())
//            .SendAsync("ReceiveMessage", finalMessage);

//        return Results.Ok(finalResult);


//    }

//private static List<object> BuildFlapperTools()
//{
//    return new List<object>
//    {
//        new
//        {
//            type = "function",
//            function = new
//            {
//                name = "AskTables",
//                description = "Ask the SQL expert (Tables) to generate and run a query against the database. Use this when you need real data from the Pulse database.",
//                parameters = new
//                {
//                    type = "object",
//                    properties = new
//                    {
//                        query = new { type = "string", description = "Natural language description of the data you need" }
//                    },
//                    required = new[] { "query" }
//                }
//            }
//        },
//        new
//        {
//            type = "function",
//            function = new
//            {
//                name = "WebSearch",
//                description = "Search the internet for current information not available in the database.",
//                parameters = new
//                {
//                    type = "object",
//                    properties = new { query = new { type = "string", description = "Search query" } },
//                    required = new[] { "query" }
//                }
//            }
//        },
//        new
//        {
//            type = "function",
//            function = new
//            {
//                name = "WebFetch",
//                description = "Fetch the full content of a specific webpage.",
//                parameters = new
//                {
//                    type = "object",
//                    properties = new { url = new { type = "string", description = "URL to fetch" } },
//                    required = new[] { "url" }
//                }
//            }
//        }
//    };
//}


//        bool isClarification = false;
//    string clarificationQuestion = "";
//    string finalResponse = string.Empty;

//        if (!req.IsStreaming)
//        {
//            var result = await flapperApi.ProcessMessageAsync(conversation, req.Message, flapperSystemPrompt);
//isClarification = result.RequiresClarification;
//            finalResponse = isClarification? result.ClarificationQuestion! : result.Content!;
//        }
//        else
//{
//    await flapperApi.StreamResponseAsync(conversation, req.Message, flapperSystemPrompt, async chunk =>
//    {

//        // Check for clarification marker
//        //if (chunk.StartsWith("[CLARIFICATION]"))
//        if (chunk.StartsWith("[") && string.IsNullOrEmpty(finalResponse))
//        {
//            isClarification = true;
//        }

//        finalResponse += chunk;

//        if (!isClarification)
//        {
//            var pulseMessage = new PulseMessage
//            {
//                SenderUserName = "Flapper",
//                RecipientUserId = req.UserId,
//                Role = "Flapper",
//                Subject = "Flapper Reply",
//                ContentType = "HTML",
//                Content = chunk,
//                SentAt = DateTime.UtcNow
//            };
//            await hubContext.Clients.User(req.UserId.ToString())
//            .SendAsync("ReceiveFlapperChunk", pulseMessage);
//        }
//    });
//}

//if (finalResponse.StartsWith("[CLARIFICATION] [", StringComparison.Ordinal))
//{
//    finalResponse = finalResponse.Replace("[CLARIFICATION] [", "");
//    finalResponse = finalResponse.Substring(0, finalResponse.Length - 1);
//}

//FlapperMessage fmsg = new FlapperMessage
//{
//    ConversationId = conversation.Id,
//    Sender = "Flapper",
//    Content = finalResponse,
//    SentAt = DateTime.UtcNow,
//    ContentType = isClarification ? "Clarification" : "HTML",
//    IsClarificationQuestion = isClarification,
//};
//db.FlapperMessages.Add(fmsg);
//await db.SaveChangesAsync();

//if (isClarification || !req.IsStreaming)
//{

//    var clarMessage = new PulseMessage
//    {
//        SenderUserName = "Flapper",
//        RecipientUserId = req.UserId,
//        SenderUserId = isClarification ? fmsg.Id.ToString() : null,
//        Role = "Flapper",
//        Subject = "Clarification Needed",
//        ContentType = "HTML",
//        Content = finalResponse,
//        SentAt = DateTime.UtcNow
//    };
//    await hubContext.Clients.User(req.UserId.ToString())
//        .SendAsync("ReceiveMessage", clarMessage);
//}
//FlapperResponse response = new FlapperResponse
//{
//    Success = true,
//    RequiresClarification = isClarification,
//    ClarificationQuestion = isClarification ? clarificationQuestion : null
//};
//return Results.Ok(response);
//    }
//}

// Simple request DTO

#endregion
