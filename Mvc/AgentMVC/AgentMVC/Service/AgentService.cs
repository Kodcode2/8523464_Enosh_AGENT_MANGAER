using AgentMVC.Models;
using System.Text.Json;

namespace AgentMVC.Service
{
    public class AgentService(IHttpClientFactory clientFactory) : IAgentService
    {
        private readonly string baseUrl = "https://localhost:7121/Agents";

        public async Task<List<AgentModel>> GetAllAgentsAsync()
        {
            var httpClient = clientFactory.CreateClient();
            var result = await httpClient.GetAsync($"{baseUrl}");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                List<AgentModel>? agents = JsonSerializer.Deserialize<List<AgentModel>>(content,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? [];

                return agents;
            }
            return new();
        }
    }
}
