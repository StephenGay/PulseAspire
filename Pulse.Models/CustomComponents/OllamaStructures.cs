using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
    public static class ToolNames
    {
        public const string ExecuteSql = "execute_sql";
        public const string WebSearch = "web_search";
        public const string WebFetch = "web_fetch";
        public const string ExecuteActionSql = "execute_action_sql";
        public const string AskTables = "ask_tables";
        public const string AskUser = "ask_user";

        public static readonly HashSet<string> All = new HashSet<string> { ExecuteSql, WebSearch, WebFetch, ExecuteActionSql, AskTables, AskUser };
    }
    public class OllamaChatResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public OllamaChatMessage Message { get; set; } = new();

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        // Metrics
        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("prompt_eval_duration")]
        public long PromptEvalDuration { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }

        [JsonPropertyName("eval_duration")]
        public long EvalDuration { get; set; }
    }
    public class OllamaChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Content { get; set; }

        [JsonPropertyName("thinking")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Thinking { get; set; }

        [JsonPropertyName("tool_calls")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    public class OllamaToolCall
    {
        [JsonPropertyName("function")]
        public OllamaFunction Function { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";
    }

    public class OllamaFunction
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public JsonElement Arguments { get; set; }
    }

    public class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "assistant";

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Content { get; set; }  // Generated SQL text

        [JsonPropertyName("tool_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ToolName { get; set; }

        [JsonPropertyName("thinking")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Thinking { get; set; }

        [JsonPropertyName("tool_calls")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    public class OllamaGenerateResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("done_reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DoneReason { get; set; }

        [JsonPropertyName("context")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? Context { get; set; }

        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }
    }

    public class OllamaWebSearchResponse
    {
        [JsonPropertyName("results")]
        public List<WebSearchResult> Results { get; set; } = new();
    }

    public class WebSearchResult
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = string.Empty;
    }

    public class OllamaWebFetchResponse
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("status_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; set; }
    }

    public class OllamaChatChunk
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public OllamaChatMessage Message { get; set; } = new();

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }
    }
    public class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModel> Models { get; set; } = new();
    }

    public class OllamaModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Model { get; set; }

        [JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("digest")]
        public string Digest { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ModelDetails? Details { get; set; }
    }

    public class ModelDetails
    {
        [JsonPropertyName("format")]
        public string Format { get; set; } = string.Empty;

        [JsonPropertyName("family")]
        public string Family { get; set; } = string.Empty;

        [JsonPropertyName("families")]
        public List<string> Families { get; set; } = new();

        [JsonPropertyName("parameter_size")]
        public string ParameterSize { get; set; } = string.Empty;

        [JsonPropertyName("quantization_level")]
        public string QuantizationLevel { get; set; } = string.Empty;
    }
    public class OllamaModelInfo
    {
        public string Name { get; set; }
    }
    public class OllamaRequest
    {
        /// <summary>
        /// The Ollama model to use (e.g., "llama3" or a fine-tuned variant for SQL generation).
        /// Default: "llama3" – assuming a general-purpose model; override if using a SQL-specific one.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-oss:latest";

        /// <summary>
        /// The prompt for the model, typically the natural language query plus any schema context.
        /// No default as it's query-specific; set in your Web Api endpoint.
        /// </summary>
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Optional inference options to fine-tune behavior.
        /// Defaults provided for reliability in SQL generation.
        /// </summary>
        [JsonPropertyName("options")]
        public OllamaOptions Options { get; set; } = new OllamaOptions();
    }

    /// <summary>
    /// Nested options for Ollama request, with defaults optimized for your project.
    /// These align with llama.cpp parameters for performance and output quality.
    /// </summary>
    public class OllamaOptions
    {
        /// <summary>
        /// Context window size in tokens. Default: 4096 – sufficient for schema descriptions + NL queries without excessive memory use.
        /// </summary>
        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; set; } = 4096;

        /// <summary>
        /// Maximum tokens to generate. Default: 200 – keeps SQL statements concise for EF execution.
        /// </summary>
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 200;

        /// <summary>
        /// Temperature for randomness. Default: 0.5 – low for deterministic, accurate SQL; avoids hallucinations.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.5;

        /// <summary>
        /// Top-p sampling. Default: 0.9 – balances focus and variety in token selection.
        /// </summary>
        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 0.9;

        /// <summary>
        /// Top-k sampling. Default: 40 – considers top 40 tokens for focused output.
        /// </summary>
        [JsonPropertyName("top_k")]
        public int TopK { get; set; } = 40;

        /// <summary>
        /// Repetition penalty. Default: 1.1 – mild penalty to avoid redundant SQL clauses.
        /// </summary>
        [JsonPropertyName("repeat_penalty")]
        public double RepeatPenalty { get; set; } = 1.1;

        /// <summary>
        /// Stop sequences. Default: [";"] – stops at SQL terminator for complete queries.
        /// </summary>
        [JsonPropertyName("stop")]
        public string[] Stop { get; set; } = new[] { ";" };

        /// <summary>
        /// Number of threads for CPU inference. Default: Environment.ProcessorCount – matches system cores for efficiency.
        /// </summary>
        [JsonPropertyName("num_thread")]
        public int NumThread { get; set; } = Environment.ProcessorCount;

        // Add other options as needed from previous discussions, e.g.:
        // [JsonPropertyName("seed")]
        // public int Seed { get; set; } = 42;  // For reproducibility in testing

        [JsonPropertyName("num_gpu")]
        public int NumGpu { get; set; } = -1;  // Offload all layers to GPU if available

        [JsonPropertyName("frequency_penalty")]
        public double FrequencyPenalty { get; set; } = 2;

        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; } = 2;
    }

    /// <summary>
    /// Represents a streaming chunk from Ollama's /api/chat.
    /// </summary>
    

    /// <summary>
    /// Delta for message in streaming chunks (partial content or tool calls).
    /// </summary>
    public class OllamaMessageDelta
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; } = string.Empty;

        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    // Ollama /api/chat response structure (single non-stream)
    

    

    

    public class OllamaTool
    {
        public string Type { get; set; } = "function";
        public string FunctionName { get; set; }
        public object FunctionParameters { get; set; }
        public string Description { get; set; }
    }

    

    
    
    public class OllamaWebLink
    {
        [JsonPropertyName("url")] // Or "href" if that's in your JSON—check logs
        public string? Url { get; set; }

        [JsonPropertyName("description")] // Or "title"/"description" if mismatched
        public string? Text { get; set; }
    }
    // Tool call class for tool_calls array
    
}
