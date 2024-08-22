using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class AgentService(ApplicationDbContext context, 
        IMissionService missionService,
        ITargetService targetService) : IAgentService
    {
        public async Task<IdDto> CreateAgentAsync(AgentDto agentDto)
        {
            AgentModel? agentModel = new()
            {
                NickName = agentDto.NickName,
                Image = agentDto.Photo_url
            };
            await context.Agents.AddAsync(agentModel);
            await context.SaveChangesAsync();
            AgentModel? agent = await context.Agents.FindAsync(agentModel)
                ?? throw new Exception("Something went wrong");
            return new() { Id = agent.Id };
        }

        public async Task<AgentModel?> GetAgentByIdAsync(long id) =>
            await context.Agents.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new Exception("Could not found the agent by the given id");

        public async Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            agent!.XPosition = locationDto.XPosition;
            agent.YPosition = locationDto.YPosition;
            context.SaveChanges();

            TargetModel? closestTarget = await targetService.GetAvailableTargetAsync(agent);
            if (closestTarget != null)
            {
                await missionService.CreateMissionAsync(closestTarget, agent);
            }
            return agent;
        }

        public async Task<List<AgentModel>> GetAvailableAgentsAsync(TargetModel target) => 
            await context.Agents
                .Where(a => a.AgentStatus == AgentModel.Status.InActive)
                .Where(a => missionService.MeasureDistance(target, a) < 200)
                .ToListAsync() ?? [];

        public async Task<bool> IsAvailableAgent(long agentId)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            return agent!.AgentStatus == AgentModel.Status.InActive;
        }
    }
}
