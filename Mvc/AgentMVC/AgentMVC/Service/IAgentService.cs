using AgentMVC.Models;
using AgentMVC.ViewModels;

namespace AgentMVC.Service
{
    public interface IAgentService
    {
        Task<List<AgentModel>> GetAllAgentsAsync();
        Task<List<AgentVM>> GetAgentVMs();
    }
}
