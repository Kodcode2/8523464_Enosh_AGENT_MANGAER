using AgentMVC.Service;
using AgentMVC.ViewModels;
using AgentMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentMVC.Controllers
{
    public class OverViewController(IMissionService missionService, ITargetSevice targetSevice, IAgentService agentService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            List<TargetModel> targetModels = await targetSevice.GetAllTargetsAsync();
            List<MissionModel> missionModels = await missionService.GetAllMissions();
            List<AgentModel> agentModels = await agentService.GetAllAgentsAsync();

            int agentAmount = agentModels.Count;
            int targetAmount = targetModels.Count;
            int availableAgents = agentModels.Where(a => a.AgentStatus == AgentStatus.InActive).Count();
            OverViewModel model = new()
            {
                AgentAmount = agentAmount,
                ActiveAgents = agentModels.Where(a => a.AgentStatus == AgentStatus.Active).Count(),
                TargetsAmount = targetAmount,
                ActiveTargets = targetModels.Where(t => t.TargetStatus == TargetStatus.Alive || t.TargetStatus == TargetStatus.Hunted).Count(),
                MissionsAmount = missionModels.Count,
                ActiveMissions = missionModels.Where(m => m.MissionStatus == MissionStatus.Assigned).Count(),
                RelativeAgentsToTargets = agentAmount / targetAmount,
                RelativeAvailableAgentsToTargets = availableAgents / targetAmount
            };
            return View(model);
        }
    }
}
