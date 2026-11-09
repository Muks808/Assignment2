namespace Assignment2.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public string Description { get; set; } = "";
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
