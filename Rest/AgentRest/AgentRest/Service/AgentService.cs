using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class AgentService(ApplicationDbContext context) : IAgentService
    {
        public async Task<AgentModel> CreateAgentAsync(AgentDto agentDto)
        {
            AgentModel? agentModel = new()
            {
                NickName = agentDto.NickName,
                Image = agentDto.Photo_url,
            };
            await context.Agents.AddAsync(agentModel);
            await context.SaveChangesAsync();
            return agentModel;
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

        public Task<AgentModel?> GetAvailableAgentAsync(TargetModel target)
        {
            throw new NotImplementedException();
        }
    }
}
