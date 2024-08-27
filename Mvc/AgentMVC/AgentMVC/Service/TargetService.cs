using AgentMVC.Models;
using System.Text.Json;

namespace AgentMVC.Service
{
    public class TargetService(IHttpClientFactory clientFactory) : ITargetSevice
    {
        private readonly string baseUrl = "https://localhost:7121/Targets";

        // Fetches all targets from the API
        public async Task<List<TargetModel>> GetAllTargetsAsync()
        {
            var httpClient = clientFactory.CreateClient();
            var result = await httpClient.GetAsync($"{baseUrl}");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                List<TargetModel>? targets = JsonSerializer.Deserialize<List<TargetModel>>(content,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? [];

                return targets;
            }
            return [];
        }
    }
}
