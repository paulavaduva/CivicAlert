using System.Text.Json.Serialization;

namespace CivicAlert.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Issue> Issues { get; set; }

        public int DepartmentId { get; set; } 
        public Department Department { get; set; }
    }
}
