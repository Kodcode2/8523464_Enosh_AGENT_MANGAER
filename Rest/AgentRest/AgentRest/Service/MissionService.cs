using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class MissionService(ApplicationDbContext context, IServiceProvider serviceProvider) : IMissionService
    {
        private IAgentService agentService => serviceProvider.GetRequiredService<IAgentService>();
        private ITargetService targetService => serviceProvider.GetRequiredService<ITargetService>();
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

        public async Task<MissionModel?> GetMissionByIdAsync(long id)
        {
            var mission = await context.Missions.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (mission == null)
            {
                throw new Exception($"Could not find the mission by the given id {id}");
            }

            return mission;
        }

            public async Task<MissionModel> ActivateMissionAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            AgentModel? agent = await agentService.GetAgentByIdAsync(mission!.AgentId);
            TargetModel? target = await targetService.GetTargetByIdAsync(mission.TargetId);
            if (MeasureDistance(target!, agent!) > 200) 
            { 
                context.Missions.Remove(mission);
                throw new Exception("The distance between the agent and the target is now standing over 200 km"); 
            }
            mission!.MissionStatus = MissionStatus.Assigned;
            agent!.AgentStatus = AgentStatus.Active;
            List<MissionModel> toCancel = context.Missions
                .Where(m => m.MissionStatus == MissionStatus.Propose)
                .Where(m => m.TargetId == target!.Id || m.AgentId == agent.Id)
                .ToList();
            context.Missions.RemoveRange(toCancel);
            await context.SaveChangesAsync();
            return mission;
        }

        public async Task<MissionModel> EndMissionAsync(long missionId)
        {
            MissionModel? mission = await GetMissionByIdAsync(missionId);
            mission!.MissionStatus = MissionStatus.Ended;
            await context.SaveChangesAsync();
            return mission;
        }

        public async Task<MissionModel?> CreateMissionAsync(TargetModel target, AgentModel agent)
        {
            MissionModel? newMission = new()
            {
                TargetId = target.Id,
                AgentId = agent!.Id,
                RemainingTime = MeasureDistance(target, agent) / 5,
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

        private async Task MoveAgentTowardsTheTarget(MissionModel mission)
        {
            AgentModel? agent = await agentService.GetAgentByIdAsync(mission.AgentId);
            TargetModel? target = await targetService.GetTargetByIdAsync(mission.TargetId);
            bool isAgentLeftToTarget = agent!.XPosition < target!.XPosition;
            bool isAgentUnderTarget = agent.YPosition < target.YPosition;
            if (isAgentLeftToTarget)
            {
                agent.XPosition++;
            }
            if (isAgentUnderTarget)
            {
                agent.YPosition++;
            }
            if (!(isAgentLeftToTarget && isAgentUnderTarget))
            {
                await Kill(mission, agent, target);
            }
            await context.SaveChangesAsync();
        }

        private async Task Kill(MissionModel mission, AgentModel agent, TargetModel target)
        {
            target.TargetStatus = TargetStatus.Dead;
            agent.AgentStatus = AgentStatus.InActive;
            mission.MissionStatus = MissionStatus.Ended;
            context.Targets.Remove(target);
            DateTime dateTime = DateTime.Now;
            mission.ExecutionTime = double.Parse(dateTime.ToString());
            await context.SaveChangesAsync();
        }

        public async Task UpdateMissionsAsync()
        {
            List<MissionModel> missions = await context.Missions
                .Where(m => m.MissionStatus == MissionStatus.Assigned)
                .ToListAsync();
            missions.ForEach(async mission => 
            {
                await MoveAgentTowardsTheTarget(mission);
                mission.RemainingTime = await EvaluateRemainingTime(mission.AgentId, mission.TargetId);
            });

        }

        private async Task<double> EvaluateRemainingTime(long agentId, long targetId)
        {
            AgentModel? agent = await agentService.GetAgentByIdAsync(agentId);
            TargetModel? target = await targetService.GetTargetByIdAsync(targetId);
            return MeasureDistance(target!, agent!) / 5;
        }
    }
}
