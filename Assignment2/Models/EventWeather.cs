namespace Assignment2.Models
{
    public class EventWeather
    {
        public int WeatherID { get; set; }
        public int EventID { get; set; }
        public DateTime ForecastDate { get; set; }
        public decimal? TemperatureC { get; set; }
        public string Condition { get; set; } = "";
        public decimal? WindSpeedKmh { get; set; }
        public int? RainChancePercent { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public Event? Event { get; set; }
    }
}