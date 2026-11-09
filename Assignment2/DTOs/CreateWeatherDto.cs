using System.ComponentModel.DataAnnotations;

public class CreateWeatherDto
{
    [Required] public int EventID { get; set; }
    [Required] public DateTime ForecastDate { get; set; }
    public decimal? TemperatureC { get; set; }
    [Required, MaxLength(200)] public string Condition { get; set; } = "";
    public decimal? WindSpeedKmh { get; set; }
    [Range(0, 100)] public int? RainChancePercent { get; set; }
}