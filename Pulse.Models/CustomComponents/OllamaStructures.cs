using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
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

        // [JsonPropertyName("num_gpu")]
        // public int NumGpu { get; set; } = -1;  // Offload all layers to GPU if available
    }

}
