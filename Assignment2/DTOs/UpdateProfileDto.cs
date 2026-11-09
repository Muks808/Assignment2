using System.ComponentModel.DataAnnotations;
namespace Assignment2.DTOs
{
    public class UpdateProfileDto
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = "";
        [Required, MaxLength(50)] public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = "";
        [Required, MaxLength(20)] public string Phone { get; set; } = "";
    }
}