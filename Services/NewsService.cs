using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WeatherNewsAPI.Models;

namespace WeatherNewsAPI.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<NewsService> _logger;

        public NewsService(HttpClient httpClient, IConfiguration configuration, ILogger<NewsService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiKeys:NewsApi"] ?? throw new InvalidOperationException("News API key not found in configuration.");
            _logger = logger;
        }

        public async Task<List<News>> GetNewsAsync(string city)
        {
            try
            {
                var encodedCity = Uri.EscapeDataString(city);
                var url = $"everything?q={encodedCity}&apiKey={_apiKey}&language=en&sortBy=publishedAt";
                _logger.LogInformation($"Requesting news for URL: {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Received response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"News API returned an error. Status Code: {response.StatusCode}. Content: {content}");
                    throw new HttpRequestException($"News API request failed with status code: {response.StatusCode}. Content: {content}");
                }

                var data = JObject.Parse(content);

                return data["articles"]?.Take(5).Select(article => new News
                {
                    Author = article["author"]?.Value<string>(),
                    Title = article["title"]?.Value<string>() ?? "No Title",
                    Description = article["description"]?.Value<string>(),
                    Url = article["url"]?.Value<string>() ?? "#",
                    UrlToImage = article["urlToImage"]?.Value<string>(),
                    PublishedAt = article["publishedAt"]?.Value<DateTime>() ?? DateTime.Now,
                    Content = article["content"]?.Value<string>()
                }).ToList() ?? new List<News>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching news data: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in GetNewsAsync: {ex.Message}");
                throw;
            }
        }
    }
}