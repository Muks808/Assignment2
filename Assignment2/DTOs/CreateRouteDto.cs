using System.ComponentModel.DataAnnotations;

namespace Assignment2.DTOs
{
    public class CreateRouteDto
    {
        [Required] public int EventID { get; set; }
        [Required, MaxLength(100)] public string RouteName { get; set; } = "";
        [Range(0.01, 10000)] public decimal DistanceKm { get; set; }
        [Range(0, 100000)] public decimal? ElevationGainM { get; set; }
        [Required, MaxLength(150)] public string StartPoint { get; set; } = "";
        [Required, MaxLength(150)] public string FinishPoint { get; set; } = "";
        [MaxLength(500)] public string RouteUrl { get; set; } = "";
    }
}
