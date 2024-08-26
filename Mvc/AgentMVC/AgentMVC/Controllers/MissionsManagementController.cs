using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AgentMVC.Models;
using AgentMVC.Service;

namespace AgentMVC.Controllers
{
    public class MissionsManagementController(IMissionService missionService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await missionService.GetAllMissions());
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
    }
}
