using AgentRest.Dto;
using AgentRest.Models;

namespace AgentRest.Service
{
    public interface IAgentService
    {
        Task<AgentModel> CreateAgentAsync(AgentDto agentDto);
        Task<AgentModel?> GetAgentByIdAsync(long id);
        Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto);
        Task<AgentModel?> GetAvailableAgentAsync(TargetModel target);
    }
}
