namespace Assignment2.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int EnrollmentID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string AveragePace { get; set; } = "";
        public string ResultStatus { get; set; } = "Finished";
        public EventEnrollment? Enrollment { get; set; }
    }
}