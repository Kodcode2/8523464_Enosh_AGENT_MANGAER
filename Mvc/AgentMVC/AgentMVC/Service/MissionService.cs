using AgentMVC.Models;
using AgentMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;

namespace AgentMVC.Service
{
    public class MissionService(
        IHttpClientFactory clientFactory,
        IServiceProvider serviceProvider
        ) : IMissionService
    {
        private ITargetSevice targetService => serviceProvider.GetRequiredService<ITargetSevice>();
        private IMissionService missionService => serviceProvider.GetRequiredService<IMissionService>();
        private IAgentService agentService => serviceProvider.GetRequiredService<IAgentService>();
        private readonly string baseUrl = "https://localhost:7121/Missions";
        public async Task<List<MissionModel>> GetAllMissions()
        {
            var httpClient = clientFactory.CreateClient();
            var result = await httpClient.GetAsync($"{baseUrl}");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                List<MissionModel>? missions = JsonSerializer.Deserialize<List<MissionModel>>(content,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? [];
                return missions;
            }
            return [];
        }

        public async Task<List<MissionVM>> GetMissionVMs()
        {
            var missionModels = await GetAllMissions();
            var missionTasks = missionModels.Select(ConvertMissionToVM).ToList();
            var missionVMs = await Task.WhenAll(missionTasks);
            return [.. missionVMs];
        }
        
        private double MeasureDistance(TargetModel target, AgentModel agent) =>
            Math.Sqrt(Math.Pow(target.XPosition - agent.XPosition, 2)
                    + Math.Pow(target.YPosition - agent.YPosition, 2));

        private async Task<MissionVM> ConvertMissionToVM(MissionModel mission)
        {
            var targets = await targetService.GetAllTargetsAsync();
            var agents = await agentService.GetAllAgentsAsync();
            AgentModel? agent = agents.FirstOrDefault(a => a.Id == mission.AgentId);
            TargetModel? target = targets.FirstOrDefault(t => t.Id == mission.TargetId);
            if (target == null || agent == null) { throw new Exception("Could not found either the agent or the target"); }
            double distance = MeasureDistance(target, agent);
            MissionVM missionVM = new()
            {
                Id = mission.Id,
                AgentName = agent.NickName,
                XAgent = agent.XPosition,
                YAgent = agent.YPosition,
                TargetName = target.Name,
                XTarget = target.XPosition,
                YTarget = target.YPosition,
                Status = mission.MissionStatus.ToString(),
                Distance = distance,
                RemainingTime = distance / 5,
            };
            return missionVM;
        }

        public async Task AssignMissionAsync(long id)
        {
            var httpClient = clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/{id}/assign");
            var result = await httpClient.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Could not assign the mission");
            }
        }

        public async Task<MissionVM?> GetMissionVMById(long id)
        {
            var missions = await GetMissionVMs();
            return missions.FirstOrDefault(m => m.Id == id); 
        }
    }
}
