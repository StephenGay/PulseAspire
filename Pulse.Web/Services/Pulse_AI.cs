using Pulse.Models.Misc;

namespace Pulse.Web.Services
{
    internal sealed class Pulse_AI(PulseApiService pulseApiService,
                                    OllamaService ollamaService)
    {
        public async Task<string> GetSQLFromOllamaAsync(string userQuery)
        {
            var dbSchema = await pulseApiService.GetSchemaAsync();
            if (string.IsNullOrWhiteSpace(dbSchema))
                return "Error: Unable to retrieve database schema information.";

            var aiQueries = await pulseApiService.GetAsync<List<AiQuery>>("/AI/examples");
            string examples = "";
            if (aiQueries != null && aiQueries.Count > 0)
            {
                examples = "\nExamples of correct queries:\n";
                foreach (var ex in aiQueries)
                {
                    examples += $"Question: {ex.Question}\nSQL: {ex.SqlQuery}\n";
                }
            }
            return await ollamaService.GenerateSQLQueryAsync(userQuery, dbSchema, examples);
        }
    }
}
