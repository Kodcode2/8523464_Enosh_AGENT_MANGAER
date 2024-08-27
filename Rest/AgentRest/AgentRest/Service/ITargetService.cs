using AgentRest.Dto;
using AgentRest.Models;

namespace AgentRest.Service
{
    public interface ITargetService
    {
        Task<IdDto> CreateTargetAsync(TargetDto targetDto);
        Task<List<TargetModel>> GetAllTargetsAsync();
        Task<TargetModel?> GetTargetByIdAsync(long id);
        Task<TargetModel> MoveTargetAsync(long targetId, DirectionDto directionDto);
        Task<TargetModel> PinTargetAsync(long targetId, LocationDto locationDto);
    }
}
