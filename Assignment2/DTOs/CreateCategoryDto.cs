using System.ComponentModel.DataAnnotations;
namespace Assignment2.DTOs
{
    public class CreateCategoryDto
    {
        [Required, MaxLength(100)] public string CategoryName { get; set; } = "";
        [MaxLength(200)] public string Description { get; set; } = "";
    }
}