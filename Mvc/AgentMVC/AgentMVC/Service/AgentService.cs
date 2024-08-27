using AgentMVC.Models;
using AgentMVC.ViewModels;
using System.Text.Json;

namespace AgentMVC.Service
{
    public class AgentService(IHttpClientFactory clientFactory, IServiceProvider serviceProvider) : IAgentService
    {
        private IMissionService missionService => serviceProvider.GetRequiredService<IMissionService>();
        private readonly string baseUrl = "https://localhost:7121/Agents";

        // Fetches all agents from the API
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
            return [];
        }

        // Converts agent models to view models
        public async Task<List<AgentVM>> GetAgentVMs()
        {
            var agentModels = await GetAllAgentsAsync();
            var agentTasks = agentModels.Select(ConvertAgentToVM).ToList();
            var agentVMs = await Task.WhenAll(agentTasks);
            return [.. agentVMs];
        }

        // Converts a single agent model to view model
        private async Task<AgentVM> ConvertAgentToVM(AgentModel agent)
        {
            var missions = await missionService.GetAllMissions();

            // Find the active mission for this agent
            var activeMission = missions
                .Where(m => m.AgentId == agent.Id)
                .Where(m => m.MissionStatus == MissionStatus.Assigned)
                .FirstOrDefault();

            // Count completed missions (kills) for this agent
            int kills = missions
                .Where(m => m.AgentId == agent.Id)
                .Where(m => m.MissionStatus == MissionStatus.Ended)
                .Count();

            AgentVM? agentVM = new()
            {
                Id = agent.Id,
                NickName = agent.NickName,
                Image = agent.Image,
                XPosition = agent.XPosition,
                YPosition = agent.YPosition,
                Status = agent.AgentStatus.ToString(),
                Mission = activeMission != null ? $"https://localhost:7214/MissionsManagement/Details/{activeMission.Id}" : "",
                RemainingTime = activeMission != null ? activeMission.RemainingTime : 0,
                killAmount = kills,
            };
            return agentVM;
        }
    }
}
