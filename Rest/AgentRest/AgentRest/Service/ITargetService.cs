using AgentRest.Dto;
using AgentRest.Models;

namespace AgentRest.Service
{
    public interface ITargetService
    {
        Task<TargetModel> CreateTargetAsync(TargetDto targetDto);
        Task<List<TargetModel>> GetAllTargetsAsync();
        Task<TargetModel?> GetTargetByIdAsync(long id);
        Task<TargetModel> UpdateTargetAsync(long targetId, TargetModel targetModel);
        Task DeleteTargetAsync(long targetId);
    }
}
