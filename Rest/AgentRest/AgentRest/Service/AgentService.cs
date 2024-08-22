using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class AgentService(ApplicationDbContext context, IMissionService missionService) : IAgentService
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
            AgentModel? target = await context.Agents.FindAsync(agentModel)
                ?? throw new Exception("Something went wrong");
            return new() { Id = agentModel.Id };
        }

        public async Task<AgentModel?> GetAgentByIdAsync(long id) =>
            await context.Agents.Where(a => a.Id == id).FirstOrDefaultAsync()
            ?? throw new Exception("Could not found the agent by the given id");

        public async Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            agent!.XPosition = locationDto.XPosition;
            agent.YPosition = locationDto.YPosition;
            context.SaveChanges();
            return agent;
        }

        public async Task<AgentModel?> GetAvailableAgentAsync(TargetModel target) => 
            await context.Agents
                .Where(a => a.AgentStatus == AgentModel.Status.Inactive)
                .Where(a => missionService.MeasureDistance(target, a) < 200)
                .FirstOrDefaultAsync();

        public async Task<bool> IsAvailableAgent(long agentId)
        {
            AgentModel? agent = await GetAgentByIdAsync(agentId);
            return agent!.AgentStatus == AgentModel.Status.Inactive;
        }
    }
}
