using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class MissionService(ApplicationDbContext context) : IMissionService
    {
        public async Task<MissionModel?> GetMissionByAgentIdAsync(long agentId) =>
            await context.Missions.FirstOrDefaultAsync(m => m.AgentId == agentId) 
            ?? throw new Exception("Could not find the mission by the given agentId");

        public async Task<MissionModel?> GetMissionByTargetIdAsync(long targetId) =>
            await context.Missions.FirstOrDefaultAsync(m => m.TargetId == targetId)
            ?? throw new Exception("Could not find the mission by the given targetId");

        public async Task<List<MissionModel>> GetMissionsAsync() =>
           await context.Missions.AnyAsync()
            ? await context.Missions.ToListAsync()
            : [];

        public async Task<MissionModel?> GetMissionByIdAsync(long id) =>
            await context.Missions.FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new Exception("Could not found the mission by the given id");

        public async Task<MissionModel> ActivateMissionAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            mission!.MissionStatus = Status.Active;
            await context.SaveChangesAsync();
            return mission;
        }

        public async Task<MissionModel> EndMissionAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            mission!.MissionStatus = Status.Ended;
            await context.SaveChangesAsync();
            return mission;
        }

        public async Task<MissionModel?> CreateMissionAsync(TargetModel target, AgentModel agent)
        {
            MissionModel? newMission = new()
            {
                Target = target,
                Agent = agent,
                TargetId = target.Id,
                AgentId = agent!.Id
            };
            await context.Missions.AddAsync(newMission);
            await context.SaveChangesAsync();
            return newMission;
        }

        public double MeasureDistance(TargetModel target, AgentModel agent) =>
            Math.Sqrt(Math.Pow(target.XPosition - agent.XPosition, 2)
                    + Math.Pow(target.YPosition - agent.YPosition, 2));

        public async Task DeleteMissionByIdAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            context.Missions.Remove(mission!);
            await context.SaveChangesAsync();
        }
    }
}
