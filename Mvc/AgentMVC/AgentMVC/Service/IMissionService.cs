using AgentMVC.Models;
using AgentMVC.ViewModels;

namespace AgentMVC.Service
{
    public interface IMissionService
    {
        Task AssignMissionAsync(long id);
        Task<List<MissionModel>> GetAllMissions();
        Task<List<MissionVM>> GetMissionVMs();
        Task<MissionVM?> GetMissionVMById(long id);
    }
}
