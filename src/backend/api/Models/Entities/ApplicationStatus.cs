namespace api.Models.Entities
{
    public class ApplicationStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Color { get; set; } = "#007bff";
        public int SortOrder { get; set; }
    }
}