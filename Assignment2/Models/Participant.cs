namespace Assignment2.Models
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public int UserID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "";
        public string EmergencyContact { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
        public ICollection<EventEnrollment> EventEnrollments { get; set; } = new List<EventEnrollment>();
    }
}