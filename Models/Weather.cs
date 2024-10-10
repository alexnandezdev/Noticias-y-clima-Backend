using System;

namespace WeatherNewsAPI.Models{

    public class Weather
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }


}

