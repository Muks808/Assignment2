using System.ComponentModel.DataAnnotations;
namespace Assignment2.DTOs
{
    public class EnrolmentDto
    {
        [Range(1, int.MaxValue)] public int? CategoryID { get; set; }
        [MaxLength(20)] public string? RaceNumber { get; set; }
        [Required, MaxLength(20)] public string Status { get; set; } = "Registered";
    }
}