using System;

namespace WeatherNewsAPI.Models{

        public class News
    {
        public int Id { get; set; }
        public string? Author { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? UrlToImage { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? Content { get; set; }
    }
}