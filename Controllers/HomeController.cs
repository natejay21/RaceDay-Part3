using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Fetch events from the Part 2 API
            var response = await client.GetAsync("api/events");

            var events = new List<EventViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON into our C# list
                events = JsonSerializer.Deserialize<List<EventViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EventViewModel>();
            }

            return View(events);
        }

        // NEW: Fetch specific event details and its categories
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // 1. Fetch the specific event
            var eventResponse = await client.GetAsync($"api/events/{id}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var eventContent = await eventResponse.Content.ReadAsStringAsync();
            var eventObj = JsonSerializer.Deserialize<EventViewModel>(eventContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (eventObj != null)
            {
                // 2. Fetch the categories for this event
                var catResponse = await client.GetAsync($"api/events/{id}/categories");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catContent = await catResponse.Content.ReadAsStringAsync();
                    eventObj.Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CategoryViewModel>();
                }
            }

            return View(eventObj);
        }
    }
}