


//using OllamaSharp;
//using Pulse.ApiService.PulseAI.Services;
//using Pulse.Models;
//using Pulse.Models.AI;
//using Pulse.Models.Api;
//using Pulse.Models.PulseContext;
//using System.Text.Json;

//namespace Pulse.ApiService.PulseAI.Characters;

//public class TablesAPI
//{
//    private readonly AiShared _aiShared;
//    private readonly PulseAiClientFactory _aiClientFactory;
//    private readonly PulseDbContext _dbContext;
//    private static PulseAiResponse? _response;
//    private static readonly Dictionary<string, Chat> _sessions = new(StringComparer.OrdinalIgnoreCase);
//    public TablesAPI(AiShared aiShared, PulseAiClientFactory pulseAiFactory, PulseDbContext dbContext)
//    {
//        _aiShared = aiShared;
//        _aiClientFactory = pulseAiFactory;
//        _dbContext = dbContext;
        
//    }

//    public static async Task<ApiResponse<PulseAiResponse>> AskTables(PulseAiRequest request) 
//    {
//        _response = new PulseAiResponse { ModelUsed = request.ModelName,
//        SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
//        Timestamp = DateTime.UtcNow
//        };
    
//    }
//    public async Task<AiQueryResponse<List<Dictionary<string, object>>>> GetSqlQueryAsync(string userQuestion)
//    {
//        var client = _aiClientFactory.GetClient("TablesClient");
//        string systemMessage = await GetTablesSystemMessageAsync();
//        var chat = _sessions.GetValueOrDefault(_response.SessionId, new Chat(client));
//        chat.Model =_response.ModelUsed;

//        if (chat.Messages.Count == 0)
//        {
//            await chat.SendAsync(systemMessage, new SystemPromptOptions());
//        }
//        else
//        {

//        }

//        var messages = new List<ChatMessage>
//        {
//            new ChatMessage(ChatRole.System, systemMessage),
//            new ChatMessage(ChatRole.User, userQuestion)
//        };
//        var response = await apiTables.ChatCompletion.CreateAsync(messages);
//        return response.Choices.FirstOrDefault()?.Message.Content ?? "";
//    }

//    private async Task<string> GetTablesSystemMessageAsync()
//    {
//        string examples = await _aiShared.GetAiExamplesAsync();
//        string schemaText = await _aiShared.GetDetailedSchemaAsync();
        

//        return $$"""
//            Your name is Tables. You are a Microsoft SQL Server expert, and have a passion for generating accurate and efficient SQL queries.
//            Once you have extracted the correct data, you love to format it in, you guessed it, TABLES! You are a data enthusiast and take pride
//            in crafting the perfect SQL query to get the job done.

//            You have been put in charge of retrieving the correct data from the company's database for your work colleagues. They, however, are
//            not very good at formulating SQL queries and will therefore ask you questions in natural language. Your job is to convert those questions
//            into SQL queries, and then run them against the database and return the data they are looking for.

//            The database is a Standard SQL Server 2022 database, so only ANSI standard SQL syntax will work (No T-SQL, etc). You must ensure that your 
//            queries are compatible with SQL Server 2022. Examine the following schema and relationships carefully to understand how to retrieve the correct data.

//            {{schemaText}}

//            Here are some examples of natural language questions and their corresponding SQL queries to help you understand how to convert questions into SQL:
//            {{examples}}


//            """;
//    }
//}
