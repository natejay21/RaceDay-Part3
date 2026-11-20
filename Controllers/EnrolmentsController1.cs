using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrolmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int categoryId, int eventId)
        {
            var token = HttpContext.Session.GetString("Token");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach the JWT token to the request header!
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send the enrolment to the API
            var enrolmentData = new { UserId = int.Parse(userId), CategoryId = categoryId };
            var response = await client.PostAsJsonAsync("api/enrolments", enrolmentData);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Successfully enrolled in the race!";
            }
            else
            {
                TempData["Error"] = "Enrolment failed. You might already be enrolled.";
            }

            return RedirectToAction("Details", "Home", new { id = eventId });
        }
    }
}