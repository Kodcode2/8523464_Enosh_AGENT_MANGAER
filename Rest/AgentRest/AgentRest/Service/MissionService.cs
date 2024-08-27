using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class MissionService(IServiceProvider serviceProvider) : IMissionService
    {
        private IAgentService agentService => serviceProvider.GetRequiredService<IAgentService>();
        private ITargetService targetService => serviceProvider.GetRequiredService<ITargetService>();
        private ApplicationDbContext context => DbContextFactory.CreateDbContext(serviceProvider);

        // Method to retrieve all missions asynchronously
        public async Task<List<MissionModel>> GetMissionsAsync() =>
           await context.Missions.AnyAsync()
            ? await context.Missions.ToListAsync()
            : [];

        // Retrieve a mission by its ID asynchronously
        public async Task<MissionModel?> GetMissionByIdAsync(long id)
        {
            var mission = await context.Missions.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (mission == null)
            {
                throw new Exception($"Could not find the mission by the given id {id}");
            }

            return mission;
        }

        // Activate a mission by ID asynchronously
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
                .Where(m => m.Id != missionId && (m.TargetId == target!.Id || m.AgentId == agent.Id))
                .ToList();
            context.Missions.RemoveRange(toCancel);
            await context.SaveChangesAsync();
            return mission;
        }

        // Create a new MissionModel
        public MissionModel CreateMissionModel(TargetModel target, AgentModel agent)
        {
            MissionModel newMission = new()
            {
                TargetId = target.Id,
                AgentId = agent!.Id,
                RemainingTime = MeasureDistance(target, agent) / 5,
            };
            return newMission;
        }

        // Calculate distance between the agent and the target
        public double MeasureDistance(TargetModel target, AgentModel agent) =>
            Math.Sqrt(Math.Pow(target.XPosition - agent.XPosition, 2)
                    + Math.Pow(target.YPosition - agent.YPosition, 2));

        // Move the agent towards the target asynchronously
        private async Task MoveAgentTowardsTheTarget(MissionModel mission)
        {
            AgentModel? agent = await agentService.GetAgentByIdAsync(mission.AgentId);
            TargetModel? target = await targetService.GetTargetByIdAsync(mission.TargetId);
            bool isAgentLeftToTarget = agent!.XPosition < target!.XPosition;
            bool isAgentRightToTarget = agent!.XPosition > target!.XPosition;
            bool isAgentUnderTarget = agent.YPosition < target.YPosition;
            bool isAgentAboveTarget = agent.YPosition > target.YPosition;
            if (isAgentLeftToTarget)
            {
                agent.XPosition++;
            }
            if (isAgentRightToTarget)
            {
                agent.XPosition--;
            }
            if (isAgentUnderTarget)
            {
                agent.YPosition++;
            }
            if (isAgentAboveTarget)
            {
                agent.YPosition--;
            }
            if (!(isAgentLeftToTarget && isAgentUnderTarget && isAgentAboveTarget && isAgentRightToTarget))
            {
                await Kill(mission, agent, target);
            }
            await context.SaveChangesAsync();
        }

        // Kill the target and update statuses
        private async Task Kill(MissionModel mission, AgentModel agent, TargetModel target)
        {
            target.TargetStatus = TargetStatus.Dead;
            agent.AgentStatus = AgentStatus.InActive;
            mission.MissionStatus = MissionStatus.Ended;
            DateTime dateTime = DateTime.UtcNow;
            mission.ExecutionTime = (dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
            await context.SaveChangesAsync();
        }

        // Update all assigned missions asynchronously
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

        // Calculate remaining time for the agent to reach the target
        private async Task<double> EvaluateRemainingTime(long agentId, long targetId)
        {
            AgentModel? agent = await agentService.GetAgentByIdAsync(agentId);
            TargetModel? target = await targetService.GetTargetByIdAsync(targetId);
            return MeasureDistance(target!, agent!) / 5;
        }
    }
}
