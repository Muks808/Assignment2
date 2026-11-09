namespace Assignment2.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = "Run";
        public string? BannerUrl { get; set; }
        public string EventName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Location { get; set; } = "";
        public string Province { get; set; } = "";
        public decimal EntryFee { get; set; }
        public int? MaxParticipants { get; set; }
        public string Status { get; set; } = "Scheduled";
        public Organiser? Organiser { get; set; }
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
        public Route? Route { get; set; }
        public ICollection<EventEnrollment> EventEnrollments { get; set; } = new List<EventEnrollment>();
        public ICollection<EventWeather> EventWeather { get; set; } = new List<EventWeather>();
    }
}