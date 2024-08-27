using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgentRest.Service
{
    public class AgentService(IServiceProvider serviceProvider) : IAgentService
    {
        private IMissionService missionService => serviceProvider.GetRequiredService<IMissionService>();

        private ApplicationDbContext context = DbContextFactory.CreateDbContext(serviceProvider);

        // Direction mapping for moving agents
        private readonly Dictionary<string, (int, int)> Direction = new()
        {
            {"n", (0, 1)},
             {"s", (0, -1)},
             {"e", (-1, 0)},
             {"w", (1, 0)},
             {"ne", (-1, 1)},
             {"nw", (1, 1)},
             {"se", (-1, -1)},
             {"sw", (1, -1)}
        };

        // Create a new agent asynchronously
        public async Task<IdDto> CreateAgentAsync(AgentDto agentDto)
        {
            AgentModel? agentModel = new()
            {
                NickName = agentDto.NickName,
                Image = agentDto.PhotoUrl
            };
            await context.Agents.AddAsync(agentModel);
            await context.SaveChangesAsync();
            var agent = await context.Agents
                .Where(a => a.NickName == agentDto.NickName)
                .Where(a => a.Image == agentDto.PhotoUrl)
                .FirstOrDefaultAsync();
            return new() { Id = agent!.Id };
        }

        // Check if the position is outside valid range
        public bool IsInvalidPosition(int x, int y) => (y > 1000 || x > 1000 || y < 0 || x < 0);

        // Move an agent based on direction asynchronously
        public async Task<AgentModel> MoveAgentAsync(long agentId, DirectionDto directionDto)
        {
            // Ensure agent is not assigned to a mission
            if (context.Missions
                .Where(m => m.MissionStatus == MissionStatus.Assigned)
                .Select(m => m.Id)
                .Contains(agentId))
            {
                throw new Exception("Could not move an agent who are currnetly assinged for a mission");
            } 
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            var (x, y) = Direction[directionDto.Direction];
            agent!.XPosition += x;
            agent!.YPosition += y;
            if (IsInvalidPosition(agent.XPosition, agent.YPosition))
            {
                throw new Exception($"The corresponds coordinats: x:{agent.XPosition}, y:{agent.YPosition} are off the map borders");
            }

            await context.SaveChangesAsync();

            // Create new missions for closest targets
            List<TargetModel> closestTargets = await GetAvailableTargestAsync(agent);
            if (closestTargets.Count > 0)
            {
                var newMissions = closestTargets.Select(target => missionService.CreateMissionModel(target, agent)).ToList();
                await context.Missions.AddRangeAsync(newMissions);
            }
            await context.SaveChangesAsync();
            return agent;
        }

        // Get an agent by ID asynchronously
        public async Task<AgentModel?> GetAgentByIdAsync(long id) =>
            await context.Agents.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new Exception($"Could not found the agent by the given id: {id}");

        // Evaluate remaining time for agent to reach target
        private double EavaluateRemainingTime(TargetModel target, AgentModel agent) =>
            missionService.MeasureDistance(target, agent) / 5;

        // Pin agent to a specific location asynchronously
        public async Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            agent!.XPosition = locationDto.x;
            agent.YPosition = locationDto.y;
            await context.SaveChangesAsync();

            // Create missions for closest targets
            var closestTargets = await GetAvailableTargestAsync(agent) ?? [];
            if (closestTargets.Count != 0)
            {
                var missions = closestTargets.Select((t) => new MissionModel() { AgentId = t.Id, TargetId = agentId, RemainingTime = EavaluateRemainingTime(t, agent)});
                await context.Missions.AddRangeAsync(missions);
                await context.SaveChangesAsync();
            }
            return agent;
        }

        // Get available agents for a target asynchronously
        public async Task<List<AgentModel>> GetAvailableAgentsAsync(TargetModel target) =>
            await context.Agents.AnyAsync()
            ? await context.Agents
                .Where(a => a.AgentStatus == AgentStatus.InActive)
                .Where(a => Math.Sqrt(Math.Pow(target.XPosition - a.XPosition, 2)
                    + Math.Pow(target.YPosition - a.YPosition, 2)) < 200.0)
                .ToListAsync()
            : [];

        // Get all inactive agents asynchronously
        public async Task<List<AgentModel>> GetAllAgentsAsync() =>
            await context.Agents
            .Where(a => a.AgentStatus == AgentStatus.InActive)
            .ToListAsync();

        // Get available targets for an agent asynchronously
        public async Task<List<TargetModel>> GetAvailableTargestAsync(AgentModel agent) =>
            await context.Targets.AnyAsync()
            ? await context.Targets
                   .Where(t => t.TargetStatus == TargetStatus.Alive)
                   .Where(t => Math.Sqrt(Math.Pow(agent.XPosition - t.XPosition, 2)
                    + Math.Pow(agent.YPosition - t.YPosition, 2)) < 200)
                   .ToListAsync()
            : [];
    }
}
