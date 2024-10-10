using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherNewsAPI.Data;
using WeatherNewsAPI.Models;
using WeatherNewsAPI.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;

namespace WeatherNewsAPI.Controllers
{
    [EnableCors("AllowReactApp")]
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherNewsController : ControllerBase
    {
        private readonly ILogger<WeatherNewsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly WeatherService _weatherService;
        private readonly NewsService _newsService;

        public WeatherNewsController(
            ILogger<WeatherNewsController> logger,
            ApplicationDbContext context,
            WeatherService weatherService,
            NewsService newsService)
        {
            _logger = logger;
            _context = context;
            _weatherService = weatherService;
            _newsService = newsService;
        }

        [HttpGet("{city}")]
        public async Task<IActionResult> Get(string city)
        {
            _logger.LogInformation($"Received request for city: {city}");

            try
            {
                var weather = await _weatherService.GetWeatherAsync(city);
                var news = await _newsService.GetNewsAsync(city);

                var weatherNews = new WeatherNews
                {
                    City = city,
                    Weather = weather,
                    News = news,
                    QueryDate = DateTime.UtcNow
                };

                _context.WeatherNews.Add(weatherNews);
                await _context.SaveChangesAsync();

                var result = new
                {
                    city = city,
                    current_weather = new
                    {
                        observation_time = DateTime.Now.ToString("h:mm tt"),
                        temperature = weather.Temperature,
                        weather_descriptions = new[] { weather.Description },
                        wind_speed = weather.WindSpeed,
                        humidity = weather.Humidity
                    },
                    news = news.Select(n => new
                    {
                        author = n.Author,
                        title = n.Title,
                        description = n.Description,
                        url = n.Url,
                        urlToImage = n.UrlToImage,
                        publishedAt = n.PublishedAt,
                        content = n.Content
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing request for city {city}: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            _logger.LogInformation("Received request for search history");

            try
            {
                var history = await _context.WeatherNews
                    .OrderByDescending(wn => wn.QueryDate)
                    .Select(wn => wn.City)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation($"Returning search history: {string.Join(", ", history)}");
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving search history: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the search history.");
            }
        }
    }
}