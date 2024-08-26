using AgentRest.Models;
using AgentRest.Dto;


namespace AgentRest.Service
{
    public interface IMissionService
    {
        MissionModel CreateMissionModel(TargetModel target, AgentModel agent);
        Task<List<MissionModel>> GetMissionsAsync();
        Task<MissionModel?> GetMissionByIdAsync(long id);
        Task<MissionModel> ActivateMissionAsync(long missionId);
        double MeasureDistance(TargetModel target, AgentModel agent);
        Task UpdateMissionsAsync();
    }
}
