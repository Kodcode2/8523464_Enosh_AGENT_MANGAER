using AgentMVC.Models;
using AgentMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AgentMVC.Service
{
    public class MissionService(
        IHttpClientFactory clientFactory,
        ITargetSevice targetSevice,
        IAgentService agentService
        ) : IMissionService
    {
        private readonly string baseUrl = "https://localhost:7121/Missions";
        public async Task<List<MissionVM>> GetAllMissions()
        {
            var httpClient = clientFactory.CreateClient();
            var result = await httpClient.GetAsync($"{baseUrl}");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                List<MissionModel>? missions = JsonSerializer.Deserialize<List<MissionModel>>(content,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? [];
                var missionTasks = missions.Select(ConvertMissionToVM).ToList();
                var missionVMs = await Task.WhenAll(missionTasks);
                return [.. missionVMs];
            }
            return [];
        }
        
        private double MeasureDistance(TargetModel target, AgentModel agent) =>
            Math.Sqrt(Math.Pow(target.XPosition - agent.XPosition, 2)
                    + Math.Pow(target.YPosition - agent.YPosition, 2));

        private async Task<MissionVM> ConvertMissionToVM(MissionModel mission)
        {
            var targets = await targetSevice.GetAllTargetsAsync();
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
    }
}
