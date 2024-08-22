using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class MissionService(ApplicationDbContext context, IAgentService agentService) : IMissionService
    {
        public async Task<MissionModel?> GetMissionByAgentIdAsync(long agentId) =>
            await context.Missions.Where(m => m.AgentId == agentId).FirstOrDefaultAsync() 
            ?? throw new Exception("Could not find the mission by the given agentId");

        public async Task<MissionModel?> GetMissionByTargetIdAsync(long targetId) =>
            await context.Missions.Where(m => m.TargetId == targetId).FirstOrDefaultAsync()
            ?? throw new Exception("Could not find the mission by the given targetId");

        public async Task<List<MissionModel>> GetMissionsAsync() =>
           await context.Missions.AnyAsync()
            ? await context.Missions.ToListAsync()
            : [];

        public async Task<MissionModel?> GetMissionByIdAsync(long id) =>
            await context.Missions.Where(m => m.Id == id).FirstOrDefaultAsync()
            ?? throw new Exception("Could not found the mission by the given id");

        public async Task<MissionModel> ActivateMissionAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            mission!.Status = Status.Active;
            await context.SaveChangesAsync();
            return mission;
        }

        public async Task<MissionModel> EndMissionAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            mission!.Status = Status.Ended;
            await context.SaveChangesAsync();
            return mission;
        }

        public async Task<MissionModel?> CreateMissionByTarget(TargetModel target)
        {
            AgentModel? closestAgent = await agentService.GetAvailableAgentAsync(target);
            MissionModel? newMission = new()
            {
                Target = target,
                Agent = closestAgent,
                TargetId = target.Id,
                AgentId = closestAgent!.Id
            };
            await context.Missions.AddAsync(newMission);
            await context.SaveChangesAsync();
            return newMission;
        }

        public double MeasureDistance(TargetModel target, AgentModel agent) =>
            Math.Sqrt(Math.Pow(target.XPosition - agent.XPosition, 2)
                    + Math.Pow(target.YPosition - agent.YPosition, 2));

    }
}
