using AgentMVC.Models;
using AgentMVC.ViewModels;

namespace AgentMVC.Service
{
    public interface IMissionService
    {
        Task<List<MissionVM>> GetAllMissions();
    }
}
