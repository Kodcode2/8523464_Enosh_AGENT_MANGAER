using AgentMVC.Models;

namespace AgentMVC.Service
{
    public interface ITargetSevice
    {
        Task<List<TargetModel>> GetAllTargetsAsync();
    }
}
