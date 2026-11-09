namespace Assignment2.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Organiser? Organiser { get; set; }
        public Participant? Participant { get; set; }
    }
}
