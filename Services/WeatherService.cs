using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WeatherNewsAPI.Models;

namespace WeatherNewsAPI.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiKeys:OpenWeatherMap"] ?? throw new InvalidOperationException("OpenWeatherMap API key not found in configuration.");
            _logger = logger;
        }

        public async Task<Weather> GetWeatherAsync(string city)
        {
            try
            {
                var response = await _httpClient.GetAsync($"weather?q={city}&APPID={_apiKey}&units=metric");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to OpenWeatherMap API. Check your API key.");
                    throw new UnauthorizedAccessException("Invalid API key for OpenWeatherMap service");
                }
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(content);

                return new Weather
                {
                    Temperature = data["main"]?["temp"]?.Value<double>() ?? 0,
                    Description = data["weather"]?[0]?["description"]?.Value<string>() ?? string.Empty,
                    Humidity = data["main"]?["humidity"]?.Value<int>() ?? 0,
                    WindSpeed = data["wind"]?["speed"]?.Value<double>() ?? 0
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching weather data: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in GetWeatherAsync: {ex.Message}");
                throw;
            }
        }
    }
}