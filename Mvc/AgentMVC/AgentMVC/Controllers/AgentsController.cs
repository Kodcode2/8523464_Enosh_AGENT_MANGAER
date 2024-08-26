using AgentMVC.Service;
using Microsoft.AspNetCore.Mvc;

namespace AgentMVC.Controllers
{
    public class AgentsController(IAgentService agentService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await agentService.GetAgentVMs());
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
    }
}
