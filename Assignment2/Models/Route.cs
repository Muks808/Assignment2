namespace Assignment2.Models
{
    public class Route
    {
        public int RouteID { get; set; }
        public int EventID { get; set; }
        public string RouteName { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public decimal? ElevationGainM { get; set; }
        public string StartPoint { get; set; } = "";
        public string FinishPoint { get; set; } = "";
        public string RouteUrl { get; set; } = "";
        public Event? Event { get; set; }
    }
}