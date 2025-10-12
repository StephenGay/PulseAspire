using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Web.Tools;

namespace Pulse.Web.Services
{
    internal sealed class Pulse_AI(PulseApiService pulseApiService,
                                    OllamaService ollamaService)
    {
        public async Task<string> GetSQLFromOllamaAsync(string userQuery)
        {
            string dbSchema = await pulseApiService.GetDetailedSchemaAsync();
            if (string.IsNullOrEmpty(dbSchema))
                return "Error: Unable to retrieve database schema information.";

            //AiModel ai = new AiModel(aiModel);
            //ai.SetSchema(dbSchema);

            var aiQueries = await pulseApiService.GetAsync<List<AiQuery>>("/AI/examples");
            string examples = "";
            if (aiQueries != null && aiQueries.Count > 0)
            {
                examples = "\nExamples of correct queries:\n";
                foreach (var ex in aiQueries)
                {
                    examples += $"Question: {ex.Question}\nSQL: {ex.SqlQuery}\n";
                }
                //ai.SetExamples(examples);
            }
            //ai.SetQuestion(userQuery);
            //ai.SetPrompt();

            return await ollamaService.GenerateSQLQueryAsync(dbSchema,examples,userQuery);
        }
    }
}
