using AgentRest.Models;
using AgentRest.Dto;


namespace AgentRest.Service
{
    public interface IMissionService
    {
        Task<MissionModel?> CreateMissionAsync(TargetModel target, AgentModel agent);
        Task<MissionModel?> GetMissionByAgentIdAsync(long agentId);
        Task<MissionModel?> GetMissionByTargetIdAsync(long targetId);
        Task<List<MissionModel>> GetMissionsAsync();
        Task<MissionModel?> GetMissionByIdAsync(long id);
        Task<MissionModel> ActivateMissionAsync(long missionId);
        Task<MissionModel> EndMissionAsync(long missionId);
        Task DeleteMissionByIdAsync(long missionId);
        double MeasureDistance(TargetModel target, AgentModel agent);
    }
}
