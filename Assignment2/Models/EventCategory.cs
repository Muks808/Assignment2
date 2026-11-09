namespace Assignment2.Models
{
    public class EventCategory
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public Category? Category { get; set; }
        public Event? Event { get; set; }
    }
}