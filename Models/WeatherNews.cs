using System;

namespace WeatherNewsAPI.Models{

        public class WeatherNews
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public Weather Weather { get; set; } = new Weather();
        public List<News> News { get; set; } = new List<News>();
        public DateTime QueryDate { get; set; }
    }
}