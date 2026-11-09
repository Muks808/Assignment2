namespace Assignment2.Models
{
    public class Enrolment
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int ParticipantId { get; set; }

        public string EmergencyContact { get; set; } = "";

        public decimal SelectedDistance { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}