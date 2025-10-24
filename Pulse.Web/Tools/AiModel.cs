using Pulse.Models.CustomComponents;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Pulse.Web.Tools
{
    public class AiModel
    {
        public string ModelName { get; set; }
        public string? Schema { get; private set; } = string.Empty;
        public string? Prompt { get; private set; } = string.Empty;
        public string? Examples { get; set; } = string.Empty;
        public string? Question { get; set; } = string.Empty;
        public AiModel(string modelName) 
        {
            ModelName = modelName;
        }
        public void SetSchema(List<SchemaDto> rawschema)
        {
            string modelSchema = string.Empty;

            switch (this.ModelName)
            {
                case "sqlcoder:15b":
                    this.Schema = CreateTblSchema(rawschema);
                    break;

                case "gemma3:27b":
                case "llama3.1:latest":
                    this.Schema = CreateDetailSchema(rawschema);
                    break;
            }
        }

        private static string CreateTblSchema(List<SchemaDto> dbSchema)
        {
            Dictionary<string, string> TypeMap = new()
        {
            { "System.Int32", "int" },
            { "System.String", "nvarchar" }, // Adjust length in SQL generation
            { "System.Decimal", "decimal(18,2)" },
            { "System.Boolean", "bit" },
            { "System.DateTime", "datetime2" }
            // Add more mappings as needed (e.g., System.Int64 → bigint)
        };

            var sb = new StringBuilder();
            var foreignKeys = new List<string>();

            foreach (var schema in dbSchema)
            {
                if (string.IsNullOrEmpty(schema.TableName) || schema.Columns == null)
                    continue;

                // Start CREATE TABLE
                sb.AppendLine($"CREATE TABLE [{schema.TableName}] (");

                // Columns
                foreach (var col in schema.Columns)
                {
                    // Map .NET type to SQL Server type
                    string sqlType = TypeMap.TryGetValue(col.DataType, out var mappedType) ? mappedType : col.DataType;

                    // Adjust nvarchar length based on schema (e.g., nvarchar(10), nvarchar(255), or nvarchar(max))
                    if (sqlType == "nvarchar" && col.DataType == "System.String")
                    {
                        // Extract length from schema if provided (e.g., "nvarchar(10)" in original JSON)
                        // For simplicity, use a default length or max based on column name or context
                        sqlType = col.Name.Contains("max", StringComparison.OrdinalIgnoreCase) ? "nvarchar(max)" :
                                  col.Name.Contains("ID", StringComparison.OrdinalIgnoreCase) ? "nvarchar(50)" : "nvarchar(255)";
                    }

                    var columnDef = $"    [{col.Name}] {sqlType}{(col.IsNullable ? "" : " NOT NULL")}";
                    sb.AppendLine($"{columnDef},");
                }

                // Primary Key
                if (schema.PrimaryKeys != null && schema.PrimaryKeys.Count > 0)
                {
                    var pkColumns = string.Join(", ", schema.PrimaryKeys.Select(pk => $"[{pk}]"));
                    sb.AppendLine($"    CONSTRAINT [PK_{schema.TableName}] PRIMARY KEY ({pkColumns}),");
                }

                // Remove trailing comma
                if (sb[sb.Length - 1] == ',')
                    sb.Length -= 3; // Trim ", \n"

                sb.AppendLine();
                sb.AppendLine(");");

                // Collect Foreign Keys
                if (schema.Relationships != null)
                {
                    foreach (var rel in schema.Relationships)
                    {
                        foreach (var fkCol in rel.ForeignKeyColumns)
                        {
                            var fkConstraint = $"ALTER TABLE [{schema.TableName}] ADD CONSTRAINT [FK_{schema.TableName}_{rel.RelatedTableName}_{fkCol}] " +
                                               $"FOREIGN KEY ([{fkCol}]) REFERENCES [{rel.RelatedTableName}] ([{fkCol}]);";
                            foreignKeys.Add(fkConstraint);
                        }
                    }
                }

                sb.AppendLine();
            }

            // Append Foreign Key Constraints
            if (foreignKeys.Count > 0)
            {
                sb.AppendLine("-- Foreign Key Constraints");
                foreach (var fk in foreignKeys)
                {
                    sb.AppendLine(fk);
                }
            }

            return sb.ToString();
        }

        private static string CreateDetailSchema(List<SchemaDto> dbSchema)
        {
            var schemaBuilder = new StringBuilder();

            foreach (var schema in dbSchema)
            {
                schemaBuilder.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
                schemaBuilder.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys)}");

                schemaBuilder.AppendLine("Columns:");
                foreach (var col in schema.Columns)
                {
                    schemaBuilder.AppendLine($"- {col.Name} (Type: {col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
                }

                schemaBuilder.AppendLine("Relationships:");
                foreach (var rel in schema.Relationships)
                {
                    schemaBuilder.AppendLine($"- To {rel.RelatedEntityType} (Table: {rel.RelatedTableName}), Navigation: {rel.NavigationName}, FK Columns: {string.Join(", ", rel.ForeignKeyColumns)}, Cardinality: {rel.Cardinality}");
                }
                schemaBuilder.AppendLine(); // Separator
            }

            return schemaBuilder.ToString();
        }

        public void SetExamples(string examples)
        {
            this.Examples = examples;
        }

        public void SetQuestion(string question) {  this.Question = question; }

        public void SetPrompt()
        {
            var sP = string.Empty;

            sP = $@"### Instructions:
                    You are an expert in SQL Server 2022 T-SQL. 
                    Your task is to convert a question into a SQL query, given a SQL Server database schema.
                    Adhere to these rules:
                    -**Deliberately go through the question and database schema word by word**to appropriately answer the question
                    -**Use Table Aliases**to prevent ambiguity. For example, `SELECT table1.col1, table2.col1 FROM table1 JOIN table2 ON table1.id = table2.id`.
                    -When creating a ratio, always cast the numerator as float

                    ### Input:
                    Generate a SQL query that answers the question `{this.Question}`.
                    This query will run on a database whose schema is represented in this string:
                    {this.Schema}

                    {this.Examples}
                    
                    ### Reasoning:

                    ### Response:
                    The Response must start with:
                    Generated SQL: '''sql";
            this.Prompt = sP;
        }
    }

    public class OllamaTagsResponse
    {
        public List<OllamaModelInfo> Models { get; set; }
    }

    public class OllamaModelInfo
    {
        public string Name { get; set; }
    }

    // Ollama /api/chat response structure (single non-stream)
    public class OllamaChatResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public OllamaMessage Message { get; set; }  // Includes content and tool_calls

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

    public class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "assistant";

        [JsonPropertyName("content")]
        public string Content { get; set; }  // Generated SQL text

        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCall>? ToolCalls { get; set; }  // Plural array, even for single

        public string ToolName { get; set; }  // Custom prop for tool response (not serialized to Ollama, for internal use)
    }

    public class OllamaGenerateResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; }  // The generated text (e.g., SQL from prompt)

        [JsonPropertyName("done")]
        public bool Done { get; set; }  // True if generation complete

        // Optional metrics for debugging/performance
        [JsonPropertyName("context")]
        public int[] Context { get; set; }  // Token context for continuations (if needed)

        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }  // Nanoseconds

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

    public class OllamaTool
    {
        public string Type { get; set; } = "function";
        public string FunctionName { get; set; }
        public object FunctionParameters { get; set; }
        public string Description { get; set; }
    }

    public class OllamaWebSearchResponse
    {
        public List<OllamaWebSearchResult> Results { get; set; }
    }

    public class OllamaWebFetchResponse
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<OllamaWebLink> Links { get; set; }
    }

    public class OllamaWebSearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Snippet { get; set; }
    }

    public class OllamaWebLink
    {
        public string Url { get; set; }
        public string Text { get; set; }
    }

    // Tool call class for tool_calls array
    public class OllamaToolCall
    {
        [JsonPropertyName("function")]
        public OllamaFunctionCall Function { get; set; }
    }

    public class OllamaFunctionCall
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("arguments")]
        public JsonElement Arguments { get; set; }  // Flexible for params like {"query": "value"}
    }
    //public class OllamaTagsResponse
    //{
    //    public List<OllamaModelInfo> Models { get; set; }
    //}

    //public class OllamaOptions
    //{
    //    public string ApiKey { get; set; } = "";
    //    public bool OptimizeSchema { get; set; } = true; // Enable keyword-based filtering of schema
    //    public int MaxSchemaTokens { get; set; } = 1500; // Rough limit to truncate schema text
    //    public string DefaultModel { get; set; } = "llama3.1:latest"; // Fallback model
    //    // Add more, e.g., public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    //}

    //public class OllamaModelInfo
    //{
    //    public string Name { get; set; }
    //}

    //// Ollama /api/chat response structure (single non-stream)
    //public class OllamaChatResponse
    //{
    //    [JsonPropertyName("model")]
    //    public string Model { get; set; }

    //    [JsonPropertyName("created_at")]
    //    public DateTime CreatedAt { get; set; }

    //    [JsonPropertyName("message")]
    //    public OllamaMessage Message { get; set; }  // Includes content and tool_calls

    //    [JsonPropertyName("done")]
    //    public bool Done { get; set; }

    //    // Metrics
    //    [JsonPropertyName("total_duration")]
    //    public long TotalDuration { get; set; }

    //    [JsonPropertyName("load_duration")]
    //    public long LoadDuration { get; set; }

    //    [JsonPropertyName("prompt_eval_count")]
    //    public int PromptEvalCount { get; set; }

    //    [JsonPropertyName("prompt_eval_duration")]
    //    public long PromptEvalDuration { get; set; }

    //    [JsonPropertyName("eval_count")]
    //    public int EvalCount { get; set; }

    //    [JsonPropertyName("eval_duration")]
    //    public long EvalDuration { get; set; }
    //}

    //public class OllamaMessage
    //{
    //    [JsonPropertyName("role")]
    //    public string Role { get; set; } = "assistant";

    //    [JsonPropertyName("content")]
    //    public string Content { get; set; }  // Generated SQL text

    //    [JsonPropertyName("tool_calls")]
    //    public List<OllamaToolCall> ToolCalls { get; set; }  // Plural array, even for single

    //    public string ToolName { get; set; }  // Custom prop for tool response (not serialized to Ollama, for internal use)
    //}

    //public class OllamaGenerateResponse
    //{
    //    [JsonPropertyName("model")]
    //    public string Model { get; set; }

    //    [JsonPropertyName("created_at")]
    //    public DateTime CreatedAt { get; set; }

    //    [JsonPropertyName("response")]
    //    public string Response { get; set; }  // The generated text (e.g., SQL from prompt)

    //    [JsonPropertyName("done")]
    //    public bool Done { get; set; }  // True if generation complete

    //    // Optional metrics for debugging/performance
    //    [JsonPropertyName("context")]
    //    public int[] Context { get; set; }  // Token context for continuations (if needed)

    //    [JsonPropertyName("total_duration")]
    //    public long TotalDuration { get; set; }  // Nanoseconds

    //    [JsonPropertyName("load_duration")]
    //    public long LoadDuration { get; set; }

    //    [JsonPropertyName("prompt_eval_count")]
    //    public int PromptEvalCount { get; set; }

    //    [JsonPropertyName("prompt_eval_duration")]
    //    public long PromptEvalDuration { get; set; }

    //    [JsonPropertyName("eval_count")]
    //    public int EvalCount { get; set; }

    //    [JsonPropertyName("eval_duration")]
    //    public long EvalDuration { get; set; }
    //}

    //public class OllamaTool
    //{
    //    public string Type { get; set; } = "function";
    //    public string FunctionName { get; set; }
    //    public object FunctionParameters { get; set; }
    //    public string Description { get; set; }
    //}

    //public class OllamaWebSearchResponse
    //{
    //    public List<OllamaWebSearchResult> Results { get; set; }
    //}

    //public class OllamaWebFetchResponse
    //{
    //    public string Title { get; set; }
    //    public string Content { get; set; }
    //    public List<OllamaWebLink> Links { get; set; }
    //}

    //public class OllamaWebSearchResult
    //{
    //    public string Title { get; set; }
    //    public string Url { get; set; }
    //    public string Snippet { get; set; }
    //}

    //public class OllamaWebLink
    //{
    //    public string Url { get; set; }
    //    public string Text { get; set; }
    //}

    //// Tool call class for tool_calls array
    //public class OllamaToolCall
    //{
    //    [JsonPropertyName("function")]
    //    public OllamaFunctionCall Function { get; set; }
    //}

    //public class OllamaFunctionCall
    //{
    //    [JsonPropertyName("name")]
    //    public string Name { get; set; }

    //    [JsonPropertyName("arguments")]
    //    public Dictionary<string, object> Arguments { get; set; }  // Flexible for params like {"query": "value"}
    //}
}
