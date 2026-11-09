using System.ComponentModel.DataAnnotations;

namespace Assignment2.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(50)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [Required, MinLength(8), MaxLength(150)]
        public string Password { get; set; } = "";
        [Required, MaxLength(20)]
        public string Phone { get; set; } = "";
        [Required]
        public string Role { get; set; } = "Participant";
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(10)] public string? Gender { get; set; }
        [MaxLength(20)] public string? EmergencyContact { get; set; }
        [MaxLength(100)] public string? OrganisationName { get; set; }
        [MaxLength(100)] public string? Website { get; set; }
    }
}