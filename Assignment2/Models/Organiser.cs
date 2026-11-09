namespace Assignment2.Models
{
    public class Organiser
    {
        public int OrganiserID { get; set; }
        public int UserID { get; set; }
        public string OrganisationName { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string Website { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}