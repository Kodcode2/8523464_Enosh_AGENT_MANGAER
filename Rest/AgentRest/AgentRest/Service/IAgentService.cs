using AgentRest.Dto;
using AgentRest.Models;

namespace AgentRest.Service
{
    public interface IAgentService
    {
        Task<IdDto> CreateAgentAsync(AgentDto agentDto);
        Task<AgentModel?> GetAgentByIdAsync(long id);
        Task<AgentModel> PinAgentAsync(long agentId, LocationDto locationDto);
        Task<AgentModel> MoveAgentAsync(long agentId, DirectionDto directionDto);
        Task<List<AgentModel>> GetAvailableAgentsAsync(TargetModel target);
        Task<List<AgentModel>> GetAllAgentsAsync();
    }
}
