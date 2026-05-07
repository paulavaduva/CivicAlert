namespace CivicAlert.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Issue> Issues { get; set; }

        public int DepartmentId { get; set; } 
        public Department Department { get; set; }
    }
}
