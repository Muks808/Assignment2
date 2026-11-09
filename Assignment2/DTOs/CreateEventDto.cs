using System.ComponentModel.DataAnnotations;
namespace Assignment2.DTOs
{
    public class CreateEventDto
    {
        [Range(0.1, 1000)] public decimal DistanceKm { get; set; } = 5;
        [RegularExpression("^(Run|Walk|Cycle)$")] public string EventType { get; set; } = "Run";
        [Required, MaxLength(150)] public string EventName { get; set; } = "";
        [MaxLength(1000)] public string Description { get; set; } = "";
        [Required] public int CategoryID { get; set; }
        [Required] public DateTime EventDate { get; set; }
        [Required] public TimeSpan StartTime { get; set; }
        [Required, MaxLength(150)] public string Location { get; set; } = "";
        [Required, MaxLength(150)] public string Province { get; set; } = "";
        [Range(0, 100000)] public decimal EntryFee { get; set; }
        [Range(1, 1000000)] public int? MaxParticipants { get; set; }
        [Required, MaxLength(150)] public string Status { get; set; } = "Scheduled";
    }
}