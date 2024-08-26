using AgentMVC.Service;
using Microsoft.AspNetCore.Mvc;

namespace AgentMVC.Controllers
{
    public class TargetsController(ITargetSevice targetSevice) : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await targetSevice.GetAllTargetsAsync());
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
    }
}
