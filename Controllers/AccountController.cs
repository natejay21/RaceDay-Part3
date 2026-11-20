using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.PostAsJsonAsync("api/auth/login", new { email, password });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                var token = doc.RootElement.GetProperty("token").GetString();
                var role = doc.RootElement.GetProperty("role").GetString();

                // FIX: Use GetInt32().ToString() because the API returns a number
                var userId = doc.RootElement.GetProperty("userId").GetInt32().ToString();

                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("UserId", userId);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid login credentials. Please try again.";
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string role)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.PostAsJsonAsync("api/auth/register", new { firstName, lastName, email, password, role });

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Registration failed. Email might already exist.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}