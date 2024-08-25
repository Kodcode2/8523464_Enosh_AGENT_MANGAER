using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AgentMVC.Models;

namespace AgentMVC.Controllers
{
    public class MissionsController(IHttpClientFactory clientFactory) : Controller
    {
        private readonly string baseUrl = "https://localhost:7121/Missions";

        public async Task<IActionResult> Index()
        {
            var httpClient = clientFactory.CreateClient();
            var result = await httpClient.GetAsync($"{baseUrl}");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                List<MissionModel>? missions = JsonSerializer.Deserialize<List<MissionModel>>(content,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? [];

                return View(missions);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
