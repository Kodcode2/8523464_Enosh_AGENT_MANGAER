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
                return View(await missionService.GetMissionVMs());
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        public async Task<IActionResult> Assign(long id)
        {
            try
            {
                await missionService.AssignMissionAsync(id);
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return View(ex.Message);
            }
        }
        
        public async Task<IActionResult> Details(long id)
        {
            try
            {
                return View(await missionService.GetMissionVMById(id));
            }
            catch(Exception ex)
            {
                return View(ex.Message);
            }
        }
    }
}
