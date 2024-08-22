using AgentRest.Dto;
using AgentRest.Models;

namespace AgentRest.Service
{
    public interface IAgentService
    {
        Task<IdDto> CreateAgentAsync(AgentDto agentDto);
        Task<AgentModel?> GetAgentByIdAsync(long id);
        Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto);
        Task<AgentModel?> GetAvailableAgentAsync(TargetModel target);
        Task<bool> IsAvailableAgent(long agentId);
    }
}
