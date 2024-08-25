using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgentRest.Service
{
    public class AgentService(ApplicationDbContext context, IServiceProvider serviceProvider) : IAgentService
    {
        private ITargetService targetService => serviceProvider.GetRequiredService<ITargetService>();
        private IMissionService missionService => serviceProvider.GetRequiredService<IMissionService>();

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

        public bool IsInvalidPosition(int x, int y) => (y > 1000 || x > 1000 || y < 0 || x < 0);

        public async Task<AgentModel> MoveAgentAsync(long agentId, DirectionDto directionDto)
        {
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

            List<TargetModel> closestTargets = await targetService.GetAvailableTargestAsync(agent);
            if (closestTargets.Count > 0)
            {
                closestTargets.ForEach(target => { missionService.CreateMissionAsync(target, agent); });
            }
            return agent;
        }

        public async Task<AgentModel?> GetAgentByIdAsync(long id) =>
            await context.Agents.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new Exception($"Could not found the agent by the given id: {id}");

        public async Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            agent!.XPosition = locationDto.x;
            agent.YPosition = locationDto.y;
            await context.SaveChangesAsync();

            var closestTargets = await targetService.GetAvailableTargestAsync(agent) ?? [];
            if (closestTargets.Count != 0)
            {
                var missions = closestTargets.Select((t) => new MissionModel() { AgentId = t.Id, TargetId = agentId });
                await context.Missions.AddRangeAsync(missions);
                await context.SaveChangesAsync();
            }
            return agent;
        }

        public async Task<List<AgentModel>> GetAvailableAgentsAsync(TargetModel target) =>
            await context.Agents.AnyAsync()
            ? await context.Agents
                .Where(a => a.AgentStatus == AgentStatus.InActive)
                .Where(a => Math.Sqrt(Math.Pow(target.XPosition - a.XPosition, 2)
                    + Math.Pow(target.YPosition - a.YPosition, 2)) < 200.0)
                .ToListAsync()
            : [];

        public async Task<bool> IsAvailableAgent(long agentId)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            return agent!.AgentStatus == AgentStatus.InActive;
        }

        public async Task<List<AgentModel>> GetAllAgentsAsync() =>
            await context.Agents
            .Where(a => a.AgentStatus == AgentStatus.InActive)
            .ToListAsync();
    }
}
