using System.ComponentModel.DataAnnotations;
namespace Assignment2.DTOs
{
    public class ResultDto
    {
        public TimeSpan? FinishTime { get; set; }
        [Range(1, int.MaxValue)] public int? Position { get; set; }
        [MaxLength(20)] public string AveragePace { get; set; } = "";
        [Required, MaxLength(20)] public string ResultStatus { get; set; } = "Finished";
    }
}
