using AgentMVC.Models;

namespace AgentMVC.Service
{
    public interface IAgentService
    {
        Task<List<AgentModel>> GetAllAgentsAsync();
    }
}
