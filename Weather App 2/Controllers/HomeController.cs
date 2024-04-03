using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Weather_App_2.Models;

namespace Weather_App_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString = @"Server=DESKTOP-OAHMLJL\SQLEXPRESS;Database=Weather App 2;Integrated Security=True;";
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cityData = await GetCachedCityDataAsync();
                return View(cityData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching city data.");
                return View("Error");
            }
        }

        private async Task<List<CityDataModel>> GetCityDataAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT CityName, Country, Latitude, Longitude FROM WorldCities";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var cityData = new List<CityDataModel>();

                    while (await reader.ReadAsync())
                    {
                        var city = new CityDataModel
                        {
                            CityName = reader.IsDBNull(0) ? null : reader.GetString(0),
                            Country = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Latitude = !reader.IsDBNull(2) ? (double?)reader.GetDouble(2) : null,
                            Longitude = !reader.IsDBNull(3) ? (double?)reader.GetDouble(3) : null
                        };

                        cityData.Add(city);
                    }

                    return cityData;
                }
            }
        }

        private async Task<List<CityDataModel>> GetCachedCityDataAsync()
        {
            var cachedData = await _cache.GetOrCreateAsync("Cities", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // Cache for 10 minutes
                return await GetCityDataAsync();
            });

            return cachedData;
        }

        // Add other action methods here...

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext?.TraceIdentifier ?? "" });
        }

        [HttpGet]
        public async Task<IActionResult> SearchCities(string term)
        {
            var terms = term.Split(' ');

            var cityData = await GetCityDataAsync();

            var filteredCities = cityData;

            foreach (var word in terms)
            {
               filteredCities = filteredCities
                                     .Where(c => c.CityName.Contains(word, StringComparison.OrdinalIgnoreCase) || c.Country.Contains(word, StringComparison.OrdinalIgnoreCase))
                                     .ToList();
            }
            return Json(filteredCities);
        }

    }
}
