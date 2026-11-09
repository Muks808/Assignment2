namespace Assignment2.Models
{
    public class EventEnrollment
    {
        public int EnrollmentID { get; set; }
        public int EventID { get; set; }
        public int? CategoryID { get; set; }
        public int ParticipantID { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string RaceNumber { get; set; } = "";
        public string Status { get; set; } = "Registered";
        public Event? Event { get; set; }
        public Participant? Participant { get; set; }
        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
